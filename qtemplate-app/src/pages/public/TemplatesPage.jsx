import { useState, useEffect, useCallback, useRef } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { templateApi, publicApi, userApi } from '../../api/services';
import { extractError, toAbsoluteUrl } from '../../api/client';
import { LoadingPage, Pagination, Price, useToast } from '../../components/ui';
import { useAuth } from '../../context/AuthContext';
import { useLang } from '../../context/Langcontext';

export default function TemplatesPage() {
  const { t } = useLang();
  const { isAuth } = useAuth();
  const toast = useToast();
  const [params, setParams] = useSearchParams();

  const [templates, setTemplates] = useState([]);
  const [pagination, setPagination] = useState(null);
  const [loading, setLoading] = useState(true);
  const [categories, setCategories] = useState([]);
  const [tags, setTags] = useState([]);
  const [wishlistLoading, setWishlistLoading] = useState({});
  const [showAdvanced, setShowAdvanced] = useState(false);
  const searchRef = useRef(null);

  // ── Đọc tất cả filters từ URL params ──────────────────────────────────────
  const filters = {
    search:       params.get('search')       || '',
    categorySlug: params.get('categorySlug') || '',
    tagSlug:      params.get('tagSlug')      || '',
    isFree:       params.get('isFree')       || '',
    sortBy:       params.get('sortBy')       || 'newest',
    page:         parseInt(params.get('page') || '1'),
    // Advanced
    onSale:       params.get('onSale')       || '',
    isFeatured:   params.get('isFeatured')   || '',
    isNew:        params.get('isNew')        || '',
    techStack:    params.get('techStack')    || '',
    minPrice:     params.get('minPrice')     || '',
    maxPrice:     params.get('maxPrice')     || '',
  };

  // Tự mở advanced nếu đang có filter nâng cao
  useEffect(() => {
    if (filters.onSale || filters.isFeatured || filters.isNew || filters.techStack || filters.minPrice || filters.maxPrice) {
      setShowAdvanced(true);
    }
  }, []);

  const setFilter = (key, val) => {
    const p = new URLSearchParams(params);
    if (val) p.set(key, val); else p.delete(key);
    p.set('page', '1');
    setParams(p);
  };

  const clearAll = () => {
    setParams(new URLSearchParams());
    if (searchRef.current) searchRef.current.value = '';
  };

  const hasActiveFilters = Object.entries(filters)
    .filter(([k]) => k !== 'page' && k !== 'sortBy')
    .some(([, v]) => !!v);

  // ── Fetch templates ────────────────────────────────────────────────────────
  const fetchTemplates = useCallback(async () => {
    setLoading(true);
    try {
      const q = { sortBy: filters.sortBy, page: filters.page, pageSize: 12 };
      if (filters.search)       q.search       = filters.search;
      if (filters.categorySlug) q.categorySlug = filters.categorySlug;
      if (filters.tagSlug)      q.tagSlug      = filters.tagSlug;
      if (filters.isFree !== '') q.isFree       = filters.isFree;
      if (filters.onSale)       q.onSale       = filters.onSale;
      if (filters.isFeatured)   q.isFeatured   = filters.isFeatured;
      if (filters.isNew)        q.isNew        = filters.isNew;
      if (filters.techStack)    q.techStack    = filters.techStack;
      if (filters.minPrice)     q.minPrice     = filters.minPrice;
      if (filters.maxPrice)     q.maxPrice     = filters.maxPrice;

      const res = await templateApi.getList(q);
      setTemplates(res.data.data.items);
      setPagination(res.data.data);
    } catch (err) {
      toast.error(extractError(err), t('tpl.load_err'));
    } finally {
      setLoading(false);
    }
  }, [params.toString()]);

  useEffect(() => { fetchTemplates(); }, [fetchTemplates]);

  useEffect(() => {
    Promise.all([publicApi.getCategories(), publicApi.getTags()])
      .then(([catRes, tagRes]) => {
        setCategories(catRes.data.data || []);
        setTags(tagRes.data.data || []);
      }).catch(() => {});
  }, []);

  // ── Wishlist toggle ────────────────────────────────────────────────────────
  const handleToggleWishlist = async (templateId, e) => {
    e.preventDefault();
    if (!isAuth) return;
    setWishlistLoading(w => ({ ...w, [templateId]: true }));
    try {
      const res = await userApi.toggleWishlist(templateId);
      const inWishlist = res.data.data;
      setTemplates(ts => ts.map(tpl =>
        tpl.id === templateId ? { ...tpl, isInWishlist: inWishlist } : tpl
      ));
      toast.success(inWishlist ? t('tpl.wish_add_ok') : t('tpl.wish_remove_ok'));
    } catch (err) {
      toast.error(extractError(err));
    } finally {
      setWishlistLoading(w => ({ ...w, [templateId]: false }));
    }
  };

  const selectStyle = {
    backgroundColor: 'var(--bg-elevated)',
    border: '1px solid var(--border)',
    color: 'var(--text-primary)',
  };

  const inputStyle = selectStyle;

  // ── Active filter chips ────────────────────────────────────────────────────
  const activeChips = [
    filters.search       && { key: 'search',       label: `"${filters.search}"` },
    filters.categorySlug && { key: 'categorySlug', label: `📂 ${filters.categorySlug}` },
    filters.tagSlug      && { key: 'tagSlug',      label: `#${filters.tagSlug}` },
    filters.techStack    && { key: 'techStack',    label: `⚡ ${filters.techStack}` },
    filters.isFree === 'true'     && { key: 'isFree',     label: t('tpl.free_filter') },
    filters.onSale === 'true'     && { key: 'onSale',     label: '🔥 Sale' },
    filters.isFeatured === 'true' && { key: 'isFeatured', label: '⭐ Featured' },
    filters.isNew === 'true'      && { key: 'isNew',      label: '🆕 Mới nhất' },
    filters.minPrice && { key: 'minPrice', label: `≥ ${Number(filters.minPrice).toLocaleString()}đ` },
    filters.maxPrice && { key: 'maxPrice', label: `≤ ${Number(filters.maxPrice).toLocaleString()}đ` },
  ].filter(Boolean);

  const accentColors = [
    'from-violet-100 to-violet-50', 'from-emerald-100 to-emerald-50',
    'from-amber-100 to-amber-50',   'from-pink-100 to-pink-50',
    'from-sky-100 to-sky-50',
  ];

  return (
    <div className="animate-fade-in">

      {/* Header */}
      <div className="mb-8">
        <div className="flex items-end justify-between">
          <div>
            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-semibold uppercase tracking-wider mb-3"
              style={{ backgroundColor: 'rgba(139,92,246,0.08)', border: '1px solid rgba(139,92,246,0.2)', color: '#7c3aed' }}>
              <span className="w-1.5 h-1.5 rounded-full bg-violet-500 animate-pulse" />
              Marketplace
            </div>
            <h1 className="text-3xl font-black tracking-tight" style={{ color: 'var(--text-primary)' }}>
              {t('tpl.title')}
            </h1>
            <p className="text-sm mt-1" style={{ color: 'var(--text-muted)' }}>
              {t('tpl.explore_title')}{' '}
              <span className="font-bold" style={{ color: 'var(--text-secondary)' }}>
                {pagination?.totalCount || 0}
              </span>{' '}
              {t('tpl.explore_subtitle')}
            </p>
          </div>
          {/* Link sang trang Săn Sale */}
          <Link
            to="/sale"
            className="hidden md:flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-bold transition-all hover:-translate-y-0.5"
            style={{ background: 'linear-gradient(135deg, #ef4444, #f97316)', color: 'white' }}
          >
            🔥 {t('sale.nav_label')}
          </Link>
        </div>
      </div>

      {/* Filter box */}
      <div className="rounded-2xl p-4 mb-4 shadow-sm"
        style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>

        {/* Row 1: Search + Category + Price type + Sort */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3">
          {/* Search */}
          <div className="relative">
            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-sm" style={{ color: 'var(--text-muted)' }}>🔍</span>
            <input
              ref={searchRef}
              className="w-full pl-9 pr-4 py-2.5 rounded-xl text-sm focus:outline-none transition-all"
              style={inputStyle}
              placeholder={t('tpl.search_ph')}
              defaultValue={filters.search}
              onKeyDown={e => e.key === 'Enter' && setFilter('search', e.target.value)}
              onChange={e => !e.target.value && setFilter('search', '')}
              onFocus={e => e.target.style.borderColor = '#0ea5e9'}
              onBlur={e => e.target.style.borderColor = 'var(--border)'}
            />
          </div>

          {/* Category */}
          <select className="w-full px-4 py-2.5 rounded-xl text-sm focus:outline-none" style={selectStyle}
            value={filters.categorySlug} onChange={e => setFilter('categorySlug', e.target.value)}>
            <option value="">{t('tpl.all_categories')}</option>
            {categories.map(cat => (
              <optgroup key={cat.id} label={cat.name}>
                {cat.children?.map(child => (
                  <option key={child.id} value={child.slug}>{child.name}</option>
                ))}
                {!cat.children?.length && <option value={cat.slug}>{cat.name}</option>}
              </optgroup>
            ))}
          </select>

          {/* Price type */}
          <select className="w-full px-4 py-2.5 rounded-xl text-sm focus:outline-none" style={selectStyle}
            value={filters.isFree} onChange={e => setFilter('isFree', e.target.value)}>
            <option value="">{t('tpl.all_prices')}</option>
            <option value="true">{t('tpl.free_filter')}</option>
            <option value="false">{t('tpl.paid_filter')}</option>
          </select>

          {/* Sort */}
          <select className="w-full px-4 py-2.5 rounded-xl text-sm focus:outline-none" style={selectStyle}
            value={filters.sortBy} onChange={e => setFilter('sortBy', e.target.value)}>
            <option value="newest">🕐 {t('tpl.sort_newest')}</option>
            <option value="popular">🔥 {t('tpl.sort_popular')}</option>
            <option value="rating">⭐ {t('tpl.sort_rating')}</option>
            <option value="price-asc">↑ {t('tpl.sort_price_asc')}</option>
            <option value="price-desc">↓ {t('tpl.sort_price_desc')}</option>
            <option value="discount">💸 {t('tpl.sort_discount')}</option>
          </select>
        </div>

        {/* Advanced toggle */}
        <div className="flex items-center justify-between mt-3 pt-3" style={{ borderTop: '1px solid var(--border)' }}>
          <button
            onClick={() => setShowAdvanced(v => !v)}
            className="inline-flex items-center gap-1.5 text-xs font-semibold transition-colors"
            style={{ color: showAdvanced ? '#7c3aed' : 'var(--text-muted)' }}
          >
            <span className={`transition-transform ${showAdvanced ? 'rotate-180' : ''}`}>▼</span>
            {t('tpl.advanced_filter')}
            {(filters.onSale || filters.isFeatured || filters.isNew || filters.techStack || filters.minPrice || filters.maxPrice) && (
              <span className="w-2 h-2 rounded-full bg-violet-500" />
            )}
          </button>

          {hasActiveFilters && (
            <button onClick={clearAll}
              className="text-xs font-semibold px-3 py-1 rounded-lg transition-all"
              style={{ backgroundColor: 'rgba(239,68,68,0.08)', color: '#ef4444', border: '1px solid rgba(239,68,68,0.2)' }}>
              ✕ {t('tpl.clear_filters')}
            </button>
          )}
        </div>

        {/* Advanced panel */}
        {showAdvanced && (
          <div className="mt-3 pt-3 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3"
            style={{ borderTop: '1px solid var(--border)' }}>

            {/* TechStack search */}
            <div className="relative">
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-xs" style={{ color: 'var(--text-muted)' }}>⚡</span>
              <input
                className="w-full pl-8 pr-4 py-2.5 rounded-xl text-sm focus:outline-none"
                style={inputStyle}
                placeholder={t('tpl.techstack_ph')}
                defaultValue={filters.techStack}
                onKeyDown={e => e.key === 'Enter' && setFilter('techStack', e.target.value)}
                onChange={e => !e.target.value && setFilter('techStack', '')}
                onFocus={e => e.target.style.borderColor = '#0ea5e9'}
                onBlur={e => e.target.style.borderColor = 'var(--border)'}
              />
            </div>

            {/* Min price */}
            <div className="relative">
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-xs" style={{ color: 'var(--text-muted)' }}>₫≥</span>
              <input
                type="number" min="0"
                className="w-full pl-9 pr-4 py-2.5 rounded-xl text-sm focus:outline-none"
                style={inputStyle}
                placeholder={t('tpl.min_price_ph')}
                defaultValue={filters.minPrice}
                onKeyDown={e => e.key === 'Enter' && setFilter('minPrice', e.target.value)}
                onChange={e => !e.target.value && setFilter('minPrice', '')}
                onFocus={e => e.target.style.borderColor = '#0ea5e9'}
                onBlur={e => e.target.style.borderColor = 'var(--border)'}
              />
            </div>

            {/* Max price */}
            <div className="relative">
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-xs" style={{ color: 'var(--text-muted)' }}>₫≤</span>
              <input
                type="number" min="0"
                className="w-full pl-9 pr-4 py-2.5 rounded-xl text-sm focus:outline-none"
                style={inputStyle}
                placeholder={t('tpl.max_price_ph')}
                defaultValue={filters.maxPrice}
                onKeyDown={e => e.key === 'Enter' && setFilter('maxPrice', e.target.value)}
                onChange={e => !e.target.value && setFilter('maxPrice', '')}
                onFocus={e => e.target.style.borderColor = '#0ea5e9'}
                onBlur={e => e.target.style.borderColor = 'var(--border)'}
              />
            </div>

            {/* Quick toggles */}
            <div className="flex items-center gap-2 flex-wrap">
              {[
                { key: 'onSale',     icon: '🔥', label: 'Sale' },
                { key: 'isFeatured', icon: '⭐', label: t('tpl.featured_label') },
                { key: 'isNew',      icon: '🆕', label: t('tpl.new_label') },
              ].map(({ key, icon, label }) => {
                const active = filters[key] === 'true';
                return (
                  <button
                    key={key}
                    onClick={() => setFilter(key, active ? '' : 'true')}
                    className="inline-flex items-center gap-1 px-3 py-1.5 rounded-xl text-xs font-semibold transition-all border"
                    style={active
                      ? { backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)', borderColor: 'var(--sidebar-active-bg)' }
                      : { backgroundColor: 'var(--bg-elevated)', color: 'var(--text-muted)', borderColor: 'var(--border)' }
                    }
                  >
                    {icon} {label}
                  </button>
                );
              })}
            </div>
          </div>
        )}

        {/* Tags row */}
        {tags.length > 0 && (
          <div className="flex flex-wrap gap-2 mt-3 pt-3" style={{ borderTop: '1px solid var(--border)' }}>
            <button
              onClick={() => setFilter('tagSlug', '')}
              className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold transition-all border"
              style={!filters.tagSlug
                ? { backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)', borderColor: 'var(--sidebar-active-bg)' }
                : { backgroundColor: 'var(--bg-elevated)', color: 'var(--text-muted)', borderColor: 'var(--border)' }
              }
            >
              {t('tpl.all_tag')}
            </button>
            {tags.slice(0, 14).map(tag => (
              <button
                key={tag.id}
                onClick={() => setFilter('tagSlug', filters.tagSlug === tag.slug ? '' : tag.slug)}
                className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold transition-all border"
                style={filters.tagSlug === tag.slug
                  ? { backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)', borderColor: 'var(--sidebar-active-bg)' }
                  : { backgroundColor: 'var(--bg-elevated)', color: 'var(--text-muted)', borderColor: 'var(--border)' }
                }
              >
                #{tag.name}
              </button>
            ))}
          </div>
        )}
      </div>

      {/* Active filter chips */}
      {activeChips.length > 0 && (
        <div className="flex flex-wrap items-center gap-2 mb-4">
          <span className="text-xs font-semibold" style={{ color: 'var(--text-muted)' }}>{t('tpl.filter_active')}:</span>
          {activeChips.map(chip => (
            <button
              key={chip.key}
              onClick={() => {
                setFilter(chip.key, '');
                if (chip.key === 'search' && searchRef.current) searchRef.current.value = '';
              }}
              className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold transition-all border"
              style={{ backgroundColor: 'rgba(139,92,246,0.08)', color: '#7c3aed', borderColor: 'rgba(139,92,246,0.2)' }}
            >
              {chip.label} ✕
            </button>
          ))}
        </div>
      )}

      {/* Results */}
      {loading ? <LoadingPage /> : (
        <>
          {templates.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-24 text-center">
              <div className="w-16 h-16 rounded-2xl flex items-center justify-center text-3xl mb-4"
                style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)' }}>🗂️</div>
              <p className="font-bold text-lg mb-1" style={{ color: 'var(--text-primary)' }}>{t('tpl.not_found')}</p>
              <p className="text-sm mb-4" style={{ color: 'var(--text-muted)' }}>{t('tpl.change_filter')}</p>
              <button onClick={clearAll}
                className="px-4 py-2 rounded-xl text-sm font-bold"
                style={{ backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)' }}>
                {t('tpl.clear_filters')}
              </button>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
              {templates.map((tpl, idx) => {
                // Check sale hợp lệ
                const now = Date.now();
                const saleValid = tpl.salePrice &&
                  (!tpl.saleStartAt || new Date(tpl.saleStartAt) <= now) &&
                  (!tpl.saleEndAt   || new Date(tpl.saleEndAt)   >  now);
                const discountPct = saleValid
                  ? Math.round((tpl.price - tpl.salePrice) / tpl.price * 100)
                  : 0;

                return (
                  <Link key={tpl.id} to={`/templates/${tpl.slug}`} className="group">
                    <div
                      className="rounded-2xl overflow-hidden transition-all duration-200 hover:-translate-y-0.5 hover:shadow-lg"
                      style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}
                      onMouseEnter={e => e.currentTarget.style.borderColor = 'var(--border-light, #94a3b8)'}
                      onMouseLeave={e => e.currentTarget.style.borderColor = 'var(--border)'}
                    >
                      {/* Thumbnail */}
                      <div className="aspect-video relative overflow-hidden"
                        style={{ backgroundColor: 'var(--bg-elevated)' }}>
                        {tpl.thumbnailUrl ? (
                          <img
                            src={toAbsoluteUrl(tpl.thumbnailUrl)}
                            alt={tpl.name}
                            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                          />
                        ) : (
                          <div className={`w-full h-full flex items-center justify-center bg-gradient-to-br ${accentColors[idx % accentColors.length]} text-4xl`}>
                            🖼️
                          </div>
                        )}

                        {/* Discount badge */}
                        {saleValid && discountPct > 0 && (
                          <div className="absolute top-0 left-0 w-12 h-12 flex items-center justify-center font-black text-white text-[11px]"
                            style={{
                              background: 'linear-gradient(135deg, #ef4444, #f97316)',
                              clipPath: 'polygon(0 0, 100% 0, 100% 75%, 50% 100%, 0 75%)',
                            }}>
                            <span className="mt-1">-{discountPct}%</span>
                          </div>
                        )}

                        {/* Badges */}
                        <div className="absolute top-2.5 left-2.5 flex gap-1.5"
                          style={saleValid ? { marginLeft: '2.5rem' } : {}}>
                          {tpl.isFree     && <span className="px-2 py-0.5 rounded-full bg-emerald-500 text-white text-[10px] font-bold uppercase shadow-sm">FREE</span>}
                          {tpl.isNew      && <span className="px-2 py-0.5 rounded-full bg-violet-500 text-white text-[10px] font-bold uppercase shadow-sm">NEW</span>}
                          {tpl.isFeatured && <span className="px-2 py-0.5 rounded-full bg-amber-400 text-black text-[10px] font-bold uppercase shadow-sm">⭐ TOP</span>}
                        </div>

                        {/* Wishlist */}
                        {isAuth && (
                          <button
                            onClick={(e) => handleToggleWishlist(tpl.id, e)}
                            disabled={wishlistLoading[tpl.id]}
                            className="absolute top-2.5 right-2.5 w-8 h-8 rounded-full bg-white/90 backdrop-blur-sm flex items-center justify-center text-sm shadow-md hover:scale-110 transition-transform border border-white/50"
                          >
                            {wishlistLoading[tpl.id]
                              ? <span className="text-xs animate-spin">⟳</span>
                              : (tpl.isInWishlist ? '❤️' : '🤍')}
                          </button>
                        )}

                        {/* Sale end countdown */}
                        {saleValid && tpl.saleEndAt && (
                          <SaleCountdownBadge endAt={tpl.saleEndAt} />
                        )}
                      </div>

                      {/* Info */}
                      <div className="p-4">
                        <h3 className="font-bold text-sm leading-snug mb-1 group-hover:text-violet-500 transition-colors line-clamp-2"
                          style={{ color: 'var(--text-primary)' }}>
                          {tpl.name}
                        </h3>
                        <p className="text-xs mb-3 line-clamp-1" style={{ color: 'var(--text-muted)' }}>
                          {tpl.shortDescription}
                        </p>

                        <div className="flex items-center justify-between">
                          <div>
                            {tpl.isFree ? (
                              <span className="text-emerald-500 font-bold text-sm">{t('tpl.free_label')}</span>
                            ) : saleValid ? (
                              <div className="flex items-center gap-1.5">
                                <Price amount={tpl.salePrice} className="font-black text-sm" style={{ color: '#ef4444' }} />
                                <Price amount={tpl.price} className="text-xs line-through" style={{ color: 'var(--text-muted)' }} />
                              </div>
                            ) : (
                              <Price amount={tpl.price} className="font-bold text-sm" style={{ color: 'var(--text-primary)' }} />
                            )}
                          </div>
                          <div className="flex items-center gap-2 text-xs" style={{ color: 'var(--text-muted)' }}>
                            <span className="flex items-center gap-0.5">
                              <span className="text-amber-400">★</span>
                              {tpl.averageRating?.toFixed(1) || '—'}
                            </span>
                            <span className="w-1 h-1 rounded-full" style={{ backgroundColor: 'var(--border)' }} />
                            <span>{tpl.salesCount} {t('tpl.sales')}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                  </Link>
                );
              })}
            </div>
          )}

          <Pagination
            page={filters.page}
            totalPages={pagination?.totalPages || 1}
            onPageChange={p => setFilter('page', p.toString())}
          />
        </>
      )}
    </div>
  );
}

