import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { Layout } from './components/layout/Layout';
import { ProtectedRoute } from './components/auth/ProtectedRoute';
import ProtectedAdminRoute from './components/auth/ProtectedAdminRoute';
import AdminLayout from './components/admin/AdminLayout';
import { NotificationProvider } from './context/NotificationContext';
// Pages
import LoginPage from './pages/public/LoginPage';
import RegisterPage from './pages/public/RegisterPage';
import { ForgotPasswordPage, ResetPasswordPage, VerifyEmailPage } from './pages/public/AuthPages';
import TemplatesPage from './pages/public/TemplatesPage';
import TemplateDetailPage from './pages/public/TemplateDetailPage';
import LandingPage from './pages/public/LandingPage';
import SalePage from './pages/public/SalePage';
import ProfilePage from './pages/public/ProfilePage';
import SecurityPage from './pages/user/SecurityPage';
import PurchasesPage from './pages/user/PurchasePages';
import DownloadsPage from './pages/user/Downloadspage';
import WishlistPage from './pages/user/Wishlistpage';
import NotificationsPage from './pages/user/NotificationsPage';
import ReviewsPage from './pages/user/ReviewsPage';
import { TicketsPage, TicketDetailPage } from './pages/user/TicketPages';
import AffiliatePage from './pages/user/AffiliatePage';
import OrderDetailPage from './pages/user/OrderDetailPage';
import CouponsPage from './pages/public/CouponsPage';
import NewsPage from './pages/public/NewsPage';
import NewsDetailPage from './pages/public/NewsDetailPage';
import CommunityPage from './pages/public/CommunityPage';
// Admin pages
import AdminUsersPage from './pages/admin/AdminUsersPage';
import AdminCouponsPage from './pages/admin/AdminCouponsPage';
import AdminReviewsPage from './pages/admin/AdminReviewsPage';
import AdminOrdersPage from './pages/admin/AdminOrdersPage';
import AdminTicketsPage from './pages/admin/AdminTicketsPage';
import AdminBannersPage from './pages/admin/AdminBannersPage';
import AdminAffiliatesPage from './pages/admin/AdminAffiliatesPage';
import AdminTemplatesPage   from './pages/admin/AdminTemplatesPage';
import AdminStatsPage       from './pages/admin/AdminStatsPage';
import AdminCategoriesPage  from './pages/admin/AdminCategoriesPage';
import AdminTagsPage        from './pages/admin/AdminTagsPage';
import AdminIpBlacklistPage from './pages/admin/AdminIpBlacklistPage';
import AdminLogsPage        from './pages/admin/AdminLogsPage';
import AdminNotificationsPage from './pages/admin/AdminNotificationsPage';
import AdminSettingsPage from './pages/admin/AdminSettingsPage';
import AdminWishlistPage from './pages/admin/AdminWishlistPage';
import AdminMediaPage from './pages/admin/AdminMediaPage';
import AdminPostsPage from './pages/admin/AdminPostsPage';
import AdminCommunityPage from './pages/admin/AdminCommunityPage';
function AdminPlaceholder({ title }) {
  return (
    <div style={{ textAlign: 'center', paddingTop: 80, color: '#94a3b8' }}>
      <div style={{ fontSize: 40, marginBottom: 12 }}>🚧</div>
      <div style={{ fontSize: 16, fontWeight: 700, color: '#0f172a', marginBottom: 6 }}>{title}</div>
      <div style={{ fontSize: 13 }}>Trang này đang được phát triển.</div>
    </div>
  );
}

function AdminRoute({ children }) {
  return (
    <ProtectedAdminRoute>
      <AdminLayout>{children}</AdminLayout>
    </ProtectedAdminRoute>
  );
}

