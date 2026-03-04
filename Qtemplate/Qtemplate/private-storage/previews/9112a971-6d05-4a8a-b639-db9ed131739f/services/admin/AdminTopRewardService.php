<?php
// services/AdminTopRewardService.php

class AdminTopRewardService {
    private $db;
    
    public function __construct() {
        $this->db = Database::getInstance()->getConnection();
    }
    
    /**
     * Lấy danh sách tất cả events
     * GET /admin/events
     */
    public function getEvents() {
        $sql = "SELECT 
                    e.id,
                    e.event_code,
                    e.event_name,
                    e.event_type,
                    e.description,
                    e.start_time,
                    e.end_time,
                    e.is_active,
                    e.is_finished,
                    e.finished_at,
                    e.finished_by,
                    e.created_at,
                    e.updated_at,
                    COUNT(DISTINCT tr.id) as total_rewards,
                    COUNT(DISTINCT er.id) as total_results
                FROM events e
                LEFT JOIN top_rewards_new tr ON e.id = tr.event_id
                LEFT JOIN event_results er ON e.id = er.event_id
                GROUP BY e.id
                ORDER BY e.id DESC";
        
        $stmt = $this->db->query($sql);
        $events = $stmt->fetchAll();
        
        return [
            'events' => $events,
            'total' => count($events)
        ];
    }
    
    /**
     * Lấy chi tiết một event
     */
    public function getEventDetail($eventId) {
        $sql = "SELECT 
                    e.*,
                    COUNT(DISTINCT tr.id) as total_rewards,
                    COUNT(DISTINCT er.id) as total_results
                FROM events e
                LEFT JOIN top_rewards_new tr ON e.id = tr.event_id
                LEFT JOIN event_results er ON e.id = er.event_id
                WHERE e.id = ?
                GROUP BY e.id";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$eventId]);
        
        return $stmt->fetch();
    }
    
