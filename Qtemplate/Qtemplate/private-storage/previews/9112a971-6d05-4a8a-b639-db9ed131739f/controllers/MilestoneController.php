<?php
// controllers/MilestoneController.php

require_once __DIR__ . '/../services/MilestoneService.php';
require_once __DIR__ . '/../helpers/Response.php';

class MilestoneController {
    private $milestoneService;
    
    public function __construct() {
        $this->milestoneService = new MilestoneService();
    }
    
    /**
     * Lấy danh sách tất cả các mốc nạp đang active
     */
    public function getAllMilestones() {
        try {
            $result = $this->milestoneService->getAllMilestones();
            
            if ($result['success']) {
                Response::success($result['data'], 'Lấy danh sách mốc nạp thành công');
            } else {
                Response::error($result['message'], 400);
            }
            
        } catch (Exception $e) {
            error_log("Get all milestones error: " . $e->getMessage());
            Response::error('Lỗi hệ thống: ' . $e->getMessage(), 500);
        }
    }
    
    /**
     * Lấy thông tin tiến độ nạp của user
     */
    public function getUserProgress() {
        try {
            $userId = $_GET['user_id'] ?? null;
            $serverId = $_GET['server_id'] ?? 1;
            
            if (!$userId) {
                Response::error('Thiếu tham số user_id', 400);
            }
            
            $result = $this->milestoneService->getUserProgress((int)$userId, (int)$serverId);
            
            if ($result['success']) {
                Response::success($result['data'], 'Lấy tiến độ nạp thành công');
            } else {
                Response::error($result['message'], 400);
            }
            
        } catch (Exception $e) {
            error_log("Get user progress error: " . $e->getMessage());
            Response::error('Lỗi hệ thống: ' . $e->getMessage(), 500);
        }
    }
    
    /**
     * Lấy danh sách các mốc đã nhận của user
     */
    public function getClaimedMilestones() {
        try {
            $userId = $_GET['user_id'] ?? null;
            $serverId = $_GET['server_id'] ?? 1;
            
            if (!$userId) {
                Response::error('Thiếu tham số user_id', 400);
            }
            
            $result = $this->milestoneService->getClaimedMilestones((int)$userId, (int)$serverId);
            
            if ($result['success']) {
                Response::success($result['data'], 'Lấy danh sách mốc đã nhận thành công');
            } else {
                Response::error($result['message'], 400);
            }
            
        } catch (Exception $e) {
            error_log("Get claimed milestones error: " . $e->getMessage());
            Response::error('Lỗi hệ thống: ' . $e->getMessage(), 500);
        }
    }
    
    /**
     * Lấy danh sách các mốc có thể nhận
     */
    public function getAvailableMilestones() {
        try {
            $userId = $_GET['user_id'] ?? null;
            $serverId = $_GET['server_id'] ?? 1;
            
            if (!$userId) {
                Response::error('Thiếu tham số user_id', 400);
            }
            
            $result = $this->milestoneService->getAvailableMilestones((int)$userId, (int)$serverId);
            
            if ($result['success']) {
                Response::success($result['data'], 'Lấy danh sách mốc có thể nhận thành công');
            } else {
                Response::error($result['message'], 400);
            }
            
        } catch (Exception $e) {
            error_log("Get available milestones error: " . $e->getMessage());
            Response::error('Lỗi hệ thống: ' . $e->getMessage(), 500);
        }
    }
    
    /**
     * Lấy tổng quan tất cả thông tin milestone của user
     */
    public function getUserOverview() {
        try {
            $userId = $_GET['user_id'] ?? null;
            $serverId = $_GET['server_id'] ?? 1;
            
            if (!$userId) {
                Response::error('Thiếu tham số user_id', 400);
            }
            
            $result = $this->milestoneService->getUserMilestoneOverview((int)$userId, (int)$serverId);
            
            if ($result['success']) {
                Response::success($result['data'], 'Lấy tổng quan mốc nạp thành công');
            } else {
                Response::error($result['message'], 400);
            }
            
        } catch (Exception $e) {
            error_log("Get milestone overview error: " . $e->getMessage());
            Response::error('Lỗi hệ thống: ' . $e->getMessage(), 500);
        }
    }
    
    /**
     * Nhận phần thưởng mốc nạp
     */
    public function claimReward() {
        try {
            $input = json_decode(file_get_contents('php://input'), true);
            
            if (!$input) {
                Response::error('Invalid request body', 400);
            }
            
            $userId = $input['user_id'] ?? null;
            $username = $input['username'] ?? null;
            $milestoneId = $input['milestone_id'] ?? null;
            $serverId = $input['server_id'] ?? 1;
            
            if (!$userId) {
                Response::error('Thiếu tham số user_id', 400);
            }
            
            if (!$username) {
                Response::error('Thiếu tham số username', 400);
            }
            
            if (!$milestoneId) {
                Response::error('Thiếu tham số milestone_id', 400);
            }
            
            $result = $this->milestoneService->claimMilestone(
                (int)$userId, 
                $username, 
                (int)$serverId, 
                (int)$milestoneId
            );
            
            if ($result['success']) {
                Response::success($result['data'], $result['message']);
            } else {
                Response::error($result['message'], 400);
            }
            
        } catch (Exception $e) {
            error_log("Claim milestone error: " . $e->getMessage());
            Response::error('Lỗi hệ thống: ' . $e->getMessage(), 500);
        }
    }
    
    /**
     * Kiểm tra điều kiện nhận mốc
     */
    public function checkEligibility() {
        try {
            $userId = $_GET['user_id'] ?? null;
            $milestoneId = $_GET['milestone_id'] ?? null;
            $serverId = $_GET['server_id'] ?? 1;
            
            if (!$userId || !$milestoneId) {
                Response::error('Thiếu tham số user_id hoặc milestone_id', 400);
            }
            
            $result = $this->milestoneService->getAvailableMilestones((int)$userId, (int)$serverId);
            
            if (!$result['success']) {
                Response::error($result['message'], 400);
            }
            
            $availableMilestones = $result['data'];
            $milestoneId = (int)$milestoneId;
            
            $isEligible = false;
            $milestoneData = null;
            
            foreach ($availableMilestones as $milestone) {
                if ($milestone['id'] === $milestoneId) {
                    $isEligible = true;
                    $milestoneData = $milestone;
                    break;
                }
            }
            
            if ($isEligible) {
                Response::success([
                    'eligible' => true,
                    'milestone' => $milestoneData
                ], 'Bạn đủ điều kiện nhận mốc này');
            } else {
                Response::success([
                    'eligible' => false,
                    'milestone' => null
                ], 'Bạn chưa đủ điều kiện hoặc đã nhận mốc này rồi');
            }
            
        } catch (Exception $e) {
            error_log("Check milestone eligibility error: " . $e->getMessage());
            Response::error('Lỗi hệ thống: ' . $e->getMessage(), 500);
        }
    }
}