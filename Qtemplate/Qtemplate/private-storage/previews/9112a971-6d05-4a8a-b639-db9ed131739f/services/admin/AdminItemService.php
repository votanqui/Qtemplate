<?php
// services/admin/AdminItemService.php

class AdminItemService {
    
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
     * Lấy danh sách items với phân trang và lọc
     */
    public function getItems($serverId, $page, $limit, $search, $type, $he, $gender, $level, $sortBy, $sortOrder) {
        $db = $this->getGameDb($serverId);
        $offset = ($page - 1) * $limit;
        
        // Build query
        $where = "1=1";
        $params = [];
        
        // Search filter
        if (!empty($search)) {
            $where .= " AND (name LIKE ? OR namein LIKE ? OR id = ?)";
            $searchTerm = "%$search%";
            $params[] = $searchTerm;
            $params[] = $searchTerm;
            $params[] = is_numeric($search) ? $search : 0;
        }
        
        // Type filter
        if ($type !== 'all' && is_numeric($type)) {
            $where .= " AND type = ?";
            $params[] = $type;
        }
        
        // He filter
        if ($he !== 'all' && is_numeric($he)) {
            $where .= " AND he = ?";
            $params[] = $he;
        }
        
        // Gender filter
        if ($gender !== 'all' && is_numeric($gender)) {
            $where .= " AND gender = ?";
            $params[] = $gender;
        }
        
        // Level filter
        if ($level !== 'all' && is_numeric($level)) {
            $where .= " AND level = ?";
            $params[] = $level;
        }
        
        // Validate sort
        $allowedSort = ['id', 'name', 'type', 'level', 'price', 'pricecu'];
        $sortBy = in_array($sortBy, $allowedSort) ? $sortBy : 'id';
        $sortOrder = strtoupper($sortOrder) === 'DESC' ? 'DESC' : 'ASC';
        
        // Get total count
        $countSql = "SELECT COUNT(*) as total FROM tob_item_templet WHERE $where";
        $stmt = $db->prepare($countSql);
        $stmt->execute($params);
        $total = $stmt->fetch()['total'];
        
        // Get items
        $sql = "SELECT * FROM tob_item_templet 
                WHERE $where 
                ORDER BY $sortBy $sortOrder 
                LIMIT $limit OFFSET $offset";
        
        $stmt = $db->prepare($sql);
        $stmt->execute($params);
        $items = $stmt->fetchAll();
        
        return [
            'server_id' => (int)$serverId,
            'items' => $items,
            'pagination' => [
                'total' => (int)$total,
                'page' => $page,
                'limit' => $limit,
                'total_pages' => ceil($total / $limit)
            ]
        ];
    }
    
    /**
     * Lấy thông tin chi tiết item
     */
    public function getItemDetail($serverId, $itemId) {
        $db = $this->getGameDb($serverId);
        
        $sql = "SELECT * FROM tob_item_templet WHERE id = ?";
        
        $stmt = $db->prepare($sql);
        $stmt->execute([$itemId]);
        $item = $stmt->fetch();
        
        if ($item) {
            $item['server_id'] = (int)$serverId;
        }
        
        return $item ?: null;
    }
    
