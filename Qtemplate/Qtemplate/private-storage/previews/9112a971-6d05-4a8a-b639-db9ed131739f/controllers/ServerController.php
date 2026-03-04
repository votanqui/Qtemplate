<?php
// ============================================
// controllers/ServerController.php
// ============================================

require_once __DIR__ . '/../middlewares/RateLimitMiddleware.php';
require_once __DIR__ . '/../services/ServerService.php';
require_once __DIR__ . '/../helpers/Response.php';

class ServerController {
    private $serverService;
    
    public function __construct() {
        $this->serverService = new ServerService();
    }
    
    /**
     * Lấy danh sách servers đang hoạt động (PUBLIC - không cần login)
     * GET /api/servers
     */
    public function getActiveServers() {
        // Áp dụng rate limiting nhẹ cho public API
        $this->applyRateLimit('/api/servers', 'apiModerate');
        
        // Lấy danh sách servers từ service
        $result = $this->serverService->getActiveServers();
        
        if (!$result['success']) {
            Response::error($result['message'], 500);
        }
        
        Response::success([
            'total' => $result['total'],
            'servers' => $result['servers']
        ], 'Đã lấy danh sách servers thành công');
    }
    
    /**
     * Lấy thông tin chi tiết 1 server (PUBLIC - không cần login)
     * GET /api/servers/:id
     */
    public function getServerById() {
        // Áp dụng rate limiting
        $this->applyRateLimit('/api/servers/:id', 'apiModerate');
        
        // Lấy server_id từ URL
        $serverId = $_GET['id'] ?? null;
        
        if (!$serverId) {
            Response::error('Thiếu server_id', 400);
        }
        
        // Validate server_id là số
        if (!is_numeric($serverId)) {
            Response::error('server_id phải là số', 400);
        }
        
        // Lấy thông tin server từ service
        $result = $this->serverService->getServerById($serverId);
        
        if (!$result['success']) {
            Response::notFound($result['message']);
        }
        
        Response::success($result['server'], 'Đã lấy thông tin server thành công');
    }
    
    /**
     * Lấy tổng số servers đang hoạt động (PUBLIC)
     * GET /api/servers/count
     */
    public function getServerCount() {
        $this->applyRateLimit('/api/servers/count', 'apiModerate');
        
        $result = $this->serverService->getTotalActiveServers();
        
        Response::success([
            'total' => $result['total']
        ], 'Đã lấy tổng số servers');
    }
    
    /**
     * Áp dụng rate limiting với xử lý lỗi
     */
    private function applyRateLimit($route, $method) {
        try {
            if (class_exists('RateLimitMiddleware')) {
                if (method_exists('RateLimitMiddleware', $method)) {
                    call_user_func(['RateLimitMiddleware', $method], $route);
                } else {
                    error_log("RateLimitMiddleware method {$method} not found");
                }
            } else {
                error_log("RateLimitMiddleware class not found, skipping rate limiting");
            }
        } catch (Exception $e) {
            error_log("Rate limit error: " . $e->getMessage());
        }
    }
}