<?php
// services/admin/AdminMilestoneService.php

class AdminMilestoneService {
    private $accountDb;
    
    public function __construct() {
        $this->accountDb = Database::getInstance()->getConnection();
    }
    
    /**
     * Get game database connection for specific server
     */
    private function getGameDb($serverId) {
        try {
            return Database::getGameInstance($serverId)->getConnection();
        } catch (Exception $e) {
            throw new Exception("Không thể kết nối database server $serverId: " . $e->getMessage());
        }
    }
    
    /**
     * Lấy danh sách milestones với phân trang và lọc
     */
    public function getMilestones($page, $limit, $search, $status) {
        $offset = ($page - 1) * $limit;
        
        // Build query
        $where = "1=1";
        $params = [];
        
        // Search filter
        if (!empty($search)) {
            $where .= " AND (milestone_amount LIKE ? OR description LIKE ?)";
            $searchTerm = "%$search%";
            $params[] = $searchTerm;
            $params[] = $searchTerm;
        }
        
        // Status filter
        if ($status === 'active') {
            $where .= " AND is_active = 1";
        } elseif ($status === 'inactive') {
            $where .= " AND is_active = 0";
        }
        
        // Get total count
        $countSql = "SELECT COUNT(*) as total FROM recharge_milestones WHERE $where";
        $stmt = $this->accountDb->prepare($countSql);
        $stmt->execute($params);
        $total = $stmt->fetch()['total'];
        
        // Get milestones with claim count
        $sql = "SELECT m.*, 
                COUNT(DISTINCT c.user_id) as total_claimed
                FROM recharge_milestones m
                LEFT JOIN user_milestone_claimed c ON m.id = c.milestone_id
                WHERE $where 
                GROUP BY m.id
                ORDER BY m.display_order ASC, m.milestone_amount ASC
                LIMIT $limit OFFSET $offset";
        
        $stmt = $this->accountDb->prepare($sql);
        $stmt->execute($params);
        $milestones = $stmt->fetchAll();
        
        // Format data
        foreach ($milestones as &$milestone) {
            $milestone['id'] = (int)$milestone['id'];
            $milestone['milestone_amount'] = (float)$milestone['milestone_amount'];
            $milestone['reward_xu'] = (int)$milestone['reward_xu'];
            $milestone['reward_luong'] = (int)$milestone['reward_luong'];
            $milestone['reward_luong_khoa'] = (int)$milestone['reward_luong_khoa'];
            $milestone['display_order'] = (int)$milestone['display_order'];
            $milestone['is_active'] = (int)$milestone['is_active'];
            $milestone['total_claimed'] = (int)$milestone['total_claimed'];
            
            // Parse items
            if (!empty($milestone['item'])) {
                $items = array_map('trim', explode(',', $milestone['item']));
                $milestone['items'] = array_filter($items);
            } else {
                $milestone['items'] = [];
            }
        }
        
        return [
            'milestones' => $milestones,
            'pagination' => [
                'total' => (int)$total,
                'page' => $page,
                'limit' => $limit,
                'total_pages' => ceil($total / $limit)
            ]
        ];
    }
    
    /**
     * Lấy chi tiết milestone
     */
    public function getMilestoneDetail($id) {
        $sql = "SELECT m.*, 
                COUNT(DISTINCT c.user_id) as total_claimed
                FROM recharge_milestones m
                LEFT JOIN user_milestone_claimed c ON m.id = c.milestone_id
                WHERE m.id = ?
                GROUP BY m.id";
        
        $stmt = $this->accountDb->prepare($sql);
        $stmt->execute([$id]);
        $milestone = $stmt->fetch();
        
        if (!$milestone) {
            return null;
        }
        
        // Format data
        $milestone['id'] = (int)$milestone['id'];
        $milestone['milestone_amount'] = (float)$milestone['milestone_amount'];
        $milestone['reward_xu'] = (int)$milestone['reward_xu'];
        $milestone['reward_luong'] = (int)$milestone['reward_luong'];
        $milestone['reward_luong_khoa'] = (int)$milestone['reward_luong_khoa'];
        $milestone['display_order'] = (int)$milestone['display_order'];
        $milestone['is_active'] = (int)$milestone['is_active'];
        $milestone['total_claimed'] = (int)$milestone['total_claimed'];
        
        // Parse items
        if (!empty($milestone['item'])) {
            $items = array_map('trim', explode(',', $milestone['item']));
            $milestone['items'] = array_filter($items);
        } else {
            $milestone['items'] = [];
        }
        
        return $milestone;
    }
    
