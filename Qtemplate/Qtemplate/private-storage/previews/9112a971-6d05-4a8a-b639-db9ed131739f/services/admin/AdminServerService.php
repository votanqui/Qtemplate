<?php
// services/admin/AdminServerService.php

class AdminServerService {
    private $db;
    
    public function __construct() {
        $this->db = Database::getInstance()->getConnection();
    }
    
    /**
     * Lấy danh sách servers với phân trang và tìm kiếm
     */
    public function getServers($page, $limit, $search, $status, $sortBy, $sortOrder) {
        $offset = ($page - 1) * $limit;
        
        $where = "1=1";
        $params = [];
        
        // Search filter
        if (!empty($search)) {
            $where .= " AND (server_name LIKE ? OR db_name LIKE ? OR server_id LIKE ?)";
            $searchTerm = "%$search%";
            $params[] = $searchTerm;
            $params[] = $searchTerm;
            $params[] = $searchTerm;
        }
        
        // Status filter
        if ($status === 'active') {
            $where .= " AND status = 1";
        } elseif ($status === 'inactive') {
            $where .= " AND status = 0";
        }
        
        // Validate sort
        $allowedSort = ['id', 'server_id', 'server_name', 'status', 'created_at'];
        $sortBy = in_array($sortBy, $allowedSort) ? $sortBy : 'created_at';
        $sortOrder = strtoupper($sortOrder) === 'ASC' ? 'ASC' : 'DESC';
        
        // Get total count
        $countSql = "SELECT COUNT(*) as total FROM servers WHERE $where";
        $stmt = $this->db->prepare($countSql);
        $stmt->execute($params);
        $total = $stmt->fetch()['total'];
        
        // Get servers
        $sql = "SELECT 
                    id, server_id, server_name, db_name, db_host, 
                    db_user, db_port, status, created_at
                FROM servers 
                WHERE $where 
                ORDER BY $sortBy $sortOrder 
                LIMIT $limit OFFSET $offset";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute($params);
        $servers = $stmt->fetchAll();
        
        return [
            'servers' => $servers,
            'pagination' => [
                'total' => (int)$total,
                'page' => $page,
                'limit' => $limit,
                'total_pages' => ceil($total / $limit)
            ]
        ];
    }
    
    /**
     * Lấy thông tin chi tiết server
     */
    public function getServerDetail($serverId) {
        $sql = "SELECT * FROM servers WHERE id = ?";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$serverId]);
        $server = $stmt->fetch();
        
        if (!$server) {
            return null;
        }
        
        // Mask password for security
        $server['db_pass'] = '********';
        
        return $server;
    }
    
    /**
     * Tạo server mới
     */
    public function createServer($data) {
        // Validate required fields
        $required = ['server_id', 'server_name', 'db_name', 'db_user', 'db_pass'];
        foreach ($required as $field) {
            if (empty($data[$field])) {
                return ['success' => false, 'message' => "Trường $field là bắt buộc"];
            }
        }
        
        // Check duplicate server_id
        $stmt = $this->db->prepare("SELECT COUNT(*) as count FROM servers WHERE server_id = ?");
        $stmt->execute([$data['server_id']]);
        if ($stmt->fetch()['count'] > 0) {
            return ['success' => false, 'message' => 'Server ID đã tồn tại'];
        }
        
        $sql = "INSERT INTO servers 
                (server_id, server_name, db_name, db_host, db_user, db_pass, db_port, status) 
                VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
        
        try {
            $stmt = $this->db->prepare($sql);
            $stmt->execute([
                $data['server_id'],
                $data['server_name'],
                $data['db_name'],
                $data['db_host'] ?? 'localhost',
                $data['db_user'],
                $data['db_pass'],
                $data['db_port'] ?? 3306,
                $data['status'] ?? 1
            ]);
            
            $serverId = $this->db->lastInsertId();
            
            return [
                'success' => true,
                'server' => $this->getServerDetail($serverId)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Tạo server thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Cập nhật thông tin server
     */
    public function updateServer($serverId, $data) {
        $allowedFields = ['server_id', 'server_name', 'db_name', 'db_host', 'db_user', 'db_pass', 'db_port', 'status'];
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
        
        $params[] = $serverId;
        
        $sql = "UPDATE servers SET " . implode(', ', $updateFields) . " WHERE id = ?";
        
        try {
            $stmt = $this->db->prepare($sql);
            $stmt->execute($params);
            
            return [
                'success' => true,
                'server' => $this->getServerDetail($serverId)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Cập nhật thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Xóa server
     */
    public function deleteServer($serverId) {
        try {
            $sql = "DELETE FROM servers WHERE id = ?";
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$serverId]);
            
            return ['success' => true];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Xóa thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Toggle status server
     */
    public function toggleStatus($serverId, $status) {
        try {
            $sql = "UPDATE servers SET status = ? WHERE id = ?";
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$status, $serverId]);
            
            return [
                'success' => true,
                'server' => $this->getServerDetail($serverId)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Cập nhật trạng thái thất bại'];
        }
    }
    
    /**
     * Test kết nối database
     */
    public function testConnection($serverId) {
        $server = $this->getServerDetail($serverId);
        
        if (!$server) {
            return ['success' => false, 'message' => 'Server không tồn tại'];
        }
        
        // Get real password
        $stmt = $this->db->prepare("SELECT db_pass FROM servers WHERE id = ?");
        $stmt->execute([$serverId]);
        $dbPass = $stmt->fetch()['db_pass'];
        
        try {
            $testDb = new PDO(
                "mysql:host={$server['db_host']};port={$server['db_port']};dbname={$server['db_name']}",
                $server['db_user'],
                $dbPass,
                [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]
            );
            
            return ['success' => true, 'message' => 'Kết nối thành công'];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Kết nối thất bại: ' . $e->getMessage()];
        }
    }
}