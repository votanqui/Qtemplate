<?php
// services/ActivationService.php
require_once __DIR__ . '/ConfigService.php';

class ActivationService {
    private $accountDb;
    private $gameDbs = [];
    private $config;
    
    public function __construct() {
        $this->accountDb = Database::getInstance()->getConnection();
        $this->config = ConfigService::getInstance();
        $this->initTables();
    }
    
    /**
     * Khởi tạo bảng nếu chưa tồn tại
     */
    private function initTables() {
        try {
            // Bảng lưu yêu cầu kích hoạt
            $sql = "
                CREATE TABLE IF NOT EXISTS `activation_requests` (
                    `id` INT AUTO_INCREMENT PRIMARY KEY,
                    `order_id` VARCHAR(50) UNIQUE NOT NULL,
                    `user_id` INT NOT NULL,
                    `username` VARCHAR(100) NOT NULL,
                    `server_id` INT DEFAULT 1,
                    `amount` DECIMAL(10,2) DEFAULT 20000.00,
                    `status` ENUM('pending', 'paid', 'completed', 'failed', 'cancelled') DEFAULT 'pending',
                    `qr_code_url` TEXT,
                    `transaction_id` VARCHAR(100),
                    `bank_name` VARCHAR(50),
                    `payment_time` DATETIME,
                    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    INDEX idx_user_id (user_id),
                    INDEX idx_username (username),
                    INDEX idx_status (status),
                    INDEX idx_order_id (order_id)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            ";
            $this->accountDb->exec($sql);
            
        } catch (Exception $e) {
            error_log("Init tables error: " . $e->getMessage());
        }
    }
    
    /**
     * Lấy thông tin user theo username
     */
    private function getUserInfoByUsername($username) {
        $stmt = $this->accountDb->prepare("
            SELECT id, username, phone, active 
            FROM team_user 
            WHERE username = ? LIMIT 1
        ");
        $stmt->execute([$username]);
        return $stmt->fetch();
    }
    
    /**
     * Lấy thông tin user theo ID
     */
    private function getUserInfo($userId) {
        $stmt = $this->accountDb->prepare("
            SELECT id, username, phone, active 
            FROM team_user 
            WHERE id = ? LIMIT 1
        ");
        $stmt->execute([$userId]);
        return $stmt->fetch();
    }
    
    /**
     * Kiểm tra trạng thái kích hoạt của user (hỗ trợ cả username và userId)
     */
    public function checkActivationStatus($identifier, $serverId = 1) {
        try {
            // Xác định identifier là username hay userId
            $user = null;
            if (is_numeric($identifier)) {
                $user = $this->getUserInfo($identifier);
            } else {
                $user = $this->getUserInfoByUsername($identifier);
            }
            
            if (!$user) {
                return [
                    'success' => false,
                    'message' => 'Tài khoản không tồn tại'
                ];
            }
            
            $userId = $user['id'];
            $username = $user['username'];
            
            // Kiểm tra trong bảng 5h_active của game database
            $gameDb = $this->getGameDb($serverId);
            $stmt = $gameDb->prepare("SELECT * FROM `5h_active` WHERE `userID` = ? LIMIT 1");
            $stmt->execute([$userId]);
            $activation = $stmt->fetch();
            
            return [
                'success' => true,
                'data' => [
                    'user_id' => $userId,
                    'username' => $username,
                    'is_activated' => ($activation !== false),
                    'activation_date' => $activation ? $activation['created_at'] : null,
                    'user_status' => $user['active'],
                    'server_id' => $serverId
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi kiểm tra trạng thái: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Tạo yêu cầu kích hoạt mới (sử dụng username)
     * @param string $username Username của tài khoản
     * @param int $serverId Server ID
     */
    public function createActivationRequest($username, $serverId) {
        try {
            // Kiểm tra user tồn tại theo username
            $user = $this->getUserInfoByUsername($username);
            
            if (!$user) {
                return [
                    'success' => false,
                    'message' => 'Tài khoản không tồn tại: ' . $username
                ];
            }
            
            $userId = $user['id'];
            
            // Kiểm tra đã kích hoạt chưa
            $gameDb = $this->getGameDb($serverId);
            $checkStmt = $gameDb->prepare("SELECT * FROM `5h_active` WHERE `userID` = ? LIMIT 1");
            $checkStmt->execute([$userId]);
            
            if ($checkStmt->fetch()) {
                return [
                    'success' => false,
                    'message' => 'Tài khoản đã được kích hoạt'
                ];
            }
            
            // Tự động chuyển các request pending cũ sang failed
            $updatePendingStmt = $this->accountDb->prepare("
                UPDATE activation_requests 
                SET status = 'failed'
                WHERE user_id = ? 
                AND server_id = ? 
                AND status = 'pending'
            ");
            $updatePendingStmt->execute([$userId, $serverId]);
            
            // Log số lượng request đã hủy (optional)
            $canceledCount = $updatePendingStmt->rowCount();
            if ($canceledCount > 0) {
                error_log("Auto-canceled {$canceledCount} pending activation request(s) for user {$username} (ID: {$userId}), server {$serverId}");
            }
            
            // Tạo order ID
            $orderId = 'ACT' . date('YmdHis') . str_pad($userId, 6, '0', STR_PAD_LEFT);
            
            // Tạo nội dung chuyển khoản - SỬ DỤNG USERNAME
            $prefix = $this->config->get('activation_description_prefix', 'kichhoat');
            $description = "{$prefix} {$username} {$serverId}";
            
            // Tạo QR code URL (VietQR)
            $qrUrl = "https://qr.sepay.vn/img?" . http_build_query([
                'acc' => $this->config->get('vietqr_account'),
                'bank' => $this->config->get('vietqr_bank'),
                'amount' => $this->config->get('activation_amount'),
                'des' => $description,
                'template' => 'compact'
            ]);
            
            // Lưu vào database
            $stmt = $this->accountDb->prepare("
                INSERT INTO activation_requests 
                (order_id, user_id, username, server_id, amount, qr_code_url, status) 
                VALUES (?, ?, ?, ?, ?, ?, 'pending')
            ");
            
            $stmt->execute([
                $orderId,
                $userId,
                $username,
                $serverId,
                $this->config->get('activation_amount'),
                $qrUrl
            ]);
            
            return [
                'success' => true,
                'data' => [
                    'order_id' => $orderId,
                    'user_id' => $userId,
                    'username' => $username,
                    'server_id' => $serverId,
                    'amount' => $this->config->get('activation_amount'),
                    'qr_code_url' => $qrUrl,
                    'description' => $description,
                    'bank_info' => [
                        'account_number' => $this->config->get('vietqr_account'),
                        'bank_name' => $this->config->get('vietqr_bank_name'),
                        'account_name' => $this->config->get('vietqr_account_name')
                    ],
                    'expires_at' => date('Y-m-d H:i:s', strtotime('+15 minutes')),
                    'instructions' => [
                        '1. Quét mã QR hoặc chuyển khoản đến số tài khoản trên',
                        '2. Số tiền: ' . number_format($this->config->get('activation_amount')) . ' VNĐ',
                        '3. Nội dung: ' . $description,
                        '4. Sau khi chuyển khoản, hệ thống sẽ tự động kích hoạt trong 5-10 phút',
                        '5. Bạn có thể kiểm tra trạng thái bằng API /activation/verify?order_id=' . $orderId
                    ]
                ]
            ];
            
        } catch (Exception $e) {
            // Thêm log chi tiết
            error_log("Activation error details: " . $e->getMessage());
            error_log("Trace: " . $e->getTraceAsString());
            
            return [
                'success' => false,
                'message' => 'Lỗi khi tạo yêu cầu kích hoạt: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Lấy QR code
     */
    public function getQRCode($orderId) {
        try {
            $stmt = $this->accountDb->prepare("
                SELECT * FROM activation_requests 
                WHERE order_id = ? 
                AND status = 'pending'
                AND created_at >= DATE_SUB(NOW(), INTERVAL 30 MINUTE)
                LIMIT 1
            ");
            $stmt->execute([$orderId]);
            $request = $stmt->fetch();
            
            if (!$request) {
                return [
                    'success' => false,
                    'message' => 'Order không tồn tại hoặc đã hết hạn'
                ];
            }
            
            $prefix = $this->config->get('activation_description_prefix', 'kichhoat');
            
            return [
                'success' => true,
                'data' => [
                    'order_id' => $request['order_id'],
                    'qr_code_url' => $request['qr_code_url'],
                    'amount' => $request['amount'],
                    'description' => "{$prefix} {$request['username']} {$request['server_id']}",
                    'status' => $request['status'],
                    'created_at' => $request['created_at'],
                    'expires_at' => date('Y-m-d H:i:s', strtotime($request['created_at'] . ' + 15 minutes'))
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy QR code: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Kiểm tra thanh toán
     */
    public function verifyPayment($orderId) {
        try {
            $stmt = $this->accountDb->prepare("
                SELECT order_id, status, updated_at 
                FROM activation_requests 
                WHERE order_id = ?
                LIMIT 1
            ");
            $stmt->execute([$orderId]);
            $request = $stmt->fetch();

            if (!$request) {
                return [
                    'success' => false,
                    'message' => 'Order không tồn tại'
                ];
            }

            return [
                'success' => true,
                'data' => [
                    'order_id' => $request['order_id'],
                    'status' => $request['status'], // pending | paid | completed
                    'last_update' => $request['updated_at'],
                    'message' => $this->mapStatusMessage($request['status'])
                ]
            ];

        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi kiểm tra trạng thái: ' . $e->getMessage()
            ];
        }
    }

    private function mapStatusMessage($status) {
        switch ($status) {
            case 'pending':
                return 'Chưa nhận được thanh toán';
            case 'paid':
                return 'Đã nhận thanh toán, đang xử lý';
            case 'completed':
                return 'Đã kích hoạt thành công';
            case 'failed':
                return 'Kích hoạt thất bại';
            case 'cancelled':
                return 'Đã hủy';
            default:
                return 'Trạng thái không xác định';
        }
    }

    /**
     * Xử lý kích hoạt tài khoản (sử dụng username)
     */
    private function processActivation($username, $serverId = 1) {
        try {
            // Lấy thông tin user
            $user = $this->getUserInfoByUsername($username);
            
            if (!$user) {
                return [
                    'success' => false,
                    'message' => 'Tài khoản không tồn tại: ' . $username
                ];
            }
            
            $userId = $user['id'];
            $gameDb = $this->getGameDb($serverId);
            
            // Kiểm tra đã kích hoạt chưa
            $checkStmt = $gameDb->prepare("SELECT * FROM `5h_active` WHERE `userID` = ? LIMIT 1");
            $checkStmt->execute([$userId]);
            
            if ($checkStmt->fetch()) {
                return [
                    'success' => false,
                    'message' => 'Tài khoản đã được kích hoạt'
                ];
            }
            
            // Thêm vào bảng 5h_active
            $gameDb->prepare("
                INSERT INTO `5h_active` (`userID`, `username`, `time_end`) 
                VALUES (?, ?, ?)
            ")->execute([$userId, $username, -1]);
            
            $rewardXu = $this->config->get('activation_reward_xu');
            $rewardLuong = $this->config->get('activation_reward_luong');
            
            // Thêm phần thưởng - SỬ DỤNG USERNAME
            $gameDb->prepare("
                INSERT INTO `board_created` 
                (`xu`, `luong`, `luongK`, `svID`, `username`, `ve`) 
                VALUES (?, ?, ?, '-1', ?, '0')
            ")->execute([$rewardXu, $rewardLuong, $rewardLuong, $username]);
            
            return [
                'success' => true,
                'rewards' => [
                    'xu' => $rewardXu,
                    'luong' => $rewardLuong,
                    'luongK' => $rewardLuong,
                    'message' => 'Kích hoạt thành công và nhận được phần thưởng'
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi kích hoạt: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Lấy lịch sử kích hoạt (hỗ trợ cả username và userId)
     */
    public function getActivationHistory($identifier, $page = 1, $limit = 10) {
        try {
            // Xác định identifier là username hay userId
            $user = null;
            if (is_numeric($identifier)) {
                $user = $this->getUserInfo($identifier);
            } else {
                $user = $this->getUserInfoByUsername($identifier);
            }
            
            if (!$user) {
                return [
                    'success' => false,
                    'message' => 'Tài khoản không tồn tại'
                ];
            }
            
            $userId = $user['id'];
            $offset = ($page - 1) * $limit;
            
            $stmt = $this->accountDb->prepare("
                SELECT * FROM activation_requests 
                WHERE user_id = ? 
                ORDER BY created_at DESC 
                LIMIT ? OFFSET ?
            ");
            $stmt->bindValue(1, $userId, PDO::PARAM_INT);
            $stmt->bindValue(2, $limit, PDO::PARAM_INT);
            $stmt->bindValue(3, $offset, PDO::PARAM_INT);
            $stmt->execute();
            
            $history = $stmt->fetchAll();
            
            // Đếm tổng
            $countStmt = $this->accountDb->prepare("
                SELECT COUNT(*) as total FROM activation_requests 
                WHERE user_id = ?
            ");
            $countStmt->execute([$userId]);
            $total = $countStmt->fetch()['total'];
            
            return [
                'success' => true,
                'data' => [
                    'history' => $history,
                    'pagination' => [
                        'page' => $page,
                        'limit' => $limit,
                        'total' => $total,
                        'total_pages' => ceil($total / $limit)
                    ]
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy lịch sử: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Kiểm tra yêu cầu pending của user (sử dụng username)
     */
    public function checkPendingRequest($username, $serverId) {
        try {
            // Lấy thông tin user
            $user = $this->getUserInfoByUsername($username);
            
            if (!$user) {
                return [
                    'success' => false,
                    'message' => 'Tài khoản không tồn tại: ' . $username
                ];
            }
            
            $userId = $user['id'];
            
            // Kiểm tra có yêu cầu pending không
            $stmt = $this->accountDb->prepare("
                SELECT * FROM activation_requests 
                WHERE user_id = ? 
                AND server_id = ? 
                AND status = 'pending'
                AND created_at >= DATE_SUB(NOW(), INTERVAL 30 MINUTE)
                ORDER BY created_at DESC
                LIMIT 1
            ");
            $stmt->execute([$userId, $serverId]);
            $request = $stmt->fetch();
            
            if (!$request) {
                return [
                    'success' => true,
                    'has_pending' => false,
                    'data' => null
                ];
            }
            
            // Có request pending, trả về thông tin QR - SỬ DỤNG USERNAME
            $prefix = $this->config->get('activation_description_prefix', 'kichhoat');
            
            return [
                'success' => true,
                'has_pending' => true,
                'data' => [
                    'order_id' => $request['order_id'],
                    'user_id' => $request['user_id'],
                    'username' => $request['username'],
                    'server_id' => $request['server_id'],
                    'amount' => $request['amount'],
                    'qr_code_url' => $request['qr_code_url'],
                    'description' => "{$prefix} {$request['username']} {$request['server_id']}",
                    'bank_info' => [
                        'account_number' => $this->config->get('vietqr_account'),
                        'bank_name' => $this->config->get('vietqr_bank_name'),
                        'account_name' => $this->config->get('vietqr_account_name')
                    ],
                    'created_at' => $request['created_at'],
                    'expires_at' => date('Y-m-d H:i:s', strtotime($request['created_at'] . ' + 15 minutes')),
                    'instructions' => [
                        '1. Quét mã QR hoặc chuyển khoản đến số tài khoản trên',
                        '2. Số tiền: ' . number_format($request['amount']) . ' VNĐ',
                        '3. Nội dung: ' . "{$prefix} {$request['username']} {$request['server_id']}",
                        '4. Sau khi chuyển khoản, hệ thống sẽ tự động kích hoạt trong 5-10 phút',
                        '5. Bạn có thể kiểm tra trạng thái bằng API /activation/verify?order_id=' . $request['order_id']
                    ]
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi kiểm tra yêu cầu pending: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Xử lý webhook từ VietQR
     */
    public function processWebhook($data) {
        // TODO: Xử lý webhook thực tế từ VietQR
        // Đây chỉ là mock function
        
        return [
            'success' => true,
            'message' => 'Webhook processed'
        ];
    }
    
    /**
     * Mock function kiểm tra thanh toán VietQR
     */
    private function mockCheckVietQRPayment($orderId) {
        // Trong thực tế, đây sẽ là API call đến VietQR
        // Tạm thời trả về true cho 50% request để test
        return rand(0, 1) === 1;
    }
    
    /**
     * Lấy kết nối game database
     */
    private function getGameDb($serverId = null) {
        if ($serverId === null) {
            $serverId = Config::DEFAULT_SERVER_ID;
        }
        
        if (!isset($this->gameDbs[$serverId])) {
            $dbInstance = Database::getGameInstance($serverId);
            $this->gameDbs[$serverId] = $dbInstance->getConnection();
        }
        
        return $this->gameDbs[$serverId];
    }
}