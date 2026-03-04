<?php
// services/GiftcodeService.php

class GiftcodeService {
    
    /**
     * Get game database connection for specific server
     */
    private function getGameDb($serverId) {
        try {
            return Database::getGameInstance($serverId)->getConnection();
        } catch (Exception $e) {
            throw new Exception("Không thể kết nối database server $serverId: " . $e->getMessage());
        }
    }
    
    /**
     * Lấy danh sách giftcode công khai (chỉ active)
     */
    public function getPublicGiftcodes($serverId) {
        $db = $this->getGameDb($serverId);
        $currentTime = time();
        
        // Chỉ lấy giftcode còn hiệu lực
        $sql = "SELECT g.*, 
                COUNT(gl.id) as used_count,
                CASE 
                    WHEN g.expire > 0 AND g.expire <= ? THEN 'expired'
                    WHEN COUNT(gl.id) >= g.limit_use THEN 'used_up'
                    ELSE 'active'
                END as status
                FROM giftcode g
                LEFT JOIN giftcode_log gl ON g.giftcode = gl.giftcode
                GROUP BY g.id
                HAVING status = 'active'
                ORDER BY g.id DESC 
                LIMIT 100";
        
        $stmt = $db->prepare($sql);
        $stmt->execute([$currentTime]);
        $giftcodes = $stmt->fetchAll();
        
        // Format data
         foreach ($giftcodes as &$giftcode) {
            $giftcode['server_id'] = (int)$serverId;
            $giftcode['id'] = (int)$giftcode['id'];
            $giftcode['xu'] = (int)$giftcode['xu'];
            $giftcode['luong'] = (int)$giftcode['luong'];
            $giftcode['luongLock'] = (int)$giftcode['luongLock'];
            $giftcode['expire'] = (int)$giftcode['expire'];
            $giftcode['limit_use'] = (int)$giftcode['limit_use'];
            $giftcode['type'] = (int)$giftcode['type'];
            $giftcode['used_count'] = (int)$giftcode['used_count'];
            $giftcode['remaining'] = max(0, $giftcode['limit_use'] - $giftcode['used_count']);
            
            // THAY ĐỔI: Parse item từ comma-separated string
            if (!empty($giftcode['item'])) {
                // Split bằng dấu phẩy và loại bỏ khoảng trắng
                $items = array_map('trim', explode(',', $giftcode['item']));
                // Loại bỏ empty values và convert to lowercase
                $giftcode['items'] = array_map('strtolower', array_filter($items));
            } else {
                $giftcode['items'] = [];
            }
            
            // Don't expose item details in public API
            unset($giftcode['item']);
        }
        return [
            'server_id' => (int)$serverId,
            'giftcodes' => $giftcodes,
            'total' => count($giftcodes)
        ];
    }
    
    /**
     * Kiểm tra thông tin giftcode
     */
    public function checkGiftcode($serverId, $giftcodeStr) {
        $db = $this->getGameDb($serverId);
        $currentTime = time();
        
        $sql = "SELECT g.*, 
                COUNT(gl.id) as used_count
                FROM giftcode g
                LEFT JOIN giftcode_log gl ON g.giftcode = gl.giftcode
                WHERE g.giftcode = ?
                GROUP BY g.id";
        
        $stmt = $db->prepare($sql);
        $stmt->execute([$giftcodeStr]);
        $giftcode = $stmt->fetch();
        
        if (!$giftcode) {
            return null;
        }
        
        // Format data
        $giftcode['server_id'] = (int)$serverId;
        $giftcode['id'] = (int)$giftcode['id'];
        $giftcode['xu'] = (int)$giftcode['xu'];
        $giftcode['luong'] = (int)$giftcode['luong'];
        $giftcode['luongLock'] = (int)$giftcode['luongLock'];
        $giftcode['expire'] = (int)$giftcode['expire'];
        $giftcode['limit_use'] = (int)$giftcode['limit_use'];
        $giftcode['type'] = (int)$giftcode['type'];
        $giftcode['used_count'] = (int)$giftcode['used_count'];
        $giftcode['remaining'] = max(0, $giftcode['limit_use'] - $giftcode['used_count']);
        
        // Status
        if ($giftcode['expire'] > 0 && $giftcode['expire'] <= $currentTime) {
            $giftcode['status'] = 'expired';
        } elseif ($giftcode['used_count'] >= $giftcode['limit_use']) {
            $giftcode['status'] = 'used_up';
        } else {
            $giftcode['status'] = 'active';
        }
        
        // THAY ĐỔI: Parse items từ comma-separated string
        if (!empty($giftcode['item'])) {
            $items = array_map('trim', explode(',', $giftcode['item']));
            $giftcode['items'] = array_map('strtolower', array_filter($items));
        } else {
            $giftcode['items'] = [];
        }
        
        unset($giftcode['item']);
        
        return $giftcode;
    }
    
