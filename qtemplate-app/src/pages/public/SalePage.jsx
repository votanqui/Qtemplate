import { useState, useEffect, useCallback, useRef } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { templateApi, publicApi, userApi } from '../../api/services';
import { extractError, toAbsoluteUrl } from '../../api/client';
import { LoadingPage, Pagination, Price, useToast } from '../../components/ui';
import { useAuth } from '../../context/AuthContext';
import { useLang } from '../../context/Langcontext';

function calcLeft(endAt) {
  const diff = new Date(endAt) - Date.now();
  if (diff <= 0) return { expired: true, d: 0, h: 0, m: 0, s: 0 };
  return {
    expired: false,
    d: Math.floor(diff / 86400000),
    h: Math.floor((diff % 86400000) / 3600000),
    m: Math.floor((diff % 3600000) / 60000),
    s: Math.floor((diff % 60000) / 1000),
  };
}

function useCountdown(endAt) {
  const [left, setLeft] = useState(() => endAt ? calcLeft(endAt) : null);
  useEffect(() => {
    if (!endAt) return;
    setLeft(calcLeft(endAt));
    const id = setInterval(() => setLeft(calcLeft(endAt)), 1000);
    return () => clearInterval(id);
  }, [endAt]);
  return left;
}

function CountdownBlock({ value, label, urgent }) {
  const pad = n => String(n).padStart(2, '0');
  return (
    <div className="flex flex-col items-center gap-0.5">
      <div
        className="w-8 h-8 rounded-md flex items-center justify-center font-black tabular-nums text-xs leading-none"
        style={{
          background: urgent
            ? 'linear-gradient(135deg, #ff3b30, #ff6b35)'
            : 'linear-gradient(135deg, #7c3aed, #0ea5e9)',
          color: '#ffffff',
          boxShadow: urgent ? '0 2px 6px rgba(255,59,48,0.5)' : '0 2px 6px rgba(124,58,237,0.4)',
        }}
      >
        {pad(value)}
      </div>
      <span className="text-[8px] font-bold uppercase tracking-wider" style={{ color: 'var(--text-muted)' }}>
        {label}
      </span>
    </div>
  );
}

function Countdown({ endAt, isOpenEnded, t }) {
  const left = useCountdown(endAt);
  if (isOpenEnded) return (
    <div className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg"
      style={{ background: 'rgba(16,185,129,0.12)', border: '1.5px solid rgba(16,185,129,0.3)', color: '#10b981' }}>
      <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse" />
      <span className="text-[11px] font-bold">{t('sale.open_ended')}</span>
    </div>
  );
  if (!left || left.expired) return (
    <div className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg"
      style={{ background: 'rgba(239,68,68,0.1)', border: '1.5px solid rgba(239,68,68,0.25)', color: '#ef4444' }}>
      <span className="text-[11px] font-bold">⛔ {t('sale.expired')}</span>
    </div>
  );
  const urgent = left.d === 0 && left.h < 1;
  const sep = <span className="text-sm font-black mb-3 leading-none" style={{ color: urgent ? '#ff3b30' : 'var(--text-muted)' }}>:</span>;
  return (
    <div className="flex items-end gap-0.5">
      {left.d > 0 && <><CountdownBlock value={left.d} label={t('sale.unit_day')} urgent={urgent} />{sep}</>}
      <CountdownBlock value={left.h}   label={t('sale.unit_hour')} urgent={urgent} />
      {sep}
      <CountdownBlock value={left.m}   label={t('sale.unit_min')}  urgent={urgent} />
      {sep}
      <CountdownBlock value={left.s}   label={t('sale.unit_sec')}  urgent={urgent} />
    </div>
  );
}

