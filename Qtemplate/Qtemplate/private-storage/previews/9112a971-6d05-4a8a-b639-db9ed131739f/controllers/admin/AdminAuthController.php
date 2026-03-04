<?php
// controllers/admin/AdminAuthController.php

class AdminAuthController {
    private $adminService;
    private $authService;
    
    public function __construct() {
        $this->adminService = new AdminService();
        $this->authService = new AuthService();
    }
    
    /**
     * Lấy danh sách tất cả users với phân trang và tìm kiếm
     * GET /admin/users?page=1&limit=20&search=username&status=all
     */
    public function getUsers() {
        // Kiểm tra quyền admin
        $this->requireAdmin();
        
        $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
        $limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 20;
        $search = $_GET['search'] ?? '';
        $status = $_GET['status'] ?? 'all';
        $sortBy = $_GET['sort_by'] ?? 'regdate';
        $sortOrder = $_GET['sort_order'] ?? 'DESC';
        
        $result = $this->adminService->getUsers($page, $limit, $search, $status, $sortBy, $sortOrder);
        
        Response::success($result, 'Lấy danh sách người dùng thành công');
    }
    
    public function getUserDetail($userId) {
        $this->requireAdmin();
        
        if (empty($userId) || !is_numeric($userId)) {
            Response::error('ID người dùng không hợp lệ', 400);
        }
        
        $result = $this->adminService->getUserDetail($userId);
        
        if (!$result) {
            Response::notFound('Không tìm thấy người dùng');
        }
        
        Response::success($result, 'Lấy thông tin người dùng thành công');
    }
    
    public function updateUser($userId) {
        $this->requireAdmin();
        
        if (empty($userId) || !is_numeric($userId)) {
            Response::error('ID người dùng không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $result = $this->adminService->updateUser($userId, $input);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['user'], 'Cập nhật người dùng thành công');
    }
    
    public function toggleBan($userId) {
        $this->requireAdmin();
        
        if (empty($userId) || !is_numeric($userId)) {
            Response::error('ID người dùng không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        $action = $input['action'] ?? 'ban';
        $reason = $input['reason'] ?? '';
        
        $result = $this->adminService->toggleBan($userId, $action, $reason);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success([
            'user_id' => $userId,
            'banned' => $result['banned']
        ], $result['message']);
    }
    
    public function deleteUser($userId) {
        $this->requireAdmin();
        
        if (empty($userId) || !is_numeric($userId)) {
            Response::error('ID người dùng không hợp lệ', 400);
        }
        
        $currentUserId = $this->getCurrentUserId();
        if ($userId == $currentUserId) {
            Response::error('Không thể xóa chính mình', 403);
        }
        
        $result = $this->adminService->deleteUser($userId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success(null, 'Xóa người dùng thành công');
    }
    
    public function resetPassword($userId) {
        $this->requireAdmin();
        
        if (empty($userId) || !is_numeric($userId)) {
            Response::error('ID người dùng không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        $newPassword = $input['new_password'] ?? '';
        
        if (empty($newPassword) || strlen($newPassword) < 6) {
            Response::error('Mật khẩu mới phải ít nhất 6 ký tự', 400);
        }
        
        $result = $this->adminService->resetPassword($userId, $newPassword);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success(null, 'Reset mật khẩu thành công');
    }
    
    public function getStats() {
        $this->requireAdmin();
        
        $period = $_GET['period'] ?? 'day';
        
        $result = $this->adminService->getStats($period);
        
        Response::success($result, 'Lấy thống kê thành công');
    }
    
    public function getLoginHistory($userId) {
        $this->requireAdmin();
        
        if (empty($userId) || !is_numeric($userId)) {
            Response::error('ID người dùng không hợp lệ', 400);
        }
        
        $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
        $limit = isset($_GET['limit']) ? min(50, max(1, intval($_GET['limit']))) : 20;
        
        $result = $this->adminService->getLoginHistory($userId, $page, $limit);
        
        Response::success($result, 'Lấy lịch sử đăng nhập thành công');
    }
    
    public function kickSessions($userId) {
        $this->requireAdmin();
        
        if (empty($userId) || !is_numeric($userId)) {
            Response::error('ID người dùng không hợp lệ', 400);
        }
        
        $result = $this->authService->logoutAll($userId);
        
        if ($result) {
            Response::success(null, 'Đã kick tất cả phiên đăng nhập của người dùng');
        } else {
            Response::error('Kick session thất bại', 500);
        }
    }
    
    public function createUser() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $username = $input['username'] ?? '';
        $password = $input['password'] ?? '';
        $phone = $input['phone'] ?? '';
        $email = $input['email'] ?? null;
        $isAdmin = isset($input['isAdmin']) ? intval($input['isAdmin']) : 0;
        
        $result = $this->adminService->createUser($username, $password, $phone, $email, $isAdmin);
        
        if (!$result['success']) {
            Response::validationError($result['errors'] ?? ['message' => $result['message']]);
        }
        
        Response::success($result['user'], 'Tạo người dùng thành công', 201);
    }
    
    public function exportUsers() {
        $this->requireAdmin();
        
        $search = $_GET['search'] ?? '';
        $status = $_GET['status'] ?? 'all';
        
        $result = $this->adminService->exportUsers($search, $status);
        
        header('Content-Type: text/csv; charset=utf-8');
        header('Content-Disposition: attachment; filename="users_export_' . date('Y-m-d_His') . '.csv"');
        
        echo "\xEF\xBB\xBF";
        echo $result;
        exit;
    }
    
    public function getLogs() {
        $this->requireAdmin();
        
        $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
        $limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 50;
        $action = $_GET['action'] ?? 'all';
        $userId = $_GET['user_id'] ?? null;
        
        $result = $this->adminService->getLogs($page, $limit, $action, $userId);
        
        Response::success($result, 'Lấy logs thành công');
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
    
    private function getCurrentUserId() {
        $token = $this->getBearerToken();
        if (!$token) return null;
        return $this->authService->validateAccessToken($token);
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