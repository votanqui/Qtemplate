<?php
// services/AdminAuthService.php

class AdminService {
    private $db;
    
    public function __construct() {
        $this->db = Database::getInstance()->getConnection();
    }
    
    /**
     * Lấy danh sách users với phân trang và tìm kiếm
     */
    public function getUsers($page, $limit, $search, $status, $sortBy, $sortOrder) {
        $offset = ($page - 1) * $limit;
        
        // Build query
        $where = "1=1";
        $params = [];
        
        // Search filter
        if (!empty($search)) {
            $where .= " AND (username LIKE ? OR phone LIKE ? OR email LIKE ?)";
            $searchTerm = "%$search%";
            $params[] = $searchTerm;
            $params[] = $searchTerm;
            $params[] = $searchTerm;
        }
        
        // Status filter
        if ($status === 'active') {
            $where .= " AND ban = 0";
        } elseif ($status === 'banned') {
            $where .= " AND ban = 1";
        }
        
        // Validate sort
        $allowedSort = ['id', 'username', 'regdate', 'ban'];
        $sortBy = in_array($sortBy, $allowedSort) ? $sortBy : 'regdate';
        $sortOrder = strtoupper($sortOrder) === 'ASC' ? 'ASC' : 'DESC';
        
        // Get total count
        $countSql = "SELECT COUNT(*) as total FROM team_user WHERE $where";
        $stmt = $this->db->prepare($countSql);
        $stmt->execute($params);
        $total = $stmt->fetch()['total'];
        
        // Get users
        $sql = "SELECT 
                    id, username, phone, email, regdate, ban, active, 
                    isAdmin, ip_addr, last_post, point_post
                FROM team_user 
                WHERE $where 
                ORDER BY $sortBy $sortOrder 
                LIMIT $limit OFFSET $offset";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute($params);
        $users = $stmt->fetchAll();
        
        return [
            'users' => $users,
            'pagination' => [
                'total' => (int)$total,
                'page' => $page,
                'limit' => $limit,
                'total_pages' => ceil($total / $limit)
            ]
        ];
    }
    
