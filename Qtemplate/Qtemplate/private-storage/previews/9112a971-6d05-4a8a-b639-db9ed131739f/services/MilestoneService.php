<?php
// services/MilestoneService.php

require_once __DIR__ . '/ConfigService.php';

class MilestoneService {
    private $accountDb;
    private $gameDbs = [];
    private $config;
    
    public function __construct() {
        $this->accountDb = Database::getInstance()->getConnection();
        $this->config = ConfigService::getInstance();
    }
    
    /**
     * Lấy danh sách tất cả các mốc nạp đang active
     */
    public function getAllMilestones() {
        try {
            $stmt = $this->accountDb->prepare("
                SELECT 
                    id,
                    milestone_amount,
                    reward_xu,
                    reward_luong,
                    reward_luong_khoa,
                    item,
                    description,
                    display_order
                FROM recharge_milestones 
                WHERE is_active = 1
                ORDER BY display_order ASC, milestone_amount ASC
            ");
            
            $stmt->execute();
            $milestones = $stmt->fetchAll(PDO::FETCH_ASSOC);
            
            $formattedMilestones = [];
            foreach ($milestones as $milestone) {
                $formattedMilestones[] = [
                    'id' => (int)$milestone['id'],
                    'milestone_amount' => (float)$milestone['milestone_amount'],
                    'rewards' => [
                        'xu' => (int)$milestone['reward_xu'],
                        'luong' => (int)$milestone['reward_luong'],
                        'luong_khoa' => (int)$milestone['reward_luong_khoa'],
                        'items' => $this->parseItemString($milestone['item'])
                    ],
                    'description' => $milestone['description'],
                    'display_order' => (int)$milestone['display_order']
                ];
            }
            
            return [
                'success' => true,
                'data' => $formattedMilestones
            ];
            
        } catch (Exception $e) {
            error_log("Error getting milestones: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy danh sách mốc nạp: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Lấy thông tin tiến độ nạp của user
     */
    public function getUserProgress($userId, $serverId = 1) {
        try {
            $stmt = $this->accountDb->prepare("
                SELECT 
                    user_id,
                    username,
                    server_id,
                    total_amount,
                    total_xu,
                    total_luong,
                    total_luong_khoa,
                    total_recharge,
                    last_recharge_at
                FROM topnap 
                WHERE user_id = ? AND server_id = ?
                LIMIT 1
            ");
            
            $stmt->execute([$userId, $serverId]);
            $userProgress = $stmt->fetch(PDO::FETCH_ASSOC);
            
            if (!$userProgress) {
                return [
                    'success' => true,
                    'data' => [
                        'user_id' => $userId,
                        'server_id' => $serverId,
                        'total_amount' => 0,
                        'total_xu' => 0,
                        'total_luong' => 0,
                        'total_luong_khoa' => 0,
                        'total_recharge' => 0,
                        'last_recharge_at' => null
                    ]
                ];
            }
            
            return [
                'success' => true,
                'data' => [
                    'user_id' => (int)$userProgress['user_id'],
                    'username' => $userProgress['username'],
                    'server_id' => (int)$userProgress['server_id'],
                    'total_amount' => (float)$userProgress['total_amount'],
                    'total_xu' => (int)$userProgress['total_xu'],
                    'total_luong' => (int)$userProgress['total_luong'],
                    'total_luong_khoa' => (int)$userProgress['total_luong_khoa'],
                    'total_recharge' => (int)$userProgress['total_recharge'],
                    'last_recharge_at' => $userProgress['last_recharge_at']
                ]
            ];
            
        } catch (Exception $e) {
            error_log("Error getting user progress: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy tiến độ nạp: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Lấy danh sách các mốc đã nhận của user
     */
    public function getClaimedMilestones($userId, $serverId = 1) {
        try {
            $stmt = $this->accountDb->prepare("
                SELECT 
                    milestone_id,
                    milestone_amount,
                    claimed_at
                FROM user_milestone_claimed 
                WHERE user_id = ? AND server_id = ?
                ORDER BY claimed_at DESC
            ");
            
            $stmt->execute([$userId, $serverId]);
            $claimed = $stmt->fetchAll(PDO::FETCH_ASSOC);
            
            $formattedClaimed = [];
            foreach ($claimed as $item) {
                $formattedClaimed[] = [
                    'milestone_id' => (int)$item['milestone_id'],
                    'milestone_amount' => (float)$item['milestone_amount'],
                    'claimed_at' => $item['claimed_at']
                ];
            }
            
            return [
                'success' => true,
                'data' => $formattedClaimed
            ];
            
        } catch (Exception $e) {
            error_log("Error getting claimed milestones: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy danh sách mốc đã nhận: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Lấy danh sách các mốc có thể nhận
     */
    public function getAvailableMilestones($userId, $serverId = 1) {
        try {
            $progressResult = $this->getUserProgress($userId, $serverId);
            if (!$progressResult['success']) {
                return $progressResult;
            }
            
            $totalAmount = $progressResult['data']['total_amount'];
            
            $claimedResult = $this->getClaimedMilestones($userId, $serverId);
            if (!$claimedResult['success']) {
                return $claimedResult;
            }
            
            $claimedIds = array_map(function($item) {
                return $item['milestone_id'];
            }, $claimedResult['data']);
            
            $milestonesResult = $this->getAllMilestones();
            if (!$milestonesResult['success']) {
                return $milestonesResult;
            }
            
            $allMilestones = $milestonesResult['data'];
            
            $availableMilestones = [];
            foreach ($allMilestones as $milestone) {
                if ($totalAmount >= $milestone['milestone_amount'] && 
                    !in_array($milestone['id'], $claimedIds)) {
                    $availableMilestones[] = $milestone;
                }
            }
            
            return [
                'success' => true,
                'data' => $availableMilestones
            ];
            
        } catch (Exception $e) {
            error_log("Error getting available milestones: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy danh sách mốc có thể nhận: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Xem chi tiết tổng hợp tất cả thông tin milestone cho user
     */
    public function getUserMilestoneOverview($userId, $serverId = 1) {
        try {
            $progressResult = $this->getUserProgress($userId, $serverId);
            if (!$progressResult['success']) {
                return $progressResult;
            }
            
            $progress = $progressResult['data'];
            $totalAmount = $progress['total_amount'];
            
            $milestonesResult = $this->getAllMilestones();
            if (!$milestonesResult['success']) {
                return $milestonesResult;
            }
            
            $allMilestones = $milestonesResult['data'];
            
            $claimedResult = $this->getClaimedMilestones($userId, $serverId);
            if (!$claimedResult['success']) {
                return $claimedResult;
            }
            
            $claimedIds = array_map(function($item) {
                return $item['milestone_id'];
            }, $claimedResult['data']);
            
            $milestoneStatus = [
                'available' => [],
                'claimed' => [],
                'locked' => []
            ];
            
            foreach ($allMilestones as $milestone) {
                $milestoneWithStatus = $milestone;
                
                if (in_array($milestone['id'], $claimedIds)) {
                    $milestoneWithStatus['status'] = 'claimed';
                    $milestoneStatus['claimed'][] = $milestoneWithStatus;
                } elseif ($totalAmount >= $milestone['milestone_amount']) {
                    $milestoneWithStatus['status'] = 'available';
                    $milestoneStatus['available'][] = $milestoneWithStatus;
                } else {
                    $milestoneWithStatus['status'] = 'locked';
                    $milestoneWithStatus['remaining_amount'] = $milestone['milestone_amount'] - $totalAmount;
                    $milestoneStatus['locked'][] = $milestoneWithStatus;
                }
            }
            
            return [
                'success' => true,
                'data' => [
                    'user_progress' => $progress,
                    'milestones' => $milestoneStatus,
                    'summary' => [
                        'total_milestones' => count($allMilestones),
                        'available_count' => count($milestoneStatus['available']),
                        'claimed_count' => count($milestoneStatus['claimed']),
                        'locked_count' => count($milestoneStatus['locked'])
                    ]
                ]
            ];
            
        } catch (Exception $e) {
            error_log("Error getting milestone overview: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy tổng quan mốc nạp: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Nhận phần thưởng mốc nạp
     */
    public function claimMilestone($userId, $username, $serverId, $milestoneId) {
        try {
            $this->accountDb->beginTransaction();
            
            $milestoneStmt = $this->accountDb->prepare("
                SELECT * FROM recharge_milestones 
                WHERE id = ? AND is_active = 1
                LIMIT 1
            ");
            $milestoneStmt->execute([$milestoneId]);
            $milestone = $milestoneStmt->fetch(PDO::FETCH_ASSOC);
            
            if (!$milestone) {
                $this->accountDb->rollBack();
                return [
                    'success' => false,
                    'message' => 'Mốc nạp không tồn tại hoặc đã bị vô hiệu hóa'
                ];
            }
            
            $claimedStmt = $this->accountDb->prepare("
                SELECT id FROM user_milestone_claimed 
                WHERE user_id = ? AND server_id = ? AND milestone_id = ?
                LIMIT 1
            ");
            $claimedStmt->execute([$userId, $serverId, $milestoneId]);
            
            if ($claimedStmt->fetch()) {
                $this->accountDb->rollBack();
                return [
                    'success' => false,
                    'message' => 'Bạn đã nhận phần thưởng mốc này rồi'
                ];
            }
            
            $progressResult = $this->getUserProgress($userId, $serverId);
            if (!$progressResult['success']) {
                $this->accountDb->rollBack();
                return $progressResult;
            }
            
            $totalAmount = $progressResult['data']['total_amount'];
            $milestoneAmount = (float)$milestone['milestone_amount'];
            
            if ($totalAmount < $milestoneAmount) {
                $this->accountDb->rollBack();
                return [
                    'success' => false,
                    'message' => sprintf(
                        'Bạn chưa đủ điều kiện nhận mốc này. Cần nạp: %s VNĐ, Đã nạp: %s VNĐ',
                        number_format($milestoneAmount, 0, ',', '.'),
                        number_format($totalAmount, 0, ',', '.')
                    )
                ];
            }
            
            $claimStmt = $this->accountDb->prepare("
                INSERT INTO user_milestone_claimed 
                (user_id, username, server_id, milestone_id, milestone_amount, claimed_at) 
                VALUES (?, ?, ?, ?, ?, NOW())
            ");
            
            $claimStmt->execute([
                $userId,
                $username,
                $serverId,
                $milestoneId,
                $milestoneAmount
            ]);
            
            $gameDb = $this->getGameDb($serverId);
            
            $rewardStmt = $gameDb->prepare("
                INSERT INTO board_milestone_rewards 
                (username, xu, luong, luongKhoa, item, milestone_amount, created_at) 
                VALUES (?, ?, ?, ?, ?, ?, NOW())
            ");
            
            $rewardStmt->execute([
                $username,
                $milestone['reward_xu'],
                $milestone['reward_luong'],
                $milestone['reward_luong_khoa'],
                $milestone['item'],
                $milestoneAmount
            ]);
            
            $this->accountDb->commit();
            
            return [
                'success' => true,
                'message' => 'Nhận phần thưởng thành công',
                'data' => [
                    'milestone_id' => $milestoneId,
                    'milestone_amount' => $milestoneAmount,
                    'rewards' => [
                        'xu' => (int)$milestone['reward_xu'],
                        'luong' => (int)$milestone['reward_luong'],
                        'luong_khoa' => (int)$milestone['reward_luong_khoa'],
                        'items' => $this->parseItemString($milestone['item'])
                    ],
                    'claimed_at' => date('Y-m-d H:i:s')
                ]
            ];
            
        } catch (Exception $e) {
            $this->accountDb->rollBack();
            error_log("Error claiming milestone: " . $e->getMessage());
            return [
                'success' => false,
                'message' => 'Lỗi khi nhận phần thưởng: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Parse chuỗi item thành array
     * Format: DB:685:1:-1,GEM:249:2000:-1
     */
    private function parseItemString($itemString) {
        if (empty($itemString)) {
            return [];
        }
        
        $items = [];
        $itemParts = explode(',', $itemString);
        
        foreach ($itemParts as $itemPart) {
            $parts = explode(':', trim($itemPart));
            if (count($parts) >= 4) {
                $items[] = [
                    'type' => $parts[0],
                    'id' => (int)$parts[1],
                    'quantity' => (int)$parts[2],
                    'expire' => (int)$parts[3]
                ];
            }
        }
        
        return $items;
    }
    
    /**
     * Lấy kết nối game database
     */
    private function getGameDb($serverId = null) {
        if ($serverId === null) {
            $serverId = Config::DEFAULT_SERVER_ID;
        }
        
        if (!isset($this->gameDbs[$serverId])) {
            $dbInstance = Database::getGameInstance($serverId);
            $this->gameDbs[$serverId] = $dbInstance->getConnection();
        }
        
        return $this->gameDbs[$serverId];
    }
}