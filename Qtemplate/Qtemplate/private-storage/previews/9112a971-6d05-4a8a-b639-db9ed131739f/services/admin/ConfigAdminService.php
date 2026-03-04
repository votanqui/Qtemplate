<?php
// services/ConfigAdminService.php

class ConfigAdminService {
    private $db;
    private $configService;
    
    public function __construct() {
        $this->db = Database::getInstance()->getConnection();
        $this->configService = ConfigService::getInstance();
    }
    
    /**
     * Lấy tất cả cấu hình
     */
    public function getAllConfigs($category = 'all') {
        $where = "1=1";
        $params = [];
        
        if ($category !== 'all') {
            $where .= " AND category = ?";
            $params[] = $category;
        }
        
        $sql = "SELECT * FROM system_config WHERE $where ORDER BY category, config_key";
        $stmt = $this->db->prepare($sql);
        $stmt->execute($params);
        $configs = $stmt->fetchAll();
        
        // Group by category
        $grouped = [];
        foreach ($configs as $config) {
            $cat = $config['category'];
            if (!isset($grouped[$cat])) {
                $grouped[$cat] = [];
            }
            
            // Parse value based on type
            $config['parsed_value'] = $this->parseValue($config['config_value'], $config['config_type']);
            $grouped[$cat][] = $config;
        }
        
        return [
            'configs' => $configs,
            'grouped' => $grouped,
            'total' => count($configs)
        ];
    }
    
    /**
     * Lấy cấu hình theo key
     */
    public function getConfigByKey($key) {
        $stmt = $this->db->prepare("SELECT * FROM system_config WHERE config_key = ?");
        $stmt->execute([$key]);
        $config = $stmt->fetch();
        
        if ($config) {
            $config['parsed_value'] = $this->parseValue($config['config_value'], $config['config_type']);
        }
        
        return $config;
    }
    
    /**
     * Cập nhật cấu hình
     */
    public function updateConfig($key, $data) {
        // Check if exists
        $existing = $this->getConfigByKey($key);
        if (!$existing) {
            return [
                'success' => false,
                'message' => 'Không tìm thấy cấu hình'
            ];
        }
        
        if (!isset($data['config_value'])) {
            return [
                'success' => false,
                'message' => 'Thiếu config_value'
            ];
        }
        
        // Validate
        $validation = $this->validateConfigValue($data['config_value'], $existing['config_type']);
        if (!$validation['valid']) {
            return [
                'success' => false,
                'message' => $validation['error']
            ];
        }
        
        // Format value
        $valueStr = $this->formatValue($data['config_value'], $existing['config_type']);
        
        try {
            $sql = "UPDATE system_config SET config_value = ?, updated_at = NOW() WHERE config_key = ?";
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$valueStr, $key]);
            
            // Log activity
            $this->logConfigChange($key, 'update', $existing['config_value'], $valueStr);
            
            // Reload cache
            $this->reloadCache();
            
