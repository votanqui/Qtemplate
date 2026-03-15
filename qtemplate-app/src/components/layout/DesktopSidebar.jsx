import { NavLink } from 'react-router-dom';
import { SIDEBAR_SECTIONS } from './navConfig';
import { ThemeToggle, LangToggle } from './ThemeToggle';
import { useNotification } from '../../context/NotificationContext';
import { useLang } from '../../context/Langcontext';
import { useTheme } from '../../context/ThemeContext';

const SECTION_LABEL_KEYS = {
  main:    'nav.explore',
  library: 'nav.library',
  account: 'nav.account',
};
const ITEM_LABEL_KEYS = {
  '/':                         'nav.home',
  '/templates':                'nav.templates',
  '/sale':                     'nav.sale',
  '/dashboard/downloads':      'nav.downloads',
  '/dashboard/wishlist':       'nav.wishlist',
  '/dashboard/notifications':  'nav.notifications',
  '/dashboard/coupons':        'nav.coupons',
  '/dashboard/profile':        'nav.profile',
  '/dashboard/purchases':      'nav.purchases',
  '/dashboard/reviews':        'nav.reviews',
  '/dashboard/tickets':        'nav.support',
  '/dashboard/affiliate':      'nav.affiliate',
  '/dashboard/security':       'nav.security',
};

// Tooltip — dùng CSS :hover selector trực tiếp qua style tag injected một lần
const TOOLTIP_CSS = `
  .sq-tip-wrap:hover .sq-tip { opacity: 1 !important; pointer-events: none; }
`;
let _tipInjected = false;
function injectTipCss() {
  if (_tipInjected || typeof document === 'undefined') return;
  const s = document.createElement('style');
  s.textContent = TOOLTIP_CSS;
  document.head.appendChild(s);
  _tipInjected = true;
}

const Tip = ({ children, color }) => {
  injectTipCss();
  return (
    <span
      className="sq-tip pointer-events-none absolute left-full ml-3 px-2.5 py-1.5 rounded-lg text-xs font-semibold whitespace-nowrap z-[999]"
      style={{
        top: '50%', transform: 'translateY(-50%)',
        opacity: 0, transition: 'opacity 0.12s',
        backgroundColor: 'var(--bg-hover)',
        border: '1px solid var(--border)',
        color: color || 'var(--text-primary)',
        boxShadow: '0 4px 14px rgba(0,0,0,0.2)',
      }}
    >
      {children}
    </span>
  );
};

const NavItem = ({ path, icon, label, badge = 0, collapsed }) => (
  <NavLink
    to={path}
    end={path === '/'}
    className={({ isActive }) =>
      `desktop-nav-item sq-tip-wrap ${isActive ? 'active' : ''}`
    }
    style={{
      justifyContent: collapsed ? 'center' : undefined,
      padding: collapsed ? '0.45rem' : undefined,
      position: 'relative',
    }}
  >
    {({ isActive }) => (
      <>
        <span
          style={{
            fontSize: collapsed ? 17 : 14,
            width: collapsed ? 'auto' : '1rem',
            textAlign: 'center',
            flexShrink: 0,
            transition: 'transform 0.15s',
          }}
        >
          {icon}
        </span>

        {!collapsed && <span className="truncate text-sm">{label}</span>}

        {badge > 0 && (
          <span
            className="min-w-[16px] h-4 px-1 rounded-full text-[10px] font-black flex items-center justify-center shrink-0"
            style={{
              backgroundColor: '#ef4444', color: '#fff',
              marginLeft: collapsed ? undefined : 'auto',
              position: collapsed ? 'absolute' : undefined,
              top: collapsed ? 2 : undefined,
              right: collapsed ? 2 : undefined,
            }}
          >
            {badge > 99 ? '99+' : badge}
          </span>
        )}

        {!collapsed && !badge && isActive && (
          <div className="ml-auto w-1 h-3.5 rounded-full bg-white/25 shrink-0" />
        )}

        {collapsed && <Tip>{label}{badge > 0 ? ` · ${badge}` : ''}</Tip>}
      </>
    )}
  </NavLink>
);

const IconBtn = ({ onClick, icon, tip }) => (
  <button
    onClick={onClick}
    className="sq-tip-wrap w-9 h-9 rounded-xl flex items-center justify-center transition-colors relative"
    style={{ fontSize: 16, color: 'var(--sidebar-text)' }}
    onMouseEnter={e => e.currentTarget.style.backgroundColor = 'var(--sidebar-hover)'}
    onMouseLeave={e => e.currentTarget.style.backgroundColor = 'transparent'}
  >
    {icon}
    <Tip>{tip}</Tip>
  </button>
);

