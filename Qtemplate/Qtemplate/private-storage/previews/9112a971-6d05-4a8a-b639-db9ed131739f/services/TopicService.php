<?php
// services/TopicService.php

class TopicService {
    private $db;
    
    public function __construct() {
        $this->db = Database::getInstance()->getConnection();
    }
    
    /**
     * Lấy danh sách bài viết với phân trang, lọc và sắp xếp
     */
    public function getTopics($page, $limit, $filters, $sort) {
        $offset = ($page - 1) * $limit;
        
        // Build WHERE clause
        $where = ['1=1'];
        $params = [];
        
        if ($filters['topic'] !== null) {
            $where[] = 'topic = ?';
            $params[] = $filters['topic'];
        }
        
        if ($filters['owner'] !== null) {
            $where[] = 'owner = ?';
            $params[] = $filters['owner'];
        }
        
        if ($filters['block'] !== null) {
            $where[] = 'block = ?';
            $params[] = $filters['block'];
        }
        
        if ($filters['stick'] !== null) {
            $where[] = 'stick = ?';
            $params[] = $filters['stick'];
        }
        
        if ($filters['done'] !== null) {
            $where[] = 'done = ?';
            $params[] = $filters['done'];
        }
        
        if (!empty($filters['search'])) {
            $where[] = 'title LIKE ?';
            $params[] = '%' . $filters['search'] . '%';
        }
        
        $whereClause = implode(' AND ', $where);
        
        // Build ORDER BY clause
        $orderBy = match($sort) {
            'oldest' => 'time_created ASC',
            'title' => 'title ASC',
            default => 'time_created DESC' // newest
        };
        
        // Get total count
        $countSql = "SELECT COUNT(*) as total FROM topic WHERE {$whereClause}";
        $countStmt = $this->db->prepare($countSql);
        $countStmt->execute($params);
        $total = $countStmt->fetch()['total'];
        
        // Get topics
        $sql = "
            SELECT 
                id,
                title,
                topic,
                thread,
                tags,
                owner,
                user,
                SUBSTRING(contents, 1, 200) as preview,
                time_created,
                block,
                stick,
                done,
                FROM_UNIXTIME(time_created) as created_date
            FROM topic 
            WHERE {$whereClause}
            ORDER BY {$orderBy}
            LIMIT ? OFFSET ?
        ";
        
        $params[] = $limit;
        $params[] = $offset;
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute($params);
        $topics = $stmt->fetchAll();
        
        // Format data
        $formattedTopics = array_map(function($topic) {
            return [
                'id' => (int)$topic['id'],
                'title' => $topic['title'],
                'topic' => (int)$topic['topic'],
                'thread' => (int)$topic['thread'],
                'tags' => (int)$topic['tags'],
                'owner' => (int)$topic['owner'],
                'user' => $topic['user'],
                'preview' => $topic['preview'] . (strlen($topic['preview']) >= 200 ? '...' : ''),
                'time_created' => (int)$topic['time_created'],
                'created_date' => $topic['created_date'],
                'is_blocked' => (bool)$topic['block'],
                'is_sticky' => (bool)$topic['stick'],
                'is_done' => (bool)$topic['done']
            ];
        }, $topics);
        
        return [
            'topics' => $formattedTopics,
            'pagination' => [
                'page' => $page,
                'limit' => $limit,
                'total' => (int)$total,
                'total_pages' => (int)ceil($total / $limit),
                'has_next' => ($page * $limit) < $total,
                'has_prev' => $page > 1
            ]
        ];
    }
    
    /**
     * Lấy chi tiết bài viết theo ID
     */
    public function getTopicById($id) {
        $sql = "
            SELECT 
                t.id,
                t.title,
                t.topic,
                t.thread,
                t.tags,
                t.owner,
                t.user,
                t.contents,
                t.time_created,
                t.block,
                t.stick,
                t.done,
                FROM_UNIXTIME(t.time_created) as created_date,
                u.username as owner_username,
                u.phone as owner_phone,
                u.email as owner_email
            FROM topic t
            LEFT JOIN team_user u ON t.owner = u.id
            WHERE t.id = ?
        ";
        
        $stmt = $this->db->prepare($sql);
        $stmt->execute([$id]);
        $topic = $stmt->fetch();
        
        if (!$topic) {
            return null;
        }
        
        // Format data
        return [
            'id' => (int)$topic['id'],
            'title' => $topic['title'],
            'topic' => (int)$topic['topic'],
            'thread' => (int)$topic['thread'],
            'tags' => (int)$topic['tags'],
            'owner' => [
                'id' => (int)$topic['owner'],
                'username' => $topic['owner_username'],
                'user' => $topic['user'],
                'phone' => $topic['owner_phone'],
                'email' => $topic['owner_email']
            ],
            'contents' => $topic['contents'],
            'time_created' => (int)$topic['time_created'],
            'created_date' => $topic['created_date'],
            'status' => [
                'is_blocked' => (bool)$topic['block'],
                'is_sticky' => (bool)$topic['stick'],
                'is_done' => (bool)$topic['done']
            ]
        ];
    }
}