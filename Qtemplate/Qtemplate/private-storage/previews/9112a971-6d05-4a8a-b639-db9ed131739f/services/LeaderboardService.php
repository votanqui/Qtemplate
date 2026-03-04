<?php
// services/LeaderboardService.php

class LeaderboardService {
    private $accountDb;
    private $gameDbs = [];
    
    public function __construct() {
        $this->accountDb = Database::getInstance()->getConnection();
    }
    
    /**
     * Lấy kết nối game database theo server_id
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
    
    /**
     * Lấy thông tin sự kiện đang active theo loại
     * Ưu tiên: 
     * 1. Event đang diễn ra (NOW BETWEEN start_time AND end_time)
     * 2. Event sắp diễn ra (is_active=1, is_finished=0)
     */
    private function getActiveEvent($eventType) {
        try {
            // Thử tìm event đang diễn ra
            $stmt = $this->accountDb->prepare("
                SELECT * FROM events 
                WHERE event_type = ? 
                AND is_active = 1 
                AND is_finished = 0
                AND NOW() BETWEEN start_time AND end_time
                ORDER BY start_time DESC
                LIMIT 1
            ");
            
            $stmt->execute([$eventType]);
            $event = $stmt->fetch(PDO::FETCH_ASSOC);
            
            if ($event) {
                return $event;
            }
            
            // Nếu không có event đang diễn ra, lấy event active gần nhất
            $stmt = $this->accountDb->prepare("
                SELECT * FROM events 
                WHERE event_type = ? 
                AND is_active = 1 
                AND is_finished = 0
                ORDER BY ABS(TIMESTAMPDIFF(SECOND, NOW(), start_time)) ASC
                LIMIT 1
            ");
            
            $stmt->execute([$eventType]);
            return $stmt->fetch(PDO::FETCH_ASSOC);
            
        } catch (Exception $e) {
            error_log("Error fetching active event: " . $e->getMessage());
            return null;
        }
    }
    
    /**
     * Lấy phần thưởng từ bảng top_rewards_new theo event_id và top_type
     */
    private function getRewardsFromDB($eventId, $topType) {
        try {
            $stmt = $this->accountDb->prepare("
                SELECT rank, reward_description
                FROM top_rewards_new
                WHERE event_id = ? AND top_type = ?
                ORDER BY rank ASC
            ");
            
            $stmt->execute([$eventId, $topType]);
            $results = $stmt->fetchAll(PDO::FETCH_ASSOC);
            
            $rewards = [];
            foreach ($results as $row) {
                $rewards[(int)$row['rank']] = trim($row['reward_description']);
            }
            
            return $rewards;
        } catch (Exception $e) {
            error_log("Error fetching rewards for event {$eventId}, type {$topType}: " . $e->getMessage());
            return [];
        }
    }
    
    /**
     * Lấy Level từ XP
     */
    private function getLevelFromExp($xp) {
        if ($xp < 1000) return 1;
        if ($xp < 5000) return 2;
        if ($xp < 10000) return 3;
        return floor(log($xp / 100) / log(1.1)) + 1;
    }
    
    /**
     * TOP 1: Xếp Hạng Level (Thời gian thực)
     */
    public function getTopLevel($limit = 10, $serverId = null) {
        try {
            $gameDb = $this->getGameDb($serverId);
            
            // Lấy thông tin event đang active
            $activeEvent = $this->getActiveEvent('level');
            
            $stmt = $gameDb->prepare("
                SELECT charname, xp 
                FROM tob_char 
                WHERE ban = '0' AND charname != 'chienthan' 
                ORDER BY xp DESC 
                LIMIT ?
            ");
            
            $stmt->execute([$limit]);
            $results = $stmt->fetchAll();
            
            // Lấy rewards từ event (nếu có)
            $rewards = [];
            if ($activeEvent) {
                $rewards = $this->getRewardsFromDB($activeEvent['id'], 'top_level');
            }
            
            $defaultReward = "20.000 lượng\n20.000 lượng khóa\n1 phi phong theo level";
            
            $leaderboard = [];
            foreach ($results as $index => $row) {
                $rank = $index + 1;
                $leaderboard[] = [
                    'rank' => $rank,
                    'charname' => $row['charname'],
                    'level' => $this->getLevelFromExp($row['xp']),
                    'xp' => (int)$row['xp'],
                    'reward' => $rewards[$rank] ?? $defaultReward,
                    'rank_color' => $this->getRankColor($rank)
                ];
            }
            
            return [
                'success' => true,
                'data' => [
                    'title' => 'Danh Sách Xếp Hạng Level',
                    'server_id' => $serverId,
                    'total' => count($leaderboard),
                    'event_info' => $activeEvent ? [
                        'event_id' => $activeEvent['id'],
                        'event_name' => $activeEvent['event_name'],
                        'event_code' => $activeEvent['event_code'],
                        'description' => $activeEvent['description'],
                        'start_time' => $activeEvent['start_time'],
                        'end_time' => $activeEvent['end_time'],
                        'time_remaining' => $this->getTimeRemaining($activeEvent['start_time'], $activeEvent['end_time']),
                        'is_ongoing' => $this->isEventOngoing($activeEvent['start_time'], $activeEvent['end_time'])
                    ] : null,
                    'leaderboard' => $leaderboard
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy bảng xếp hạng level: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * TOP 2: Xếp Hạng Sự Kiện (Thời gian thực)
     */
    public function getTopEvent($limit = 10, $serverId = null) {
        try {
            $gameDb = $this->getGameDb($serverId);
            
            // Lấy thông tin event đang active
            $activeEvent = $this->getActiveEvent('event');
            
            $stmt = $gameDb->prepare("
                SELECT charname, point1 
                FROM tob_top_event 
                ORDER BY point1 DESC 
                LIMIT ?
            ");
            
            $stmt->execute([$limit]);
            $results = $stmt->fetchAll();
            
            // Lấy rewards từ event (nếu có)
            $rewards = [];
            if ($activeEvent) {
                $rewards = $this->getRewardsFromDB($activeEvent['id'], 'top_event');
            }
            
            $defaultReward = "Không có phần thưởng";
            
            $leaderboard = [];
            foreach ($results as $index => $row) {
                $rank = $index + 1;
                $leaderboard[] = [
                    'rank' => $rank,
                    'charname' => $row['charname'],
                    'points' => (int)$row['point1'],
                    'reward' => $rewards[$rank] ?? $defaultReward,
                    'rank_color' => $this->getRankColor($rank)
                ];
            }
            
            return [
                'success' => true,
                'data' => [
                    'title' => 'Danh Sách Xếp Hạng Sự Kiện',
                    'server_id' => $serverId,
                    'total' => count($leaderboard),
                    'event_info' => $activeEvent ? [
                        'event_id' => $activeEvent['id'],
                        'event_name' => $activeEvent['event_name'],
                        'event_code' => $activeEvent['event_code'],
                        'description' => $activeEvent['description'],
                        'start_time' => $activeEvent['start_time'],
                        'end_time' => $activeEvent['end_time'],
                        'time_remaining' => $this->getTimeRemaining($activeEvent['start_time'], $activeEvent['end_time']),
                        'is_ongoing' => $this->isEventOngoing($activeEvent['start_time'], $activeEvent['end_time'])
                    ] : null,
                    'leaderboard' => $leaderboard
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy bảng xếp hạng sự kiện: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * TOP 3: Xếp Hạng Tích Lũy (Thời gian thực)
     */
   public function getTopRecharge($limit = 10, $serverId = null) {
        try {
            // Lấy thông tin event đang active
            $activeEvent = $this->getActiveEvent('recharge');
            
            // THAY ĐỔI: Lấy từ event_recharge thay vì topnap
            $query = "
                SELECT 
                    username,
                    total_amount,
                    total_xu,
                    total_luong,
                    total_luong_khoa,
                    total_recharge
                FROM event_recharge 
                WHERE username != 'chienthan'
            ";
            
            $params = [];
            
            // THÊM: Lọc theo event_id nếu có event active
            if ($activeEvent) {
                $query .= " AND event_id = ?";
                $params[] = $activeEvent['id'];
            }
            
            if ($serverId !== null) {
                $query .= " AND server_id = ?";
                $params[] = $serverId;
            }
            
            $query .= " ORDER BY total_amount DESC LIMIT ?";
            $params[] = $limit;
            
            $stmt = $this->accountDb->prepare($query);
            $stmt->execute($params);
            $results = $stmt->fetchAll();
            
            // Lấy rewards từ event (nếu có)
            $rewards = [];
            if ($activeEvent) {
                $rewards = $this->getRewardsFromDB($activeEvent['id'], 'top_nap');
            }
            
            $defaultReward = "Phượng hoàng băng\nPhượng hoàng lửa 30 ngày";
            
            $leaderboard = [];
            foreach ($results as $index => $row) {
                $rank = $index + 1;
                $leaderboard[] = [
                    'rank' => $rank,
                    'username' => $row['username'],
                    'total_amount' => (float)$row['total_amount'],
                    'total_xu' => (int)$row['total_xu'],
                    'total_luong' => (int)$row['total_luong'],
                    'total_luong_khoa' => (int)$row['total_luong_khoa'],
                    'total_recharge' => (int)$row['total_recharge'],
                    'formatted_amount' => number_format($row['total_amount'], 0, ',', '.') . ' VNĐ',
                    'reward' => $rewards[$rank] ?? $defaultReward,
                    'rank_color' => $this->getRankColor($rank)
                ];
            }
            
            return [
                'success' => true,
                'data' => [
                    'title' => 'Danh Sách Xếp Hạng Tích Lũy',
                    'server_id' => $serverId,
                    'total' => count($leaderboard),
                    'event_info' => $activeEvent ? [
                        'event_id' => $activeEvent['id'],
                        'event_name' => $activeEvent['event_name'],
                        'event_code' => $activeEvent['event_code'],
                        'description' => $activeEvent['description'],
                        'start_time' => $activeEvent['start_time'],
                        'end_time' => $activeEvent['end_time'],
                        'time_remaining' => $this->getTimeRemaining($activeEvent['start_time'], $activeEvent['end_time']),
                        'is_ongoing' => $this->isEventOngoing($activeEvent['start_time'], $activeEvent['end_time'])
                    ] : null,
                    'leaderboard' => $leaderboard
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy bảng xếp hạng tích lũy: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * TOP 4: Xếp Hạng Săn Boss (Thời gian thực)
     */
    public function getTopBoss($limit = 10, $serverId = null) {
        try {
            $gameDb = $this->getGameDb($serverId);
            
            // Lấy thông tin event đang active
            $activeEvent = $this->getActiveEvent('boss');
            
            $stmt = $gameDb->prepare("
                SELECT player, win 
                FROM tob_sanboss 
                WHERE player != 'chienthan' 
                ORDER BY win DESC 
                LIMIT ?
            ");
            
            $stmt->execute([$limit]);
            $results = $stmt->fetchAll();
            
            // Lấy rewards từ event (nếu có)
            $rewards = [];
            if ($activeEvent) {
                $rewards = $this->getRewardsFromDB($activeEvent['id'], 'top_boss');
            }
            
            $defaultReward = "Không có phần thưởng";
            
            $leaderboard = [];
            foreach ($results as $index => $row) {
                $rank = $index + 1;
                $leaderboard[] = [
                    'rank' => $rank,
                    'player' => $row['player'],
                    'boss_kills' => (int)$row['win'],
                    'reward' => $rewards[$rank] ?? $defaultReward,
                    'rank_color' => $this->getRankColor($rank)
                ];
            }
            
            return [
                'success' => true,
                'data' => [
                    'title' => 'Danh Sách Xếp Hạng Săn Boss Tiên La',
                    'server_id' => $serverId,
                    'total' => count($leaderboard),
                    'event_info' => $activeEvent ? [
                        'event_id' => $activeEvent['id'],
                        'event_name' => $activeEvent['event_name'],
                        'event_code' => $activeEvent['event_code'],
                        'description' => $activeEvent['description'],
                        'start_time' => $activeEvent['start_time'],
                        'end_time' => $activeEvent['end_time'],
                        'time_remaining' => $this->getTimeRemaining($activeEvent['start_time'], $activeEvent['end_time']),
                        'is_ongoing' => $this->isEventOngoing($activeEvent['start_time'], $activeEvent['end_time'])
                    ] : null,
                    'leaderboard' => $leaderboard
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy bảng xếp hạng săn boss: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Lấy kết quả event từ bảng event_results
     */
   public function getEventRechargeHistory($eventId, $limit = 10, $serverId = null) {
        try {
            // Kiểm tra event có tồn tại và đã kết thúc chưa
            $stmt = $this->accountDb->prepare("
                SELECT * FROM events 
                WHERE id = ? AND is_finished = 1
            ");
            $stmt->execute([$eventId]);
            $event = $stmt->fetch(PDO::FETCH_ASSOC);
            
            if (!$event) {
                return [
                    'success' => false,
                    'message' => 'Event không tồn tại hoặc chưa được chốt kết quả'
                ];
            }
            
            // Lấy kết quả từ event_results
            $query = "
                SELECT 
                    rank,
                    user_id,
                    player_name,
                    server_id,
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
            ";
            
            $params = [$eventId];
            
            if ($serverId !== null) {
                $query .= " AND server_id = ?";
                $params[] = $serverId;
            }
            
            $query .= " ORDER BY rank ASC LIMIT ?";
            $params[] = $limit;
            
            $stmt = $this->accountDb->prepare($query);
            $stmt->execute($params);
            $results = $stmt->fetchAll(PDO::FETCH_ASSOC);
            
            $leaderboard = [];
            foreach ($results as $row) {
                $item = [
                    'rank' => (int)$row['rank'],
                    'username' => $row['player_name'],
                    'reward' => $row['reward_description'],
                    'is_claimed' => (bool)$row['is_claimed'],
                    'claimed_at' => $row['claimed_at'],
                    'saved_at' => $row['saved_at'],
                    'rank_color' => $this->getRankColor($row['rank'])
                ];
                
                // Thêm fields theo event_type
                if ($event['event_type'] == 'recharge') {
                    $item['total_amount'] = (float)$row['total_amount'];
                    $item['formatted_amount'] = number_format($row['total_amount'], 0, ',', '.') . ' VNĐ';
                    $item['total_xu'] = (int)$row['total_xu'];
                    $item['total_luong'] = (int)$row['total_luong'];
                    $item['total_luong_khoa'] = (int)$row['total_luong_khoa'];
                    $item['total_recharge'] = (int)$row['total_recharge'];
                } elseif ($event['event_type'] == 'level') {
                    $item['level'] = (int)$row['level'];
                    $item['xp'] = (int)$row['xp'];
                    $item['formatted_stat'] = "Level {$item['level']}";
                } elseif ($event['event_type'] == 'boss') {
                    $item['boss_kills'] = (int)$row['boss_kills'];
                    $item['formatted_stat'] = number_format($item['boss_kills']) . ' Boss';
                } elseif ($event['event_type'] == 'event') {
                    $item['event_points'] = (int)$row['event_points'];
                    $item['formatted_stat'] = number_format($item['event_points']) . ' điểm';
                }
                
                $leaderboard[] = $item;
            }
            
            return [
                'success' => true,
                'data' => [
                    'title' => 'Kết Quả ' . $event['event_name'],
                    'event_info' => [
                        'event_id' => $event['id'],
                        'event_name' => $event['event_name'],
                        'event_code' => $event['event_code'],
                        'event_type' => $event['event_type'],
                        'description' => $event['description'],
                        'start_time' => $event['start_time'],
                        'end_time' => $event['end_time'],
                        'finished_at' => $event['finished_at']
                    ],
                    'server_id' => $serverId,
                    'total' => count($leaderboard),
                    'leaderboard' => $leaderboard
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy kết quả event: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Lấy danh sách các event đã kết thúc theo loại
     */
    public function getFinishedEvents($eventType = null) {
        try {
            $query = "
                SELECT 
                    id,
                    event_code,
                    event_name,
                    event_type,
                    description,
                    start_time,
                    end_time,
                    finished_at,
                    finished_by
                FROM events 
                WHERE is_finished = 1
            ";
            
            $params = [];
            
            if ($eventType !== null) {
                $query .= " AND event_type = ?";
                $params[] = $eventType;
            }
            
            $query .= " ORDER BY finished_at DESC";
            
            $stmt = $this->accountDb->prepare($query);
            $stmt->execute($params);
            $results = $stmt->fetchAll(PDO::FETCH_ASSOC);
            
            $events = [];
            foreach ($results as $row) {
                $events[] = [
                    'event_id' => (int)$row['id'],
                    'event_code' => $row['event_code'],
                    'event_name' => $row['event_name'],
                    'event_type' => $row['event_type'],
                    'description' => $row['description'],
                    'start_time' => $row['start_time'],
                    'end_time' => $row['end_time'],
                    'finished_at' => $row['finished_at'],
                    'duration' => $this->getEventDuration($row['start_time'], $row['end_time'])
                ];
            }
            
            return [
                'success' => true,
                'data' => [
                    'total' => count($events),
                    'events' => $events
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy danh sách event: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Lấy danh sách server
     */
    public function getServerList() {
        try {
            $servers = Database::getAllActiveServers();
            
            $serverList = [];
            foreach ($servers as $server) {
                $serverList[] = [
                    'server_id' => $server['server_id'],
                    'server_name' => $server['server_name'],
                    'db_name' => $server['db_name'],
                    'db_host' => $server['db_host'],
                    'db_port' => $server['db_port'],
                    'created_at' => $server['created_at']
                ];
            }
            
            return [
                'success' => true,
                'data' => [
                    'servers' => $serverList,
                    'default_server_id' => Config::DEFAULT_SERVER_ID
                ]
            ];
            
        } catch (Exception $e) {
            return [
                'success' => false,
                'message' => 'Lỗi khi lấy danh sách server: ' . $e->getMessage()
            ];
        }
    }
    
    /**
     * Tính thời gian còn lại hoặc thời gian đến khi bắt đầu
     */
    private function getTimeRemaining($startTime, $endTime) {
    // Set timezone mặc định (thay 'Asia/Ho_Chi_Minh' bằng timezone của bạn)
    date_default_timezone_set('Asia/Ho_Chi_Minh');
    
    $now = new DateTime();
    $start = new DateTime($startTime);
    $end = new DateTime($endTime);
    
    // Debug: log thời gian để kiểm tra
    error_log("Now: " . $now->format('Y-m-d H:i:s'));
    error_log("End: " . $end->format('Y-m-d H:i:s'));
    
    // Nếu event chưa bắt đầu
    if ($now < $start) {
        $diff = $now->diff($start);
        $parts = [];
        if ($diff->d > 0) $parts[] = $diff->d . ' ngày';
        if ($diff->h > 0) $parts[] = $diff->h . ' giờ';
        if ($diff->i > 0) $parts[] = $diff->i . ' phút';
        
        return 'Bắt đầu sau ' . (count($parts) > 0 ? implode(' ', $parts) : '0 phút');
    }
    
    // Nếu event đã kết thúc
    if ($now > $end) {
        return 'Đã kết thúc';
    }
    
    // Event đang diễn ra
    $diff = $now->diff($end);
    
    $parts = [];
    if ($diff->d > 0) $parts[] = $diff->d . ' ngày';
    if ($diff->h > 0) $parts[] = $diff->h . ' giờ';
    if ($diff->i > 0) $parts[] = $diff->i . ' phút';
    
    return 'Còn lại ' . (count($parts) > 0 ? implode(' ', $parts) : '0 phút');
}
    
    /**
     * Kiểm tra event có đang diễn ra không
     */
    private function isEventOngoing($startTime, $endTime) {
        $now = new DateTime();
        $start = new DateTime($startTime);
        $end = new DateTime($endTime);
        
        return ($now >= $start && $now <= $end);
    }
    
    /**
     * Tính thời lượng event
     */
    private function getEventDuration($startTime, $endTime) {
        $start = new DateTime($startTime);
        $end = new DateTime($endTime);
        $diff = $start->diff($end);
        
        return $diff->days . ' ngày';
    }
    
    /**
     * Lấy màu theo thứ hạng
     */
    private function getRankColor($rank) {
        if ($rank == 1) return 'danger';
        if ($rank == 2) return 'warning';
        if ($rank == 3) return 'info';
        return 'dark';
    }
}