import { useTheme } from '../../context/ThemeContext';
import { useLang } from '../../context/Langcontext';

export const ThemeToggle = ({ className = '' }) => {
  const { theme, toggleTheme } = useTheme();
  const { t } = useLang();
  const isDark = theme === 'dark';

  return (
    <button
      onClick={toggleTheme}
      aria-label={isDark ? t('theme.to_light') : t('theme.to_dark')}
      className={`flex items-center gap-2.5 px-3 py-2 rounded-xl transition-all duration-200 w-full ${className}`}
      style={{ color: 'var(--sidebar-text)' }}
      onMouseEnter={e => e.currentTarget.style.backgroundColor = 'var(--sidebar-hover)'}
      onMouseLeave={e => e.currentTarget.style.backgroundColor = 'transparent'}
    >
      {/* Toggle track */}
      <div style={{
        position: 'relative', width: 36, height: 20, borderRadius: 10, flexShrink: 0,
        backgroundColor: isDark ? '#0ea5e9' : '#cbd5e1',
        transition: 'background-color 0.3s'
      }}>
        <div style={{
          position: 'absolute', top: 2,
          left: isDark ? 18 : 2,
          width: 16, height: 16, borderRadius: '50%',
          backgroundColor: '#fff',
          transition: 'left 0.25s',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          fontSize: 9
        }}>
          {isDark ? '🌙' : '☀️'}
        </div>
      </div>
      <span className="text-sm font-medium" style={{ color: 'var(--sidebar-text)' }}>
        {isDark ? t('theme.dark') : t('theme.light')}
      </span>
    </button>
  );
};

// ─── Language Toggle ──────────────────────────────────────────────────────────
export const LangToggle = ({ className = '' }) => {
  const { lang, toggleLang, t } = useLang();
  const isVi = lang === 'vi';

  return (
    <button
      onClick={toggleLang}
      aria-label={isVi ? t('lang.switch_to_en') : t('lang.switch_to_vi')}
      className={`flex items-center gap-2.5 px-3 py-2 rounded-xl transition-all duration-200 w-full ${className}`}
      style={{ color: 'var(--sidebar-text)' }}
      onMouseEnter={e => e.currentTarget.style.backgroundColor = 'var(--sidebar-hover)'}
      onMouseLeave={e => e.currentTarget.style.backgroundColor = 'transparent'}
    >
      {/* Toggle track */}
      <div style={{
        position: 'relative', width: 36, height: 20, borderRadius: 10, flexShrink: 0,
        backgroundColor: '#0ea5e9',
        transition: 'background-color 0.3s'
      }}>
        <div style={{
          position: 'absolute', top: 2,
          left: isVi ? 2 : 18,
          width: 16, height: 16, borderRadius: '50%',
          backgroundColor: '#fff',
          transition: 'left 0.25s',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          fontSize: 9
        }}>
          {isVi ? '🇻🇳' : '🇬🇧'}
        </div>
      </div>
      <span className="text-sm font-medium" style={{ color: 'var(--sidebar-text)' }}>
        {isVi ? t('lang.vi') : t('lang.en')}
      </span>
    </button>
  );
};