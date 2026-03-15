// ── Shared Admin UI — Theme-aware (CSS variables) ─────────────

export const fmt      = d => d ? new Date(d).toLocaleDateString('vi-VN') : '—';
export const fmtFull  = d => d ? new Date(d).toLocaleString('vi-VN')    : '—';
export const fmtMoney = n => n != null ? Number(n).toLocaleString('vi-VN') + '₫' : '—';

// ── Chip badge ──────────────────────────────────────────────────
export function Chip({ label, color = 'slate' }) {
  const map = {
    slate:  'bg-slate-100  text-slate-600  dark:bg-slate-700  dark:text-slate-300',
    blue:   'bg-blue-50    text-blue-700   dark:bg-blue-900/40  dark:text-blue-300',
    green:  'bg-green-50   text-green-700  dark:bg-green-900/40 dark:text-green-300',
    yellow: 'bg-amber-50   text-amber-700  dark:bg-amber-900/40 dark:text-amber-300',
    red:    'bg-red-50     text-red-600    dark:bg-red-900/40   dark:text-red-300',
    purple: 'bg-violet-50  text-violet-700 dark:bg-violet-900/40 dark:text-violet-300',
    indigo: 'bg-indigo-50  text-indigo-700 dark:bg-indigo-900/40 dark:text-indigo-300',
    orange: 'bg-orange-50  text-orange-700 dark:bg-orange-900/40 dark:text-orange-300',
  };
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-[11px] font-semibold whitespace-nowrap ${map[color] || map.slate}`}>
      {label}
    </span>
  );
}

// ── Active status dot ────────────────────────────────────────────
export function ActiveDot({ active, onLabel = 'Active', offLabel = 'Khoá' }) {
  return (
    <span className={`inline-flex items-center gap-1.5 text-[12px] font-semibold ${active ? 'text-green-500' : 'text-red-500'}`}>
      <span className={`w-1.5 h-1.5 rounded-full flex-shrink-0 ${active ? 'bg-green-500' : 'bg-red-500'}`} />
      {active ? onLabel : offLabel}
    </span>
  );
}

// ── Page header ──────────────────────────────────────────────────
export function PageHeader({ title, sub, action }) {
  return (
    <div className="flex items-end justify-between mb-6">
      <div>
        <h1 className="text-[22px] font-extrabold tracking-tight" style={{ color: 'var(--text-primary)', fontFamily: '"Syne", sans-serif' }}>{title}</h1>
        {sub && <p className="text-[13px] mt-0.5" style={{ color: 'var(--text-muted)' }}>{sub}</p>}
      </div>
      {action}
    </div>
  );
}

// ── Filters bar ──────────────────────────────────────────────────
export function FiltersBar({ children }) {
  return <div className="flex gap-2.5 flex-wrap items-center mb-4">{children}</div>;
}

// ── Card ─────────────────────────────────────────────────────────
export function Card({ children, className = '' }) {
  return (
    <div
      className={`rounded-2xl overflow-hidden shadow-sm ${className}`}
      style={{ background: 'var(--bg-card)', border: '1px solid var(--border)' }}
    >
      {children}
    </div>
  );
}

// ── Input ────────────────────────────────────────────────────────
export function Input({ className = '', ...props }) {
  return (
    <input
      className={`w-full px-3 py-2 rounded-xl text-[13px] focus:outline-none transition-all ${className}`}
      style={{
        background: 'var(--input-bg)',
        border: '1px solid var(--input-border)',
        color: 'var(--input-text)',
      }}
      onFocus={e => { e.target.style.borderColor = '#0ea5e9'; e.target.style.boxShadow = '0 0 0 3px rgba(14,165,233,0.15)'; }}
      onBlur={e  => { e.target.style.borderColor = 'var(--input-border)'; e.target.style.boxShadow = 'none'; }}
      {...props}
    />
  );
}

export function Select({ children, className = '', ...props }) {
  return (
    <select
      className={`px-3 py-2 rounded-xl text-[13px] cursor-pointer focus:outline-none transition-all ${className}`}
      style={{
        background: 'var(--input-bg)',
        border: '1px solid var(--input-border)',
        color: 'var(--input-text)',
      }}
      {...props}
    >
      {children}
    </select>
  );
}

export function Textarea({ className = '', ...props }) {
  return (
    <textarea
      className={`w-full px-3 py-2 rounded-xl text-[13px] focus:outline-none transition-all resize-none ${className}`}
      style={{
        background: 'var(--input-bg)',
        border: '1px solid var(--input-border)',
        color: 'var(--input-text)',
      }}
      onFocus={e => { e.target.style.borderColor = '#0ea5e9'; e.target.style.boxShadow = '0 0 0 3px rgba(14,165,233,0.15)'; }}
      onBlur={e  => { e.target.style.borderColor = 'var(--input-border)'; e.target.style.boxShadow = 'none'; }}
      {...props}
    />
  );
}

// ── Buttons ──────────────────────────────────────────────────────
export function BtnPrimary({ children, className = '', ...props }) {
  return (
    <button
      className={`inline-flex items-center gap-1.5 px-4 py-2 rounded-xl text-white text-[13px] font-semibold transition-colors disabled:opacity-40 disabled:cursor-not-allowed ${className}`}
      style={{ background: 'var(--sidebar-active-bg, #0f172a)' }}
      onMouseEnter={e => !e.currentTarget.disabled && (e.currentTarget.style.opacity = '0.85')}
      onMouseLeave={e => (e.currentTarget.style.opacity = '1')}
      {...props}
    >
      {children}
    </button>
  );
}

export function BtnSecondary({ children, className = '', ...props }) {
  return (
    <button
      className={`inline-flex items-center gap-1.5 px-4 py-2 rounded-xl text-[13px] font-semibold transition-colors disabled:opacity-40 ${className}`}
      style={{
        background: 'var(--bg-elevated)',
        border: '1px solid var(--border)',
        color: 'var(--text-secondary)',
      }}
      onMouseEnter={e => !e.currentTarget.disabled && (e.currentTarget.style.background = 'var(--bg-hover)')}
      onMouseLeave={e => (e.currentTarget.style.background = 'var(--bg-elevated)')}
      {...props}
    >
      {children}
    </button>
  );
}

export function BtnDanger({ children, className = '', ...props }) {
  return (
    <button
      className={`inline-flex items-center gap-1.5 px-4 py-2 rounded-xl bg-red-500 hover:bg-red-600 text-white text-[13px] font-semibold transition-colors disabled:opacity-40 ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}

