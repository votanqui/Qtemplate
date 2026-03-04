<?php
// controllers/admin/AdminTopRewardController.php

class AdminTopRewardController {
    private $adminTopRewardService;
    private $authService;
    
    public function __construct() {
        $this->adminTopRewardService = new AdminTopRewardService();
        $this->authService = new AuthService();
    }
    
    /**
     * Lấy danh sách tất cả events
     * GET /admin/events
     */
    public function getEvents() {
        $this->requireAdmin();
        
        $result = $this->adminTopRewardService->getEvents();
        
        Response::success($result, 'Lấy danh sách events thành công');
    }
    
    /**
     * Cập nhật thông tin event
     * PUT /admin/events/{id}
     */
    public function updateEvent($eventId) {
        $this->requireAdmin();
        
        if (empty($eventId) || !is_numeric($eventId)) {
            Response::error('ID event không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        // Validate dates nếu có
        if (isset($input['start_time']) && isset($input['end_time'])) {
            $startTime = strtotime($input['start_time']);
            $endTime = strtotime($input['end_time']);
            
            if ($startTime >= $endTime) {
                Response::error('Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc', 400);
            }
        }
        
        $result = $this->adminTopRewardService->updateEvent($eventId, $input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['event'], 'Cập nhật event thành công');
    }
    
    /**
     * Lấy danh sách phần thưởng của một event (nhóm theo top_type)
     * GET /admin/events/{id}/rewards
     */
    public function getEventRewards($eventId) {
        $this->requireAdmin();
        
        if (empty($eventId) || !is_numeric($eventId)) {
            Response::error('ID event không hợp lệ', 400);
        }
        
        $result = $this->adminTopRewardService->getEventRewards($eventId);
        
        if (!$result['success']) {
            Response::error($result['message'], 404);
        }
        
        Response::success($result, 'Lấy danh sách phần thưởng thành công');
    }
    
    /**
     * Lấy chi tiết một phần thưởng
     * GET /admin/rewards/{id}
     */
    public function getRewardDetail($rewardId) {
        $this->requireAdmin();
        
        if (empty($rewardId) || !is_numeric($rewardId)) {
            Response::error('ID phần thưởng không hợp lệ', 400);
        }
        
        $result = $this->adminTopRewardService->getRewardDetail($rewardId);
        
        if (!$result) {
            Response::notFound('Không tìm thấy phần thưởng');
        }
        
        Response::success($result, 'Lấy thông tin phần thưởng thành công');
    }
    
    /**
     * Thêm phần thưởng mới
     * POST /admin/rewards
     */
    public function createReward() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $eventId = $input['event_id'] ?? 0;
        $topType = $input['top_type'] ?? '';
        $rank = $input['rank'] ?? 0;
        $rewardDescription = $input['reward_description'] ?? '';
        
        // Validate required fields
        if (empty($eventId) || empty($topType) || empty($rank) || empty($rewardDescription)) {
            Response::error('Vui lòng điền đầy đủ thông tin (event_id, top_type, rank, reward_description)', 400);
        }
        
        // Validate top_type
        $validTopTypes = ['top_nap', 'top_level', 'top_boss', 'top_event'];
        if (!in_array($topType, $validTopTypes)) {
            Response::error('top_type không hợp lệ. Chỉ chấp nhận: ' . implode(', ', $validTopTypes), 400);
        }
        
        // Validate rank
        if (!is_numeric($rank) || $rank < 1) {
            Response::error('Rank phải là số nguyên dương', 400);
        }
        
        $result = $this->adminTopRewardService->createReward($eventId, $topType, $rank, $rewardDescription);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['reward'], 'Tạo phần thưởng thành công', 201);
    }
    
