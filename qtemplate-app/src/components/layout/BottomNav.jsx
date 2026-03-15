import { NavLink } from 'react-router-dom';
import { useTheme } from '../../context/ThemeContext';
import { useLang } from '../../context/Langcontext';         // ← THÊM
import { useNotification } from '../../context/NotificationContext';

// Định nghĩa nav với translation keys thay vì label cố định
const BOTTOM_NAV_AUTH = [
  { path: '/templates',               icon: '🗂️', labelKey: 'bottom.explore' },
  { path: '/dashboard/profile',       icon: '👤', labelKey: 'bottom.profile' },
  { path: '/dashboard/notifications', icon: '🔔', labelKey: 'bottom.notif' },
];

const BOTTOM_NAV_GUEST = [
  { path: '/templates', icon: '🗂️', labelKey: 'bottom.explore' },
  { path: '/login',     icon: '🔐', labelKey: 'bottom.login' },
  { path: '/register',  icon: '✨', labelKey: 'bottom.register' },
];

export const BottomNav = ({ isAuth, onLogout }) => {
  const items = isAuth ? BOTTOM_NAV_AUTH : BOTTOM_NAV_GUEST;
  const { theme, toggleTheme } = useTheme();
  const isDark = theme === 'dark';
  const { unreadCount } = isAuth ? useNotification() : { unreadCount: 0 };
  const { t, lang, toggleLang } = useLang();   // ← THÊM

  return (
    <nav
      className="lg:hidden fixed bottom-0 left-0 right-0 z-40 border-t backdrop-blur-md"
      style={{
        backgroundColor: 'var(--bottom-bg)',
        borderColor: 'var(--bottom-border)',
        paddingBottom: 'env(safe-area-inset-bottom, 0px)'
      }}
    >
      <div className="flex items-stretch h-16">
        {items.map(({ path, icon, labelKey }) => (
          <NavLink
            key={path}
            to={path}
            className="relative flex-1 flex flex-col items-center justify-center gap-0.5 transition-all duration-150"
            style={({ isActive }) => ({ color: isActive ? 'var(--bottom-active)' : 'var(--bottom-text)' })}
          >
            {({ isActive }) => (
              <>
                {isActive && (
                  <span className="absolute top-0 left-1/2 -translate-x-1/2 w-6 h-0.5 rounded-b-full"
                    style={{ backgroundColor: 'var(--bottom-indicator)' }} />
                )}
                <span className="relative">
                  <span className={`text-xl transition-all duration-150 ${isActive ? 'scale-110' : 'scale-100'} block`}>{icon}</span>
                  {path === '/dashboard/notifications' && unreadCount > 0 && (
                    <span className="absolute -top-1 -right-1 min-w-[14px] h-[14px] px-0.5 rounded-full text-[9px] font-black flex items-center justify-center"
                      style={{ backgroundColor: '#ef4444', color: '#fff' }}>
                      {unreadCount > 99 ? '99+' : unreadCount}
                    </span>
                  )}
                </span>
                <span className="text-[9px] font-bold tracking-wide uppercase leading-none">{t(labelKey)}</span>
              </>
            )}
          </NavLink>
        ))}

        {/* Language toggle */}
        <button
          onClick={toggleLang}
          className="relative flex-1 flex flex-col items-center justify-center gap-0.5 transition-all duration-150"
          style={{ color: 'var(--bottom-text)' }}
        >
          <span className="text-xl">{lang === 'vi' ? '🇻🇳' : '🇬🇧'}</span>
          <span className="text-[9px] font-bold tracking-wide uppercase leading-none">
            {lang === 'vi' ? 'VI' : 'EN'}
          </span>
        </button>

        {/* Theme toggle */}
        <button
          onClick={toggleTheme}
          className="relative flex-1 flex flex-col items-center justify-center gap-0.5 transition-all duration-150"
          style={{ color: 'var(--bottom-text)' }}
        >
          <span className="text-xl">{isDark ? '☀️' : '🌙'}</span>
          <span className="text-[9px] font-bold tracking-wide uppercase leading-none">
            {isDark ? t('bottom.light') : t('bottom.dark')}
          </span>
        </button>

        {/* Logout */}
        {isAuth && (
          <button
            onClick={onLogout}
            className="relative flex-1 flex flex-col items-center justify-center gap-0.5 text-red-400 active:text-red-600 transition-all"
          >
            <span className="text-xl">↩</span>
            <span className="text-[9px] font-bold tracking-wide uppercase leading-none">{t('bottom.logout')}</span>
          </button>
        )}
      </div>
    </nav>
  );
};