export function BtnSuccess({ children, className = '', ...props }) {
  return (
    <button
      className={`inline-flex items-center gap-1.5 px-4 py-2 rounded-xl bg-emerald-500 hover:bg-emerald-600 text-white text-[13px] font-semibold transition-colors disabled:opacity-40 ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}

// ── Table ────────────────────────────────────────────────────────
export function Table({ heads, children, loading, colCount }) {
  const cols = colCount || heads.length;
  return (
    <div className="overflow-x-auto">
      <table className="w-full text-[13px] border-collapse">
        <thead>
          <tr style={{ background: 'var(--bg-elevated)', borderBottom: '1px solid var(--border)' }}>
            {heads.map(h => (
              <th key={h} className="px-4 py-3 text-left text-[11px] font-bold uppercase tracking-wider whitespace-nowrap"
                style={{ color: 'var(--text-muted)' }}>
                {h}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {loading
            ? Array.from({ length: 7 }).map((_, i) => (
                <tr key={i} style={{ borderBottom: '1px solid var(--border)' }}>
                  {Array.from({ length: cols }).map((_, j) => (
                    <td key={j} className="px-4 py-3">
                      <div
                        className={`h-3.5 rounded-md animate-pulse ${j === 0 ? 'w-2/5' : 'w-3/4'}`}
                        style={{ background: 'var(--bg-elevated)' }}
                      />
                    </td>
                  ))}
                </tr>
              ))
            : children
          }
        </tbody>
      </table>
    </div>
  );
}

// ── Table row / cell ─────────────────────────────────────────────
// Dùng JS string để dễ apply inline style khi cần
export const trBase = 'adm-tr border-b cursor-pointer transition-colors';
export const tdBase = 'px-4 py-3 text-[13px]';

// Inject row hover style once
let _rowCss = false;
if (typeof document !== 'undefined' && !_rowCss) {
  _rowCss = true;
  const s = document.createElement('style');
  s.textContent = `
    .adm-tr { border-color: var(--border) !important; color: var(--text-primary); }
    .adm-tr:hover { background: var(--bg-elevated) !important; }
  `;
  document.head.appendChild(s);
}

// ── Pagination ───────────────────────────────────────────────────
export function Pager({ page, total, pageSize, onChange }) {
  const tp = Math.max(1, Math.ceil(total / pageSize));
  if (tp <= 1) return null;
  const start = Math.max(1, Math.min(page - 2, tp - 4));
  const pages = Array.from({ length: Math.min(5, tp) }, (_, i) => start + i).filter(p => p <= tp);

  const btnBase = {
    border: '1px solid var(--border)',
    background: 'var(--bg-elevated)',
    color: 'var(--text-secondary)',
  };

  return (
    <div className="flex items-center justify-between px-4 py-3" style={{ borderTop: '1px solid var(--border)', background: 'var(--bg-elevated)' }}>
      <span className="text-[12px]" style={{ color: 'var(--text-muted)' }}>Trang {page}/{tp} · {total} mục</span>
      <div className="flex gap-1">
        <button onClick={() => onChange(page - 1)} disabled={page === 1}
          className="px-3 py-1.5 rounded-lg text-[12px] font-semibold disabled:opacity-30 transition-all"
          style={btnBase}
          onMouseEnter={e => !e.currentTarget.disabled && (e.currentTarget.style.background = 'var(--bg-hover)')}
          onMouseLeave={e => (e.currentTarget.style.background = 'var(--bg-elevated)')}
        >←</button>

        {pages.map(p => (
          <button key={p} onClick={() => onChange(p)}
            className="w-8 h-8 rounded-lg text-[12px] font-semibold transition-all"
            style={p === page
              ? { background: 'var(--sidebar-active-bg)', border: 'none', color: '#fff' }
              : btnBase
            }
            onMouseEnter={e => p !== page && (e.currentTarget.style.background = 'var(--bg-hover)')}
            onMouseLeave={e => p !== page && (e.currentTarget.style.background = 'var(--bg-elevated)')}
          >
            {p}
          </button>
        ))}

        <button onClick={() => onChange(page + 1)} disabled={page === tp}
          className="px-3 py-1.5 rounded-lg text-[12px] font-semibold disabled:opacity-30 transition-all"
          style={btnBase}
          onMouseEnter={e => !e.currentTarget.disabled && (e.currentTarget.style.background = 'var(--bg-hover)')}
          onMouseLeave={e => (e.currentTarget.style.background = 'var(--bg-elevated)')}
        >→</button>
      </div>
    </div>
  );
}

// ── Modal ────────────────────────────────────────────────────────
export function Modal({ open, onClose, title, width = 560, children }) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={onClose} />
      <div
        className="relative rounded-2xl shadow-2xl flex flex-col overflow-hidden"
        style={{
          width, maxWidth: '95vw', maxHeight: '90vh',
          background: 'var(--bg-card)',
          border: '1px solid var(--border)',
        }}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 flex-shrink-0"
          style={{ borderBottom: '1px solid var(--border)' }}>
          <h3 className="text-[15px] font-bold" style={{ color: 'var(--text-primary)', fontFamily: '"Syne", sans-serif' }}>
            {title}
          </h3>
          <button onClick={onClose}
            className="w-7 h-7 rounded-lg text-lg leading-none flex items-center justify-center transition-colors"
            style={{ border: '1px solid var(--border)', background: 'var(--bg-elevated)', color: 'var(--text-muted)' }}
            onMouseEnter={e => e.currentTarget.style.background = 'var(--bg-hover)'}
            onMouseLeave={e => e.currentTarget.style.background = 'var(--bg-elevated)'}
          >×</button>
        </div>
        {/* Body */}
        <div className="flex-1 overflow-y-auto p-5">{children}</div>
      </div>
    </div>
  );
}

// ── Field ────────────────────────────────────────────────────────
export function Field({ label, children, required, className = '' }) {
  return (
    <div className={`mb-3.5 ${className}`}>
      <label className="block text-[11px] font-bold uppercase tracking-wide mb-1.5" style={{ color: 'var(--text-muted)' }}>
        {label}{required && <span className="text-red-500 ml-0.5">*</span>}
      </label>
      {children}
    </div>
  );
}

// ── Tabs ─────────────────────────────────────────────────────────
export function Tabs({ tabs, active, onChange }) {
  return (
    <div className="flex mb-5" style={{ borderBottom: '1px solid var(--border)' }}>
      {tabs.map(([k, l]) => (
        <button key={k} onClick={() => onChange(k)}
          className="px-4 py-2.5 text-[12px] font-semibold transition-colors border-b-2 -mb-px"
          style={{
            color: active === k ? 'var(--text-primary)' : 'var(--text-muted)',
            borderBottomColor: active === k ? 'var(--text-primary)' : 'transparent',
          }}
        >
          {l}
        </button>
      ))}
    </div>
  );
}

// ── Confirm modal ────────────────────────────────────────────────
export function ConfirmModal({ open, onClose, onConfirm, busy, msg = 'Bạn chắc chắn muốn xoá mục này?' }) {
  return (
    <Modal open={open} onClose={onClose} title="Xác nhận" width={400}>
      <p className="text-[14px] mb-5" style={{ color: 'var(--text-secondary)' }}>{msg}</p>
      <div className="flex gap-2.5 justify-end">
        <BtnSecondary onClick={onClose}>Huỷ</BtnSecondary>
        <BtnDanger onClick={onConfirm} disabled={busy}>{busy ? '…' : 'Xoá'}</BtnDanger>
      </div>
    </Modal>
  );
}

// ── Toast ────────────────────────────────────────────────────────
export function Toast({ msg }) {
  if (!msg) return null;
  return (
    <div className="fixed top-4 left-1/2 -translate-x-1/2 z-[9999] text-white text-[13px] font-semibold px-4 py-2.5 rounded-xl shadow-xl animate-fade-in pointer-events-none"
      style={{ background: 'var(--sidebar-active-bg, #0f172a)' }}>
      {msg}
    </div>
  );
}

// ── Search input ─────────────────────────────────────────────────
export function SearchInput({ value, onChange, placeholder = 'Tìm kiếm…', className = '' }) {
  return (
    <div className={`relative ${className}`}>
      <span className="absolute left-3 top-1/2 -translate-y-1/2 text-[13px] pointer-events-none" style={{ color: 'var(--text-muted)' }}>🔍</span>
      <Input value={value} onChange={onChange} placeholder={placeholder} className="pl-8" />
    </div>
  );
}

// ── Empty state ───────────────────────────────────────────────────
export function Empty({ msg = 'Không có dữ liệu.' }) {
  return (
    <tr>
      <td colSpan={99} className="text-center py-12 text-[13px]" style={{ color: 'var(--text-muted)' }}>{msg}</td>
    </tr>
  );
}

// ── Stat card ─────────────────────────────────────────────────────
export function StatCard({ label, value, sub, icon }) {
  return (
    <div className="rounded-2xl p-5 shadow-sm" style={{ background: 'var(--bg-card)', border: '1px solid var(--border)' }}>
      <div className="flex items-center justify-between mb-3">
        <span className="text-[11px] font-bold uppercase tracking-wider" style={{ color: 'var(--text-muted)' }}>{label}</span>
        <span className="text-xl">{icon}</span>
      </div>
      <div className="text-[26px] font-extrabold tracking-tight leading-none" style={{ color: 'var(--text-primary)' }}>{value ?? '—'}</div>
      {sub && <div className="text-[12px] mt-1.5" style={{ color: 'var(--text-muted)' }}>{sub}</div>}
    </div>
  );
}