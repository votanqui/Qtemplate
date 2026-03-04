<?php
// services/admin/AdminGiftcodeService.php

class AdminGiftcodeService {
    
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
     * Lấy danh sách giftcodes với phân trang và lọc
     */
    public function getGiftcodes($serverId, $page, $limit, $search, $type, $status) {
        $db = $this->getGameDb($serverId);
        $offset = ($page - 1) * $limit;
        
        // Build query
        $where = "1=1";
        $params = [];
        
        // Search filter
        if (!empty($search)) {
            $where .= " AND (g.giftcode LIKE ? OR g.id = ?)";
            $searchTerm = "%$search%";
            $params[] = $searchTerm;
            $params[] = is_numeric($search) ? $search : 0;
        }
        
        // Type filter
        if ($type !== 'all' && is_numeric($type)) {
            $where .= " AND g.type = ?";
            $params[] = $type;
        }
        
        // Status filter (expired, active, used_up)
        $currentTime = time();
        if ($status === 'active') {
            $where .= " AND (g.expire = 0 OR g.expire > ?)";
            $params[] = $currentTime;
        } elseif ($status === 'expired') {
            $where .= " AND g.expire > 0 AND g.expire <= ?";
            $params[] = $currentTime;
        }
        
        // Get total count
        $countSql = "SELECT COUNT(*) as total FROM giftcode g WHERE $where";
        $stmt = $db->prepare($countSql);
        $stmt->execute($params);
        $total = $stmt->fetch()['total'];
        
        // Get giftcodes with usage count
        $sql = "SELECT g.*, 
                COUNT(gl.id) as used_count,
                CASE 
                    WHEN g.expire > 0 AND g.expire <= ? THEN 'expired'
                    WHEN COUNT(gl.id) >= g.limit_use THEN 'used_up'
                    ELSE 'active'
                END as status
                FROM giftcode g
                LEFT JOIN giftcode_log gl ON g.giftcode = gl.giftcode
                WHERE $where 
                GROUP BY g.id
                ORDER BY g.id DESC 
                LIMIT $limit OFFSET $offset";
        
        array_unshift($params, $currentTime);
        
        $stmt = $db->prepare($sql);
        $stmt->execute($params);
        $giftcodes = $stmt->fetchAll();
        
        // Format data
         foreach ($giftcodes as &$giftcode) {
        $giftcode['server_id'] = (int)$serverId;
        $giftcode['id'] = (int)$giftcode['id'];
        $giftcode['xu'] = (int)$giftcode['xu'];
        $giftcode['luong'] = (int)$giftcode['luong'];
        $giftcode['luongLock'] = (int)$giftcode['luongLock'];
        $giftcode['expire'] = (int)$giftcode['expire'];
        $giftcode['limit_use'] = (int)$giftcode['limit_use'];
        $giftcode['type'] = (int)$giftcode['type'];
        $giftcode['used_count'] = (int)$giftcode['used_count'];
        $giftcode['remaining'] = max(0, $giftcode['limit_use'] - $giftcode['used_count']);
        
        // Parse item - THAY ĐỔI: từ JSON sang comma-separated string
        if (!empty($giftcode['item'])) {
            // Nếu là chuỗi phân tách bằng dấu phẩy
            $items = array_map('trim', explode(',', $giftcode['item']));
            $giftcode['items'] = array_filter($items); // loại bỏ empty values
        } else {
            $giftcode['items'] = [];
        }
    }
        
        return [
            'server_id' => (int)$serverId,
            'giftcodes' => $giftcodes,
            'pagination' => [
                'total' => (int)$total,
                'page' => $page,
                'limit' => $limit,
                'total_pages' => ceil($total / $limit)
            ]
        ];
    }
    
