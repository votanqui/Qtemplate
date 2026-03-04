<?php
// services/RechargeService.php
require_once __DIR__ . '/ConfigService.php';

class RechargeService {
    private $accountDb;
    private $gameDbs = [];
    private $config;
    
    public function __construct() {
        $this->accountDb = Database::getInstance()->getConnection();
        $this->config = ConfigService::getInstance();
        $this->initTables();
    }
    
    /**
     * ✅ HÀM MỚI: LẤY USER_ID TỪ PLAYER_NAME VIA tob_char
     */
    private function getUserIdFromPlayerName($playerName, $serverId = 1) {
        try {
            $gameDb = $this->getGameDb($serverId);
            
            $stmt = $gameDb->prepare("
                SELECT userId, charname 
                FROM tob_char 
                WHERE charname = ? 
                AND del = 1
                LIMIT 1
            ");
            $stmt->execute([$playerName]);
            $char = $stmt->fetch();
            
            if ($char) {
                return [
                    'success' => true,
                    'user_id' => (int)$char['userId'],
                    'player_name' => $char['charname']
                ];
            }
            
            return [
                'success' => false,
                'message' => 'Nhân vật không tồn tại'
            ];
            
        } catch (Exception $e) {
            error_log("Error getting userId from playerName: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi tra cứu nhân vật: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Khởi tạo bảng nếu chưa tồn tại
     */
    private function initTables() {
        try {
            $sql = "
                CREATE TABLE IF NOT EXISTS `recharge_orders` (
                    `id` INT AUTO_INCREMENT PRIMARY KEY,
                    `order_id` VARCHAR(50) UNIQUE NOT NULL,
                    `user_id` INT NOT NULL,
                    `player_name` VARCHAR(100) NOT NULL,
                    `server_id` INT DEFAULT 1,
                    `type` ENUM('recharge_xu', 'recharge_luong') NOT NULL,
                    `amount` DECIMAL(10,2) NOT NULL,
                    `expected_reward` BIGINT DEFAULT 0,
                    `expected_luong_khoa` BIGINT DEFAULT 0,
                    `status` ENUM('pending', 'paid', 'completed', 'failed', 'cancelled') DEFAULT 'pending',
                    `qr_code_url` TEXT,
                    `transaction_id` VARCHAR(100),
                    `bank_name` VARCHAR(50),
                    `payment_time` DATETIME,
                    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    INDEX idx_user_id (user_id),
                    INDEX idx_player_name (player_name),
                    INDEX idx_status (status),
                    INDEX idx_type (type),
                    INDEX idx_order_id (order_id)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
                
                CREATE TABLE IF NOT EXISTS `order_cancellation_logs` (
                    `id` INT AUTO_INCREMENT PRIMARY KEY,
                    `order_id` VARCHAR(50),
                    `user_id` INT NOT NULL,
                    `player_name` VARCHAR(100),
                    `reason` VARCHAR(100),
                    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    INDEX idx_user_id (user_id),
                    INDEX idx_player_name (player_name),
                    INDEX idx_order_id (order_id),
                    INDEX idx_created_at (created_at)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            ";
            $this->accountDb->exec($sql);
            
        } catch (Exception $e) {
            error_log("Init tables error: " . $e->getMessage());
        }
    }
    
    /**
     * Tạo lệnh nạp xu - ĐÃ SỬA: TRA userId TỪ tob_char
     */
    public function createXuRechargeOrder($playerName, $serverId, $amount) {
        try {
            // Kiểm tra mệnh giá hợp lệ
            $xuRates = $this->config->get('xu_exchange_rates', []);
            $xuBonusMultiplier = $this->config->get('xu_bonus_multiplier', 1);

            if (!isset($xuRates[$amount])) {
                return [
                    'success' => false,
                    'message' => 'Mệnh giá không hợp lệ. Mệnh giá hợp lệ: ' . implode(', ', array_keys($xuRates))
                ];
            }

            $xuAmount = $xuRates[$amount] * $xuBonusMultiplier;
            
            // ✅ TRA userId TỪ tob_char
            $userInfo = $this->getUserIdFromPlayerName($playerName, $serverId);
            
            if (!$userInfo['success']) {
                return [
                    'success' => false,
                    'message' => 'Nhân vật không tồn tại trong game'
                ];
            }
            
            $userId = $userInfo['user_id']; // userId THẬT từ tob_char
            
            // Kiểm tra có order pending không
            $pendingStmt = $this->accountDb->prepare("
                SELECT * FROM recharge_orders 
                WHERE player_name = ? 
                AND server_id = ?
                AND type = 'recharge_xu'
                AND status = 'pending'
                AND created_at >= DATE_SUB(NOW(), INTERVAL 30 MINUTE)
                LIMIT 1
            ");
            $pendingStmt->execute([$playerName, $serverId]);
            
            if ($pendingStmt->fetch()) {
                return [
                    'success' => false,
                    'message' => 'Bạn đang có lệnh nạp xu chờ thanh toán'
                ];
            }
            
            // Tạo order ID
            $orderId = 'XU_' . date('YmdHis') . '_' . $playerName;
            
            // Tạo nội dung chuyển khoản
            $prefix = $this->config->get('recharge_xu_prefix', 'napxu');
            $description = "{$prefix} {$playerName} {$serverId}";
            
            // Tạo QR code URL
            $qrUrl = "https://qr.sepay.vn/img?" . http_build_query([
                'acc' => $this->config->get('vietqr_account'),
                'bank' => $this->config->get('vietqr_bank'),
                'amount' => $amount,
                'des' => $description,
                'template' => 'compact'
            ]);
            
            // Lưu vào database với userId THẬT
            $stmt = $this->accountDb->prepare("
                INSERT INTO recharge_orders 
                (order_id, user_id, player_name, server_id, type, amount, expected_reward, qr_code_url, status) 
                VALUES (?, ?, ?, ?, 'recharge_xu', ?, ?, ?, 'pending')
            ");
            
            $stmt->execute([
                $orderId,
                $userId,  // userId THẬT từ tob_char
                $playerName,
                $serverId,
                $amount,
                $xuAmount,
                $qrUrl
            ]);
            
            return [
                'success' => true,
                'data' => [
                    'order_id' => $orderId,
                    'user_id' => $userId,  // userId THẬT
                    'player_name' => $playerName,
                    'server_id' => $serverId,
                    'type' => 'recharge_xu',
                    'amount' => $amount,
                    'expected_xu' => $xuAmount,
                    'qr_code_url' => $qrUrl,
                    'description' => $description,
                    'bank_info' => [
                        'account_number' => $this->config->get('vietqr_account'),
                        'bank_name' => $this->config->get('vietqr_bank_name'),
                        'account_name' => $this->config->get('vietqr_account_name')
                    ],
                    'expires_at' => date('Y-m-d H:i:s', strtotime('+30 minutes')),
                    'instructions' => [
                        '1. Quét mã QR hoặc chuyển khoản đến số tài khoản trên',
                        '2. Số tiền: ' . number_format($amount) . ' VNĐ',
                        '3. Nội dung chuyển khoản: ' . $description,
                        '4. Sau khi chuyển khoản, hệ thống sẽ tự động nạp xu trong 5-10 phút',
                        '5. Bạn sẽ nhận được: ' . number_format($xuAmount) . ' xu',
                        '6. Kiểm tra trạng thái: /recharge/verify?order_id=' . $orderId
                    ]
                ]
            ];
            
        } catch (Exception $e) {
            error_log("Create xu order error: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi tạo lệnh nạp xu: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Tạo lệnh nạp lượng - ĐÃ SỬA: TRA userId TỪ tob_char
     */
    public function createLuongRechargeOrder($playerName, $serverId, $amount) {
        try {
            // Kiểm tra số tiền tối thiểu
            $luongExchangeRate = $this->config->get('luong_exchange_rate', 20);
            $luongKhoaPercent = $this->config->get('luong_khoa_percent', 0.5);
            $luongBonusMultiplier = $this->config->get('luong_bonus_multiplier', 1);

            if ($amount < $luongExchangeRate) {
                return [
                    'success' => false,
                    'message' => 'Số tiền tối thiểu: ' . number_format($luongExchangeRate) . ' VNĐ'
                ];
            }

            // Tính lượng
            $baseLuongAmount = floor($amount / $luongExchangeRate);
            $baseLuongKhoaAmount = floor($baseLuongAmount * $luongKhoaPercent);
            
            $luongAmount = floor($baseLuongAmount * $luongBonusMultiplier);
            $luongKhoaAmount = floor($baseLuongKhoaAmount * $luongBonusMultiplier);
            
            // ✅ TRA userId TỪ tob_char
            $userInfo = $this->getUserIdFromPlayerName($playerName, $serverId);
            
            if (!$userInfo['success']) {
                return [
                    'success' => false,
                    'message' => 'Nhân vật không tồn tại trong game'
                ];
            }
            
            $userId = $userInfo['user_id']; // userId THẬT từ tob_char
            
            // Kiểm tra có order pending không
            $pendingStmt = $this->accountDb->prepare("
                SELECT * FROM recharge_orders 
                WHERE player_name = ? 
                AND server_id = ?
                AND type = 'recharge_luong'
                AND status = 'pending'
                AND created_at >= DATE_SUB(NOW(), INTERVAL 30 MINUTE)
                LIMIT 1
            ");
            $pendingStmt->execute([$playerName, $serverId]);
            
            if ($pendingStmt->fetch()) {
                return [
                    'success' => false,
                    'message' => 'Bạn đang có lệnh nạp lượng chờ thanh toán'
                ];
            }
            
            // Tạo order ID
            $orderId = 'LUONG_' . date('YmdHis') . '_' . $playerName;
            
            // Tạo nội dung chuyển khoản
            $prefix = $this->config->get('recharge_luong_prefix', 'napluong');
            $description = "{$prefix} {$playerName} {$serverId}";
            
            // Tạo QR code URL
            $qrUrl = "https://qr.sepay.vn/img?" . http_build_query([
                'acc' => $this->config->get('vietqr_account'),
                'bank' => $this->config->get('vietqr_bank'),
                'amount' => $amount,
                'des' => $description,
                'template' => 'compact'
            ]);
            
            // Lưu vào database với userId THẬT
            $stmt = $this->accountDb->prepare("
                INSERT INTO recharge_orders 
                (order_id, user_id, player_name, server_id, type, amount, expected_reward, expected_luong_khoa, qr_code_url, status) 
                VALUES (?, ?, ?, ?, 'recharge_luong', ?, ?, ?, ?, 'pending')
            ");
            
            $stmt->execute([
                $orderId,
                $userId,  // userId THẬT từ tob_char
                $playerName,
                $serverId,
                $amount,
                $luongAmount,
                $luongKhoaAmount,
                $qrUrl
            ]);
            
            return [
                'success' => true,
                'data' => [
                    'order_id' => $orderId,
                    'user_id' => $userId,  // userId THẬT
                    'player_name' => $playerName,
                    'server_id' => $serverId,
                    'type' => 'recharge_luong',
                    'amount' => $amount,
                    'expected_luong' => $luongAmount,
                    'expected_luong_khoa' => $luongKhoaAmount,
                    'qr_code_url' => $qrUrl,
                    'description' => $description,
                    'bank_info' => [
                        'account_number' => $this->config->get('vietqr_account'),
                        'bank_name' => $this->config->get('vietqr_bank_name'),
                        'account_name' => $this->config->get('vietqr_account_name')
                    ],
                    'expires_at' => date('Y-m-d H:i:s', strtotime('+30 minutes')),
                    'instructions' => [
                        '1. Quét mã QR hoặc chuyển khoản đến số tài khoản trên',
                        '2. Số tiền: ' . number_format($amount) . ' VNĐ',
                        '3. Nội dung chuyển khoản: ' . $description,
                        '4. Sau khi chuyển khoản, hệ thống sẽ tự động nạp lượng trong 5-10 phút',
                        '5. Bạn sẽ nhận được:',
                        '   - Lượng: ' . number_format($luongAmount),
                        '   - Lượng khóa: ' . number_format($luongKhoaAmount),
                        '6. Kiểm tra trạng thái: /recharge/verify?order_id=' . $orderId
                    ]
                ]
            ];
            
        } catch (Exception $e) {
            error_log("Create luong order error: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi tạo lệnh nạp lượng: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Hủy lệnh nạp đang pending
     */
    public function cancelPendingOrder($orderId, $playerName = null) {
        try {
            $sql = "SELECT * FROM recharge_orders WHERE order_id = ?";
            $params = [$orderId];
            
            if ($playerName !== null) {
                $sql .= " AND player_name = ?";
                $params[] = $playerName;
            }
            
            $sql .= " AND status = 'pending' LIMIT 1";
            
            $stmt = $this->accountDb->prepare($sql);
            $stmt->execute($params);
            $order = $stmt->fetch();
            
            if (!$order) {
                return [
                    'success' => false,
                    'message' => 'Order không tồn tại hoặc không thể hủy'
                ];
            }
            
            $updateStmt = $this->accountDb->prepare("
                UPDATE recharge_orders 
                SET status = 'cancelled', 
                    updated_at = NOW() 
                WHERE order_id = ?
            ");
            $updateStmt->execute([$orderId]);
            
            $this->logCancellation($orderId, $order['user_id'], $order['player_name'], 'user_cancelled');
            
            return [
                'success' => true,
                'message' => 'Đã hủy lệnh nạp thành công',
                'data' => [
                    'order_id' => $orderId,
                    'user_id' => $order['user_id'],
                    'player_name' => $order['player_name'],
                    'type' => $order['type'],
                    'amount' => $order['amount'],
                    'cancelled_at' => date('Y-m-d H:i:s')
                ]
            ];
            
        } catch (Exception $e) {
            error_log("Cancel order error: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi hủy lệnh nạp: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Hủy tất cả order pending của player
     */
    public function cancelAllPendingOrders($playerName, $serverId = null) {
        try {
            // Lấy danh sách order trước khi hủy để log
            $selectSql = "SELECT order_id, user_id FROM recharge_orders 
                         WHERE player_name = ? 
                         AND status = 'pending'";
            
            $selectParams = [$playerName];
            
            if ($serverId !== null) {
                $selectSql .= " AND server_id = ?";
                $selectParams[] = $serverId;
            }
            
            $selectStmt = $this->accountDb->prepare($selectSql);
            $selectStmt->execute($selectParams);
            $ordersToCancel = $selectStmt->fetchAll();
            
            if (empty($ordersToCancel)) {
                return [
                    'success' => true,
                    'message' => 'Không có lệnh nạp nào đang chờ',
                    'data' => [
                        'player_name' => $playerName,
                        'server_id' => $serverId,
                        'cancelled_count' => 0,
                        'cancelled_at' => date('Y-m-d H:i:s')
                    ]
                ];
            }
            
            // Cập nhật trạng thái
            $updateSql = "UPDATE recharge_orders 
                         SET status = 'cancelled', 
                             updated_at = NOW() 
                         WHERE player_name = ? 
                         AND status = 'pending'";
            
            $updateParams = [$playerName];
            
            if ($serverId !== null) {
                $updateSql .= " AND server_id = ?";
                $updateParams[] = $serverId;
            }
            
            $updateStmt = $this->accountDb->prepare($updateSql);
            $updateStmt->execute($updateParams);
            
            $affectedRows = $updateStmt->rowCount();
            
            // Log từng order đã hủy
            foreach ($ordersToCancel as $order) {
                $this->logCancellation($order['order_id'], $order['user_id'], $playerName, 'cancel_all');
            }
            
            return [
                'success' => true,
                'message' => 'Đã hủy ' . $affectedRows . ' lệnh nạp đang chờ',
                'data' => [
                    'player_name' => $playerName,
                    'server_id' => $serverId,
                    'cancelled_count' => $affectedRows,
                    'cancelled_at' => date('Y-m-d H:i:s')
                ]
            ];
            
        } catch (Exception $e) {
            error_log("Cancel all orders error: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi hủy lệnh nạp: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Kiểm tra và tạo order mới (tự động hủy order cũ nếu có)
     */
    public function createNewOrderWithAutoCancel($playerName, $serverId, $amount, $type) {
        try {
            $cancelResult = $this->cancelAllPendingOrders($playerName, $serverId);
            
            if (!$cancelResult['success']) {
                return $cancelResult;
            }
            
            if ($type === 'xu') {
                return $this->createXuRechargeOrder($playerName, $serverId, $amount);
            } elseif ($type === 'luong') {
                return $this->createLuongRechargeOrder($playerName, $serverId, $amount);
            } else {
                return [
                    'success' => false,
                    'message' => 'Loại nạp không hợp lệ'
                ];
            }
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi tạo order mới: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Lấy danh sách order pending của player
     */
    public function getPendingOrders($playerName, $serverId = null) {
        try {
            $sql = "SELECT * FROM recharge_orders 
                    WHERE player_name = ? 
                    AND status = 'pending'
                    AND created_at >= DATE_SUB(NOW(), INTERVAL 30 MINUTE)";
            
            $params = [$playerName];
            
            if ($serverId !== null) {
                $sql .= " AND server_id = ?";
                $params[] = $serverId;
            }
            
            $sql .= " ORDER BY created_at DESC";
            
            $stmt = $this->accountDb->prepare($sql);
            $stmt->execute($params);
            $orders = $stmt->fetchAll();
            
            foreach ($orders as &$order) {
                $createdTime = strtotime($order['created_at']);
                $expireTime = $createdTime + (30 * 60);
                $remainingTime = $expireTime - time();
                $order['remaining_minutes'] = max(0, floor($remainingTime / 60));
                $order['expires_at'] = date('Y-m-d H:i:s', $expireTime);
            }
            
            return [
                'success' => true,
                'data' => [
                    'orders' => $orders,
                    'total' => count($orders)
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy danh sách order: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Lấy QR code
     */
    public function getQRCode($orderId) {
        try {
            $stmt = $this->accountDb->prepare("
                SELECT * FROM recharge_orders 
                WHERE order_id = ? 
                AND status = 'pending'
                AND created_at >= DATE_SUB(NOW(), INTERVAL 30 MINUTE)
                LIMIT 1
            ");
            $stmt->execute([$orderId]);
            $order = $stmt->fetch();
            
            if (!$order) {
                return [
                    'success' => false,
                    'message' => 'Order không tồn tại hoặc đã hết hạn'
                ];
            }
            
            $prefix = $order['type'] === 'recharge_xu' 
                ? $this->config->get('recharge_xu_prefix', 'napxu')
                : $this->config->get('recharge_luong_prefix', 'napluong');
            $description = "{$prefix} {$order['player_name']} {$order['server_id']}";
            
            return [
                'success' => true,
                'data' => [
                    'order_id' => $order['order_id'],
                    'type' => $order['type'],
                    'qr_code_url' => $order['qr_code_url'],
                    'amount' => $order['amount'],
                    'description' => $description,
                    'expected_reward' => $order['expected_reward'],
                    'expected_luong_khoa' => $order['expected_luong_khoa'],
                    'status' => $order['status'],
                    'created_at' => $order['created_at'],
                    'expires_at' => date('Y-m-d H:i:s', strtotime($order['created_at'] . ' + 30 minutes'))
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
     * Kiểm tra trạng thái order
     */
    public function verifyOrder($orderId) {
        try {
            $stmt = $this->accountDb->prepare("
                SELECT order_id, type, status, updated_at 
                FROM recharge_orders 
                WHERE order_id = ?
                LIMIT 1
            ");
            $stmt->execute([$orderId]);
            $order = $stmt->fetch();
            
            if (!$order) {
                return [
                    'success' => false,
                    'message' => 'Order không tồn tại'
                ];
            }
            
            return [
                'success' => true,
                'data' => [
                    'order_id' => $order['order_id'],
                    'type' => $order['type'],
                    'status' => $order['status'],
                    'last_update' => $order['updated_at'],
                    'message' => $this->mapStatusMessage($order['status'])
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
     * Lấy lịch sử nạp theo player name
     */
    public function getRechargeHistory($playerName, $type = null, $page = 1, $limit = 10) {
        try {
            $offset = ($page - 1) * $limit;
            
            $sql = "SELECT * FROM recharge_orders WHERE player_name = ?";
            $params = [$playerName];
            
            if ($type === 'xu' || $type === 'luong') {
                $sql .= " AND type = ?";
                $params[] = 'recharge_' . $type;
            }
            
            $sql .= " ORDER BY created_at DESC LIMIT ? OFFSET ?";
            
            $stmt = $this->accountDb->prepare($sql);
            foreach ($params as $i => $value) {
                $stmt->bindValue($i + 1, $value, is_int($value) ? PDO::PARAM_INT : PDO::PARAM_STR);
            }
            $stmt->bindValue(count($params) + 1, $limit, PDO::PARAM_INT);
            $stmt->bindValue(count($params) + 2, $offset, PDO::PARAM_INT);
            $stmt->execute();
            
            $history = $stmt->fetchAll();
            
            $countSql = "SELECT COUNT(*) as total FROM recharge_orders WHERE player_name = ?";
            $countParams = [$playerName];

            if ($type === 'xu' || $type === 'luong') {
                $countSql .= " AND type = ?";
                $countParams[] = 'recharge_' . $type;
            }

            $countStmt = $this->accountDb->prepare($countSql);
            $countStmt->execute($countParams);
            $total = (int)$countStmt->fetchColumn();
            
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
     * Map status message
     */
    private function mapStatusMessage($status) {
        switch ($status) {
            case 'pending':
                return 'Chờ thanh toán';
            case 'paid':
                return 'Đã thanh toán, đang xử lý';
            case 'completed':
                return 'Hoàn thành';
            case 'failed':
                return 'Thất bại';
            case 'cancelled':
                return 'Đã hủy';
            default:
                return 'Không xác định';
        }
    }
    
    /**
     * Log hành động hủy order
     */
    private function logCancellation($orderId, $userId, $playerName, $reason) {
        try {
            $stmt = $this->accountDb->prepare("
                INSERT INTO order_cancellation_logs 
                (order_id, user_id, player_name, reason, created_at) 
                VALUES (?, ?, ?, ?, NOW())
            ");
            $stmt->execute([$orderId, $userId, $playerName, $reason]);
        } catch (Exception $e) {
            error_log("Failed to log cancellation: " . $e->getMessage());
        }
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