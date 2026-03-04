<?php
// controllers/RechargeController.php

require_once __DIR__ . '/../middlewares/RateLimitMiddleware.php';
require_once __DIR__ . '/../services/RechargeService.php';
require_once __DIR__ . '/../helpers/Response.php';

class RechargeController {
    private $rechargeService;
    
    public function __construct() {
        $this->rechargeService = new RechargeService();
    }
    
    /**
     * POST /recharge/xu/create
     * Tạo lệnh nạp xu (SỬ DỤNG PLAYER NAME)
     */
    public function createXuOrder() {
        // Rate limit: 5 requests mỗi phút
        RateLimitMiddleware::apply('/recharge/xu/create', 5, 60, 300, true);
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $playerName = $input['player_name'] ?? null;
        $serverId = $input['server_id'] ?? Config::DEFAULT_SERVER_ID;
        $amount = $input['amount'] ?? null;
        
        if (!$playerName || !$amount) {
            Response::error('Thiếu thông tin player_name hoặc amount', 400);
        }
        
        $result = $this->rechargeService->createXuRechargeOrder($playerName, $serverId, $amount);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['data'], 'Tạo lệnh nạp xu thành công');
    }
    
    /**
     * POST /recharge/luong/create
     * Tạo lệnh nạp lượng (SỬ DỤNG PLAYER NAME)
     */
    public function createLuongOrder() {
        // Rate limit: 5 requests mỗi phút
        RateLimitMiddleware::apply('/recharge/luong/create', 5, 60, 300, true);
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $playerName = $input['player_name'] ?? null;
        $serverId = $input['server_id'] ?? Config::DEFAULT_SERVER_ID;
        $amount = $input['amount'] ?? null;
        
        if (!$playerName || !$amount) {
            Response::error('Thiếu thông tin player_name hoặc amount', 400);
        }
        
        $result = $this->rechargeService->createLuongRechargeOrder($playerName, $serverId, $amount);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['data'], 'Tạo lệnh nạp lượng thành công');
    }
    
    /**
     * POST /recharge/cancel
     * Hủy một lệnh nạp cụ thể (HỖ TRỢ CẢ PLAYER NAME)
     */
    public function cancelOrder() {
        // Rate limit: 10 requests mỗi phút
        RateLimitMiddleware::apply('/recharge/cancel', 10, 60, 300, true);
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $orderId = $input['order_id'] ?? null;
        $playerName = $input['player_name'] ?? null;
        
        if (!$orderId) {
            Response::error('Thiếu tham số order_id', 400);
        }
        
        $result = $this->rechargeService->cancelPendingOrder($orderId, $playerName);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['data'], $result['message']);
    }
    
    /**
     * POST /recharge/cancel-all
     * Hủy tất cả lệnh nạp đang pending của player (SỬ DỤNG PLAYER NAME)
     */
    public function cancelAllOrders() {
        // Rate limit: 5 requests mỗi phút
        RateLimitMiddleware::apply('/recharge/cancel-all', 5, 60, 300, true);
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $playerName = $input['player_name'] ?? null;
        $serverId = $input['server_id'] ?? null;
        
        if (!$playerName) {
            Response::error('Thiếu tham số player_name', 400);
        }
        
        $result = $this->rechargeService->cancelAllPendingOrders($playerName, $serverId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['data'], $result['message']);
    }
    
    /**
     * POST /recharge/create-with-cancel
     * Tạo lệnh nạp mới với tự động hủy order cũ (SỬ DỤNG PLAYER NAME)
     */
    public function createWithCancel() {
        // Rate limit: 5 requests mỗi phút
        RateLimitMiddleware::apply('/recharge/create-with-cancel', 5, 60, 300, true);
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $playerName = $input['player_name'] ?? null;
        $serverId = $input['server_id'] ?? Config::DEFAULT_SERVER_ID;
        $amount = $input['amount'] ?? null;
        $type = $input['type'] ?? null; // 'xu' hoặc 'luong'
        
        if (!$playerName || !$amount || !$type) {
            Response::error('Thiếu thông tin player_name, amount hoặc type', 400);
        }
        
        if (!in_array($type, ['xu', 'luong'])) {
            Response::error('Loại nạp không hợp lệ. Chỉ chấp nhận "xu" hoặc "luong"', 400);
        }
        
        $result = $this->rechargeService->createNewOrderWithAutoCancel($playerName, $serverId, $amount, $type);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['data'], $result['message'] ?? 'Tạo lệnh nạp thành công');
    }
    
    /**
     * GET /recharge/pending
     * Lấy danh sách lệnh nạp đang pending (HỖ TRỢ CẢ PLAYER NAME VÀ USER ID)
     */
    public function getPending() {
        // Rate limit: 20 requests mỗi phút
        RateLimitMiddleware::apply('/recharge/pending', 60, 60, 300, false);
        
        $playerName = $_GET['player_name'] ?? null;
        $userId = isset($_GET['user_id']) ? (int)$_GET['user_id'] : null;
        $serverId = isset($_GET['server_id']) ? (int)$_GET['server_id'] : null;
        
        // Ưu tiên player_name, fallback sang user_id
        if ($playerName) {
            $result = $this->rechargeService->getPendingOrders($playerName, $serverId);
        } elseif ($userId) {
            // Tìm username từ user_id để tương thích ngược
            $accountDb = Database::getInstance()->getConnection();
            $stmt = $accountDb->prepare("SELECT username FROM team_user WHERE id = ? LIMIT 1");
            $stmt->execute([$userId]);
            $user = $stmt->fetch();
            
            if (!$user) {
                Response::error('User không tồn tại', 404);
            }
            
            $result = $this->rechargeService->getPendingOrders($user['username'], $serverId);
        } else {
            Response::error('Thiếu tham số player_name hoặc user_id', 400);
        }
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], 'Lấy danh sách order pending thành công');
    }
    
    /**
     * GET /recharge/qr
     * Lấy QR code cho lệnh nạp
     */
    public function getQR() {
        // Rate limit: 20 requests mỗi phút
        RateLimitMiddleware::apply('/recharge/qr', 60, 60, 300, false);
        
        $orderId = isset($_GET['order_id']) ? $_GET['order_id'] : null;
        
        if (!$orderId) {
            Response::error('Thiếu tham số order_id', 400);
        }
        
        $result = $this->rechargeService->getQRCode($orderId);
        
        if (!$result['success']) {
            Response::error($result['message'], 404);
        }
        
        Response::success($result['data'], 'Lấy QR code thành công');
    }
    
    /**
     * GET /recharge/verify
     * Kiểm tra trạng thái lệnh nạp
     */
    public function verify() {
        // Rate limit: 100 requests mỗi phút
        RateLimitMiddleware::apply('/recharge/verify', 100, 60, 300, false);
        
        $orderId = isset($_GET['order_id']) ? $_GET['order_id'] : null;
        
        if (!$orderId) {
            Response::error('Thiếu tham số order_id', 400);
        }
        
        $result = $this->rechargeService->verifyOrder($orderId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['data'], 'Kiểm tra lệnh nạp thành công');
    }
    
    /**
     * GET /recharge/history
     * Lịch sử nạp (HỖ TRỢ CẢ PLAYER NAME VÀ USER ID)
     */
    public function history() {
        // Rate limit: 20 requests mỗi phút
        RateLimitMiddleware::apply('/recharge/history', 60, 60, 300, false);
        
        $playerName = $_GET['player_name'] ?? null;
        $userId = isset($_GET['user_id']) ? (int)$_GET['user_id'] : null;
        $type = isset($_GET['type']) ? $_GET['type'] : null; // 'xu' hoặc 'luong'
        $page = isset($_GET['page']) ? (int)$_GET['page'] : 1;
        $limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 10;
        
        // Ưu tiên player_name, fallback sang user_id
        if ($playerName) {
            $result = $this->rechargeService->getRechargeHistory($playerName, $type, $page, $limit);
        } elseif ($userId) {
            // Tìm username từ user_id để tương thích ngược
            $accountDb = Database::getInstance()->getConnection();
            $stmt = $accountDb->prepare("SELECT username FROM team_user WHERE id = ? LIMIT 1");
            $stmt->execute([$userId]);
            $user = $stmt->fetch();
            
            if (!$user) {
                Response::error('User không tồn tại', 404);
            }
            
            $result = $this->rechargeService->getRechargeHistory($user['username'], $type, $page, $limit);
        } else {
            Response::error('Thiếu tham số player_name hoặc user_id', 400);
        }
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], 'Lấy lịch sử thành công');
    }
}