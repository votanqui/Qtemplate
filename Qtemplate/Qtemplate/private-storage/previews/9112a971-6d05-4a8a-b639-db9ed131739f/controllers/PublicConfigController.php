<?php
// controllers/PublicConfigController.php

class PublicConfigController {
    private $configService;
    
    public function __construct() {
        $this->configService = ConfigService::getInstance();
    }
    
    /**
     * Lấy tỷ giá nạp tiền cho frontend
     * GET /config/rates
     */
    public function getRates() {
        try {
            $rates = [
                // Tỷ giá xu
                'xu_rates' => $this->configService->get('xu_exchange_rates', []),
                'xu_bonus_multiplier' => $this->configService->get('xu_bonus_multiplier', 1),
                
                // Tỷ giá lượng
                'luong_rate' => $this->configService->get('luong_exchange_rate', 20),
                'luong_khoa_percent' => $this->configService->get('luong_khoa_percent', 0.5),
                'luong_bonus_multiplier' => $this->configService->get('luong_bonus_multiplier', 1),
                
                // Kích hoạt
                'activation_amount' => $this->configService->get('activation_amount', 20000),
                'activation_reward_xu' => $this->configService->get('activation_reward_xu', 10000000),
                'activation_reward_luong' => $this->configService->get('activation_reward_luong', 50000),
                
                // Prefix
                'recharge_xu_prefix' => $this->configService->get('recharge_xu_prefix', 'napxu'),
                'recharge_luong_prefix' => $this->configService->get('recharge_luong_prefix', 'napluong'),
                'activation_prefix' => $this->configService->get('activation_description_prefix', 'kichhoat')
            ];
            
            Response::success($rates, 'Lấy tỷ giá thành công');
        } catch (Exception $e) {
            Response::error('Không thể lấy tỷ giá: ' . $e->getMessage(), 500);
        }
    }
    
    /**
     * Lấy cấu hình xu
     * GET /config/xu
     */
    public function getXuConfig() {
        try {
            $config = [
                'rates' => $this->configService->get('xu_exchange_rates', []),
                'bonus_multiplier' => $this->configService->get('xu_bonus_multiplier', 1),
                'prefix' => $this->configService->get('recharge_xu_prefix', 'napxu')
            ];
            
            Response::success($config, 'Lấy cấu hình xu thành công');
        } catch (Exception $e) {
            Response::error('Không thể lấy cấu hình xu: ' . $e->getMessage(), 500);
        }
    }
    
    /**
     * Lấy cấu hình lượng
     * GET /config/luong
     */
    public function getLuongConfig() {
        try {
            $config = [
                'rate' => $this->configService->get('luong_exchange_rate', 20),
                'khoa_percent' => $this->configService->get('luong_khoa_percent', 0.5),
                'bonus_multiplier' => $this->configService->get('luong_bonus_multiplier', 1),
                'prefix' => $this->configService->get('recharge_luong_prefix', 'napluong')
            ];
            
            Response::success($config, 'Lấy cấu hình lượng thành công');
        } catch (Exception $e) {
            Response::error('Không thể lấy cấu hình lượng: ' . $e->getMessage(), 500);
        }
    }
    
    /**
     * Lấy cấu hình kích hoạt
     * GET /config/activation
     */
    public function getActivationConfig() {
        try {
            $config = [
                'amount' => $this->configService->get('activation_amount', 20000),
                'reward_xu' => $this->configService->get('activation_reward_xu', 10000000),
                'reward_luong' => $this->configService->get('activation_reward_luong', 50000),
                'prefix' => $this->configService->get('activation_description_prefix', 'kichhoat')
            ];
            
            Response::success($config, 'Lấy cấu hình kích hoạt thành công');
        } catch (Exception $e) {
            Response::error('Không thể lấy cấu hình kích hoạt: ' . $e->getMessage(), 500);
        }
    }
    
    /**
     * Tính toán xu sẽ nhận được
     * POST /config/calculate-xu
     */
    public function calculateXu() {
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!isset($input['amount']) || !is_numeric($input['amount'])) {
            Response::error('Số tiền không hợp lệ', 400);
        }
        
        try {
            $amount = (int)$input['amount'];
            $rates = $this->configService->get('xu_exchange_rates', []);
            $bonusMultiplier = $this->configService->get('xu_bonus_multiplier', 1);
            
            // Tìm xu tương ứng từ bảng tỷ giá
            $xu = isset($rates[$amount]) ? $rates[$amount] : 0;
            
            // Áp dụng bonus
            $bonusXu = floor($xu * ($bonusMultiplier - 1));
            $totalXu = $xu + $bonusXu;
            
            $result = [
                'amount' => $amount,
                'base_xu' => $xu,
                'bonus_xu' => $bonusXu,
                'total_xu' => $totalXu,
                'bonus_multiplier' => $bonusMultiplier
            ];
            
            Response::success($result, 'Tính toán thành công');
        } catch (Exception $e) {
            Response::error('Không thể tính toán: ' . $e->getMessage(), 500);
        }
    }
    
    /**
     * Tính toán lượng sẽ nhận được
     * POST /config/calculate-luong
     */
    public function calculateLuong() {
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!isset($input['amount']) || !is_numeric($input['amount'])) {
            Response::error('Số tiền không hợp lệ', 400);
        }
        
        try {
            $amount = (float)$input['amount'];
            $rate = $this->configService->get('luong_exchange_rate', 20);
            $khoaPercent = $this->configService->get('luong_khoa_percent', 0.5);
            $bonusMultiplier = $this->configService->get('luong_bonus_multiplier', 1);
            
            // Tính lượng cơ bản
            $luong = floor($amount / $rate);
            
            // Tính lượng khóa
            $luongKhoa = floor($luong * $khoaPercent * $bonusMultiplier);
            
            $result = [
                'amount' => $amount,
                'luong' => $luong,
                'luong_khoa' => $luongKhoa,
                'total' => $luong + $luongKhoa,
                'rate' => $rate,
                'khoa_percent' => $khoaPercent,
                'bonus_multiplier' => $bonusMultiplier
            ];
            
            Response::success($result, 'Tính toán thành công');
        } catch (Exception $e) {
            Response::error('Không thể tính toán: ' . $e->getMessage(), 500);
        }
    }
}