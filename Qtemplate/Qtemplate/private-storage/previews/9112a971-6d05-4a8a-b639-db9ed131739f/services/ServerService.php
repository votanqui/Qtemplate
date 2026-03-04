<?php
require_once __DIR__ . '/../config/Database.php';

class ServerService {
    private $db;
    
    public function __construct() {
        $this->db = Database::getInstance()->getConnection();
    }
    
    /**
     * Lấy danh sách servers đang hoạt động (status = 1)
     * Chỉ lấy server_id và server_name
     */
    public function getActiveServers() {
        try {
            $stmt = $this->db->prepare("
                SELECT server_id, server_name 
                FROM servers 
                WHERE status = 1
                ORDER BY server_id ASC
            ");
            
            $stmt->execute();
            $servers = $stmt->fetchAll(PDO::FETCH_ASSOC);
            
            return [
                'success' => true,
                'total' => count($servers),
                'servers' => $servers
            ];
            
        } catch (PDOException $e) {
            error_log("Database error in getActiveServers: " . $e->getMessage());
            
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy danh sách servers'
            ];
        }
    }
    
    /**
     * Lấy thông tin chi tiết 1 server theo ID
     * Chỉ lấy server đang hoạt động
     */
    public function getServerById($serverId) {
        try {
            $stmt = $this->db->prepare("
                SELECT server_id, server_name, status
                FROM servers 
                WHERE server_id = ? AND status = 1
            ");
            
            $stmt->execute([$serverId]);
            $server = $stmt->fetch(PDO::FETCH_ASSOC);
            
            if (!$server) {
                return [
                    'success' => false,
                    'message' => 'Không tìm thấy server hoặc server đã bị tắt'
                ];
            }
            
            return [
                'success' => true,
                'server' => $server
            ];
            
        } catch (PDOException $e) {
            error_log("Database error in getServerById: " . $e->getMessage());
            
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy thông tin server'
            ];
        }
    }
    
    /**
     * Kiểm tra server có tồn tại và đang hoạt động không
     * Hàm helper cho các service khác
     */
    public function isServerActive($serverId) {
        try {
            $stmt = $this->db->prepare("
                SELECT COUNT(*) as count 
                FROM servers 
                WHERE server_id = ? AND status = 1
            ");
            
            $stmt->execute([$serverId]);
            $result = $stmt->fetch(PDO::FETCH_ASSOC);
            
            return $result['count'] > 0;
            
        } catch (PDOException $e) {
            error_log("Database error in isServerActive: " . $e->getMessage());
            return false;
        }
    }
    
    /**
     * Lấy tổng số servers đang hoạt động
     */
    public function getTotalActiveServers() {
        try {
            $stmt = $this->db->prepare("
                SELECT COUNT(*) as total 
                FROM servers 
                WHERE status = 1
            ");
            
            $stmt->execute();
            $result = $stmt->fetch(PDO::FETCH_ASSOC);
            
            return [
                'success' => true,
                'total' => (int)$result['total']
            ];
            
        } catch (PDOException $e) {
            error_log("Database error in getTotalActiveServers: " . $e->getMessage());
            
            return [
                'success' => false,
                'total' => 0
            ];
        }
    }
    
    /**
     * Lấy thông tin server kèm database config
     * Hàm này CHỈ dùng cho internal service, KHÔNG public ra API
     */
    public function getServerFullInfo($serverId) {
        try {
            $stmt = $this->db->prepare("
                SELECT * 
                FROM servers 
                WHERE server_id = ? AND status = 1
            ");
            
            $stmt->execute([$serverId]);
            $server = $stmt->fetch(PDO::FETCH_ASSOC);
            
            if (!$server) {
                return [
                    'success' => false,
                    'message' => 'Server không tồn tại'
                ];
            }
            
            return [
                'success' => true,
                'server' => $server
            ];
            
        } catch (PDOException $e) {
            error_log("Database error in getServerFullInfo: " . $e->getMessage());
            
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy thông tin server'
            ];
        }
    }
}