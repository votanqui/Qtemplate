<?php
// controllers/admin/AdminSocialLinksController.php

class AdminSocialLinksController {
    private $socialLinksService;
    private $authService;
    
    public function __construct() {
        $this->socialLinksService = new AdminSocialLinksService();
        $this->authService = new AuthService();
    }
    
    /**
     * Lấy danh sách social links
     * GET /admin/social-links
     */
    public function getSocialLinks() {
        $this->requireAdmin();
        
        $result = $this->socialLinksService->getSocialLinks();
        
        Response::success(['social_links' => $result], 'Lấy danh sách social links thành công');
    }
    
    /**
     * Lấy chi tiết social link
     * GET /admin/social-links/{id}
     */
    public function getSocialLinkDetail($id) {
        $this->requireAdmin();
        
        if (empty($id) || !is_numeric($id)) {
            Response::error('ID social link không hợp lệ', 400);
        }
        
        $result = $this->socialLinksService->getSocialLinkDetail($id);
        
        if (!$result) {
            Response::notFound('Không tìm thấy social link');
        }
        
        Response::success($result, 'Lấy thông tin social link thành công');
    }
    
    /**
     * Tạo social link mới
     * POST /admin/social-links
     */
    public function createSocialLink() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $result = $this->socialLinksService->createSocialLink($input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['social_link'], 'Tạo social link thành công', 201);
    }
    
    /**
     * Cập nhật social link
     * PUT /admin/social-links/{id}
     */
    public function updateSocialLink($id) {
        $this->requireAdmin();
        
        if (empty($id) || !is_numeric($id)) {
            Response::error('ID social link không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $result = $this->socialLinksService->updateSocialLink($id, $input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['social_link'], 'Cập nhật social link thành công');
    }
    
    /**
     * Xóa social link
     * DELETE /admin/social-links/{id}
     */
    public function deleteSocialLink($id) {
        $this->requireAdmin();
        
        if (empty($id) || !is_numeric($id)) {
            Response::error('ID social link không hợp lệ', 400);
        }
        
        $result = $this->socialLinksService->deleteSocialLink($id);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success(null, 'Xóa social link thành công');
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