    /**
     * Lấy chi tiết giftcode
     */
    public function getGiftcodeDetail($serverId, $id) {
        $db = $this->getGameDb($serverId);
        
        $sql = "SELECT g.*, 
                COUNT(gl.id) as used_count
                FROM giftcode g
                LEFT JOIN giftcode_log gl ON g.giftcode = gl.giftcode
                WHERE g.id = ?
                GROUP BY g.id";
        
        $stmt = $db->prepare($sql);
        $stmt->execute([$id]);
        $giftcode = $stmt->fetch();
        
        if (!$giftcode) {
            return null;
        }
        
        // Format data
        $giftcode['server_id'] = (int)$serverId;
        $giftcode['id'] = (int)$giftcode['id'];
        $giftcode['xu'] = (int)$giftcode['xu'];
        $giftcode['luong'] = (int)$giftcode['luong'];
        $giftcode['luongLock'] = (int)$giftcode['luongLock'];
        $giftcode['expire'] = (int)$giftcode['expire'];
        $giftcode['limit_use'] = (int)$giftcode['limit_use'];
        $giftcode['type'] = (int)$giftcode['type'];
        $giftcode['used_count'] = (int)$giftcode['used_count'];
        $giftcode['remaining'] = max(0, $giftcode['limit_use'] - $giftcode['used_count']);
        
        // Status
        $currentTime = time();
        if ($giftcode['expire'] > 0 && $giftcode['expire'] <= $currentTime) {
            $giftcode['status'] = 'expired';
        } elseif ($giftcode['used_count'] >= $giftcode['limit_use']) {
            $giftcode['status'] = 'used_up';
        } else {
            $giftcode['status'] = 'active';
        }
        
        // Parse items
       if (!empty($giftcode['item'])) {
        $items = array_map('trim', explode(',', $giftcode['item']));
        $giftcode['items'] = array_filter($items);
    } else {
        $giftcode['items'] = [];
    }
        
        return $giftcode;
    }
    
    /**
     * Tạo giftcode mới
     */
public function createGiftcode($serverId, $data) {
    $db = $this->getGameDb($serverId);
    
    // Validate
    $errors = $this->validateGiftcodeData($data);
    if (!empty($errors)) {
        return ['success' => false, 'errors' => $errors];
    }
    
    // Check if giftcode exists
    $stmt = $db->prepare("SELECT id FROM giftcode WHERE giftcode = ?");
    $stmt->execute([$data['giftcode']]);
    if ($stmt->fetch()) {
        return ['success' => false, 'message' => 'Mã giftcode đã tồn tại'];
    }
    
    try {
        // Prepare items - THAY ĐỔI: từ JSON sang comma-separated string
        $itemString = null;
        if (!empty($data['items']) && is_array($data['items'])) {
            // Chuyển array thành chuỗi phân tách bằng dấu phẩy
            $itemString = implode(',', $data['items']);
        }
        
        $sql = "INSERT INTO giftcode (giftcode, xu, luong, luongLock, item, expire, limit_use, type) 
                VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
        
        $stmt = $db->prepare($sql);
        $stmt->execute([
            $data['giftcode'],
            $data['xu'] ?? 0,
            $data['luong'] ?? 0,
            $data['luongLock'] ?? 0,
            $itemString,
            $data['expire'] ?? 0,
            $data['limit_use'] ?? 99999,
            $data['type'] ?? 0
        ]);
        
        $id = $db->lastInsertId();
        
        return [
            'success' => true,
            'giftcode' => $this->getGiftcodeDetail($serverId, $id)
        ];
    } catch (PDOException $e) {
        return ['success' => false, 'message' => 'Tạo giftcode thất bại: ' . $e->getMessage()];
    }
}
    
