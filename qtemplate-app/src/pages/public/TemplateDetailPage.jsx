import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { templateApi, userApi, orderApi, couponApi } from '../../api/services';
import { extractError, toAbsoluteUrl } from '../../api/client';
import { LoadingPage, Price, StarRating, Spinner, FormField, EmptyState, useToast, Portal } from '../../components/ui';
import { useAuth } from '../../context/AuthContext';
import PreviewModal from '../../modals/public/PreviewModal';
import { getAffiliateCode, clearAffiliateCode } from '../../utils/affiliate';
import { useLang } from '../../context/Langcontext';

export default function TemplateDetailPage() {
  const { t } = useLang();
  const { slug } = useParams();
  const { isAuth } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();

  const [template, setTemplate] = useState(null);
  const [reviews, setReviews] = useState([]);
  const [loading, setLoading] = useState(true);
  const [wishlistLoading, setWishlistLoading] = useState(false);
  const [downloadLoading, setDownloadLoading] = useState(false);
  const [orderLoading, setOrderLoading] = useState(false);
  const [couponCode, setCouponCode] = useState('');
  const [couponLoading, setCouponLoading] = useState(false);
  const [couponResult, setCouponResult] = useState(null);
  const [affiliateCode, setAffiliateCode] = useState(() => {
    try { return getAffiliateCode() || ''; } catch { return ''; }
  });
  const [showCouponBox, setShowCouponBox] = useState(false);
  const [reviewModal, setReviewModal] = useState(false);
  const [previewModal, setPreviewModal] = useState(false);
  const [reviewForm, setReviewForm] = useState({ rating: 5, title: '', comment: '' });
  const [reviewLoading, setReviewLoading] = useState(false);

  useEffect(() => {
    const controller = new AbortController();
    let cancelled = false;
    const fetchData = async () => {
      setLoading(true);
      try {
        const [detailRes, reviewsRes] = await Promise.all([
          templateApi.getDetail(slug, controller.signal),
          templateApi.getReviews(slug, 1, 10, controller.signal),
        ]);
        if (!cancelled) {
          setTemplate(detailRes.data.data);
          setReviews(reviewsRes.data.data?.items || []);
        }
      } catch (err) {
        if (!cancelled && err.name !== 'CanceledError' && err.code !== 'ERR_CANCELED')
          toast.error(extractError(err), t('tpl_detail.load_err'));
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    fetchData();
    return () => { cancelled = true; controller.abort(); };
  }, [slug]);

  const handleWishlist = async () => {
    if (!isAuth) { navigate('/login'); return; }
    setWishlistLoading(true);
    try {
      const res = await userApi.toggleWishlist(template.id);
      const inWishlist = res.data.data;
      setTemplate(t => ({ ...t, isInWishlist: inWishlist }));
      toast.success(inWishlist ? t('tpl_detail.wish_add_ok') : t('tpl_detail.wish_remove_ok'));
    } catch (err) {
      toast.error(extractError(err));
    } finally { setWishlistLoading(false); }
  };

  const handleDownload = async () => {
    if (!isAuth) { navigate('/login'); return; }
    setDownloadLoading(true);
    try {
      const res = await templateApi.download(slug);
      if (res.data?.isExternal) {
        window.open(res.data.redirectUrl, '_blank');
      } else {
        const blob = new Blob([res.data]);
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url; a.download = `${slug}.zip`; a.click();
        URL.revokeObjectURL(url);
      }
      toast.success(t('tpl_detail.dl_ok'));
    } catch (err) {
      toast.error(extractError(err), t('tpl_detail.dl_err'));
    } finally { setDownloadLoading(false); }
  };

  const handlePreviewCoupon = async () => {
    if (!couponCode.trim()) return;
    setCouponLoading(true);
    setCouponResult(null);
    try {
      const res = await couponApi.preview(couponCode.trim().toUpperCase(), [template.id]);
      setCouponResult({ valid: true, ...res.data.data });
    } catch (err) {
      setCouponResult({ valid: false, message: extractError(err) });
    } finally { setCouponLoading(false); }
  };

  const handleBuy = async () => {
    if (!isAuth) { navigate('/login'); return; }
    setOrderLoading(true);
    try {
      const res = await orderApi.create({
        templateIds: [template.id],
        couponCode: couponResult?.valid ? couponCode.trim().toUpperCase() : undefined,
        affiliateCode: affiliateCode.trim() || undefined,
      });
      clearAffiliateCode();
      navigate(`/dashboard/orders/${res.data.data.id}`);
    } catch (err) {
      toast.error(extractError(err), t('tpl_detail.order_err'));
    } finally { setOrderLoading(false); }
  };

  const handlePreview = () => {
    if (template.previewType === 'ExternalUrl' && template.previewUrl)
      window.open(template.previewUrl, '_blank', 'noopener,noreferrer');
    else if (template.previewType === 'Iframe' && template.previewFolder)
      setPreviewModal(true);
  };

  const handleSubmitReview = async (e) => {
    e.preventDefault();
    setReviewLoading(true);
    try {
      await templateApi.createReview(slug, reviewForm);
      setReviewModal(false);
      toast.success(t('tpl_detail.review_ok'));
      const res = await templateApi.getReviews(slug);
      setReviews(res.data.data?.items || []);
    } catch (err) {
      toast.error(extractError(err), t('tpl_detail.review_err'));
    } finally { setReviewLoading(false); }
  };

  if (loading) return <LoadingPage />;
  if (!template) return null;

  const canPreview =
    (template.previewType === 'ExternalUrl' && template.previewUrl) ||
    (template.previewType === 'Iframe' && template.previewFolder);

  const galleryImages = (() => {
    const imgs = (template.images || []).map(img => ({
      url: toAbsoluteUrl(img.imageUrl),
      alt: img.altText || template.name,
    }));
    if (template.thumbnailUrl) {
      const thumbUrl = toAbsoluteUrl(template.thumbnailUrl);
      if (!imgs.some(i => i.url === thumbUrl))
        imgs.unshift({ url: thumbUrl, alt: 'Thumbnail' });
    }
    return imgs;
  })();

  const inputStyle = {
    backgroundColor: 'var(--bg-elevated)',
    border: '1px solid var(--border)',
    color: 'var(--text-primary)',
  };

  // Stats items
  const statsItems = [
    { value: template.salesCount,   label: t('tpl_detail.sold') },
    { value: template.viewCount,    label: t('tpl_detail.views') },
    { value: template.wishlistCount,label: 'Wishlist' },
  ];

  // Tech info rows
  const techRows = [
    { label: 'Tech Stack',               value: template.techStack },
    { label: t('tpl_detail.compatible'), value: template.compatibleWith },
    { label: t('tpl_detail.format'),     value: template.fileFormat },
    { label: t('tpl_detail.version'),    value: template.version ? `v${template.version}` : null },
  ].filter(x => x.value);

  return (
    <div className="animate-fade-in max-w-6xl">

      {/* Back */}
      <Link to="/templates"
        className="inline-flex items-center gap-1.5 text-sm font-medium mb-5 group transition-colors"
        style={{ color: 'var(--text-muted)' }}
        onMouseEnter={e => e.currentTarget.style.color = 'var(--text-primary)'}
        onMouseLeave={e => e.currentTarget.style.color = 'var(--text-muted)'}
      >
        <span className="w-6 h-6 rounded-lg flex items-center justify-center text-xs transition-colors"
          style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>←</span>
        {t('tpl_detail.back')}
      </Link>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">

        {/* ── Left column ── */}
        <div className="lg:col-span-2 space-y-5">

          <StackedGallery images={galleryImages} templateName={template.name} hintText={t('tpl_detail.gallery_hint')} />

          {/* Description */}
          <div className="rounded-2xl p-6 shadow-sm"
            style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>
            <h2 className="text-sm font-bold uppercase tracking-widest mb-4" style={{ color: 'var(--text-muted)' }}>
              {t('tpl_detail.description')}
            </h2>
            <div
              className="text-sm leading-relaxed prose max-w-none"
              style={{ color: 'var(--text-secondary)' }}
              dangerouslySetInnerHTML={{ __html: template.description || template.shortDescription || '' }}
            />
          </div>

          {/* Tech info */}
          {(template.techStack || template.features?.length > 0) && (
            <div className="rounded-2xl p-6 shadow-sm"
              style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>
              <h2 className="text-sm font-bold uppercase tracking-widest mb-4" style={{ color: 'var(--text-muted)' }}>
                {t('tpl_detail.tech_info')}
              </h2>
              <div className="grid grid-cols-2 gap-4 text-sm">
                {techRows.map(({ label, value }) => (
                  <div key={label}>
                    <p className="text-xs font-semibold uppercase tracking-wide mb-1" style={{ color: 'var(--text-muted)' }}>{label}</p>
                    <p className="font-medium" style={{ color: 'var(--text-secondary)' }}>{value}</p>
                  </div>
                ))}
              </div>
              {template.features?.length > 0 && (
                <div className="mt-5 pt-4" style={{ borderTop: '1px solid var(--border)' }}>
                  <p className="text-xs font-semibold uppercase tracking-wide mb-3" style={{ color: 'var(--text-muted)' }}>
                    {t('tpl_detail.features')}
                  </p>
                  <div className="flex flex-wrap gap-2">
                    {template.features.map((f, i) => (
                      <span key={i} className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-emerald-500/10 border border-emerald-500/20 text-emerald-500 text-xs font-semibold">
                        <span>✓</span> {f}
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}

          {/* Changelog */}
          {template.changeLog && (
            <div className="rounded-2xl p-6 shadow-sm"
              style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>
              <h2 className="text-sm font-bold uppercase tracking-widest mb-4" style={{ color: 'var(--text-muted)' }}>
                {t('tpl_detail.changelog')}
              </h2>
              <pre className="text-xs font-mono whitespace-pre-wrap leading-relaxed rounded-xl p-4"
                style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
                {template.changeLog}
              </pre>
            </div>
          )}

          {/* Reviews */}
          <div className="rounded-2xl p-6 shadow-sm"
            style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>
            <div className="flex items-center justify-between mb-5">
              <h2 className="text-sm font-bold uppercase tracking-widest" style={{ color: 'var(--text-muted)' }}>
                {t('tpl_detail.reviews')}{' '}
                <span className="normal-case font-semibold" style={{ color: 'var(--text-secondary)' }}>
                  ({template.reviewCount})
                </span>
              </h2>
              {isAuth && template.isPurchased && (
                <button
                  onClick={() => setReviewModal(true)}
                  className="px-3 py-1.5 rounded-xl text-xs font-bold transition-all"
                  style={{ border: '1px solid var(--border)', color: 'var(--text-secondary)' }}
                  onMouseEnter={e => e.currentTarget.style.color = 'var(--text-primary)'}
                  onMouseLeave={e => e.currentTarget.style.color = 'var(--text-secondary)'}
                >
                  {t('tpl_detail.write_review')}
                </button>
              )}
            </div>

            {/* Rating summary */}
            <div className="flex items-center gap-5 p-4 rounded-xl mb-5"
              style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)' }}>
              <div className="text-center shrink-0">
                <p className="text-4xl font-black" style={{ color: 'var(--text-primary)' }}>
                  {template.averageRating?.toFixed(1) || '—'}
                </p>
                <StarRating value={Math.round(template.averageRating || 0)} />
                <p className="text-xs mt-1" style={{ color: 'var(--text-muted)' }}>
                  {template.reviewCount} {t('tpl_detail.reviews')}
                </p>
              </div>
            </div>

            {reviews.length === 0 ? (
              <EmptyState icon="💬" title={t('tpl_detail.no_reviews')} />
            ) : (
              <div className="space-y-4">
                {reviews.map(r => (
                  <div key={r.id} className="pb-4 last:pb-0" style={{ borderBottom: '1px solid var(--border)' }}>
                    <div className="flex items-start justify-between mb-1.5">
                      <div>
                        <p className="font-bold text-sm" style={{ color: 'var(--text-primary)' }}>{r.userName}</p>
                        <StarRating value={r.rating} />
                      </div>
                      <p className="text-xs" style={{ color: 'var(--text-muted)' }}>
                        {new Date(r.createdAt).toLocaleDateString('vi-VN')}
                      </p>
                    </div>
                    {r.title   && <p className="font-semibold text-sm mt-2" style={{ color: 'var(--text-primary)' }}>{r.title}</p>}
                    {r.comment && <p className="text-sm mt-1 leading-relaxed" style={{ color: 'var(--text-secondary)' }}>{r.comment}</p>}
                    {r.adminReply && (
                      <div className="mt-3 pl-3 border-l-2 border-violet-400 rounded-r-xl py-2 pr-3"
                        style={{ backgroundColor: 'var(--bg-elevated)' }}>
                        <p className="text-xs font-bold text-violet-500 mb-0.5">{t('tpl_detail.admin_reply')}</p>
                        <p className="text-sm" style={{ color: 'var(--text-secondary)' }}>{r.adminReply}</p>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* ── Right: Purchase card ── */}
        <div className="space-y-4">
          <div className="rounded-2xl p-5 shadow-sm sticky top-4"
            style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>

            <h1 className="font-black text-xl leading-tight mb-3" style={{ color: 'var(--text-primary)' }}>
              {template.name}
            </h1>

            {/* Tags */}
            <div className="flex flex-wrap gap-1.5 mb-4">
              {template.category && (
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full bg-violet-500/10 border border-violet-500/20 text-violet-500 text-xs font-semibold">
                  {template.category.name}
                </span>
              )}
              {template.tags?.map(tag => (
                <span key={tag} className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                  style={{ backgroundColor: 'var(--bg-elevated)', color: 'var(--text-muted)', border: '1px solid var(--border)' }}>
                  #{tag}
                </span>
              ))}
            </div>

            {/* Stats */}
            <div className="grid grid-cols-3 gap-2 mb-4">
              {statsItems.map(({ value, label }) => (
                <div key={label} className="rounded-xl p-2.5 text-center"
                  style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)' }}>
                  <p className="text-base font-black" style={{ color: 'var(--text-primary)' }}>{value}</p>
                  <p className="text-[10px] font-medium" style={{ color: 'var(--text-muted)' }}>{label}</p>
                </div>
              ))}
            </div>

            {/* Price */}
            <div className="mb-4 pb-4" style={{ borderBottom: '1px solid var(--border)' }}>
              {template.isFree ? (
                <p className="text-2xl font-black text-emerald-500">{t('tpl_detail.free')}</p>
              ) : template.salePrice ? (
                <div>
                  <div className="flex items-baseline gap-2">
                    <Price amount={template.salePrice} className="text-2xl font-black" style={{ color: 'var(--text-primary)' }} />
                    <Price amount={template.price} className="text-sm line-through" style={{ color: 'var(--text-muted)' }} />
                    <span className="px-2 py-0.5 rounded-full bg-red-500/10 border border-red-500/20 text-red-500 text-[10px] font-bold">SALE</span>
                  </div>
                  {template.saleEndAt && (
                    <p className="text-xs text-amber-500 mt-1 font-medium">
                      {t('tpl_detail.sale_end')} {new Date(template.saleEndAt).toLocaleDateString('vi-VN')}
                    </p>
                  )}
                </div>
              ) : (
                <div>
                  <Price amount={couponResult?.valid ? couponResult.finalAmount : template.price}
                    className="text-2xl font-black" style={{ color: 'var(--text-primary)' }} />
                  {couponResult?.valid && (
                    <div className="flex items-center gap-2 mt-1">
                      <Price amount={template.price} className="text-sm line-through" style={{ color: 'var(--text-muted)' }} />
                      <span className="text-xs font-bold px-2 py-0.5 rounded-full"
                        style={{ backgroundColor: 'rgba(16,185,129,0.1)', color: '#10b981', border: '1px solid rgba(16,185,129,0.2)' }}>
                        −<Price amount={couponResult.discountAmount} />
                      </span>
                    </div>
                  )}
                </div>
              )}
            </div>

            {/* Coupon + Affiliate */}
            {!template.isFree && !template.isPurchased && (
              <div className="mb-4">
                <button
                  onClick={() => setShowCouponBox(s => !s)}
                  className="flex items-center gap-1.5 text-xs font-semibold mb-2 transition-colors"
                  style={{ color: showCouponBox ? 'var(--text-primary)' : 'var(--text-muted)' }}
                >
                  <span style={{
                    display: 'inline-block', transition: 'transform 0.2s',
                    transform: showCouponBox ? 'rotate(90deg)' : 'rotate(0deg)'
                  }}>›</span>
                  {t('tpl_detail.coupon_title')}
                </button>

                {showCouponBox && (
                  <div className="rounded-xl p-3 space-y-3"
                    style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)' }}>

                    {/* Coupon */}
                    <div>
                      <p className="text-[11px] font-semibold mb-1.5 uppercase tracking-wide" style={{ color: 'var(--text-muted)' }}>
                        {t('tpl_detail.coupon_lbl')}
                      </p>
                      <div className="flex gap-1.5">
                        <input
                          className="flex-1 px-3 py-2 rounded-lg text-xs font-mono uppercase focus:outline-none"
                          style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)', color: 'var(--text-primary)' }}
                          placeholder="VD: SUMMER20"
                          value={couponCode}
                          onChange={e => { setCouponCode(e.target.value.toUpperCase()); setCouponResult(null); }}
                          onKeyDown={e => e.key === 'Enter' && handlePreviewCoupon()}
                          onFocus={e => e.target.style.borderColor = '#8b5cf6'}
                          onBlur={e => e.target.style.borderColor = 'var(--border)'}
                        />
                        <button
                          onClick={handlePreviewCoupon}
                          disabled={!couponCode.trim() || couponLoading}
                          className="px-3 py-2 rounded-lg text-xs font-bold transition-all disabled:opacity-40"
                          style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)', color: 'var(--text-primary)' }}
                        >
                          {couponLoading ? <Spinner /> : t('tpl_detail.check_btn')}
                        </button>
                      </div>
                      {couponResult && (
                        <p className="text-[11px] font-semibold mt-1.5 px-2 py-1 rounded-lg"
                          style={{
                            backgroundColor: couponResult.valid ? 'rgba(16,185,129,0.08)' : 'rgba(239,68,68,0.08)',
                            color: couponResult.valid ? '#10b981' : '#ef4444',
                          }}>
                          {couponResult.valid
                            ? `${t('tpl_detail.coupon_valid')} ${couponResult.discountLabel || ''}`
                            : `${t('tpl_detail.coupon_invalid')} ${couponResult.message}`}
                        </p>
                      )}
                    </div>

                    {/* Affiliate */}
                    <div>
                      <p className="text-[11px] font-semibold mb-1.5 uppercase tracking-wide" style={{ color: 'var(--text-muted)' }}>
                        {t('tpl_detail.aff_lbl')}
                        {getAffiliateCode() && (
                          <span className="ml-2 normal-case font-normal" style={{ color: '#10b981' }}>
                            {t('tpl_detail.aff_auto')}
                          </span>
                        )}
                      </p>
                      <input
                        className="w-full px-3 py-2 rounded-lg text-xs font-mono uppercase focus:outline-none"
                        style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)', color: 'var(--text-primary)' }}
                        placeholder="VD: NGUY1234"
                        value={affiliateCode}
                        onChange={e => setAffiliateCode(e.target.value.toUpperCase())}
                        onFocus={e => e.target.style.borderColor = '#8b5cf6'}
                        onBlur={e => e.target.style.borderColor = 'var(--border)'}
                      />
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* CTA */}
            <div className="space-y-2">
              {template.isFree || template.isPurchased ? (
                <button onClick={handleDownload} disabled={downloadLoading}
                  className="w-full flex items-center justify-center gap-2 py-3 rounded-xl font-bold text-sm transition-all shadow-md disabled:opacity-50"
                  style={{ backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)' }}>
                  {downloadLoading
                    ? <><Spinner /> {t('tpl_detail.downloading')}</>
                    : t('tpl_detail.download_btn')}
                </button>
              ) : (
                <button onClick={handleBuy} disabled={orderLoading}
                  className="w-full flex items-center justify-center gap-2 py-3 rounded-xl font-bold text-sm transition-all shadow-md disabled:opacity-50"
                  style={{ backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)' }}>
                  {orderLoading
                    ? <><Spinner /> {t('tpl_detail.buy_loading')}</>
                    : t('tpl_detail.buy_btn')}
                </button>
              )}

              <button onClick={handleWishlist} disabled={wishlistLoading}
                className="w-full flex items-center justify-center gap-2 py-2.5 rounded-xl font-bold text-sm transition-all"
                style={{ border: '2px solid var(--border)', color: 'var(--text-secondary)' }}
                onMouseEnter={e => e.currentTarget.style.borderColor = 'var(--text-muted)'}
                onMouseLeave={e => e.currentTarget.style.borderColor = 'var(--border)'}
              >
                {wishlistLoading ? <Spinner /> : (
                  template.isInWishlist
                    ? t('tpl_detail.wishlisted')
                    : t('tpl_detail.add_wish')
                )}
              </button>

              {canPreview && (
                <button onClick={handlePreview}
                  className="w-full flex items-center justify-center gap-2 py-2.5 rounded-xl font-semibold text-sm transition-all"
                  style={{ border: '1px solid var(--border)', color: 'var(--text-muted)' }}
                  onMouseEnter={e => e.currentTarget.style.color = 'var(--text-primary)'}
                  onMouseLeave={e => e.currentTarget.style.color = 'var(--text-muted)'}
                >
                  {template.previewType === 'ExternalUrl'
                    ? t('tpl_detail.live_demo')
                    : t('tpl_detail.preview')}
                </button>
              )}
            </div>

            {template.isPurchased && (
              <div className="mt-3 flex items-center justify-center gap-1.5">
                <span className="w-4 h-4 rounded-full bg-emerald-500/10 flex items-center justify-center text-[10px]">✓</span>
                <span className="text-xs text-emerald-500 font-semibold">{t('tpl_detail.purchased')}</span>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Preview modal */}
      {template.previewType === 'Iframe' && (
        <PreviewModal
          open={previewModal}
          onClose={() => setPreviewModal(false)}
          title={template.name}
          templateId={template.id}
        />
      )}

      {/* Review modal */}
      {reviewModal && (
        <Portal>
          <div className="fixed inset-0 bg-black/50 backdrop-blur-sm" style={{ zIndex: 200 }}
            onClick={() => setReviewModal(false)} />
          <div className="fixed inset-0 flex items-start justify-center p-4 pt-20 lg:items-center lg:pt-4 pointer-events-none"
            style={{ zIndex: 201 }}>
            <div
              className="relative w-full max-w-md rounded-3xl shadow-2xl animate-fade-in pointer-events-auto flex flex-col max-h-[80vh]"
              style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}
              onClick={e => e.stopPropagation()}
            >
              <div className="flex items-center justify-between px-6 py-5 shrink-0"
                style={{ borderBottom: '1px solid var(--border)' }}>
                <h3 className="font-black text-base tracking-tight" style={{ color: 'var(--text-primary)' }}>
                  {t('tpl_detail.review_title')}
                </h3>
                <button onClick={() => setReviewModal(false)}
                  className="w-8 h-8 rounded-lg flex items-center justify-center text-xl leading-none"
                  style={{ backgroundColor: 'var(--bg-elevated)', color: 'var(--text-secondary)' }}>×</button>
              </div>
              <div className="p-6 overflow-y-auto">
                <form onSubmit={handleSubmitReview} className="space-y-4">
                  <FormField label={t('tpl_detail.review_star')} required>
                    <StarRating value={reviewForm.rating} onChange={v => setReviewForm(f => ({ ...f, rating: v }))} />
                  </FormField>
                  <FormField label={t('tpl_detail.review_title_lbl')}>
                    <input className="w-full px-4 py-3 rounded-xl text-sm focus:outline-none transition-all"
                      style={inputStyle}
                      placeholder={t('tpl_detail.review_title_ph')}
                      value={reviewForm.title}
                      onChange={e => setReviewForm(f => ({ ...f, title: e.target.value }))}
                      onFocus={e => e.target.style.borderColor = '#0ea5e9'}
                      onBlur={e => e.target.style.borderColor = 'var(--border)'}
                    />
                  </FormField>
                  <FormField label={t('tpl_detail.review_comment')}>
                    <textarea className="w-full px-4 py-3 rounded-xl text-sm focus:outline-none transition-all resize-none"
                      style={inputStyle}
                      rows={4}
                      placeholder={t('tpl_detail.review_ph')}
                      value={reviewForm.comment}
                      onChange={e => setReviewForm(f => ({ ...f, comment: e.target.value }))}
                      onFocus={e => e.target.style.borderColor = '#0ea5e9'}
                      onBlur={e => e.target.style.borderColor = 'var(--border)'}
                    />
                  </FormField>
                  <div className="flex gap-2 justify-end pt-1">
                    <button type="button" onClick={() => setReviewModal(false)}
                      className="px-4 py-2 rounded-xl text-sm font-semibold transition-all"
                      style={{ border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
                      {t('common.cancel')}
                    </button>
                    <button type="submit" disabled={reviewLoading}
                      className="flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-bold transition-all disabled:opacity-50"
                      style={{ backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)' }}>
                      {reviewLoading
                        ? <><Spinner /> {t('tpl_detail.review_sending')}</>
                        : t('tpl_detail.review_submit')}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </Portal>
      )}
    </div>
  );
}

// ── Stacked 3D Gallery ──────────────────────────────────────────────────────

function StackedGallery({ images, templateName, hintText }) {
  const [active, setActive] = useState(0);
  const [lightbox, setLightbox] = useState(false);
  const [dragging, setDragging] = useState(false);
  const dragStartX = useRef(0);
  const dragDelta = useRef(0);

  if (!images || images.length === 0) return (
    <div className="aspect-video rounded-2xl flex items-center justify-center text-6xl"
      style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-muted)' }}>
      🖼️
    </div>
  );

  const total = images.length;
  const prev = () => setActive(a => (a - 1 + total) % total);
  const next = () => setActive(a => (a + 1) % total);

  const onDragStart = (e) => {
    setDragging(false);
    dragDelta.current = 0;
    dragStartX.current = e.type === 'touchstart' ? e.touches[0].clientX : e.clientX;
  };
  const onDragMove = (e) => {
    const x = e.type === 'touchmove' ? e.touches[0].clientX : e.clientX;
    dragDelta.current = x - dragStartX.current;
    if (Math.abs(dragDelta.current) > 5) setDragging(true);
  };
  const onDragEnd = () => {
    if (Math.abs(dragDelta.current) > 50) {
      dragDelta.current > 0 ? prev() : next();
    }
    setTimeout(() => setDragging(false), 0);
  };

  const getCardStyle = (i) => {
    const offset = ((i - active) % total + total) % total;
    if (offset === 0) return {
      transform: 'translateX(0) translateY(0) rotate(0deg) scale(1)',
      zIndex: 10, opacity: 1, filter: 'none',
    };
    if (offset === 1 || offset === total - 1) {
      const dir = offset === 1 ? 1 : -1;
      return {
        transform: `translateX(${dir * 18}px) translateY(8px) rotate(${dir * 2.5}deg) scale(0.95)`,
        zIndex: 7, opacity: 0.7, filter: 'blur(0.5px)',
      };
    }
    if (offset === 2 || offset === total - 2) {
      const dir = offset === 2 ? 1 : -1;
      return {
        transform: `translateX(${dir * 30}px) translateY(16px) rotate(${dir * 5}deg) scale(0.9)`,
        zIndex: 4, opacity: 0.4, filter: 'blur(1px)',
      };
    }
    return { transform: 'translateY(20px) scale(0.85)', zIndex: 1, opacity: 0, filter: 'blur(2px)' };
  };

  return (
    <>
      <div className="relative select-none" style={{ paddingBottom: '32px' }}>
        <div
          className="relative w-full cursor-grab active:cursor-grabbing"
          style={{ aspectRatio: '16/9' }}
          onMouseDown={onDragStart}
          onMouseMove={onDragMove}
          onMouseUp={onDragEnd}
          onMouseLeave={onDragEnd}
          onTouchStart={onDragStart}
          onTouchMove={onDragMove}
          onTouchEnd={onDragEnd}
        >
          {images.map((img, i) => {
            const style = getCardStyle(i);
            const isActive = i === active;
            return (
              <div
                key={i}
                onClick={() => { if (!dragging) { if (isActive) setLightbox(true); else setActive(i); } }}
                style={{
                  position: 'absolute', inset: 0,
                  borderRadius: '16px', overflow: 'hidden',
                  transition: 'transform 0.45s cubic-bezier(0.34,1.56,0.64,1), opacity 0.4s ease, filter 0.4s ease',
                  border: isActive ? '1px solid rgba(255,255,255,0.12)' : '1px solid var(--border)',
                  boxShadow: isActive
                    ? '0 24px 64px rgba(0,0,0,0.45), 0 0 0 1px rgba(255,255,255,0.06)'
                    : '0 8px 24px rgba(0,0,0,0.3)',
                  cursor: isActive ? 'zoom-in' : 'pointer',
                  ...style,
                }}
              >
                <img
                  src={img.url}
                  alt={img.alt || templateName}
                  draggable={false}
                  style={{ width: '100%', height: '100%', objectFit: 'cover', pointerEvents: 'none' }}
                />
                {isActive && (
                  <>
                    <div style={{ position: 'absolute', top: 12, right: 12, display: 'flex', alignItems: 'center', gap: 6 }}>
                      <span style={{
                        padding: '3px 10px', borderRadius: '999px', fontSize: '11px', fontWeight: 700,
                        backgroundColor: 'rgba(0,0,0,0.55)', backdropFilter: 'blur(8px)',
                        color: '#fff', letterSpacing: '0.05em',
                      }}>
                        {active + 1} / {total}
                      </span>
                    </div>
                    <div style={{
                      position: 'absolute', bottom: 12, left: '50%', transform: 'translateX(-50%)',
                      padding: '4px 12px', borderRadius: '999px', fontSize: '11px', fontWeight: 600,
                      backgroundColor: 'rgba(0,0,0,0.5)', backdropFilter: 'blur(8px)',
                      color: 'rgba(255,255,255,0.7)', letterSpacing: '0.04em',
                      pointerEvents: 'none', whiteSpace: 'nowrap',
                    }}>
                      {hintText}
                    </div>
                  </>
                )}
              </div>
            );
          })}
        </div>

        {total > 1 && (
          <>
            <button onClick={prev} style={{
              position: 'absolute', left: -16, top: '50%', transform: 'translateY(-60%)',
              width: 36, height: 36, borderRadius: '50%', border: '1px solid var(--border)',
              backgroundColor: 'var(--bg-card)', color: 'var(--text-primary)',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              fontSize: 14, cursor: 'pointer', zIndex: 20,
              boxShadow: '0 4px 16px rgba(0,0,0,0.3)', transition: 'transform 0.2s',
            }}
              onMouseEnter={e => e.currentTarget.style.transform = 'translateY(-60%) scale(1.1)'}
              onMouseLeave={e => e.currentTarget.style.transform = 'translateY(-60%) scale(1)'}
            >←</button>
            <button onClick={next} style={{
              position: 'absolute', right: -16, top: '50%', transform: 'translateY(-60%)',
              width: 36, height: 36, borderRadius: '50%', border: '1px solid var(--border)',
              backgroundColor: 'var(--bg-card)', color: 'var(--text-primary)',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              fontSize: 14, cursor: 'pointer', zIndex: 20,
              boxShadow: '0 4px 16px rgba(0,0,0,0.3)', transition: 'transform 0.2s',
            }}
              onMouseEnter={e => e.currentTarget.style.transform = 'translateY(-60%) scale(1.1)'}
              onMouseLeave={e => e.currentTarget.style.transform = 'translateY(-60%) scale(1)'}
            >→</button>
          </>
        )}

        {total > 1 && (
          <div style={{
            position: 'absolute', bottom: 0, left: '50%', transform: 'translateX(-50%)',
            display: 'flex', gap: 6, alignItems: 'center',
          }}>
            {images.map((_, i) => (
              <button key={i} onClick={() => setActive(i)} style={{
                width: i === active ? 20 : 6, height: 6,
                borderRadius: 999, border: 'none', padding: 0, cursor: 'pointer',
                backgroundColor: i === active ? 'var(--text-primary)' : 'var(--border)',
                transition: 'all 0.35s cubic-bezier(0.34,1.56,0.64,1)',
              }} />
            ))}
          </div>
        )}
      </div>

      {/* Lightbox */}
      {lightbox && (
        <Portal>
          <div
            onClick={() => setLightbox(false)}
            style={{
              position: 'fixed', inset: 0, zIndex: 999,
              backgroundColor: 'rgba(0,0,0,0.92)', backdropFilter: 'blur(12px)',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              padding: '20px', animation: 'fadeIn 0.2s ease',
            }}
          >
            <style>{`@keyframes fadeIn{from{opacity:0}to{opacity:1}} @keyframes scaleIn{from{transform:scale(0.92);opacity:0}to{transform:scale(1);opacity:1}}`}</style>

            <button onClick={() => setLightbox(false)} style={{
              position: 'absolute', top: 20, right: 20,
              width: 40, height: 40, borderRadius: '50%',
              backgroundColor: 'rgba(255,255,255,0.1)', border: '1px solid rgba(255,255,255,0.15)',
              color: '#fff', fontSize: 20, cursor: 'pointer', zIndex: 1001,
              display: 'flex', alignItems: 'center', justifyContent: 'center',
            }}>×</button>

            {total > 1 && <>
              <button onClick={(e) => { e.stopPropagation(); prev(); }} style={{
                position: 'absolute', left: 20, top: '50%', transform: 'translateY(-50%)',
                width: 44, height: 44, borderRadius: '50%',
                backgroundColor: 'rgba(255,255,255,0.1)', border: '1px solid rgba(255,255,255,0.15)',
                color: '#fff', fontSize: 18, cursor: 'pointer', zIndex: 1001,
                display: 'flex', alignItems: 'center', justifyContent: 'center',
              }}>←</button>
              <button onClick={(e) => { e.stopPropagation(); next(); }} style={{
                position: 'absolute', right: 20, top: '50%', transform: 'translateY(-50%)',
                width: 44, height: 44, borderRadius: '50%',
                backgroundColor: 'rgba(255,255,255,0.1)', border: '1px solid rgba(255,255,255,0.15)',
                color: '#fff', fontSize: 18, cursor: 'pointer', zIndex: 1001,
                display: 'flex', alignItems: 'center', justifyContent: 'center',
              }}>→</button>
            </>}

            <img
              src={images[active].url}
              alt={images[active].alt || templateName}
              onClick={e => e.stopPropagation()}
              style={{
                maxWidth: '90vw', maxHeight: '85vh',
                borderRadius: 16, objectFit: 'contain',
                boxShadow: '0 40px 120px rgba(0,0,0,0.8)',
                animation: 'scaleIn 0.3s cubic-bezier(0.34,1.56,0.64,1)',
              }}
            />

            <div style={{
              position: 'absolute', bottom: 24, left: '50%', transform: 'translateX(-50%)',
              padding: '6px 16px', borderRadius: 999, fontSize: 12, fontWeight: 700,
              backgroundColor: 'rgba(255,255,255,0.1)', color: '#fff',
              backdropFilter: 'blur(8px)', letterSpacing: '0.08em',
            }}>
              {active + 1} / {total}
            </div>
          </div>
        </Portal>
      )}
    </>
  );
}