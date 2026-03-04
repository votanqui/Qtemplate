<?php
// controllers/GiftcodeController.php

class GiftcodeController {
    private $giftcodeService;
    
    public function __construct() {
        $this->giftcodeService = new GiftcodeService();
    }
    
    /**
     * GET /giftcodes?server_id=1
     * Lấy danh sách giftcode công khai (chỉ những giftcode còn hiệu lực)
     */
    public function getPublicGiftcodes() {
        $serverId = $_GET['server_id'] ?? null;
        
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $result = $this->giftcodeService->getPublicGiftcodes($serverId);
        
        Response::success($result, 'Lấy danh sách giftcode thành công');
    }
    
    /**
     * POST /giftcodes/use
     * Sử dụng giftcode
     * Body: { "server_id": 1, "user_id": 123, "giftcode": "ABC123" }
     */
    public function useGiftcode() {
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $serverId = $input['server_id'] ?? null;
        $userId = $input['user_id'] ?? null;
        $giftcode = $input['giftcode'] ?? null;
        
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        if (!$userId || !is_numeric($userId)) {
            Response::error('user_id là bắt buộc', 400);
        }
        
        if (empty($giftcode)) {
            Response::error('giftcode là bắt buộc', 400);
        }
        
        $result = $this->giftcodeService->useGiftcode($serverId, $userId, $giftcode);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['data'], $result['message']);
    }
    
    /**
     * GET /giftcodes/check?server_id=1&giftcode=ABC123
     * Kiểm tra thông tin giftcode
     */
    public function checkGiftcode() {
        $serverId = $_GET['server_id'] ?? null;
        $giftcode = $_GET['giftcode'] ?? null;
        
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        if (empty($giftcode)) {
            Response::error('giftcode là bắt buộc', 400);
        }
        
        $result = $this->giftcodeService->checkGiftcode($serverId, $giftcode);
        
        if (!$result) {
            Response::notFound('Giftcode không tồn tại');
        }
        
        Response::success($result, 'Lấy thông tin giftcode thành công');
    }
}