<?php
// controllers/admin/AdminServerController.php

class AdminServerController {
    private $serverService;
    private $authService;
    
    public function __construct() {
        $this->serverService = new AdminServerService();
        $this->authService = new AuthService();
    }
    
    /**
     * Lấy danh sách servers
     * GET /admin/servers?page=1&limit=20&search=&status=all
     */
    public function getServers() {
        $this->requireAdmin();
        
        $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
        $limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 20;
        $search = $_GET['search'] ?? '';
        $status = $_GET['status'] ?? 'all';
        $sortBy = $_GET['sort_by'] ?? 'created_at';
        $sortOrder = $_GET['sort_order'] ?? 'DESC';
        
        $result = $this->serverService->getServers($page, $limit, $search, $status, $sortBy, $sortOrder);
        
        Response::success($result, 'Lấy danh sách servers thành công');
    }
    
    /**
     * Lấy chi tiết server
     * GET /admin/servers/{id}
     */
    public function getServerDetail($serverId) {
        $this->requireAdmin();
        
        if (empty($serverId) || !is_numeric($serverId)) {
            Response::error('ID server không hợp lệ', 400);
        }
        
        $result = $this->serverService->getServerDetail($serverId);
        
        if (!$result) {
            Response::notFound('Không tìm thấy server');
        }
        
        Response::success($result, 'Lấy thông tin server thành công');
    }
    
    /**
     * Tạo server mới
     * POST /admin/servers
     */
    public function createServer() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $result = $this->serverService->createServer($input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['server'], 'Tạo server thành công', 201);
    }
    
    /**
     * Cập nhật server
     * PUT /admin/servers/{id}
     */
    public function updateServer($serverId) {
        $this->requireAdmin();
        
        if (empty($serverId) || !is_numeric($serverId)) {
            Response::error('ID server không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $result = $this->serverService->updateServer($serverId, $input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['server'], 'Cập nhật server thành công');
    }
    
    /**
     * Xóa server
     * DELETE /admin/servers/{id}
     */
    public function deleteServer($serverId) {
        $this->requireAdmin();
        
        if (empty($serverId) || !is_numeric($serverId)) {
            Response::error('ID server không hợp lệ', 400);
        }
        
        $result = $this->serverService->deleteServer($serverId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success(null, 'Xóa server thành công');
    }
    
    /**
     * Toggle status server
     * POST /admin/servers/{id}/toggle-status
     */
    public function toggleStatus($serverId) {
        $this->requireAdmin();
        
        if (empty($serverId) || !is_numeric($serverId)) {
            Response::error('ID server không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        $status = isset($input['status']) ? intval($input['status']) : 1;
        
        $result = $this->serverService->toggleStatus($serverId, $status);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['server'], 'Cập nhật trạng thái thành công');
    }
    
    /**
     * Test kết nối database
     * POST /admin/servers/{id}/test-connection
     */
    public function testConnection($serverId) {
        $this->requireAdmin();
        
        if (empty($serverId) || !is_numeric($serverId)) {
            Response::error('ID server không hợp lệ', 400);
        }
        
        $result = $this->serverService->testConnection($serverId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success(null, $result['message']);
    }
    
    // Helper methods
    private function requireAdmin() {
        $token = $this->getBearerToken();
        
        if (!$token) {
            Response::unauthorized('Yêu cầu mã truy cập');
        }
        
        $userId = $this->authService->validateAccessToken($token);
        
        if (!$userId) {
            Response::unauthorized('Mã không hợp lệ hoặc đã hết hạn');
        }
        
        if (!AdminMiddleware::isAdmin($userId)) {
            Response::forbidden('Bạn không có quyền truy cập');
        }
        
        return $userId;
    }
    
    private function getBearerToken() {
        $headers = getallheaders();
        
        if (isset($headers['Authorization'])) {
            $matches = [];
            if (preg_match('/Bearer\s+(.*)$/i', $headers['Authorization'], $matches)) {
                return $matches[1];
            }
        }
        
        return null;
    }
}