<?php
// controllers/admin/AdminMilestoneController.php

class AdminMilestoneController {
    private $milestoneService;
    private $authService;
    
    public function __construct() {
        $this->milestoneService = new AdminMilestoneService();
        $this->authService = new AuthService();
    }
    
    /**
     * GET /admin/milestones?page=1&limit=20&search=&status=all
     */
    public function getMilestones() {
        $this->requireAdmin();
        
        $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
        $limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 20;
        $search = $_GET['search'] ?? '';
        $status = $_GET['status'] ?? 'all'; // all, active, inactive
        
        $result = $this->milestoneService->getMilestones($page, $limit, $search, $status);
        
        Response::success($result, 'Lấy danh sách mốc nạp thành công');
    }
    
    /**
     * GET /admin/milestones/{id}
     */
    public function getMilestoneDetail($id) {
        $this->requireAdmin();
        
        if (empty($id) || !is_numeric($id)) {
            Response::error('ID mốc nạp không hợp lệ', 400);
        }
        
        $result = $this->milestoneService->getMilestoneDetail($id);
        
        if (!$result) {
            Response::notFound('Không tìm thấy mốc nạp');
        }
        
        Response::success($result, 'Lấy thông tin mốc nạp thành công');
    }
    
    /**
     * POST /admin/milestones
     * Body: {
     *   "milestone_amount": 100000,
     *   "reward_xu": 50000,
     *   "reward_luong": 5000,
     *   "reward_luong_khoa": 5000,
     *   "items": ["DB:685:1:-1", "GEM:249:2000:-1"],
     *   "description": "Mốc nạp 100k",
     *   "display_order": 1,
     *   "is_active": 1
     * }
     */
    public function createMilestone() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $result = $this->milestoneService->createMilestone($input);
        
        if (!$result['success']) {
            Response::validationError($result['errors'] ?? ['message' => $result['message']]);
        }
        
        Response::success($result['milestone'], 'Tạo mốc nạp thành công', 201);
    }
    
    /**
     * PUT /admin/milestones/{id}
     * Body: { ...update_data }
     */
    public function updateMilestone($id) {
        $this->requireAdmin();
        
        if (empty($id) || !is_numeric($id)) {
            Response::error('ID mốc nạp không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $result = $this->milestoneService->updateMilestone($id, $input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['milestone'], 'Cập nhật mốc nạp thành công');
    }
    
    /**
     * DELETE /admin/milestones/{id}
     */
    public function deleteMilestone($id) {
        $this->requireAdmin();
        
        if (empty($id) || !is_numeric($id)) {
            Response::error('ID mốc nạp không hợp lệ', 400);
        }
        
        $result = $this->milestoneService->deleteMilestone($id);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success(null, 'Xóa mốc nạp thành công');
    }
    
    /**
     * GET /admin/milestones/{id}/logs?page=1&limit=20
     */
    public function getMilestoneClaimLog($id) {
        $this->requireAdmin();
        
        if (empty($id) || !is_numeric($id)) {
            Response::error('ID mốc nạp không hợp lệ', 400);
        }
        
        $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
        $limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 20;
        
        $result = $this->milestoneService->getMilestoneClaimLog($id, $page, $limit);
        
        if (!$result) {
            Response::notFound('Không tìm thấy mốc nạp');
        }
        
        Response::success($result, 'Lấy lịch sử claim thành công');
    }
    
    /**
     * GET /admin/milestones/stats
     */
    public function getMilestoneStats() {
        $this->requireAdmin();
        
        $result = $this->milestoneService->getMilestoneStats();
        
        Response::success($result, 'Lấy thống kê mốc nạp thành công');
    }
    
    /**
     * GET /admin/milestones/{id}/users?server_id=1&page=1&limit=20
     */
    public function getMilestoneUsers($id) {
        $this->requireAdmin();
        
        if (empty($id) || !is_numeric($id)) {
            Response::error('ID mốc nạp không hợp lệ', 400);
        }
        
        $serverId = isset($_GET['server_id']) && is_numeric($_GET['server_id']) ? intval($_GET['server_id']) : null;
        $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
        $limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 20;
        
        $result = $this->milestoneService->getMilestoneUsers($id, $serverId, $page, $limit);
        
        Response::success($result, 'Lấy danh sách users thành công');
    }
    
    /**
     * GET /admin/milestones/export?search=&status=all
     */
    public function exportMilestones() {
        $this->requireAdmin();
        
        $search = $_GET['search'] ?? '';
        $status = $_GET['status'] ?? 'all';
        
        $result = $this->milestoneService->exportMilestones($search, $status);
        
        header('Content-Type: text/csv; charset=utf-8');
        header('Content-Disposition: attachment; filename="milestones_export_' . date('Y-m-d_His') . '.csv"');
        
        echo "\xEF\xBB\xBF";
        echo $result;
        exit;
    }
    
    /**
     * POST /admin/milestones/bulk-delete
     * Body: { "milestone_ids": [1, 2, 3] }
     */
    public function bulkDelete() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input || empty($input['milestone_ids'])) {
            Response::error('Dữ liệu không hợp lệ', 400);
        }
        
        $result = $this->milestoneService->bulkDelete($input['milestone_ids']);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success([
            'deleted_count' => $result['deleted_count']
        ], 'Xóa hàng loạt thành công');
    }
    
    /**
     * POST /admin/milestones/{id}/toggle-active
     */
    public function toggleActive($id) {
        $this->requireAdmin();
        
        if (empty($id) || !is_numeric($id)) {
            Response::error('ID mốc nạp không hợp lệ', 400);
        }
        
        $result = $this->milestoneService->toggleActive($id);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['milestone'], 'Cập nhật trạng thái thành công');
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