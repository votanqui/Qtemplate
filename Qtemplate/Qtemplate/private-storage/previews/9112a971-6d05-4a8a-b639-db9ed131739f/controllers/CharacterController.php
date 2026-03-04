<?php
// controllers/CharacterController.php

require_once __DIR__ . '/../middlewares/RateLimitMiddleware.php';
require_once __DIR__ . '/../services/CharacterService.php';
require_once __DIR__ . '/../services/AuthService.php';
require_once __DIR__ . '/../helpers/Response.php';

class CharacterController {
    private $characterService;
    private $authService;
    
    public function __construct() {
        $this->characterService = new CharacterService();
        $this->authService = new AuthService();
    }
    
    /**
     * GET /characters
     * Lấy danh sách nhân vật của user đang login
     */
    public function getCharacters() {
        // Rate limit: 20 requests mỗi phút
        RateLimitMiddleware::apply('/characters', 60, 60, 300, false);
        
        // Validate token
        $token = $this->getBearerToken();
        
        if (!$token) {
            Response::unauthorized('Yêu cầu mã truy cập');
        }
        
        $userId = $this->authService->validateAccessToken($token);
        
        if (!$userId) {
            Response::unauthorized('Mã không hợp lệ hoặc đã hết hạn');
        }
        
        // Lấy server_id từ query params (mặc định = 1)
        $serverId = isset($_GET['server_id']) ? (int)$_GET['server_id'] : 1;
        
        // Lấy danh sách nhân vật
        $result = $this->characterService->getUserCharacters($userId, $serverId);
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], $result['message']);
    }
    
    /**
     * GET /characters/detail
     * Lấy thông tin chi tiết 1 nhân vật
     */
    public function getCharacterDetail() {
        // Rate limit: 30 requests mỗi phút
        RateLimitMiddleware::apply('/characters/detail', 60, 60, 300, false);
        
        // Validate token
        $token = $this->getBearerToken();
        
        if (!$token) {
            Response::unauthorized('Yêu cầu mã truy cập');
        }
        
        $userId = $this->authService->validateAccessToken($token);
        
        if (!$userId) {
            Response::unauthorized('Mã không hợp lệ hoặc đã hết hạn');
        }
        
        // Lấy charname từ query params
        $charname = $_GET['charname'] ?? null;
        $serverId = isset($_GET['server_id']) ? (int)$_GET['server_id'] : 1;
        
        if (!$charname) {
            Response::error('Thiếu tham số charname', 400);
        }
        
        // Lấy thông tin nhân vật
        $result = $this->characterService->getCharacterDetail($userId, $charname, $serverId);
        
        if (!$result['success']) {
            Response::error($result['message'], 404);
        }
        
        Response::success($result['data'], $result['message']);
    }
    
    /**
     * GET /characters/stats
     * Lấy thống kê nhân vật của user
     */
    public function getCharacterStats() {
        // Rate limit: 20 requests mỗi phút
        RateLimitMiddleware::apply('/characters/stats', 60, 60, 300, false);
        
        // Validate token
        $token = $this->getBearerToken();
        
        if (!$token) {
            Response::unauthorized('Yêu cầu mã truy cập');
        }
        
        $userId = $this->authService->validateAccessToken($token);
        
        if (!$userId) {
            Response::unauthorized('Mã không hợp lệ hoặc đã hết hạn');
        }
        
        // Lấy server_id từ query params
        $serverId = isset($_GET['server_id']) ? (int)$_GET['server_id'] : 1;
        
        // Lấy thống kê
        $result = $this->characterService->getUserCharacterStats($userId, $serverId);
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], $result['message']);
    }
    
    /**
     * Lấy Bearer token từ header
     */
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