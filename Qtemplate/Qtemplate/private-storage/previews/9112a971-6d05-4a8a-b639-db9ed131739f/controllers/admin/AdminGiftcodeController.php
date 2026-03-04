<?php
// controllers/admin/AdminGiftcodeController.php

class AdminGiftcodeController {
    private $giftcodeService;
    private $authService;
    
    public function __construct() {
        $this->giftcodeService = new AdminGiftcodeService();
        $this->authService = new AuthService();
    }
    
    /**
     * GET /admin/giftcodes?server_id=1&page=1&limit=20&search=&type=&status=
     */
    public function getGiftcodes() {
        $this->requireAdmin();
        
        // Server ID is required
        $serverId = $_GET['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
        $limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 20;
        $search = $_GET['search'] ?? '';
        $type = $_GET['type'] ?? 'all';
        $status = $_GET['status'] ?? 'all'; // all, active, expired, used_up
        
        $result = $this->giftcodeService->getGiftcodes($serverId, $page, $limit, $search, $type, $status);
        
        Response::success($result, 'Lấy danh sách giftcode thành công');
    }
    
    /**
     * GET /admin/giftcodes/{id}?server_id=1
     */
    public function getGiftcodeDetail($id) {
        $this->requireAdmin();
        
        $serverId = $_GET['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        if (empty($id) || !is_numeric($id)) {
            Response::error('ID giftcode không hợp lệ', 400);
        }
        
        $result = $this->giftcodeService->getGiftcodeDetail($serverId, $id);
        
        if (!$result) {
            Response::notFound('Không tìm thấy giftcode');
        }
        
        Response::success($result, 'Lấy thông tin giftcode thành công');
    }
    
    /**
     * POST /admin/giftcodes
     * Body: {
     *   "server_id": 1,
     *   "giftcode": "ABC123",
     *   "xu": 10000,
     *   "luong": 1000,
     *   "luongLock": 500,
     *   "items": [{"id": 123, "quantity": 1}],
     *   "expire": 1704067200,
     *   "limit_use": 100,
     *   "type": 0
     * }
     */
    public function createGiftcode() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $serverId = $input['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $result = $this->giftcodeService->createGiftcode($serverId, $input);
        
        if (!$result['success']) {
            Response::validationError($result['errors'] ?? ['message' => $result['message']]);
        }
        
        Response::success($result['giftcode'], 'Tạo giftcode thành công', 201);
    }
    
    /**
     * PUT /admin/giftcodes/{id}
     * Body: { "server_id": 1, ...update_data }
     */
    public function updateGiftcode($id) {
        $this->requireAdmin();
        
        if (empty($id) || !is_numeric($id)) {
            Response::error('ID giftcode không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $serverId = $input['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $result = $this->giftcodeService->updateGiftcode($serverId, $id, $input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['giftcode'], 'Cập nhật giftcode thành công');
    }
    
    /**
     * DELETE /admin/giftcodes/{id}?server_id=1
     */
    public function deleteGiftcode($id) {
        $this->requireAdmin();
        
        $serverId = $_GET['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        if (empty($id) || !is_numeric($id)) {
            Response::error('ID giftcode không hợp lệ', 400);
        }
        
        $result = $this->giftcodeService->deleteGiftcode($serverId, $id);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success(null, 'Xóa giftcode thành công');
    }
    
    /**
     * GET /admin/giftcodes/{id}/logs?server_id=1&page=1&limit=20
     */
    public function getGiftcodeUsageLog($id) {
        $this->requireAdmin();
        
        $serverId = $_GET['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        if (empty($id) || !is_numeric($id)) {
            Response::error('ID giftcode không hợp lệ', 400);
        }
        
        $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
        $limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 20;
        
        $result = $this->giftcodeService->getGiftcodeUsageLog($serverId, $id, $page, $limit);
        
        if (!$result) {
            Response::notFound('Không tìm thấy giftcode');
        }
        
        Response::success($result, 'Lấy lịch sử sử dụng thành công');
    }
    
    /**
     * GET /admin/giftcodes/stats?server_id=1
     */
    public function getGiftcodeStats() {
        $this->requireAdmin();
        
        $serverId = $_GET['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $result = $this->giftcodeService->getGiftcodeStats($serverId);
        
        Response::success($result, 'Lấy thống kê giftcode thành công');
    }
    
    /**
     * POST /admin/giftcodes/generate
     * Body: {
     *   "server_id": 1,
     *   "length": 10,
     *   "prefix": "EVENT"
     * }
     */
    public function generateCode() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        $serverId = $input['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $length = isset($input['length']) ? max(6, min(20, intval($input['length']))) : 10;
        $prefix = $input['prefix'] ?? '';
        
        $code = $this->giftcodeService->generateRandomCode($serverId, $length, $prefix);
        
        Response::success(['giftcode' => $code], 'Tạo mã thành công');
    }
    
    /**
     * POST /admin/giftcodes/batch-create
     * Body: {
     *   "server_id": 1,
     *   "count": 100,
     *   "prefix": "EVENT",
     *   "code_length": 10,
     *   "xu": 10000,
     *   "luong": 0,
     *   "luongLock": 0,
     *   "items": [],
     *   "expire": 0,
     *   "limit_use": 1,
     *   "type": 0
     * }
     */
    public function batchCreateGiftcodes() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input || empty($input['count'])) {
            Response::error('Số lượng không hợp lệ', 400);
        }
        
        $serverId = $input['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $count = max(1, min(1000, intval($input['count'])));
        
        $result = $this->giftcodeService->batchCreateGiftcodes($serverId, $count, $input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success([
            'created_count' => $result['created_count'],
            'giftcodes' => $result['giftcodes']
        ], 'Tạo hàng loạt thành công', 201);
    }
    
    /**
     * GET /admin/giftcodes/export?server_id=1&search=&type=&status=
     */
    public function exportGiftcodes() {
        $this->requireAdmin();
        
        $serverId = $_GET['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $search = $_GET['search'] ?? '';
        $type = $_GET['type'] ?? 'all';
        $status = $_GET['status'] ?? 'all';
        
        $result = $this->giftcodeService->exportGiftcodes($serverId, $search, $type, $status);
        
        header('Content-Type: text/csv; charset=utf-8');
        header('Content-Disposition: attachment; filename="giftcodes_server' . $serverId . '_export_' . date('Y-m-d_His') . '.csv"');
        
        echo "\xEF\xBB\xBF";
        echo $result;
        exit;
    }
    
    /**
     * POST /admin/giftcodes/bulk-delete
     * Body: { "server_id": 1, "giftcode_ids": [...] }
     */
    public function bulkDelete() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input || empty($input['giftcode_ids'])) {
            Response::error('Dữ liệu không hợp lệ', 400);
        }
        
        $serverId = $input['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $result = $this->giftcodeService->bulkDelete($serverId, $input['giftcode_ids']);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success([
            'deleted_count' => $result['deleted_count']
        ], 'Xóa hàng loạt thành công');
    }
    
    /**
     * GET /admin/servers
     * Lấy danh sách servers để chọn
     */
    public function getServers() {
        $this->requireAdmin();
        
        try {
            $servers = Database::getAllActiveServers();
            Response::success($servers, 'Lấy danh sách servers thành công');
        } catch (Exception $e) {
            Response::error('Không thể lấy danh sách servers: ' . $e->getMessage(), 500);
        }
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