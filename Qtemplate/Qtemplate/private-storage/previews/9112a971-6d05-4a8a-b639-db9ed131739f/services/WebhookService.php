<?php
// services/WebhookService.php
require_once __DIR__ . '/ConfigService.php';

class WebhookService {
    private $accountDb;
    private $gameDbs = [];
    private $config;
    
    public function __construct() {
        $this->accountDb = Database::getInstance()->getConnection();
        $this->config = ConfigService::getInstance();
    }
    
    /**
     * LẤY USER_ID TỪ PLAYER_NAME VIA tob_char
     * Đây là hàm quan trọng nhất - tra userId thực từ game DB
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
     * Xử lý webhook từ SePay (tự động phân loại)
     */
    public function processSepayWebhook($data) {
        try {
            $this->logWebhook($data);
            
            if (empty($data['content']) || empty($data['transferAmount']) || $data['transferType'] !== 'in') {
                return [
                    'success' => false,
                    'message' => 'Invalid transaction data'
                ];
            }
            
            $content = trim($data['content']);
            
            // EXTRACT phần quan trọng từ content dài
            $extractedContent = $this->extractTransactionContent($content);
            
            if (!$extractedContent) {
                return [
                    'success' => true,
                    'message' => 'Transaction received but not recognized',
                    'data' => [
                        'type' => 'other',
                        'content' => $content,
                        'amount' => $data['transferAmount']
                    ]
                ];
            }
            
            // CHUẨN HÓA: thay khoảng trắng bằng dấu gạch dưới
            $normalizedContent = preg_replace('/\s+/', '_', $extractedContent);
            
            // Phân loại giao dịch
            $activationPrefix = $this->config->get('activation_description_prefix', 'kichhoat');
            $luongPrefix = $this->config->get('recharge_luong_prefix', 'napluong');
            $xuPrefix = $this->config->get('recharge_xu_prefix', 'napxu');
            
            // Kiểm tra activation
            if (strpos($normalizedContent, $activationPrefix) === 0) {
                return $this->processActivationPayment(
                    $normalizedContent, 
                    $data['transferAmount'], 
                    $data['transactionDate'] ?? date('Y-m-d H:i:s'), 
                    $data['accountNumber'] ?? '', 
                    $data['id'] ?? 'SEPAY_' . time(), 
                    $data
                );
            }
            
            // Kiểm tra nạp lượng
            if (strpos($normalizedContent, $luongPrefix) === 0) {
                $data['content'] = $normalizedContent;
                return $this->processRechargeLuong($data);
            }
            
            // Kiểm tra nạp xu
            if (strpos($normalizedContent, $xuPrefix) === 0) {
                $data['content'] = $normalizedContent;
                return $this->processRechargeXu($data);
            }
            
            return [
                'success' => true,
                'message' => 'Transaction received but not recognized',
                'data' => [
                    'type' => 'other',
                    'extracted_content' => $extractedContent,
                    'amount' => $data['transferAmount']
                ]
            ];
            
        } catch (Exception $e) {
            error_log("Webhook error: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Error processing webhook: ' . $e->getMessage()
            ];
        }
    }

    private function extractTransactionContent($content) {
        $activationPrefix = $this->config->get('activation_description_prefix', 'kichhoat');
        $luongPrefix = $this->config->get('recharge_luong_prefix', 'napluong');
        $xuPrefix = $this->config->get('recharge_xu_prefix', 'napxu');
        
        $prefixes = [$activationPrefix, $luongPrefix, $xuPrefix];
        
        foreach ($prefixes as $prefix) {
            $pos = stripos($content, $prefix);
            
            if ($pos !== false) {
                $substring = substr($content, $pos);
                $endPos = strpos($substring, '.');
                
                if ($endPos !== false) {
                    $extracted = substr($substring, 0, $endPos);
                } else {
                    $extracted = substr($substring, 0, 50);
                }
                
                return trim($extracted);
            }
        }
        
        return null;
    }

