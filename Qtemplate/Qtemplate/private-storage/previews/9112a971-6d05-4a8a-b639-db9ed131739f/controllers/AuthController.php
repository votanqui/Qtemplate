<?php
// controllers/AuthController.php

// Include middleware classes
require_once __DIR__ . '/../middlewares/RateLimitMiddleware.php';
require_once __DIR__ . '/../services/AuthService.php';
require_once __DIR__ . '/../helpers/Response.php';

class AuthController {
    private $authService;
    
    public function __construct() {
        $this->authService = new AuthService();
    }
    
    public function register() {
        // Kiểm tra và áp dụng giới hạn tốc độ
        $this->applyRateLimit('/auth/register', 'authStrict');
        
        // Lấy dữ liệu JSON đầu vào
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $username = $input['username'] ?? '';
        $password = $input['password'] ?? '';
        $phone = $input['phone'] ?? '';
        $email = $input['email'] ?? null;
        
        $result = $this->authService->register($username, $password, $phone, $email);
        
        if (!$result['success']) {
            Response::validationError($result['errors']);
        }
        
        Response::success([
            'user_id' => $result['user_id'],
            'username' => $result['username']
        ], 'Đăng ký thành công', 201);
    }
    public function changePassword() {
    // Apply rate limiting
    $this->applyRateLimit('/auth/change-password', 'authStrict');
    
    // Get and validate token
    $token = $this->getBearerToken();
    
    if (!$token) {
        Response::unauthorized('Yêu cầu mã truy cập');
    }
    
    $userId = $this->authService->validateAccessToken($token);
    
    if (!$userId) {
        Response::unauthorized('Mã không hợp lệ hoặc đã hết hạn');
    }
    
    // Get input data
    $input = json_decode(file_get_contents('php://input'), true);
    
    if (!$input) {
        Response::error('Dữ liệu JSON không hợp lệ', 400);
    }
    
    $currentPassword = $input['current_password'] ?? '';
    $newPassword = $input['new_password'] ?? '';
    $confirmPassword = $input['confirm_password'] ?? '';
    
    // Validate input
    $errors = [];
    
    if (empty($currentPassword)) {
        $errors['current_password'] = 'Mật khẩu hiện tại là bắt buộc';
    }
    
    if (empty($newPassword)) {
        $errors['new_password'] = 'Mật khẩu mới là bắt buộc';
    }
    
    if (empty($confirmPassword)) {
        $errors['confirm_password'] = 'Xác nhận mật khẩu là bắt buộc';
    }
    
    if ($newPassword !== $confirmPassword) {
        $errors['confirm_password'] = 'Mật khẩu xác nhận không khớp';
    }
    
    if (!empty($errors)) {
        Response::validationError($errors);
    }
    
    // Change password
    $result = $this->authService->changePassword($userId, $currentPassword, $newPassword);
    
    if (!$result['success']) {
        Response::error($result['message'], 400);
    }
    
    Response::success(null, $result['message']);
}
    public function login() {
        // Kiểm tra và áp dụng giới hạn tốc độ
        $this->applyRateLimit('/auth/login', 'authStrict');
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $username = $input['username'] ?? '';
        $password = $input['password'] ?? '';
        
        if (empty($username) || empty($password)) {
            Response::validationError([
                'username' => 'Tên đăng nhập là bắt buộc',
                'password' => 'Mật khẩu là bắt buộc'
            ]);
        }
        
        $result = $this->authService->login($username, $password);
        
        if (!$result['success']) {
            Response::unauthorized($result['message']);
        }
        
        Response::success([
            'tokens' => $result['tokens']
        ], 'Đăng nhập thành công');
    }
    
    public function logout() {
        $token = $this->getBearerToken();
        
        if (!$token) {
            Response::unauthorized('Yêu cầu mã truy cập');
        }
        
        $userId = $this->authService->validateAccessToken($token);
        
        if (!$userId) {
            Response::unauthorized('Mã không hợp lệ hoặc đã hết hạn');
        }
        
        $this->authService->logout($token);
        
        Response::success(null, 'Đăng xuất thành công');
    }
    