// ── Mini countdown badge trên card ───────────────────────────────────────────
function SaleCountdownBadge({ endAt }) {
  const [left, setLeft] = useState(() => calcLeft(endAt));
  useEffect(() => {
    const id = setInterval(() => setLeft(calcLeft(endAt)), 1000);
    return () => clearInterval(id);
  }, [endAt]);
  if (!left || left.expired) return null;
  const pad = n => String(n).padStart(2, '0');
  const urgent = left.h === 0 && left.m < 60;
  return (
    <div className="absolute bottom-0 left-0 right-0 px-2.5 py-1.5 flex items-center justify-between"
      style={{ background: 'linear-gradient(to top, rgba(0,0,0,0.75) 0%, transparent 100%)' }}>
      <span className="text-[9px] font-bold text-white/70">⏱ Còn lại</span>
      <span className="text-[11px] font-black tabular-nums"
        style={{ color: urgent ? '#fca5a5' : '#fde68a' }}>
        {pad(left.h)}:{pad(left.m)}:{pad(left.s)}
      </span>
    </div>
  );
}

function calcLeft(endAt) {
  const diff = new Date(endAt) - Date.now();
  if (diff <= 0) return { expired: true };
  return {
    h: Math.floor(diff / 3600000),
    m: Math.floor((diff % 3600000) / 60000),
    s: Math.floor((diff % 60000) / 1000),
    expired: false,
  };
}