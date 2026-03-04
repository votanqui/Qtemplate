<?php
// services/AdminRechargeService.php

class AdminRechargeService {
    private $db;
    
    public function __construct() {
        $this->db = Database::getInstance()->getConnection();
    }
    
    /**
     * Lấy danh sách giao dịch nạp tiền
     */
    public function getTransactions($page, $limit, $status, $type, $userId, $search, $dateFrom, $dateTo) {
        $offset = ($page - 1) * $limit;
        
        $where = "1=1";
        $params = [];
        
        // Status filter
        if ($status !== 'all') {
            $where .= " AND status = ?";
            $params[] = $status;
        }
        
        // Type filter
        if ($type !== 'all') {
            $where .= " AND type = ?";
            $params[] = $type;
        }
        
        // User filter
        if ($userId) {
            $where .= " AND user_id = ?";
            $params[] = $userId;
        }
        
        // Search filter
        if (!empty($search)) {
            $where .= " AND (order_id LIKE ? OR username LIKE ? OR transaction_id LIKE ?)";
            $searchTerm = "%$search%";
            $params[] = $searchTerm;
            $params[] = $searchTerm;
            $params[] = $searchTerm;
        }
        
        // Date range filter
        if ($dateFrom) {
            $where .= " AND created_at >= ?";
            $params[] = $dateFrom;
        }
        if ($dateTo) {
            $where .= " AND created_at <= ?";
            $params[] = $dateTo . ' 23:59:59';
        }
        
        // Get total count
        $countSql = "SELECT COUNT(*) as total FROM recharge_transactions WHERE $where";
        $stmt = $this->db->prepare($countSql);
        $stmt->execute($params);
        $total = $stmt->fetch()['total'];
        
        // Get transactions
        $sql = "SELECT * FROM recharge_transactions 
                WHERE $where 
                ORDER BY created_at DESC 
                LIMIT $limit OFFSET $offset";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute($params);
        $transactions = $stmt->fetchAll();
        
        return [
            'transactions' => $transactions,
            'pagination' => [
                'total' => (int)$total,
                'page' => $page,
                'limit' => $limit,
                'total_pages' => ceil($total / $limit)
            ]
        ];
    }
    
    /**
     * Lấy chi tiết giao dịch
     */
    public function getTransactionDetail($transactionId) {
        $sql = "SELECT rt.*, tu.username, tu.phone, tu.email
                FROM recharge_transactions rt
                LEFT JOIN team_user tu ON rt.user_id = tu.id
                WHERE rt.id = ?";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$transactionId]);
        $transaction = $stmt->fetch();
        
        if (!$transaction) {
            return null;
        }
        
        // Get related logs
        $stmt = $this->db->prepare("
            SELECT * FROM recharge_logs 
            WHERE order_id = ? 
            ORDER BY created_at DESC
        ");
        $stmt->execute([$transaction['order_id']]);
        $transaction['logs'] = $stmt->fetchAll();
        
        return $transaction;
    }
    
    /**
     * Cập nhật trạng thái giao dịch
     */
    public function updateTransactionStatus($transactionId, $status, $note) {
        $allowedStatuses = ['pending', 'processing', 'completed', 'failed'];
        
        if (!in_array($status, $allowedStatuses)) {
            return ['success' => false, 'message' => 'Trạng thái không hợp lệ'];
        }
        
        try {
            $sql = "UPDATE recharge_transactions SET status = ?, updated_at = NOW() WHERE id = ?";
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$status, $transactionId]);
            
            // Log the change
            $this->logAdminAction($transactionId, 'status_update', $note);
            
            return [
                'success' => true,
                'transaction' => $this->getTransactionDetail($transactionId)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Cập nhật thất bại'];
        }
    }
    
