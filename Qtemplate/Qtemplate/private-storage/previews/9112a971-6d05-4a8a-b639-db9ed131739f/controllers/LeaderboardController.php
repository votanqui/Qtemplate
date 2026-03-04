<?php
// controllers/LeaderboardController.php

require_once __DIR__ . '/../middlewares/RateLimitMiddleware.php';
require_once __DIR__ . '/../services/LeaderboardService.php';
require_once __DIR__ . '/../helpers/Response.php';

class LeaderboardController {
    private $leaderboardService;
    
    public function __construct() {
        $this->leaderboardService = new LeaderboardService();
    }
    
    /**
     * GET /leaderboard/level
     * Bảng xếp hạng Level hiện tại (realtime)
     */
    public function level() {
        RateLimitMiddleware::apply('/leaderboard/level', 60, 60, 300, false);
        
        $serverId = isset($_GET['server_id']) ? (int)$_GET['server_id'] : null;
        $limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 10;
        
        $result = $this->leaderboardService->getTopLevel($limit, $serverId);
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], 'Lấy bảng xếp hạng level thành công');
    }
    
    /**
     * GET /leaderboard/event
     * Bảng xếp hạng Sự Kiện hiện tại (realtime)
     */
    public function event() {
        RateLimitMiddleware::apply('/leaderboard/event', 60, 60, 300, false);
        
        $serverId = isset($_GET['server_id']) ? (int)$_GET['server_id'] : null;
        $limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 10;
        
        $result = $this->leaderboardService->getTopEvent($limit, $serverId);
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], 'Lấy bảng xếp hạng sự kiện thành công');
    }
    
    /**
     * GET /leaderboard/recharge
     * Bảng xếp hạng Tích Lũy hiện tại (realtime)
     */
    public function recharge() {
        RateLimitMiddleware::apply('/leaderboard/recharge', 60, 60, 300, false);
        
        $serverId = isset($_GET['server_id']) ? (int)$_GET['server_id'] : null;
        $limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 10;
        
        $result = $this->leaderboardService->getTopRecharge($limit, $serverId);
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], 'Lấy bảng xếp hạng tích lũy thành công');
    }
    
    /**
     * GET /leaderboard/boss
     * Bảng xếp hạng Săn Boss hiện tại (realtime)
     */
    public function boss() {
        RateLimitMiddleware::apply('/leaderboard/boss', 60, 60, 300, false);
        
        $serverId = isset($_GET['server_id']) ? (int)$_GET['server_id'] : null;
        $limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 10;
        
        $result = $this->leaderboardService->getTopBoss($limit, $serverId);
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], 'Lấy bảng xếp hạng săn boss thành công');
    }
    
    /**
     * GET /leaderboard/event-recharge/history
     * Lấy kết quả event recharge từ mùa trước
     * 
     * Query params:
     * - event_id: ID của event (bắt buộc)
     * - server_id: ID server (optional)
     * - limit: Số lượng kết quả (default: 10)
     */
    public function eventRechargeHistory() {
        RateLimitMiddleware::apply('/leaderboard/event-recharge/history', 60, 60, 300, false);
        
        if (!isset($_GET['event_id'])) {
            Response::error('Thiếu event_id', 400);
        }
        
        $eventId = (int)$_GET['event_id'];
        $serverId = isset($_GET['server_id']) ? (int)$_GET['server_id'] : null;
        $limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 10;
        
        $result = $this->leaderboardService->getEventRechargeHistory($eventId, $limit, $serverId);
        
        if (!$result['success']) {
            Response::error($result['message'], 404);
        }
        
        Response::success($result['data'], 'Lấy kết quả event thành công');
    }
    
    /**
     * GET /leaderboard/events/finished
     * Lấy danh sách các event đã kết thúc
     * 
     * Query params:
     * - type: Loại event (recharge, level, boss, event) - optional
     */
    public function finishedEvents() {
        RateLimitMiddleware::apply('/leaderboard/events/finished', 60, 60, 300, false);
        
        $eventType = isset($_GET['type']) ? $_GET['type'] : null;
        
        // Validate event type nếu có
        if ($eventType !== null) {
            $validTypes = ['recharge', 'level', 'boss', 'event'];
            if (!in_array($eventType, $validTypes)) {
                Response::error('Loại event không hợp lệ', 400);
            }
        }
        
        $result = $this->leaderboardService->getFinishedEvents($eventType);
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], 'Lấy danh sách event thành công');
    }
    
    /**
     * GET /leaderboard/servers
     * Lấy danh sách server
     */
    public function servers() {
        RateLimitMiddleware::apply('/leaderboard/servers', 60, 60, 300, false);
        
        $result = $this->leaderboardService->getServerList();
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], 'Lấy danh sách server thành công');
    }
}