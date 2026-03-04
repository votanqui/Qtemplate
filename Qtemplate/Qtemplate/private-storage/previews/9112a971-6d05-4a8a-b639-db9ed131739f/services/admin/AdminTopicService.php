<?php
// services/AdminTopicService.php

class AdminTopicService {
    private $db;
    
    public function __construct() {
        $this->db = Database::getInstance()->getConnection();
    }
    
    /**
     * Lấy danh sách topics với phân trang và tìm kiếm
     */
    public function getTopics($page, $limit, $search, $status, $sortBy, $sortOrder) {
        $offset = ($page - 1) * $limit;
        
        // Build query
        $where = "1=1";
        $params = [];
        
        // Search filter
        if (!empty($search)) {
            $where .= " AND (title LIKE ? OR contents LIKE ?)";
            $searchTerm = "%$search%";
            $params[] = $searchTerm;
            $params[] = $searchTerm;
        }
        
        // Status filter
        if ($status === 'active') {
            $where .= " AND block = 0";
        } elseif ($status === 'blocked') {
            $where .= " AND block = 1";
        } elseif ($status === 'sticky') {
            $where .= " AND stick = 1";
        } elseif ($status === 'done') {
            $where .= " AND done = 1";
        }
        
        // Validate sort
        $allowedSort = ['id', 'title', 'time_created', 'owner', 'stick', 'block'];
        $sortBy = in_array($sortBy, $allowedSort) ? $sortBy : 'time_created';
        $sortOrder = strtoupper($sortOrder) === 'ASC' ? 'ASC' : 'DESC';
        
        // Get total count
        $countSql = "SELECT COUNT(*) as total FROM topic WHERE $where";
        $stmt = $this->db->prepare($countSql);
        $stmt->execute($params);
        $total = $stmt->fetch()['total'];
        
        // Get topics
        $sql = "SELECT 
                    t.id, t.title, t.topic, t.thread, t.tags, 
                    t.owner, t.user, t.contents, t.time_created, 
                    t.block, t.stick, t.done
                FROM topic t
                WHERE $where 
                ORDER BY $sortBy $sortOrder 
                LIMIT $limit OFFSET $offset";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute($params);
        $topics = $stmt->fetchAll();
        
        return [
            'topics' => $topics,
            'pagination' => [
                'total' => (int)$total,
                'page' => $page,
                'limit' => $limit,
                'total_pages' => ceil($total / $limit)
            ]
        ];
    }
    
    /**
     * Lấy thông tin chi tiết topic
     */
    public function getTopicDetail($topicId) {
        $sql = "SELECT 
                    t.id, t.title, t.topic, t.thread, t.tags, 
                    t.owner, t.user, t.contents, t.time_created, 
                    t.block, t.stick, t.done
                FROM topic t
                WHERE t.id = ?";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$topicId]);
        $topic = $stmt->fetch();
        
        if (!$topic) {
            return null;
        }
        
        return $topic;
    }
    
