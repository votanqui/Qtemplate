<?php
// helpers/RateLimiter.php

class RateLimiter {
    private $db;
    
    public function __construct() {
        $this->db = Database::getInstance()->getConnection();
    }
    
   
    /**
     * Check if request is allowed
     * 
     * @param string $identifier IP address or user ID
     * @param string $endpoint API endpoint
     * @param int $maxAttempts Maximum attempts allowed
     * @param int $windowSeconds Time window in seconds
     * @param int $blockSeconds How long to block after exceeding limit
     * @return array ['allowed' => bool, 'remaining' => int, 'reset_at' => string]
     */
    public function check($identifier, $endpoint, $maxAttempts = 10, $windowSeconds = 60, $blockSeconds = 300) {
        error_log(">>> RateLimiter::check() called");
        error_log(">>> Identifier: $identifier, Endpoint: $endpoint, Max: $maxAttempts");
        
        // Clean old records
        $this->cleanup();
        
        // Check if currently blocked
        $stmt = $this->db->prepare("
            SELECT blocked_until 
            FROM rate_limit 
            WHERE identifier = ? AND endpoint = ? AND blocked_until > NOW()
        ");
        $stmt->execute([$identifier, $endpoint]);
        $blocked = $stmt->fetch();
        
        if ($blocked) {
            error_log(">>> ALREADY BLOCKED until: " . $blocked['blocked_until']);
            return [
                'allowed' => false,
                'remaining' => 0,
                'reset_at' => $blocked['blocked_until'],
                'retry_after' => strtotime($blocked['blocked_until']) - time(),
                'message' => 'spam ít thôi.'
            ];
        }
        
        // Get current attempts
        $stmt = $this->db->prepare("
            SELECT attempts, window_start 
            FROM rate_limit 
            WHERE identifier = ? AND endpoint = ?
        ");
        $stmt->execute([$identifier, $endpoint]);
        $record = $stmt->fetch();
        
        error_log(">>> Current record: " . json_encode($record));
        
        $now = time();
        
        if (!$record) {
            error_log(">>> First request - creating record");
            // First request - create new record
            $this->createRecord($identifier, $endpoint);
            return [
                'allowed' => true,
                'remaining' => $maxAttempts - 1,
                'reset_at' => date('Y-m-d H:i:s', $now + $windowSeconds),
                'message' => 'Request allowed'
            ];
        }
        
        $windowStart = strtotime($record['window_start']);
        $windowEnd = $windowStart + $windowSeconds;
        
        error_log(">>> Window: start=" . date('Y-m-d H:i:s', $windowStart) . ", end=" . date('Y-m-d H:i:s', $windowEnd) . ", now=" . date('Y-m-d H:i:s', $now));
        
        // Check if window has expired - reset counter
        if ($now > $windowEnd) {
            error_log(">>> Window expired - resetting");
            $this->resetRecord($identifier, $endpoint);
            return [
                'allowed' => true,
                'remaining' => $maxAttempts - 1,
                'reset_at' => date('Y-m-d H:i:s', $now + $windowSeconds),
                'message' => 'Request allowed'
            ];
        }
        
        error_log(">>> Current attempts: {$record['attempts']} / $maxAttempts");
        
        // Check if exceeded limit
        if ($record['attempts'] >= $maxAttempts) {
            error_log(">>> LIMIT EXCEEDED - BLOCKING!");
            // Block the identifier
            $blockedUntil = date('Y-m-d H:i:s', $now + $blockSeconds);
            $this->blockIdentifier($identifier, $endpoint, $blockedUntil);
            
            return [
                'allowed' => false,
                'remaining' => 0,
                'reset_at' => $blockedUntil,
                'retry_after' => $blockSeconds,
                'message' => 'Rate limit exceeded. Blocked for ' . $blockSeconds . ' seconds.'
            ];
        }
        
        // Increment attempts
        error_log(">>> Incrementing attempts");
        $this->incrementAttempts($identifier, $endpoint);
        
        $remaining = $maxAttempts - ($record['attempts'] + 1);
        error_log(">>> Remaining: $remaining");
        
        return [
            'allowed' => true,
            'remaining' => $remaining,
            'reset_at' => date('Y-m-d H:i:s', $windowEnd),
            'message' => 'Request allowed'
        ];
    }
    
    private function createRecord($identifier, $endpoint) {
        $stmt = $this->db->prepare("
            INSERT INTO rate_limit (identifier, endpoint, attempts, window_start) 
            VALUES (?, ?, 1, NOW())
            ON DUPLICATE KEY UPDATE 
                attempts = 1, 
                window_start = NOW(),
                blocked_until = NULL
        ");
        $stmt->execute([$identifier, $endpoint]);
    }
    
    private function resetRecord($identifier, $endpoint) {
        $stmt = $this->db->prepare("
            UPDATE rate_limit 
            SET attempts = 1, window_start = NOW(), blocked_until = NULL 
            WHERE identifier = ? AND endpoint = ?
        ");
        $stmt->execute([$identifier, $endpoint]);
    }
    
    private function incrementAttempts($identifier, $endpoint) {
        $stmt = $this->db->prepare("
            UPDATE rate_limit 
            SET attempts = attempts + 1 
            WHERE identifier = ? AND endpoint = ?
        ");
        $stmt->execute([$identifier, $endpoint]);
    }
    
    private function blockIdentifier($identifier, $endpoint, $blockedUntil) {
        $stmt = $this->db->prepare("
            UPDATE rate_limit 
            SET blocked_until = ? 
            WHERE identifier = ? AND endpoint = ?
        ");
        $stmt->execute([$blockedUntil, $identifier, $endpoint]);
    }
    
    private function cleanup() {
        // Delete records older than 24 hours
        $stmt = $this->db->prepare("
            DELETE FROM rate_limit 
            WHERE window_start < DATE_SUB(NOW(), INTERVAL 24 HOUR)
            AND (blocked_until IS NULL OR blocked_until < NOW())
        ");
        $stmt->execute();
    }
    
    /**
     * Get rate limit info for identifier
     */
    public function getInfo($identifier, $endpoint) {
        $stmt = $this->db->prepare("
            SELECT * FROM rate_limit 
            WHERE identifier = ? AND endpoint = ?
        ");
        $stmt->execute([$identifier, $endpoint]);
        return $stmt->fetch();
    }
    
    /**
     * Clear rate limit for identifier (admin function)
     */
    public function clear($identifier, $endpoint = null) {
        if ($endpoint) {
            $stmt = $this->db->prepare("
                DELETE FROM rate_limit 
                WHERE identifier = ? AND endpoint = ?
            ");
            $stmt->execute([$identifier, $endpoint]);
        } else {
            $stmt = $this->db->prepare("
                DELETE FROM rate_limit 
                WHERE identifier = ?
            ");
            $stmt->execute([$identifier]);
        }
    }
    
    /**
     * Get identifier from request (IP or User ID)
     */
    public static function getIdentifier($useUserId = false) {
        if ($useUserId && isset($_SERVER['HTTP_AUTHORIZATION'])) {
            // Try to get user ID from token
            $token = self::getBearerToken();
            if ($token) {
                $payload = JWTHelper::validateToken($token);
                if ($payload && isset($payload['user_id'])) {
                    return 'user_' . $payload['user_id'];
                }
            }
        }
        
        // Fallback to IP address
        $ip = self::getClientIP();
        return 'ip_' . $ip;
    }
    
    private static function getBearerToken() {
        $headers = getallheaders();
        if (isset($headers['Authorization'])) {
            $matches = [];
            if (preg_match('/Bearer\s+(.*)$/i', $headers['Authorization'], $matches)) {
                return $matches[1];
            }
        }
        return null;
    }
    
    private static function getClientIP() {
        $ip = $_SERVER['REMOTE_ADDR'] ?? '0.0.0.0';
        
        // Check for proxy headers
        if (!empty($_SERVER['HTTP_CLIENT_IP'])) {
            $ip = $_SERVER['HTTP_CLIENT_IP'];
        } elseif (!empty($_SERVER['HTTP_X_FORWARDED_FOR'])) {
            $ips = explode(',', $_SERVER['HTTP_X_FORWARDED_FOR']);
            $ip = trim($ips[0]);
        } elseif (!empty($_SERVER['HTTP_X_REAL_IP'])) {
            $ip = $_SERVER['HTTP_X_REAL_IP'];
        }
        
        return $ip;
    }
}