import { useState, useEffect, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { templateApi, publicApi, userApi } from '../../api/services';
import { toAbsoluteUrl } from '../../api/client';
import { Price, useToast } from '../../components/ui';
import { useAuth } from '../../context/AuthContext';
import { useLang } from '../../context/Langcontext';

// ─── Countdown mini (cho sale cards) ─────────────────────────────────────────
function calcLeft(endAt) {
  const diff = new Date(endAt) - Date.now();
  if (diff <= 0) return null;
  return {
    h: Math.floor(diff / 3600000),
    m: Math.floor((diff % 3600000) / 60000),
    s: Math.floor((diff % 60000) / 1000),
  };
}
function MiniCountdown({ endAt }) {
  const [left, setLeft] = useState(() => calcLeft(endAt));
  useEffect(() => {
    const id = setInterval(() => setLeft(calcLeft(endAt)), 1000);
    return () => clearInterval(id);
  }, [endAt]);
  if (!left) return null;
  const pad = n => String(n).padStart(2, '0');
  return (
    <span className="font-black tabular-nums text-xs" style={{ color: '#ff3b30' }}>
      {pad(left.h)}:{pad(left.m)}:{pad(left.s)}
    </span>
  );
}

// ─── Template card compact ────────────────────────────────────────────────────
function TemplateCard({ tpl, onWishlist, wishlistLoading, isAuth }) {
  const { t } = useLang();
  const now = Date.now();
  const saleValid = tpl.salePrice &&
    (!tpl.saleStartAt || new Date(tpl.saleStartAt) <= now) &&
    (!tpl.saleEndAt   || new Date(tpl.saleEndAt)   >  now);
  const discountPct = saleValid ? Math.round((tpl.price - tpl.salePrice) / tpl.price * 100) : 0;

  return (
    <Link to={`/templates/${tpl.slug}`} className="group block">
      <div className="rounded-xl overflow-hidden transition-all duration-200 hover:-translate-y-0.5 hover:shadow-lg"
        style={{ backgroundColor: 'var(--bg-card)', border: '1.5px solid var(--border)' }}
        onMouseEnter={e => { e.currentTarget.style.borderColor = '#7c3aed40'; }}
        onMouseLeave={e => { e.currentTarget.style.borderColor = 'var(--border)'; }}>
        <div className="relative overflow-hidden" style={{ aspectRatio: '16/9', backgroundColor: 'var(--bg-elevated)' }}>
          {tpl.thumbnailUrl ? (
            <img src={toAbsoluteUrl(tpl.thumbnailUrl)} alt={tpl.name}
              className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" />
          ) : (
            <div className="w-full h-full flex items-center justify-center text-3xl"
              style={{ background: 'linear-gradient(135deg, rgba(124,58,237,0.06), rgba(14,165,233,0.06))' }}>🖼️</div>
          )}
          {/* Badges */}
          <div className="absolute top-2 left-2 flex gap-1">
            {tpl.isFree && <span className="px-2 py-0.5 rounded-full bg-emerald-500 text-white text-[9px] font-black">FREE</span>}
            {tpl.isNew  && <span className="px-2 py-0.5 rounded-full bg-violet-500 text-white text-[9px] font-black">NEW</span>}
            {saleValid  && <span className="px-2 py-0.5 rounded-full bg-red-500 text-white text-[9px] font-black">-{discountPct}%</span>}
          </div>
          {isAuth && (
            <button onClick={e => { e.preventDefault(); onWishlist(tpl.id, e); }}
              disabled={wishlistLoading?.[tpl.id]}
              className="absolute top-2 right-2 w-7 h-7 rounded-full flex items-center justify-center text-xs hover:scale-110 transition-transform"
              style={{ background: 'rgba(255,255,255,0.9)', backdropFilter: 'blur(4px)' }}>
              {wishlistLoading?.[tpl.id] ? '⟳' : (tpl.isInWishlist ? '❤️' : '🤍')}
            </button>
          )}
        </div>
        <div className="p-3">
          <h3 className="font-bold text-sm leading-snug line-clamp-1 mb-1 group-hover:text-violet-500 transition-colors"
            style={{ color: 'var(--text-primary)' }}>{tpl.name}</h3>
          <div className="flex items-center justify-between">
            <div className="flex items-baseline gap-1.5">
              {tpl.isFree ? (
                <span className="text-xs font-bold text-emerald-500">{t('tpl.free_label')}</span>
              ) : saleValid ? (
                <>
                  <span className="text-sm font-black" style={{ color: '#ef4444' }}><Price amount={tpl.salePrice} /></span>
                  <span className="text-xs line-through" style={{ color: 'var(--text-muted)' }}><Price amount={tpl.price} /></span>
                </>
              ) : (
                <span className="text-sm font-bold" style={{ color: 'var(--text-primary)' }}><Price amount={tpl.price} /></span>
              )}
            </div>
            <div className="flex items-center gap-1 text-xs" style={{ color: 'var(--text-muted)' }}>
              <span className="text-amber-400">★</span>
              <span>{tpl.averageRating > 0 ? tpl.averageRating.toFixed(1) : '—'}</span>
            </div>
          </div>
        </div>
      </div>
    </Link>
  );
}

// ─── Section header ───────────────────────────────────────────────────────────
function SectionHeader({ eyebrow, title, subtitle, action }) {
  return (
    <div className="flex items-end justify-between mb-6">
      <div>
        {eyebrow && (
          <p className="text-[10px] font-black uppercase tracking-widest mb-1.5" style={{ color: '#7c3aed' }}>{eyebrow}</p>
        )}
        <h2 className="text-xl font-black tracking-tight" style={{ color: 'var(--text-primary)', fontFamily: 'Syne, sans-serif' }}>
          {title}
        </h2>
        {subtitle && <p className="text-sm mt-0.5" style={{ color: 'var(--text-muted)' }}>{subtitle}</p>}
      </div>
      {action && (
        <Link to={action.href}
          className="hidden sm:inline-flex items-center gap-1 text-xs font-bold transition-colors hover:opacity-80"
          style={{ color: '#7c3aed' }}>
          {action.label} →
        </Link>
      )}
    </div>
  );
}

// ─── Main Landing Page ────────────────────────────────────────────────────────
export default function LandingPage() {
  const { t } = useLang();
  const { isAuth } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();
  const searchRef = useRef(null);

  const [featured,    setFeatured]    = useState([]);
  const [newest,      setNewest]      = useState([]);
  const [onSale,      setOnSale]      = useState([]);
  const [categories,  setCategories]  = useState([]);
  const [banners,     setBanners]     = useState([]);
  const [bannerIdx,   setBannerIdx]   = useState(0);
  const [wishlistLoading, setWishlistLoading] = useState({});
  const [heroVisible, setHeroVisible] = useState(false);

  useEffect(() => {
    setHeroVisible(true);
    // Parallel fetches
    Promise.all([
      templateApi.getList({ sortBy: 'popular', isFeatured: true, pageSize: 8 }),
      templateApi.getList({ sortBy: 'newest', pageSize: 8 }),
      templateApi.getOnSaleList({ pageSize: 6 }),
      publicApi.getCategories(),
      publicApi.getBanners('Home'),
    ]).then(([featRes, newRes, saleRes, catRes, banRes]) => {
      setFeatured(featRes.data.data?.items || []);
      setNewest(newRes.data.data?.items || []);
      setOnSale(saleRes.data.data?.items || []);
      setCategories(catRes.data.data || []);
      setBanners(banRes.data.data || []);
    }).catch(() => {});
  }, []);

  // Banner auto-rotate
  useEffect(() => {
    if (banners.length < 2) return;
    const id = setInterval(() => setBannerIdx(i => (i + 1) % banners.length), 4500);
    return () => clearInterval(id);
  }, [banners.length]);

  const handleToggleWishlist = async (templateId, e) => {
    e.preventDefault();
    if (!isAuth) { navigate('/login'); return; }
    setWishlistLoading(w => ({ ...w, [templateId]: true }));
    try {
      const res = await userApi.toggleWishlist(templateId);
      const inWishlist = res.data.data;
      setFeatured(ts => ts.map(t => t.id === templateId ? { ...t, isInWishlist: inWishlist } : t));
      setNewest(ts  => ts.map(t => t.id === templateId ? { ...t, isInWishlist: inWishlist } : t));
    } catch {}
    finally { setWishlistLoading(w => ({ ...w, [templateId]: false })); }
  };

  const handleSearch = (e) => {
    e.preventDefault();
    const q = searchRef.current?.value?.trim();
    if (q) navigate(`/templates?search=${encodeURIComponent(q)}`);
    else navigate('/templates');
  };

  // Stats mock – replace with real API if needed
  const stats = [
    { value: '500+', label: t('land.stat_templates') },
    { value: '2K+',  label: t('land.stat_users') },
    { value: '50+',  label: t('land.stat_categories') },
    { value: '4.9★', label: t('land.stat_rating') },
  ];

  const catIcons = ['🖥️','📱','🛒','🎨','📊','✉️','🎯','🔑'];

  return (
    <div className="animate-fade-in">

      {/* ═══════════════════════════════════════════════════════
          HERO
      ═══════════════════════════════════════════════════════ */}
      <section className="relative rounded-3xl overflow-hidden mb-8"
        style={{
          minHeight: 360,
          backgroundColor: 'var(--bg-card)',
          border: '1.5px solid var(--border)',
        }}>

        {/* BG mesh */}
        <div className="absolute inset-0 pointer-events-none overflow-hidden">
          <div className="absolute -top-20 -right-20 w-72 h-72 rounded-full opacity-20"
            style={{ background: 'radial-gradient(circle, #7c3aed, transparent 70%)' }} />
          <div className="absolute -bottom-12 -left-12 w-56 h-56 rounded-full opacity-10"
            style={{ background: 'radial-gradient(circle, #0ea5e9, transparent 70%)' }} />
          <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-96 h-96 rounded-full opacity-5"
            style={{ background: 'radial-gradient(circle, #f59e0b, transparent 70%)' }} />
          {/* Grid dots */}
          <svg className="absolute inset-0 w-full h-full opacity-[0.025]" xmlns="http://www.w3.org/2000/svg">
            <defs>
              <pattern id="dots" x="0" y="0" width="24" height="24" patternUnits="userSpaceOnUse">
                <circle cx="2" cy="2" r="1.5" fill="currentColor" />
              </pattern>
            </defs>
            <rect width="100%" height="100%" fill="url(#dots)" />
          </svg>
        </div>

        <div className={`relative z-10 flex flex-col items-center text-center px-6 py-16 md:py-20 transition-all duration-700 ${heroVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>

          {/* Eyebrow pill */}
          <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-full text-xs font-black uppercase tracking-widest mb-5"
            style={{ background: 'linear-gradient(135deg, rgba(124,58,237,0.12), rgba(14,165,233,0.08))', border: '1.5px solid rgba(124,58,237,0.25)', color: '#7c3aed' }}>
            <span className="w-1.5 h-1.5 rounded-full bg-violet-500 animate-pulse" />
            {t('land.hero_badge')}
          </div>

          {/* Headline */}
          <h1 className="font-black tracking-tight leading-none mb-4"
            style={{
              fontFamily: 'Syne, sans-serif',
              fontSize: 'clamp(2rem, 5vw, 3.5rem)',
              color: 'var(--text-primary)',
              maxWidth: 700,
            }}>
            {t('land.hero_title_1')}{' '}
            <span style={{
              background: 'linear-gradient(135deg, #7c3aed 0%, #0ea5e9 50%, #10b981 100%)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              backgroundClip: 'text',
            }}>
              {t('land.hero_title_2')}
            </span>
            <br />{t('land.hero_title_3')}
          </h1>

          <p className="text-base mb-8 max-w-md leading-relaxed" style={{ color: 'var(--text-muted)' }}>
            {t('land.hero_desc')}
          </p>

          {/* Search bar */}
          <form onSubmit={handleSearch}
            className="flex w-full max-w-lg gap-2 mb-8">
            <div className="relative flex-1">
              <span className="absolute left-3.5 top-1/2 -translate-y-1/2" style={{ color: 'var(--text-muted)' }}>🔍</span>
              <input
                ref={searchRef}
                className="w-full pl-10 pr-4 py-3 rounded-xl text-sm focus:outline-none transition-all"
                style={{
                  backgroundColor: 'var(--bg-elevated)',
                  border: '1.5px solid var(--border)',
                  color: 'var(--text-primary)',
                }}
                placeholder={t('land.search_ph')}
                onFocus={e => { e.target.style.borderColor = '#7c3aed'; e.target.style.boxShadow = '0 0 0 3px rgba(124,58,237,0.1)'; }}
                onBlur={e => { e.target.style.borderColor = 'var(--border)'; e.target.style.boxShadow = 'none'; }}
              />
            </div>
            <button type="submit"
              className="px-5 py-3 rounded-xl text-sm font-bold text-white transition-all hover:-translate-y-0.5 active:scale-95"
              style={{ background: 'linear-gradient(135deg, #7c3aed, #0ea5e9)', boxShadow: '0 4px 16px rgba(124,58,237,0.35)' }}>
              {t('land.search_btn')}
            </button>
          </form>

          {/* CTA buttons */}
          <div className="flex flex-wrap justify-center gap-3 mb-10">
            <Link to="/templates"
              className="px-6 py-2.5 rounded-xl text-sm font-bold text-white transition-all hover:-translate-y-0.5"
              style={{ background: 'linear-gradient(135deg, #7c3aed, #0ea5e9)', boxShadow: '0 4px 14px rgba(124,58,237,0.3)' }}>
              {t('land.cta_browse')}
            </Link>
            <Link to="/sale"
              className="px-6 py-2.5 rounded-xl text-sm font-bold transition-all hover:-translate-y-0.5"
              style={{ backgroundColor: 'rgba(255,59,48,0.08)', border: '1.5px solid rgba(255,59,48,0.25)', color: '#ff3b30' }}>
              🔥 {t('sale.nav_label')}
            </Link>
          </div>

          {/* Stats row */}
          <div className="flex flex-wrap justify-center gap-6">
            {stats.map(s => (
              <div key={s.label} className="text-center">
                <p className="text-xl font-black" style={{ color: 'var(--text-primary)', fontFamily: 'Syne, sans-serif' }}>{s.value}</p>
                <p className="text-xs" style={{ color: 'var(--text-muted)' }}>{s.label}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ═══════════════════════════════════════════════════════
          BANNER CAROUSEL (chỉ show nếu có banners từ API)
      ═══════════════════════════════════════════════════════ */}
      {banners.length > 0 && (
        <section className="mb-8">
          <div className="relative rounded-2xl overflow-hidden"
            style={{ height: 180, border: '1.5px solid var(--border)' }}>
            {banners.map((b, i) => (
              <div key={b.id}
                className="absolute inset-0 transition-opacity duration-700"
                style={{ opacity: i === bannerIdx ? 1 : 0, pointerEvents: i === bannerIdx ? 'auto' : 'none' }}>
                {b.imageUrl ? (
                  <img src={toAbsoluteUrl(b.imageUrl)} alt={b.title}
                    className="w-full h-full object-cover" />
                ) : (
                  <div className="w-full h-full flex items-center justify-center"
                    style={{ background: 'linear-gradient(135deg, #7c3aed22, #0ea5e922)' }}>
                    <div className="text-center">
                      <p className="font-black text-xl mb-1" style={{ color: 'var(--text-primary)', fontFamily: 'Syne, sans-serif' }}>{b.title}</p>
                      {b.subTitle && <p className="text-sm" style={{ color: 'var(--text-muted)' }}>{b.subTitle}</p>}
                      {b.linkUrl && (
                        <Link to={b.linkUrl}
                          className="inline-block mt-3 px-4 py-2 rounded-lg text-xs font-bold text-white"
                          style={{ background: 'linear-gradient(135deg, #7c3aed, #0ea5e9)' }}>
                          {t('land.banner_cta')}
                        </Link>
                      )}
                    </div>
                  </div>
                )}
                {/* Overlay text on image banners */}
                {b.imageUrl && (
                  <div className="absolute inset-0 flex items-center px-8"
                    style={{ background: 'linear-gradient(to right, rgba(0,0,0,0.65) 0%, transparent 60%)' }}>
                    <div>
                      <p className="font-black text-xl text-white mb-1" style={{ fontFamily: 'Syne, sans-serif' }}>{b.title}</p>
                      {b.subTitle && <p className="text-sm text-white/70">{b.subTitle}</p>}
                      {b.linkUrl && (
                        <Link to={b.linkUrl}
                          className="inline-block mt-3 px-4 py-2 rounded-lg text-xs font-bold text-white"
                          style={{ background: 'rgba(255,255,255,0.2)', backdropFilter: 'blur(8px)', border: '1px solid rgba(255,255,255,0.3)' }}>
                          {t('land.banner_cta')} →
                        </Link>
                      )}
                    </div>
                  </div>
                )}
              </div>
            ))}
            {/* Dots */}
            {banners.length > 1 && (
              <div className="absolute bottom-3 right-4 flex gap-1.5">
                {banners.map((_, i) => (
                  <button key={i} onClick={() => setBannerIdx(i)}
                    className="rounded-full transition-all"
                    style={{ width: i === bannerIdx ? 20 : 6, height: 6, backgroundColor: i === bannerIdx ? '#ffffff' : 'rgba(255,255,255,0.4)' }} />
                ))}
              </div>
            )}
          </div>
        </section>
      )}

      {/* ═══════════════════════════════════════════════════════
          CATEGORIES
      ═══════════════════════════════════════════════════════ */}
      {categories.length > 0 && (
        <section className="mb-10">
          <SectionHeader
            eyebrow={t('land.cat_eyebrow')}
            title={t('land.cat_title')}
            action={{ href: '/templates', label: t('land.see_all') }}
          />
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-3">
            {categories.slice(0, 12).map((cat, i) => (
              <Link key={cat.id} to={`/templates?categorySlug=${cat.slug}`}
                className="group flex flex-col items-center gap-2.5 p-4 rounded-2xl text-center transition-all duration-200 hover:-translate-y-0.5 hover:shadow-md"
                style={{ backgroundColor: 'var(--bg-card)', border: '1.5px solid var(--border)' }}
                onMouseEnter={e => { e.currentTarget.style.borderColor = '#7c3aed30'; }}
                onMouseLeave={e => { e.currentTarget.style.borderColor = 'var(--border)'; }}>
                <div className="w-10 h-10 rounded-xl flex items-center justify-center text-xl transition-transform group-hover:scale-110"
                  style={{ background: `linear-gradient(135deg, hsl(${(i * 37 + 240) % 360},70%,90%) 0%, hsl(${(i * 37 + 260) % 360},60%,85%) 100%)` }}>
                  {catIcons[i % catIcons.length]}
                </div>
                <p className="text-xs font-bold line-clamp-1 transition-colors group-hover:text-violet-500"
                  style={{ color: 'var(--text-secondary)' }}>
                  {cat.name}
                </p>
              </Link>
            ))}
          </div>
        </section>
      )}

      {/* ═══════════════════════════════════════════════════════
          FLASH SALE STRIP (nếu có templates đang sale)
      ═══════════════════════════════════════════════════════ */}
      {onSale.length > 0 && (
        <section className="mb-10">
          {/* Header nổi bật */}
          <div className="flex items-center justify-between mb-5">
            <div className="flex items-center gap-3">
              <div className="flex items-center gap-2 px-3.5 py-1.5 rounded-xl font-black text-sm"
                style={{ background: 'linear-gradient(135deg, #ff3b30, #ff6b35)', color: '#fff', boxShadow: '0 4px 12px rgba(255,59,48,0.3)' }}>
                🔥 Flash Sale
              </div>
              <div className="hidden sm:block">
                <p className="text-[10px] font-bold uppercase tracking-widest" style={{ color: 'var(--text-muted)' }}>
                  {t('land.sale_ends')}
                </p>
                {onSale[0]?.saleEndAt && (
                  <MiniCountdown endAt={onSale[0].saleEndAt} />
                )}
              </div>
            </div>
            <Link to="/sale"
              className="inline-flex items-center gap-1 text-xs font-bold transition-colors hover:opacity-80"
              style={{ color: '#ff3b30' }}>
              {t('land.see_all')} →
            </Link>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-3">
            {onSale.map(tpl => (
              <Link key={tpl.id} to={`/templates/${tpl.slug}`} className="group block">
                <div className="rounded-xl overflow-hidden transition-all duration-200 hover:-translate-y-0.5"
                  style={{ backgroundColor: 'var(--bg-card)', border: '1.5px solid var(--border)' }}
                  onMouseEnter={e => { e.currentTarget.style.borderColor = '#ff3b3040'; e.currentTarget.style.boxShadow = '0 6px 20px rgba(255,59,48,0.1)'; }}
                  onMouseLeave={e => { e.currentTarget.style.borderColor = 'var(--border)'; e.currentTarget.style.boxShadow = 'none'; }}>
                  {/* Thumbnail */}
                  <div className="relative overflow-hidden" style={{ aspectRatio: '4/3', backgroundColor: 'var(--bg-elevated)' }}>
                    {tpl.thumbnailUrl ? (
                      <img src={toAbsoluteUrl(tpl.thumbnailUrl)} alt={tpl.name}
                        className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" />
                    ) : (
                      <div className="w-full h-full flex items-center justify-center text-2xl"
                        style={{ background: 'linear-gradient(135deg, rgba(255,59,48,0.05), rgba(124,58,237,0.05))' }}>🔥</div>
                    )}
                    {/* % badge */}
                    <div className="absolute top-0 left-0 flex items-center justify-center font-black text-white"
                      style={{
                        width: 40, height: 40, fontSize: 11,
                        background: 'linear-gradient(135deg, #ff3b30, #ff6b35)',
                        clipPath: 'polygon(0 0, 100% 0, 100% 76%, 50% 100%, 0 76%)',
                      }}>
                      -{tpl.discountPercent}%
                    </div>
                  </div>
                  <div className="p-2.5">
                    <p className="text-xs font-bold line-clamp-1 mb-1" style={{ color: 'var(--text-primary)' }}>{tpl.name}</p>
                    <div className="flex items-baseline gap-1">
                      <span className="text-sm font-black" style={{ color: '#ff3b30' }}><Price amount={tpl.salePrice} /></span>
                      <span className="text-[10px] line-through" style={{ color: 'var(--text-muted)' }}><Price amount={tpl.originalPrice} /></span>
                    </div>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        </section>
      )}

      {/* ═══════════════════════════════════════════════════════
          FEATURED TEMPLATES
      ═══════════════════════════════════════════════════════ */}
      {featured.length > 0 && (
        <section className="mb-10">
          <SectionHeader
            eyebrow={t('land.featured_eyebrow')}
            title={t('land.featured_title')}
            subtitle={t('land.featured_subtitle')}
            action={{ href: '/templates?isFeatured=true&sortBy=popular', label: t('land.see_all') }}
          />
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {featured.slice(0, 8).map(tpl => (
              <TemplateCard key={tpl.id} tpl={tpl}
                onWishlist={handleToggleWishlist}
                wishlistLoading={wishlistLoading}
                isAuth={isAuth} />
            ))}
          </div>
        </section>
      )}

      {/* ═══════════════════════════════════════════════════════
          NEWEST TEMPLATES
      ═══════════════════════════════════════════════════════ */}
      {newest.length > 0 && (
        <section className="mb-10">
          <SectionHeader
            eyebrow={t('land.new_eyebrow')}
            title={t('land.new_title')}
            subtitle={t('land.new_subtitle')}
            action={{ href: '/templates?sortBy=newest', label: t('land.see_all') }}
          />
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {newest.slice(0, 8).map(tpl => (
              <TemplateCard key={tpl.id} tpl={tpl}
                onWishlist={handleToggleWishlist}
                wishlistLoading={wishlistLoading}
                isAuth={isAuth} />
            ))}
          </div>
        </section>
      )}

      {/* ═══════════════════════════════════════════════════════
          WHY QTEMPLATE — feature grid
      ═══════════════════════════════════════════════════════ */}
      <section className="mb-10">
        <SectionHeader
          eyebrow={t('land.why_eyebrow')}
          title={t('land.why_title')}
        />
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {[
            { icon: '⚡', title: t('land.feat_ready_title'),  desc: t('land.feat_ready_desc') },
            { icon: '🎨', title: t('land.feat_design_title'), desc: t('land.feat_design_desc') },
            { icon: '🔧', title: t('land.feat_code_title'),   desc: t('land.feat_code_desc') },
            { icon: '📱', title: t('land.feat_resp_title'),   desc: t('land.feat_resp_desc') },
            { icon: '🔄', title: t('land.feat_update_title'), desc: t('land.feat_update_desc') },
            { icon: '💬', title: t('land.feat_support_title'),desc: t('land.feat_support_desc') },
          ].map((f, i) => (
            <div key={i} className="p-5 rounded-2xl transition-all duration-200 hover:-translate-y-0.5"
              style={{ backgroundColor: 'var(--bg-card)', border: '1.5px solid var(--border)' }}>
              <div className="w-10 h-10 rounded-xl flex items-center justify-center text-xl mb-3"
                style={{ background: `linear-gradient(135deg, hsl(${240 + i * 20},70%,92%) 0%, hsl(${260 + i * 20},60%,88%) 100%)` }}>
                {f.icon}
              </div>
              <h3 className="font-bold text-sm mb-1.5" style={{ color: 'var(--text-primary)' }}>{f.title}</h3>
              <p className="text-xs leading-relaxed" style={{ color: 'var(--text-muted)' }}>{f.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* ═══════════════════════════════════════════════════════
          CTA BOTTOM
      ═══════════════════════════════════════════════════════ */}
      <section className="relative rounded-3xl overflow-hidden p-10 text-center mb-2"
        style={{ backgroundColor: 'var(--bg-card)', border: '1.5px solid var(--border)' }}>
        <div className="absolute inset-0 pointer-events-none"
          style={{ background: 'linear-gradient(135deg, rgba(124,58,237,0.07) 0%, rgba(14,165,233,0.05) 50%, rgba(16,185,129,0.04) 100%)' }} />
        <div className="absolute -top-12 -right-12 w-48 h-48 rounded-full opacity-15 pointer-events-none"
          style={{ background: 'radial-gradient(circle, #7c3aed, transparent 70%)' }} />
        <div className="relative">
          <p className="text-[11px] font-black uppercase tracking-widest mb-3" style={{ color: '#7c3aed' }}>
            {t('land.cta_eyebrow')}
          </p>
          <h2 className="font-black tracking-tight mb-3"
            style={{ fontFamily: 'Syne, sans-serif', fontSize: 'clamp(1.4rem, 3vw, 2.2rem)', color: 'var(--text-primary)' }}>
            {t('land.cta_title')}
          </h2>
          <p className="text-sm mb-7 max-w-sm mx-auto" style={{ color: 'var(--text-muted)' }}>
            {t('land.cta_desc')}
          </p>
          <div className="flex flex-wrap justify-center gap-3">
            <Link to="/templates"
              className="px-7 py-3 rounded-xl text-sm font-bold text-white transition-all hover:-translate-y-0.5"
              style={{ background: 'linear-gradient(135deg, #7c3aed, #0ea5e9)', boxShadow: '0 4px 16px rgba(124,58,237,0.35)' }}>
              {t('land.cta_browse')}
            </Link>
            {!isAuth && (
              <Link to="/register"
                className="px-7 py-3 rounded-xl text-sm font-bold transition-all hover:-translate-y-0.5"
                style={{ backgroundColor: 'var(--bg-elevated)', border: '1.5px solid var(--border)', color: 'var(--text-primary)' }}>
                {t('land.cta_signup')}
              </Link>
            )}
          </div>
        </div>
      </section>
    </div>
  );
}