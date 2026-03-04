<?php
// controllers/admin/AdminConfigController.php

class AdminConfigController {
    private $configService;
    private $authService;
    
    public function __construct() {
        $this->configService = new ConfigAdminService();
        $this->authService = new AuthService();
    }
    
    /**
     * Lấy tất cả cấu hình
     * GET /admin/configs?category=all
     */
    public function getAllConfigs() {
        $this->requireAdmin();
        
        $category = $_GET['category'] ?? 'all';
        
        $result = $this->configService->getAllConfigs($category);
        
        Response::success($result, 'Lấy danh sách cấu hình thành công');
    }
    
    /**
     * Lấy chi tiết một cấu hình
     * GET /admin/configs/:key
     */
    public function getConfigByKey($key) {
        $this->requireAdmin();
        
        if (empty($key)) {
            Response::error('Config key không được để trống', 400);
        }
        
        $result = $this->configService->getConfigByKey($key);
        
        if (!$result) {
            Response::notFound('Không tìm thấy cấu hình');
        }
        
        Response::success($result, 'Lấy cấu hình thành công');
    }
    
    /**
     * Cập nhật cấu hình
     * PUT /admin/configs/:key
     */
    public function updateConfig($key) {
        $this->requireAdmin();
        
        if (empty($key)) {
            Response::error('Config key không được để trống', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $result = $this->configService->updateConfig($key, $input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['config'], 'Cập nhật cấu hình thành công');
    }
    
    /**
     * Reset về cấu hình mặc định
     * POST /admin/configs/reset
     */
    public function resetToDefault() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        $category = $input['category'] ?? 'all';
        $confirm = $input['confirm'] ?? false;
        
        if (!$confirm) {
            Response::error('Vui lòng xác nhận reset cấu hình', 400);
        }
        
        $result = $this->configService->resetToDefault($category);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result, 'Reset cấu hình mặc định thành công');
    }
    
    /**
     * Lấy danh sách categories
     * GET /admin/configs/categories
     */
    public function getCategories() {
        $this->requireAdmin();
        
        $result = $this->configService->getCategories();
        
        Response::success($result, 'Lấy danh sách categories thành công');
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