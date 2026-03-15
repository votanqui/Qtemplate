import { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { couponApi } from '../../api/services';
import { extractError } from '../../api/client';
import { LoadingPage, useToast } from '../../components/ui';
import { useLang } from '../../context/Langcontext';

export default function CouponsPage() {
  const { t } = useLang();
  const toast = useToast();
  const [coupons, setCoupons] = useState([]);
  const [loading, setLoading] = useState(true);
  const [copiedId, setCopiedId] = useState(null);
  const [filter, setFilter] = useState('all');

  useEffect(() => {
    couponApi.getPublicList()
      .then(res => setCoupons(res.data.data?.items || res.data.data || []))
      .catch(err => toast.error(extractError(err)))
      .finally(() => setLoading(false));
  }, []);

  const handleCopy = (code, id) => {
    navigator.clipboard.writeText(code);
    setCopiedId(id);
    toast.success(`${t('coupon.copy_ok')} ${code} 🎉`);
    setTimeout(() => setCopiedId(null), 2500);
  };

  const filtered = filter === 'all' ? coupons : coupons.filter(c => c.type === filter);

  const activeCount = coupons.filter(c => {
    const expired = c.expiredAt && new Date(c.expiredAt) < new Date();
    const full = c.usageLimit && c.usedCount >= c.usageLimit;
    return !expired && !full;
  }).length;

  if (loading) return <LoadingPage />;

  return (
    <div className="animate-fade-in">
      <style>{`
        @keyframes shimmer {
          0%   { background-position: -200% center; }
          100% { background-position:  200% center; }
        }
        @keyframes float {
          0%,100% { transform: translateY(0px) rotate(0deg); }
          50%      { transform: translateY(-8px) rotate(-5deg); }
        }
        @keyframes pulse-ring {
          0%   { transform: scale(0.9); opacity: 0.8; }
          100% { transform: scale(2);   opacity: 0; }
        }
        @keyframes card-in {
          from { opacity: 0; transform: translateY(24px) scale(0.96); }
          to   { opacity: 1; transform: translateY(0)    scale(1); }
        }
        @keyframes progress-fill {
          from { width: 0%; }
        }
        .coupon-card { animation: card-in 0.45s cubic-bezier(0.34,1.56,0.64,1) both; }
        .shimmer-code {
          background: linear-gradient(90deg, var(--text-primary), #8b5cf6, #06b6d4, var(--text-primary));
          background-size: 300% auto;
          -webkit-background-clip: text;
          -webkit-text-fill-color: transparent;
          background-clip: text;
          animation: shimmer 4s linear infinite;
        }
      `}</style>

      {/* ── Hero ── */}
      <div className="relative mb-8 rounded-3xl overflow-hidden p-7"
        style={{
          backgroundColor: 'var(--bg-card)',
          border: '1px solid var(--border)',
          boxShadow: '0 1px 3px rgba(0,0,0,0.05)',
        }}>
        <div style={{ position: 'absolute', inset: 0, pointerEvents: 'none', overflow: 'hidden', borderRadius: 24 }}>
          <div style={{
            position: 'absolute', top: -60, right: -60, width: 240, height: 240, borderRadius: '50%',
            background: 'radial-gradient(circle, rgba(139,92,246,0.1) 0%, transparent 65%)',
          }} />
          <div style={{
            position: 'absolute', bottom: -40, left: 60, width: 160, height: 160, borderRadius: '50%',
            background: 'radial-gradient(circle, rgba(6,182,212,0.08) 0%, transparent 65%)',
          }} />
        </div>

        <div className="relative flex items-center justify-between flex-wrap gap-5">
          <div className="flex items-center gap-4">
            <div style={{ fontSize: 40, animation: 'float 3.5s ease-in-out infinite', lineHeight: 1 }}>🎟️</div>
            <div>
              <h1 className="text-2xl font-black tracking-tight mb-1" style={{ color: 'var(--text-primary)' }}>
                {t('coupon.hero_title')}
              </h1>
              <p className="text-sm" style={{ color: 'var(--text-muted)' }}>
                {t('coupon.hero_desc')}
              </p>
              <div className="flex items-center gap-2 mt-2">
                <div className="relative w-2 h-2 shrink-0">
                  <div style={{ width: 8, height: 8, borderRadius: '50%', backgroundColor: '#10b981' }} />
                  <div style={{
                    position: 'absolute', inset: 0, borderRadius: '50%',
                    backgroundColor: '#10b981',
                    animation: 'pulse-ring 1.8s ease-out infinite',
                  }} />
                </div>
                <span className="text-xs font-semibold" style={{ color: '#10b981' }}>
                  {activeCount} {t('coupon.active_count')}
                </span>
                <span style={{ color: 'var(--border)' }}>·</span>
                <span className="text-xs" style={{ color: 'var(--text-muted)' }}>
                  {coupons.length} {t('coupon.total_count')}
                </span>
              </div>
            </div>
          </div>

          {/* Filter */}
          <div className="flex items-center gap-1 p-1 rounded-xl"
            style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)' }}>
            {[
              ['all',     t('coupon.filter_all')],
              ['Percent', t('coupon.filter_percent')],
              ['Fixed',   t('coupon.filter_fixed')],
            ].map(([val, label]) => (
              <button key={val} onClick={() => setFilter(val)}
                className="px-3 py-1.5 rounded-lg text-xs font-bold transition-all"
                style={{
                  backgroundColor: filter === val ? 'var(--sidebar-active-bg)' : 'transparent',
                  color: filter === val ? 'var(--sidebar-active-text)' : 'var(--text-secondary)',
                }}>
                {label}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* ── Grid ── */}
      {filtered.length === 0 ? (
        <div className="rounded-2xl p-16 text-center"
          style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>
          <div style={{ fontSize: 48, marginBottom: 12 }}>🎟️</div>
          <p className="font-bold text-lg mb-1" style={{ color: 'var(--text-primary)' }}>
            {t('coupon.empty_title')}
          </p>
          <p className="text-sm mb-6" style={{ color: 'var(--text-muted)' }}>
            {t('coupon.empty_desc')}
          </p>
          <Link to="/templates"
            className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-bold transition-all"
            style={{ backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)' }}>
            {t('coupon.explore_btn')}
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
          {filtered.map((coupon, i) => (
            <CouponCard
              key={coupon.id}
              coupon={coupon}
              copied={copiedId === coupon.id}
              onCopy={handleCopy}
              index={i}
            />
          ))}
        </div>
      )}
    </div>
  );
}

// ─────────────────────────────────────
const GRADIENTS = {
  Percent: { from: '#7c3aed', to: '#6366f1' },
  Fixed:   { from: '#0284c7', to: '#06b6d4' },
};

function CouponCard({ coupon, copied, onCopy, index }) {
  const { t } = useLang();
  const cardRef = useRef(null);

  const type        = coupon.type;
  const value       = coupon.value;
  const expiredAt   = coupon.expiredAt;
  const usageLimit  = coupon.usageLimit;
  const usedCount   = coupon.usedCount;

  const isExpired      = expiredAt && new Date(expiredAt) < new Date();
  const isFull         = usageLimit && usedCount >= usageLimit;
  const isUnavailable  = isExpired || isFull;
  const isExpiringSoon = !isExpired && expiredAt &&
    (new Date(expiredAt) - new Date() < 3 * 24 * 60 * 60 * 1000);

  const g = GRADIENTS[type] || GRADIENTS.Fixed;
  const gradient = `linear-gradient(135deg, ${g.from}, ${g.to})`;

  const discountLabel = type === 'Percent'
    ? `${value}% `
    : `${Number(value).toLocaleString('vi-VN')}₫ `;

  const remaining = usageLimit ? usageLimit - usedCount : null;
  const usagePct  = usageLimit ? Math.min(100, Math.round((usedCount / usageLimit) * 100)) : 0;

  const statusLabel = isExpired
    ? t('coupon.expired')
    : isFull
    ? t('coupon.full')
    : isExpiringSoon
    ? t('coupon.expiring_soon')
    : t('coupon.available');

  // 3D tilt
  const onMove = (e) => {
    if (!cardRef.current || isUnavailable) return;
    const r = cardRef.current.getBoundingClientRect();
    const x = (e.clientX - r.left) / r.width  - 0.5;
    const y = (e.clientY - r.top)  / r.height - 0.5;
    cardRef.current.style.transform =
      `perspective(700px) rotateY(${x * 9}deg) rotateX(${-y * 9}deg) translateY(-3px)`;
  };
  const onLeave = () => {
    if (cardRef.current)
      cardRef.current.style.transform =
        'perspective(700px) rotateY(0deg) rotateX(0deg) translateY(0)';
  };

  return (
    <div
      ref={cardRef}
      className="coupon-card"
      onMouseMove={onMove}
      onMouseLeave={onLeave}
      style={{
        animationDelay: `${index * 0.06}s`,
        borderRadius: 20,
        overflow: 'hidden',
        transition: 'transform 0.12s ease, box-shadow 0.25s ease',
        boxShadow: '0 4px 20px rgba(0,0,0,0.07)',
        backgroundColor: 'var(--bg-card)',
        border: '1px solid var(--border)',
        opacity: isUnavailable ? 0.6 : 1,
        filter: isUnavailable ? 'grayscale(30%)' : 'none',
      }}
    >
      {/* ── Gradient Header ── */}
      <div style={{
        background: isUnavailable ? 'var(--bg-elevated)' : gradient,
        padding: '20px 20px 18px',
        position: 'relative',
        overflow: 'hidden',
      }}>
        {!isUnavailable && <>
          <div style={{ position:'absolute',top:-16,right:-16,width:80,height:80,borderRadius:'50%',backgroundColor:'rgba(255,255,255,0.12)' }} />
          <div style={{ position:'absolute',bottom:-24,right:32,width:56,height:56,borderRadius:'50%',backgroundColor:'rgba(255,255,255,0.08)' }} />
        </>}

        <div className="relative">
          <p style={{
            fontSize: 10, fontWeight: 700, letterSpacing: '0.1em',
            textTransform: 'uppercase', marginBottom: 6,
            color: isUnavailable ? 'var(--text-muted)' : 'rgba(255,255,255,0.8)',
          }}>
            {type === 'Percent' ? t('coupon.type_percent') : t('coupon.type_fixed')}
          </p>

          <p style={{
            fontSize: 30, fontWeight: 900, lineHeight: 1, letterSpacing: '-0.02em',
            color: isUnavailable ? 'var(--text-secondary)' : '#ffffff',
          }}>
            {discountLabel}
          </p>

          {coupon.maxDiscountAmount > 0 && type === 'Percent' && (
            <p style={{
              fontSize: 11, fontWeight: 600, marginTop: 5,
              color: isUnavailable ? 'var(--text-muted)' : 'rgba(255,255,255,0.75)',
            }}>
              {t('coupon.max_discount')} {Number(coupon.maxDiscountAmount).toLocaleString('vi-VN')}₫
            </p>
          )}

          <div style={{
            position: 'absolute', top: 0, right: 0,
            padding: '4px 10px', borderRadius: 999,
            fontSize: 10, fontWeight: 800, letterSpacing: '0.04em',
            textTransform: 'uppercase',
            backgroundColor: isUnavailable
              ? 'var(--bg-elevated)'
              : isExpiringSoon
              ? 'rgba(245,158,11,0.85)'
              : 'rgba(255,255,255,0.22)',
            color: isUnavailable ? 'var(--text-muted)' : '#ffffff',
            backdropFilter: 'blur(6px)',
            border: isUnavailable ? '1px solid var(--border)' : 'none',
          }}>
            {statusLabel}
          </div>
        </div>
      </div>

      {/* ── Ticket Tear ── */}
      <div style={{ position: 'relative', height: 1, margin: '0' }}>
        <div style={{
          width: 16, height: 16, borderRadius: '50%',
          backgroundColor: 'var(--bg-page)',
          border: '1px solid var(--border)',
          position: 'absolute', left: -8, top: '50%', transform: 'translateY(-50%)', zIndex: 2,
        }} />
        <div style={{
          width: 16, height: 16, borderRadius: '50%',
          backgroundColor: 'var(--bg-page)',
          border: '1px solid var(--border)',
          position: 'absolute', right: -8, top: '50%', transform: 'translateY(-50%)', zIndex: 2,
        }} />
        <div style={{
          position: 'absolute', left: 12, right: 12, top: '50%',
          borderTop: '1.5px dashed var(--border)',
        }} />
      </div>

      {/* ── Body ── */}
      <div style={{ padding: '16px 20px 20px' }}>
        {coupon.description && (
          <p className="text-sm mb-3 leading-relaxed" style={{ color: 'var(--text-secondary)' }}>
            {coupon.description}
          </p>
        )}

        <div style={{ display: 'flex', flexDirection: 'column', gap: 6, marginBottom: 14 }}>
          {coupon.minOrderAmount > 0 && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 12, color: 'var(--text-muted)' }}>
              <span>📦</span>
              <span>{t('coupon.min_order')}{' '}
                <span style={{ fontWeight: 700, color: 'var(--text-secondary)' }}>
                  {Number(coupon.minOrderAmount).toLocaleString('vi-VN')}₫
                </span>
              </span>
            </div>
          )}
          {coupon.startAt && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 12, color: 'var(--text-muted)' }}>
              <span>🟢</span>
              <span>{t('coupon.valid_from')}{' '}
                <span style={{ fontWeight: 700, color: 'var(--text-secondary)' }}>
                  {new Date(coupon.startAt).toLocaleDateString('vi-VN')}
                </span>
              </span>
            </div>
          )}
          {expiredAt && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 12, color: 'var(--text-muted)' }}>
              <span>📅</span>
              <span>{t('coupon.expires')}{' '}
                <span style={{
                  fontWeight: 700,
                  color: isExpired ? '#ef4444' : isExpiringSoon ? '#f59e0b' : 'var(--text-secondary)',
                }}>
                  {new Date(expiredAt).toLocaleDateString('vi-VN')}
                </span>
              </span>
            </div>
          )}
        </div>

        {/* Usage bar */}
        {usageLimit > 0 && (
          <div style={{ marginBottom: 16 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6, fontSize: 11 }}>
              <span style={{ color: 'var(--text-muted)' }}>
                {t('coupon.used_count')} {usedCount}/{usageLimit} {t('coupon.lượt')}
              </span>
              <span style={{
                fontWeight: 700,
                color: isFull ? '#ef4444' : usagePct > 80 ? '#f59e0b' : 'var(--text-muted)',
              }}>
                {isFull ? t('coupon.fully_used') : `${t('coupon.remaining')} ${remaining} ${t('coupon.lượt')}`}
              </span>
            </div>
            <div style={{
              height: 6, borderRadius: 999, overflow: 'hidden',
              backgroundColor: 'var(--bg-elevated)',
              border: '1px solid var(--border)',
            }}>
              <div style={{
                height: '100%', borderRadius: 999,
                width: `${usagePct}%`,
                background: isFull ? 'var(--text-muted)'
                  : usagePct > 80 ? 'linear-gradient(90deg,#ef4444,#f97316)'
                  : gradient,
                animation: 'progress-fill 1.2s cubic-bezier(0.4,0,0.2,1)',
                transition: 'width 0.8s ease',
              }} />
            </div>
          </div>
        )}

        {/* Code row */}
        <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
          <div
            className={!isUnavailable ? 'shimmer-code' : ''}
            style={{
              flex: 1,
              padding: '10px 12px',
              borderRadius: 12,
              fontFamily: 'monospace',
              fontSize: 15,
              fontWeight: 900,
              textAlign: 'center',
              letterSpacing: '0.2em',
              userSelect: 'all',
              backgroundColor: 'var(--bg-elevated)',
              border: '2px dashed var(--border)',
              color: isUnavailable ? 'var(--text-muted)' : undefined,
            }}
          >
            {coupon.code}
          </div>

          <button
            onClick={() => !isUnavailable && onCopy(coupon.code, coupon.id)}
            disabled={isUnavailable}
            style={{
              flexShrink: 0,
              minWidth: 80,
              padding: '10px 14px',
              borderRadius: 12,
              fontSize: 12,
              fontWeight: 800,
              border: 'none',
              cursor: isUnavailable ? 'not-allowed' : 'pointer',
              background: copied
                ? 'linear-gradient(135deg,#10b981,#059669)'
                : isUnavailable
                ? 'var(--bg-elevated)'
                : gradient,
              color: isUnavailable ? 'var(--text-muted)' : '#ffffff',
              boxShadow: copied || isUnavailable ? 'none' : `0 4px 14px ${g.from}55`,
              transform: copied ? 'scale(0.94)' : 'scale(1)',
              transition: 'all 0.22s cubic-bezier(0.34,1.56,0.64,1)',
            }}
          >
            {copied ? `✓ ${t('common.copied')}` : isUnavailable ? t('coupon.fully_used') : t('aff.copy_btn')}
          </button>
        </div>

        {/* CTA */}
        {!isUnavailable && (
          <Link
            to="/templates"
            style={{
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              gap: 6, marginTop: 10, padding: '8px 12px', borderRadius: 12,
              fontSize: 12, fontWeight: 600, textDecoration: 'none',
              color: 'var(--text-muted)',
              border: '1px solid var(--border)',
              transition: 'all 0.2s',
            }}
            onMouseEnter={e => {
              e.currentTarget.style.color = 'var(--text-primary)';
              e.currentTarget.style.borderColor = 'var(--text-secondary)';
              e.currentTarget.style.backgroundColor = 'var(--bg-elevated)';
            }}
            onMouseLeave={e => {
              e.currentTarget.style.color = 'var(--text-muted)';
              e.currentTarget.style.borderColor = 'var(--border)';
              e.currentTarget.style.backgroundColor = 'transparent';
            }}
          >
            {t('coupon.use_now')} <span style={{ fontSize: 14 }}>→</span>
          </Link>
        )}
      </div>
    </div>
  );
}