    /**
     * Sử dụng giftcode
     */
   public function useGiftcode($serverId, $userId, $giftcodeStr) {
        $db = $this->getGameDb($serverId);
        
        try {
            $db->beginTransaction();
            
            // Lấy thông tin giftcode
            $giftcode = $this->checkGiftcode($serverId, $giftcodeStr);
            
            if (!$giftcode) {
                $db->rollBack();
                return ['success' => false, 'message' => 'Giftcode không tồn tại'];
            }
            
            // Kiểm tra trạng thái
            if ($giftcode['status'] === 'expired') {
                $db->rollBack();
                return ['success' => false, 'message' => 'Giftcode đã hết hạn'];
            }
            
            if ($giftcode['status'] === 'used_up') {
                $db->rollBack();
                return ['success' => false, 'message' => 'Giftcode đã hết lượt sử dụng'];
            }
            
            // Kiểm tra user đã dùng chưa
            $stmt = $db->prepare("SELECT id FROM giftcode_log WHERE giftcode = ? AND id_user = ?");
            $stmt->execute([$giftcodeStr, $userId]);
            if ($stmt->fetch()) {
                $db->rollBack();
                return ['success' => false, 'message' => 'Bạn đã sử dụng giftcode này rồi'];
            }
            
            // THAY ĐỔI: Lưu log với comma-separated items
            $itemString = null;
            if (!empty($giftcode['items'])) {
                // Convert array thành comma-separated string
                $itemString = implode(',', $giftcode['items']);
            }
            
            $sql = "INSERT INTO giftcode_log (giftcode, xu, luong, luongK, item, id_user, type) 
                    VALUES (?, ?, ?, ?, ?, ?, ?)";
            
            $stmt = $db->prepare($sql);
            $stmt->execute([
                $giftcodeStr,
                $giftcode['xu'],
                $giftcode['luong'],
                $giftcode['luongLock'],
                $itemString,  // THAY ĐỔI: dùng string thay vì JSON
                $userId,
                $giftcode['type']
            ]);
            
            // Cộng xu, lượng cho user (nếu có)
            if ($giftcode['xu'] > 0 || $giftcode['luong'] > 0 || $giftcode['luongLock'] > 0) {
                $updateParts = [];
                if ($giftcode['xu'] > 0) {
                    $updateParts[] = "coin = coin + " . $giftcode['xu'];
                }
                if ($giftcode['luong'] > 0) {
                    $updateParts[] = "coin_lock = coin_lock + " . $giftcode['luong'];
                }
                if ($giftcode['luongLock'] > 0) {
                    $updateParts[] = "luongKhoa = luongKhoa + " . $giftcode['luongLock'];
                }
                
                if (!empty($updateParts)) {
                    $sql = "UPDATE player SET " . implode(', ', $updateParts) . " WHERE id = ?";
                    $stmt = $db->prepare($sql);
                    $stmt->execute([$userId]);
                }
            }
            
            // TODO: Xử lý thêm items vào túi đồ nếu có
            // Tùy thuộc vào cấu trúc database game
            
            $db->commit();
            
            return [
                'success' => true,
                'message' => 'Sử dụng giftcode thành công!',
                'data' => [
                    'xu' => $giftcode['xu'],
                    'luong' => $giftcode['luong'],
                    'luongLock' => $giftcode['luongLock'],
                    'items' => $giftcode['items']
                ]
            ];
            
        } catch (Exception $e) {
            $db->rollBack();
            return ['success' => false, 'message' => 'Có lỗi xảy ra: ' . $e->getMessage()];
        }
    }
}