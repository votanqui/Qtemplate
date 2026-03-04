<?php
// config/Config.php

class Config {
    // Main Database configuration (account)
    const DB_HOST = '14.225.212.100';
    const DB_NAME = 'account'; // Database chứa bảng servers
    const DB_USER = 'root';
    const DB_PASS = 'lalzpupiyn51srv';
    const DB_CHARSET = 'utf8mb4';
    
    // Không cần cấu hình game database cố định nữa

    const SEPAY_API_KEY = 'API_KEY_CUA_KVTEAM'; // API Key từ SePay
    const ACTIVATION_DESCRIPTION_PREFIX = 'kichhoat_';
     const VIETQR_ACCOUNT = '0919332046'; // Số tài khoản ngân hàng
    const VIETQR_BANK = 'MBBank'; // Mã ngân hàng
    const VIETQR_BANK_NAME = 'Ngân hàng Quân đội (MBBank)';
    const VIETQR_ACCOUNT_NAME = 'TÊN CHỦ TÀI KHOẢN'; // Thay bằng tên thật
    
    const RECHARGE_XU_PREFIX = 'napxu';      // Format: napxu_{userId}_{serverId
    const RECHARGE_LUONG_PREFIX = 'napluong'; // Format: napluong_{userId}_{serverId}

     const XU_EXCHANGE_RATES = [
        10000 => 10000000,
        20000 => 20000000,
        30000 => 30000000,
        50000 => 60000000,
        100000 => 130000000,
        200000 => 280000000,
        300000 => 435000000,
        500000 => 750000000,
        1000000 => 1700000000,
    ];
    
    // Tỷ giá nạp lượng (VNĐ -> Lượng)
    // 20 VNĐ = 1 lượng, 50% lượng khóa
    const LUONG_EXCHANGE_RATE = 20; // 20 VNĐ = 1 lượng
    const LUONG_KHOA_PERCENT = 0.5; // 50% lượng khóa
    const LUONG_BONUS_MULTIPLIER = 1;
    // Activation Configuration
    const ACTIVATION_AMOUNT = 20000; // 20,000 VNĐ
    const ACTIVATION_REWARD_XU = 10000000; // 10 triệu xu
    const ACTIVATION_REWARD_LUONG = 50000; // 50,000 lượng
    // JWT configuration
    const JWT_SECRET = 'your-secret-key-change-this-in-production';
    const JWT_ACCESS_EXPIRY = 7200; // 1 hour
    const JWT_REFRESH_EXPIRY = 2592000; // 30 days
    
    // API configuration
    const API_VERSION = 'v1';
    const TIMEZONE = 'Asia/Ho_Chi_Minh';
    
    // Server configuration
    const DEFAULT_SERVER_ID = 1; // Server mặc định nếu không chỉ định
}

date_default_timezone_set(Config::TIMEZONE);