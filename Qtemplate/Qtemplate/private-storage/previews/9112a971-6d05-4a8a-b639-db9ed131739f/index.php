<?php
// index.php

// Enable error reporting for development
error_reporting(E_ALL);
ini_set('display_errors', 1);

// Danh sách domain được phép
$allowedOrigins = [
    'https://kvteam.site',
    'http://admin.kvteam.site'
];

// Set CORS Origin động
if (isset($_SERVER['HTTP_ORIGIN']) && in_array($_SERVER['HTTP_ORIGIN'], $allowedOrigins)) {
    header("Access-Control-Allow-Origin: " . $_SERVER['HTTP_ORIGIN']);
    header("Access-Control-Allow-Credentials: true");
}

header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type, Authorization');
header('Content-Type: application/json; charset=utf-8');

// Handle preflight requests
if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit;
}

// Autoload classes
spl_autoload_register(function ($className) {
    $directories = [
        __DIR__ . '/config/',
        __DIR__ . '/core/',
        __DIR__ . '/helpers/',
        __DIR__ . '/controllers/',
        __DIR__ . '/controllers/admin/',  // ← Thêm dòng này
        __DIR__ . '/services/',
        __DIR__ . '/services/admin/',     // ← Thêm dòng này
        __DIR__ . '/middlewares/'         // ← Thêm dòng này
    ];
    
    foreach ($directories as $directory) {
        $file = $directory . $className . '.php';
        if (file_exists($file)) {
            require_once $file;
            return;
        }
    }
});

// Load configuration
require_once __DIR__ . '/config/Config.php';

// Error handling
set_exception_handler(function($e) {
    error_log($e->getMessage());
    Response::error('Internal server error: ' . $e->getMessage(), 500);
});

