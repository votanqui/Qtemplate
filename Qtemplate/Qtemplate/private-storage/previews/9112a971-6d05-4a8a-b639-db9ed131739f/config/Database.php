<?php
// config/Database.php

class Database {
    private static $accountInstance = null;
    private static $gameInstances = []; // Mảng lưu các kết nối game theo server_id
    
    private $connection;
    
    private function __construct($dbConfig) {
        try {
            $dsn = "mysql:host=" . $dbConfig['host'] . 
                   ";dbname=" . $dbConfig['name'] . 
                   ";port=" . $dbConfig['port'] . 
                   ";charset=" . $dbConfig['charset'];
            $options = [
                PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
                PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
                PDO::ATTR_EMULATE_PREPARES => false,
            ];
            
            $this->connection = new PDO($dsn, $dbConfig['user'], $dbConfig['pass'], $options);
        } catch (PDOException $e) {
            throw new Exception("Database connection failed: " . $e->getMessage());
        }
    }
    
    /**
     * Get Account Database Instance (Singleton)
     */
    public static function getInstance() {
        if (self::$accountInstance === null) {
            $config = [
                'host' => Config::DB_HOST,
                'name' => Config::DB_NAME,
                'user' => Config::DB_USER,
                'pass' => Config::DB_PASS,
                'port' => 3306,
                'charset' => Config::DB_CHARSET
            ];
            self::$accountInstance = new self($config);
        }
        return self::$accountInstance;
    }
    
    /**
     * Get Game Database Instance by server_id
     * @param int $server_id ID của server trong bảng servers
     * @return Database instance cho game database
     */
    public static function getGameInstance($server_id = null) {
        // Nếu không chỉ định server_id, sử dụng default
        if ($server_id === null) {
            $server_id = Config::DEFAULT_SERVER_ID;
        }
        
        // Kiểm tra nếu đã có instance cho server này
        if (!isset(self::$gameInstances[$server_id])) {
            // Lấy thông tin server từ database account
            $serverConfig = self::getServerConfig($server_id);
            
            if (!$serverConfig) {
                throw new Exception("Server configuration not found for server_id: " . $server_id);
            }
            
            // Tạo instance mới
            $config = [
                'host' => $serverConfig['db_host'],
                'name' => $serverConfig['db_name'],
                'user' => $serverConfig['db_user'],
                'pass' => $serverConfig['db_pass'],
                'port' => $serverConfig['db_port'],
                'charset' => Config::DB_CHARSET
            ];
            
            self::$gameInstances[$server_id] = new self($config);
        }
        
        return self::$gameInstances[$server_id];
    }
    
    /**
     * Lấy cấu hình server từ database account
     * @param int $server_id
     * @return array|null
     */
    private static function getServerConfig($server_id) {
        try {
            // Lấy kết nối account database
            $accountDb = self::getInstance();
            $pdo = $accountDb->getConnection();
            
            // Query lấy thông tin server
            $stmt = $pdo->prepare("
                SELECT db_host, db_name, db_user, db_pass, db_port 
                FROM servers 
                WHERE server_id = ? AND status = 1
            ");
            $stmt->execute([$server_id]);
            $config = $stmt->fetch();
            
            return $config ?: null;
            
        } catch (Exception $e) {
            throw new Exception("Failed to get server config: " . $e->getMessage());
        }
    }
    
    /**
     * Lấy danh sách tất cả server đang active
     * @return array
     */
    public static function getAllActiveServers() {
        try {
            $accountDb = self::getInstance();
            $pdo = $accountDb->getConnection();
            
            $stmt = $pdo->query("
                SELECT server_id, server_name, db_name, db_host, db_port, created_at 
                FROM servers 
                WHERE status = 1 
                ORDER BY server_id ASC
            ");
            
            return $stmt->fetchAll();
            
        } catch (Exception $e) {
            throw new Exception("Failed to get server list: " . $e->getMessage());
        }
    }
    
    public function getConnection() {
        return $this->connection;
    }
    
    private function __clone() {}
    
    public function __wakeup() {
        throw new Exception("Cannot unserialize singleton");
    }
}