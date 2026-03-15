import { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { createPortal } from 'react-dom';
import { useLang } from '../../context/Langcontext';

// ══════════════════════════════════════════════
// TOAST SYSTEM
// ══════════════════════════════════════════════
const ToastContext = createContext(null);

const toastStyles = {
  success: { bg: '#f0fdf4', border: '#86efac', text: '#15803d', icon: '✅' },
  error:   { bg: '#fef2f2', border: '#fca5a5', text: '#dc2626', icon: '❌' },
  warning: { bg: '#fffbeb', border: '#fcd34d', text: '#d97706', icon: '⚠️' },
  info:    { bg: '#eff6ff', border: '#93c5fd', text: '#2563eb', icon: 'ℹ️' },
};

export const Portal = ({ children }) => {
  return createPortal(children, document.body);
};

const ToastItem = ({ toast, onRemove }) => {
  const { t } = useLang();
  const [visible, setVisible] = useState(false);
  const [leaving, setLeaving] = useState(false);
  const s = toastStyles[toast.type] || toastStyles.info;
  const displayIcon = toast._icon || s.icon;

  useEffect(() => {
    const t1 = setTimeout(() => setVisible(true), 10);
    const t2 = setTimeout(() => dismiss(), toast.duration || 3500);
    return () => { clearTimeout(t1); clearTimeout(t2); };
  }, []);

  const dismiss = () => {
    setLeaving(true);
    setTimeout(() => onRemove(toast.id), 300);
  };

  const handleClick = () => {
    if (toast.onClick) { toast.onClick(); dismiss(); }
    else dismiss();
  };

  return (
    <div
      onClick={handleClick}
      style={{
        display: 'flex',
        alignItems: 'flex-start',
        gap: 10,
        padding: '12px 14px',
        borderRadius: 14,
        border: `1px solid ${s.border}`,
        backgroundColor: s.bg,
        color: s.text,
        boxShadow: '0 4px 24px rgba(0,0,0,0.10)',
        cursor: 'pointer',
        minWidth: 260,
        maxWidth: 360,
        transform: visible && !leaving ? 'translateY(0) scale(1)' : 'translateY(12px) scale(0.95)',
        opacity: visible && !leaving ? 1 : 0,
        transition: 'all 0.28s cubic-bezier(0.34,1.56,0.64,1)',
        userSelect: 'none',
      }}
    >
      <span style={{ fontSize: 16, lineHeight: 1.4, flexShrink: 0 }}>{displayIcon}</span>
      <div style={{ flex: 1, minWidth: 0 }}>
        {toast.title && (
          <p style={{ fontWeight: 700, fontSize: 13, marginBottom: 1, lineHeight: 1.3 }}>{toast.title}</p>
        )}
        {toast.message && (
          <p style={{ fontSize: 13, lineHeight: 1.4, opacity: toast.title ? 0.8 : 1 }}>{toast.message}</p>
        )}
        {toast.onClick && (
          <p style={{ fontSize: 11, marginTop: 4, opacity: 0.6, fontWeight: 600, textDecoration: 'underline' }}>
            {t('ui.click_to_notify')}
          </p>
        )}
      </div>
      <button
        onClick={e => { e.stopPropagation(); dismiss(); }}
        style={{ fontSize: 16, lineHeight: 1, opacity: 0.4, flexShrink: 0, background: 'none', border: 'none', cursor: 'pointer', color: 'inherit', padding: 0 }}
      >×</button>
    </div>
  );
};

export const ToastProvider = ({ children }) => {
  const [toasts, setToasts] = useState([]);

  const toast = useCallback((type, messageOrObj, title) => {
    const id = Date.now() + Math.random();
    const item = typeof messageOrObj === 'object'
      ? { id, type, ...messageOrObj }
      : { id, type, message: messageOrObj, title };
    setToasts(prev => [...prev, item]);
    return id;
  }, []);

  const remove = useCallback(id => {
    setToasts(prev => prev.filter(t => t.id !== id));
  }, []);

  const api = {
    success: (msg, title) => toast('success', msg, title),
    error:   (msg, title) => toast('error',   msg, title),
    warning: (msg, title) => toast('warning', msg, title),
    info:    (msg, title) => toast('info',    msg, title),
  };

  return (
    <ToastContext.Provider value={api}>
      {children}
      <div style={{
        position: 'fixed',
        zIndex: 9999,
        display: 'flex',
        flexDirection: 'column',
        gap: 8,
        top: 20,
        right: 20,
        pointerEvents: 'none',
      }}
        className="toast-container"
      >
        {toasts.map(t => (
          <div key={t.id} style={{ pointerEvents: 'auto' }}>
            <ToastItem toast={t} onRemove={remove} />
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
};

export const useToast = () => {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast must be used within ToastProvider');
  return ctx;
};

// ══════════════════════════════════════════════
// SPINNER
// ══════════════════════════════════════════════
export const Spinner = ({ size = 'sm', className = '' }) => {
  const s = size === 'lg' ? 'w-8 h-8 border-[3px]' : 'w-4 h-4 border-2';
  return (
    <div className={`${s} border-gray-200 border-t-current rounded-full animate-spin inline-block ${className}`} />
  );
};

// ══════════════════════════════════════════════
// LOADING PAGE
// ══════════════════════════════════════════════
export const LoadingPage = () => {
  const { t } = useLang();
  return (
    <div className="flex flex-col items-center justify-center min-h-[400px] gap-3">
      <div className="relative">
        <div className="w-12 h-12 rounded-2xl flex items-center justify-center"
          style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)' }}>
          <Spinner size="sm" style={{ color: 'var(--text-primary)' }} />
        </div>
      </div>
      <p className="text-sm font-medium" style={{ color: 'var(--text-muted)' }}>{t('common.loading')}</p>
    </div>
  );
};

// ══════════════════════════════════════════════
// ALERT
// ══════════════════════════════════════════════
const alertStyles = {
  error:   'bg-red-50 border-red-200 text-red-700',
  success: 'bg-emerald-50 border-emerald-200 text-emerald-700',
  info:    'bg-violet-50 border-violet-200 text-violet-700',
  warning: 'bg-amber-50 border-amber-200 text-amber-700',
};
const alertIcons = { error: '⚠️', success: '✅', info: 'ℹ️', warning: '⚡' };

export const Alert = ({ type = 'error', children, onClose }) => (
  <div className={`flex items-start gap-2.5 p-3.5 rounded-xl text-sm border animate-fade-in ${alertStyles[type]}`}>
    <span className="shrink-0 mt-px">{alertIcons[type]}</span>
    <span className="flex-1 whitespace-pre-wrap">{children}</span>
    {onClose && (
      <button onClick={onClose} className="shrink-0 opacity-50 hover:opacity-100 leading-none text-lg transition-opacity">×</button>
    )}
  </div>
);

// ══════════════════════════════════════════════
// STATUS BADGE
// ══════════════════════════════════════════════
const statusMap = {
  Paid:       { bg: 'bg-emerald-50 border-emerald-200 text-emerald-700' },
  Pending:    { bg: 'bg-amber-50 border-amber-200 text-amber-700' },
  Cancelled:  { bg: 'bg-red-50 border-red-200 text-red-600' },
  Completed:  { bg: 'bg-emerald-50 border-emerald-200 text-emerald-700' },
  Open:       { bg: 'bg-sky-50 border-sky-200 text-sky-700' },
  InProgress: { bg: 'bg-violet-50 border-violet-200 text-violet-700' },
  Resolved:   { bg: 'bg-emerald-50 border-emerald-200 text-emerald-700' },
  Closed:     { bg: 'bg-gray-100 border-gray-200 text-gray-600' },
  Failed:     { bg: 'bg-red-50 border-red-200 text-red-600' },
  Published:  { bg: 'bg-emerald-50 border-emerald-200 text-emerald-700' },
};

const STATUS_KEYS = {
  Paid:       'status.paid',
  Pending:    'status.pending',
  Cancelled:  'status.cancelled',
  Completed:  'status.completed',
  Open:       'status.open',
  InProgress: 'status.in_progress',
  Resolved:   'status.resolved',
  Closed:     'status.closed',
  Failed:     'status.failed',
};

export const StatusBadge = ({ status }) => {
  const { t } = useLang();
  const s = statusMap[status] || { bg: 'bg-gray-100 border-gray-200 text-gray-600' };
  const label = STATUS_KEYS[status] ? t(STATUS_KEYS[status]) : status;
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold border ${s.bg}`}>
      {label}
    </span>
  );
};

// ══════════════════════════════════════════════
// PRIORITY BADGE
// ══════════════════════════════════════════════
const priorityMap = {
  Low:    'bg-gray-100 border-gray-200 text-gray-600',
  Medium: 'bg-amber-50 border-amber-200 text-amber-700',
  High:   'bg-orange-50 border-orange-200 text-orange-700',
  Urgent: 'bg-red-50 border-red-200 text-red-700',
};

export const PriorityBadge = ({ priority }) => (
  <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold border ${priorityMap[priority] || 'bg-gray-100 border-gray-200 text-gray-600'}`}>
    {priority}
  </span>
);

// ══════════════════════════════════════════════
// PAGINATION
// ══════════════════════════════════════════════
export const Pagination = ({ page, totalPages, onPageChange }) => {
  const { t } = useLang();
  if (totalPages <= 1) return null;
  return (
    <div className="flex items-center justify-center gap-3 mt-8">
      <button
        disabled={page === 1}
        onClick={() => onPageChange(page - 1)}
        className="flex items-center gap-1.5 px-4 py-2 rounded-xl border text-sm font-semibold disabled:opacity-30 disabled:cursor-not-allowed transition-all shadow-sm"
        style={{ backgroundColor: 'var(--bg-card)', borderColor: 'var(--border)', color: 'var(--text-secondary)' }}
      >{t('ui.prev')}</button>

      <div className="flex items-center gap-1">
        {Array.from({ length: Math.min(totalPages, 5) }, (_, i) => {
          const p = i + Math.max(1, page - 2);
          if (p > totalPages) return null;
          return (
            <button
              key={p}
              onClick={() => onPageChange(p)}
              className="w-9 h-9 rounded-xl text-sm font-bold transition-all"
              style={p === page
                ? { backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)' }
                : { backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }
              }
            >{p}</button>
          );
        })}
      </div>

      <button
        disabled={page === totalPages}
        onClick={() => onPageChange(page + 1)}
        className="flex items-center gap-1.5 px-4 py-2 rounded-xl border text-sm font-semibold disabled:opacity-30 disabled:cursor-not-allowed transition-all shadow-sm"
        style={{ backgroundColor: 'var(--bg-card)', borderColor: 'var(--border)', color: 'var(--text-secondary)' }}
      >{t('ui.next')}</button>
    </div>
  );
};

// ══════════════════════════════════════════════
// MODAL
// ══════════════════════════════════════════════
export const Modal = ({ open, onClose, title, children, size = 'md' }) => {
  if (!open) return null;

  useEffect(() => {
    document.body.style.overflow = 'hidden';
    return () => { document.body.style.overflow = ''; };
  }, []);

  const sizeMap = { sm: 'max-w-sm', md: 'max-w-md', lg: 'max-w-2xl', xl: 'max-w-4xl' };

  return (
    <div
      className="fixed inset-0 z-[200] flex items-start justify-center p-4 pt-20 lg:items-center lg:pt-4"
      onClick={e => e.target === e.currentTarget && onClose()}
    >
      <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={onClose} />
      <div
        className={`relative w-full ${sizeMap[size]} rounded-3xl shadow-2xl animate-fade-in max-h-[80vh] flex flex-col`}
        style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-5 shrink-0"
          style={{ borderBottom: '1px solid var(--border)' }}>
          <h3 className="font-black text-base tracking-tight" style={{ color: 'var(--text-primary)' }}>{title}</h3>
          <button
            onClick={onClose}
            className="w-8 h-8 rounded-lg flex items-center justify-center text-xl leading-none transition-all"
            style={{ backgroundColor: 'var(--bg-elevated)', color: 'var(--text-secondary)' }}
          >×</button>
        </div>
        {/* Body — scrollable */}
        <div className="p-6 overflow-y-auto">{children}</div>
      </div>
    </div>
  );
};

// ══════════════════════════════════════════════
// EMPTY STATE
// ══════════════════════════════════════════════
export const EmptyState = ({ icon = '📭', title, description = '' }) => {
  const { t } = useLang();
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      <div className="w-16 h-16 rounded-2xl flex items-center justify-center text-3xl mb-4"
        style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)' }}>
        {icon}
      </div>
      <p className="font-bold mb-1" style={{ color: 'var(--text-primary)' }}>{title ?? t('ui.no_data')}</p>
      {description && <p className="text-sm mt-0.5" style={{ color: 'var(--text-muted)' }}>{description}</p>}
    </div>
  );
};

// ══════════════════════════════════════════════
// STAR RATING
// ══════════════════════════════════════════════
export const StarRating = ({ value = 0, onChange = null, max = 5 }) => (
  <div className="flex gap-1">
    {Array.from({ length: max }).map((_, i) => (
      <button
        key={i}
        type="button"
        onClick={() => onChange && onChange(i + 1)}
        className={`text-xl transition-all duration-150
          ${i < value ? 'text-amber-400 scale-110' : 'text-gray-300'}
          ${onChange ? 'hover:text-amber-300 hover:scale-125 cursor-pointer' : 'cursor-default'}
        `}
      >★</button>
    ))}
  </div>
);

// ══════════════════════════════════════════════
// FORM FIELD
// ══════════════════════════════════════════════
export const FormField = ({ label, error, children, required }) => (
  <div>
    {label && (
      <label className="block text-sm font-semibold mb-1.5" style={{ color: 'var(--text-secondary)' }}>
        {label} {required && <span className="text-red-500">*</span>}
      </label>
    )}
    {children}
    {error && <p className="text-xs text-red-500 mt-1 font-medium">{error}</p>}
  </div>
);

// ══════════════════════════════════════════════
// PRICE
// ══════════════════════════════════════════════
export const Price = ({ amount, className = '', style }) => (
  <span className={className} style={style}>
    {amount?.toLocaleString('vi-VN')}₫
  </span>
);