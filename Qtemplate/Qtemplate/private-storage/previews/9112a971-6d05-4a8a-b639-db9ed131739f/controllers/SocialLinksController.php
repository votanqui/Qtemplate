<?php
// controllers/SocialLinksController.php

class SocialLinksController {
    private $db;
    
    public function __construct() {
        $this->db = Database::getInstance()->getConnection();
    }
    
    /**
     * Lấy danh sách social links (public - không cần auth)
     * GET /social-links
     */
    public function getSocialLinks() {
        try {
            $sql = "SELECT id, facebook_link, zalo_link FROM social_links ORDER BY created_at DESC";
            $stmt = $this->db->query($sql);
            $socialLinks = $stmt->fetchAll();
            
            Response::success([
                'social_links' => $socialLinks,
                'total' => count($socialLinks)
            ], 'Lấy danh sách social links thành công');
        } catch (PDOException $e) {
            Response::error('Lỗi khi lấy dữ liệu', 500);
        }
    }
    
    /**
     * Lấy social link mới nhất (public)
     * GET /social-links/latest
     */
    public function getLatestSocialLink() {
        try {
            $sql = "SELECT id, facebook_link, zalo_link FROM social_links ORDER BY created_at DESC LIMIT 1";
            $stmt = $this->db->query($sql);
            $socialLink = $stmt->fetch();
            
            if (!$socialLink) {
                Response::notFound('Không tìm thấy social link');
            }
            
            Response::success($socialLink, 'Lấy thông tin social link thành công');
        } catch (PDOException $e) {
            Response::error('Lỗi khi lấy dữ liệu', 500);
        }
    }
}