<?php
// services/CharacterService.php

class CharacterService {
    private $accountDb;
    private $gameDbs = [];
    
    public function __construct() {
        $this->accountDb = Database::getInstance()->getConnection();
    }
    
    /**
     * Lấy danh sách nhân vật của user theo userId
     * @param int $userId User ID
     * @param int $serverId Server ID (mặc định = 1)
     * @return array
     */
    public function getUserCharacters($userId, $serverId = 1) {
        try {
            // Lấy game database
            $gameDb = $this->getGameDb($serverId);
            
            // Query lấy danh sách nhân vật
            $stmt = $gameDb->prepare("
                SELECT 
                    id,
                    charname,
                    userId,
                    xp,
                    lastLv,
                    hp,
                    mp,
                    map,
                    gold,
                    headStyle,
                    class,
                    gender,
                    luong,
                    luonglock,
                    strength,
                    agitity,
                    spirit,
                    dexterity,
                    luck,
                    basepoint,
                    skillpoint,
                    killer,
                    nPKill,
                    totalTimePlay,
                    lastLog,
                    idClan,
                    del,
                    ban,
                    isAdmin,
                    topNap,
                    tichluy,
                    tichluy_bosung,
                    tichluy_tuan
                FROM tob_char 
                WHERE userId = ? 
                AND del = 1
                ORDER BY lastLog DESC
            ");
            
            $stmt->execute([$userId]);
            $characters = $stmt->fetchAll();
            
            if (empty($characters)) {
                return [
                    'success' => true,
                    'data' => [
                        'total' => 0,
                        'characters' => []
                    ],
                    'message' => 'Người chơi chưa có nhân vật nào'
                ];
            }
            
            // Format dữ liệu
            $formattedCharacters = [];
            foreach ($characters as $char) {
                $formattedCharacters[] = $this->formatCharacterData($char);
            }
            
            return [
                'success' => true,
                'data' => [
                    'total' => count($formattedCharacters),
                    'characters' => $formattedCharacters
                ],
                'message' => 'Lấy danh sách nhân vật thành công'
            ];
            
        } catch (Exception $e) {
            error_log("Get characters error: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy danh sách nhân vật: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Lấy thông tin chi tiết 1 nhân vật
     * @param int $userId User ID
     * @param string $charname Tên nhân vật
     * @param int $serverId Server ID
     * @return array
     */
    public function getCharacterDetail($userId, $charname, $serverId = 1) {
        try {
            $gameDb = $this->getGameDb($serverId);
            
            $stmt = $gameDb->prepare("
                SELECT * FROM tob_char 
                WHERE userId = ? 
                AND charname = ? 
                AND del = 1
                LIMIT 1
            ");
            
            $stmt->execute([$userId, $charname]);
            $char = $stmt->fetch();
            
            if (!$char) {
                return [
                    'success' => false,
                    'message' => 'Không tìm thấy nhân vật'
                ];
            }
            
            return [
                'success' => true,
                'data' => $this->formatCharacterData($char, true),
                'message' => 'Lấy thông tin nhân vật thành công'
            ];
            
        } catch (Exception $e) {
            error_log("Get character detail error: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy thông tin nhân vật: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Lấy thống kê nhân vật của user
     * @param int $userId User ID
     * @param int $serverId Server ID
     * @return array
     */
    public function getUserCharacterStats($userId, $serverId = 1) {
        try {
            $gameDb = $this->getGameDb($serverId);
            
            $stmt = $gameDb->prepare("
                SELECT 
                    COUNT(*) as total_characters,
                    SUM(CASE WHEN ban = 0 THEN 1 ELSE 0 END) as active_characters,
                    SUM(CASE WHEN ban = 1 THEN 1 ELSE 0 END) as banned_characters,
                    MAX(lastLv) as highest_level,
                    SUM(gold) as total_gold,
                    SUM(luong) as total_luong,
                    SUM(luonglock) as total_luonglock,
                    AVG(lastLv) as average_level,
                    MAX(xp) as max_xp,
                    SUM(nPKill) as total_pk_kills,
                    SUM(totalTimePlay) as total_playtime,
                    MAX(lastLog) as last_login
                FROM tob_char 
                WHERE userId = ? 
                AND del = 1
            ");
            
            $stmt->execute([$userId]);
            $stats = $stmt->fetch();
            
            return [
                'success' => true,
                'data' => [
                    'total_characters' => (int)$stats['total_characters'],
                    'active_characters' => (int)$stats['active_characters'],
                    'banned_characters' => (int)$stats['banned_characters'],
                    'highest_level' => (int)$stats['highest_level'],
                    'total_gold' => (int)$stats['total_gold'],
                    'total_luong' => (int)$stats['total_luong'],
                    'total_luonglock' => (int)$stats['total_luonglock'],
                    'average_level' => round((float)$stats['average_level'], 2),
                    'max_experience' => (int)$stats['max_xp'],
                    'total_pk_kills' => (int)$stats['total_pk_kills'],
                    'total_playtime_minutes' => (int)$stats['total_playtime'],
                    'last_login' => $stats['last_login']
                ],
                'message' => 'Lấy thống kê nhân vật thành công'
            ];
            
        } catch (Exception $e) {
            error_log("Get character stats error: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy thống kê: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Format dữ liệu nhân vật
     * @param array $char Raw character data
     * @param bool $detailed Có lấy thông tin chi tiết không
     * @return array
     */
    private function formatCharacterData($char, $detailed = false) {
        $baseData = [
            'id' => (int)$char['id'],
            'name' => $char['charname'],
            'user_id' => (int)$char['userId'],
            'level' => (int)$char['lastLv'],
            'experience' => (int)$char['xp'],
            'hp' => (int)$char['hp'],
            'mp' => (int)$char['mp'],
            'gold' => (int)$char['gold'],
            'luong' => (int)$char['luong'],
            'luong_lock' => (int)$char['luonglock'],
            'class' => $this->getClassName((int)$char['class']),
            'class_id' => (int)$char['class'],
            'gender' => $this->getGenderName((int)$char['gender']),
            'gender_id' => (int)$char['gender'],
            'head_style' => (int)$char['headStyle'],
            'map_id' => (int)$char['map'],
            'clan_id' => (int)$char['idClan'],
            'is_banned' => (bool)$char['ban'],
            'is_admin' => (bool)$char['isAdmin'],
            'pk_kills' => (int)$char['nPKill'],
            'killer_status' => (int)$char['killer'],
            'last_login' => $char['lastLog'],
            'total_playtime_minutes' => (int)$char['totalTimePlay'],
            'top_recharge' => (int)$char['topNap']
        ];
        
        // Nếu cần thông tin chi tiết
        if ($detailed) {
            $baseData['stats'] = [
                'strength' => (int)$char['strength'],
                'agility' => (int)$char['agitity'],
                'spirit' => (int)$char['spirit'],
                'dexterity' => (int)$char['dexterity'],
                'luck' => (int)$char['luck'],
                'base_points' => (int)$char['basepoint'],
                'skill_points' => (int)$char['skillpoint']
            ];
            
            $baseData['accumulation'] = [
                'total' => (int)$char['tichluy'],
                'bonus' => (int)$char['tichluy_bosung'],
                'weekly' => (int)$char['tichluy_tuan']
            ];
            
            // Parse skill data nếu có
            if (!empty($char['skill'])) {
                $baseData['skills'] = $this->parseSkillData($char['skill']);
            }
            
            // Parse equipment nếu có
            if (!empty($char['equip'])) {
                $baseData['equipment'] = $this->parseEquipmentData($char['equip']);
            }
        }
        
        return $baseData;
    }
    
    /**
     * Lấy tên class
     */
    private function getClassName($classId) {
        $classes = [
            0 => 'Kiếm Khách',
            1 => 'Đao Khách',
            2 => 'Kunai',
            3 => 'Cung Thủ',
            4 => 'Tiêu Khách',
            5 => 'Quạt Khách'
        ];
        
        return $classes[$classId] ?? 'Unknown';
    }
    
    /**
     * Lấy tên giới tính
     */
    private function getGenderName($genderId) {
        return $genderId == 0 ? 'Nam' : 'Nữ';
    }
    
    /**
     * Parse skill data từ string
     */
    private function parseSkillData($skillString) {
        // TODO: Implement skill parsing based on game logic
        return $skillString;
    }
    
    /**
     * Parse equipment data từ string
     */
    private function parseEquipmentData($equipString) {
        // TODO: Implement equipment parsing based on game logic
        return $equipString;
    }
    
    /**
     * Lấy kết nối game database
     */
    private function getGameDb($serverId = 1) {
        if (!isset($this->gameDbs[$serverId])) {
            $dbInstance = Database::getGameInstance($serverId);
            $this->gameDbs[$serverId] = $dbInstance->getConnection();
        }
        
        return $this->gameDbs[$serverId];
    }
}