function SaleCard({ tpl, onWishlist, wishlistLoading, isAuth, t }) {
  const urgent = !tpl.isOpenEnded && tpl.saleEndAt && (() => {
    const l = calcLeft(tpl.saleEndAt);
    return !l.expired && l.d === 0 && l.h < 1;
  })();

  return (
    <Link to={`/templates/${tpl.slug}`} className="group block">
      <div
        className="rounded-xl overflow-hidden flex flex-col transition-all duration-300 hover:-translate-y-0.5"
        style={{ backgroundColor: 'var(--bg-card)', border: '1.5px solid var(--border)' }}
        onMouseEnter={e => {
          e.currentTarget.style.borderColor = urgent ? '#ff3b30' : '#7c3aed';
          e.currentTarget.style.boxShadow = urgent
            ? '0 8px 24px rgba(255,59,48,0.15)'
            : '0 8px 24px rgba(124,58,237,0.12)';
        }}
        onMouseLeave={e => {
          e.currentTarget.style.borderColor = 'var(--border)';
          e.currentTarget.style.boxShadow = 'none';
        }}
      >
        {/* Thumbnail — compact ratio */}
        <div className="relative overflow-hidden flex-shrink-0" style={{ aspectRatio: '16/8', backgroundColor: 'var(--bg-elevated)' }}>
          {tpl.thumbnailUrl ? (
            <img src={toAbsoluteUrl(tpl.thumbnailUrl)} alt={tpl.name}
              className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" />
          ) : (
            <div className="w-full h-full flex items-center justify-center text-4xl"
              style={{ background: 'linear-gradient(135deg, rgba(255,59,48,0.05), rgba(124,58,237,0.05))' }}>🔥</div>
          )}

          {/* Discount badge */}
          <div className="absolute top-0 left-0">
            <div className="flex flex-col items-center justify-center w-13 h-13 font-black text-white"
              style={{
                width: 52, height: 52,
                background: urgent
                  ? 'linear-gradient(135deg, #ff3b30, #ff6b35)'
                  : 'linear-gradient(135deg, #7c3aed, #0ea5e9)',
                clipPath: 'polygon(0 0, 100% 0, 100% 76%, 50% 100%, 0 76%)',
                filter: 'drop-shadow(0 2px 5px rgba(0,0,0,0.28))',
              }}>
              <span className="text-[8px] font-semibold mt-1 opacity-80">{t('sale.discount')}</span>
              <span className="text-[15px] font-black leading-tight">-{tpl.discountPercent}%</span>
            </div>
          </div>

          {/* Wishlist */}
          {isAuth && (
            <button
              onClick={e => { e.preventDefault(); onWishlist(tpl.id, e); }}
              disabled={wishlistLoading[tpl.id]}
              className="absolute top-2 right-2 w-7 h-7 rounded-full flex items-center justify-center text-xs hover:scale-110 transition-transform"
              style={{ background: 'rgba(255,255,255,0.92)', backdropFilter: 'blur(8px)', boxShadow: '0 2px 6px rgba(0,0,0,0.15)' }}>
              {wishlistLoading[tpl.id] ? <span className="animate-spin">⟳</span> : (tpl.isInWishlist ? '❤️' : '🤍')}
            </button>
          )}

          {/* Badges */}
          <div className="absolute top-2 flex gap-1" style={{ left: '3.6rem' }}>
            {tpl.isFeatured && <span className="px-1.5 py-0.5 rounded-full bg-amber-400 text-black text-[9px] font-black shadow">⭐ TOP</span>}
            {tpl.isNew      && <span className="px-1.5 py-0.5 rounded-full bg-emerald-500 text-white text-[9px] font-black shadow">NEW</span>}
          </div>
        </div>

        {/* Body */}
        <div className="p-3 flex flex-col gap-2">

          {/* Category + name */}
          <div>
            <p className="text-[9px] font-bold uppercase tracking-widest mb-0.5"
              style={{ color: urgent ? '#ff3b30' : '#7c3aed' }}>
              {tpl.categoryName}
            </p>
            <h3 className="font-bold text-sm leading-snug line-clamp-1" style={{ color: 'var(--text-primary)' }}>
              {tpl.name}
            </h3>
          </div>

          {/* Price row */}
          <div className="flex items-center justify-between">
            <div className="flex items-baseline gap-1.5">
              <span className="text-base font-black" style={{ color: urgent ? '#ff3b30' : '#7c3aed' }}>
                <Price amount={tpl.salePrice} />
              </span>
              <span className="text-xs line-through" style={{ color: 'var(--text-muted)' }}>
                <Price amount={tpl.originalPrice} />
              </span>
            </div>
            <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[10px] font-bold"
              style={{ background: 'rgba(16,185,129,0.1)', border: '1px solid rgba(16,185,129,0.2)', color: '#10b981' }}>
              {t('sale.save')} <Price amount={tpl.saveAmount} style={{ color: '#10b981', display: 'inline' }} />
            </span>
          </div>

          {/* Countdown */}
          <div className="pt-2" style={{ borderTop: '1px solid var(--border)' }}>
            <p className="text-[9px] font-bold uppercase tracking-wider mb-1.5"
              style={{ color: urgent ? '#ff3b30' : 'var(--text-muted)' }}>
              {tpl.isOpenEnded ? `🟢 ${t('sale.on_sale')}` : urgent ? `🔴 ${t('sale.ending_soon')}` : `⏱ ${t('sale.time_left')}`}
            </p>
            <Countdown endAt={tpl.saleEndAt} isOpenEnded={tpl.isOpenEnded} t={t} />
          </div>

          {/* Rating */}
          <div className="flex items-center gap-2 text-[11px]" style={{ color: 'var(--text-muted)' }}>
            <span className="flex items-center gap-0.5">
              <span className="text-amber-400">★</span>
              <span className="font-semibold" style={{ color: 'var(--text-secondary)' }}>
                {tpl.averageRating > 0 ? tpl.averageRating.toFixed(1) : '—'}
              </span>
            </span>
            <span className="w-1 h-1 rounded-full" style={{ backgroundColor: 'var(--border)' }} />
            <span>{tpl.salesCount} {t('tpl.sales')}</span>
            {tpl.tags?.[0] && (
              <span className="ml-auto px-1.5 py-0.5 rounded text-[9px] font-medium"
                style={{ backgroundColor: 'var(--bg-elevated)', color: 'var(--text-muted)' }}>
                #{tpl.tags[0]}
              </span>
            )}
          </div>
        </div>
      </div>
    </Link>
  );
}

