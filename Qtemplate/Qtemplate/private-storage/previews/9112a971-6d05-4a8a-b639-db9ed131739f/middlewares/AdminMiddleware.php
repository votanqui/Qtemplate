<?php
// middlewares/AdminMiddleware.php

class AdminMiddleware {
    
    /**
     * Kiểm tra user có phải admin không
     */
    public static function isAdmin($userId) {
        if (!$userId) {
            return false;
        }
        
        $db = Database::getInstance()->getConnection();
        $stmt = $db->prepare("SELECT isAdmin FROM team_user WHERE id = ? AND ban = 0");
        $stmt->execute([$userId]);
        $user = $stmt->fetch();
        
        return $user && $user['isAdmin'] == 1;
    }
    
    /**
     * Middleware để bảo vệ route admin
     */
    public static function requireAdmin($userId) {
        if (!self::isAdmin($userId)) {
            Response::forbidden('Bạn không có quyền truy cập tính năng này');
        }
    }
}