    /**
     * Tạo milestone mới
     */
    public function createMilestone($data) {
        // Validate
        $errors = $this->validateMilestoneData($data);
        if (!empty($errors)) {
            return ['success' => false, 'errors' => $errors];
        }
        
        // Check if milestone_amount exists
        $stmt = $this->accountDb->prepare("SELECT id FROM recharge_milestones WHERE milestone_amount = ?");
        $stmt->execute([$data['milestone_amount']]);
        if ($stmt->fetch()) {
            return ['success' => false, 'message' => 'Mốc nạp với số tiền này đã tồn tại'];
        }
        
        try {
            // Prepare items
            $itemString = null;
            if (!empty($data['items']) && is_array($data['items'])) {
                $itemString = implode(',', $data['items']);
            }
            
            $sql = "INSERT INTO recharge_milestones 
                    (milestone_amount, reward_xu, reward_luong, reward_luong_khoa, item, description, display_order, is_active, created_at) 
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, NOW())";
            
            $stmt = $this->accountDb->prepare($sql);
            $stmt->execute([
                $data['milestone_amount'],
                $data['reward_xu'] ?? 0,
                $data['reward_luong'] ?? 0,
                $data['reward_luong_khoa'] ?? 0,
                $itemString,
                $data['description'] ?? '',
                $data['display_order'] ?? 0,
                $data['is_active'] ?? 1
            ]);
            
            $id = $this->accountDb->lastInsertId();
            
            return [
                'success' => true,
                'milestone' => $this->getMilestoneDetail($id)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Tạo mốc nạp thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Cập nhật milestone
     */
    public function updateMilestone($id, $data) {
        // Check if exists
        $milestone = $this->getMilestoneDetail($id);
        if (!$milestone) {
            return ['success' => false, 'message' => 'Mốc nạp không tồn tại'];
        }
        
        try {
            $updateFields = [];
            $params = [];
            
            if (isset($data['milestone_amount'])) {
                // Check duplicate
                $stmt = $this->accountDb->prepare("SELECT id FROM recharge_milestones WHERE milestone_amount = ? AND id != ?");
                $stmt->execute([$data['milestone_amount'], $id]);
                if ($stmt->fetch()) {
                    return ['success' => false, 'message' => 'Mốc nạp với số tiền này đã tồn tại'];
                }
                $updateFields[] = "milestone_amount = ?";
                $params[] = $data['milestone_amount'];
            }
            
            if (isset($data['reward_xu'])) {
                $updateFields[] = "reward_xu = ?";
                $params[] = $data['reward_xu'];
            }
            
            if (isset($data['reward_luong'])) {
                $updateFields[] = "reward_luong = ?";
                $params[] = $data['reward_luong'];
            }
            
            if (isset($data['reward_luong_khoa'])) {
                $updateFields[] = "reward_luong_khoa = ?";
                $params[] = $data['reward_luong_khoa'];
            }
            
            if (isset($data['items']) && is_array($data['items'])) {
                $updateFields[] = "item = ?";
                $params[] = implode(',', $data['items']);
            }
            
            if (isset($data['description'])) {
                $updateFields[] = "description = ?";
                $params[] = $data['description'];
            }
            
            if (isset($data['display_order'])) {
                $updateFields[] = "display_order = ?";
                $params[] = $data['display_order'];
            }
            
            if (isset($data['is_active'])) {
                $updateFields[] = "is_active = ?";
                $params[] = $data['is_active'];
            }
            
            if (empty($updateFields)) {
                return ['success' => false, 'message' => 'Không có dữ liệu để cập nhật'];
            }
            
            $params[] = $id;
            
            $sql = "UPDATE recharge_milestones SET " . implode(', ', $updateFields) . " WHERE id = ?";
            $stmt = $this->accountDb->prepare($sql);
            $stmt->execute($params);
            
            return [
                'success' => true,
                'milestone' => $this->getMilestoneDetail($id)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Cập nhật thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Xóa milestone
     */
    public function deleteMilestone($id) {
        try {
            $this->accountDb->beginTransaction();
            
            // Xóa claims trước
            $stmt = $this->accountDb->prepare("DELETE FROM user_milestone_claimed WHERE milestone_id = ?");
            $stmt->execute([$id]);
            
            // Xóa milestone
            $stmt = $this->accountDb->prepare("DELETE FROM recharge_milestones WHERE id = ?");
            $stmt->execute([$id]);
            
            if ($stmt->rowCount() === 0) {
                $this->accountDb->rollBack();
                return ['success' => false, 'message' => 'Mốc nạp không tồn tại'];
            }
            
            $this->accountDb->commit();
            return ['success' => true];
        } catch (PDOException $e) {
            $this->accountDb->rollBack();
            return ['success' => false, 'message' => 'Xóa thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Lấy lịch sử claim của milestone
     */
    public function getMilestoneClaimLog($milestoneId, $page, $limit) {
        $offset = ($page - 1) * $limit;
        
        // Check if milestone exists
        $stmt = $this->accountDb->prepare("SELECT id, milestone_amount FROM recharge_milestones WHERE id = ?");
        $stmt->execute([$milestoneId]);
        $milestone = $stmt->fetch();
        
        if (!$milestone) {
            return null;
        }
        
        // Count
        $stmt = $this->accountDb->prepare("SELECT COUNT(*) as total FROM user_milestone_claimed WHERE milestone_id = ?");
        $stmt->execute([$milestoneId]);
        $total = $stmt->fetch()['total'];
        
        // Get logs
        $sql = "SELECT * FROM user_milestone_claimed 
                WHERE milestone_id = ? 
                ORDER BY claimed_at DESC 
                LIMIT $limit OFFSET $offset";
        
        $stmt = $this->accountDb->prepare($sql);
        $stmt->execute([$milestoneId]);
        $logs = $stmt->fetchAll();
        
        // Format
        foreach ($logs as &$log) {
            $log['id'] = (int)$log['id'];
            $log['user_id'] = (int)$log['user_id'];
            $log['server_id'] = (int)$log['server_id'];
            $log['milestone_id'] = (int)$log['milestone_id'];
            $log['milestone_amount'] = (float)$log['milestone_amount'];
        }
        
        return [
            'milestone_id' => (int)$milestoneId,
            'milestone_amount' => (float)$milestone['milestone_amount'],
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
     * Thống kê milestone
     */
    public function getMilestoneStats() {
        // Total milestones
        $stmt = $this->accountDb->query("SELECT COUNT(*) as total FROM recharge_milestones");
        $totalMilestones = $stmt->fetch()['total'];
        
        // Active milestones
        $stmt = $this->accountDb->query("SELECT COUNT(*) as total FROM recharge_milestones WHERE is_active = 1");
        $activeMilestones = $stmt->fetch()['total'];
        
        // Inactive milestones
        $stmt = $this->accountDb->query("SELECT COUNT(*) as total FROM recharge_milestones WHERE is_active = 0");
        $inactiveMilestones = $stmt->fetch()['total'];
        
        // Total claims
        $stmt = $this->accountDb->query("SELECT COUNT(*) as total FROM user_milestone_claimed");
        $totalClaims = $stmt->fetch()['total'];
        
        // Unique users claimed
        $stmt = $this->accountDb->query("SELECT COUNT(DISTINCT user_id) as total FROM user_milestone_claimed");
        $uniqueUsers = $stmt->fetch()['total'];
        
        // Most claimed milestones
        $stmt = $this->accountDb->query("
            SELECT m.id, m.milestone_amount, m.description, COUNT(c.id) as claim_count
            FROM recharge_milestones m
            LEFT JOIN user_milestone_claimed c ON m.id = c.milestone_id
            GROUP BY m.id
            ORDER BY claim_count DESC
            LIMIT 10
        ");
        $mostClaimed = $stmt->fetchAll();
        
        // Recent claims (last 7 days)
        $stmt = $this->accountDb->query("
            SELECT DATE(claimed_at) as date, COUNT(*) as count
            FROM user_milestone_claimed
            WHERE claimed_at >= DATE_SUB(NOW(), INTERVAL 7 DAY)
            GROUP BY DATE(claimed_at)
            ORDER BY date DESC
        ");
        $recentClaims = $stmt->fetchAll();
        
        // Claims by server
        $stmt = $this->accountDb->query("
            SELECT server_id, COUNT(*) as count
            FROM user_milestone_claimed
            GROUP BY server_id
            ORDER BY count DESC
        ");
        $byServer = $stmt->fetchAll();
        
        return [
            'total_milestones' => (int)$totalMilestones,
            'active_milestones' => (int)$activeMilestones,
            'inactive_milestones' => (int)$inactiveMilestones,
            'total_claims' => (int)$totalClaims,
            'unique_users' => (int)$uniqueUsers,
            'most_claimed' => $mostClaimed,
            'recent_claims' => $recentClaims,
            'by_server' => $byServer
        ];
    }
    
    /**
     * Lấy danh sách users đã claim milestone
     */
    public function getMilestoneUsers($milestoneId, $serverId = null, $page = 1, $limit = 20) {
        $offset = ($page - 1) * $limit;
        
        $where = "c.milestone_id = ?";
        $params = [$milestoneId];
        
        if ($serverId) {
            $where .= " AND c.server_id = ?";
            $params[] = $serverId;
        }
        
        // Count
        $countSql = "SELECT COUNT(*) as total FROM user_milestone_claimed c WHERE $where";
        $stmt = $this->accountDb->prepare($countSql);
        $stmt->execute($params);
        $total = $stmt->fetch()['total'];
        
        // Get users with topnap info
        $sql = "SELECT c.*, t.total_amount, t.total_recharge
                FROM user_milestone_claimed c
                LEFT JOIN topnap t ON c.user_id = t.user_id AND c.server_id = t.server_id
                WHERE $where
                ORDER BY c.claimed_at DESC
                LIMIT $limit OFFSET $offset";
        
        $stmt = $this->accountDb->prepare($sql);
        $stmt->execute($params);
        $users = $stmt->fetchAll();
        
        return [
            'milestone_id' => (int)$milestoneId,
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
     * Export milestones to CSV
     */
    public function exportMilestones($search, $status) {
        $where = "1=1";
        $params = [];
        
        if (!empty($search)) {
            $where .= " AND (milestone_amount LIKE ? OR description LIKE ?)";
            $searchTerm = "%$search%";
            $params[] = $searchTerm;
            $params[] = $searchTerm;
        }
        
        if ($status === 'active') {
            $where .= " AND is_active = 1";
        } elseif ($status === 'inactive') {
            $where .= " AND is_active = 0";
        }
        
        $sql = "SELECT m.*, COUNT(DISTINCT c.user_id) as total_claimed
                FROM recharge_milestones m
                LEFT JOIN user_milestone_claimed c ON m.id = c.milestone_id
                WHERE $where 
                GROUP BY m.id
                ORDER BY m.display_order ASC, m.milestone_amount ASC";
        
        $stmt = $this->accountDb->prepare($sql);
        $stmt->execute($params);
        $milestones = $stmt->fetchAll();
        
        // Create CSV
        $output = fopen('php://temp', 'r+');
        
        // Header
        fputcsv($output, ['ID', 'Số tiền', 'Xu', 'Lượng', 'Lượng Khóa', 'Items', 'Mô tả', 'Thứ tự', 'Trạng thái', 'Đã claim']);
        
        // Data
        foreach ($milestones as $m) {
            fputcsv($output, [
                $m['id'],
                $m['milestone_amount'],
                $m['reward_xu'],
                $m['reward_luong'],
                $m['reward_luong_khoa'],
                $m['item'],
                $m['description'],
                $m['display_order'],
                $m['is_active'] ? 'Active' : 'Inactive',
                $m['total_claimed']
            ]);
        }
        
        rewind($output);
        $csv = stream_get_contents($output);
        fclose($output);
        
        return $csv;
    }
    
    /**
     * Xóa hàng loạt
     */
    public function bulkDelete($milestoneIds) {
        if (empty($milestoneIds) || !is_array($milestoneIds)) {
            return ['success' => false, 'message' => 'Danh sách ID không hợp lệ'];
        }
        
        try {
            $this->accountDb->beginTransaction();
            
            // Delete claims first
            $placeholders = implode(',', array_fill(0, count($milestoneIds), '?'));
            $sql = "DELETE FROM user_milestone_claimed WHERE milestone_id IN ($placeholders)";
            $stmt = $this->accountDb->prepare($sql);
            $stmt->execute($milestoneIds);
            
            // Delete milestones
            $sql = "DELETE FROM recharge_milestones WHERE id IN ($placeholders)";
            $stmt = $this->accountDb->prepare($sql);
            $stmt->execute($milestoneIds);
            
            $deletedCount = $stmt->rowCount();
            
            $this->accountDb->commit();
            
            return [
                'success' => true,
                'deleted_count' => $deletedCount
            ];
        } catch (PDOException $e) {
            $this->accountDb->rollBack();
            return ['success' => false, 'message' => 'Xóa thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Toggle active status
     */
    public function toggleActive($id) {
        try {
            $milestone = $this->getMilestoneDetail($id);
            if (!$milestone) {
                return ['success' => false, 'message' => 'Mốc nạp không tồn tại'];
            }
            
            $newStatus = $milestone['is_active'] ? 0 : 1;
            
            $stmt = $this->accountDb->prepare("UPDATE recharge_milestones SET is_active = ? WHERE id = ?");
            $stmt->execute([$newStatus, $id]);
            
            return [
                'success' => true,
                'milestone' => $this->getMilestoneDetail($id)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Cập nhật thất bại: ' . $e->getMessage()];
        }
    }
    
    // Helper methods
    
    private function validateMilestoneData($data) {
        $errors = [];
        
        if (empty($data['milestone_amount']) || $data['milestone_amount'] <= 0) {
            $errors['milestone_amount'] = 'Số tiền mốc nạp không hợp lệ';
        }
        
        if (!isset($data['reward_xu']) && !isset($data['reward_luong']) && !isset($data['reward_luong_khoa']) && empty($data['items'])) {
            $errors['rewards'] = 'Mốc nạp phải có ít nhất một phần thưởng';
        }
        
        return $errors;
    }
}