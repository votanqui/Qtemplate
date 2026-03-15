import { NavLink } from 'react-router-dom';
import { SIDEBAR_ALL } from './navConfig';
import { ThemeToggle } from './ThemeToggle';
import { useLang } from '../../context/Langcontext';

export const MobileSidebar = ({ open, onClose, user, isAuth, onLogout }) => {
  const { t } = useLang();

  return (
    <>
      {open && (
        <div className="lg:hidden fixed inset-0 z-50 bg-black/40 backdrop-blur-sm" onClick={onClose} />
      )}

      <aside
        className={`lg:hidden fixed inset-y-0 left-0 w-72 border-r shadow-2xl flex flex-col ${open ? 'translate-x-0' : '-translate-x-full'}`}
        style={{ zIndex: 60, backgroundColor: 'var(--sidebar-bg)', borderColor: 'var(--sidebar-border)', transition: 'transform 0.3s ease, background-color 0.25s ease' }}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-5 pt-5 pb-4 shrink-0">
          <NavLink to="/" onClick={onClose} className="flex items-center gap-2.5">
            <div className="w-8 h-8 rounded-xl bg-gray-900 flex items-center justify-center text-white font-black text-sm font-mono">Q</div>
            <div className="leading-none">
              <div className="font-black text-base tracking-tight" style={{ color: 'var(--sidebar-text-strong)' }}>Qtemplate</div>
              <div className="text-[9px] font-semibold tracking-widest uppercase mt-0.5" style={{ color: 'var(--text-muted)' }}>Marketplace</div>
            </div>
          </NavLink>
          <button onClick={onClose}
            className="w-8 h-8 rounded-xl flex items-center justify-center text-lg leading-none transition-colors"
            style={{ color: 'var(--text-secondary)' }}
          >×</button>
        </div>

        <div className="mx-5 h-px shrink-0" style={{ backgroundColor: 'var(--border)' }} />

        {/* User card */}
        {isAuth && (
          <div className="px-4 py-3 shrink-0">
            <div className="flex items-center gap-3 px-3 py-3 rounded-xl border"
              style={{ backgroundColor: 'var(--bg-elevated)', borderColor: 'var(--border)' }}>
              <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-violet-500 to-pink-500 flex items-center justify-center text-white font-bold text-sm shadow-sm shrink-0">
                {user?.fullName?.[0]?.toUpperCase() || 'U'}
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-bold truncate" style={{ color: 'var(--sidebar-text-strong)' }}>{user?.fullName || t('nav.user_default')}</p>
                <p className="text-xs capitalize truncate mt-0.5" style={{ color: 'var(--text-muted)' }}>{user?.role || 'Member'}</p>
              </div>
              <div className="w-2.5 h-2.5 rounded-full bg-emerald-400 shrink-0" />
            </div>
          </div>
        )}

        <div className="px-5 pb-2 shrink-0">
          <span className="text-[10px] font-bold uppercase tracking-widest" style={{ color: 'var(--text-muted)' }}>{t('sidebar.account_section')}</span>
        </div>

        {/* Nav */}
        <nav className="flex-1 overflow-y-auto min-h-0 px-4 pb-4 space-y-0.5">
          {isAuth ? (
            SIDEBAR_ALL.map(({ path, icon, label }) => (
              <NavLink
                key={path}
                to={path}
                onClick={onClose}
                className={({ isActive }) =>
                  `flex items-center gap-3 px-3 py-3 rounded-xl text-sm font-medium transition-colors ${
                    isActive ? 'mobile-nav-active' : 'mobile-nav-default'
                  }`
                }
              >
                {({ isActive }) => (
                  <>
                    <span className="text-lg w-5 text-center shrink-0">{icon}</span>
                    <span>{label}</span>
                  </>
                )}
              </NavLink>
            ))
          ) : (
            <div className="px-3 py-8 text-center space-y-2">
              <NavLink to="/login" onClick={onClose}
                className="flex items-center justify-center gap-2 w-full px-4 py-3 rounded-xl bg-gray-900 text-white text-sm font-bold hover:opacity-80 transition-opacity">
                {t('sidebar.login')}
              </NavLink>
              <NavLink to="/register" onClick={onClose}
                className="flex items-center justify-center gap-2 w-full px-4 py-3 rounded-xl border text-sm font-bold"
                style={{ borderColor: 'var(--border)', color: 'var(--sidebar-text)' }}>
                {t('sidebar.register_free')}
              </NavLink>
            </div>
          )}
        </nav>

        <div className="mx-4 h-px shrink-0" style={{ backgroundColor: 'var(--border)' }} />

        {/* Bottom actions */}
        <div className="px-4 py-3 space-y-0.5 shrink-0">
          <NavLink
            to="/templates"
            onClick={onClose}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-3 rounded-xl text-sm font-medium transition-colors ${
                isActive ? 'mobile-nav-active' : 'mobile-nav-default'
              }`
            }
          >
            <span className="text-lg w-5 text-center shrink-0">🗂️</span>
            <span>Templates</span>
          </NavLink>

          <ThemeToggle />

          {isAuth && (
            <button
              onClick={() => { onClose(); onLogout(); }}
              className="w-full flex items-center gap-3 px-3 py-3 rounded-xl text-sm font-medium text-red-500 hover:bg-red-500/10 transition-all"
            >
              <span className="text-lg w-5 text-center shrink-0">↩</span>
              <span>{t('sidebar.logout')}</span>
            </button>
          )}
        </div>
      </aside>
    </>
  );
};