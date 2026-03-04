<?php
class ConfigService {
    private static $instance = null;
    private $db;
    private $cache = [];
    
    private function __construct() {
        $this->db = Database::getInstance()->getConnection();
        $this->loadAllConfigs();
    }
    
    public static function getInstance() {
        if (self::$instance === null) {
            self::$instance = new self();
        }
        return self::$instance;
    }
    
    private function loadAllConfigs() {
        $stmt = $this->db->query("SELECT config_key, config_value, config_type FROM system_config");
        while ($row = $stmt->fetch()) {
            $this->cache[$row['config_key']] = $this->parseValue($row['config_value'], $row['config_type']);
        }
    }
    
    private function parseValue($value, $type) {
        switch ($type) {
            case 'number':
                return is_numeric($value) ? (strpos($value, '.') !== false ? (float)$value : (int)$value) : $value;
            case 'json':
                return json_decode($value, true);
            case 'boolean':
                return filter_var($value, FILTER_VALIDATE_BOOLEAN);
            default:
                return $value;
        }
    }
    
    public function get($key, $default = null) {
        return $this->cache[$key] ?? $default;
    }
    
    public function set($key, $value) {
        $type = is_array($value) ? 'json' : (is_numeric($value) ? 'number' : 'string');
        $valueStr = is_array($value) ? json_encode($value) : $value;
        
        $stmt = $this->db->prepare("
            INSERT INTO system_config (config_key, config_value, config_type) 
            VALUES (?, ?, ?)
            ON DUPLICATE KEY UPDATE config_value = ?, updated_at = NOW()
        ");
        $stmt->execute([$key, $valueStr, $type, $valueStr]);
        
        $this->cache[$key] = $value;
        return true;
    }
}