<?php
// controllers/admin/AdminTopicController.php

class AdminTopicController {
    private $adminTopicService;
    private $authService;
    
    public function __construct() {
        $this->adminTopicService = new AdminTopicService();
        $this->authService = new AuthService();
    }
    
    /**
     * Lấy danh sách topics với phân trang và tìm kiếm
     * GET /admin/topics?page=1&limit=20&search=keyword&status=all
     */
    public function getTopics() {
        $this->requireAdmin();
        
        $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
        $limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 20;
        $search = $_GET['search'] ?? '';
        $status = $_GET['status'] ?? 'all';
        $sortBy = $_GET['sort_by'] ?? 'time_created';
        $sortOrder = $_GET['sort_order'] ?? 'DESC';
        
        $result = $this->adminTopicService->getTopics($page, $limit, $search, $status, $sortBy, $sortOrder);
        
        Response::success($result, 'Lấy danh sách topics thành công');
    }
    
    /**
     * Lấy chi tiết topic
     * GET /admin/topics/:id
     */
    public function getTopicDetail($topicId) {
        $this->requireAdmin();
        
        if (empty($topicId) || !is_numeric($topicId)) {
            Response::error('ID topic không hợp lệ', 400);
        }
        
        $result = $this->adminTopicService->getTopicDetail($topicId);
        
        if (!$result) {
            Response::notFound('Không tìm thấy topic');
        }
        
        Response::success($result, 'Lấy thông tin topic thành công');
    }
    
    /**
     * Tạo topic mới
     * POST /admin/topics
     */
    public function createTopic() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $result = $this->adminTopicService->createTopic($input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['topic'], 'Tạo topic thành công', 201);
    }
    
    /**
     * Cập nhật topic
     * PUT /admin/topics/:id
     */
    public function updateTopic($topicId) {
        $this->requireAdmin();
        
        if (empty($topicId) || !is_numeric($topicId)) {
            Response::error('ID topic không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $result = $this->adminTopicService->updateTopic($topicId, $input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['topic'], 'Cập nhật topic thành công');
    }
    
    /**
     * Block/Unblock topic
     * POST /admin/topics/:id/block
     */
    public function toggleBlock($topicId) {
        $this->requireAdmin();
        
        if (empty($topicId) || !is_numeric($topicId)) {
            Response::error('ID topic không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        $action = $input['action'] ?? 'block';
        
        $result = $this->adminTopicService->toggleBlock($topicId, $action);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success([
            'topic_id' => $topicId,
            'blocked' => $result['blocked']
        ], $result['message']);
    }
    
    /**
     * Sticky/Unsticky topic
     * POST /admin/topics/:id/sticky
     */
    public function toggleSticky($topicId) {
        $this->requireAdmin();
        
        if (empty($topicId) || !is_numeric($topicId)) {
            Response::error('ID topic không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        $action = $input['action'] ?? 'stick';
        
        $result = $this->adminTopicService->toggleSticky($topicId, $action);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success([
            'topic_id' => $topicId,
            'sticky' => $result['sticky']
        ], $result['message']);
    }
    
    /**
     * Mark as done/undone
     * POST /admin/topics/:id/done
     */
    public function toggleDone($topicId) {
        $this->requireAdmin();
        
        if (empty($topicId) || !is_numeric($topicId)) {
            Response::error('ID topic không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        $action = $input['action'] ?? 'done';
        
        $result = $this->adminTopicService->toggleDone($topicId, $action);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success([
            'topic_id' => $topicId,
            'done' => $result['done']
        ], $result['message']);
    }
    
    /**
     * Xóa topic
     * DELETE /admin/topics/:id
     */
    public function deleteTopic($topicId) {
        $this->requireAdmin();
        
        if (empty($topicId) || !is_numeric($topicId)) {
            Response::error('ID topic không hợp lệ', 400);
        }
        
        $result = $this->adminTopicService->deleteTopic($topicId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success(null, 'Xóa topic thành công');
    }
    
    /**
     * Lấy thống kê
     * GET /admin/topics/stats?period=day
     */
    public function getStats() {
        $this->requireAdmin();
        
        $period = $_GET['period'] ?? 'day';
        
        $result = $this->adminTopicService->getStats($period);
        
        Response::success($result, 'Lấy thống kê thành công');
    }
    
    /**
     * Export topics to CSV
     * GET /admin/topics/export?search=keyword&status=all
     */
    public function exportTopics() {
        $this->requireAdmin();
        
        $search = $_GET['search'] ?? '';
        $status = $_GET['status'] ?? 'all';
        
        $result = $this->adminTopicService->exportTopics($search, $status);
        
        header('Content-Type: text/csv; charset=utf-8');
        header('Content-Disposition: attachment; filename="topics_export_' . date('Y-m-d_His') . '.csv"');
        
        echo "\xEF\xBB\xBF";
        echo $result;
        exit;
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