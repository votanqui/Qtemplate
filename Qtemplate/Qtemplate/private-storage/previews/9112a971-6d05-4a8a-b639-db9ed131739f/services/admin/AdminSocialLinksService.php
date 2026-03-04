<?php
// services/admin/AdminSocialLinksService.php

class AdminSocialLinksService {
    private $db;
    
    public function __construct() {
        $this->db = Database::getInstance()->getConnection();
    }
    
    /**
     * Lấy tất cả social links
     */
    public function getSocialLinks() {
        $sql = "SELECT * FROM social_links ORDER BY created_at DESC";
        $stmt = $this->db->query($sql);
        return $stmt->fetchAll();
    }
    
    /**
     * Lấy chi tiết social link
     */
    public function getSocialLinkDetail($id) {
        $sql = "SELECT * FROM social_links WHERE id = ?";
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$id]);
        return $stmt->fetch();
    }
    
    /**
     * Tạo social link mới
     */
    public function createSocialLink($data) {
        // Validate required fields
        if (empty($data['facebook_link']) || empty($data['zalo_link'])) {
            return ['success' => false, 'message' => 'Facebook link và Zalo link là bắt buộc'];
        }
        
        $sql = "INSERT INTO social_links (facebook_link, zalo_link) VALUES (?, ?)";
        
        try {
            $stmt = $this->db->prepare($sql);
            $stmt->execute([
                $data['facebook_link'],
                $data['zalo_link']
            ]);
            
            $id = $this->db->lastInsertId();
            
            return [
                'success' => true,
                'social_link' => $this->getSocialLinkDetail($id)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Tạo social link thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Cập nhật social link
     */
    public function updateSocialLink($id, $data) {
        $updateFields = [];
        $params = [];
        
        if (isset($data['facebook_link'])) {
            $updateFields[] = "facebook_link = ?";
            $params[] = $data['facebook_link'];
        }
        
        if (isset($data['zalo_link'])) {
            $updateFields[] = "zalo_link = ?";
            $params[] = $data['zalo_link'];
        }
        
        if (empty($updateFields)) {
            return ['success' => false, 'message' => 'Không có dữ liệu để cập nhật'];
        }
        
        $params[] = $id;
        $sql = "UPDATE social_links SET " . implode(', ', $updateFields) . " WHERE id = ?";
        
        try {
            $stmt = $this->db->prepare($sql);
            $stmt->execute($params);
            
            return [
                'success' => true,
                'social_link' => $this->getSocialLinkDetail($id)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Cập nhật thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Xóa social link
     */
    public function deleteSocialLink($id) {
        try {
            $sql = "DELETE FROM social_links WHERE id = ?";
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$id]);
            
            return ['success' => true];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Xóa thất bại: ' . $e->getMessage()];
        }
    }
}