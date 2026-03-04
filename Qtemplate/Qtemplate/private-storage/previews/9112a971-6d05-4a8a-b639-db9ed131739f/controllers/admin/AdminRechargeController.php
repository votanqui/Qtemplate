<?php
// controllers/admin/AdminRechargeController.php

class AdminRechargeController {
    private $rechargeService;
    private $authService;
    
    public function __construct() {
        $this->rechargeService = new AdminRechargeService();
        $this->authService = new AuthService();
    }
    
    /**
     * Lấy danh sách giao dịch nạp tiền
     * GET /admin/recharge/transactions?page=1&limit=20&status=all&type=all&user_id=1
     */
    public function getTransactions() {
        $this->requireAdmin();
        
        $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
        $limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 20;
        $status = $_GET['status'] ?? 'all';
        $type = $_GET['type'] ?? 'all';
        $userId = $_GET['user_id'] ?? null;
        $search = $_GET['search'] ?? '';
        $dateFrom = $_GET['date_from'] ?? null;
        $dateTo = $_GET['date_to'] ?? null;
        
        $result = $this->rechargeService->getTransactions(
            $page, $limit, $status, $type, $userId, $search, $dateFrom, $dateTo
        );
        
        Response::success($result, 'Lấy danh sách giao dịch thành công');
    }
    
    /**
     * Lấy chi tiết giao dịch
     * GET /admin/recharge/transactions/:id
     */
    public function getTransactionDetail($transactionId) {
        $this->requireAdmin();
        
        if (empty($transactionId) || !is_numeric($transactionId)) {
            Response::error('ID giao dịch không hợp lệ', 400);
        }
        
        $result = $this->rechargeService->getTransactionDetail($transactionId);
        
        if (!$result) {
            Response::notFound('Không tìm thấy giao dịch');
        }
        
        Response::success($result, 'Lấy thông tin giao dịch thành công');
    }
    
    /**
     * Cập nhật trạng thái giao dịch
     * PUT /admin/recharge/transactions/:id
     */
    public function updateTransaction($transactionId) {
        $this->requireAdmin();
        
        if (empty($transactionId) || !is_numeric($transactionId)) {
            Response::error('ID giao dịch không hợp lệ', 400);
        }
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input || !isset($input['status'])) {
            Response::error('Dữ liệu không hợp lệ', 400);
        }
        
        $result = $this->rechargeService->updateTransactionStatus(
            $transactionId, 
            $input['status'],
            $input['note'] ?? ''
        );
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['transaction'], 'Cập nhật giao dịch thành công');
    }
    
    /**
     * Xử lý lại giao dịch thất bại
     * POST /admin/recharge/transactions/:id/retry
     */
    public function retryTransaction($transactionId) {
        $this->requireAdmin();
        
        if (empty($transactionId) || !is_numeric($transactionId)) {
            Response::error('ID giao dịch không hợp lệ', 400);
        }
        
        $result = $this->rechargeService->retryTransaction($transactionId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['transaction'], 'Xử lý lại giao dịch thành công');
    }
    
    /**
     * Lấy top nạp
     * GET /admin/recharge/top?server_id=1&limit=100&period=all
     */
    public function getTopRecharge() {
        $this->requireAdmin();
        
        $serverId = $_GET['server_id'] ?? null;
        $limit = isset($_GET['limit']) ? min(1000, max(1, intval($_GET['limit']))) : 100;
        $period = $_GET['period'] ?? 'all'; // all, today, week, month
        
        $result = $this->rechargeService->getTopRecharge($serverId, $limit, $period);
        
        Response::success($result, 'Lấy bảng xếp hạng nạp thành công');
    }
    
    /**
     * Lấy thống kê nạp tiền
     * GET /admin/recharge/stats?period=day&server_id=1
     */
    public function getStats() {
        $this->requireAdmin();
        
        $period = $_GET['period'] ?? 'day';
        $serverId = $_GET['server_id'] ?? null;
        
        $result = $this->rechargeService->getRechargeStats($period, $serverId);
        
        Response::success($result, 'Lấy thống kê thành công');
    }
    
    /**
     * Lấy lịch sử nạp của user
     * GET /admin/recharge/user/:userId/history
     */
    public function getUserRechargeHistory($userId) {
        $this->requireAdmin();
        
        if (empty($userId) || !is_numeric($userId)) {
            Response::error('ID người dùng không hợp lệ', 400);
        }
        
        $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
        $limit = isset($_GET['limit']) ? min(50, max(1, intval($_GET['limit']))) : 20;
        
        $result = $this->rechargeService->getUserRechargeHistory($userId, $page, $limit);
        
        Response::success($result, 'Lấy lịch sử nạp thành công');
    }
    
    /**
     * Lấy webhook logs
     * GET /admin/recharge/webhook-logs?page=1&limit=50
     */
    public function getWebhookLogs() {
        $this->requireAdmin();
        
        $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
        $limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 50;
        $type = $_GET['type'] ?? 'all';
        
        $result = $this->rechargeService->getWebhookLogs($page, $limit, $type);
        
        Response::success($result, 'Lấy webhook logs thành công');
    }
    
    /**
     * Export giao dịch ra CSV
     * GET /admin/recharge/export
     */
    public function exportTransactions() {
        $this->requireAdmin();
        
        $status = $_GET['status'] ?? 'all';
        $type = $_GET['type'] ?? 'all';
        $dateFrom = $_GET['date_from'] ?? null;
        $dateTo = $_GET['date_to'] ?? null;
        
        $result = $this->rechargeService->exportTransactions($status, $type, $dateFrom, $dateTo);
        
        header('Content-Type: text/csv; charset=utf-8');
        header('Content-Disposition: attachment; filename="recharge_transactions_' . date('Y-m-d_His') . '.csv"');
        
        echo "\xEF\xBB\xBF";
        echo $result;
        exit;
    }
    
    /**
     * Tạo giao dịch thủ công
     * POST /admin/recharge/manual
     */
    public function createManualRecharge() {
        $this->requireAdmin();
        
        $input = json_decode(file_get_contents('php://input'), true);
        
        if (!$input) {
            Response::error('Dữ liệu JSON không hợp lệ', 400);
        }
        
        $userId = $input['user_id'] ?? null;
        $amount = $input['amount'] ?? null;
        $type = $input['type'] ?? 'recharge_xu';
        $serverId = $input['server_id'] ?? 1;
        $note = $input['note'] ?? 'Nạp thủ công bởi admin';
        
        if (!$userId || !$amount) {
            Response::error('Thiếu thông tin user_id hoặc amount', 400);
        }
        
        $result = $this->rechargeService->createManualRecharge(
            $userId, $amount, $type, $serverId, $note
        );
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success($result['transaction'], 'Tạo giao dịch thành công', 201);
    }
    
    /**
     * Xóa giao dịch (chỉ xóa pending/failed)
     * DELETE /admin/recharge/transactions/:id
     */
    public function deleteTransaction($transactionId) {
        $this->requireAdmin();
        
        if (empty($transactionId) || !is_numeric($transactionId)) {
            Response::error('ID giao dịch không hợp lệ', 400);
        }
        
        $result = $this->rechargeService->deleteTransaction($transactionId);
        
        if (!$result['success']) {
            Response::error($result['message'], 400);
        }
        
        Response::success(null, 'Xóa giao dịch thành công');
    }
    
    /**
     * Lấy biểu đồ doanh thu
     * GET /admin/recharge/revenue-chart?period=7days
     */
    public function getRevenueChart() {
        $this->requireAdmin();
        
        $period = $_GET['period'] ?? '7days'; // 7days, 30days, 12months
        $serverId = $_GET['server_id'] ?? null;
        
        $result = $this->rechargeService->getRevenueChart($period, $serverId);
        
        Response::success($result, 'Lấy biểu đồ doanh thu thành công');
    }
    /**
 * Lấy danh sách recharge orders
 * GET /admin/recharge/orders?page=1&limit=20&status=all
 */