export default function SalePage() {
  const { t } = useLang();
  const { isAuth } = useAuth();
  const toast = useToast();
  const [params, setParams] = useSearchParams();
  const [templates, setTemplates] = useState([]);
  const [pagination, setPagination] = useState(null);
  const [loading, setLoading] = useState(true);
  const [categories, setCategories] = useState([]);
  const [wishlistLoading, setWishlistLoading] = useState({});
  const searchRef = useRef(null);

  const filters = {
    search:       params.get('search')       || '',
    categorySlug: params.get('categorySlug') || '',
    page:         parseInt(params.get('page') || '1'),
  };

  const setFilter = (key, val) => {
    const p = new URLSearchParams(params);
    if (val) p.set(key, val); else p.delete(key);
    p.set('page', '1');
    setParams(p);
  };

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const q = { page: filters.page, pageSize: 12 };
      if (filters.search)       q.search       = filters.search;
      if (filters.categorySlug) q.categorySlug = filters.categorySlug;
      const res = await templateApi.getOnSaleList(q);
      setTemplates(res.data.data.items || []);
      setPagination(res.data.data);
    } catch (err) {
      toast.error(extractError(err), t('sale.load_err'));
    } finally {
      setLoading(false);
    }
  }, [params.toString()]);

  useEffect(() => { fetchData(); }, [fetchData]);
  useEffect(() => {
    publicApi.getCategories().then(r => setCategories(r.data.data || [])).catch(() => {});
  }, []);

  const handleToggleWishlist = async (templateId, e) => {
    e.preventDefault();
    if (!isAuth) return;
    setWishlistLoading(w => ({ ...w, [templateId]: true }));
    try {
      const res = await userApi.toggleWishlist(templateId);
      const inWishlist = res.data.data;
      setTemplates(ts => ts.map(item => item.id === templateId ? { ...item, isInWishlist: inWishlist } : item));
      toast.success(inWishlist ? t('tpl.wish_add_ok') : t('tpl.wish_remove_ok'));
    } catch (err) {
      toast.error(extractError(err));
    } finally {
      setWishlistLoading(w => ({ ...w, [templateId]: false }));
    }
  };

  const totalCount = pagination?.totalCount || 0;
  const inputStyle = { backgroundColor: 'var(--bg-elevated)', border: '1.5px solid var(--border)', color: 'var(--text-primary)' };

  return (
    <div className="animate-fade-in">

      {/* Hero */}
      <div className="relative rounded-3xl mb-6 overflow-hidden"
        style={{ backgroundColor: 'var(--bg-card)', border: '1.5px solid var(--border)' }}>
        <div className="absolute inset-0 pointer-events-none"
          style={{ background: 'linear-gradient(135deg, rgba(255,59,48,0.07) 0%, rgba(255,107,53,0.05) 40%, rgba(124,58,237,0.05) 100%)' }} />
        <div className="absolute -top-10 -right-10 w-48 h-48 rounded-full opacity-15 pointer-events-none"
          style={{ background: 'radial-gradient(circle, #ff3b30, transparent 70%)' }} />

        <div className="relative p-6 md:p-8">
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div>
              <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full text-xs font-black uppercase tracking-widest mb-3"
                style={{ background: 'rgba(255,59,48,0.12)', border: '1.5px solid rgba(255,59,48,0.3)', color: '#ff3b30' }}>
                <span className="w-2 h-2 rounded-full bg-red-500 animate-pulse" />
                🔥 Flash Sale
              </div>
              <h1 className="text-2xl md:text-3xl font-black tracking-tight mb-1.5" style={{ color: 'var(--text-primary)' }}>
                {t('sale.title')}
              </h1>
              <p className="text-sm" style={{ color: 'var(--text-muted)' }}>
                {t('sale.subtitle')}{' '}
                <span className="font-black px-2 py-0.5 rounded-lg"
                  style={{ background: 'linear-gradient(135deg, rgba(255,59,48,0.1), rgba(124,58,237,0.1))', color: 'var(--text-primary)' }}>
                  {totalCount}
                </span>{' '}
                {t('sale.count_suffix')}
              </p>
            </div>

            <div className="flex flex-wrap gap-2">
              {[
                { icon: '🔥', label: t('sale.on_sale'),       value: `${totalCount} ${t('sale.stat_products')}` },
                { icon: '💸', label: t('sale.stat_save_to'),  value: '70%' },
              ].map(item => (
                <div key={item.label} className="flex items-center gap-2.5 px-4 py-2.5 rounded-2xl"
                  style={{ backgroundColor: 'var(--bg-elevated)', border: '1.5px solid var(--border)' }}>
                  <span className="text-xl">{item.icon}</span>
                  <div>
                    <p className="text-[10px] font-semibold uppercase tracking-wide" style={{ color: 'var(--text-muted)' }}>{item.label}</p>
                    <p className="text-sm font-black" style={{ color: 'var(--text-primary)' }}>{item.value}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="rounded-2xl p-3.5 mb-5"
        style={{ backgroundColor: 'var(--bg-card)', border: '1.5px solid var(--border)' }}>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <div className="relative">
            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm" style={{ color: 'var(--text-muted)' }}>🔍</span>
            <input
              ref={searchRef}
              className="w-full pl-9 pr-4 py-2.5 rounded-xl text-sm focus:outline-none transition-all"
              style={inputStyle}
              placeholder={t('sale.search_ph')}
              defaultValue={filters.search}
              onKeyDown={e => e.key === 'Enter' && setFilter('search', e.target.value)}
              onChange={e => !e.target.value && setFilter('search', '')}
              onFocus={e => { e.target.style.borderColor = '#ff3b30'; e.target.style.boxShadow = '0 0 0 3px rgba(255,59,48,0.08)'; }}
              onBlur={e => { e.target.style.borderColor = 'var(--border)'; e.target.style.boxShadow = 'none'; }}
            />
          </div>
          <select className="w-full px-4 py-2.5 rounded-xl text-sm focus:outline-none" style={inputStyle}
            value={filters.categorySlug} onChange={e => setFilter('categorySlug', e.target.value)}>
            <option value="">{t('tpl.all_categories')}</option>
            {categories.map(cat => (
              <optgroup key={cat.id} label={cat.name}>
                {cat.children?.map(child => <option key={child.id} value={child.slug}>{child.name}</option>)}
                {!cat.children?.length && <option value={cat.slug}>{cat.name}</option>}
              </optgroup>
            ))}
          </select>
        </div>
      </div>

      {/* Results */}
      {loading ? <LoadingPage /> : templates.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <div className="w-16 h-16 rounded-2xl flex items-center justify-center text-3xl mb-4"
            style={{ background: 'linear-gradient(135deg, rgba(255,59,48,0.08), rgba(124,58,237,0.08))', border: '1.5px solid var(--border)' }}>
            🔥
          </div>
          <p className="font-black text-lg mb-2" style={{ color: 'var(--text-primary)' }}>{t('sale.empty_title')}</p>
          <p className="text-sm mb-5" style={{ color: 'var(--text-muted)' }}>{t('sale.empty_desc')}</p>
          <Link to="/templates"
            className="px-5 py-2.5 rounded-xl text-sm font-bold transition-all hover:-translate-y-0.5"
            style={{ background: 'linear-gradient(135deg, #ff3b30, #7c3aed)', color: '#ffffff', boxShadow: '0 4px 14px rgba(255,59,48,0.3)' }}>
            {t('sale.explore_btn')} →
          </Link>
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            {templates.map(tpl => (
              <SaleCard key={tpl.id} tpl={tpl} onWishlist={handleToggleWishlist}
                wishlistLoading={wishlistLoading} isAuth={isAuth} t={t} />
            ))}
          </div>
          <Pagination page={filters.page} totalPages={pagination?.totalPages || 1}
            onPageChange={p => setFilter('page', p.toString())} />
        </>
      )}
    </div>
  );
}