export const DesktopSidebar = ({ user, isAuth, onLogout, collapsed, onToggleCollapse }) => {
  const sections = isAuth
    ? SIDEBAR_SECTIONS
    : SIDEBAR_SECTIONS.filter(s => s.key === 'main');

  const { unreadCount }         = isAuth ? useNotification() : { unreadCount: 0 };
  const { t, lang, toggleLang } = useLang();
  const { theme, toggleTheme }  = useTheme();
  const isDark = theme === 'dark';
  const isVi   = lang === 'vi';

  const W = collapsed ? 64 : 224;

  return (
    <aside
      className="hidden lg:flex flex-col h-screen sticky top-0 border-r shrink-0"
      style={{
        width: W, minWidth: W,
        backgroundColor: 'var(--sidebar-bg)',
        borderColor: 'var(--sidebar-border)',
        transition: 'width 0.25s cubic-bezier(0.4,0,0.2,1), min-width 0.25s cubic-bezier(0.4,0,0.2,1)',
        overflow: collapsed ? 'visible' : 'hidden',
        zIndex: 30,
      }}
    >
      {/* Logo row */}
      {collapsed ? (
        /* Collapsed: logo trên, toggle dưới — xếp dọc để không đè nhau */
        <div className="flex flex-col items-center gap-2 pt-4 pb-3 shrink-0">
          <NavLink to="/" className="group">
            <div className="w-8 h-8 rounded-xl bg-gray-900 flex items-center justify-center text-white font-black text-sm font-mono shadow-md group-hover:opacity-80 transition-opacity">
              Q
            </div>
          </NavLink>
          <button
            onClick={onToggleCollapse}
            className="w-7 h-7 rounded-lg flex items-center justify-center shrink-0 transition-colors"
            style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-muted)' }}
            onMouseEnter={e => e.currentTarget.style.backgroundColor = 'var(--sidebar-hover)'}
            onMouseLeave={e => e.currentTarget.style.backgroundColor = 'var(--bg-elevated)'}
            title="Mở sidebar"
          >
            <svg width="10" height="10" viewBox="0 0 10 10" fill="none"
              style={{ transform: 'rotate(180deg)', transition: 'transform 0.25s' }}>
              <path d="M7 1.5L3 5l4 3.5" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round"/>
            </svg>
          </button>
        </div>
      ) : (
        /* Expanded: logo trái, toggle phải — ngang hàng */
        <div className="px-3 pt-4 pb-3 shrink-0 flex items-center justify-between gap-2">
          <NavLink to="/" className="flex items-center gap-2 group min-w-0">
            <div className="w-8 h-8 rounded-xl bg-gray-900 flex items-center justify-center text-white font-black text-sm font-mono shadow-md group-hover:opacity-80 transition-opacity shrink-0">
              Q
            </div>
            <div className="leading-none min-w-0">
              <div className="font-black text-base tracking-tight truncate" style={{ color: 'var(--sidebar-text-strong)' }}>Qtemplate</div>
              <div className="text-[9px] font-semibold tracking-widest uppercase mt-0.5" style={{ color: 'var(--text-muted)' }}>Marketplace</div>
            </div>
          </NavLink>
          <button
            onClick={onToggleCollapse}
            className="w-7 h-7 rounded-lg flex items-center justify-center shrink-0 transition-colors"
            style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-muted)' }}
            onMouseEnter={e => e.currentTarget.style.backgroundColor = 'var(--sidebar-hover)'}
            onMouseLeave={e => e.currentTarget.style.backgroundColor = 'var(--bg-elevated)'}
            title="Thu nhỏ"
          >
            <svg width="10" height="10" viewBox="0 0 10 10" fill="none">
              <path d="M7 1.5L3 5l4 3.5" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round"/>
            </svg>
          </button>
        </div>
      )}

      <div className="mx-3 h-px shrink-0" style={{ backgroundColor: 'var(--sidebar-border)' }} />

      {/* User card – expanded */}
      {isAuth && !collapsed && (
        <div className="px-3 pt-2.5 pb-1.5 shrink-0">
          <div className="flex items-center gap-2.5 px-2.5 py-2 rounded-xl border"
            style={{ backgroundColor: 'var(--bg-elevated)', borderColor: 'var(--border)' }}>
            <div className="w-7 h-7 rounded-lg bg-gradient-to-br from-violet-500 to-pink-500 flex items-center justify-center text-white font-bold text-xs shadow-sm shrink-0">
              {user?.fullName?.[0]?.toUpperCase() || 'U'}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-xs font-bold truncate leading-tight" style={{ color: 'var(--sidebar-text-strong)' }}>
                {user?.fullName || t('nav.user_default')}
              </p>
              <p className="text-[10px] capitalize truncate" style={{ color: 'var(--text-muted)' }}>
                {user?.role || 'Member'}
              </p>
            </div>
            <div className="w-1.5 h-1.5 rounded-full bg-emerald-400 shrink-0" />
          </div>
        </div>
      )}

      {/* Avatar – collapsed */}
      {isAuth && collapsed && (
        <div className="flex justify-center py-2.5 shrink-0">
          <div className="sq-tip-wrap relative">
            <div className="w-8 h-8 rounded-xl bg-gradient-to-br from-violet-500 to-pink-500 flex items-center justify-center text-white font-bold text-xs shadow-sm">
              {user?.fullName?.[0]?.toUpperCase() || 'U'}
            </div>
            <div className="w-2 h-2 rounded-full bg-emerald-400 absolute -bottom-0.5 -right-0.5 border-2"
              style={{ borderColor: 'var(--sidebar-bg)' }} />
            <Tip>{user?.fullName || 'User'} · {user?.role || 'Member'}</Tip>
          </div>
        </div>
      )}

      {/* Nav */}
      <nav
        className="flex-1 min-h-0 py-2 space-y-3"
        style={{ overflowY: 'auto', overflowX: collapsed ? 'visible' : 'hidden', padding: '8px' }}
      >
        {sections.map(section => (
          <div key={section.key}>
            {!collapsed ? (
              <p className="px-3 mb-1 text-[10px] font-bold uppercase tracking-widest" style={{ color: 'var(--text-muted)' }}>
                {t(SECTION_LABEL_KEYS[section.key] || section.label)}
              </p>
            ) : (
              <div className="mx-1 h-px mb-1" style={{ backgroundColor: 'var(--sidebar-border)' }} />
            )}
            <div className="space-y-0.5">
              {section.items.map(item => (
                <NavItem
                  key={item.path}
                  {...item}
                  label={t(ITEM_LABEL_KEYS[item.path] || item.label)}
                  badge={item.path === '/dashboard/notifications' ? unreadCount : 0}
                  collapsed={collapsed}
                />
              ))}
            </div>
          </div>
        ))}

        {!isAuth && (
          <div className={`pt-1 space-y-1.5 ${collapsed ? 'flex flex-col items-center' : 'px-1'}`}>
            <NavLink to="/login"
              className="sq-tip-wrap flex items-center justify-center rounded-xl bg-gray-900 text-white text-xs font-bold hover:opacity-80 transition-opacity relative"
              style={{ width: collapsed ? 36 : '100%', height: 34 }}>
              {collapsed ? '🔐' : `🔐 ${t('nav.login')}`}
              {collapsed && <Tip>{t('nav.login')}</Tip>}
            </NavLink>
            <NavLink to="/register"
              className="sq-tip-wrap flex items-center justify-center rounded-xl border text-xs font-bold transition-colors relative"
              style={{ width: collapsed ? 36 : '100%', height: 34, borderColor: 'var(--border)', color: 'var(--sidebar-text)' }}>
              {collapsed ? '✨' : `✨ ${t('nav.register')}`}
              {collapsed && <Tip>{t('nav.register')}</Tip>}
            </NavLink>
          </div>
        )}
      </nav>

      <div className="mx-3 h-px shrink-0" style={{ backgroundColor: 'var(--sidebar-border)' }} />

      {/* Theme + Lang */}
      <div className="px-2 pt-1.5 pb-1 shrink-0">
        {collapsed ? (
          <div className="flex flex-col items-center gap-0.5">
            <IconBtn onClick={toggleTheme} icon={isDark ? '🌙' : '☀️'} tip={isDark ? 'Light mode' : 'Dark mode'} />
            <IconBtn onClick={toggleLang}  icon={isVi  ? '🇻🇳' : '🇬🇧'} tip={isVi  ? 'Switch to English' : 'Chuyển Tiếng Việt'} />
          </div>
        ) : (
          <div className="space-y-0.5">
            <ThemeToggle />
            <LangToggle />
          </div>
        )}
      </div>

      {/* Logout */}
      {isAuth && (
        <div className="px-2 pb-3 shrink-0">
          <button
            onClick={onLogout}
            className="sq-tip-wrap w-full flex items-center rounded-xl text-sm font-medium text-red-500 hover:bg-red-500/10 transition-all relative"
            style={{
              justifyContent: collapsed ? 'center' : undefined,
              gap: collapsed ? 0 : '0.6rem',
              padding: collapsed ? '0.45rem' : '0.5rem 0.75rem',
            }}
          >
            <span className="text-sm shrink-0" style={{ transition: 'transform 0.15s' }}>↩</span>
            {!collapsed && <span>{t('nav.logout')}</span>}
            {collapsed && <Tip color="#ef4444">{t('nav.logout')}</Tip>}
          </button>
        </div>
      )}
    </aside>
  );
};