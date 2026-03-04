<?php
// middlewares/RateLimitMiddleware.php

error_log("✓ RateLimitMiddleware.php loaded");

class RateLimitMiddleware {
    
    /**
     * Apply rate limiting to request
     * 
     * @param string $endpoint
     * @param int $maxAttempts
     * @param int $windowSeconds
     * @param int $blockSeconds
     * @param bool $useUserId Use user ID instead of IP
     */
    public static function apply($endpoint, $maxAttempts = 10, $windowSeconds = 60, $blockSeconds = 300, $useUserId = false) {
        error_log("=== RATE LIMIT CHECK ===");
        error_log("Endpoint: $endpoint");
        error_log("Max Attempts: $maxAttempts");
        error_log("Window: $windowSeconds seconds");
        
        $rateLimiter = new RateLimiter();
        $identifier = RateLimiter::getIdentifier($useUserId);
        
        error_log("Identifier: $identifier");
        
        $result = $rateLimiter->check($identifier, $endpoint, $maxAttempts, $windowSeconds, $blockSeconds);
        
        error_log("Result: " . json_encode($result));
        
        // Add rate limit headers
        header('X-RateLimit-Limit: ' . $maxAttempts);
        header('X-RateLimit-Remaining: ' . $result['remaining']);
        header('X-RateLimit-Reset: ' . strtotime($result['reset_at']));
        
        if (!$result['allowed']) {
            error_log("❌ REQUEST BLOCKED!");
            header('Retry-After: ' . $result['retry_after']);
            Response::error($result['message'], 429, [
                'retry_after' => $result['retry_after'],
                'reset_at' => $result['reset_at']
            ]);
        }
        
        error_log("✓ REQUEST ALLOWED");
        return true;
    }
    
    /**
     * Preset: Strict rate limit for authentication endpoints
     * 5 attempts per 5 minutes, block for 15 minutes
     */
    public static function authStrict($endpoint) {
        return self::apply($endpoint, 5, 300, 900, false);
    }
    
    /**
     * Preset: Normal rate limit for authentication endpoints
     * 10 attempts per minute, block for 5 minutes
     */
    public static function authNormal($endpoint) {
        return self::apply($endpoint, 10, 60, 300, false);
    }
    
    /**
     * Preset: Lenient rate limit for general API
     * 100 requests per minute
     */
    public static function apiLenient($endpoint) {
        return self::apply($endpoint, 100, 60, 60, true);
    }
    
    /**
     * Preset: Moderate rate limit for general API
     * 30 requests per minute
     */
    public static function apiModerate($endpoint) {
        return self::apply($endpoint, 30, 60, 120, true);
    }
    
    /**
     * Preset: Strict rate limit for sensitive operations
     * 5 requests per minute, block for 10 minutes
     */
    public static function apiStrict($endpoint) {
        return self::apply($endpoint, 5, 60, 600, true);
    }
}