try {
    // Initialize router
    $router = new Router();
    
    // Admin routes
    $router->get('/admin/users', 'AdminAuthController', 'getUsers');
    $router->get('/admin/users/(\d+)', 'AdminAuthController', 'getUserDetail');
    $router->post('/admin/users', 'AdminAuthController', 'createUser');
    $router->put('/admin/users/(\d+)', 'AdminAuthController', 'updateUser');
    $router->delete('/admin/users/(\d+)', 'AdminAuthController', 'deleteUser');
    $router->post('/admin/users/(\d+)/ban', 'AdminAuthController', 'toggleBan');
    $router->post('/admin/users/(\d+)/reset-password', 'AdminAuthController', 'resetPassword');
    $router->post('/admin/users/(\d+)/kick-sessions', 'AdminAuthController', 'kickSessions');
    $router->get('/admin/users/(\d+)/login-history', 'AdminAuthController', 'getLoginHistory');
    $router->get('/admin/stats', 'AdminAuthController', 'getStats');
    $router->get('/admin/users/export', 'AdminAuthController', 'exportUsers');
    $router->get('/admin/logs', 'AdminAuthController', 'getLogs');
        // =======================
    // Admin Recharge routes
    // =======================
    // Danh sách giao dịch nạp
    $router->get('/admin/recharge/transactions', 'AdminRechargeController', 'getTransactions');
    // Chi tiết giao dịch
    $router->get('/admin/recharge/transactions/(\d+)', 'AdminRechargeController', 'getTransactionDetail');
    // Cập nhật giao dịch
    $router->put('/admin/recharge/transactions/(\d+)', 'AdminRechargeController', 'updateTransaction');
    // Xóa giao dịch
    $router->delete('/admin/recharge/transactions/(\d+)', 'AdminRechargeController', 'deleteTransaction');
    // Retry xử lý giao dịch lỗi
    $router->post('/admin/recharge/transactions/(\d+)/retry', 'AdminRechargeController', 'retryTransaction');
    // Top nạp
    $router->get('/admin/recharge/top', 'AdminRechargeController', 'getTopRecharge');
    // Thống kê nạp
    $router->get('/admin/recharge/stats', 'AdminRechargeController', 'getRechargeStats');
    // Lịch sử nạp theo user
    $router->get('/admin/recharge/user/(\d+)/history', 'AdminRechargeController', 'getUserRechargeHistory');
    // Log webhook
    $router->get('/admin/recharge/webhook-logs', 'AdminRechargeController', 'getWebhookLogs');
    // Export giao dịch nạp
    $router->get('/admin/recharge/export', 'AdminRechargeController', 'exportRecharge');
    // Nạp tay (admin)
    $router->post('/admin/recharge/manual', 'AdminRechargeController', 'manualRecharge');
    // Biểu đồ doanh thu
    $router->get('/admin/recharge/revenue-chart', 'AdminRechargeController', 'getRevenueChart');
     // Recharge Orders Management
    $router->get('/admin/recharge/orders', 'AdminRechargeController', 'getOrders');
    $router->get('/admin/recharge/orders/stats', 'AdminRechargeController', 'getOrderStats');
    $router->get('/admin/recharge/orders/(\w+)', 'AdminRechargeController', 'getOrderDetail');
    $router->put('/admin/recharge/orders/(\w+)', 'AdminRechargeController', 'updateOrder');
    // Webhook logs
    $router->get('/admin/recharge/webhook-logs', 'AdminRechargeController', 'getWebhookLogs');
    // Cancellation Logs
    $router->get('/admin/recharge/cancellation-logs', 'AdminRechargeController', 'getCancellationLogs'); 
    // =======================
    // Admin Items routes
    // =======================
    // Danh sách items
    $router->get('/admin/items', 'AdminItemController', 'getItems');
    // Thống kê items (phải đặt trước /admin/items/(\d+) để tránh conflict)
    $router->get('/admin/items/stats', 'AdminItemController', 'getItemStats');
    // Danh sách loại items
    $router->get('/admin/items/types', 'AdminItemController', 'getItemTypes');
    // Export items
    //$router->get('/admin/items/export', 'AdminItemController', 'exportItems');
    // Import items
    //$router->post('/admin/items/import', 'AdminItemController', 'importItems');
    // Cập nhật hàng loạt
    $router->post('/admin/items/bulk-update', 'AdminItemController', 'bulkUpdate');
    // Xóa hàng loạt
    $router->post('/admin/items/bulk-delete', 'AdminItemController', 'bulkDelete');
    // Chi tiết item
    $router->get('/admin/items/(\d+)', 'AdminItemController', 'getItemDetail');
    // Tạo item mới
    $router->post('/admin/items', 'AdminItemController', 'createItem');
    // Cập nhật item
    $router->put('/admin/items/(\d+)', 'AdminItemController', 'updateItem');
    // Xóa item
    $router->delete('/admin/items/(\d+)', 'AdminItemController', 'deleteItem');
    // Sao chép item
    $router->post('/admin/items/(\d+)/clone', 'AdminItemController', action: 'cloneItem');
    // admin Giftcode routes
    // Trong routes.php thêm:
   // routes.php - Sửa để khớp với tài liệu API

// Danh sách servers
  

    // Quản lý giftcode (CRUD)
    $router->get('/admin/giftcodes', 'AdminGiftcodeController', 'getGiftcodes');
    $router->get('/admin/giftcodes/(\d+)', 'AdminGiftcodeController', 'getGiftcodeDetail');
    $router->post('/admin/giftcodes', 'AdminGiftcodeController', 'createGiftcode');
    $router->put('/admin/giftcodes/(\d+)', 'AdminGiftcodeController', 'updateGiftcode');
    $router->delete('/admin/giftcodes/(\d+)', 'AdminGiftcodeController', 'deleteGiftcode');

    // Tính năng nâng cao
    $router->get('/admin/giftcodes/(\d+)/logs', 'AdminGiftcodeController', 'getGiftcodeUsageLog');
    $router->get('/admin/giftcodes/stats', 'AdminGiftcodeController', 'getGiftcodeStats');
    $router->post('/admin/giftcodes/generate', 'AdminGiftcodeController', 'generateCode');
    $router->post('/admin/giftcodes/batch-create', 'AdminGiftcodeController', 'batchCreateGiftcodes');
    $router->get('/admin/giftcodes/export', 'AdminGiftcodeController', 'exportGiftcodes'); // Sửa thành GET
    $router->post('/admin/giftcodes/bulk-delete', 'AdminGiftcodeController', 'bulkDelete'); // Thêm mới
    // =======================
    // Admin Topic routes
    // Danh sách topics với filter, search, sort
    $router->get('/admin/topics', 'AdminTopicController', 'getTopics');
    // Thống kê topics
    $router->get('/admin/topics/stats', 'AdminTopicController', 'getStats');
    // Export topics to CSV
    $router->get('/admin/topics/export', 'AdminTopicController', 'exportTopics');
    // Tạo topic mới
    $router->post('/admin/topics', 'AdminTopicController', 'createTopic');
    // Chi tiết topic
    $router->get('/admin/topics/(\d+)', 'AdminTopicController', 'getTopicDetail');
    // Cập nhật topic
    $router->put('/admin/topics/(\d+)', 'AdminTopicController', 'updateTopic');
    // Xóa topic
    $router->delete('/admin/topics/(\d+)', 'AdminTopicController', 'deleteTopic');
    // Block/Unblock topic
    $router->post('/admin/topics/(\d+)/block', 'AdminTopicController', 'toggleBlock');
    // Sticky/Unsticky topic
    $router->post('/admin/topics/(\d+)/sticky', 'AdminTopicController', 'toggleSticky');
    // Mark as done/undone
    $router->post('/admin/topics/(\d+)/done', 'AdminTopicController', 'toggleDone');
    // =======================
    // Danh sách servers
    $router->get('/admin/servers', 'AdminServerController', 'getServers');
    // Chi tiết server
    $router->get('/admin/servers/(\d+)', 'AdminServerController', 'getServerDetail');
    // Tạo server mới
    $router->post('/admin/servers', 'AdminServerController', 'createServer');
    // Cập nhật server
    $router->put('/admin/servers/(\d+)', 'AdminServerController', 'updateServer');
    // Xóa server
    $router->delete('/admin/servers/(\d+)', 'AdminServerController', 'deleteServer');
    // Toggle status server (bật/tắt)
    $router->post('/admin/servers/(\d+)/toggle-status', 'AdminServerController', 'toggleStatus');
    // Test kết nối database
    $router->post('/admin/servers/(\d+)/test-connection', 'AdminServerController', 'testConnection');
    // ADMIN ROUTES - Social Links Management
    // Danh sách social links
    $router->get('/admin/social-links', 'AdminSocialLinksController', 'getSocialLinks');
    // Chi tiết social link
    $router->get('/admin/social-links/(\d+)', 'AdminSocialLinksController', 'getSocialLinkDetail');
    // Tạo social link mới
    $router->post('/admin/social-links', 'AdminSocialLinksController', 'createSocialLink');
    // Cập nhật social link
    $router->put('/admin/social-links/(\d+)', 'AdminSocialLinksController', 'updateSocialLink');
    // Xóa social link
    $router->delete('/admin/social-links/(\d+)', 'AdminSocialLinksController', 'deleteSocialLink');
    // lấy config hiện tại
    // Admin Configs
    $router->get('/admin/configs', 'AdminConfigController', 'getAllConfigs');
    $router->get('/admin/configs/categories', 'AdminConfigController', 'getCategories');
    $router->post('/admin/configs/reset', 'AdminConfigController', 'resetToDefault');
    $router->get('/admin/configs/([^/]+)', 'AdminConfigController', 'getConfigByKey');
    $router->put('/admin/configs/([^/]+)', 'AdminConfigController', 'updateConfig');
    // Admin Top Rewards
// Lấy danh sách tất cả events
$router->get('/admin/events', 'AdminTopRewardController', 'getEvents');

// Cập nhật thông tin event
$router->put('/admin/events/(\d+)', 'AdminTopRewardController', 'updateEvent');

// Lấy danh sách phần thưởng của event (grouped by top_type)
$router->get('/admin/events/(\d+)/rewards', 'AdminTopRewardController', 'getEventRewards');

// ============================================
// REWARDS MANAGEMENT
// ============================================

// Lấy chi tiết một phần thưởng
$router->get('/admin/rewards/(\d+)', 'AdminTopRewardController', 'getRewardDetail');

// Tạo phần thưởng mới
$router->post('/admin/rewards', 'AdminTopRewardController', 'createReward');

// Cập nhật phần thưởng
$router->put('/admin/rewards/(\d+)', 'AdminTopRewardController', 'updateReward');

// Xóa phần thưởng
$router->delete('/admin/rewards/(\d+)', 'AdminTopRewardController', 'deleteReward');

// Sao chép phần thưởng giữa events
$router->post('/admin/rewards/copy-by-event', 'AdminTopRewardController', 'copyRewardsByEvent');

// Cập nhật hàng loạt phần thưởng
$router->put('/admin/rewards/bulk-update', 'AdminTopRewardController', 'bulkUpdateRewards');

// Lấy thống kê phần thưởng
$router->get('/admin/rewards/statistics', 'AdminTopRewardController', 'getRewardsStatistics');

// ============================================
// EVENT FINALIZE (CHỐT KẾT QUẢ)
// ============================================

// Xem trước kết quả trước khi chốt
// Hỗ trợ: recharge, level, boss, event
$router->get('/admin/events/(\d+)/preview-finalize', 'AdminTopRewardController', 'previewFinalizeEvent');

// Chốt kết quả event
// Lưu snapshot vào event_results và đánh dấu event.is_finished = 1
$router->post('/admin/events/(\d+)/finalize', 'AdminTopRewardController', 'finalizeEvent');

// Lấy kết quả đã chốt
$router->get('/admin/events/(\d+)/results', 'AdminTopRewardController', 'getEventResults');

// ============================================
// EVENT RESULTS - CLAIM MANAGEMENT
// ============================================

// Đánh dấu đã nhận thưởng (single)
$router->put('/admin/event-results/(\d+)/claim', 'AdminTopRewardController', 'markAsClaimed');

// Hủy đánh dấu đã nhận thưởng (single)
$router->put('/admin/event-results/(\d+)/unclaim', 'AdminTopRewardController', 'markAsUnclaimed');

// Đánh dấu hàng loạt đã nhận thưởng (bulk)
$router->put('/admin/event-results/bulk-claim', 'AdminTopRewardController', 'bulkMarkAsClaimed');


    // Milestone admin routes
    $router->get('/admin/milestones', 'AdminMilestoneController', 'getMilestones');
    $router->get('/admin/milestones/stats', 'AdminMilestoneController', 'getMilestoneStats');
    $router->get('/admin/milestones/export', 'AdminMilestoneController', 'exportMilestones');
    $router->get('/admin/milestones/{id}', 'AdminMilestoneController', 'getMilestoneDetail');
    $router->get('/admin/milestones/{id}/logs', 'AdminMilestoneController', 'getMilestoneClaimLog');
    $router->get('/admin/milestones/{id}/users', 'AdminMilestoneController', 'getMilestoneUsers');

    // POST routes
    $router->post('/admin/milestones', 'AdminMilestoneController', 'createMilestone');
    $router->post('/admin/milestones/{id}/toggle-active', 'AdminMilestoneController', 'toggleActive');
    $router->post('/admin/milestones/bulk-delete', 'AdminMilestoneController', 'bulkDelete');

    // PUT routes
    $router->put('/admin/milestones/{id}', 'AdminMilestoneController', 'updateMilestone');

    // DELETE routes
    $router->delete('/admin/milestones/{id}', 'AdminMilestoneController', 'deleteMilestone');

    /*-----------------------------------------------User-Facing Routes-----------------------------------------------*/
    // character routes
    $router->get('/characters', 'CharacterController', 'getCharacters');

    // Lấy thông tin chi tiết 1 nhân vật
    $router->get('/characters/detail', 'CharacterController', 'getCharacterDetail');

    // Lấy thống kê nhân vật của user
    $router->get('/characters/stats', 'CharacterController', 'getCharacterStats');

    // config routes

    // GET /config/rates - Lấy toàn bộ tỷ giá
    $router->get('/config/rates', 'PublicConfigController', 'getRates');
    // GET /config/xu - Lấy cấu hình xu
    $router->get('/config/xu', 'PublicConfigController', 'getXuConfig');
    // GET /config/luong - Lấy cấu hình lượng
    $router->get('/config/luong', 'PublicConfigController', 'getLuongConfig');
    // GET /config/activation - Lấy cấu hình kích hoạt
    $router->get('/config/activation', 'PublicConfigController', 'getActivationConfig');
    // POST /config/calculate-xu - Tính toán xu
    $router->post('/config/calculate-xu', 'PublicConfigController', 'calculateXu');
    // POST /config/calculate-luong - Tính toán lượng
    $router->post('/config/calculate-luong', 'PublicConfigController', 'calculateLuong');

    // Auth routes
    $router->post('/auth/register', 'AuthController', 'register');
    $router->post('/auth/login', 'AuthController', 'login');
    $router->post('/auth/logout', 'AuthController', 'logout');
    $router->post('/auth/refresh', 'AuthController', 'refresh');
    $router->get('/auth/me', 'AuthController', 'me');
    $router->post('/auth/logout-all', 'AuthController', 'logoutAll');
    $router->post('/auth/change-password', 'AuthController', 'changePassword');
    // server routes
    $router->get('/social-links', 'SocialLinksController', 'getSocialLinks');
    // Lấy social link mới nhất
    $router->get('/social-links/latest', 'SocialLinksController', 'getLatestSocialLink');
    $router->get('/servers', 'ServerController', 'getActiveServers');
    //$router->get('/servers/(\d+)', 'ServerController', 'getServerById');
    // Leaderboard routes
    $router->get('/leaderboard/level', 'LeaderboardController', 'level');
    $router->get('/leaderboard/event', 'LeaderboardController', 'event');
    $router->get('/leaderboard/recharge', 'LeaderboardController', 'recharge');
    $router->get('/leaderboard/boss', 'LeaderboardController', 'boss');
    $router->get('/leaderboard/event-recharge/history', 'LeaderboardController', 'eventRechargeHistory');
    $router->get('/leaderboard/events/finished', 'LeaderboardController', 'finishedEvents');
    // Activation routes
    $router->get('/activation/check-pending', 'ActivationController', 'checkPending');
    $router->get('/activation/status', 'ActivationController', 'status');
    $router->post('/activation/request', 'ActivationController', 'request');
    $router->get('/activation/qr', 'ActivationController', 'qr');
    $router->get('/activation/verify', 'ActivationController', 'verify');
    $router->get('/activation/history', 'ActivationController', 'history');    
    // Webhook routes
    $router->post('/webhook/sepay', 'WebhookController', 'sepay');
    //$router->post('/webhook/sepay/xu', 'WebhookController', 'sepayXu');      // Webhook nạp xu
    //$router->post('/webhook/sepay/luong', 'WebhookController', 'sepayLuong');

    // Recharge routes
    $router->post('/recharge/xu/create', 'RechargeController', 'createXuOrder');
    $router->post('/recharge/luong/create', 'RechargeController', 'createLuongOrder');
    $router->post('/recharge/create-with-cancel', 'RechargeController', 'createWithCancel');
    
    // Cancel recharge orders
    $router->post('/recharge/cancel', 'RechargeController', 'cancelOrder');
    $router->post('/recharge/cancel-all', 'RechargeController', 'cancelAllOrders');
    // Lấy QR code cho lệnh nạp
    $router->get('/recharge/qr', 'RechargeController', 'getQR');
    // Kiểm tra trạng thái lệnh nạp
    $router->get('/recharge/verify', 'RechargeController', 'verify');
    // Lịch sử nạp của user
    $router->get('/recharge/history', 'RechargeController', 'history');
    // Lấy tỷ giá quy đổi
    $router->get('/recharge/rates', 'RechargeController', 'getRates');
    // Transaction Status & History APIs
    $router->get('/transactions/status', 'TransactionController', 'getStatus');
    $router->get('/transactions/history', 'TransactionController', 'getHistory');
    $router->get('/transactions/pending', 'TransactionController', 'getPending');
    $router->get('/transactions/statistics', 'TransactionController', 'getStatistics');
    // Topic routes (thêm vào phần user-facing routes)
    $router->get('/topics', 'TopicController', 'getTopics');
    $router->get('/topics/(\d+)', 'TopicController', 'getTopicDetail');
    // Giftcode routes
    $router->get('/giftcodes', 'GiftcodeController', 'getPublicGiftcodes');
    $router->post('/giftcodes/use', 'GiftcodeController', 'useGiftcode');
    $router->get('/giftcodes/check', 'GiftcodeController', 'checkGiftcode');
    // Milestone routes
    $router->get('/milestones', 'MilestoneController', 'getAllMilestones');
    $router->get('/milestones/progress', 'MilestoneController', 'getUserProgress');
    $router->get('/milestones/claimed', 'MilestoneController', 'getClaimedMilestones');
    $router->get('/milestones/available', 'MilestoneController', 'getAvailableMilestones');
    $router->get('/milestones/overview', 'MilestoneController', 'getUserOverview');
    $router->get('/milestones/check', 'MilestoneController', 'checkEligibility');

    // POST routes
    $router->post('/milestones/claim', 'MilestoneController', 'claimReward');
    // Get request info
    $requestMethod = $_SERVER['REQUEST_METHOD'];
    $requestUri = $_SERVER['REQUEST_URI'];
    
    // Dispatch request
    $router->dispatch($requestMethod, $requestUri);
    
} catch (Exception $e) {
    Response::error('Server error: ' . $e->getMessage(), 500);
}