    /**
     * Cập nhật thông tin event
     * PUT /admin/events/{id}
     */
    public function updateEvent($eventId, $data) {
        try {
            $allowedFields = [
                'event_name', 
                'event_type', 
                'description', 
                'start_time', 
                'end_time', 
                'is_active'
            ];
            
            $updateFields = [];
            $params = [];
            
            foreach ($allowedFields as $field) {
                if (isset($data[$field])) {
                    $updateFields[] = "$field = ?";
                    $params[] = $data[$field];
                }
            }
            
            if (empty($updateFields)) {
                return ['success' => false, 'message' => 'Không có dữ liệu để cập nhật'];
            }
            
            $params[] = $eventId;
            
            $sql = "UPDATE events SET " . implode(', ', $updateFields) . " WHERE id = ?";
            $stmt = $this->db->prepare($sql);
            $stmt->execute($params);
            
            return [
                'success' => true,
                'event' => $this->getEventDetail($eventId)
            ];
            
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Cập nhật thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Lấy danh sách phần thưởng của một event (nhóm theo top_type)
     * GET /admin/events/{id}/rewards
     */
    public function getEventRewards($eventId) {
        // Kiểm tra event tồn tại
        $event = $this->getEventDetail($eventId);
        if (!$event) {
            return ['success' => false, 'message' => 'Event không tồn tại'];
        }
        
        $sql = "SELECT 
                    id,
                    event_id,
                    top_type,
                    rank,
                    reward_description,
                    created_at,
                    updated_at
                FROM top_rewards_new
                WHERE event_id = ?
                ORDER BY 
                    FIELD(top_type, 'top_nap', 'top_level', 'top_boss', 'top_event'),
                    rank ASC";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$eventId]);
        $rewards = $stmt->fetchAll();
        
        // Nhóm theo top_type
        $grouped = [
            'top_nap' => [],
            'top_level' => [],
            'top_boss' => [],
            'top_event' => []
        ];
        
        foreach ($rewards as $reward) {
            $topType = $reward['top_type'];
            if (isset($grouped[$topType])) {
                $grouped[$topType][] = $reward;
            }
        }
        
        return [
            'success' => true,
            'event' => [
                'id' => $event['id'],
                'event_code' => $event['event_code'],
                'event_name' => $event['event_name'],
                'event_type' => $event['event_type'],
                'start_time' => $event['start_time'],
                'end_time' => $event['end_time'],
                'is_active' => $event['is_active'],
                'is_finished' => $event['is_finished']
            ],
            'rewards_by_type' => $grouped,
            'total_rewards' => count($rewards)
        ];
    }
    
    /**
     * Lấy chi tiết một phần thưởng
     * GET /admin/rewards/{id}
     */
    public function getRewardDetail($rewardId) {
        $sql = "SELECT 
                    tr.*,
                    e.event_code,
                    e.event_name,
                    e.event_type,
                    e.start_time,
                    e.end_time,
                    e.is_active,
                    e.is_finished
                FROM top_rewards_new tr
                INNER JOIN events e ON tr.event_id = e.id
                WHERE tr.id = ?";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$rewardId]);
        
        return $stmt->fetch();
    }
    
    /**
     * Thêm phần thưởng mới
     * POST /admin/rewards
     */
    public function createReward($eventId, $topType, $rank, $rewardDescription) {
        try {
            // Validate top_type
            $validTopTypes = ['top_nap', 'top_level', 'top_boss', 'top_event'];
            if (!in_array($topType, $validTopTypes)) {
                return ['success' => false, 'message' => 'Loại top không hợp lệ'];
            }
            
            // Kiểm tra event tồn tại
            $event = $this->getEventDetail($eventId);
            if (!$event) {
                return ['success' => false, 'message' => 'Event không tồn tại'];
            }
            
            // Kiểm tra duplicate rank trong cùng event và top_type
            $stmt = $this->db->prepare("
                SELECT id FROM top_rewards_new 
                WHERE event_id = ? AND top_type = ? AND rank = ?
            ");
            $stmt->execute([$eventId, $topType, $rank]);
            
            if ($stmt->fetch()) {
                return ['success' => false, 'message' => 'Rank này đã tồn tại trong loại top này'];
            }
            
            // Thêm phần thưởng mới
            $sql = "INSERT INTO top_rewards_new (event_id, top_type, rank, reward_description) 
                    VALUES (?, ?, ?, ?)";
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$eventId, $topType, $rank, $rewardDescription]);
            
            $rewardId = $this->db->lastInsertId();
            
            return [
                'success' => true,
                'reward' => $this->getRewardDetail($rewardId)
            ];
            
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Tạo phần thưởng thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Cập nhật phần thưởng
     * PUT /admin/rewards/{id}
     */
    public function updateReward($rewardId, $data) {
        try {
            // Validate top_type nếu có
            if (isset($data['top_type'])) {
                $validTopTypes = ['top_nap', 'top_level', 'top_boss', 'top_event'];
                if (!in_array($data['top_type'], $validTopTypes)) {
                    return ['success' => false, 'message' => 'Loại top không hợp lệ'];
                }
            }
            
            $allowedFields = ['top_type', 'rank', 'reward_description'];
            $updateFields = [];
            $params = [];
            
            foreach ($allowedFields as $field) {
                if (isset($data[$field])) {
                    $updateFields[] = "$field = ?";
                    $params[] = $data[$field];
                }
            }
            
            if (empty($updateFields)) {
                return ['success' => false, 'message' => 'Không có dữ liệu để cập nhật'];
            }
            
            // Nếu cập nhật rank hoặc top_type, kiểm tra duplicate
            if (isset($data['rank']) || isset($data['top_type'])) {
                $currentReward = $this->getRewardDetail($rewardId);
                if (!$currentReward) {
                    return ['success' => false, 'message' => 'Phần thưởng không tồn tại'];
                }
                
                $checkRank = $data['rank'] ?? $currentReward['rank'];
                $checkTopType = $data['top_type'] ?? $currentReward['top_type'];
                
                $stmt = $this->db->prepare("
                    SELECT id FROM top_rewards_new 
                    WHERE event_id = ? AND top_type = ? AND rank = ? AND id != ?
                ");
                $stmt->execute([
                    $currentReward['event_id'], 
                    $checkTopType, 
                    $checkRank, 
                    $rewardId
                ]);
                
                if ($stmt->fetch()) {
                    return ['success' => false, 'message' => 'Rank này đã tồn tại trong loại top này'];
                }
            }
            
            $params[] = $rewardId;
            
            $sql = "UPDATE top_rewards_new SET " . implode(', ', $updateFields) . " WHERE id = ?";
            $stmt = $this->db->prepare($sql);
            $stmt->execute($params);
            
            return [
                'success' => true,
                'reward' => $this->getRewardDetail($rewardId)
            ];
            
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Cập nhật thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Xóa phần thưởng
     * DELETE /admin/rewards/{id}
     */
    public function deleteReward($rewardId) {
        try {
            $sql = "DELETE FROM top_rewards_new WHERE id = ?";
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$rewardId]);
            
            if ($stmt->rowCount() === 0) {
                return ['success' => false, 'message' => 'Phần thưởng không tồn tại'];
            }
            
            return ['success' => true, 'message' => 'Xóa phần thưởng thành công'];
            
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Xóa thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Sao chép phần thưởng từ event này sang event khác
     * POST /admin/rewards/copy-by-event
     */
    public function copyRewardsByEvent($fromEventId, $toEventId) {
        try {
            $this->db->beginTransaction();
            
            // Kiểm tra cả 2 event có tồn tại không
            $fromEvent = $this->getEventDetail($fromEventId);
            $toEvent = $this->getEventDetail($toEventId);
            
            if (!$fromEvent) {
                $this->db->rollBack();
                return ['success' => false, 'message' => 'Event nguồn không tồn tại'];
            }
            
            if (!$toEvent) {
                $this->db->rollBack();
                return ['success' => false, 'message' => 'Event đích không tồn tại'];
            }
            
            // Xóa rewards cũ của event đích (nếu có)
            $stmt = $this->db->prepare("DELETE FROM top_rewards_new WHERE event_id = ?");
            $stmt->execute([$toEventId]);
            
            // Sao chép rewards
            $sql = "INSERT INTO top_rewards_new (event_id, top_type, rank, reward_description)
                    SELECT ?, top_type, rank, reward_description
                    FROM top_rewards_new
                    WHERE event_id = ?
                    ORDER BY 
                        FIELD(top_type, 'top_nap', 'top_level', 'top_boss', 'top_event'),
                        rank ASC";
            
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$toEventId, $fromEventId]);
            
            $copiedCount = $stmt->rowCount();
            
            $this->db->commit();
            
            return [
                'success' => true,
                'message' => "Đã sao chép {$copiedCount} phần thưởng từ '{$fromEvent['event_name']}' sang '{$toEvent['event_name']}'",
                'copied_count' => $copiedCount,
                'from_event' => [
                    'id' => $fromEvent['id'],
                    'name' => $fromEvent['event_name']
                ],
                'to_event' => [
                    'id' => $toEvent['id'],
                    'name' => $toEvent['event_name']
                ]
            ];
            
        } catch (PDOException $e) {
            $this->db->rollBack();
            return ['success' => false, 'message' => 'Sao chép thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Cập nhật nhiều phần thưởng cùng lúc
     * PUT /admin/rewards/bulk-update
     */
    public function bulkUpdateRewards($rewards) {
        try {
            $this->db->beginTransaction();
            
            $updatedCount = 0;
            $errors = [];
            
            foreach ($rewards as $reward) {
                if (!isset($reward['id']) || !isset($reward['reward_description'])) {
                    $errors[] = "Missing required fields for reward ID: " . ($reward['id'] ?? 'unknown');
                    continue;
                }
                
                $sql = "UPDATE top_rewards_new SET reward_description = ? WHERE id = ?";
                $stmt = $this->db->prepare($sql);
                
                if ($stmt->execute([$reward['reward_description'], $reward['id']])) {
                    $updatedCount++;
                } else {
                    $errors[] = "Failed to update reward ID: " . $reward['id'];
                }
            }
            
            $this->db->commit();
            
            return [
                'success' => true,
                'message' => "Đã cập nhật {$updatedCount} phần thưởng",
                'updated_count' => $updatedCount,
                'total_requested' => count($rewards),
                'errors' => $errors
            ];
            
        } catch (PDOException $e) {
            $this->db->rollBack();
            return ['success' => false, 'message' => 'Cập nhật hàng loạt thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Thống kê phần thưởng
     */
    public function getRewardsStatistics($eventId = null) {
        if ($eventId) {
            $sql = "SELECT 
                        top_type,
                        COUNT(*) as total_rewards,
                        MIN(rank) as min_rank,
                        MAX(rank) as max_rank
                    FROM top_rewards_new
                    WHERE event_id = ?
                    GROUP BY top_type
                    ORDER BY FIELD(top_type, 'top_nap', 'top_level', 'top_boss', 'top_event')";
            
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$eventId]);
        } else {
            $sql = "SELECT 
                        e.id as event_id,
                        e.event_name,
                        e.event_type,
                        tr.top_type,
                        COUNT(tr.id) as total_rewards,
                        MIN(tr.rank) as min_rank,
                        MAX(tr.rank) as max_rank
                    FROM events e
                    LEFT JOIN top_rewards_new tr ON e.id = tr.event_id
                    GROUP BY e.id, tr.top_type
                    ORDER BY e.id DESC, FIELD(tr.top_type, 'top_nap', 'top_level', 'top_boss', 'top_event')";
            
            $stmt = $this->db->query($sql);
        }
        
        return $stmt->fetchAll();
    }
    
    // ============================================
    // FINALIZE METHODS (MỚI THÊM)
    // ============================================
    
    /**
     * Xem trước kết quả trước khi chốt - HỖ TRỢ CẢ 4 LOẠI EVENT
     */
    public function previewFinalizeEvent($eventId, $limit = 50) {
        try {
            $event = $this->getEventDetail($eventId);
            if (!$event) {
                return ['success' => false, 'message' => 'Event không tồn tại'];
            }
            
            $eventType = $event['event_type'];
            
            $preview = [];
            switch ($eventType) {
                case 'recharge':
                    $preview = $this->previewRechargeEvent($eventId, $limit);
                    break;
                case 'level':
                    $preview = $this->previewLevelEvent($eventId, $limit);
                    break;
                case 'boss':
                    $preview = $this->previewBossEvent($eventId, $limit);
                    break;
                case 'event':
                    $preview = $this->previewGameEvent($eventId, $limit);
                    break;
                default:
                    return ['success' => false, 'message' => 'Loại event không hợp lệ'];
            }
            
            if (empty($preview)) {
                return ['success' => false, 'message' => 'Event này chưa có dữ liệu'];
            }
            
            return [
                'success' => true,
                'event' => [
                    'id' => $event['id'],
                    'event_code' => $event['event_code'],
                    'event_name' => $event['event_name'],
                    'event_type' => $event['event_type'],
                    'start_time' => $event['start_time'],
                    'end_time' => $event['end_time'],
                    'is_finished' => (bool)$event['is_finished']
                ],
                'total_players' => count($preview),
                'preview' => $preview
            ];
            
        } catch (Exception $e) {
            error_log("Preview finalize error: " . $e->getMessage());
            return ['success' => false, 'message' => 'Lỗi khi xem trước: ' . $e->getMessage()];
        }
    }
    
    private function previewRechargeEvent($eventId, $limit) {
        $sql = "SELECT 
                    user_id,
                    username as player_name,
                    server_id,
                    total_amount,
                    total_xu,
                    total_luong,
                    total_luong_khoa,
                    total_recharge
                FROM event_recharge
                WHERE event_id = ?
                AND username != 'chienthan'
                ORDER BY total_amount DESC
                LIMIT ?";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$eventId, $limit]);
        $data = $stmt->fetchAll(PDO::FETCH_ASSOC);
        
        $rewards = $this->getRewardsMap($eventId, 'top_nap');
        
        $preview = [];
        foreach ($data as $index => $row) {
            $rank = $index + 1;
            $preview[] = [
                'rank' => $rank,
                'user_id' => $row['user_id'],
                'player_name' => $row['player_name'],
                'server_id' => $row['server_id'],
                'total_amount' => (float)$row['total_amount'],
                'formatted_amount' => number_format($row['total_amount'], 0, ',', '.') . ' VNĐ',
                'total_xu' => (int)$row['total_xu'],
                'total_luong' => (int)$row['total_luong'],
                'total_luong_khoa' => (int)$row['total_luong_khoa'],
                'total_recharge' => (int)$row['total_recharge'],
                'reward_description' => $rewards[$rank] ?? 'Không có phần thưởng'
            ];
        }
        
        return $preview;
    }
    
    private function previewLevelEvent($eventId, $limit) {
        $gameDb = $this->getGameDb(1);
        
        $sql = "SELECT 
                    userId as user_id,
                    charname as player_name,
                    xp
                FROM tob_char 
                WHERE ban = '0' 
                AND charname != 'chienthan' 
                ORDER BY xp DESC 
                LIMIT ?";
        
        $stmt = $gameDb->prepare($sql);
        $stmt->execute([$limit]);
        $data = $stmt->fetchAll(PDO::FETCH_ASSOC);
        
        $rewards = $this->getRewardsMap($eventId, 'top_level');
        
        $preview = [];
        foreach ($data as $index => $row) {
            $rank = $index + 1;
            $level = $this->getLevelFromExp($row['xp']);
            
            $preview[] = [
                'rank' => $rank,
                'user_id' => $row['user_id'],
                'player_name' => $row['player_name'],
                'server_id' => 1,
                'level' => $level,
                'xp' => (int)$row['xp'],
                'reward_description' => $rewards[$rank] ?? 'Không có phần thưởng'
            ];
        }
        
        return $preview;
    }
    
    private function previewBossEvent($eventId, $limit) {
        $gameDb = $this->getGameDb(1);
        
        $sql = "SELECT 
                    player as player_name,
                    win as boss_kills
                FROM tob_sanboss 
                WHERE player != 'chienthan' 
                ORDER BY win DESC 
                LIMIT ?";
        
        $stmt = $gameDb->prepare($sql);
        $stmt->execute([$limit]);
        $data = $stmt->fetchAll(PDO::FETCH_ASSOC);
        
        $rewards = $this->getRewardsMap($eventId, 'top_boss');
        
        $preview = [];
        foreach ($data as $index => $row) {
            $rank = $index + 1;
            
            $preview[] = [
                'rank' => $rank,
                'user_id' => null,
                'player_name' => $row['player_name'],
                'server_id' => 1,
                'boss_kills' => (int)$row['boss_kills'],
                'reward_description' => $rewards[$rank] ?? 'Không có phần thưởng'
            ];
        }
        
        return $preview;
    }
    
    private function previewGameEvent($eventId, $limit) {
        $gameDb = $this->getGameDb(1);
        
        $sql = "SELECT 
                    charname as player_name,
                    point1 as event_points
                FROM tob_top_event 
                ORDER BY point1 DESC 
                LIMIT ?";
        
        $stmt = $gameDb->prepare($sql);
        $stmt->execute([$limit]);
        $data = $stmt->fetchAll(PDO::FETCH_ASSOC);
        
        $rewards = $this->getRewardsMap($eventId, 'top_event');
        
        $preview = [];
        foreach ($data as $index => $row) {
            $rank = $index + 1;
            
            $preview[] = [
                'rank' => $rank,
                'user_id' => null,
                'player_name' => $row['player_name'],
                'server_id' => 1,
                'event_points' => (int)$row['event_points'],
                'reward_description' => $rewards[$rank] ?? 'Không có phần thưởng'
            ];
        }
        
        return $preview;
    }
    
    /**
     * Chốt kết quả event - HỖ TRỢ CẢ 4 LOẠI EVENT
     */
    public function finalizeEvent($eventId, $adminUserId) {
        try {
            $this->db->beginTransaction();
            
            $event = $this->getEventDetail($eventId);
            if (!$event) {
                $this->db->rollBack();
                return ['success' => false, 'message' => 'Event không tồn tại'];
            }
            
            if ($event['is_finished'] == 1) {
                $this->db->rollBack();
                return ['success' => false, 'message' => 'Event này đã được chốt trước đó'];
            }
            
            $eventType = $event['event_type'];
            $savedCount = 0;
            
            switch ($eventType) {
                case 'recharge':
                    $savedCount = $this->finalizeRechargeEvent($eventId);
                    break;
                case 'level':
                    $savedCount = $this->finalizeLevelEvent($eventId);
                    break;
                case 'boss':
                    $savedCount = $this->finalizeBossEvent($eventId);
                    break;
                case 'event':
                    $savedCount = $this->finalizeGameEvent($eventId);
                    break;
                default:
                    $this->db->rollBack();
                    return ['success' => false, 'message' => 'Loại event không hợp lệ'];
            }
            
            if ($savedCount === 0) {
                $this->db->rollBack();
                return ['success' => false, 'message' => 'Event này chưa có dữ liệu'];
            }
            
            $updateEventSql = "UPDATE events 
                              SET is_finished = 1, 
                                  finished_at = NOW(),
                                  finished_by = ?
                              WHERE id = ?";
            $updateEventStmt = $this->db->prepare($updateEventSql);
            $updateEventStmt->execute([$adminUserId, $eventId]);
            
            $this->db->commit();
            
            return [
                'success' => true,
                'message' => "Đã chốt kết quả event '{$event['event_name']}' thành công",
                'event_id' => $eventId,
                'event_name' => $event['event_name'],
                'event_type' => $eventType,
                'total_saved' => $savedCount,
                'finalized_at' => date('Y-m-d H:i:s')
            ];
            
        } catch (Exception $e) {
            $this->db->rollBack();
            error_log("Finalize event error: " . $e->getMessage());
            return ['success' => false, 'message' => 'Lỗi khi chốt kết quả: ' . $e->getMessage()];
        }
    }
    
    private function finalizeRechargeEvent($eventId) {
        $sql = "SELECT 
                    user_id,
                    username as player_name,
                    server_id,
                    total_amount,
                    total_xu,
                    total_luong,
                    total_luong_khoa,
                    total_recharge
                FROM event_recharge
                WHERE event_id = ?
                AND username != 'chienthan'
                ORDER BY total_amount DESC";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$eventId]);
        $data = $stmt->fetchAll(PDO::FETCH_ASSOC);
        
        if (empty($data)) return 0;
        
        $rewards = $this->getRewardsMap($eventId, 'top_nap');
        
        $insertSql = "INSERT INTO event_results 
                     (event_id, user_id, player_name, server_id, rank, 
                      total_amount, total_xu, total_luong, total_luong_khoa, total_recharge,
                      reward_description, is_claimed, saved_at)
                     VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 0, NOW())";
        
        $insertStmt = $this->db->prepare($insertSql);
        
        $savedCount = 0;
        foreach ($data as $index => $row) {
            $rank = $index + 1;
            $rewardDesc = $rewards[$rank] ?? 'Không có phần thưởng';
            
            $insertStmt->execute([
                $eventId,
                $row['user_id'],
                $row['player_name'],
                $row['server_id'],
                $rank,
                $row['total_amount'],
                $row['total_xu'],
                $row['total_luong'],
                $row['total_luong_khoa'],
                $row['total_recharge'],
                $rewardDesc
            ]);
            
            $savedCount++;
        }
        
        return $savedCount;
    }
    
    private function finalizeLevelEvent($eventId) {
        $gameDb = $this->getGameDb(1);
        
        $sql = "SELECT 
                    userId as user_id,
                    charname as player_name,
                    xp
                FROM tob_char 
                WHERE ban = '0' 
                AND charname != 'chienthan' 
                ORDER BY xp DESC 
                LIMIT 100";
        
        $stmt = $gameDb->prepare($sql);
        $stmt->execute();
        $data = $stmt->fetchAll(PDO::FETCH_ASSOC);
        
        if (empty($data)) return 0;
        
        $rewards = $this->getRewardsMap($eventId, 'top_level');
        
        $insertSql = "INSERT INTO event_results 
                     (event_id, user_id, player_name, server_id, rank, 
                      level, xp, reward_description, is_claimed, saved_at)
                     VALUES (?, ?, ?, ?, ?, ?, ?, ?, 0, NOW())";
        
        $insertStmt = $this->db->prepare($insertSql);
        
        $savedCount = 0;
        foreach ($data as $index => $row) {
            $rank = $index + 1;
            $level = $this->getLevelFromExp($row['xp']);
            $rewardDesc = $rewards[$rank] ?? 'Không có phần thưởng';
            
            $insertStmt->execute([
                $eventId,
                $row['user_id'],
                $row['player_name'],
                1,
                $rank,
                $level,
                $row['xp'],
                $rewardDesc
            ]);
            
            $savedCount++;
        }
        
        return $savedCount;
    }
    
    private function finalizeBossEvent($eventId) {
        $gameDb = $this->getGameDb(1);
        
        $sql = "SELECT 
                    player as player_name,
                    win as boss_kills
                FROM tob_sanboss 
                WHERE player != 'chienthan' 
                ORDER BY win DESC 
                LIMIT 100";
        
        $stmt = $gameDb->prepare($sql);
        $stmt->execute();
        $data = $stmt->fetchAll(PDO::FETCH_ASSOC);
        
        if (empty($data)) return 0;
        
        $rewards = $this->getRewardsMap($eventId, 'top_boss');
        
        $insertSql = "INSERT INTO event_results 
                     (event_id, user_id, player_name, server_id, rank, 
                      boss_kills, reward_description, is_claimed, saved_at)
                     VALUES (?, NULL, ?, ?, ?, ?, ?, 0, NOW())";
        
        $insertStmt = $this->db->prepare($insertSql);
        
        $savedCount = 0;
        foreach ($data as $index => $row) {
            $rank = $index + 1;
            $rewardDesc = $rewards[$rank] ?? 'Không có phần thưởng';
            
            $insertStmt->execute([
                $eventId,
                $row['player_name'],
                1,
                $rank,
                $row['boss_kills'],
                $rewardDesc
            ]);
            
            $savedCount++;
        }
        
        return $savedCount;
    }
    
    private function finalizeGameEvent($eventId) {
        $gameDb = $this->getGameDb(1);
        
        $sql = "SELECT 
                    charname as player_name,
                    point1 as event_points
                FROM tob_top_event 
                ORDER BY point1 DESC 
                LIMIT 100";
        
        $stmt = $gameDb->prepare($sql);
        $stmt->execute();
        $data = $stmt->fetchAll(PDO::FETCH_ASSOC);
        
        if (empty($data)) return 0;
        
        $rewards = $this->getRewardsMap($eventId, 'top_event');
        
        $insertSql = "INSERT INTO event_results 
                     (event_id, user_id, player_name, server_id, rank, 
                      event_points, reward_description, is_claimed, saved_at)
                     VALUES (?, NULL, ?, ?, ?, ?, ?, 0, NOW())";
        
        $insertStmt = $this->db->prepare($insertSql);
        
        $savedCount = 0;
        foreach ($data as $index => $row) {
            $rank = $index + 1;
            $rewardDesc = $rewards[$rank] ?? 'Không có phần thưởng';
            
            $insertStmt->execute([
                $eventId,
                $row['player_name'],
                1,
                $rank,
                $row['event_points'],
                $rewardDesc
            ]);
            
            $savedCount++;
        }
        
        return $savedCount;
    }
    
    /**
     * Lấy kết quả đã chốt từ event_results
     */
    public function getEventResults($eventId, $limit = 50) {
        try {
            $event = $this->getEventDetail($eventId);
            if (!$event) {
                return ['success' => false, 'message' => 'Event không tồn tại'];
            }
            
            if (!$event['is_finished']) {
                return ['success' => false, 'message' => 'Event này chưa được chốt'];
            }
            
            $sql = "SELECT 
                        id,
                        user_id,
                        player_name,
                        server_id,
                        rank,
                        total_amount,
                        total_xu,
                        total_luong,
                        total_luong_khoa,
                        total_recharge,
                        level,
                        xp,
                        boss_kills,
                        event_points,
                        reward_description,
                        is_claimed,
                        claimed_at,
                        saved_at
                    FROM event_results
                    WHERE event_id = ?
                    ORDER BY rank ASC
                    LIMIT ?";
            
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$eventId, $limit]);
            $results = $stmt->fetchAll(PDO::FETCH_ASSOC);
            
            $formattedResults = [];
            foreach ($results as $result) {
                $formatted = [
                    'id' => $result['id'],
                    'rank' => (int)$result['rank'],
                    'user_id' => $result['user_id'],
                    'player_name' => $result['player_name'],
                    'server_id' => (int)$result['server_id'],
                    'reward_description' => $result['reward_description'],
                    'is_claimed' => (bool)$result['is_claimed'],
                    'claimed_at' => $result['claimed_at'],
                    'saved_at' => $result['saved_at']
                ];
                
                // Thêm fields theo event_type
                if ($event['event_type'] == 'recharge') {
                    $formatted['total_amount'] = (float)$result['total_amount'];
                    $formatted['formatted_amount'] = number_format($result['total_amount'], 0, ',', '.') . ' VNĐ';
                    $formatted['total_xu'] = (int)$result['total_xu'];
                    $formatted['total_luong'] = (int)$result['total_luong'];
                    $formatted['total_luong_khoa'] = (int)$result['total_luong_khoa'];
                    $formatted['total_recharge'] = (int)$result['total_recharge'];
                } elseif ($event['event_type'] == 'level') {
                    $formatted['level'] = (int)$result['level'];
                    $formatted['xp'] = (int)$result['xp'];
                } elseif ($event['event_type'] == 'boss') {
                    $formatted['boss_kills'] = (int)$result['boss_kills'];
                } elseif ($event['event_type'] == 'event') {
                    $formatted['event_points'] = (int)$result['event_points'];
                }
                
                $formattedResults[] = $formatted;
            }
            
            return [
                'success' => true,
                'event' => [
                    'id' => $event['id'],
                    'event_code' => $event['event_code'],
                    'event_name' => $event['event_name'],
                    'event_type' => $event['event_type'],
                    'start_time' => $event['start_time'],
                    'end_time' => $event['end_time'],
                    'finished_at' => $event['finished_at'],
                    'finished_by' => $event['finished_by']
                ],
                'total_results' => count($results),
                'results' => $formattedResults
            ];
            
        } catch (Exception $e) {
            error_log("Get event results error: " . $e->getMessage());
            return ['success' => false, 'message' => 'Lỗi khi lấy kết quả: ' . $e->getMessage()];
        }
    }
    
    // ============================================
    // HELPER METHODS
    // ============================================
    
    private function getRewardsMap($eventId, $topType) {
        $sql = "SELECT rank, reward_description 
                FROM top_rewards_new 
                WHERE event_id = ? AND top_type = ?
                ORDER BY rank ASC";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$eventId, $topType]);
        $rewards = $stmt->fetchAll(PDO::FETCH_ASSOC);
        
        $map = [];
        foreach ($rewards as $reward) {
            $map[(int)$reward['rank']] = $reward['reward_description'];
        }
        
        return $map;
    }
    public function markAsClaimed($resultId) {
        try {
            $sql = "UPDATE event_results 
                    SET is_claimed = 1, 
                        claimed_at = NOW() 
                    WHERE id = ? AND is_claimed = 0";
            
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$resultId]);
            
            if ($stmt->rowCount() === 0) {
                return [
                    'success' => false,
                    'message' => 'Kết quả không tồn tại hoặc đã được đánh dấu trước đó'
                ];
            }
            
            return [
                'success' => true,
                'data' => [
                    'id' => (int)$resultId,
                    'is_claimed' => true,
                    'claimed_at' => date('Y-m-d H:i:s')
                ]
            ];
            
        } catch (PDOException $e) {
            error_log("Mark as claimed error: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Hủy đánh dấu đã nhận thưởng (single)
     * PUT /admin/event-results/{id}/unclaim
     */
    public function markAsUnclaimed($resultId) {
        try {
            $sql = "UPDATE event_results 
                    SET is_claimed = 0, 
                        claimed_at = NULL 
                    WHERE id = ?";
            
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$resultId]);
            
            if ($stmt->rowCount() === 0) {
                return [
                    'success' => false,
                    'message' => 'Kết quả không tồn tại'
                ];
            }
            
            return [
                'success' => true,
                'data' => [
                    'id' => (int)$resultId,
                    'is_claimed' => false,
                    'claimed_at' => null
                ]
            ];
            
        } catch (PDOException $e) {
            error_log("Mark as unclaimed error: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Đánh dấu hàng loạt đã nhận thưởng (bulk)
     * PUT /admin/event-results/bulk-claim
     */
    public function bulkMarkAsClaimed($resultIds) {
        try {
            if (empty($resultIds)) {
                return [
                    'success' => false,
                    'message' => 'Danh sách result_ids không được rỗng'
                ];
            }
            
            if (count($resultIds) > 100) {
                return [
                    'success' => false,
                    'message' => 'Chỉ được cập nhật tối đa 100 kết quả cùng lúc'
                ];
            }
            
            $placeholders = implode(',', array_fill(0, count($resultIds), '?'));
            $sql = "UPDATE event_results 
                    SET is_claimed = 1, 
                        claimed_at = NOW() 
                    WHERE id IN ($placeholders) AND is_claimed = 0";
            
            $stmt = $this->db->prepare($sql);
            $stmt->execute($resultIds);
            
            $updatedCount = $stmt->rowCount();
            
            return [
                'success' => true,
                'data' => [
                    'updated_count' => $updatedCount,
                    'requested_count' => count($resultIds)
                ],
                'message' => "Đã đánh dấu {$updatedCount} kết quả"
            ];
            
        } catch (PDOException $e) {
            error_log("Bulk mark as claimed error: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi: ' . $e->getMessage()
            ];
        }
    }
    private function getGameDb($serverId = 1) {
        static $gameDbs = [];
        
        if (!isset($gameDbs[$serverId])) {
            $dbInstance = Database::getGameInstance($serverId);
            $gameDbs[$serverId] = $dbInstance->getConnection();
        }
        
        return $gameDbs[$serverId];
    }
    
    private function getLevelFromExp($xp) {
        if ($xp < 1000) return 1;
        if ($xp < 5000) return 2;
        if ($xp < 10000) return 3;
        return floor(log($xp / 100) / log(1.1)) + 1;
    }
}