public function getOrders() {
    $this->requireAdmin();
    
    $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
    $limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 20;
    $status = $_GET['status'] ?? 'all';
    $type = $_GET['type'] ?? 'all';
    $search = $_GET['search'] ?? '';
    
    $result = $this->rechargeService->getOrders($page, $limit, $status, $type, $search);
    
    Response::success($result, 'Lấy danh sách orders thành công');
}

/**
 * Lấy chi tiết order
 * GET /admin/recharge/orders/:orderId
 */
public function getOrderDetail($orderId) {
    $this->requireAdmin();
    
    if (empty($orderId)) {
        Response::error('Order ID không hợp lệ', 400);
    }
    
    $result = $this->rechargeService->getOrderDetail($orderId);
    
    if (!$result) {
        Response::notFound('Không tìm thấy order');
    }
    
    Response::success($result, 'Lấy thông tin order thành công');
}

/**
 * Cập nhật status order
 * PUT /admin/recharge/orders/:orderId
 */
public function updateOrder($orderId) {
    $this->requireAdmin();
    
    if (empty($orderId)) {
        Response::error('Order ID không hợp lệ', 400);
    }
    
    $input = json_decode(file_get_contents('php://input'), true);
    
    if (!$input || !isset($input['status'])) {
        Response::error('Dữ liệu không hợp lệ', 400);
    }
    
    $result = $this->rechargeService->updateOrderStatus(
        $orderId,
        $input['status'],
        $input['note'] ?? ''
    );
    
    if (!$result['success']) {
        Response::error($result['message'], 400);
    }
    
    Response::success($result['order'], 'Cập nhật order thành công');
}

/**
 * Lấy cancellation logs
 * GET /admin/recharge/cancellation-logs?page=1&limit=50
 */
public function getCancellationLogs() {
    $this->requireAdmin();
    
    $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
    $limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 50;
    $userId = $_GET['user_id'] ?? null;
    
    $result = $this->rechargeService->getCancellationLogs($page, $limit, $userId);
    
    Response::success($result, 'Lấy cancellation logs thành công');
}

/**
 * Thống kê orders
 * GET /admin/recharge/orders/stats
 */
public function getOrderStats() {
    $this->requireAdmin();
    
    $period = $_GET['period'] ?? 'today';
    
    $result = $this->rechargeService->getOrderStats($period);
    
    Response::success($result, 'Lấy thống kê orders thành công');
}
    // Helper methods
    private function requireAdmin() {
        $token = $this->getBearerToken();
        
        if (!$token) {
            Response::unauthorized('Yêu cầu mã truy cập');
        }
        
        $userId = $this->authService->validateAccessToken($token);
        
        if (!$userId) {
            Response::unauthorized('Mã không hợp lệ hoặc đã hết hạn');
        }
        
        if (!AdminMiddleware::isAdmin($userId)) {
            Response::forbidden('Bạn không có quyền truy cập');
        }
        
        return $userId;
    }
    
    private function getBearerToken() {
        $headers = getallheaders();
        
        if (isset($headers['Authorization'])) {
            $matches = [];
            if (preg_match('/Bearer\s+(.*)$/i', $headers['Authorization'], $matches)) {
                return $matches[1];
            }
        }
        
        return null;
    }
}