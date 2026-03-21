import { useState, useEffect, useRef } from 'react';
import { NavLink, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useTheme } from '../../context/ThemeContext';

/* ─── Nav config ─────────────────────────────────────────────── */
const NAV_SECTIONS = [
  {
    label: 'Tổng quan',
    items: [{ path: '/admin', icon: '📊', label: 'Dashboard', end: true }],
  },
  {
    label: 'Nội dung',
    items: [
      { path: '/admin/templates',  icon: '🗂️',  label: 'Templates' },
      { path: '/admin/categories', icon: '📂',  label: 'Danh mục'  },
      { path: '/admin/tags',       icon: '🏷️',  label: 'Tags'      },
      { path: '/admin/banners',    icon: '🖼️',  label: 'Banners'   },
      { path: '/admin/media',      icon: '📁',  label: 'Media'     },
           { path: '/admin/posts',      icon: '📰',  label: 'Bảng tin'   },
     { path: '/admin/community',  icon: '👥',  label: 'Cộng đồng'  },
    ],
  },
  {
    label: 'Kinh doanh',
    items: [
      { path: '/admin/users',      icon: '👥',  label: 'Người dùng' },
      { path: '/admin/orders',     icon: '🛍️', label: 'Đơn hàng'   },
      { path: '/admin/coupons',    icon: '🎟️', label: 'Coupons'    },
      { path: '/admin/affiliates', icon: '🤝',  label: 'Affiliate'  },
      { path: '/admin/wishlist',   icon: '❤️',  label: 'Yêu thích'  },
    ],
  },
  {
    label: 'Hỗ trợ',
    items: [
      { path: '/admin/reviews',       icon: '⭐', label: 'Reviews'   },
      { path: '/admin/tickets',       icon: '🎫', label: 'Tickets'   },
      { path: '/admin/notifications', icon: '📢', label: 'Thông báo' },
    ],
  },
  {
    label: 'Hệ thống',
    items: [
      { path: '/admin/settings',     icon: '⚙️',  label: 'Cài đặt'      },
      { path: '/admin/security',     icon: '🔐',  label: 'Bảo mật'      },
      { path: '/admin/logs',         icon: '📋',  label: 'Logs'         },
      { path: '/admin/ip-blacklist', icon: '🚫',  label: 'IP Blacklist' },
    ],
  },
];

const allItems = NAV_SECTIONS.flatMap(s => s.items);

/* ─── Hooks ──────────────────────────────────────────────────── */
function useClock() {
  const [t, setT] = useState(new Date());
  useEffect(() => { const id = setInterval(() => setT(new Date()), 1000); return () => clearInterval(id); }, []);
  return t;
}

/* ─── Tooltip ────────────────────────────────────────────────── */
let _css = false;
function injectTipCss() {
  if (_css || typeof document === 'undefined') return; _css = true;
  const s = document.createElement('style');
  s.textContent = `
    .adm-tip-wrap { position: relative; }
    .adm-tip-wrap .adm-tip {
      opacity: 0; pointer-events: none;
      position: absolute; left: calc(100% + 12px); top: 50%;
      transform: translateY(-50%) translateX(-6px);
      transition: opacity .15s ease, transform .15s ease;
      white-space: nowrap; z-index: 999;
    }
    .adm-tip-wrap:hover .adm-tip {
      opacity: 1;
      transform: translateY(-50%) translateX(0);
    }
    /* Sidebar scrollbar */
    .adm-nav::-webkit-scrollbar { width: 3px; }
    .adm-nav::-webkit-scrollbar-track { background: transparent; }
    .adm-nav::-webkit-scrollbar-thumb { background: var(--border); border-radius: 99px; }
    /* Page content fade-in on route change */
    .adm-page { animation: admFade .2s ease; }
    @keyframes admFade { from { opacity:.4; transform:translateY(5px); } to { opacity:1; transform:none; } }
    /* Active nav indicator pip */
    .desktop-nav-item.active .adm-pip { opacity: 1; }
    .adm-pip { opacity: 0; transition: opacity .15s; }
  `;
  document.head.appendChild(s);
}