    /**
     * Tạo item mới
     */
    public function createItem($serverId, $data) {
        $db = $this->getGameDb($serverId);
        
        // Validate required fields
        $errors = $this->validateItemData($data);
        if (!empty($errors)) {
            return ['success' => false, 'errors' => $errors];
        }
        
        // Check if ID already exists
        if (isset($data['id'])) {
            $stmt = $db->prepare("SELECT id FROM tob_item_templet WHERE id = ?");
            $stmt->execute([$data['id']]);
            if ($stmt->fetch()) {
                return ['success' => false, 'message' => 'ID item đã tồn tại'];
            }
        } else {
            // Auto generate ID
            $stmt = $db->query("SELECT MAX(id) as max_id FROM tob_item_templet");
            $data['id'] = $stmt->fetch()['max_id'] + 1;
        }
        
        try {
            $fields = $this->getItemFields();
            $columns = [];
            $placeholders = [];
            $values = [];
            
            foreach ($fields as $field) {
                $columns[] = $field;
                $placeholders[] = '?';
                $values[] = $data[$field] ?? $this->getDefaultValue($field);
            }
            
            $sql = "INSERT INTO tob_item_templet (" . implode(', ', $columns) . ") 
                    VALUES (" . implode(', ', $placeholders) . ")";
            
            $stmt = $db->prepare($sql);
            $stmt->execute($values);
            
            return [
                'success' => true,
                'item' => $this->getItemDetail($serverId, $data['id'])
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Tạo item thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Cập nhật item
     */
    public function updateItem($serverId, $itemId, $data) {
        $db = $this->getGameDb($serverId);
        
        // Check if item exists
        $item = $this->getItemDetail($serverId, $itemId);
        if (!$item) {
            return ['success' => false, 'message' => 'Item không tồn tại'];
        }
        
        $allowedFields = $this->getItemFields();
        $updateFields = [];
        $params = [];
        
        foreach ($allowedFields as $field) {
            if (isset($data[$field]) && $field !== 'id') {
                $updateFields[] = "$field = ?";
                $params[] = $data[$field];
            }
        }
        
        if (empty($updateFields)) {
            return ['success' => false, 'message' => 'Không có dữ liệu để cập nhật'];
        }
        
        $params[] = $itemId;
        
        $sql = "UPDATE tob_item_templet SET " . implode(', ', $updateFields) . " WHERE id = ?";
        
        try {
            $stmt = $db->prepare($sql);
            $stmt->execute($params);
            
            return [
                'success' => true,
                'item' => $this->getItemDetail($serverId, $itemId)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Cập nhật thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Xóa item
     */
    public function deleteItem($serverId, $itemId) {
        $db = $this->getGameDb($serverId);
        
        try {
            $sql = "DELETE FROM tob_item_templet WHERE id = ?";
            $stmt = $db->prepare($sql);
            $stmt->execute([$itemId]);
            
            if ($stmt->rowCount() === 0) {
                return ['success' => false, 'message' => 'Item không tồn tại'];
            }
            
            return ['success' => true];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Xóa thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Sao chép item
     */
    public function cloneItem($serverId, $itemId) {
        $db = $this->getGameDb($serverId);
        
        $item = $this->getItemDetail($serverId, $itemId);
        
        if (!$item) {
            return ['success' => false, 'message' => 'Item không tồn tại'];
        }
        
        // Generate new ID
        $stmt = $db->query("SELECT MAX(id) as max_id FROM tob_item_templet");
        $newId = $stmt->fetch()['max_id'] + 1;
        
        $item['id'] = $newId;
        $item['name'] = $item['name'] . ' (Copy)';
        unset($item['server_id']); // Remove server_id from item data
        
        return $this->createItem($serverId, $item);
    }
    
    /**
     * Thống kê items
     */
    public function getItemStats($serverId) {
        $db = $this->getGameDb($serverId);
        
        // Total items
        $stmt = $db->query("SELECT COUNT(*) as total FROM tob_item_templet");
        $totalItems = $stmt->fetch()['total'];
        
        // By type
        $stmt = $db->query("
            SELECT type, COUNT(*) as count 
            FROM tob_item_templet 
            GROUP BY type
            ORDER BY type
        ");
        $byType = $stmt->fetchAll();
        
        // By he (faction)
        $stmt = $db->query("
            SELECT he, COUNT(*) as count 
            FROM tob_item_templet 
            GROUP BY he
            ORDER BY he
        ");
        $byHe = $stmt->fetchAll();
        
        // By level range
        $stmt = $db->query("
            SELECT 
                CASE 
                    WHEN level BETWEEN 0 AND 10 THEN '0-10'
                    WHEN level BETWEEN 11 AND 20 THEN '11-20'
                    WHEN level BETWEEN 21 AND 30 THEN '21-30'
                    WHEN level BETWEEN 31 AND 40 THEN '31-40'
                    WHEN level > 40 THEN '40+'
                END as level_range,
                COUNT(*) as count
            FROM tob_item_templet
            GROUP BY level_range
            ORDER BY MIN(level)
        ");
        $byLevel = $stmt->fetchAll();
        
        // Price statistics
        $stmt = $db->query("
            SELECT 
                MIN(price) as min_price,
                MAX(price) as max_price,
                AVG(price) as avg_price,
                MIN(pricecu) as min_pricecu,
                MAX(pricecu) as max_pricecu,
                AVG(pricecu) as avg_pricecu
            FROM tob_item_templet
        ");
        $priceStats = $stmt->fetch();
        
        return [
            'server_id' => (int)$serverId,
            'total_items' => (int)$totalItems,
            'by_type' => $byType,
            'by_faction' => $byHe,
            'by_level_range' => $byLevel,
            'price_statistics' => $priceStats
        ];
    }
    
    /**
     * Lấy danh sách loại item
     */
    public function getItemTypes() {
        $types = [
            0 => 'Áo',
            1 => 'Quần',
            2 => 'Giày',
            3 => 'Găng tay',
            4 => 'Vũ khí',
            5 => 'Phụ kiện',
            6 => 'Vật phẩm',
            7 => 'Đá',
            8 => 'Pet'
        ];
        
        return $types;
    }
    
    /**
     * Cập nhật hàng loạt
     */
    public function bulkUpdate($serverId, $itemIds, $updates) {
        $db = $this->getGameDb($serverId);
        
        if (empty($itemIds) || !is_array($itemIds)) {
            return ['success' => false, 'message' => 'Danh sách ID không hợp lệ'];
        }
        
        $allowedFields = $this->getItemFields();
        $updateFields = [];
        $params = [];
        
        foreach ($updates as $field => $value) {
            if (in_array($field, $allowedFields) && $field !== 'id') {
                $updateFields[] = "$field = ?";
                $params[] = $value;
            }
        }
        
        if (empty($updateFields)) {
            return ['success' => false, 'message' => 'Không có trường hợp lệ để cập nhật'];
        }
        
        $placeholders = implode(',', array_fill(0, count($itemIds), '?'));
        $params = array_merge($params, $itemIds);
        
        $sql = "UPDATE tob_item_templet 
                SET " . implode(', ', $updateFields) . " 
                WHERE id IN ($placeholders)";
        
        try {
            $stmt = $db->prepare($sql);
            $stmt->execute($params);
            
            return [
                'success' => true,
                'updated_count' => $stmt->rowCount()
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Cập nhật thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Xóa hàng loạt
     */
    public function bulkDelete($serverId, $itemIds) {
        $db = $this->getGameDb($serverId);
        
        if (empty($itemIds) || !is_array($itemIds)) {
            return ['success' => false, 'message' => 'Danh sách ID không hợp lệ'];
        }
        
        $placeholders = implode(',', array_fill(0, count($itemIds), '?'));
        $sql = "DELETE FROM tob_item_templet WHERE id IN ($placeholders)";
        
        try {
            $stmt = $db->prepare($sql);
            $stmt->execute($itemIds);
            
            return [
                'success' => true,
                'deleted_count' => $stmt->rowCount()
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Xóa thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Export items to CSV
     */
    public function exportItems($serverId, $search, $type, $he) {
        $db = $this->getGameDb($serverId);
        
        $where = "1=1";
        $params = [];
        
        if (!empty($search)) {
            $where .= " AND (name LIKE ? OR namein LIKE ?)";
            $searchTerm = "%$search%";
            $params[] = $searchTerm;
            $params[] = $searchTerm;
        }
        
        if ($type !== 'all' && is_numeric($type)) {
            $where .= " AND type = ?";
            $params[] = $type;
        }
        
        if ($he !== 'all' && is_numeric($he)) {
            $where .= " AND he = ?";
            $params[] = $he;
        }
        
        $sql = "SELECT * FROM tob_item_templet WHERE $where ORDER BY id ASC";
        
        $stmt = $db->prepare($sql);
        $stmt->execute($params);
        $items = $stmt->fetchAll();
        
        // Create CSV
        $output = fopen('php://temp', 'r+');
        
        // Header
        fputcsv($output, $this->getItemFields());
        
        // Data
        foreach ($items as $item) {
            fputcsv($output, array_values($item));
        }
        
        rewind($output);
        $csv = stream_get_contents($output);
        fclose($output);
        
        return $csv;
    }
    
    /**
     * Import items from CSV
     */
    public function importItems($serverId, $filePath) {
        $db = $this->getGameDb($serverId);
        
        $file = fopen($filePath, 'r');
        if (!$file) {
            return ['success' => false, 'message' => 'Không thể đọc file'];
        }
        
        $header = fgetcsv($file);
        $imported = 0;
        $failed = 0;
        $errors = [];
        
        while (($row = fgetcsv($file)) !== false) {
            $data = array_combine($header, $row);
            
            if (isset($data['id'])) {
                // Update if exists
                $existing = $this->getItemDetail($serverId, $data['id']);
                if ($existing) {
                    $result = $this->updateItem($serverId, $data['id'], $data);
                } else {
                    $result = $this->createItem($serverId, $data);
                }
                
                if ($result['success']) {
                    $imported++;
                } else {
                    $failed++;
                    $errors[] = "ID {$data['id']}: {$result['message']}";
                }
            }
        }
        
        fclose($file);
        
        return [
            'success' => true,
            'imported_count' => $imported,
            'failed_count' => $failed,
            'errors' => $errors
        ];
    }
    
    // Helper methods
    
    private function getItemFields() {
        return [
            'id', 'name', 'type', 'style', 'he', 'gender', 'level', 'durable',
            'atb0', 'atb1', 'atb2', 'atb3', 'atb4', 'atb5', 'atb6', 'atb7', 'atb8', 'atb9',
            'price', 'clazz', 'xstart', 'ystart', 'colortype', 'nloan', 
            'idUpLevel', 'namein', 'ideff', 'pricecu'
        ];
    }
    
    private function getDefaultValue($field) {
        $defaults = [
            'id' => 0,
            'name' => '',
            'type' => 0,
            'style' => 0,
            'he' => 0,
            'gender' => 0,
            'level' => 0,
            'durable' => 0,
            'atb0' => 0, 'atb1' => 0, 'atb2' => 0, 'atb3' => 0, 'atb4' => 0,
            'atb5' => 0, 'atb6' => 0, 'atb7' => 0, 'atb8' => 0, 'atb9' => 0,
            'price' => 0,
            'clazz' => -1,
            'xstart' => 0,
            'ystart' => 0,
            'colortype' => -1,
            'nloan' => 0,
            'idUpLevel' => -1,
            'namein' => null,
            'ideff' => -1,
            'pricecu' => 0
        ];
        
        return $defaults[$field] ?? null;
    }
    
    private function validateItemData($data) {
        $errors = [];
        
        if (empty($data['name'])) {
            $errors['name'] = 'Tên item không được để trống';
        }
        
        if (!isset($data['type']) || !is_numeric($data['type'])) {
            $errors['type'] = 'Loại item không hợp lệ';
        }
        
        return $errors;
    }
}