    /**
     * Tạo topic mới
     */
    public function createTopic($data) {
        $requiredFields = ['title', 'owner', 'contents'];
        
        foreach ($requiredFields as $field) {
            if (!isset($data[$field]) || empty($data[$field])) {
                return ['success' => false, 'message' => "Thiếu trường bắt buộc: $field"];
            }
        }
        
        $sql = "INSERT INTO topic 
                (title, topic, thread, tags, owner, user, contents, time_created, block, stick, done) 
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
        
        try {
            $stmt = $this->db->prepare($sql);
            $stmt->execute([
                $data['title'],
                $data['topic'] ?? 0,
                $data['thread'] ?? null,
                $data['tags'] ?? null,
                $data['owner'],
                $data['user'] ?? null,
                $data['contents'],
                $data['time_created'] ?? time(),
                $data['block'] ?? 0,
                $data['stick'] ?? 0,
                $data['done'] ?? 0
            ]);
            
            $topicId = $this->db->lastInsertId();
            
            return [
                'success' => true,
                'topic_id' => $topicId,
                'topic' => $this->getTopicDetail($topicId)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Tạo topic thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Cập nhật thông tin topic
     */
    public function updateTopic($topicId, $data) {
        $allowedFields = ['title', 'topic', 'thread', 'tags', 'user', 'contents', 'block', 'stick', 'done'];
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
        
        $params[] = $topicId;
        
        $sql = "UPDATE topic SET " . implode(', ', $updateFields) . " WHERE id = ?";
        
        try {
            $stmt = $this->db->prepare($sql);
            $stmt->execute($params);
            
            return [
                'success' => true,
                'topic' => $this->getTopicDetail($topicId)
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Cập nhật thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Block/Unblock topic
     */
    public function toggleBlock($topicId, $action) {
        $blockValue = ($action === 'block') ? 1 : 0;
        
        $sql = "UPDATE topic SET block = ? WHERE id = ?";
        
        try {
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$blockValue, $topicId]);
            
            return [
                'success' => true,
                'blocked' => $blockValue,
                'message' => $action === 'block' ? 'Đã khóa topic' : 'Đã mở khóa topic'
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Thao tác thất bại'];
        }
    }
    
    /**
     * Sticky/Unsticky topic
     */
    public function toggleSticky($topicId, $action) {
        $stickValue = ($action === 'stick') ? 1 : 0;
        
        $sql = "UPDATE topic SET stick = ? WHERE id = ?";
        
        try {
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$stickValue, $topicId]);
            
            return [
                'success' => true,
                'sticky' => $stickValue,
                'message' => $action === 'stick' ? 'Đã ghim topic' : 'Đã bỏ ghim topic'
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Thao tác thất bại'];
        }
    }
    
    /**
     * Mark as done/undone
     */
    public function toggleDone($topicId, $action) {
        $doneValue = ($action === 'done') ? 1 : 0;
        
        $sql = "UPDATE topic SET done = ? WHERE id = ?";
        
        try {
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$doneValue, $topicId]);
            
            return [
                'success' => true,
                'done' => $doneValue,
                'message' => $action === 'done' ? 'Đã đánh dấu hoàn thành' : 'Đã bỏ đánh dấu'
            ];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Thao tác thất bại'];
        }
    }
    
    /**
     * Xóa topic
     */
    public function deleteTopic($topicId) {
        try {
            $sql = "DELETE FROM topic WHERE id = ?";
            $stmt = $this->db->prepare($sql);
            $stmt->execute([$topicId]);
            
            return ['success' => true, 'message' => 'Xóa topic thành công'];
        } catch (PDOException $e) {
            return ['success' => false, 'message' => 'Xóa thất bại: ' . $e->getMessage()];
        }
    }
    
    /**
     * Lấy thống kê
     */
    public function getStats($period) {
        $dateFilter = $this->getDateFilter($period);
        
        // Total topics
        $stmt = $this->db->query("SELECT COUNT(*) as total FROM topic");
        $totalTopics = $stmt->fetch()['total'];
        
        // New topics in period
        $stmt = $this->db->prepare("SELECT COUNT(*) as total FROM topic WHERE time_created >= ?");
        $stmt->execute([$dateFilter]);
        $newTopics = $stmt->fetch()['total'];
        
        // Blocked topics
        $stmt = $this->db->query("SELECT COUNT(*) as total FROM topic WHERE block = 1");
        $blockedTopics = $stmt->fetch()['total'];
        
        // Sticky topics
        $stmt = $this->db->query("SELECT COUNT(*) as total FROM topic WHERE stick = 1");
        $stickyTopics = $stmt->fetch()['total'];
        
        // Done topics
        $stmt = $this->db->query("SELECT COUNT(*) as total FROM topic WHERE done = 1");
        $doneTopics = $stmt->fetch()['total'];
        
        // Topic creation trend (last 7 days)
        $trend = [];
        for ($i = 6; $i >= 0; $i--) {
            $date = strtotime("-$i days");
            $dateStart = strtotime(date('Y-m-d 00:00:00', $date));
            $dateEnd = strtotime(date('Y-m-d 23:59:59', $date));
            
            $stmt = $this->db->prepare("
                SELECT COUNT(*) as count 
                FROM topic 
                WHERE time_created >= ? AND time_created <= ?
            ");
            $stmt->execute([$dateStart, $dateEnd]);
            $trend[] = [
                'date' => date('Y-m-d', $date),
                'count' => (int)$stmt->fetch()['count']
            ];
        }
        
        return [
            'total_topics' => (int)$totalTopics,
            'new_topics' => (int)$newTopics,
            'blocked_topics' => (int)$blockedTopics,
            'sticky_topics' => (int)$stickyTopics,
            'done_topics' => (int)$doneTopics,
            'creation_trend' => $trend,
            'period' => $period
        ];
    }
    
    /**
     * Export topics to CSV
     */
    public function exportTopics($search, $status) {
        $where = "1=1";
        $params = [];
        
        if (!empty($search)) {
            $where .= " AND (title LIKE ? OR contents LIKE ?)";
            $searchTerm = "%$search%";
            $params[] = $searchTerm;
            $params[] = $searchTerm;
        }
        
        if ($status === 'active') {
            $where .= " AND block = 0";
        } elseif ($status === 'blocked') {
            $where .= " AND block = 1";
        } elseif ($status === 'sticky') {
            $where .= " AND stick = 1";
        }
        
        $sql = "SELECT 
                    id, title, topic, owner, user, 
                    FROM_UNIXTIME(time_created) as created, 
                    block, stick, done
                FROM topic 
                WHERE $where 
                ORDER BY time_created DESC";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute($params);
        $topics = $stmt->fetchAll();
        
        // Create CSV
        $output = fopen('php://temp', 'r+');
        
        // Header
        fputcsv($output, ['ID', 'Title', 'Topic ID', 'Owner', 'User', 'Created', 'Blocked', 'Sticky', 'Done']);
        
        // Data
        foreach ($topics as $topic) {
            fputcsv($output, [
                $topic['id'],
                $topic['title'],
                $topic['topic'],
                $topic['owner'],
                $topic['user'],
                $topic['created'],
                $topic['block'] ? 'Yes' : 'No',
                $topic['stick'] ? 'Yes' : 'No',
                $topic['done'] ? 'Yes' : 'No'
            ]);
        }
        
        rewind($output);
        $csv = stream_get_contents($output);
        fclose($output);
        
        return $csv;
    }
    
    // Helper methods
    
    private function getDateFilter($period) {
        switch ($period) {
            case 'day':
                return strtotime('today 00:00:00');
            case 'week':
                return strtotime('-7 days');
            case 'month':
                return strtotime('-30 days');
            case 'year':
                return strtotime('-365 days');
            default:
                return strtotime('today 00:00:00');
        }
    }
}