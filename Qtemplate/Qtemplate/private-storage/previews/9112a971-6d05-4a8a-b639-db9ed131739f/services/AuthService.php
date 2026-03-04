<?php
// services/AuthService.php

class AuthService {
    private $db;
    
    public function __construct() {
        $this->db = Database::getInstance()->getConnection();
    }
    
public function register($username, $password, $phone, $email = null) {
    // Validate input
    $errors = [];
    
    if (empty($username) || strlen($username) < 3 || strlen($username) > 20) {
        $errors['username'] = 'Tên đăng nhập phải từ 3 đến 20 ký tự';
    }
    
    if (empty($password) || strlen($password) < 6) {
        $errors['password'] = 'Mật khẩu phải ít nhất 6 ký tự';
    }
    
    // Validate phone với hàm mới
    $phoneValidation = $this->validateAndNormalizePhone($phone);
    if (!$phoneValidation['valid']) {
        $errors['phone'] = $phoneValidation['message'];
    } else {
        $phone = $phoneValidation['normalized']; // Sử dụng số đã chuẩn hóa
    }
    
    // Validate email với hàm mới
    $emailValidation = $this->validateEmail($email);
    if (!$emailValidation['valid']) {
        $errors['email'] = $emailValidation['message'];
    }
    
    if (!empty($errors)) {
        return ['success' => false, 'errors' => $errors];
    }
    
    // Check if username exists
    $stmt = $this->db->prepare("SELECT id FROM team_user WHERE username = ?");
    $stmt->execute([$username]);
    
    if ($stmt->fetch()) {
        return ['success' => false, 'errors' => ['username' => 'Tên đăng nhập đã tồn tại']];
    }
    
    // Check if phone exists (với số đã chuẩn hóa)
    $stmt = $this->db->prepare("SELECT id FROM team_user WHERE phone = ?");
    $stmt->execute([$phone]);
    
    if ($stmt->fetch()) {
        return ['success' => false, 'errors' => ['phone' => 'Số điện thoại đã được sử dụng']];
    }
    
    // Check if email exists (nếu có email)
    if (!empty($email)) {
        $stmt = $this->db->prepare("SELECT id FROM team_user WHERE email = ?");
        $stmt->execute([$email]);
        
        if ($stmt->fetch()) {
            return ['success' => false, 'errors' => ['email' => 'Email đã được sử dụng']];
        }
    }
    
    // Hash password using MySQL PASSWORD() function format
    $hashedPassword = '*' . strtoupper(sha1(sha1($password, true)));
    
    // Insert user
    $stmt = $this->db->prepare("
        INSERT INTO team_user (username, password, phone, email, regdate, ip_addr) 
        VALUES (?, ?, ?, ?, NOW(), ?)
    ");
    
    $ipAddress = $_SERVER['REMOTE_ADDR'] ?? null;
    
    try {
        $stmt->execute([$username, $hashedPassword, $phone, $email, $ipAddress]);
        $userId = $this->db->lastInsertId();
        
        return [
            'success' => true,
            'user_id' => $userId,
            'username' => $username
        ];
    } catch (PDOException $e) {
        return ['success' => false, 'errors' => ['database' => 'Đăng ký thất bại']];
    }
}
    
    public function login($username, $password) {
        // Hash password
        $hashedPassword = '*' . strtoupper(sha1(sha1($password, true)));
        
        // Get user
        $stmt = $this->db->prepare("
            SELECT id, username, phone, email, ban, active, isAdmin 
            FROM team_user 
            WHERE username = ? AND password = ?
        ");
        
        $stmt->execute([$username, $hashedPassword]);
        $user = $stmt->fetch();
        
        if (!$user) {
            return ['success' => false, 'message' => 'sai tên đăng nhập hoặc mật khẩu'];
        }
        
        // Check if user is banned
        if ($user['ban'] == 1) {
            return ['success' => false, 'message' => 'tài khoản của bạn đã bị khóa'];
        }
        
        // Generate tokens
        $accessToken = JWTHelper::generateAccessToken($user['id']);
        $refreshToken = JWTHelper::generateRefreshToken($user['id']);
        
        // Save tokens to database
        $this->saveAuthTokens($user['id'], $accessToken, $refreshToken);
        
        return [
            'success' => true,          
            'tokens' => [
                'access_token' => $accessToken,
                'refresh_token' => $refreshToken,
                'expires_in' => Config::JWT_ACCESS_EXPIRY
            ]
        ];
    }
    
    public function logout($accessToken) {
        // Deactivate token
        $stmt = $this->db->prepare("
            UPDATE team_auth 
            SET is_active = 0, updated_at = NOW() 
            WHERE access_token = ?
        ");
        
        return $stmt->execute([$accessToken]);
    }
    
    public function logoutAll($userId) {
        // Logout all sessions for a user
        $stmt = $this->db->prepare("
            UPDATE team_auth 
            SET is_active = 0, updated_at = NOW() 
            WHERE user_id = ? AND is_active = 1
        ");
        
        return $stmt->execute([$userId]);
    }
    
    public function getActiveSessions($userId) {
        // Get all active sessions for a user
        $stmt = $this->db->prepare("
            SELECT 
                id,
                ip_address,
                user_agent,
                created_at,
                last_used,
                CASE 
                    WHEN last_used > DATE_SUB(NOW(), INTERVAL 5 MINUTE) THEN 'active'
                    WHEN last_used > DATE_SUB(NOW(), INTERVAL 1 HOUR) THEN 'recent'
                    ELSE 'idle'
                END as status
            FROM team_auth 
            WHERE user_id = ? AND is_active = 1
            ORDER BY last_used DESC
        ");
        
        $stmt->execute([$userId]);
        return $stmt->fetchAll();
    }
    
    public function revokeSession($userId, $sessionId) {
        // Revoke a specific session
        $stmt = $this->db->prepare("
            UPDATE team_auth 
            SET is_active = 0, updated_at = NOW() 
            WHERE id = ? AND user_id = ?
        ");
        
        return $stmt->execute([$sessionId, $userId]);
    }
    
    public function refreshToken($refreshToken) {
        // Validate refresh token
        $payload = JWTHelper::validateToken($refreshToken);
        
        if (!$payload || $payload['type'] !== 'refresh') {
            return ['success' => false, 'message' => 'Invalid refresh token'];
        }
        
        // Check if refresh token exists and is active
        $stmt = $this->db->prepare("
            SELECT user_id 
            FROM team_auth 
            WHERE refresh_token = ? AND is_active = 1
        ");
        
        $stmt->execute([$refreshToken]);
        $auth = $stmt->fetch();
        
        if (!$auth) {
            return ['success' => false, 'message' => 'Refresh token not found or inactive'];
        }
        
        // Generate new access token
        $newAccessToken = JWTHelper::generateAccessToken($auth['user_id']);
        
        // Update access token
        $stmt = $this->db->prepare("
            UPDATE team_auth 
            SET access_token = ?, access_token_expires = DATE_ADD(NOW(), INTERVAL ? SECOND), updated_at = NOW()
            WHERE refresh_token = ?
        ");
        
        $stmt->execute([$newAccessToken, Config::JWT_ACCESS_EXPIRY, $refreshToken]);
        
        return [
            'success' => true,
            'tokens' => [
                'access_token' => $newAccessToken,
                'expires_in' => Config::JWT_ACCESS_EXPIRY
            ]
        ];
    }
    public function changePassword($userId, $currentPassword, $newPassword) {
    // Validate new password
    if (strlen($newPassword) < 6) {
        return [
            'success' => false,
            'message' => 'Mật khẩu mới phải có ít nhất 6 ký tự'
        ];
    }
    
    if (strlen($newPassword) > 32) {
        return [
            'success' => false,
            'message' => 'Mật khẩu mới không được vượt quá 32 ký tự'
        ];
    }
    
    // Get current user data
    $stmt = $this->db->prepare("SELECT password FROM team_user WHERE id = ?");
    $stmt->execute([$userId]);
    $user = $stmt->fetch();
    
    if (!$user) {
        return [
            'success' => false,
            'message' => 'Không tìm thấy người dùng'
        ];
    }
    
    // Verify current password
    $hashedCurrentPassword = '*' . strtoupper(sha1(sha1($currentPassword, true)));
    
    if ($user['password'] !== $hashedCurrentPassword) {
        return [
            'success' => false,
            'message' => 'Mật khẩu hiện tại không đúng'
        ];
    }
    
    // Check if new password is same as current
    $hashedNewPassword = '*' . strtoupper(sha1(sha1($newPassword, true)));
    
    if ($user['password'] === $hashedNewPassword) {
        return [
            'success' => false,
            'message' => 'Mật khẩu mới phải khác mật khẩu hiện tại'
        ];
    }
    
    // Update password
    try {
        $stmt = $this->db->prepare("UPDATE team_user SET password = ? WHERE id = ?");
        $stmt->execute([$hashedNewPassword, $userId]);
        
        // Optional: Log out all other sessions for security
        // Uncomment if you want to force re-login after password change
        // $this->db->prepare("UPDATE team_auth SET is_active = 0 WHERE user_id = ?")
        //          ->execute([$userId]);
        
        return [
            'success' => true,
            'message' => 'Đổi mật khẩu thành công'
        ];
    } catch (PDOException $e) {
        return [
            'success' => false,
            'message' => 'Đổi mật khẩu thất bại'
        ];
    }
}
public function getUserInfo($userId) {
    try {
        // Lấy thông tin user
        $stmt = $this->db->prepare("
            SELECT id, username, phone, email, ban, active, isAdmin, regdate
            FROM team_user 
            WHERE id = ? 
            LIMIT 1
        ");
        $stmt->execute([$userId]);
        $user = $stmt->fetch();
        
        if (!$user) {
            return [
                'success' => false,
                'message' => 'Người dùng không tồn tại'
            ];
        }
        
        // Kiểm tra trạng thái kích hoạt từ game database
        $accountActive = $this->checkUserActivation($userId);
        
        return [
            'success' => true,
            'data' => [
                'id' => (int)$user['id'],
                'username' => $user['username'],
                'phone' => $user['phone'],
                'email' => $user['email'],
                'isAdmin' => (bool)$user['isAdmin'],
                'active' => (bool)$user['active'], // Trạng thái tài khoản (ban/unban)
                'account_active' => $accountActive, // Đã kích hoạt chưa (5h_active)
                'regdate' => $user['regdate']
            ]
        ];
        
    } catch (Exception $e) {
        return [
            'success' => false,
            'message' => 'Lỗi khi lấy thông tin: ' . $e->getMessage()
        ];
    }
}
private function checkUserActivation($userId, $serverId = 1) {
    try {
        // Lấy game database
        $gameDb = Database::getGameInstance($serverId)->getConnection();
        
        $stmt = $gameDb->prepare("
            SELECT COUNT(*) as count 
            FROM `5h_active` 
            WHERE `userID` = ? 
            LIMIT 1
        ");
        $stmt->execute([$userId]);
        $result = $stmt->fetch();
        
        return (int)$result['count'] > 0;
        
    } catch (Exception $e) {
        error_log("Check activation error: " . $e->getMessage());
        return false; // Trả về false nếu lỗi
    }
}
    public function validateAccessToken($accessToken) {
        $payload = JWTHelper::validateToken($accessToken);
        
        if (!$payload || $payload['type'] !== 'access') {
            http_response_code(401);
            return false;
        }
        
        // Check if token is active in database
        $stmt = $this->db->prepare("
            SELECT user_id 
            FROM team_auth 
            WHERE access_token = ? AND is_active = 1
        ");
        
        $stmt->execute([$accessToken]);
        
        if (!$stmt->fetch()) {
            return false;
        }
        
        // Update last used
        $stmt = $this->db->prepare("
            UPDATE team_auth 
            SET last_used = NOW() 
            WHERE access_token = ?
        ");
        $stmt->execute([$accessToken]);
        
        return $payload['user_id'];
    }
    // Thêm vào class AuthService

/**
 * Kiểm tra và chuẩn hóa số điện thoại
 * Tránh spam và kiểu nhập 012345679
 */
private function validateAndNormalizePhone($phone) {
    if (empty($phone)) {
        return ['valid' => false, 'message' => 'Số điện thoại là bắt buộc'];
    }
    
    // 1. Loại bỏ tất cả ký tự không phải số
    $phone = preg_replace('/[^0-9]/', '', $phone);
    
    // 2. Kiểm tra độ dài (10-11 số cho VN)
    if (strlen($phone) < 10 || strlen($phone) > 11) {
        return ['valid' => false, 'message' => 'Số điện thoại phải có 10-11 số'];
    }
    
    // 3. Kiểm tra đầu số Việt Nam hợp lệ
    $validPrefixes = [
        '03', '05', '07', '08', '09', // Mobifone, Vinaphone, Viettel
        '032', '033', '034', '035', '036', '037', '038', '039', // Viettel
        '070', '076', '077', '078', '079', // Mobifone
        '081', '082', '083', '084', '085', '086', '087', '088', // Vinaphone
        '056', '058', '059', // Vietnamobile
        '052', '055', // Vietnammobile
        '091', '094', // Vinaphone 3G
        '092', '093', // Vietnamobile 3G
        '096', '097', '098', // Viettel 4G
        '086' // Itelecom
    ];
    
    $prefix = substr($phone, 0, 2);
    $prefix3 = substr($phone, 0, 3);
    
    if (!in_array($prefix, $validPrefixes) && !in_array($prefix3, $validPrefixes)) {
        return ['valid' => false, 'message' => 'Đầu số không hợp lệ'];
    }
    
    // 4. Kiểm tra số trùng lặp (spam như 012345679)
    $digits = str_split($phone);
    
    // Kiểm tra chuỗi số tăng dần
    $isSequential = true;
    for ($i = 1; $i < count($digits); $i++) {
        if ((int)$digits[$i] != (int)$digits[$i-1] + 1) {
            $isSequential = false;
            break;
        }
    }
    
    if ($isSequential) {
        return ['valid' => false, 'message' => 'Số điện thoại không hợp lệ (số tăng dần)'];
    }
    
    // Kiểm tra số lặp lại
    $isRepeating = true;
    for ($i = 1; $i < count($digits); $i++) {
        if ($digits[$i] != $digits[0]) {
            $isRepeating = false;
            break;
        }
    }
    
    if ($isRepeating) {
        return ['valid' => false, 'message' => 'Số điện thoại không hợp lệ (số lặp lại)'];
    }
    
    // 5. Kiểm tra số dễ đoán (123456, 111111, etc.)
    $commonPatterns = [
        '1234567890', '0123456789', '0987654321',
        '1111111111', '2222222222', '3333333333',
        '4444444444', '5555555555', '6666666666',
        '7777777777', '8888888888', '9999999999',
        '0000000000'
    ];
    
    if (in_array($phone, $commonPatterns)) {
        return ['valid' => false, 'message' => 'Số điện thoại không hợp lệ (số dễ đoán)'];
    }
    
    // 6. Chuẩn hóa về định dạng 0xxxxxxxxx
    if (strpos($phone, '0') !== 0) {
        if (strpos($phone, '84') === 0) {
            $phone = '0' . substr($phone, 2);
        } else {
            $phone = '0' . $phone;
        }
    }
    
    // Giới hạn 10 số
    if (strlen($phone) > 10) {
        $phone = substr($phone, 0, 10);
    }
    
    return [
        'valid' => true, 
        'normalized' => $phone,
        'message' => 'Số điện thoại hợp lệ'
    ];
}

/**
 * Kiểm tra email hợp lệ và không spam
 */
private function validateEmail($email) {
    if (empty($email)) {
        return ['valid' => true, 'message' => 'Email có thể để trống']; // Email optional
    }
    
    // 1. Validate format cơ bản
    if (!filter_var($email, FILTER_VALIDATE_EMAIL)) {
        return ['valid' => false, 'message' => 'Email không hợp lệ'];
    }
    
    // 2. Kiểm tra domain spam/temporary
    $spamDomains = [
        'tempmail.com', '10minutemail.com', 'guerrillamail.com',
        'mailinator.com', 'yopmail.com', 'trashmail.com',
        'disposablemail.com', 'fakeinbox.com', 'throwawaymail.com',
        'temp-mail.org', 'getairmail.com', 'meltmail.com',
        'sharklasers.com', 'grr.la', 'guerrillamail.net'
    ];
    
    $domain = strtolower(substr(strrchr($email, "@"), 1));
    
    foreach ($spamDomains as $spamDomain) {
        if (strpos($domain, $spamDomain) !== false) {
            return ['valid' => false, 'message' => 'Email tạm thời không được chấp nhận'];
        }
    }
    
    // 3. Kiểm tra username spam (chứa nhiều số random)
    $username = strtok($email, '@');
    $digitCount = preg_match_all('/\d/', $username);
    $totalLength = strlen($username);
    
    // Nếu username chứa quá nhiều số (> 50%)
    if ($totalLength > 0 && ($digitCount / $totalLength) > 0.5) {
        return ['valid' => false, 'message' => 'Email không hợp lệ (chứa quá nhiều số)'];
    }
    
    // 4. Kiểm tra pattern dễ đoán
    $commonPatterns = [
        'test@', 'admin@', 'user@', 'demo@',
        '123456@', 'abcdef@', 'qwerty@'
    ];
    
    foreach ($commonPatterns as $pattern) {
        if (stripos($email, $pattern) === 0) {
            return ['valid' => false, 'message' => 'Email không hợp lệ (email test)'];
        }
    }
    
    return ['valid' => true, 'message' => 'Email hợp lệ'];
}
    private function saveAuthTokens($userId, $accessToken, $refreshToken) {
        $ipAddress = $_SERVER['REMOTE_ADDR'] ?? null;
        $userAgent = $_SERVER['HTTP_USER_AGENT'] ?? null;
        
        // Kiểm tra xem user đã có session active chưa
        $stmtCheck = $this->db->prepare("
            SELECT id FROM team_auth 
            WHERE user_id = ? AND is_active = 1 
            LIMIT 1
        ");
        $stmtCheck->execute([$userId]);
        $existingSession = $stmtCheck->fetch();
        
        if ($existingSession) {
            // CẬP NHẬT session cũ thay vì tạo mới
            $stmt = $this->db->prepare("
                UPDATE team_auth SET
                    access_token = ?,
                    refresh_token = ?,
                    access_token_expires = DATE_ADD(NOW(), INTERVAL ? SECOND),
                    refresh_token_expires = DATE_ADD(NOW(), INTERVAL ? SECOND),
                    ip_address = ?,
                    user_agent = ?,
                    is_active = 1,
                    updated_at = NOW(),
                    last_used = NOW()
                WHERE user_id = ? AND is_active = 1
            ");
            
            $stmt->execute([
                $accessToken,
                $refreshToken,
                Config::JWT_ACCESS_EXPIRY,
                Config::JWT_REFRESH_EXPIRY,
                $ipAddress,
                $userAgent,
                $userId
            ]);
        } else {
            // Tạo session mới nếu chưa có
            $stmt = $this->db->prepare("
                INSERT INTO team_auth (
                    user_id, access_token, refresh_token, 
                    access_token_expires, refresh_token_expires,
                    ip_address, user_agent, is_active, last_used
                ) VALUES (
                    ?, ?, ?, 
                    DATE_ADD(NOW(), INTERVAL ? SECOND),
                    DATE_ADD(NOW(), INTERVAL ? SECOND),
                    ?, ?, 1, NOW()
                )
            ");
            
            $stmt->execute([
                $userId, 
                $accessToken, 
                $refreshToken,
                Config::JWT_ACCESS_EXPIRY,
                Config::JWT_REFRESH_EXPIRY,
                $ipAddress,
                $userAgent
            ]);
        }
    }
}