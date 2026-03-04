<?php
// services/TransactionService.php

class TransactionService {
    private $accountDb;
    
    public function __construct() {
        $this->accountDb = Database::getInstance()->getConnection();
    }
    
    /**
     * Lấy trạng thái giao dịch theo order_id
     * Tự động phát hiện loại giao dịch (activation, xu, luong)
     */
    public function getTransactionStatus($orderId) {
        try {
            // Kiểm tra trong bảng activation_requests
            $activationStmt = $this->accountDb->prepare("
                SELECT 
                    order_id,
                    user_id,
                    username,
                    server_id,
                    amount,
                    status,
                    transaction_id,
                    bank_name,
                    payment_time,
                    created_at,
                    updated_at,
                    'activation' as type
                FROM activation_requests 
                WHERE order_id = ? 
                LIMIT 1
            ");
            $activationStmt->execute([$orderId]);
            $activation = $activationStmt->fetch();
            
            if ($activation) {
                return [
                    'success' => true,
                    'data' => $this->formatActivationStatus($activation)
                ];
            }
            
            // Kiểm tra trong bảng recharge_transactions
            $rechargeStmt = $this->accountDb->prepare("
                SELECT 
                    order_id,
                    user_id,
                    player_name,
                    server_id,
                    amount,
                    reward_amount,
                    luong_khoa,
                    type,
                    status,
                    transaction_id,
                    bank_name,
                    payment_time,
                    created_at,
                    updated_at
                FROM recharge_transactions 
                WHERE order_id = ? 
                LIMIT 1
            ");
            $rechargeStmt->execute([$orderId]);
            $recharge = $rechargeStmt->fetch();
            
            if ($recharge) {
                return [
                    'success' => true,
                    'data' => $this->formatRechargeStatus($recharge)
                ];
            }
            
            // Kiểm tra trong bảng recharge_orders
            $orderStmt = $this->accountDb->prepare("
                SELECT 
                    order_id,
                    user_id,
                    player_name,
                    server_id,
                    amount,
                    expected_reward,
                    expected_luong_khoa,
                    type,
                    status,
                    transaction_id,
                    bank_name,
                    payment_time,
                    created_at,
                    updated_at
                FROM recharge_orders 
                WHERE order_id = ? 
                LIMIT 1
            ");
            $orderStmt->execute([$orderId]);
            $order = $orderStmt->fetch();
            
            if ($order) {
                return [
                    'success' => true,
                    'data' => $this->formatOrderStatus($order)
                ];
            }
            
            return [
                'success' => false,
                'message' => 'Không tìm thấy giao dịch với order_id: ' . $orderId
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy trạng thái: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Lấy lịch sử giao dịch của user
     */
    public function getTransactionHistory($userId, $type = 'all', $page = 1, $limit = 10) {
        try {
            $offset = ($page - 1) * $limit;
            $transactions = [];
            $total = 0;
            
            if ($type === 'all' || $type === 'activation') {
                // Lấy activation transactions
                $activationStmt = $this->accountDb->prepare("
                    SELECT 
                        order_id,
                        user_id,
                        username,
                        server_id,
                        amount,
                        status,
                        transaction_id,
                        bank_name,
                        payment_time,
                        created_at,
                        updated_at,
                        'activation' as type
                    FROM activation_requests 
                    WHERE user_id = ?
                    ORDER BY created_at DESC
                ");
                $activationStmt->execute([$userId]);
                $activations = $activationStmt->fetchAll();
                
                foreach ($activations as $activation) {
                    $transactions[] = $this->formatActivationStatus($activation);
                }
            }
            
            if ($type === 'all' || $type === 'recharge_xu' || $type === 'recharge_luong') {
                // Lấy recharge transactions
                $rechargeQuery = "
                    SELECT 
                        order_id,
                        user_id,
                        player_name,
                        server_id,
                        amount,
                        reward_amount,
                        luong_khoa,
                        type,
                        status,
                        transaction_id,
                        bank_name,
                        payment_time,
                        created_at,
                        updated_at
                    FROM recharge_transactions 
                    WHERE user_id = ?
                ";
                
                if ($type !== 'all') {
                    $rechargeQuery .= " AND type = ?";
                }
                
                $rechargeQuery .= " ORDER BY created_at DESC";
                
                $rechargeStmt = $this->accountDb->prepare($rechargeQuery);
                
                if ($type !== 'all') {
                    $rechargeStmt->execute([$userId, $type]);
                } else {
                    $rechargeStmt->execute([$userId]);
                }
                
                $recharges = $rechargeStmt->fetchAll();
                
                foreach ($recharges as $recharge) {
                    $transactions[] = $this->formatRechargeStatus($recharge);
                }
            }
            
            // Sắp xếp theo thời gian tạo
            usort($transactions, function($a, $b) {
                return strtotime($b['created_at']) - strtotime($a['created_at']);
            });
            
            $total = count($transactions);
            $transactions = array_slice($transactions, $offset, $limit);
            
            return [
                'success' => true,
                'data' => [
                    'transactions' => $transactions,
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
     * Lấy danh sách giao dịch đang chờ
     */
    public function getPendingTransactions($userId) {
        try {
            $pending = [];
            
            // Pending activations
            $activationStmt = $this->accountDb->prepare("
                SELECT 
                    order_id,
                    user_id,
                    username,
                    server_id,
                    amount,
                    status,
                    qr_code_url,
                    created_at,
                    updated_at,
                    'activation' as type
                FROM activation_requests 
                WHERE user_id = ? 
                AND status = 'pending'
                AND created_at >= DATE_SUB(NOW(), INTERVAL 30 MINUTE)
                ORDER BY created_at DESC
            ");
            $activationStmt->execute([$userId]);
            $activations = $activationStmt->fetchAll();
            
            foreach ($activations as $activation) {
                $pending[] = [
                    'order_id' => $activation['order_id'],
                    'type' => 'activation',
                    'type_label' => 'Kích hoạt tài khoản',
                    'amount' => (float)$activation['amount'],
                    'status' => $activation['status'],
                    'status_label' => $this->getStatusLabel($activation['status']),
                    'qr_code_url' => $activation['qr_code_url'],
                    'created_at' => $activation['created_at'],
                    'expires_at' => date('Y-m-d H:i:s', strtotime($activation['created_at'] . ' + 30 minutes')),
                    'time_remaining' => $this->getTimeRemaining($activation['created_at'], 30)
                ];
            }
            
            // Pending recharges
            $rechargeStmt = $this->accountDb->prepare("
                SELECT 
                    order_id,
                    user_id,
                    player_name,
                    server_id,
                    amount,
                    reward_amount,
                    type,
                    status,
                    created_at,
                    updated_at
                FROM recharge_transactions 
                WHERE user_id = ? 
                AND status IN ('pending', 'processing')
                AND created_at >= DATE_SUB(NOW(), INTERVAL 30 MINUTE)
                ORDER BY created_at DESC
            ");
            $rechargeStmt->execute([$userId]);
            $recharges = $rechargeStmt->fetchAll();
            
            foreach ($recharges as $recharge) {
                $pending[] = [
                    'order_id' => $recharge['order_id'],
                    'type' => $recharge['type'],
                    'type_label' => $recharge['type'] === 'recharge_xu' ? 'Nạp Xu' : 'Nạp Lượng',
                    'amount' => (float)$recharge['amount'],
                    'reward_amount' => (int)$recharge['reward_amount'],
                    'status' => $recharge['status'],
                    'status_label' => $this->getStatusLabel($recharge['status']),
                    'created_at' => $recharge['created_at'],
                    'expires_at' => date('Y-m-d H:i:s', strtotime($recharge['created_at'] . ' + 30 minutes')),
                    'time_remaining' => $this->getTimeRemaining($recharge['created_at'], 30)
                ];
            }
            
            // Pending orders (recharge_orders)
            $orderStmt = $this->accountDb->prepare("
                SELECT 
                    order_id,
                    user_id,
                    player_name,
                    server_id,
                    amount,
                    expected_reward,
                    expected_luong_khoa,
                    type,
                    status,
                    qr_code_url,
                    created_at,
                    updated_at
                FROM recharge_orders 
                WHERE user_id = ? 
                AND status = 'pending'
                AND created_at >= DATE_SUB(NOW(), INTERVAL 30 MINUTE)
                ORDER BY created_at DESC
            ");
            $orderStmt->execute([$userId]);
            $orders = $orderStmt->fetchAll();
            
            foreach ($orders as $order) {
                $data = [
                    'order_id' => $order['order_id'],
                    'type' => $order['type'],
                    'type_label' => $order['type'] === 'recharge_xu' ? 'Nạp Xu' : 'Nạp Lượng',
                    'amount' => (float)$order['amount'],
                    'expected_reward' => (int)$order['expected_reward'],
                    'status' => $order['status'],
                    'status_label' => $this->getStatusLabel($order['status']),
                    'qr_code_url' => $order['qr_code_url'],
                    'created_at' => $order['created_at'],
                    'expires_at' => date('Y-m-d H:i:s', strtotime($order['created_at'] . ' + 30 minutes')),
                    'time_remaining' => $this->getTimeRemaining($order['created_at'], 30)
                ];
                
                // Thêm expected_luong_khoa cho nạp lượng
                if ($order['type'] === 'recharge_luong') {
                    $data['expected_luong_khoa'] = (int)$order['expected_luong_khoa'];
                }
                
                $pending[] = $data;
            }
            
            return [
                'success' => true,
                'data' => [
                    'pending_count' => count($pending),
                    'transactions' => $pending
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy pending transactions: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Thống kê giao dịch của user
     */
    public function getUserStatistics($userId) {
        try {
            // Activation statistics
            $activationStmt = $this->accountDb->prepare("
                SELECT 
                    COUNT(*) as total_activations,
                    SUM(CASE WHEN status = 'completed' THEN 1 ELSE 0 END) as completed_activations,
                    SUM(CASE WHEN status = 'completed' THEN amount ELSE 0 END) as total_activation_amount
                FROM activation_requests 
                WHERE user_id = ?
            ");
            $activationStmt->execute([$userId]);
            $activationStats = $activationStmt->fetch();
            
            // Recharge statistics
            $rechargeStmt = $this->accountDb->prepare("
                SELECT 
                    type,
                    COUNT(*) as total_count,
                    SUM(CASE WHEN status = 'completed' THEN 1 ELSE 0 END) as completed_count,
                    SUM(CASE WHEN status = 'completed' THEN amount ELSE 0 END) as total_amount,
                    SUM(CASE WHEN status = 'completed' THEN reward_amount ELSE 0 END) as total_reward
                FROM recharge_transactions 
                WHERE user_id = ?
                GROUP BY type
            ");
            $rechargeStmt->execute([$userId]);
            $rechargeStats = $rechargeStmt->fetchAll();
            
            $xuStats = null;
            $luongStats = null;
            
            foreach ($rechargeStats as $stat) {
                if ($stat['type'] === 'recharge_xu') {
                    $xuStats = [
                        'total_transactions' => (int)$stat['total_count'],
                        'completed_transactions' => (int)$stat['completed_count'],
                        'total_amount_paid' => (float)$stat['total_amount'],
                        'total_xu_received' => (int)$stat['total_reward']
                    ];
                } else if ($stat['type'] === 'recharge_luong') {
                    $luongStats = [
                        'total_transactions' => (int)$stat['total_count'],
                        'completed_transactions' => (int)$stat['completed_count'],
                        'total_amount_paid' => (float)$stat['total_amount'],
                        'total_luong_received' => (int)$stat['total_reward']
                    ];
                }
            }
            
            // Tổng hợp
            $totalAmount = (float)$activationStats['total_activation_amount'];
            $totalAmount += $xuStats ? (float)$xuStats['total_amount_paid'] : 0;
            $totalAmount += $luongStats ? (float)$luongStats['total_amount_paid'] : 0;
            
            return [
                'success' => true,
                'data' => [
                    'activation' => [
                        'total_requests' => (int)$activationStats['total_activations'],
                        'completed' => (int)$activationStats['completed_activations'],
                        'total_amount' => (float)$activationStats['total_activation_amount']
                    ],
                    'recharge_xu' => $xuStats ?? [
                        'total_transactions' => 0,
                        'completed_transactions' => 0,
                        'total_amount_paid' => 0,
                        'total_xu_received' => 0
                    ],
                    'recharge_luong' => $luongStats ?? [
                        'total_transactions' => 0,
                        'completed_transactions' => 0,
                        'total_amount_paid' => 0,
                        'total_luong_received' => 0
                    ],
                    'summary' => [
                        'total_amount_paid' => $totalAmount,
                        'total_transactions' => (int)$activationStats['total_activations'] + 
                                              ($xuStats ? (int)$xuStats['total_transactions'] : 0) + 
                                              ($luongStats ? (int)$luongStats['total_transactions'] : 0)
                    ]
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy thống kê: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Format activation status
     */
    private function formatActivationStatus($activation) {
        return [
            'order_id' => $activation['order_id'],
            'type' => 'activation',
            'type_label' => 'Kích hoạt tài khoản',
            'user_id' => (int)$activation['user_id'],
            'username' => $activation['username'],
            'server_id' => (int)$activation['server_id'],
            'amount' => (float)$activation['amount'],
            'status' => $activation['status'],
            'status_label' => $this->getStatusLabel($activation['status']),
            'transaction_id' => $activation['transaction_id'],
            'bank_name' => $activation['bank_name'],
            'payment_time' => $activation['payment_time'],
            'created_at' => $activation['created_at'],
            'updated_at' => $activation['updated_at']
        ];
    }
    
    /**
     * Format recharge status
     * LƯU Ý: reward_amount và luong_khoa đã bao gồm bonus multiplier
     */
    private function formatRechargeStatus($recharge) {
        $data = [
            'order_id' => $recharge['order_id'],
            'type' => $recharge['type'],
            'type_label' => $recharge['type'] === 'recharge_xu' ? 'Nạp Xu' : 'Nạp Lượng',
            'user_id' => (int)$recharge['user_id'],
            'player_name' => $recharge['player_name'],
            'server_id' => (int)$recharge['server_id'],
            'amount' => (float)$recharge['amount'],
            'status' => $recharge['status'],
            'status_label' => $this->getStatusLabel($recharge['status']),
            'transaction_id' => $recharge['transaction_id'],
            'bank_name' => $recharge['bank_name'],
            'payment_time' => $recharge['payment_time'],
            'created_at' => $recharge['created_at'],
            'updated_at' => $recharge['updated_at']
        ];
        
        if ($recharge['type'] === 'recharge_xu') {
            $data['xu_received'] = (int)$recharge['reward_amount'];
        } else {
            // Cả lượng và lượng khóa đều đã được nhân với bonus_multiplier
            $data['luong_received'] = (int)$recharge['reward_amount'];
            $data['luong_khoa_received'] = (int)$recharge['luong_khoa'];
        }
        
        return $data;
    }
    
    /**
     * Format order status
     * LƯU Ý: expected_reward và expected_luong_khoa đã bao gồm bonus multiplier
     */
    private function formatOrderStatus($order) {
        $data = [
            'order_id' => $order['order_id'],
            'type' => $order['type'],
            'type_label' => $order['type'] === 'recharge_xu' ? 'Nạp Xu' : 'Nạp Lượng',
            'user_id' => (int)$order['user_id'],
            'player_name' => $order['player_name'],
            'server_id' => (int)$order['server_id'],
            'amount' => (float)$order['amount'],
            'status' => $order['status'],
            'status_label' => $this->getStatusLabel($order['status']),
            'transaction_id' => $order['transaction_id'],
            'bank_name' => $order['bank_name'],
            'payment_time' => $order['payment_time'],
            'created_at' => $order['created_at'],
            'updated_at' => $order['updated_at']
        ];
        
        if ($order['type'] === 'recharge_xu') {
            $data['expected_xu'] = (int)$order['expected_reward'];
        } else {
            // Cả lượng và lượng khóa đều đã được nhân với bonus_multiplier
            $data['expected_luong'] = (int)$order['expected_reward'];
            $data['expected_luong_khoa'] = (int)$order['expected_luong_khoa'];
        }
        
        return $data;
    }
    
    /**
     * Lấy label trạng thái
     */
    private function getStatusLabel($status) {
        $labels = [
            'pending' => 'Đang chờ thanh toán',
            'processing' => 'Đang xử lý',
            'paid' => 'Đã thanh toán',
            'completed' => 'Hoàn thành',
            'failed' => 'Thất bại',
            'cancelled' => 'Đã hủy'
        ];
        
        return $labels[$status] ?? $status;
    }
    
    /**
     * Tính thời gian còn lại (phút)
     */
    private function getTimeRemaining($createdAt, $expiryMinutes) {
        $created = strtotime($createdAt);
        $expiry = $created + ($expiryMinutes * 60);
        $now = time();
        $remaining = $expiry - $now;
        
        if ($remaining <= 0) {
            return 0;
        }
        
        return ceil($remaining / 60); // Return minutes
    }
}