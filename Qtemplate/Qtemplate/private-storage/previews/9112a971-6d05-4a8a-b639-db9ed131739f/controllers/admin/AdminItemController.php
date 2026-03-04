<?php
// controllers/admin/AdminItemController.php

class AdminItemController {
    private $itemService;
    private $authService;
    
    public function __construct() {
        $this->itemService = new AdminItemService();
        $this->authService = new AuthService();
    }
    
    /**
     * GET /admin/items?server_id=1&page=1&limit=20&search=&type=&he=&gender=&level=
     */
    public function getItems() {
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
        $he = $_GET['he'] ?? 'all';
        $gender = $_GET['gender'] ?? 'all';
        $level = $_GET['level'] ?? 'all';
        $sortBy = $_GET['sort_by'] ?? 'id';
        $sortOrder = $_GET['sort_order'] ?? 'ASC';
        
        $result = $this->itemService->getItems($serverId, $page, $limit, $search, $type, $he, $gender, $level, $sortBy, $sortOrder);
        
        Response::success($result, 'Lấy danh sách items thành công');
    }
    
    /**
     * GET /admin/items/{id}?server_id=1
     */
    public function getItemDetail($itemId) {
        $this->requireAdmin();
        
        $serverId = $_GET['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        if (empty($itemId) || !is_numeric($itemId)) {
            Response::error('ID item không hợp lệ', 400);
        }
        
        $result = $this->itemService->getItemDetail($serverId, $itemId);
        
        if (!$result) {
            Response::notFound('Không tìm thấy item');
        }
        
        Response::success($result, 'Lấy thông tin item thành công');
    }
    
    /**
     * POST /admin/items
     * Body: { "server_id": 1, ...item_data }
     */
    public function createItem() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $serverId = $input['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $result = $this->itemService->createItem($serverId, $input);
        
        if (!$result['success']) {
            Response::validationError($result['errors'] ?? ['message' => $result['message']]);
        }
        
        Response::success($result['item'], 'Tạo item thành công', 201);
    }
    
    /**
     * PUT /admin/items/{id}
     * Body: { "server_id": 1, ...item_data }
     */
    public function updateItem($itemId) {
        $this->requireAdmin();
        
        if (empty($itemId) || !is_numeric($itemId)) {
            Response::error('ID item không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $serverId = $input['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $result = $this->itemService->updateItem($serverId, $itemId, $input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['item'], 'Cập nhật item thành công');
    }
    
    /**
     * DELETE /admin/items/{id}?server_id=1
     */
    public function deleteItem($itemId) {
        $this->requireAdmin();
        
        $serverId = $_GET['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        if (empty($itemId) || !is_numeric($itemId)) {
            Response::error('ID item không hợp lệ', 400);
        }
        
        $result = $this->itemService->deleteItem($serverId, $itemId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success(null, 'Xóa item thành công');
    }
    
    /**
     * POST /admin/items/{id}/clone
     * Body: { "server_id": 1 }
     */
    public function cloneItem($itemId) {
        $this->requireAdmin();
        
        if (empty($itemId) || !is_numeric($itemId)) {
            Response::error('ID item không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        $serverId = $input['server_id'] ?? null;
        
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $result = $this->itemService->cloneItem($serverId, $itemId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['item'], 'Sao chép item thành công', 201);
    }
    
    /**
     * GET /admin/items/stats?server_id=1
     */
    public function getItemStats() {
        $this->requireAdmin();
        
        $serverId = $_GET['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $result = $this->itemService->getItemStats($serverId);
        
        Response::success($result, 'Lấy thống kê items thành công');
    }
    
    /**
     * GET /admin/items/types
     * (Không cần server_id vì đây là danh sách cố định)
     */
    public function getItemTypes() {
        $this->requireAdmin();
        
        $result = $this->itemService->getItemTypes();
        
        Response::success($result, 'Lấy danh sách loại item thành công');
    }
    
    /**
     * POST /admin/items/bulk-update
     * Body: { "server_id": 1, "item_ids": [...], "updates": {...} }
     */
    public function bulkUpdate() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input || empty($input['item_ids']) || empty($input['updates'])) {
            Response::error('Dữ liệu không hợp lệ', 400);
        }
        
        $serverId = $input['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $result = $this->itemService->bulkUpdate($serverId, $input['item_ids'], $input['updates']);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success([
            'updated_count' => $result['updated_count']
        ], 'Cập nhật hàng loạt thành công');
    }
    
    /**
     * POST /admin/items/bulk-delete
     * Body: { "server_id": 1, "item_ids": [...] }
     */
    public function bulkDelete() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input || empty($input['item_ids'])) {
            Response::error('Dữ liệu không hợp lệ', 400);
        }
        
        $serverId = $input['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $result = $this->itemService->bulkDelete($serverId, $input['item_ids']);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success([
            'deleted_count' => $result['deleted_count']
        ], 'Xóa hàng loạt thành công');
    }
    
    /**
     * GET /admin/items/export?server_id=1
     */
    public function exportItems() {
        $this->requireAdmin();
        
        $serverId = $_GET['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        $search = $_GET['search'] ?? '';
        $type = $_GET['type'] ?? 'all';
        $he = $_GET['he'] ?? 'all';
        
        $result = $this->itemService->exportItems($serverId, $search, $type, $he);
        
        header('Content-Type: text/csv; charset=utf-8');
        header('Content-Disposition: attachment; filename="items_server' . $serverId . '_export_' . date('Y-m-d_His') . '.csv"');
        
        echo "\xEF\xBB\xBF";
        echo $result;
        exit;
    }
    
    /**
     * POST /admin/items/import?server_id=1
     */
    public function importItems() {
        $this->requireAdmin();
        
        $serverId = $_GET['server_id'] ?? null;
        if (!$serverId || !is_numeric($serverId)) {
            Response::error('server_id là bắt buộc', 400);
        }
        
        if (!isset($_FILES['file'])) {
            Response::error('Không tìm thấy file upload', 400);
        }
        
        $file = $_FILES['file'];
        
        if ($file['error'] !== UPLOAD_ERR_OK) {
            Response::error('Upload file thất bại', 400);
        }
        
        $result = $this->itemService->importItems($serverId, $file['tmp_name']);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success([
            'imported_count' => $result['imported_count'],
            'failed_count' => $result['failed_count'],
            'errors' => $result['errors']
        ], 'Import thành công');
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