    /**
     * Cập nhật giftcode
     */
    public function updateGiftcode($serverId, $id, $data) {
    $db = $this->getGameDb($serverId);
    
    // Check if exists
    $giftcode = $this->getGiftcodeDetail($serverId, $id);
    if (!$giftcode) {
        return ['success' => false, 'message' => 'Giftcode không tồn tại'];
    }
    
    try {
        $updateFields = [];
        $params = [];
        
        if (isset($data['xu'])) {
            $updateFields[] = "xu = ?";
            $params[] = $data['xu'];
        }
        if (isset($data['luong'])) {
            $updateFields[] = "luong = ?";
            $params[] = $data['luong'];
        }
        if (isset($data['luongLock'])) {
            $updateFields[] = "luongLock = ?";
            $params[] = $data['luongLock'];
        }
        if (isset($data['items']) && is_array($data['items'])) {
            // THAY ĐỔI: chuyển từ array sang comma-separated string
            $updateFields[] = "item = ?";
            $params[] = implode(',', $data['items']);
        }
        if (isset($data['expire'])) {
            $updateFields[] = "expire = ?";
            $params[] = $data['expire'];
        }
        if (isset($data['limit_use'])) {
            $updateFields[] = "limit_use = ?";
            $params[] = $data['limit_use'];
        }
        if (isset($data['type'])) {
            $updateFields[] = "type = ?";
            $params[] = $data['type'];
        }
        
        if (empty($updateFields)) {
            return ['success' => false, 'message' => 'Không có dữ liệu để cập nhật'];
        }
        
        $params[] = $id;
        
        $sql = "UPDATE giftcode SET " . implode(', ', $updateFields) . " WHERE id = ?";
        $stmt = $db->prepare($sql);
        $stmt->execute($params);
        
        return [
            'success' => true,
            'giftcode' => $this->getGiftcodeDetail($serverId, $id)
        ];
    } catch (PDOException $e) {
        return ['success' => false, 'message' => 'Cập nhật thất bại: ' . $e->getMessage()];
    }
}
    
