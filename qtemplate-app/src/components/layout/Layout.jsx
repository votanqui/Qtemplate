import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useLang } from '../../context/Langcontext';
import { DesktopSidebar } from './DesktopSidebar';
import { MobileSidebar } from './MobileSidebar';
import { BottomNav } from './BottomNav';
import { RightPanel } from './RightPanel';

export const Layout = ({ children }) => {
  const { user, logout, isAuth } = useAuth();
  const { t } = useLang();
  const navigate = useNavigate();
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [sideCollapsed, setSideCollapsed] = useState(true);

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <div className="flex min-h-screen" style={{ backgroundColor: 'var(--bg-page)', color: 'var(--text-primary)' }}>

      {/* Left sidebar */}
      <DesktopSidebar
        user={user}
        isAuth={isAuth}
        onLogout={handleLogout}
        collapsed={sideCollapsed}
        onToggleCollapse={() => setSideCollapsed(v => !v)}
      />

      <MobileSidebar
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        user={user}
        isAuth={isAuth}
        onLogout={handleLogout}
      />

      {/* Center */}
      <div className="flex-1 flex flex-col min-w-0 min-h-0">

        {/* Mobile top bar */}
        <header className="lg:hidden sticky top-0 z-20 flex items-center justify-between px-4 h-14 border-b shadow-sm"
          style={{ backgroundColor: 'var(--topbar-bg)', borderColor: 'var(--topbar-border)' }}>
          <button
            onClick={() => setDrawerOpen(true)}
            className="w-9 h-9 flex items-center justify-center rounded-xl transition-colors"
            style={{ backgroundColor: 'var(--topbar-btn-bg)', color: 'var(--text-secondary)' }}
            aria-label={t('layout.open_menu')}
          >
            <svg width="18" height="14" viewBox="0 0 18 14" fill="none">
              <path d="M0 1h18M0 7h12M0 13h18" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
            </svg>
          </button>

          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-lg bg-gray-900 flex items-center justify-center text-white font-black text-xs font-mono">Q</div>
            <span className="font-black text-base tracking-tight" style={{ color: 'var(--text-primary)' }}>Qtemplate</span>
          </div>

          <div className="w-9 h-9 flex items-center justify-center">
            {isAuth ? (
              <div className="w-8 h-8 rounded-xl bg-gradient-to-br from-violet-500 to-pink-500 flex items-center justify-center text-white font-bold text-xs shadow-sm">
                {user?.fullName?.[0]?.toUpperCase() || 'U'}
              </div>
            ) : <div className="w-9" />}
          </div>
        </header>

        <main className="flex-1 overflow-y-auto p-4 lg:p-8 max-w-7xl mx-auto w-full pb-20 lg:pb-8">
          {children}
        </main>
      </div>

      {/* Right panel (xl+) */}
      <RightPanel />

      <BottomNav isAuth={isAuth} onLogout={handleLogout} />
    </div>
  );
};