    public function logoutAll() {
        $token = $this->getBearerToken();
        
        if (!$token) {
            Response::unauthorized('Yêu cầu mã truy cập');
        }
        
        $userId = $this->authService->validateAccessToken($token);
        
        if (!$userId) {
            Response::unauthorized('Mã không hợp lệ hoặc đã hết hạn');
        }
        
        $this->authService->logoutAll($userId);
        
        Response::success(null, 'Đã đăng xuất tất cả các phiên');
    }
    
    public function sessions() {
        $token = $this->getBearerToken();
        
        if (!$token) {
            Response::unauthorized('Yêu cầu mã truy cập');
        }
        
        $userId = $this->authService->validateAccessToken($token);
        
        if (!$userId) {
            Response::unauthorized('Mã không hợp lệ hoặc đã hết hạn');
        }
        
        $sessions = $this->authService->getActiveSessions($userId);
        
        Response::success([
            'total' => count($sessions),
            'sessions' => $sessions
        ], 'Đã lấy danh sách phiên đang hoạt động');
    }
    
    public function revokeSession() {
        $token = $this->getBearerToken();
        
        if (!$token) {
            Response::unauthorized('Yêu cầu mã truy cập');
        }
        
        $userId = $this->authService->validateAccessToken($token);
        
        if (!$userId) {
            Response::unauthorized('Mã không hợp lệ hoặc đã hết hạn');
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        $sessionId = $input['session_id'] ?? null;
        
        if (!$sessionId) {
            Response::error('Yêu cầu ID phiên', 400);
        }
        
        $this->authService->revokeSession($userId, $sessionId);
        
        Response::success(null, 'Đã thu hồi phiên thành công');
    }
    
    public function refresh() {
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input || empty($input['refresh_token'])) {
            Response::error('Yêu cầu mã làm mới', 400);
        }
        
        $result = $this->authService->refreshToken($input['refresh_token']);
        
        if (!$result['success']) {
            Response::unauthorized($result['message']);
        }
        
        Response::success($result['tokens'], 'Đã làm mới mã thành công');
    }
    
   public function me() {
    // Kiểm tra và áp dụng giới hạn tốc độ
    $this->applyRateLimit('/auth/me', 'apiModerate');
    
    $token = $this->getBearerToken();
    
    if (!$token) {
        Response::unauthorized('Yêu cầu mã truy cập');
    }
    
    $userId = $this->authService->validateAccessToken($token);
    
    if (!$userId) {
        Response::unauthorized('Mã không hợp lệ hoặc đã hết hạn');
    }
    
    // Lấy thông tin người dùng từ AuthService (đã có method getUserInfo)
    $userInfo = $this->authService->getUserInfo($userId);
    
    if (!$userInfo['success']) {
        Response::notFound('Không tìm thấy người dùng');
    }
    
    Response::success($userInfo['data'], 'Đã lấy thông tin người dùng');
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
    
    /**
     * Áp dụng rate limiting với xử lý lỗi
     */
    private function applyRateLimit($route, $method) {
        try {
            // Kiểm tra xem class RateLimitMiddleware có tồn tại không
            if (class_exists('RateLimitMiddleware')) {
                // Gọi phương thức tương ứng
                if (method_exists('RateLimitMiddleware', $method)) {
                    call_user_func(['RateLimitMiddleware', $method], $route);
                } else {
                    // Nếu phương thức không tồn tại, ghi log và tiếp tục (không block request)
                    error_log("RateLimitMiddleware method {$method} not found");
                }
            } else {
                // Nếu class không tồn tại, ghi log và tiếp tục (không block request)
                error_log("RateLimitMiddleware class not found, skipping rate limiting");
            }
        } catch (Exception $e) {
            // Ghi log lỗi nhưng không làm gián đoạn request
            error_log("Rate limit error: " . $e->getMessage());
        }
    }
}