    /**
     * Cập nhật phần thưởng
     * PUT /admin/rewards/{id}
     */
    public function updateReward($rewardId) {
        $this->requireAdmin();
        
        if (empty($rewardId) || !is_numeric($rewardId)) {
            Response::error('ID phần thưởng không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        // Validate top_type nếu có
        if (isset($input['top_type'])) {
            $validTopTypes = ['top_nap', 'top_level', 'top_boss', 'top_event'];
            if (!in_array($input['top_type'], $validTopTypes)) {
                Response::error('top_type không hợp lệ. Chỉ chấp nhận: ' . implode(', ', $validTopTypes), 400);
            }
        }
        
        // Validate rank nếu có
        if (isset($input['rank']) && (!is_numeric($input['rank']) || $input['rank'] < 1)) {
            Response::error('Rank phải là số nguyên dương', 400);
        }
        
        $result = $this->adminTopRewardService->updateReward($rewardId, $input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['reward'], 'Cập nhật phần thưởng thành công');
    }
    
    /**
     * Xóa phần thưởng
     * DELETE /admin/rewards/{id}
     */
    public function deleteReward($rewardId) {
        $this->requireAdmin();
        
        if (empty($rewardId) || !is_numeric($rewardId)) {
            Response::error('ID phần thưởng không hợp lệ', 400);
        }
        
        $result = $this->adminTopRewardService->deleteReward($rewardId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success(null, $result['message']);
    }
    
    /**
     * Sao chép phần thưởng từ event này sang event khác
     * POST /admin/rewards/copy-by-event
     */
    public function copyRewardsByEvent() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $fromEventId = $input['from_event_id'] ?? null;
        $toEventId = $input['to_event_id'] ?? null;
        
        if (empty($fromEventId) || empty($toEventId)) {
            Response::error('Vui lòng cung cấp from_event_id và to_event_id', 400);
        }
        
        if (!is_numeric($fromEventId) || !is_numeric($toEventId)) {
            Response::error('Event ID phải là số', 400);
        }
        
        if ($fromEventId == $toEventId) {
            Response::error('Event nguồn và event đích không được giống nhau', 400);
        }
        
        $result = $this->adminTopRewardService->copyRewardsByEvent($fromEventId, $toEventId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result, 'Sao chép phần thưởng thành công');
    }
    
    /**
     * Cập nhật nhiều phần thưởng cùng lúc
     * PUT /admin/rewards/bulk-update
     */
    public function bulkUpdateRewards() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input || !isset($input['rewards']) || !is_array($input['rewards'])) {
            Response::error('Dữ liệu không hợp lệ. Cần cung cấp mảng rewards', 400);
        }
        
        if (empty($input['rewards'])) {
            Response::error('Danh sách rewards không được rỗng', 400);
        }
        
        if (count($input['rewards']) > 100) {
            Response::error('Chỉ được cập nhật tối đa 100 phần thưởng cùng lúc', 400);
        }
        
        $result = $this->adminTopRewardService->bulkUpdateRewards($input['rewards']);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result, 'Cập nhật hàng loạt thành công');
    }
    
    /**
     * Lấy thống kê phần thưởng
     * GET /admin/rewards/statistics?event_id=1
     */
    public function getRewardsStatistics() {
        $this->requireAdmin();
        
        $eventId = isset($_GET['event_id']) ? (int)$_GET['event_id'] : null;
        
        $result = $this->adminTopRewardService->getRewardsStatistics($eventId);
        
        Response::success([
            'statistics' => $result,
            'total' => count($result)
        ], 'Lấy thống kê thành công');
    }
    
    // ============================================
    // FINALIZE EVENT METHODS (MỚI)
    // ============================================
    
    /**
     * Xem trước kết quả trước khi chốt
     * GET /admin/events/{id}/preview-finalize?limit=50
     * 
     * Hỗ trợ cả 4 loại event: recharge, level, boss, event
     */
    public function previewFinalizeEvent($eventId) {
        $this->requireAdmin();
        
        if (empty($eventId) || !is_numeric($eventId)) {
            Response::error('ID event không hợp lệ', 400);
        }
        
        $limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 50;
        
        if ($limit < 1 || $limit > 200) {
            Response::error('Limit phải từ 1 đến 200', 400);
        }
        
        $result = $this->adminTopRewardService->previewFinalizeEvent($eventId, $limit);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result, 'Xem trước kết quả thành công');
    }
    
    /**
     * Chốt kết quả event
     * POST /admin/events/{id}/finalize
     * 
     * Body: {"confirmed": true}
     * 
     * Hỗ trợ cả 4 loại event:
     * - recharge: Lấy từ event_recharge
     * - level: Lấy từ tob_char
     * - boss: Lấy từ tob_sanboss
     * - event: Lấy từ tob_top_event
     */
    public function finalizeEvent($eventId) {
        $adminUserId = $this->requireAdmin();
        
        if (empty($eventId) || !is_numeric($eventId)) {
            Response::error('ID event không hợp lệ', 400);
        }
        
        // Require confirmation
        $input = json_decode(file_get_contents('php://input'), true);
        $confirmed = $input['confirmed'] ?? false;
        
        if (!$confirmed) {
            Response::error('Vui lòng xác nhận chốt kết quả bằng cách gửi {"confirmed": true}', 400);
        }
        
        $result = $this->adminTopRewardService->finalizeEvent($eventId, $adminUserId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result, 'Chốt kết quả event thành công');
    }
    
    /**
     * Lấy kết quả đã chốt
     * GET /admin/events/{id}/results?limit=50
     */
    public function getEventResults($eventId) {
        $this->requireAdmin();
        
        if (empty($eventId) || !is_numeric($eventId)) {
            Response::error('ID event không hợp lệ', 400);
        }
        
        $limit = isset($_GET['limit']) ? (int)$_GET['limit'] : 50;
        
        if ($limit < 1 || $limit > 200) {
            Response::error('Limit phải từ 1 đến 200', 400);
        }
        
        $result = $this->adminTopRewardService->getEventResults($eventId, $limit);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result, 'Lấy kết quả thành công');
    }
    
    /**
     * Đánh dấu người chơi đã nhận thưởng
     * PUT /admin/event-results/{id}/claim
     */
   public function markAsClaimed($resultId) {
        $this->requireAdmin();
        
        if (empty($resultId) || !is_numeric($resultId)) {
            Response::error('ID kết quả không hợp lệ', 400);
        }
        
        $result = $this->adminTopRewardService->markAsClaimed($resultId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['data'], 'Đánh dấu đã nhận thưởng thành công');
    }
    
    /**
     * Hủy đánh dấu đã nhận thưởng
     * PUT /admin/event-results/{id}/unclaim
     */
    public function markAsUnclaimed($resultId) {
        $this->requireAdmin();
        
        if (empty($resultId) || !is_numeric($resultId)) {
            Response::error('ID kết quả không hợp lệ', 400);
        }
        
        $result = $this->adminTopRewardService->markAsUnclaimed($resultId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['data'], 'Hủy đánh dấu thành công');
    }
    
    /**
     * Đánh dấu hàng loạt đã nhận thưởng
     * PUT /admin/event-results/bulk-claim
     * 
     * Body: {
     *   "result_ids": [1, 2, 3, 4, 5]
     * }
     */
    public function bulkMarkAsClaimed() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input || !isset($input['result_ids']) || !is_array($input['result_ids'])) {
            Response::error('Dữ liệu không hợp lệ. Cần cung cấp mảng result_ids', 400);
        }
        
        $result = $this->adminTopRewardService->bulkMarkAsClaimed($input['result_ids']);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['data'], $result['message']);
    }
    
    // ========== Helper Methods ==========
    
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