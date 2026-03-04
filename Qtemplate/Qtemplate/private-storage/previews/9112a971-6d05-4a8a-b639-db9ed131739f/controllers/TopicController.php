<?php
// controllers/TopicController.php

require_once __DIR__ . '/../services/TopicService.php';
require_once __DIR__ . '/../helpers/Response.php';
require_once __DIR__ . '/../middlewares/RateLimitMiddleware.php';

class TopicController {
    private $topicService;
    
    public function __construct() {
        $this->topicService = new TopicService();
    }
    
    /**
     * Lấy danh sách bài viết với phân trang, lọc, sắp xếp
     * GET /topics
     * Query params:
     * - page: số trang (mặc định 1)
     * - limit: số bài viết mỗi trang (mặc định 20, max 100)
     * - topic: lọc theo loại topic
     * - owner: lọc theo chủ sở hữu
     * - block: lọc theo trạng thái block (0/1)
     * - stick: lọc theo trạng thái stick (0/1)
     * - done: lọc theo trạng thái done (0/1)
     * - search: tìm kiếm theo title
     * - sort: sắp xếp (newest, oldest, title)
     */
    public function getTopics() {
        // Áp dụng rate limiting
        $this->applyRateLimit('/topics', 'apiModerate');
        
        // Lấy query parameters
        $page = max(1, intval($_GET['page'] ?? 1));
        $limit = min(100, max(1, intval($_GET['limit'] ?? 20)));
        $filters = [
            'topic' => isset($_GET['topic']) ? intval($_GET['topic']) : null,
            'owner' => isset($_GET['owner']) ? intval($_GET['owner']) : null,
            'block' => isset($_GET['block']) ? intval($_GET['block']) : null,
            'stick' => isset($_GET['stick']) ? intval($_GET['stick']) : null,
            'done' => isset($_GET['done']) ? intval($_GET['done']) : null,
            'search' => $_GET['search'] ?? null
        ];
        $sort = $_GET['sort'] ?? 'newest';
        
        $result = $this->topicService->getTopics($page, $limit, $filters, $sort);
        
        Response::success([
            'topics' => $result['topics'],
            'pagination' => $result['pagination']
        ], 'Lấy danh sách bài viết thành công');
    }
    
    /**
     * Lấy chi tiết bài viết theo ID
     * GET /topics/{id}
     */
    public function getTopicDetail($id) {
        // Áp dụng rate limiting
        $this->applyRateLimit('/topics/detail', 'apiModerate');
        
        if (!is_numeric($id) || $id <= 0) {
            Response::error('ID bài viết không hợp lệ', 400);
        }
        
        $topic = $this->topicService->getTopicById($id);
        
        if (!$topic) {
            Response::notFound('Không tìm thấy bài viết');
        }
        
        Response::success($topic, 'Lấy chi tiết bài viết thành công');
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