    /**
     * Tìm order_id từ recharge_orders dựa trên thông tin thanh toán
     */
    private function findPendingOrder($playerName, $serverId, $amount, $type) {
        try {
            $stmt = $this->accountDb->prepare("
                SELECT order_id 
                FROM recharge_orders 
                WHERE player_name = ? 
                AND server_id = ?
                AND type = ?
                AND amount = ?
                AND status = 'pending'
                AND created_at >= DATE_SUB(NOW(), INTERVAL 30 MINUTE)
                ORDER BY created_at DESC 
                LIMIT 1
            ");
            $stmt->execute([$playerName, $serverId, $type, $amount]);
            $result = $stmt->fetch();
            
            return $result ? $result['order_id'] : null;
            
        } catch (Exception $e) {
            error_log("Error finding pending order: " . $e->getMessage());
            return null;
        }
    }

    /**
     * Xử lý nạp xu - ĐÃ SỬA: TRA userId TỪ tob_char
     */
    public function processRechargeXu($data) {
        try {
            $this->logWebhook($data, 'recharge_xu');
            
            if (empty($data['content']) || empty($data['transferAmount'])) {
                return [
                    'success' => false,
                    'message' => 'Invalid transaction data'
                ];
            }
            
            $content = $data['content'];
            $amount = $data['transferAmount'];
            $transactionDate = $data['transactionDate'] ?? date('Y-m-d H:i:s');
            $transactionId = $data['id'] ?? 'SEPAY_XU_' . time();
            $content = preg_replace('/\s+/', '_', $content);
            
            $parts = explode('_', $content);
            
            if (count($parts) < 3) {
                return [
                    'success' => false,
                    'message' => 'Invalid content format. Expected: napxu_playerName_serverId'
                ];
            }
            
            $playerName = $parts[1];
            $serverId = (int)$parts[2];
            
            // ✅ QUAN TRỌNG: TRA userId TỪ tob_char
            $userInfo = $this->getUserIdFromPlayerName($playerName, $serverId);
            
            if (!$userInfo['success']) {
                return [
                    'success' => false,
                    'message' => 'Nhân vật không tồn tại trong game. Player: ' . $playerName
                ];
            }
            
            $userId = $userInfo['user_id']; // Đây là userId THẬT từ tob_char
            
            // Kiểm tra duplicate transaction
            if ($this->isDuplicateTransaction($transactionId, 'recharge_xu')) {
                return [
                    'success' => false,
                    'message' => 'Transaction already processed'
                ];
            }
            
            // Tính xu dựa trên tỷ giá
            $xuAmount = $this->calculateXuAmount($amount);
            
            if ($xuAmount === 0) {
                return [
                    'success' => false,
                    'message' => 'Invalid recharge amount. Amount: ' . $amount . '. Valid amounts: ' . implode(', ', array_keys(ConfigService::getInstance()->get('xu_exchange_rates')))
                ];
            }
            
            // TÌM PENDING ORDER
            $orderId = $this->findPendingOrder($playerName, $serverId, $amount, 'recharge_xu');
            
            // Nếu không tìm thấy pending order, tạo order_id mới
            if (!$orderId) {
                $orderId = 'XU_' . date('YmdHis') . '_' . $playerName;
                
                $this->createRechargeOrderIfNotExists(
                    $orderId, 
                    $userId,  // userId THẬT
                    $playerName, 
                    $serverId, 
                    'recharge_xu', 
                    $amount, 
                    $xuAmount
                );
            }
            
            // Cập nhật order trong recharge_orders
            $this->updateRechargeOrder(
                $orderId,
                $transactionId,
                $data['gateway'] ?? 'Unknown',
                $transactionDate,
                'paid'
            );
            
            // Lưu transaction vào recharge_transactions
            $this->saveRechargeTransaction(
                $orderId, 
                $userId,  // userId THẬT
                $playerName, 
                $serverId, 
                $amount, 
                $xuAmount, 
                'recharge_xu', 
                $transactionId, 
                $data, 
                $transactionDate
            );
            
            // Thực hiện nạp xu vào game (SỬ DỤNG PLAYER NAME)
            $result = $this->addXuToGame($playerName, $serverId, $xuAmount);
            
            if (!$result['success']) {
                $this->updateTransactionStatus($orderId, 'failed');
                $this->updateRechargeOrderStatus($orderId, 'failed');
                return [
                    'success' => false,
                    'message' => 'Failed to add xu: ' . $result['message']
                ];
            }
            
            // Cập nhật trạng thái thành công
            $this->updateTransactionStatus($orderId, 'completed');
            $this->updateRechargeOrderStatus($orderId, 'completed');
            
            // GHI VÀO BẢNG TOPNAP với userId THẬT
            $this->updateTopNap($userId, $playerName, $serverId, $amount, $xuAmount, 0, 0, 1);
            
             $this->updateEventRecharge($userId, $playerName, $serverId, $amount, $xuAmount, 0, 0);
            // Log với userId THẬT
            $this->logRecharge($userId, $serverId, $orderId, $amount, $xuAmount, 0, 'completed');
            
            return [
                'success' => true,
                'message' => 'Xu recharged successfully',
                'data' => [
                    'order_id' => $orderId,
                    'user_id' => $userId,  // userId THẬT
                    'player_name' => $playerName,
                    'server_id' => $serverId,
                    'amount_paid' => $amount,
                    'xu_received' => $xuAmount,
                    'transaction_id' => $transactionId,
                    'completed_at' => date('Y-m-d H:i:s')
                ]
            ];
            
        } catch (Exception $e) {
            error_log("Error in processRechargeXu: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Error processing xu recharge: ' . $e->getMessage()
            ];
        }
    }

    /**
     * Tạo recharge order nếu chưa tồn tại
     */
    private function createRechargeOrderIfNotExists($orderId, $userId, $playerName, $serverId, $type, $amount, $expectedReward, $expectedLuongKhoa = 0) {
        try {
            $checkStmt = $this->accountDb->prepare("
                SELECT id FROM recharge_orders WHERE order_id = ? LIMIT 1
            ");
            $checkStmt->execute([$orderId]);
            
            if (!$checkStmt->fetch()) {
                $prefix = $type === 'recharge_xu' ? ConfigService::getInstance()->get('recharge_xu_prefix') : ConfigService::getInstance()->get('recharge_luong_prefix');
                $description = "{$prefix} {$playerName} {$serverId}";
                
                $qrUrl = "https://qr.sepay.vn/img?" . http_build_query([
                    'acc' => $this->config->get('vietqr_account'),
                    'bank' => $this->config->get('vietqr_bank'),
                    'amount' => $amount,
                    'des' => $description,
                    'template' => 'compact'
                ]);
                
                $stmt = $this->accountDb->prepare("
                    INSERT INTO recharge_orders 
                    (order_id, user_id, player_name, server_id, type, amount, expected_reward, 
                     expected_luong_khoa, qr_code_url, status, created_at) 
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, 'pending', NOW())
                ");
                
                $stmt->execute([
                    $orderId,
                    $userId,  // userId THẬT từ tob_char
                    $playerName,
                    $serverId,
                    $type,
                    $amount,
                    $expectedReward,
                    $expectedLuongKhoa,
                    $qrUrl
                ]);
            }
        } catch (Exception $e) {
            error_log("Error creating recharge order: " . $e->getMessage());
        }
    }

    /**
     * Cập nhật recharge order
     */
    private function updateRechargeOrder($orderId, $transactionId, $bankName, $paymentTime, $status = 'paid') {
        try {
            $stmt = $this->accountDb->prepare("
                UPDATE recharge_orders 
                SET status = ?,
                    transaction_id = ?,
                    bank_name = ?,
                    payment_time = ?,
                    updated_at = NOW()
                WHERE order_id = ?
            ");
            
            $stmt->execute([
                $status,
                $transactionId,
                $bankName,
                $paymentTime,
                $orderId
            ]);
            
            return $stmt->rowCount() > 0;
        } catch (Exception $e) {
            error_log("Error updating recharge order: " . $e->getMessage());
            return false;
        }
    }

    /**
     * Cập nhật trạng thái recharge order
     */
    private function updateRechargeOrderStatus($orderId, $status) {
        try {
            $stmt = $this->accountDb->prepare("
                UPDATE recharge_orders 
                SET status = ?,
                    updated_at = NOW()
                WHERE order_id = ?
            ");
            
            $stmt->execute([$status, $orderId]);
            
            return $stmt->rowCount() > 0;
        } catch (Exception $e) {
            error_log("Error updating recharge order status: " . $e->getMessage());
            return false;
        }
    }

    /**
     * Xử lý nạp lượng - ĐÃ SỬA: TRA userId TỪ tob_char
     */
    public function processRechargeLuong($data) {
        try {
            $this->logWebhook($data, 'recharge_luong');
            
            if (empty($data['content']) || empty($data['transferAmount'])) {
                return [
                    'success' => false,
                    'message' => 'Invalid transaction data'
                ];
            }
            
            $content = $data['content'];
            $amount = $data['transferAmount'];
            $transactionDate = $data['transactionDate'] ?? date('Y-m-d H:i:s');
            $transactionId = $data['id'] ?? 'SEPAY_LUONG_' . time();
            
            // CHUẨN HÓA CONTENT
            $content = preg_replace('/\s+/', '_', $content);
            
            $parts = explode('_', $content);
            
            if (count($parts) < 3) {
                $contentOriginal = $data['content'];
                $partsSpace = preg_split('/\s+/', $contentOriginal);
                
                if (count($partsSpace) >= 3) {
                    $parts = $partsSpace;
                } else {
                    return [
                        'success' => false,
                        'message' => 'Invalid content format. Expected: napluong_playerName_serverId'
                    ];
                }
            }
            
            if ($parts[0] !== ConfigService::getInstance()->get('recharge_luong_prefix')) {
                return [
                    'success' => false,
                    'message' => 'Invalid prefix. Expected: ' . ConfigService::getInstance()->get('recharge_luong_prefix')
                ];
            }
            
            $playerName = $parts[1];
            $serverId = (int)$parts[2];
            
            // ✅ QUAN TRỌNG: TRA userId TỪ tob_char
            $userInfo = $this->getUserIdFromPlayerName($playerName, $serverId);
            
            if (!$userInfo['success']) {
                return [
                    'success' => false,
                    'message' => 'Nhân vật không tồn tại trong game. Player: ' . $playerName
                ];
            }
            
            $userId = $userInfo['user_id']; // Đây là userId THẬT từ tob_char
            
            // Kiểm tra duplicate transaction
            if ($this->isDuplicateTransaction($transactionId, 'recharge_luong')) {
                return [
                    'success' => false,
                    'message' => 'Transaction already processed'
                ];
            }
            
            // TÍNH LƯỢNG VÀ LƯỢNG KHÓA
            $baseLuongAmount = floor($amount / ConfigService::getInstance()->get('luong_exchange_rate'));
            $baseLuongKhoaAmount = floor($baseLuongAmount * ConfigService::getInstance()->get('luong_khoa_percent'));
            
            $bonusMultiplier = ConfigService::getInstance()->get('luong_bonus_multiplier');
            $luongAmount = floor($baseLuongAmount * $bonusMultiplier);
            $luongKhoaAmount = floor($baseLuongKhoaAmount * $bonusMultiplier);
            
            if ($baseLuongAmount === 0) {
                return [
                    'success' => false,
                    'message' => 'Invalid recharge amount. Minimum: ' . ConfigService::getInstance()->get('luong_exchange_rate') . ' VND'
                ];
            }
            
            // TÌM PENDING ORDER
            $orderId = $this->findPendingOrder($playerName, $serverId, $amount, 'recharge_luong');
            
            if (!$orderId) {
                $orderId = 'LUONG_' . date('YmdHis') . '_' . $playerName;
                
                $this->createRechargeOrderIfNotExists(
                    $orderId, 
                    $userId,  // userId THẬT
                    $playerName, 
                    $serverId, 
                    'recharge_luong', 
                    $amount, 
                    $luongAmount,
                    $luongKhoaAmount
                );
            }
            
            // Cập nhật order
            $this->updateRechargeOrder(
                $orderId,
                $transactionId,
                $data['gateway'] ?? 'Unknown',
                $transactionDate,
                'paid'
            );
            
            // Lưu transaction
            $this->saveRechargeTransaction(
                $orderId, 
                $userId,  // userId THẬT
                $playerName, 
                $serverId, 
                $amount, 
                $luongAmount, 
                'recharge_luong', 
                $transactionId, 
                $data, 
                $transactionDate, 
                $luongKhoaAmount
            );
            
            // Thực hiện nạp lượng vào game
            $result = $this->addLuongToGame($playerName, $serverId, $luongAmount, $luongKhoaAmount);
            
            if (!$result['success']) {
                $this->updateTransactionStatus($orderId, 'failed');
                $this->updateRechargeOrderStatus($orderId, 'failed');
                return [
                    'success' => false,
                    'message' => 'Failed to add luong: ' . $result['message']
                ];
            }
            
            // Cập nhật trạng thái thành công
            $this->updateTransactionStatus($orderId, 'completed');
            $this->updateRechargeOrderStatus($orderId, 'completed');
            
            // GHI VÀO BẢNG TOPNAP với userId THẬT
            $this->updateTopNap($userId, $playerName, $serverId, $amount, 0, $luongAmount, $luongKhoaAmount, 1);
            
            $this->updateEventRecharge($userId, $playerName, $serverId, $amount, 0, $luongAmount, $luongKhoaAmount);
            // Log với userId THẬT
            $this->logRecharge($userId, $serverId, $orderId, $amount, 0, $luongAmount, 'completed', $luongKhoaAmount);
            
            return [
                'success' => true,
                'message' => 'Luong recharged successfully',
                'data' => [
                    'order_id' => $orderId,
                    'user_id' => $userId,  // userId THẬT
                    'player_name' => $playerName,
                    'server_id' => $serverId,
                    'amount_paid' => $amount,
                    'luong_received' => $luongAmount,
                    'luong_khoa_received' => $luongKhoaAmount,
                    'transaction_id' => $transactionId,
                    'bonus_multiplier' => $bonusMultiplier,
                    'completed_at' => date('Y-m-d H:i:s')
                ]
            ];
            
        } catch (Exception $e) {
            error_log("Error in processRechargeLuong: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Error processing luong recharge: ' . $e->getMessage()
            ];
        }
    }

    /**
     * Xử lý thanh toán kích hoạt
     */
    private function processActivationPayment($content, $amount, $transactionDate, $accountNumber, $transactionId, $webhookData) {
        try {
            $parts = explode('_', $content);
            
            if (count($parts) < 3) {
                return [
                    'success' => false,
                    'message' => 'Invalid content format'
                ];
            }
            
            $userId = (int)$parts[1];
            $serverId = (int)$parts[2];
            
            if ($amount != ConfigService::getInstance()->get('activation_amount')) {
                return [
                    'success' => false,
                    'message' => sprintf(
                        'Invalid amount. Expected: %s, Received: %s',
                        ConfigService::getInstance()->get('activation_amount'),
                        $amount
                    )
                ];
            }
            
            $checkStmt = $this->accountDb->prepare("
                SELECT * FROM activation_requests 
                WHERE transaction_id = ? 
                AND status IN ('paid', 'completed')
                LIMIT 1
            ");
            $checkStmt->execute([$transactionId]);
            
            if ($checkStmt->fetch()) {
                return [
                    'success' => false,
                    'message' => 'Transaction already processed'
                ];
            }
            
            $orderStmt = $this->accountDb->prepare("
                SELECT * FROM activation_requests 
                WHERE user_id = ? 
                AND server_id = ?
                AND status = 'pending'
                AND amount = ?
                ORDER BY created_at DESC 
                LIMIT 1
            ");
            $orderStmt->execute([$userId, $serverId, ConfigService::getInstance()->get('activation_amount')]);
            $order = $orderStmt->fetch();
            
            $orderId = $order ? $order['order_id'] : 'SEPAY_' . $transactionId;
            
            $userStmt = $this->accountDb->prepare("
                SELECT id, username 
                FROM team_user 
                WHERE id = ? LIMIT 1
            ");
            $userStmt->execute([$userId]);
            $user = $userStmt->fetch();
            
            if (!$user) {
                return [
                    'success' => false,
                    'message' => 'User not found'
                ];
            }
            
            if (!$order) {
                $orderId = 'AUTO_' . date('YmdHis') . '_' . $userId;
                
                $insertStmt = $this->accountDb->prepare("
                    INSERT INTO activation_requests 
                    (order_id, user_id, username, server_id, amount, status, transaction_id, bank_name, payment_time) 
                    VALUES (?, ?, ?, ?, ?, 'paid', ?, ?, ?)
                ");
                
                $insertStmt->execute([
                    $orderId,
                    $userId,
                    $user['username'],
                    $serverId,
                    ConfigService::getInstance()->get('activation_amount'),
                    $transactionId,
                    $webhookData['gateway'] ?? 'Unknown',
                    $transactionDate
                ]);
            } else {
                $updateStmt = $this->accountDb->prepare("
                    UPDATE activation_requests 
                    SET status = 'paid',
                        transaction_id = ?,
                        bank_name = ?,
                        payment_time = ?,
                        updated_at = NOW()
                    WHERE order_id = ? AND status = 'pending'
                ");
                
                $updateStmt->execute([
                    $transactionId,
                    $webhookData['gateway'] ?? 'Unknown',
                    $transactionDate,
                    $order['order_id']
                ]);
            }
            
            $activationResult = $this->activateAccount($userId, $serverId, $orderId);
            
            if (!$activationResult['success']) {
                $this->accountDb->prepare("
                    UPDATE activation_requests 
                    SET status = 'failed',
                        updated_at = NOW()
                    WHERE order_id = ?
                ")->execute([$orderId]);
                
                return [
                    'success' => false,
                    'message' => 'Activation failed: ' . $activationResult['message']
                ];
            }
            
            $this->accountDb->prepare("
                UPDATE activation_requests 
                SET status = 'completed',
                    updated_at = NOW()
                WHERE order_id = ?
            ")->execute([$orderId]);
            
            $rewards = $activationResult['rewards'] ?? [];
            $rewardXu = $rewards['xu'] ?? ConfigService::getInstance()->get('activation_reward_xu');
            $rewardLuong = $rewards['luong'] ?? ConfigService::getInstance()->get('activation_reward_luong');
            $rewardLuongKhoa = $rewards['luongK'] ?? ConfigService::getInstance()->get('activation_reward_luong_khoa');
            
            $this->updateTopNap($userId, $user['username'], $serverId, $amount, $rewardXu, $rewardLuong, $rewardLuongKhoa, 1);
            
            $this->updateEventRecharge($userId, $user['username'], $serverId, $amount, $rewardXu, $rewardLuong, $rewardLuongKhoa);
            
            $this->logActivation($userId, $serverId, $orderId, $amount, 'completed');
            
            return [
                'success' => true,
                'message' => 'Account activated successfully',
                'data' => [
                    'order_id' => $orderId,
                    'user_id' => $userId,
                    'server_id' => $serverId,
                    'amount' => $amount,
                    'status' => 'completed',
                    'rewards' => $activationResult['rewards'],
                    'transaction_id' => $transactionId,
                    'activated_at' => date('Y-m-d H:i:s')
                ]
            ];
            
        } catch (Exception $e) {
            error_log("Error in processActivationPayment: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Error processing activation: ' . $e->getMessage()
            ];
        }
    }

    /**
     * Cập nhật bảng topnap
     */
    private function updateTopNap($userId, $playerName, $serverId, $amount, $xuAmount, $luongAmount, $luongKhoaAmount, $rechargeCount = 1) {
        try {
            $checkStmt = $this->accountDb->prepare("
                SELECT id, total_amount, total_xu, total_luong, total_luong_khoa, total_recharge 
                FROM topnap 
                WHERE user_id = ? AND server_id = ?
                LIMIT 1
            ");
            $checkStmt->execute([$userId, $serverId]);
            $existingRecord = $checkStmt->fetch();
            
            if ($existingRecord) {
                $newTotalAmount = $existingRecord['total_amount'] + $amount;
                $newTotalXu = $existingRecord['total_xu'] + $xuAmount;
                $newTotalLuong = $existingRecord['total_luong'] + $luongAmount;
                $newTotalLuongKhoa = $existingRecord['total_luong_khoa'] + $luongKhoaAmount;
                $newTotalRecharge = $existingRecord['total_recharge'] + $rechargeCount;
                
                $updateStmt = $this->accountDb->prepare("
                    UPDATE topnap 
                    SET total_amount = ?,
                        total_xu = ?,
                        total_luong = ?,
                        total_luong_khoa = ?,
                        total_recharge = ?,
                        last_recharge_at = NOW(),
                        updated_at = NOW()
                    WHERE id = ?
                ");
                
                $updateStmt->execute([
                    $newTotalAmount,
                    $newTotalXu,
                    $newTotalLuong,
                    $newTotalLuongKhoa,
                    $newTotalRecharge,
                    $existingRecord['id']
                ]);
                
                return [
                    'success' => true,
                    'action' => 'updated',
                    'old_amount' => $existingRecord['total_amount'],
                    'new_amount' => $newTotalAmount
                ];
            } else {
                $insertStmt = $this->accountDb->prepare("
                    INSERT INTO topnap 
                    (user_id, username, server_id, total_amount, total_xu, total_luong, total_luong_khoa, total_recharge, last_recharge_at) 
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, NOW())
                ");
                
                $insertStmt->execute([
                    $userId,
                    $playerName,
                    $serverId,
                    $amount,
                    $xuAmount,
                    $luongAmount,
                    $luongKhoaAmount,
                    $rechargeCount
                ]);
                
                return [
                    'success' => true,
                    'action' => 'created',
                    'new_amount' => $amount
                ];
            }
            
        } catch (Exception $e) {
            error_log("Error updating topnap: " . $e->getMessage());
            return [
                'success' => false,
                'error' => $e->getMessage()
            ];
        }
    }
    private function updateEventRecharge($userId, $username, $serverId, $amount, $xuAmount, $luongAmount, $luongKhoaAmount) {
        try {
            // Lấy tất cả events đang active (recharge type)
            $stmt = $this->accountDb->prepare("
                SELECT id, start_time, end_time 
                FROM events 
                WHERE event_type = 'recharge'
                AND is_active = 1 
                AND is_finished = 0
                AND NOW() BETWEEN start_time AND end_time
            ");
            $stmt->execute();
            $activeEvents = $stmt->fetchAll(PDO::FETCH_ASSOC);
            
            // Cập nhật cho từng event đang active
            foreach ($activeEvents as $event) {
                $eventId = $event['id'];
                
                // Kiểm tra user đã có record chưa
                $checkStmt = $this->accountDb->prepare("
                    SELECT id, total_amount, total_xu, total_luong, total_luong_khoa, total_recharge 
                    FROM event_recharge 
                    WHERE event_id = ? AND user_id = ? AND server_id = ?
                    LIMIT 1
                ");
                $checkStmt->execute([$eventId, $userId, $serverId]);
                $existingRecord = $checkStmt->fetch(PDO::FETCH_ASSOC);
                
                if ($existingRecord) {
                    // Cập nhật record hiện có
                    $updateStmt = $this->accountDb->prepare("
                        UPDATE event_recharge 
                        SET total_amount = total_amount + ?,
                            total_xu = total_xu + ?,
                            total_luong = total_luong + ?,
                            total_luong_khoa = total_luong_khoa + ?,
                            total_recharge = total_recharge + 1,
                            last_recharge_at = NOW(),
                            updated_at = NOW()
                        WHERE id = ?
                    ");
                    $updateStmt->execute([
                        $amount,
                        $xuAmount,
                        $luongAmount,
                        $luongKhoaAmount,
                        $existingRecord['id']
                    ]);
                } else {
                    // Tạo record mới
                    $insertStmt = $this->accountDb->prepare("
                        INSERT INTO event_recharge 
                        (event_id, user_id, username, server_id, total_amount, total_xu, total_luong, total_luong_khoa, total_recharge, first_recharge_at, last_recharge_at)
                        VALUES (?, ?, ?, ?, ?, ?, ?, ?, 1, NOW(), NOW())
                    ");
                    $insertStmt->execute([
                        $eventId,
                        $userId,
                        $username,
                        $serverId,
                        $amount,
                        $xuAmount,
                        $luongAmount,
                        $luongKhoaAmount
                    ]);
                }
            }
            
            return ['success' => true, 'events_updated' => count($activeEvents)];
            
        } catch (Exception $e) {
            error_log("Error updating event_recharge: " . $e->getMessage());
            return ['success' => false, 'error' => $e->getMessage()];
        }
    }
    /**
     * Kích hoạt tài khoản
     */
    private function activateAccount($userId, $serverId = 1, $orderId = null) {
        try {
            $gameDb = $this->getGameDb($serverId);
            
            $checkStmt = $gameDb->prepare("SELECT * FROM `5h_active` WHERE `userID` = ? LIMIT 1");
            $checkStmt->execute([$userId]);
            
            if ($checkStmt->fetch()) {
                return [
                    'success' => false,
                    'message' => 'Tài khoản đã được kích hoạt'
                ];
            }
            
            $userStmt = $this->accountDb->prepare("
                SELECT id, username 
                FROM team_user 
                WHERE id = ? LIMIT 1
            ");
            $userStmt->execute([$userId]);
            $user = $userStmt->fetch();
            
            if (!$user) {
                return [
                    'success' => false,
                    'message' => 'Tài khoản không tồn tại'
                ];
            }
            
            $activeStmt = $gameDb->prepare("
                INSERT INTO `5h_active` (`userID`, `username`, `time_end`) 
                VALUES (?, ?, ?)
            ");
            $activeStmt->execute([
                $userId, 
                $user['username'], 
                -1
            ]);
            
            $money = ConfigService::getInstance()->get('activation_reward_luong');
            $rewardStmt = $gameDb->prepare("
                INSERT INTO `board_created` 
                (`xu`, `luong`, `luongK`, `svID`, `username`, `ve`) 
                VALUES (?, ?, ?, ?, ?, '0')
            ");
            $rewardStmt->execute([
                ConfigService::getInstance()->get('activation_reward_xu'),
                $money,
                $money,
                -1,
                $userId
            ]);
            
            return [
                'success' => true,
                'rewards' => [
                    'xu' => ConfigService::getInstance()->get('activation_reward_xu'),
                    'luong' => $money,
                    'luongK' => $money,
                    'message' => 'Kích hoạt thành công'
                ]
            ];
            
        } catch (Exception $e) {
            error_log("Activation error: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi kích hoạt: ' . $e->getMessage()
            ];
        }
    }

    /**
     * Tính xu dựa trên mệnh giá
     */
    private function calculateXuAmount($amount) {
        $rates = $this->config->get('xu_exchange_rates', []);
        $xuAmount = $rates[$amount] ?? 0;
        $bonusMultiplier = $this->config->get('xu_bonus_multiplier', 1);
        return $xuAmount * $bonusMultiplier;
    }

    /**
     * Kiểm tra transaction đã xử lý chưa
     */
    private function isDuplicateTransaction($transactionId, $type) {
        $stmt = $this->accountDb->prepare("
            SELECT id FROM recharge_transactions 
            WHERE transaction_id = ? 
            AND type = ?
            AND status IN ('completed', 'processing')
            LIMIT 1
        ");
        $stmt->execute([$transactionId, $type]);
        return $stmt->fetch() !== false;
    }

    /**
     * Lưu transaction vào database
     */
    private function saveRechargeTransaction($orderId, $userId, $playerName, $serverId, $amount, $rewardAmount, $type, $transactionId, $webhookData, $transactionDate, $luongKhoa = 0) {
        $stmt = $this->accountDb->prepare("
            INSERT INTO recharge_transactions 
            (order_id, user_id, player_name, server_id, amount, reward_amount, luong_khoa, type, status, transaction_id, bank_name, payment_time, webhook_data) 
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, 'processing', ?, ?, ?, ?)
        ");
        
        $stmt->execute([
            $orderId,
            $userId,
            $playerName,
            $serverId,
            $amount,
            $rewardAmount,
            $luongKhoa,
            $type,
            $transactionId,
            $webhookData['gateway'] ?? 'Unknown',
            $transactionDate,
            json_encode($webhookData)
        ]);
    }

    /**
     * Cập nhật trạng thái transaction
     */
    private function updateTransactionStatus($orderId, $status) {
        $stmt = $this->accountDb->prepare("
            UPDATE recharge_transactions 
            SET status = ?, updated_at = NOW()
            WHERE order_id = ?
        ");
        $stmt->execute([$status, $orderId]);
    }

    /**
     * Nạp xu vào game (SỬ DỤNG PLAYER NAME)
     */
    private function addXuToGame($playerName, $serverId, $xuAmount) {
        try {
            $gameDb = $this->getGameDb($serverId);
            
            $stmt = $gameDb->prepare("
                INSERT INTO board_naptien (xu, luong, luongKhoa, username) 
                VALUES (?, 0, 0, ?)
            ");
            
            $stmt->execute([$xuAmount, $playerName]);
            
            return ['success' => true];
        } catch (Exception $e) {
            error_log("Add xu error: " . $e->getMessage());
            return [
                'success' => false,
                'message' => $e->getMessage()
            ];
        }
    }

    /**
     * Nạp lượng vào game (SỬ DỤNG PLAYER NAME)
     */
    private function addLuongToGame($playerName, $serverId, $luongAmount, $luongKhoaAmount) {
        try {
            $gameDb = $this->getGameDb($serverId);
            
            $stmt = $gameDb->prepare("
                INSERT INTO board_naptien (xu, luong, luongKhoa, username) 
                VALUES (0, ?, ?, ?)
            ");
            
            $stmt->execute([$luongAmount, $luongKhoaAmount, $playerName]);
            
            return ['success' => true];
        } catch (Exception $e) {
            error_log("Add luong error: " . $e->getMessage());
            return [
                'success' => false,
                'message' => $e->getMessage()
            ];
        }
    }

    /**
     * Log webhook nhận được
     */
    private function logWebhook($data, $type = 'sepay') {
        try {
            $stmt = $this->accountDb->prepare("
                INSERT INTO webhook_logs 
                (type, data, created_at) 
                VALUES (?, ?, NOW())
            ");
            
            $stmt->execute([$type, json_encode($data)]);
        } catch (Exception $e) {
            error_log("Failed to log webhook: " . $e->getMessage());
        }
    }

    /**
     * Log kích hoạt
     */
    private function logActivation($userId, $serverId, $orderId, $amount, $status) {
        try {
            $stmt = $this->accountDb->prepare("
                INSERT INTO activation_logs 
                (user_id, order_id, server_id, amount, status, created_at) 
                VALUES (?, ?, ?, ?, ?, NOW())
            ");
            
            $stmt->execute([$userId, $orderId, $serverId, $amount, $status]);
        } catch (Exception $e) {
            error_log("Failed to log activation: " . $e->getMessage());
        }
    }

    /**
     * Log recharge
     */
    private function logRecharge($userId, $serverId, $orderId, $amount, $xu, $luong, $status, $luongKhoa = 0) {
        try {
            $stmt = $this->accountDb->prepare("
                INSERT INTO recharge_logs 
                (user_id, order_id, server_id, amount, xu, luong, luong_khoa, status, created_at) 
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, NOW())
            ");
            
            $stmt->execute([$userId, $orderId, $serverId, $amount, $xu, $luong, $luongKhoa, $status]);
        } catch (Exception $e) {
            error_log("Failed to log recharge: " . $e->getMessage());
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

    /**
     * Khởi tạo bảng log
     */
    public function initLogTables() {
        try {
            $sql = "
                CREATE TABLE IF NOT EXISTS `webhook_logs` (
                    `id` INT AUTO_INCREMENT PRIMARY KEY,
                    `type` VARCHAR(50) NOT NULL,
                    `data` TEXT,
                    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    INDEX idx_type (type),
                    INDEX idx_created_at (created_at)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
                
                CREATE TABLE IF NOT EXISTS `activation_logs` (
                    `id` INT AUTO_INCREMENT PRIMARY KEY,
                    `user_id` INT NOT NULL,
                    `order_id` VARCHAR(50),
                    `server_id` INT DEFAULT 1,
                    `amount` DECIMAL(10,2),
                    `status` VARCHAR(50),
                    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    INDEX idx_user_id (user_id),
                    INDEX idx_order_id (order_id),
                    INDEX idx_status (status)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
                
                CREATE TABLE IF NOT EXISTS `recharge_transactions` (
                    `id` INT AUTO_INCREMENT PRIMARY KEY,
                    `order_id` VARCHAR(50) UNIQUE NOT NULL,
                    `user_id` INT NOT NULL,
                    `player_name` VARCHAR(100),
                    `server_id` INT DEFAULT 1,
                    `amount` DECIMAL(10,2) NOT NULL,
                    `reward_amount` BIGINT DEFAULT 0,
                    `luong_khoa` BIGINT DEFAULT 0,
                    `type` ENUM('recharge_xu', 'recharge_luong') NOT NULL,
                    `status` ENUM('pending', 'processing', 'completed', 'failed') DEFAULT 'pending',
                    `transaction_id` VARCHAR(100),
                    `bank_name` VARCHAR(100),
                    `payment_time` DATETIME,
                    `webhook_data` TEXT,
                    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    INDEX idx_user_id (user_id),
                    INDEX idx_player_name (player_name),
                    INDEX idx_order_id (order_id),
                    INDEX idx_transaction_id (transaction_id),
                    INDEX idx_type (type),
                    INDEX idx_status (status)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
                
                CREATE TABLE IF NOT EXISTS `recharge_logs` (
                    `id` INT AUTO_INCREMENT PRIMARY KEY,
                    `user_id` INT NOT NULL,
                    `order_id` VARCHAR(50),
                    `server_id` INT DEFAULT 1,
                    `amount` DECIMAL(10,2),
                    `xu` BIGINT DEFAULT 0,
                    `luong` BIGINT DEFAULT 0,
                    `luong_khoa` BIGINT DEFAULT 0,
                    `status` VARCHAR(50),
                    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    INDEX idx_user_id (user_id),
                    INDEX idx_order_id (order_id),
                    INDEX idx_status (status)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            ";
            
            $this->accountDb->exec($sql);
            
        } catch (Exception $e) {
            error_log("Init log tables error: " . $e->getMessage());
        }
    }
}