function NotFoundPage() {
  return (
    <div className="text-center py-20">
      <p className="text-6xl mb-4">🔍</p>
      <h1 className="text-2xl font-bold mb-2" style={{ color: 'var(--text-primary)' }}>404 – Không tìm thấy</h1>
      <a href="/" style={{ color: '#7c3aed' }} className="hover:underline">← Về trang chủ</a>
    </div>
  );
}

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <NotificationProvider>
          <Routes>
            {/* Auth pages */}
            <Route path="/login"            element={<LoginPage />} />
            <Route path="/register"         element={<RegisterPage />} />
            <Route path="/forgot-password"  element={<ForgotPasswordPage />} />
            <Route path="/reset-password"   element={<ResetPasswordPage />} />
            <Route path="/verify-email"     element={<VerifyEmailPage />} />

            {/* Public pages */}
            <Route path="/"                 element={<Layout><LandingPage /></Layout>} />
            <Route path="/templates"        element={<Layout><TemplatesPage /></Layout>} />
            <Route path="/templates/:slug"  element={<Layout><TemplateDetailPage /></Layout>} />
            <Route path="/sale"             element={<Layout><SalePage /></Layout>} />
            <Route path="/tin-tuc"          element={<Layout><NewsPage /></Layout>} />
            <Route path="/tin-tuc/:slug"    element={<Layout><NewsDetailPage /></Layout>} />
              <Route path="/cong-dong"       element={<Layout><CommunityPage /></Layout>} />
            {/* Protected dashboard */}
            <Route path="/dashboard/profile"       element={<Layout><ProtectedRoute><ProfilePage /></ProtectedRoute></Layout>} />
            <Route path="/dashboard/security"      element={<Layout><ProtectedRoute><SecurityPage /></ProtectedRoute></Layout>} />
            <Route path="/dashboard/purchases"     element={<Layout><ProtectedRoute><PurchasesPage /></ProtectedRoute></Layout>} />
            <Route path="/dashboard/orders/:id"    element={<Layout><ProtectedRoute><OrderDetailPage /></ProtectedRoute></Layout>} />
            <Route path="/dashboard/downloads"     element={<Layout><ProtectedRoute><DownloadsPage /></ProtectedRoute></Layout>} />
            <Route path="/dashboard/wishlist"      element={<Layout><ProtectedRoute><WishlistPage /></ProtectedRoute></Layout>} />
            <Route path="/dashboard/notifications" element={<Layout><ProtectedRoute><NotificationsPage /></ProtectedRoute></Layout>} />
            <Route path="/dashboard/reviews"       element={<Layout><ProtectedRoute><ReviewsPage /></ProtectedRoute></Layout>} />
            <Route path="/dashboard/tickets"       element={<Layout><ProtectedRoute><TicketsPage /></ProtectedRoute></Layout>} />
            <Route path="/dashboard/tickets/:id"   element={<Layout><ProtectedRoute><TicketDetailPage /></ProtectedRoute></Layout>} />
            <Route path="/dashboard/affiliate"     element={<Layout><ProtectedRoute><AffiliatePage /></ProtectedRoute></Layout>} />
            <Route path="/dashboard/coupons"       element={<Layout><ProtectedRoute><CouponsPage /></ProtectedRoute></Layout>} />

            {/* ── Admin ── */}
            <Route path="/admin"             element={<AdminRoute><AdminStatsPage /></AdminRoute>} />
            <Route path="/admin/users"       element={<AdminRoute><AdminUsersPage /></AdminRoute>} />
            <Route path="/admin/templates"   element={<AdminRoute><AdminTemplatesPage /></AdminRoute>} />
            <Route path="/admin/orders"      element={<AdminRoute><AdminOrdersPage/></AdminRoute>} />
            <Route path="/admin/reviews"     element={<AdminRoute><AdminReviewsPage  /></AdminRoute>} />
            <Route path="/admin/tickets"     element={<AdminRoute><AdminTicketsPage /></AdminRoute>} />
            <Route path="/admin/coupons"     element={<AdminRoute><AdminCouponsPage /></AdminRoute>} />
            <Route path="/admin/banners"     element={<AdminRoute><AdminBannersPage /></AdminRoute>} />
            <Route path="/admin/categories"  element={<AdminRoute><AdminCategoriesPage /></AdminRoute>} />
            <Route path="/admin/tags"        element={<AdminRoute><AdminTagsPage /></AdminRoute>} />
            <Route path="/admin/affiliates"  element={<AdminRoute><AdminAffiliatesPage /></AdminRoute>} />
            <Route path="/admin/media"       element={<AdminRoute><AdminMediaPage /></AdminRoute>} />
            <Route path="/admin/settings"    element={<AdminRoute><AdminSettingsPage /></AdminRoute>} />
            <Route path="/admin/logs"        element={<AdminRoute><AdminLogsPage /></AdminRoute>} />
            <Route path="/admin/security"    element={<AdminRoute><AdminPlaceholder title="Bảo mật" /></AdminRoute>} />
            <Route path="/admin/ip-blacklist" element={<AdminRoute><AdminIpBlacklistPage /></AdminRoute>} />
            <Route path="/admin/notifications" element={<AdminRoute><AdminNotificationsPage /></AdminRoute>} />
            <Route path="/admin/wishlist" element={<AdminRoute><AdminWishlistPage /></AdminRoute>} />
            <Route path="/admin/posts" element={<AdminRoute><AdminPostsPage /></AdminRoute>} />
            <Route path="/admin/community" element={<AdminRoute><AdminCommunityPage /></AdminRoute>} />
            {/* 404 */}
            <Route path="*" element={<Layout><NotFoundPage /></Layout>} />
          </Routes>
        </NotificationProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;