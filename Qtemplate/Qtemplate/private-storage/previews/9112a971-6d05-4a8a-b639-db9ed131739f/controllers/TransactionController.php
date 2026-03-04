<?php
// controllers/TransactionController.php

require_once __DIR__ . '/../services/TransactionService.php';
require_once __DIR__ . '/../helpers/Response.php';
require_once __DIR__ . '/../middlewares/RateLimitMiddleware.php';

class TransactionController {
    private $transactionService;
    
    public function __construct() {
        $this->transactionService = new TransactionService();
    }
    
    /**
     * GET /transactions/status
     * Kiểm tra trạng thái giao dịch theo order_id
     */
    public function getStatus() {
        // Rate limit: 10 requests mỗi phút
        RateLimitMiddleware::apply('/transactions/status', 60, 60, 300, false);
        
        $orderId = $_GET['order_id'] ?? null;
        
        if (!$orderId) {
            Response::error('Thiếu tham số order_id', 400);
        }
        
        $result = $this->transactionService->getTransactionStatus($orderId);
        
        if (!$result['success']) {
            Response::error($result['message'], 404);
        }
        
        Response::success($result['data'], 'Lấy trạng thái giao dịch thành công');
    }
    
    /**
     * GET /transactions/history
     * Lịch sử giao dịch của user (activation + recharge)
     */
    public function getHistory() {
        // Rate limit: 10 requests mỗi phút
        RateLimitMiddleware::apply('/transactions/history', 60, 60, 300, false);
        
        $userId = isset($_GET['user_id']) ? (int)$_GET['user_id'] : null;
        $type = $_GET['type'] ?? 'all'; // all, activation, recharge_xu, recharge_luong
        $page = isset($_GET['page']) ? (int)$_GET['page'] : 1;
        $limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 10;
        
        if (!$userId) {
            Response::error('Thiếu tham số user_id', 400);
        }
        
        $result = $this->transactionService->getTransactionHistory($userId, $type, $page, $limit);
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], 'Lấy lịch sử giao dịch thành công');
    }
    
    /**
     * GET /transactions/pending
     * Lấy danh sách giao dịch đang chờ của user
     */
    public function getPending() {
        // Rate limit: 10 requests mỗi phút
        RateLimitMiddleware::apply('/transactions/pending', 60, 60, 300, false);
        
        $userId = isset($_GET['user_id']) ? (int)$_GET['user_id'] : null;
        
        if (!$userId) {
            Response::error('Thiếu tham số user_id', 400);
        }
        
        $result = $this->transactionService->getPendingTransactions($userId);
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], 'Lấy giao dịch pending thành công');
    }
    
    /**
     * GET /transactions/statistics
     * Thống kê giao dịch của user
     */
    public function getStatistics() {
        // Rate limit: 10 requests mỗi phút
        RateLimitMiddleware::apply('/transactions/statistics', 60, 60, 300, false);
        
        $userId = isset($_GET['user_id']) ? (int)$_GET['user_id'] : null;
        
        if (!$userId) {
            Response::error('Thiếu tham số user_id', 400);
        }
        
        $result = $this->transactionService->getUserStatistics($userId);
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success($result['data'], 'Lấy thống kê thành công');
    }
}