function Tip({ label }) {
  injectTipCss();
  return (
    <span className="adm-tip px-2.5 py-1.5 rounded-lg text-xs font-semibold"
      style={{ background: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-primary)', boxShadow: '0 4px 16px rgba(0,0,0,.15)' }}>
      {label}
    </span>
  );
}

/* ─── Theme toggle pill ──────────────────────────────────────── */
function ThemePill({ collapsed }) {
  injectTipCss();
  const { isDark, toggleTheme } = useTheme();
  return (
    <button
      onClick={toggleTheme}
      className={`adm-tip-wrap desktop-nav-item w-full ${collapsed ? 'justify-center px-2' : ''}`}
    >
      {/* Track */}
      <div style={{
        position: 'relative', width: 34, height: 19, borderRadius: 99, flexShrink: 0,
        background: isDark ? '#0ea5e9' : '#e2e8f0',
        transition: 'background .3s',
        boxShadow: 'inset 0 1px 3px rgba(0,0,0,.15)',
      }}>
        <div style={{
          position: 'absolute', top: 2.5, left: isDark ? 17 : 2.5,
          width: 14, height: 14, borderRadius: '50%', background: '#fff',
          transition: 'left .25s', display: 'flex', alignItems: 'center', justifyContent: 'center',
          fontSize: 8, boxShadow: '0 1px 4px rgba(0,0,0,.2)',
        }}>
          {isDark ? '🌙' : '☀️'}
        </div>
      </div>
      {!collapsed && <span className="text-sm truncate">{isDark ? 'Dark mode' : 'Light mode'}</span>}
      {collapsed && <Tip label={isDark ? 'Light mode' : 'Dark mode'} />}
    </button>
  );
}

/* ─── Main layout ────────────────────────────────────────────── */
export default function AdminLayout({ children }) {
  injectTipCss();
  const { user, logout } = useAuth();
  const navigate  = useNavigate();
  const location  = useLocation();
  const clock     = useClock();
  const [collapsed, setCollapsed] = useState(false);
  const pageKey = location.pathname;

  const currentItem = allItems.find(i =>
    i.end ? location.pathname === i.path : location.pathname.startsWith(i.path)
  );

  return (
    <div className="flex min-h-screen" style={{ background: 'var(--bg-page)', color: 'var(--text-primary)' }}>

      {/* ═══════════════════════════════ SIDEBAR ═══════════════════════════════ */}
      <aside
        className="hidden lg:flex flex-col sticky top-0 h-screen shrink-0 z-40"
        style={{
          width: collapsed ? 64 : 228,
          minWidth: collapsed ? 64 : 228,
          background: 'var(--sidebar-bg)',
          borderRight: '1px solid var(--sidebar-border)',
          transition: 'width .25s cubic-bezier(.4,0,.2,1), min-width .25s cubic-bezier(.4,0,.2,1)',
          overflow: collapsed ? 'visible' : 'hidden',
        }}
      >
        {/* ── Logo row ── */}
        <div
          className="flex items-center shrink-0"
          style={{
            height: 56,
            padding: collapsed ? '0 14px' : '0 16px',
            justifyContent: collapsed ? 'center' : 'space-between',
            borderBottom: '1px solid var(--sidebar-border)',
          }}
        >
          {!collapsed && (
            <div className="flex items-center gap-2.5 min-w-0">
              <div style={{
                width: 30, height: 30, borderRadius: 10, flexShrink: 0,
                background: '#0f172a',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                color: '#fff', fontFamily: 'monospace', fontSize: 14, fontWeight: 900,
                boxShadow: '0 2px 8px rgba(0,0,0,.15)',
              }}>Q</div>
              <div className="min-w-0">
                <div className="font-black tracking-tight text-sm truncate" style={{ color: 'var(--sidebar-text-strong)', fontFamily: '"Syne", sans-serif' }}>
                  Qtemplate
                </div>
                <div className="text-[9px] font-bold tracking-[.15em] uppercase" style={{ color: 'var(--text-muted)' }}>
                  Admin Panel
                </div>
              </div>
            </div>
          )}

          {collapsed && (
            <div style={{
              width: 30, height: 30, borderRadius: 10,
              background: '#0f172a',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              color: '#fff', fontFamily: 'monospace', fontSize: 14, fontWeight: 900,
            }}>Q</div>
          )}

          {/* Toggle button */}
          <button
            onClick={() => setCollapsed(v => !v)}
            className="w-7 h-7 rounded-lg flex items-center justify-center shrink-0 transition-colors"
            style={{
              background: 'var(--bg-elevated)', border: '1px solid var(--border)',
              color: 'var(--text-muted)',
              marginLeft: collapsed ? 0 : 'auto',
              display: collapsed ? 'none' : 'flex',
            }}
            onMouseEnter={e => e.currentTarget.style.background = 'var(--sidebar-hover)'}
            onMouseLeave={e => e.currentTarget.style.background = 'var(--bg-elevated)'}
          >
            <svg width="10" height="10" viewBox="0 0 10 10" fill="none">
              <path d="M7 1.5L3 5l4 3.5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"/>
            </svg>
          </button>

          {/* Collapsed: floating expand btn */}
          {collapsed && (
            <button
              onClick={() => setCollapsed(false)}
              className="absolute z-50 w-5 h-5 rounded-full flex items-center justify-center transition-colors"
              style={{
                right: -10, top: 18,
                background: 'var(--bg-elevated)',
                border: '1px solid var(--border)',
                color: 'var(--text-muted)',
                boxShadow: '0 2px 8px rgba(0,0,0,.12)',
              }}
              onMouseEnter={e => e.currentTarget.style.background = 'var(--bg-hover)'}
              onMouseLeave={e => e.currentTarget.style.background = 'var(--bg-elevated)'}
            >
              <svg width="8" height="8" viewBox="0 0 10 10" fill="none" style={{ transform: 'rotate(180deg)' }}>
                <path d="M7 1.5L3 5l4 3.5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"/>
              </svg>
            </button>
          )}
        </div>

        {/* ── User card ── */}
        {!collapsed ? (
          <div style={{ padding: '10px 12px 6px' }} className="shrink-0">
            <div className="flex items-center gap-2.5 rounded-xl px-3 py-2.5"
              style={{ background: 'var(--bg-elevated)', border: '1px solid var(--border)' }}>
              <div style={{
                width: 28, height: 28, borderRadius: 8, flexShrink: 0,
                background: 'linear-gradient(135deg, #6366f1, #ec4899)',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                color: '#fff', fontSize: 11, fontWeight: 700,
              }}>
                {user?.fullName?.[0]?.toUpperCase() || 'A'}
              </div>
              <div className="flex-1 min-w-0">
                <div className="text-xs font-bold truncate" style={{ color: 'var(--sidebar-text-strong)', lineHeight: 1.2 }}>
                  {user?.fullName || 'Admin'}
                </div>
                <div className="text-[10px] flex items-center gap-1 mt-0.5" style={{ color: 'var(--text-muted)' }}>
                  <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 inline-block animate-pulse" />
                  Administrator
                </div>
              </div>
            </div>
          </div>
        ) : (
          <div className="flex justify-center py-3 shrink-0 adm-tip-wrap">
            <div className="relative">
              <div style={{
                width: 30, height: 30, borderRadius: 9,
                background: 'linear-gradient(135deg, #6366f1, #ec4899)',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                color: '#fff', fontSize: 11, fontWeight: 700,
              }}>
                {user?.fullName?.[0]?.toUpperCase() || 'A'}
              </div>
              <span className="absolute -bottom-0.5 -right-0.5 w-2 h-2 rounded-full bg-emerald-400 border-2 animate-pulse"
                style={{ borderColor: 'var(--sidebar-bg)' }} />
            </div>
            <Tip label={user?.fullName || 'Admin'} />
          </div>
        )}

        {/* ── Divider ── */}
        <div className="mx-3 mb-1 shrink-0" style={{ height: 1, background: 'var(--sidebar-border)' }} />

        {/* ── Nav ── */}
        <nav className="adm-nav flex-1 min-h-0 overflow-y-auto overflow-x-visible px-2 py-1">
          {NAV_SECTIONS.map(section => (
            <div key={section.label} className="mb-1">
              {!collapsed ? (
                <div className="px-3 py-1.5 text-[10px] font-bold uppercase tracking-[.12em]" style={{ color: 'var(--text-muted)' }}>
                  {section.label}
                </div>
              ) : (
                <div className="h-px mx-1 my-2" style={{ background: 'var(--sidebar-border)' }} />
              )}

              <div className="space-y-0.5">
                {section.items.map(item => (
                  <NavLink
                    key={item.path}
                    to={item.path}
                    end={item.end}
                    className={({ isActive }) => `desktop-nav-item adm-tip-wrap ${isActive ? 'active' : ''} ${collapsed ? 'justify-center px-2' : ''}`}
                    style={{ textDecoration: 'none' }}
                  >
                    {({ isActive }) => (
                      <>
                        <span style={{ fontSize: 14, flexShrink: 0, lineHeight: 1 }}>{item.icon}</span>
                        {!collapsed && (
                          <>
                            <span className="flex-1 truncate text-[13px]">{item.label}</span>
                            {/* Active right pip */}
                            {isActive && (
                              <div className="adm-pip w-1 h-3.5 rounded-full shrink-0"
                                style={{ background: 'rgba(255,255,255,0.35)' }} />
                            )}
                          </>
                        )}
                        {collapsed && <Tip label={item.label} />}
                      </>
                    )}
                  </NavLink>
                ))}
              </div>
            </div>
          ))}
        </nav>

        {/* ── Divider ── */}
        <div className="mx-3 shrink-0" style={{ height: 1, background: 'var(--sidebar-border)' }} />

        {/* ── Theme toggle ── */}
        <div className="px-2 pt-2 shrink-0">
          <ThemePill collapsed={collapsed} />
        </div>

        {/* ── Logout ── */}
        <div className="px-2 pb-3 shrink-0">
          <button
            onClick={() => logout().then(() => navigate('/login'))}
            className={`desktop-nav-item adm-tip-wrap w-full text-red-500 ${collapsed ? 'justify-center px-2' : ''}`}
            style={{ textDecoration: 'none' }}
            onMouseEnter={e => { e.currentTarget.style.background = 'rgba(239,68,68,.08)'; e.currentTarget.style.color = '#ef4444'; }}
            onMouseLeave={e => { e.currentTarget.style.background = 'transparent'; e.currentTarget.style.color = '#ef4444'; }}
          >
            <span style={{ fontSize: 14 }}>↩</span>
            {!collapsed && <span className="text-[13px]">Đăng xuất</span>}
            {collapsed && <Tip label="Đăng xuất" />}
          </button>
        </div>
      </aside>

      {/* ═══════════════════════════════ MAIN ══════════════════════════════════ */}
      <div className="flex-1 flex flex-col min-w-0">

        {/* ── Topbar ── */}
        <header
          className="sticky top-0 z-30 shrink-0 flex items-center justify-between"
          style={{
            height: 52,
            padding: '0 24px',
            background: 'var(--topbar-bg)',
            borderBottom: '1px solid var(--topbar-border)',
            backdropFilter: 'blur(8px)',
          }}
        >
          {/* Breadcrumb */}
          <div className="flex items-center gap-2" style={{ fontFamily: '"DM Sans", sans-serif' }}>
            <span className="text-xs font-semibold" style={{ color: 'var(--text-muted)' }}>Admin</span>
            {currentItem && (
              <>
                <svg width="12" height="12" viewBox="0 0 12 12" fill="none" style={{ color: 'var(--border)' }}>
                  <path d="M4.5 2.5L7.5 6l-3 3.5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
                </svg>
                <span className="text-[11px]" style={{ color: 'var(--text-muted)' }}>{currentItem.icon}</span>
                <span className="text-[13px] font-semibold" style={{ color: 'var(--text-primary)', fontFamily: '"Syne", sans-serif' }}>
                  {currentItem.label}
                </span>
              </>
            )}
          </div>

          {/* Right controls */}
          <div className="flex items-center gap-2">
            {/* Date */}
            <span className="text-[11px] hidden xl:block" style={{ color: 'var(--text-muted)' }}>
              {clock.toLocaleDateString('vi-VN', { weekday: 'short', day: 'numeric', month: 'short' })}
            </span>

            {/* Live clock */}
            <div
              className="font-mono tabular-nums text-[12px] px-2.5 py-1 rounded-lg"
              style={{
                background: 'var(--bg-elevated)',
                border: '1px solid var(--border)',
                color: 'var(--text-secondary)',
                letterSpacing: '.04em',
              }}
            >
              {clock.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
            </div>

            {/* Divider */}
            <div className="w-px h-5" style={{ background: 'var(--border)' }} />

            {/* View site */}
            <a
              href="/"
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-[12px] font-semibold transition-colors no-underline"
              style={{
                background: 'var(--bg-elevated)',
                border: '1px solid var(--border)',
                color: 'var(--text-secondary)',
              }}
              onMouseEnter={e => { e.currentTarget.style.background = 'var(--bg-hover)'; e.currentTarget.style.color = 'var(--text-primary)'; }}
              onMouseLeave={e => { e.currentTarget.style.background = 'var(--bg-elevated)'; e.currentTarget.style.color = 'var(--text-secondary)'; }}
            >
              <span>↗</span>
              <span>Xem website</span>
            </a>
          </div>
        </header>

        {/* ── Page content ── */}
        <main key={pageKey} className="adm-page flex-1 overflow-y-auto" style={{ padding: '28px 28px 48px' }}>
          {children}
        </main>
      </div>
    </div>
  );
}