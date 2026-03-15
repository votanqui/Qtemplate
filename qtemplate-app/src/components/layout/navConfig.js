// ─────────────────────────────────────────────────────────
// navConfig.js — Single source of truth for navigation
// ─────────────────────────────────────────────────────────

// Desktop sidebar — ALL items, grouped by section
// Section keys: 'main' | 'account' | 'library'
export const SIDEBAR_SECTIONS = [
  {
    key: 'main',
    label: 'Khám phá',
    items: [
      { path: '/',          icon: '🏠', label: 'Trang chủ' },
      { path: '/templates', icon: '🗂️', label: 'Templates' },
      { path: '/sale',      icon: '🔥', label: 'Săn Sale'  },
    ],
  },
  {
    key: 'library',
    label: 'Thư viện',
    items: [
      { path: '/dashboard/downloads',     icon: '⬇️', label: 'Downloads' },
      { path: '/dashboard/wishlist',      icon: '❤️', label: 'Yêu thích' },
      { path: '/dashboard/notifications', icon: '🔔', label: 'Thông báo' },
      { path: '/dashboard/coupons',   icon: '🎟️', label: 'Mã giảm giá' },
    ],
  },
  {
    key: 'account',
    label: 'Tài khoản',
    items: [
      { path: '/dashboard/profile',    icon: '👤', label: 'Hồ sơ' },
      { path: '/dashboard/purchases',  icon: '🛍️', label: 'Đơn mua' },
      { path: '/dashboard/reviews',    icon: '⭐', label: 'Reviews' },
      { path: '/dashboard/tickets',    icon: '🎫', label: 'Hỗ trợ' },
      { path: '/dashboard/affiliate',  icon: '🤝', label: 'Affiliate' },
      { path: '/dashboard/security',   icon: '🔒', label: 'Bảo mật' },
    ],
  },
];

// Flat list for MobileSidebar (same items, no sections needed)
export const SIDEBAR_ALL = SIDEBAR_SECTIONS.flatMap(s => s.items);

// Mobile bottom nav — 5 most-used shortcuts, picked to NOT overlap with each other
// (overlap with sidebar is fine since sidebar is desktop-only)
export const BOTTOM_NAV_AUTH = [
  { path: '/templates',                icon: '🗂️', label: 'Khám phá' },
  { path: '/dashboard/profile',        icon: '👤', label: 'Hồ sơ' },
  { path: '/dashboard/downloads',      icon: '⬇️', label: 'Downloads' },
  { path: '/dashboard/wishlist',       icon: '❤️', label: 'Yêu thích' },
  { path: '/dashboard/notifications',  icon: '🔔', label: 'Tin' },
  { path: '/dashboard/security',       icon: '🔒', label: 'Bảo mật' },
  // Logout button added separately in BottomNav
];

// Guest bottom nav
export const BOTTOM_NAV_GUEST = [
  { path: '/templates', icon: '🗂️', label: 'Khám phá' },
  { path: '/login',     icon: '🔐', label: 'Đăng nhập' },
  { path: '/register',  icon: '✨', label: 'Đăng ký' },
];