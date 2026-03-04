<?php
// controllers/ActivationController.php

require_once __DIR__ . '/../middlewares/RateLimitMiddleware.php';
require_once __DIR__ . '/../services/ActivationService.php';
require_once __DIR__ . '/../helpers/Response.php';
require_once __DIR__ . '/../helpers/RateLimiter.php';

class ActivationController {
    private $activationService;
    
    public function __construct() {
        $this->activationService = new ActivationService();
    }
    
    /**
     * GET /activation/status
     * Kiểm tra trạng thái kích hoạt
     */
    public function status() {
        // Rate limit: 10 requests mỗi phút
        RateLimitMiddleware::apply('/activation/status', 30, 60, 300, false);
        
        $userId = isset($_GET['user_id']) ? (int)$_GET['user_id'] : null;
        $serverId = isset($_GET['server_id']) ? (int)$_GET['server_id'] : Config::DEFAULT_SERVER_ID;
        
        if (!$userId) {
            Response::error('Thiếu tham số user_id', 400);
        }
        
        $result = $this->activationService->checkActivationStatus($userId, $serverId);
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], 'Kiểm tra trạng thái thành công');
    }
    
    /**
     * POST /activation/request
     * Yêu cầu kích hoạt mới
     */
    public function request() {
        // Rate limit: 3 requests mỗi phút (chống spam)
        RateLimitMiddleware::apply('/activation/request', 3, 60, 600, true);
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $userId = $input['user_id'] ?? null;
        $serverId = $input['server_id'] ?? Config::DEFAULT_SERVER_ID;
       
        
        if (!$userId ) {
            Response::error('Thiếu thông tin user_id ', 400);
        }
        
        $result = $this->activationService->createActivationRequest($userId, $serverId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['data'], 'Tạo yêu cầu kích hoạt thành công');
    }
    
    /**
     * GET /activation/qr
     * Lấy QR code cho chuyển khoản
     */
    public function qr() {
        // Rate limit: 20 requests mỗi phút
        RateLimitMiddleware::apply('/activation/qr', 60, 60, 300, false);
        
        $orderId = isset($_GET['order_id']) ? $_GET['order_id'] : null;
        
        if (!$orderId) {
            Response::error('Thiếu tham số order_id', 400);
        }
        
        $result = $this->activationService->getQRCode($orderId);
        
        if (!$result['success']) {
            Response::error($result['message'], 404);
        }
        
        Response::success($result['data'], 'Lấy QR code thành công');
    }
    
    /**
     * GET /activation/verify
     * Kiểm tra giao dịch (dùng cho webhook hoặc polling)
     */
    public function verify() {
        // Rate limit: 5 requests mỗi phút
        RateLimitMiddleware::apply('/activation/verify', 100, 60, 300, false);
        
        $orderId = isset($_GET['order_id']) ? $_GET['order_id'] : null;
        
        if (!$orderId) {
            Response::error('Thiếu tham số order_id', 400);
        }
        
        $result = $this->activationService->verifyPayment($orderId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['data'], 'Kiểm tra giao dịch thành công');
    }
    
    /**
     * POST /activation/webhook
     * Webhook từ VietQR (nếu có)
     */
    public function webhook() {
        // Xác thực webhook nếu cần
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu webhook không hợp lệ', 400);
        }
        
        $result = $this->activationService->processWebhook($input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success(null, 'Webhook processed successfully');
    }
    
    /**
     * GET /activation/history
     * Lịch sử kích hoạt của user
     */
    public function history() {
        // Rate limit: 10 requests mỗi phút
        RateLimitMiddleware::apply('/activation/history', 30, 60, 300, false);
        
        $userId = isset($_GET['user_id']) ? (int)$_GET['user_id'] : null;
        $page = isset($_GET['page']) ? (int)$_GET['page'] : 1;
        $limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 10;
        
        if (!$userId) {
            Response::error('Thiếu tham số user_id', 400);
        }
        
        $result = $this->activationService->getActivationHistory($userId, $page, $limit);
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], 'Lấy lịch sử thành công');
    }
    public function checkPending() {
    // Rate limit: 20 requests mỗi phút
    RateLimitMiddleware::apply('/activation/check-pending', 30, 60, 300, false);
    
    $userId = isset($_GET['user_id']) ? (int)$_GET['user_id'] : null;
    $serverId = isset($_GET['server_id']) ? (int)$_GET['server_id'] : Config::DEFAULT_SERVER_ID;
    
    if (!$userId) {
        Response::error('Thiếu tham số user_id', 400);
    }
    
    $result = $this->activationService->checkPendingRequest($userId, $serverId);
    
    if (!$result['success']) {
        Response::error($result['message'], 500);
    }
    
    Response::success($result, 'Kiểm tra yêu cầu pending thành công');
}
}