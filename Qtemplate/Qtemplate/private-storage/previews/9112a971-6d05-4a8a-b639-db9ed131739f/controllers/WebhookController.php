<?php
// controllers/WebhookController.php

require_once __DIR__ . '/../services/WebhookService.php';
require_once __DIR__ . '/../helpers/Response.php';

class WebhookController {
    private $webhookService;
    
    public function __construct() {
        $this->webhookService = new WebhookService();
    }
    
    /**
     * POST /webhook/sepay
     * Xử lý webhook từ SePay (tất cả loại giao dịch)
     */
    public function sepay() {
        try {
            $headers = getallheaders();
            $apiKey = $headers['Authorization'] ?? '';
            
            $input = json_decode(file_get_contents('php://input'), true);
            
            if (!$input) {
                Response::error('Invalid request body', 400);
            }
            
            if (!$this->validateApiKey($apiKey)) {
                Response::unauthorized('Invalid API Key');
            }
            
            $result = $this->webhookService->processSepayWebhook($input);
            
            if ($result['success']) {
                Response::success($result['data'], $result['message']);
            } else {
                Response::error($result['message'], 400);
            }
            
        } catch (Exception $e) {
            error_log("Webhook error: " . $e->getMessage());
            Response::error('Internal server error: ' . $e->getMessage(), 500);
        }
    }
    
    /**
     * POST /webhook/sepay/xu
     * Webhook riêng cho nạp xu
     */
    public function sepayXu() {
        try {
            $headers = getallheaders();
            $apiKey = $headers['Authorization'] ?? '';
            
            $input = json_decode(file_get_contents('php://input'), true);
            
            if (!$input) {
                Response::error('Invalid request body', 400);
            }
            
            if (!$this->validateApiKey($apiKey)) {
                Response::unauthorized('Invalid API Key');
            }
            
            $result = $this->webhookService->processRechargeXu($input);
            
            if ($result['success']) {
                Response::success($result['data'], $result['message']);
            } else {
                Response::error($result['message'], 400);
            }
            
        } catch (Exception $e) {
            error_log("Webhook XU error: " . $e->getMessage());
            Response::error('Internal server error: ' . $e->getMessage(), 500);
        }
    }
    
    /**
     * POST /webhook/sepay/luong
     * Webhook riêng cho nạp lượng
     */
    public function sepayLuong() {
        try {
            $headers = getallheaders();
            $apiKey = $headers['Authorization'] ?? '';
            
            $input = json_decode(file_get_contents('php://input'), true);
            
            if (!$input) {
                Response::error('Invalid request body', 400);
            }
            
            if (!$this->validateApiKey($apiKey)) {
                Response::unauthorized('Invalid API Key');
            }
            
            $result = $this->webhookService->processRechargeLuong($input);
            
            if ($result['success']) {
                Response::success($result['data'], $result['message']);
            } else {
                Response::error($result['message'], 400);
            }
            
        } catch (Exception $e) {
            error_log("Webhook LUONG error: " . $e->getMessage());
            Response::error('Internal server error: ' . $e->getMessage(), 500);
        }
    }
    
    /*  private function validateApiKey() {
    $headers = getallheaders();

    if (!isset($headers['Authorization'])) {
        Response::unauthorized('Missing Authorization header');
    }

    $apiKey = trim($headers['Authorization']);

    // API key đúng (có thể hardcode hoặc lấy từ config)
    $expectedApiKey = ConfigService::getInstance()->get('sepay_api_key');

    if ($apiKey !== $expectedApiKey) {
        Response::unauthorized('Invalid API Key');
    }

    return true;
}
     */
   private function validateApiKey() {
        $headers = getallheaders();
        
        // Log để debug
        error_log("All headers: " . print_r($headers, true));
        
        // SePay gửi với format: "Authorization: Apikey YOUR_API_KEY"
        if (!isset($headers['Authorization'])) {
            error_log("Missing Authorization header");
            Response::unauthorized('Missing Authorization header');
        }
        
        $authHeader = $headers['Authorization'];
        error_log("Authorization header: " . $authHeader);
        
        // Extract API Key từ header
        // Format: "Apikey API_KEY_CUA_BAN"
        if (!preg_match('/^Apikey\s+(.+)$/i', $authHeader, $matches)) {
            error_log("Invalid Authorization format: " . $authHeader);
            Response::unauthorized('Invalid Authorization format');
        }
        
        $apiKey = trim($matches[1]);
        error_log("Extracted API Key: " . $apiKey);
        
        // So sánh với API Key trong config
        $config = ConfigService::getInstance();
        $expectedApiKey = $config->get('sepay_api_key');
        
        error_log("Expected API Key: " . $expectedApiKey);
        
        if ($apiKey !== $expectedApiKey) {
            error_log("API Key mismatch - Received: '" . $apiKey . "' | Expected: '" . $expectedApiKey . "'");
            Response::unauthorized('Invalid API Key');
        }
        
        error_log("API Key validation successful");
        return true;
    }
}