    /**
     * Xóa giftcode
     */
    public function deleteGiftcode($serverId, $id) {
        $db = $this->getGameDb($serverId);
        
        try {
            // Xóa logs trước
            $stmt = $db->prepare("DELETE FROM giftcode_log WHERE giftcode = (SELECT giftcode FROM giftcode WHERE id = ?)");
            $stmt->execute([$id]);
            
            // Xóa giftcode
            $stmt = $db->prepare("DELETE FROM giftcode WHERE id = ?");
            $stmt->execute([$id]);
            
            if ($stmt->rowCount() === 0) {
                return ['success' => false, 'message' => 'Giftcode không tồn tại'];
            }
            
            return ['success' => true];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Xóa thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Lấy lịch sử sử dụng giftcode
     */
    public function getGiftcodeUsageLog($serverId, $giftcodeId, $page, $limit) {
        $db = $this->getGameDb($serverId);
        $offset = ($page - 1) * $limit;
        
        // Get giftcode code
        $stmt = $db->prepare("SELECT giftcode FROM giftcode WHERE id = ?");
        $stmt->execute([$giftcodeId]);
        $giftcodeData = $stmt->fetch();
        
        if (!$giftcodeData) {
            return null;
        }
        
        $giftcode = $giftcodeData['giftcode'];
        
        // Count
        $stmt = $db->prepare("SELECT COUNT(*) as total FROM giftcode_log WHERE giftcode = ?");
        $stmt->execute([$giftcode]);
        $total = $stmt->fetch()['total'];
        
        // Get logs
        $sql = "SELECT * FROM giftcode_log 
                WHERE giftcode = ? 
                ORDER BY id DESC 
                LIMIT $limit OFFSET $offset";
        
        $stmt = $db->prepare($sql);
        $stmt->execute([$giftcode]);
        $logs = $stmt->fetchAll();
        
        // Format
        foreach ($logs as &$log) {
        $log['id'] = (int)$log['id'];
        $log['xu'] = (int)$log['xu'];
        $log['luong'] = (int)$log['luong'];
        $log['luongK'] = (int)$log['luongK'];
        $log['id_user'] = (int)$log['id_user'];
        $log['type'] = (int)$log['type'];
        
        // THAY ĐỔI: parse items từ comma-separated string
        if (!empty($log['item'])) {
            $items = array_map('trim', explode(',', $log['item']));
            $log['items'] = array_filter($items);
        } else {
            $log['items'] = [];
        }
    }
        
        return [
            'server_id' => (int)$serverId,
            'giftcode_id' => (int)$giftcodeId,
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
     * Thống kê giftcode
     */
    public function getGiftcodeStats($serverId) {
        $db = $this->getGameDb($serverId);
        $currentTime = time();
        
        // Total giftcodes
        $stmt = $db->query("SELECT COUNT(*) as total FROM giftcode");
        $totalGiftcodes = $stmt->fetch()['total'];
        
        // Active giftcodes
        $stmt = $db->prepare("
            SELECT COUNT(*) as total 
            FROM giftcode g
            LEFT JOIN (
                SELECT giftcode, COUNT(*) as used 
                FROM giftcode_log 
                GROUP BY giftcode
            ) gl ON g.giftcode = gl.giftcode
            WHERE (g.expire = 0 OR g.expire > ?)
            AND (gl.used IS NULL OR gl.used < g.limit_use)
        ");
        $stmt->execute([$currentTime]);
        $activeGiftcodes = $stmt->fetch()['total'];
        
        // Expired giftcodes
        $stmt = $db->prepare("SELECT COUNT(*) as total FROM giftcode WHERE expire > 0 AND expire <= ?");
        $stmt->execute([$currentTime]);
        $expiredGiftcodes = $stmt->fetch()['total'];
        
        // Total usage
        $stmt = $db->query("SELECT COUNT(*) as total FROM giftcode_log");
        $totalUsage = $stmt->fetch()['total'];
        
        // By type
        $stmt = $db->query("
            SELECT type, COUNT(*) as count 
            FROM giftcode 
            GROUP BY type
            ORDER BY type
        ");
        $byType = $stmt->fetchAll();
        
        // Most used
        $stmt = $db->query("
            SELECT g.giftcode, g.id, COUNT(gl.id) as usage_count
            FROM giftcode g
            LEFT JOIN giftcode_log gl ON g.giftcode = gl.giftcode
            GROUP BY g.id
            ORDER BY usage_count DESC
            LIMIT 10
        ");
        $mostUsed = $stmt->fetchAll();
        
        // Recent usage (last 7 days)
        $stmt = $db->query("
            SELECT DATE(FROM_UNIXTIME(UNIX_TIMESTAMP())) as date, COUNT(*) as count
            FROM giftcode_log
            GROUP BY date
            ORDER BY date DESC
            LIMIT 7
        ");
        $recentUsage = $stmt->fetchAll();
        
        return [
            'server_id' => (int)$serverId,
            'total_giftcodes' => (int)$totalGiftcodes,
            'active_giftcodes' => (int)$activeGiftcodes,
            'expired_giftcodes' => (int)$expiredGiftcodes,
            'total_usage' => (int)$totalUsage,
            'by_type' => $byType,
            'most_used' => $mostUsed,
            'recent_usage' => $recentUsage
        ];
    }
    
    /**
     * Generate random giftcode
     */
    public function generateRandomCode($serverId, $length = 10, $prefix = '') {
        $db = $this->getGameDb($serverId);
        $characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
        $code = $prefix;
        
        for ($i = 0; $i < $length; $i++) {
            $code .= $characters[rand(0, strlen($characters) - 1)];
        }
        
        // Check if exists
        $stmt = $db->prepare("SELECT id FROM giftcode WHERE giftcode = ?");
        $stmt->execute([$code]);
        
        if ($stmt->fetch()) {
            // Generate again if exists
            return $this->generateRandomCode($serverId, $length, $prefix);
        }
        
        return $code;
    }
    
    /**
     * Batch create giftcodes
     */
    public function batchCreateGiftcodes($serverId, $count, $data) {
    $db = $this->getGameDb($serverId);
    $created = [];
    $failed = 0;
    
    try {
        $db->beginTransaction();
        
        // THAY ĐỔI: chuẩn bị item string trước khi loop
        $itemString = null;
        if (!empty($data['items']) && is_array($data['items'])) {
            $itemString = implode(',', $data['items']);
        }
        
        for ($i = 0; $i < $count; $i++) {
            $code = $this->generateRandomCode($serverId, $data['code_length'] ?? 10, $data['prefix'] ?? '');
            
            $sql = "INSERT INTO giftcode (giftcode, xu, luong, luongLock, item, expire, limit_use, type) 
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
            
            $stmt = $db->prepare($sql);
            $stmt->execute([
                $code,
                $data['xu'] ?? 0,
                $data['luong'] ?? 0,
                $data['luongLock'] ?? 0,
                $itemString,
                $data['expire'] ?? 0,
                $data['limit_use'] ?? 1,
                $data['type'] ?? 0
            ]);
            
            $created[] = $code;
        }
        
        $db->commit();
        
        return [
            'success' => true,
            'created_count' => count($created),
            'giftcodes' => $created
        ];
    } catch (Exception $e) {
        $db->rollBack();
        return ['success' => false, 'message' => 'Tạo hàng loạt thất bại: ' . $e->getMessage()];
    }
}
    /**
     * Export giftcodes to CSV
     */
    public function exportGiftcodes($serverId, $search, $type, $status) {
        $db = $this->getGameDb($serverId);
        
        $where = "1=1";
        $params = [];
        
        if (!empty($search)) {
            $where .= " AND (g.giftcode LIKE ?)";
            $params[] = "%$search%";
        }
        
        if ($type !== 'all' && is_numeric($type)) {
            $where .= " AND g.type = ?";
            $params[] = $type;
        }
        
        $currentTime = time();
        if ($status === 'active') {
            $where .= " AND (g.expire = 0 OR g.expire > ?)";
            $params[] = $currentTime;
        } elseif ($status === 'expired') {
            $where .= " AND g.expire > 0 AND g.expire <= ?";
            $params[] = $currentTime;
        }
        
        $sql = "SELECT g.*, COUNT(gl.id) as used_count
                FROM giftcode g
                LEFT JOIN giftcode_log gl ON g.giftcode = gl.giftcode
                WHERE $where 
                GROUP BY g.id
                ORDER BY g.id ASC";
        
        $stmt = $db->prepare($sql);
        $stmt->execute($params);
        $giftcodes = $stmt->fetchAll();
        
        // Create CSV
        $output = fopen('php://temp', 'r+');
        
        // Header
        fputcsv($output, ['ID', 'Giftcode', 'Xu', 'Lượng', 'Lượng Khóa', 'Items', 'Expire', 'Giới hạn', 'Đã dùng', 'Type']);
        
        // Data
        foreach ($giftcodes as $gc) {
            fputcsv($output, [
                $gc['id'],
                $gc['giftcode'],
                $gc['xu'],
                $gc['luong'],
                $gc['luongLock'],
                $gc['item'],
                $gc['expire'],
                $gc['limit_use'],
                $gc['used_count'],
                $gc['type']
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
    public function bulkDelete($serverId, $giftcodeIds) {
        $db = $this->getGameDb($serverId);
        
        if (empty($giftcodeIds) || !is_array($giftcodeIds)) {
            return ['success' => false, 'message' => 'Danh sách ID không hợp lệ'];
        }
        
        try {
            $db->beginTransaction();
            
            // Delete logs first
            $placeholders = implode(',', array_fill(0, count($giftcodeIds), '?'));
            $sql = "DELETE FROM giftcode_log WHERE giftcode IN (SELECT giftcode FROM giftcode WHERE id IN ($placeholders))";
            $stmt = $db->prepare($sql);
            $stmt->execute($giftcodeIds);
            
            // Delete giftcodes
            $sql = "DELETE FROM giftcode WHERE id IN ($placeholders)";
            $stmt = $db->prepare($sql);
            $stmt->execute($giftcodeIds);
            
            $deletedCount = $stmt->rowCount();
            
            $db->commit();
            
            return [
                'success' => true,
                'deleted_count' => $deletedCount
            ];
        } catch (PDOException $e) {
            $db->rollBack();
            return ['success' => false, 'message' => 'Xóa thất bại: ' . $e->getMessage()];
        }
    }
    
    // Helper methods
    
    private function validateGiftcodeData($data) {
        $errors = [];
        
        if (empty($data['giftcode'])) {
            $errors['giftcode'] = 'Mã giftcode không được để trống';
        }
        
        if (!isset($data['xu']) && !isset($data['luong']) && !isset($data['luongLock']) && empty($data['items'])) {
            $errors['rewards'] = 'Giftcode phải có ít nhất một phần thưởng';
        }
        
        return $errors;
    }
}