    /**
     * Xử lý lại giao dịch thất bại
     */
    public function retryTransaction($transactionId) {
        // Get transaction
        $transaction = $this->getTransactionDetail($transactionId);
        
        if (!$transaction) {
            return ['success' => false, 'message' => 'Không tìm thấy giao dịch'];
        }
        
        if ($transaction['status'] !== 'failed') {
            return ['success' => false, 'message' => 'Chỉ có thể xử lý lại giao dịch thất bại'];
        }
        
        try {
            // Update status to processing
            $this->db->prepare("UPDATE recharge_transactions SET status = 'processing' WHERE id = ?")
                     ->execute([$transactionId]);
            
            // Try to process again (integrate with your payment processing logic)
            // This is a placeholder - implement your actual processing logic
            
            $this->logAdminAction($transactionId, 'retry', 'Admin retry transaction');
            
            return [
                'success' => true,
                'transaction' => $this->getTransactionDetail($transactionId)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Xử lý lại thất bại'];
        }
    }
    
    /**
     * Lấy top nạp
     */
    public function getTopRecharge($serverId, $limit, $period) {
        $where = "1=1";
        $params = [];
        
        if ($serverId) {
            $where .= " AND server_id = ?";
            $params[] = $serverId;
        }
        
        // Period filter
        if ($period !== 'all') {
            $dateFilter = $this->getDateFilter($period);
            $where .= " AND last_recharge_at >= ?";
            $params[] = $dateFilter;
        }
        
        $sql = "SELECT * FROM topnap 
                WHERE $where 
                ORDER BY total_amount DESC 
                LIMIT $limit";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute($params);
        $topList = $stmt->fetchAll();
        
        return [
            'top_recharge' => $topList,
            'period' => $period,
            'server_id' => $serverId
        ];
    }
    
    /**
     * Lấy thống kê nạp tiền
     */
    public function getRechargeStats($period, $serverId) {
        $dateFilter = $this->getDateFilter($period);
        
        $where = "created_at >= ?";
        $params = [$dateFilter];
        
        if ($serverId) {
            $where .= " AND server_id = ?";
            $params[] = $serverId;
        }
        
        // Total revenue
        $stmt = $this->db->prepare("
            SELECT 
                COUNT(*) as total_transactions,
                SUM(amount) as total_revenue,
                SUM(CASE WHEN status = 'completed' THEN amount ELSE 0 END) as completed_revenue,
                SUM(CASE WHEN status = 'pending' THEN amount ELSE 0 END) as pending_revenue,
                SUM(CASE WHEN status = 'failed' THEN amount ELSE 0 END) as failed_revenue
            FROM recharge_transactions 
            WHERE $where
        ");
        $stmt->execute($params);
        $stats = $stmt->fetch();
        
        // Count by status
        $stmt = $this->db->prepare("
            SELECT status, COUNT(*) as count 
            FROM recharge_transactions 
            WHERE $where
            GROUP BY status
        ");
        $stmt->execute($params);
        $statusCounts = $stmt->fetchAll();
        
        // Count by type
        $stmt = $this->db->prepare("
            SELECT type, COUNT(*) as count, SUM(amount) as total_amount
            FROM recharge_transactions 
            WHERE $where
            GROUP BY type
        ");
        $stmt->execute($params);
        $typeCounts = $stmt->fetchAll();
        
        // Unique users
        $stmt = $this->db->prepare("
            SELECT COUNT(DISTINCT user_id) as unique_users 
            FROM recharge_transactions 
            WHERE $where AND status = 'completed'
        ");
        $stmt->execute($params);
        $uniqueUsers = $stmt->fetch()['unique_users'];
        
        return [
            'total_transactions' => (int)$stats['total_transactions'],
            'total_revenue' => (float)$stats['total_revenue'],
            'completed_revenue' => (float)$stats['completed_revenue'],
            'pending_revenue' => (float)$stats['pending_revenue'],
            'failed_revenue' => (float)$stats['failed_revenue'],
            'unique_users' => (int)$uniqueUsers,
            'status_breakdown' => $statusCounts,
            'type_breakdown' => $typeCounts,
            'period' => $period
        ];
    }
    
    /**
     * Lấy lịch sử nạp của user
     */
    public function getUserRechargeHistory($userId, $page, $limit) {
        $offset = ($page - 1) * $limit;
        
        // Get total count
        $stmt = $this->db->prepare("SELECT COUNT(*) as total FROM recharge_transactions WHERE user_id = ?");
        $stmt->execute([$userId]);
        $total = $stmt->fetch()['total'];
        
        // Get history
        $sql = "SELECT * FROM recharge_transactions 
                WHERE user_id = ? 
                ORDER BY created_at DESC 
                LIMIT $limit OFFSET $offset";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$userId]);
        $history = $stmt->fetchAll();
        
        // Get summary
        $stmt = $this->db->prepare("
            SELECT 
                SUM(CASE WHEN status = 'completed' THEN amount ELSE 0 END) as total_recharged,
                COUNT(*) as total_transactions
            FROM recharge_transactions 
            WHERE user_id = ?
        ");
        $stmt->execute([$userId]);
        $summary = $stmt->fetch();
        
        return [
            'history' => $history,
            'summary' => $summary,
            'pagination' => [
                'total' => (int)$total,
                'page' => $page,
                'limit' => $limit,
                'total_pages' => ceil($total / $limit)
            ]
        ];
    }
    
    /**
     * Lấy webhook logs
     */
    public function getWebhookLogs($page, $limit, $type) {
        $offset = ($page - 1) * $limit;
        
        $where = "1=1";
        $params = [];
        
        if ($type !== 'all') {
            $where .= " AND type = ?";
            $params[] = $type;
        }
        
        // Get total count
        $stmt = $this->db->prepare("SELECT COUNT(*) as total FROM webhook_logs WHERE $where");
        $stmt->execute($params);
        $total = $stmt->fetch()['total'];
        
        // Get logs
        $sql = "SELECT * FROM webhook_logs 
                WHERE $where 
                ORDER BY created_at DESC 
                LIMIT $limit OFFSET $offset";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute($params);
        $logs = $stmt->fetchAll();
        
        return [
            'logs' => $logs,
            'pagination' => [
                'total' => (int)$total,
                'page' => $page,
                'limit' => $limit,
                'total_pages' => ceil($total / $limit)
            ]
        ];
    }
    
    /**
     * Export giao dịch ra CSV
     */
    public function exportTransactions($status, $type, $dateFrom, $dateTo) {
        $where = "1=1";
        $params = [];
        
        if ($status !== 'all') {
            $where .= " AND status = ?";
            $params[] = $status;
        }
        
        if ($type !== 'all') {
            $where .= " AND type = ?";
            $params[] = $type;
        }
        
        if ($dateFrom) {
            $where .= " AND created_at >= ?";
            $params[] = $dateFrom;
        }
        
        if ($dateTo) {
            $where .= " AND created_at <= ?";
            $params[] = $dateTo . ' 23:59:59';
        }
        
        $sql = "SELECT * FROM recharge_transactions WHERE $where ORDER BY created_at DESC";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute($params);
        $transactions = $stmt->fetchAll();
        
        // Create CSV
        $output = fopen('php://temp', 'r+');
        
        // Header
        fputcsv($output, [
            'ID', 'Order ID', 'User ID', 'Username', 'Server ID', 'Amount', 
            'Reward Amount', 'Luong Khoa', 'Type', 'Status', 'Transaction ID', 
            'Bank Name', 'Payment Time', 'Created At'
        ]);
        
        // Data
        foreach ($transactions as $tx) {
            fputcsv($output, [
                $tx['id'],
                $tx['order_id'],
                $tx['user_id'],
                $tx['username'],
                $tx['server_id'],
                $tx['amount'],
                $tx['reward_amount'],
                $tx['luong_khoa'],
                $tx['type'],
                $tx['status'],
                $tx['transaction_id'],
                $tx['bank_name'],
                $tx['payment_time'],
                $tx['created_at']
            ]);
        }
        
        rewind($output);
        $csv = stream_get_contents($output);
        fclose($output);
        
        return $csv;
    }
    
    /**
     * Tạo giao dịch thủ công
     */
    public function createManualRecharge($userId, $amount, $type, $serverId, $note) {
        // Verify user exists
        $stmt = $this->db->prepare("SELECT username FROM team_user WHERE id = ?");
        $stmt->execute([$userId]);
        $user = $stmt->fetch();
        
        if (!$user) {
            return ['success' => false, 'message' => 'Người dùng không tồn tại'];
        }
        
        try {
            $orderId = 'MANUAL_' . time() . '_' . $userId;
            
            $sql = "INSERT INTO recharge_transactions 
                    (order_id, user_id, username, server_id, amount, type, status, created_at) 
                    VALUES (?, ?, ?, ?, ?, ?, 'completed', NOW())";
            
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$orderId, $userId, $user['username'], $serverId, $amount, $type]);
            
            $transactionId = $this->db->lastInsertId();
            
            // Log the manual recharge
            $this->logAdminAction($transactionId, 'manual_create', $note);
            
            return [
                'success' => true,
                'transaction' => $this->getTransactionDetail($transactionId)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Tạo giao dịch thất bại'];
        }
    }
    
    /**
     * Xóa giao dịch
     */
    public function deleteTransaction($transactionId) {
        // Check if can delete (only pending or failed)
        $stmt = $this->db->prepare("SELECT status FROM recharge_transactions WHERE id = ?");
        $stmt->execute([$transactionId]);
        $transaction = $stmt->fetch();
        
        if (!$transaction) {
            return ['success' => false, 'message' => 'Không tìm thấy giao dịch'];
        }
        
        if (!in_array($transaction['status'], ['pending', 'failed'])) {
            return ['success' => false, 'message' => 'Chỉ có thể xóa giao dịch pending hoặc failed'];
        }
        
        try {
            $this->db->prepare("DELETE FROM recharge_transactions WHERE id = ?")
                     ->execute([$transactionId]);
            
            $this->logAdminAction($transactionId, 'delete', 'Transaction deleted by admin');
            
            return ['success' => true];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Xóa thất bại'];
        }
    }
    
    /**
     * Lấy biểu đồ doanh thu
     */
    public function getRevenueChart($period, $serverId) {
        $chart = [];
        
        if ($period === '7days') {
            for ($i = 6; $i >= 0; $i--) {
                $date = date('Y-m-d', strtotime("-$i days"));
                $chart[] = $this->getRevenueForDate($date, $serverId);
            }
        } elseif ($period === '30days') {
            for ($i = 29; $i >= 0; $i--) {
                $date = date('Y-m-d', strtotime("-$i days"));
                $chart[] = $this->getRevenueForDate($date, $serverId);
            }
        } elseif ($period === '12months') {
            for ($i = 11; $i >= 0; $i--) {
                $month = date('Y-m', strtotime("-$i months"));
                $chart[] = $this->getRevenueForMonth($month, $serverId);
            }
        }
        
        return [
            'chart' => $chart,
            'period' => $period
        ];
    }
    
    // Helper methods
    
    private function getRevenueForDate($date, $serverId) {
        $where = "DATE(created_at) = ? AND status = 'completed'";
        $params = [$date];
        
        if ($serverId) {
            $where .= " AND server_id = ?";
            $params[] = $serverId;
        }
        
        $stmt = $this->db->prepare("
            SELECT 
                COUNT(*) as transactions,
                SUM(amount) as revenue
            FROM recharge_transactions 
            WHERE $where
        ");
        $stmt->execute($params);
        $data = $stmt->fetch();
        
        return [
            'date' => $date,
            'transactions' => (int)$data['transactions'],
            'revenue' => (float)$data['revenue']
        ];
    }
    
    private function getRevenueForMonth($month, $serverId) {
        $where = "DATE_FORMAT(created_at, '%Y-%m') = ? AND status = 'completed'";
        $params = [$month];
        
        if ($serverId) {
            $where .= " AND server_id = ?";
            $params[] = $serverId;
        }
        
        $stmt = $this->db->prepare("
            SELECT 
                COUNT(*) as transactions,
                SUM(amount) as revenue
            FROM recharge_transactions 
            WHERE $where
        ");
        $stmt->execute($params);
        $data = $stmt->fetch();
        
        return [
            'month' => $month,
            'transactions' => (int)$data['transactions'],
            'revenue' => (float)$data['revenue']
        ];
    }
    
    private function getDateFilter($period) {
        switch ($period) {
            case 'today':
                return date('Y-m-d 00:00:00');
            case 'week':
                return date('Y-m-d 00:00:00', strtotime('-7 days'));
            case 'month':
                return date('Y-m-d 00:00:00', strtotime('-30 days'));
            default:
                return '1970-01-01 00:00:00';
        }
    }
    /**
 * Lấy danh sách recharge orders
 */
public function getOrders($page, $limit, $status, $type, $search) {
    $offset = ($page - 1) * $limit;
    
    $where = "1=1";
    $params = [];
    
    if ($status !== 'all') {
        $where .= " AND status = ?";
        $params[] = $status;
    }
    
    if ($type !== 'all') {
        $where .= " AND type = ?";
        $params[] = $type;
    }
    
    if (!empty($search)) {
        $where .= " AND (order_id LIKE ? OR username LIKE ? OR transaction_id LIKE ?)";
        $searchTerm = "%$search%";
        $params[] = $searchTerm;
        $params[] = $searchTerm;
        $params[] = $searchTerm;
    }
    
    // Get total count
    $countSql = "SELECT COUNT(*) as total FROM recharge_orders WHERE $where";
    $stmt = $this->db->prepare($countSql);
    $stmt->execute($params);
    $total = $stmt->fetch()['total'];
    
    // Get orders
    $sql = "SELECT * FROM recharge_orders 
            WHERE $where 
            ORDER BY created_at DESC 
            LIMIT $limit OFFSET $offset";
    
    $stmt = $this->db->prepare($sql);
    $stmt->execute($params);
    $orders = $stmt->fetchAll();
    
    return [
        'orders' => $orders,
        'pagination' => [
            'total' => (int)$total,
            'page' => $page,
            'limit' => $limit,
            'total_pages' => ceil($total / $limit)
        ]
    ];
}

/**
 * Lấy chi tiết order
 */
public function getOrderDetail($orderId) {
    $sql = "SELECT ro.*, tu.phone, tu.email
            FROM recharge_orders ro
            LEFT JOIN team_user tu ON ro.user_id = tu.id
            WHERE ro.order_id = ?";
    
    $stmt = $this->db->prepare($sql);
    $stmt->execute([$orderId]);
    $order = $stmt->fetch();
    
    if (!$order) {
        return null;
    }
    
    // Get cancellation log if exists
    $stmt = $this->db->prepare("
        SELECT * FROM order_cancellation_logs 
        WHERE order_id = ? 
        ORDER BY created_at DESC LIMIT 1
    ");
    $stmt->execute([$orderId]);
    $order['cancellation_log'] = $stmt->fetch();
    
    // Get related transaction if exists
    $stmt = $this->db->prepare("
        SELECT * FROM recharge_transactions 
        WHERE order_id = ? 
        LIMIT 1
    ");
    $stmt->execute([$orderId]);
    $order['transaction'] = $stmt->fetch();
    
    return $order;
}

/**
 * Cập nhật trạng thái order
 */
public function updateOrderStatus($orderId, $status, $note) {
    $allowedStatuses = ['pending', 'paid', 'completed', 'failed', 'cancelled'];
    
    if (!in_array($status, $allowedStatuses)) {
        return ['success' => false, 'message' => 'Trạng thái không hợp lệ'];
    }
    
    try {
        $this->db->beginTransaction();
        
        // Get current order
        $stmt = $this->db->prepare("SELECT * FROM recharge_orders WHERE order_id = ?");
        $stmt->execute([$orderId]);
        $order = $stmt->fetch();
        
        if (!$order) {
            $this->db->rollBack();
            return ['success' => false, 'message' => 'Không tìm thấy order'];
        }
        
        // Update order status
        $sql = "UPDATE recharge_orders SET status = ?, updated_at = NOW() WHERE order_id = ?";
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$status, $orderId]);
        
        // If cancelled, log the cancellation
        if ($status === 'cancelled') {
            $stmt = $this->db->prepare("
                INSERT INTO order_cancellation_logs (order_id, user_id, reason, created_at)
                VALUES (?, ?, ?, NOW())
            ");
            $stmt->execute([$orderId, $order['user_id'], $note ?: 'Cancelled by admin']);
        }
        
        $this->db->commit();
        
        return [
            'success' => true,
            'order' => $this->getOrderDetail($orderId)
        ];
    } catch (PDOException $e) {
        $this->db->rollBack();
        return ['success' => false, 'message' => 'Cập nhật thất bại: ' . $e->getMessage()];
    }
}

/**
 * Lấy cancellation logs
 */
public function getCancellationLogs($page, $limit, $userId) {
    $offset = ($page - 1) * $limit;
    
    $where = "1=1";
    $params = [];
    
    if ($userId) {
        $where .= " AND user_id = ?";
        $params[] = $userId;
    }
    
    // Get total count
    $stmt = $this->db->prepare("SELECT COUNT(*) as total FROM order_cancellation_logs WHERE $where");
    $stmt->execute($params);
    $total = $stmt->fetch()['total'];
    
    // Get logs with user info
    $sql = "SELECT ocl.*, tu.username, tu.phone
            FROM order_cancellation_logs ocl
            LEFT JOIN team_user tu ON ocl.user_id = tu.id
            WHERE $where 
            ORDER BY ocl.created_at DESC 
            LIMIT $limit OFFSET $offset";
    
    $stmt = $this->db->prepare($sql);
    $stmt->execute($params);
    $logs = $stmt->fetchAll();
    
    return [
        'logs' => $logs,
        'pagination' => [
            'total' => (int)$total,
            'page' => $page,
            'limit' => $limit,
            'total_pages' => ceil($total / $limit)
        ]
    ];
}

/**
 * Thống kê orders
 */
public function getOrderStats($period) {
    $dateFilter = $this->getDateFilter($period);
    
    // Order statistics
    $stmt = $this->db->prepare("
        SELECT 
            COUNT(*) as total_orders,
            SUM(CASE WHEN status = 'pending' THEN 1 ELSE 0 END) as pending_count,
            SUM(CASE WHEN status = 'paid' THEN 1 ELSE 0 END) as paid_count,
            SUM(CASE WHEN status = 'completed' THEN 1 ELSE 0 END) as completed_count,
            SUM(CASE WHEN status = 'failed' THEN 1 ELSE 0 END) as failed_count,
            SUM(CASE WHEN status = 'cancelled' THEN 1 ELSE 0 END) as cancelled_count,
            SUM(CASE WHEN status = 'completed' THEN amount ELSE 0 END) as completed_amount,
            SUM(amount) as total_amount
        FROM recharge_orders 
        WHERE created_at >= ?
    ");
    $stmt->execute([$dateFilter]);
    $stats = $stmt->fetch();
    
    // Type breakdown
    $stmt = $this->db->prepare("
        SELECT type, COUNT(*) as count, SUM(amount) as total_amount
        FROM recharge_orders 
        WHERE created_at >= ?
        GROUP BY type
    ");
    $stmt->execute([$dateFilter]);
    $typeBreakdown = $stmt->fetchAll();
    
    // Cancellation stats
    $stmt = $this->db->prepare("
        SELECT COUNT(*) as total_cancellations
        FROM order_cancellation_logs 
        WHERE created_at >= ?
    ");
    $stmt->execute([$dateFilter]);
    $cancellations = $stmt->fetch();
    
    return [
        'total_orders' => (int)$stats['total_orders'],
        'pending_count' => (int)$stats['pending_count'],
        'paid_count' => (int)$stats['paid_count'],
        'completed_count' => (int)$stats['completed_count'],
        'failed_count' => (int)$stats['failed_count'],
        'cancelled_count' => (int)$stats['cancelled_count'],
        'total_amount' => (float)$stats['total_amount'],
        'completed_amount' => (float)$stats['completed_amount'],
        'total_cancellations' => (int)$cancellations['total_cancellations'],
        'type_breakdown' => $typeBreakdown,
        'period' => $period
    ];
}

    private function logAdminAction($transactionId, $action, $note) {
        // Placeholder for logging admin actions
        error_log("Admin Recharge Action: Transaction $transactionId - $action - $note");
    }
}