    /**
     * Lấy thông tin chi tiết user
     */
    public function getUserDetail($userId) {
        $sql = "SELECT 
                    id, username, phone, email, regdate, ban, active, 
                    isAdmin, ip_addr, last_post, point_post, provider, fromgame
                FROM team_user 
                WHERE id = ?";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$userId]);
        $user = $stmt->fetch();
        
        if (!$user) {
            return null;
        }
        
        // Get active sessions count
        $stmt = $this->db->prepare("
            SELECT COUNT(*) as session_count 
            FROM team_auth 
            WHERE user_id = ? AND is_active = 1
        ");
        $stmt->execute([$userId]);
        $user['active_sessions'] = $stmt->fetch()['session_count'];
        
        return $user;
    }
    
    /**
     * Cập nhật thông tin user
     */
    public function updateUser($userId, $data) {
        $allowedFields = ['email', 'phone', 'isAdmin', 'active'];
        $updateFields = [];
        $params = [];
        
        foreach ($allowedFields as $field) {
            if (isset($data[$field])) {
                $updateFields[] = "$field = ?";
                $params[] = $data[$field];
            }
        }
        
        if (empty($updateFields)) {
            return ['success' => false, 'message' => 'Không có dữ liệu để cập nhật'];
        }
        
        $params[] = $userId;
        
        $sql = "UPDATE team_user SET " . implode(', ', $updateFields) . " WHERE id = ?";
        
        try {
            $stmt = $this->db->prepare($sql);
            $stmt->execute($params);
            
            // Log activity
            $this->logActivity($userId, 'update', 'Admin updated user info');
            
            return [
                'success' => true,
                'user' => $this->getUserDetail($userId)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Cập nhật thất bại'];
        }
    }
    
    /**
     * Ban/Unban user
     */
    public function toggleBan($userId, $action, $reason) {
        $banValue = ($action === 'ban') ? 1 : 0;
        
        $sql = "UPDATE team_user SET ban = ? WHERE id = ?";
        
        try {
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$banValue, $userId]);
            
            // Kick all sessions if banning
            if ($action === 'ban') {
                $this->db->prepare("UPDATE team_auth SET is_active = 0 WHERE user_id = ?")
                         ->execute([$userId]);
            }
            
            // Log activity
            $this->logActivity($userId, $action, $reason ?: "User $action by admin");
            
            return [
                'success' => true,
                'banned' => $banValue,
                'message' => $action === 'ban' ? 'Đã khóa tài khoản' : 'Đã mở khóa tài khoản'
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Thao tác thất bại'];
        }
    }
    
    /**
     * Xóa user (soft delete)
     */
    public function deleteUser($userId) {
        // Check if user is admin
        $stmt = $this->db->prepare("SELECT isAdmin FROM team_user WHERE id = ?");
        $stmt->execute([$userId]);
        $user = $stmt->fetch();
        
        if ($user && $user['isAdmin'] == 1) {
            return ['success' => false, 'message' => 'Không thể xóa tài khoản admin'];
        }
        
        try {
            // Soft delete: set active = 0, ban = 1
            $sql = "UPDATE team_user SET active = 0, ban = 1 WHERE id = ?";
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$userId]);
            
            // Deactivate all sessions
            $this->db->prepare("UPDATE team_auth SET is_active = 0 WHERE user_id = ?")
                     ->execute([$userId]);
            
            // Log activity
            $this->logActivity($userId, 'delete', 'User deleted by admin');
            
            return ['success' => true];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Xóa thất bại'];
        }
    }
    
    /**
     * Reset password
     */
    public function resetPassword($userId, $newPassword) {
        $hashedPassword = '*' . strtoupper(sha1(sha1($newPassword, true)));
        
        try {
            $sql = "UPDATE team_user SET password = ? WHERE id = ?";
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$hashedPassword, $userId]);
            
            // Logout all sessions
            $this->db->prepare("UPDATE team_auth SET is_active = 0 WHERE user_id = ?")
                     ->execute([$userId]);
            
            // Log activity
            $this->logActivity($userId, 'reset_password', 'Password reset by admin');
            
            return ['success' => true];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Reset mật khẩu thất bại'];
        }
    }
    
    /**
     * Lấy thống kê
     */
    public function getStats($period) {
        $dateFilter = $this->getDateFilter($period);
        
        // Total users
        $stmt = $this->db->query("SELECT COUNT(*) as total FROM team_user");
        $totalUsers = $stmt->fetch()['total'];
        
        // New users in period
        $stmt = $this->db->prepare("SELECT COUNT(*) as total FROM team_user WHERE regdate >= ?");
        $stmt->execute([$dateFilter]);
        $newUsers = $stmt->fetch()['total'];
        
        // Banned users
        $stmt = $this->db->query("SELECT COUNT(*) as total FROM team_user WHERE ban = 1");
        $bannedUsers = $stmt->fetch()['total'];
        
        // Active sessions
        $stmt = $this->db->query("SELECT COUNT(*) as total FROM team_auth WHERE is_active = 1");
        $activeSessions = $stmt->fetch()['total'];
        
        // User registration trend (last 7 days)
        $trend = [];
        for ($i = 6; $i >= 0; $i--) {
            $date = date('Y-m-d', strtotime("-$i days"));
            $stmt = $this->db->prepare("
                SELECT COUNT(*) as count 
                FROM team_user 
                WHERE DATE(regdate) = ?
            ");
            $stmt->execute([$date]);
            $trend[] = [
                'date' => $date,
                'count' => (int)$stmt->fetch()['count']
            ];
        }
        
        return [
            'total_users' => (int)$totalUsers,
            'new_users' => (int)$newUsers,
            'banned_users' => (int)$bannedUsers,
            'active_sessions' => (int)$activeSessions,
            'registration_trend' => $trend,
            'period' => $period
        ];
    }
    
    /**
     * Lấy lịch sử đăng nhập
     */
    public function getLoginHistory($userId, $page, $limit) {
        $offset = ($page - 1) * $limit;
        
        // Get total count
        $stmt = $this->db->prepare("SELECT COUNT(*) as total FROM team_auth WHERE user_id = ?");
        $stmt->execute([$userId]);
        $total = $stmt->fetch()['total'];
        
        // Get history
        $sql = "SELECT 
                    id, ip_address, user_agent, created_at, last_used, is_active
                FROM team_auth 
                WHERE user_id = ? 
                ORDER BY created_at DESC 
                LIMIT $limit OFFSET $offset";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$userId]);
        $history = $stmt->fetchAll();
        
        return [
            'history' => $history,
            'pagination' => [
                'total' => (int)$total,
                'page' => $page,
                'limit' => $limit,
                'total_pages' => ceil($total / $limit)
            ]
        ];
    }
    
    /**
     * Tạo user mới
     */
    public function createUser($username, $password, $phone, $email, $isAdmin) {

        $authService = new AuthService();
        
        // Register user
        $result = $authService->register($username, $password, $phone, $email);
        
        if (!$result['success']) {
            return $result;
        }
        
        // Update isAdmin if needed
        if ($isAdmin == 1) {
            $this->db->prepare("UPDATE team_user SET isAdmin = 1 WHERE id = ?")
                     ->execute([$result['user_id']]);
        }
        
        // Log activity
        $this->logActivity($result['user_id'], 'create', 'User created by admin');
        
        return [
            'success' => true,
            'user' => $this->getUserDetail($result['user_id'])
        ];
    }
    
    /**
     * Export users to CSV
     */
    public function exportUsers($search, $status) {
        $where = "1=1";
        $params = [];
        
        if (!empty($search)) {
            $where .= " AND (username LIKE ? OR phone LIKE ? OR email LIKE ?)";
            $searchTerm = "%$search%";
            $params[] = $searchTerm;
            $params[] = $searchTerm;
            $params[] = $searchTerm;
        }
        
        if ($status === 'active') {
            $where .= " AND ban = 0";
        } elseif ($status === 'banned') {
            $where .= " AND ban = 1";
        }
        
        $sql = "SELECT 
                    id, username, phone, email, regdate, ban, active, isAdmin, ip_addr
                FROM team_user 
                WHERE $where 
                ORDER BY regdate DESC";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute($params);
        $users = $stmt->fetchAll();
        
        // Create CSV
        $output = fopen('php://temp', 'r+');
        
        // Header
        fputcsv($output, ['ID', 'Username', 'Phone', 'Email', 'Reg Date', 'Banned', 'Active', 'Is Admin', 'IP Address']);
        
        // Data
        foreach ($users as $user) {
            fputcsv($output, [
                $user['id'],
                $user['username'],
                $user['phone'],
                $user['email'],
                $user['regdate'],
                $user['ban'] ? 'Yes' : 'No',
                $user['active'] ? 'Yes' : 'No',
                $user['isAdmin'] ? 'Yes' : 'No',
                $user['ip_addr']
            ]);
        }
        
        rewind($output);
        $csv = stream_get_contents($output);
        fclose($output);
        
        return $csv;
    }
    
    /**
     * Lấy logs
     */
    public function getLogs($page, $limit, $action, $userId) {
        // Note: This requires an admin_logs table to be created
        // For now, return from team_auth as placeholder
        $offset = ($page - 1) * $limit;
        
        $where = "1=1";
        $params = [];
        
        if ($userId) {
            $where .= " AND user_id = ?";
            $params[] = $userId;
        }
        
        // Get total
        $stmt = $this->db->prepare("SELECT COUNT(*) as total FROM team_auth WHERE $where");
        $stmt->execute($params);
        $total = $stmt->fetch()['total'];
        
        // Get logs
        $sql = "SELECT * FROM team_auth WHERE $where ORDER BY created_at DESC LIMIT $limit OFFSET $offset";
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
    
    // Helper methods
    
    private function getDateFilter($period) {
        switch ($period) {
            case 'day':
                return date('Y-m-d 00:00:00');
            case 'week':
                return date('Y-m-d 00:00:00', strtotime('-7 days'));
            case 'month':
                return date('Y-m-d 00:00:00', strtotime('-30 days'));
            case 'year':
                return date('Y-m-d 00:00:00', strtotime('-365 days'));
            default:
                return date('Y-m-d 00:00:00');
        }
    }
    
    private function logActivity($userId, $action, $description) {
        // Placeholder for activity logging
        // Implement this based on your logging requirements
        error_log("Admin Activity: User $userId - $action - $description");
    }
}