            return [
                'success' => true,
                'config' => $this->getConfigByKey($key)
            ];
        } catch (PDOException $e) {
            return [
                'success' => false,
                'message' => 'Cập nhật thất bại: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Reset về cấu hình mặc định
     */
    public function resetToDefault($category = 'all') {
        $defaultConfigs = $this->getDefaultConfigs();
        
        // Filter by category
        if ($category !== 'all') {
            $defaultConfigs = array_filter($defaultConfigs, function($config) use ($category) {
                return $config['category'] === $category;
            });
        }
        
        if (empty($defaultConfigs)) {
            return [
                'success' => false,
                'message' => 'Không tìm thấy cấu hình mặc định cho category này'
            ];
        }
        
        $this->db->beginTransaction();
        
        try {
            // Delete existing configs in category
            if ($category === 'all') {
                $this->db->query("DELETE FROM system_config");
            } else {
                $stmt = $this->db->prepare("DELETE FROM system_config WHERE category = ?");
                $stmt->execute([$category]);
            }
            
            // Insert default configs
            $stmt = $this->db->prepare("
                INSERT INTO system_config 
                (config_key, config_value, config_type, description, category) 
                VALUES (?, ?, ?, ?, ?)
            ");
            
            foreach ($defaultConfigs as $config) {
                $stmt->execute([
                    $config['key'],
                    $config['value'],
                    $config['type'],
                    $config['desc'],
                    $config['category']
                ]);
            }
            
            $this->db->commit();
            
            // Log activity
            $this->logConfigChange('system', 'reset', $category, 'Reset to default');
            
            // Reload cache
            $this->reloadCache();
            
            return [
                'success' => true,
                'message' => 'Reset cấu hình thành công',
                'count' => count($defaultConfigs)
            ];
        } catch (Exception $e) {
            $this->db->rollBack();
            return [
                'success' => false,
                'message' => 'Reset thất bại: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Lấy danh sách categories
     */
    public function getCategories() {
        $stmt = $this->db->query("
            SELECT DISTINCT category, COUNT(*) as count 
            FROM system_config 
            GROUP BY category 
            ORDER BY category
        ");
        
        $categories = $stmt->fetchAll();
        
        return [
            'categories' => $categories,
            'total' => count($categories)
        ];
    }
    
    /**
     * Validate giá trị cấu hình
     */
    private function validateConfigValue($value, $type) {
        switch ($type) {
            case 'number':
                if (!is_numeric($value)) {
                    return ['valid' => false, 'error' => 'Giá trị phải là số'];
                }
                break;
                
            case 'json':
                if (is_string($value)) {
                    json_decode($value);
                    if (json_last_error() !== JSON_ERROR_NONE) {
                        return ['valid' => false, 'error' => 'JSON không hợp lệ'];
                    }
                } elseif (!is_array($value)) {
                    return ['valid' => false, 'error' => 'Giá trị phải là JSON hoặc array'];
                }
                break;
                
            case 'boolean':
                if (!is_bool($value) && !in_array($value, ['0', '1', 'true', 'false', 0, 1])) {
                    return ['valid' => false, 'error' => 'Giá trị phải là boolean'];
                }
                break;
                
            case 'string':
            default:
                // String always valid
                break;
        }
        
        return ['valid' => true];
    }
    
    // Helper methods
    
    private function parseValue($value, $type) {
        switch ($type) {
            case 'number':
                return is_numeric($value) ? (strpos($value, '.') !== false ? (float)$value : (int)$value) : $value;
            case 'json':
                return json_decode($value, true);
            case 'boolean':
                return filter_var($value, FILTER_VALIDATE_BOOLEAN);
            default:
                return $value;
        }
    }
    
    private function formatValue($value, $type) {
        switch ($type) {
            case 'json':
                return is_array($value) ? json_encode($value) : $value;
            case 'boolean':
                return filter_var($value, FILTER_VALIDATE_BOOLEAN) ? '1' : '0';
            default:
                return (string)$value;
        }
    }
    
    private function reloadCache() {
        // Reload ConfigService cache
        $reflection = new ReflectionClass('ConfigService');
        $property = $reflection->getProperty('instance');
        $property->setAccessible(true);
        $property->setValue(null, null);
        
        // Force reload
        ConfigService::getInstance();
    }
    
    private function logConfigChange($configKey, $action, $oldValue, $newValue) {
        // Chỉ log vào error_log
        $logMessage = sprintf(
            "[Config Change] Key: %s | Action: %s | Old: %s | New: %s | Time: %s",
            $configKey,
            $action,
            $oldValue ? substr($oldValue, 0, 100) : 'null',
            $newValue ? substr($newValue, 0, 100) : 'null',
            date('Y-m-d H:i:s')
        );
        
        error_log($logMessage);
    }
    
   private function getDefaultConfigs() {
    return [
        // === PAYMENT - Thông tin ngân hàng ===
        [
            'key' => 'sepay_api_key',
            'value' => 'API_KEY_CUA_KVTEAM',
            'type' => 'string',
            'desc' => 'API Key từ SePay',
            'category' => 'payment'
        ],
        [
            'key' => 'vietqr_account',
            'value' => '0919332046',
            'type' => 'string',
            'desc' => 'Số tài khoản ngân hàng',
            'category' => 'payment'
        ],
        [
            'key' => 'vietqr_bank',
            'value' => 'MBBank',
            'type' => 'string',
            'desc' => 'Mã ngân hàng',
            'category' => 'payment'
        ],
        [
            'key' => 'vietqr_bank_name',
            'value' => 'Ngân hàng Quân đội (MBBank)',
            'type' => 'string',
            'desc' => 'Tên ngân hàng',
            'category' => 'payment'
        ],
        [
            'key' => 'vietqr_account_name',
            'value' => 'TÊN CHỦ TÀI KHOẢN',
            'type' => 'string',
            'desc' => 'Tên chủ tài khoản',
            'category' => 'payment'
        ],
        
        // === RECHARGE - Tỷ giá nạp xu ===
        [
            'key' => 'xu_exchange_rates',
            'value' => json_encode([
                '10000' => 10000000,
                '20000' => 20000000,
                '30000' => 30000000,
                '50000' => 60000000,
                '100000' => 130000000,
                '200000' => 280000000,
                '300000' => 435000000,
                '500000' => 750000000,
                '1000000' => 1700000000
            ]),
            'type' => 'json',
            'desc' => 'Bảng tỷ giá nạp xu (VNĐ => Xu)',
            'category' => 'recharge'
        ],
        [
            'key' => 'xu_bonus_multiplier',
            'value' => '1',
            'type' => 'number',
            'desc' => 'Hệ số nhân bonus xu (1 = 100%, 1.5 = 150%)',
            'category' => 'recharge'
        ],
        
        // === RECHARGE - Tỷ giá nạp lượng ===
        [
            'key' => 'luong_exchange_rate',
            'value' => '20',
            'type' => 'number',
            'desc' => 'Tỷ giá VNĐ -> Lượng (20 VNĐ = 1 lượng)',
            'category' => 'recharge'
        ],
        [
            'key' => 'luong_khoa_percent',
            'value' => '0.5',
            'type' => 'number',
            'desc' => 'Phần trăm lượng khóa (0.5 = 50%)',
            'category' => 'recharge'
        ],
        [
            'key' => 'luong_bonus_multiplier',
            'value' => '1',
            'type' => 'number',
            'desc' => 'Hệ số nhân bonus lượng (1 = 100%, 2 = 200%)',
            'category' => 'recharge'
        ],
        
        // === RECHARGE - Prefix nội dung chuyển khoản ===
        [
            'key' => 'recharge_xu_prefix',
            'value' => 'napxu',
            'type' => 'string',
            'desc' => 'Tiền tố nội dung chuyển khoản nạp xu',
            'category' => 'recharge'
        ],
        [
            'key' => 'recharge_luong_prefix',
            'value' => 'napluong',
            'type' => 'string',
            'desc' => 'Tiền tố nội dung chuyển khoản nạp lượng',
            'category' => 'recharge'
        ],
        
        // === ACTIVATION - Kích hoạt tài khoản ===
        [
            'key' => 'activation_amount',
            'value' => '20000',
            'type' => 'number',
            'desc' => 'Số tiền kích hoạt tài khoản (VNĐ)',
            'category' => 'activation'
        ],
        [
            'key' => 'activation_reward_xu',
            'value' => '10000000',
            'type' => 'number',
            'desc' => 'Xu thưởng khi kích hoạt thành công',
            'category' => 'activation'
        ],
        [
            'key' => 'activation_reward_luong',
            'value' => '50000',
            'type' => 'number',
            'desc' => 'Lượng thưởng khi kích hoạt thành công',
            'category' => 'activation'
        ],
        [
            'key' => 'activation_description_prefix',
            'value' => 'kichhoat',
            'type' => 'string',
            'desc' => 'Tiền tố nội dung chuyển khoản kích hoạt',
            'category' => 'activation'
        ]
    ];
}
}