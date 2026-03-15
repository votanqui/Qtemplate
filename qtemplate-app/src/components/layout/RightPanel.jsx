import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { publicApi } from '../../api/services';
import { toAbsoluteUrl } from '../../api/client';
import { useLang } from '../../context/Langcontext';

export function RightPanel() {
  const { isVi } = useLang();
  const [banners, setBanners] = useState([]);
  const [bannerIdx, setBannerIdx] = useState(0);

  useEffect(() => {
    publicApi.getBanners('Sidebar')
      .then(res => setBanners(res.data.data || []))
      .catch(() => {});
  }, []);

  // Auto-rotate
  useEffect(() => {
    if (banners.length < 2) return;
    const id = setInterval(() => setBannerIdx(i => (i + 1) % banners.length), 4000);
    return () => clearInterval(id);
  }, [banners.length]);

  const quickLinks = [
    { href: '/templates',                label: isVi ? '🗂️ Tất cả template' : '🗂️ All templates' },
    { href: '/sale',                     label: isVi ? '🔥 Đang giảm giá'   : '🔥 On sale'       },
    { href: '/templates?sortBy=newest',  label: isVi ? '✨ Mới nhất'         : '✨ Newest'        },
    { href: '/templates?sortBy=popular', label: isVi ? '⭐ Phổ biến'         : '⭐ Popular'       },
  ];

  return (
    <aside
      className="hidden xl:flex flex-col shrink-0 overflow-y-auto sticky top-0 h-screen"
      style={{
        width: 220,
        borderLeft: '1px solid var(--border)',
        backgroundColor: 'var(--bg-page)',
        padding: '1.25rem 0.875rem',
        gap: '1.25rem',
      }}
    >

      {/* ── Banner Slider ── */}
      {banners.length > 0 && (
        <div className="flex flex-col gap-2">

          {/* Ảnh */}
          <div className="relative rounded-xl overflow-hidden"
            style={{ height: 400, border: '1px solid var(--border)' }}>
            {banners.map((b, i) => (
              <div key={b.id}
                className="absolute inset-0 transition-opacity duration-700"
                style={{ opacity: i === bannerIdx ? 1 : 0, pointerEvents: i === bannerIdx ? 'auto' : 'none' }}>
                {b.imageUrl ? (
                  <img src={toAbsoluteUrl(b.imageUrl)} alt={b.title}
                    className="w-full h-full object-cover" />
                ) : (
                  <div className="w-full h-full flex flex-col items-center justify-center gap-2 p-3 text-center"
                    style={{ background: 'linear-gradient(135deg, rgba(124,58,237,0.08), rgba(14,165,233,0.06))' }}>
                    <p className="text-xs font-bold" style={{ color: 'var(--text-primary)' }}>{b.title}</p>
                    {b.subTitle && <p className="text-[10px]" style={{ color: 'var(--text-muted)' }}>{b.subTitle}</p>}
                  </div>
                )}
                {banners.length > 1 && (
                  <div className="absolute bottom-2 left-1/2 -translate-x-1/2 flex gap-1">
                    {banners.map((_, j) => (
                      <button key={j} onClick={() => setBannerIdx(j)}
                        className="rounded-full transition-all"
                        style={{
                          width: j === bannerIdx ? 14 : 5, height: 5,
                          backgroundColor: j === bannerIdx ? '#fff' : 'rgba(255,255,255,0.4)',
                        }} />
                    ))}
                  </div>
                )}
              </div>
            ))}
          </div>

          {/* Thông tin bên dưới ảnh */}
          {(() => {
            const b = banners[bannerIdx];
            if (!b) return null;
            return (
              <div className="rounded-xl p-3 flex flex-col gap-1.5"
                style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>
                <p className="text-xs font-bold leading-snug line-clamp-2"
                  style={{ color: 'var(--text-primary)' }}>{b.title}</p>
                {b.subTitle && (
                  <p className="text-[10px] leading-relaxed line-clamp-2"
                    style={{ color: 'var(--text-muted)' }}>{b.subTitle}</p>
                )}
                {b.linkUrl && (
                  <Link to={b.linkUrl}
                    className="mt-0.5 inline-flex items-center gap-1 text-[10px] font-black transition-opacity hover:opacity-75"
                    style={{ color: '#7c3aed' }}>
                    {isVi ? 'Xem ngay' : 'View now'} →
                  </Link>
                )}
              </div>
            );
          })()}
        </div>
      )}

      {/* ── Placeholder khi không có banner ── */}
      {banners.length === 0 && (
        <div className="rounded-xl overflow-hidden"
          style={{ border: '1px solid var(--border)', background: 'linear-gradient(135deg, rgba(124,58,237,0.05), rgba(14,165,233,0.03))' }}>
          <div className="p-4 text-center">
            <div className="text-2xl mb-2">🚀</div>
            <p className="text-xs font-bold mb-1" style={{ color: 'var(--text-primary)' }}>
              {isVi ? 'Template cao cấp' : 'Premium templates'}
            </p>
            <p className="text-[10px] mb-3" style={{ color: 'var(--text-muted)' }}>
              {isVi ? 'Hàng trăm lựa chọn chất lượng' : 'Hundreds of quality choices'}
            </p>
            <Link to="/templates"
              className="inline-block px-3 py-1.5 rounded-lg text-[10px] font-black text-white"
              style={{ background: 'linear-gradient(135deg, #7c3aed, #0ea5e9)' }}>
              {isVi ? 'Khám phá ngay' : 'Browse now'}
            </Link>
          </div>
        </div>
      )}

      {/* ── Khám phá ── */}
      <div>
        <p className="text-[10px] font-black uppercase tracking-widest mb-2"
          style={{ color: 'var(--text-muted)' }}>
          {isVi ? 'Khám phá' : 'Explore'}
        </p>
        <nav className="flex flex-col gap-0.5">
          {quickLinks.map(lk => (
            <Link key={lk.href} to={lk.href}
              className="flex items-center gap-2 px-2.5 py-2 rounded-lg text-xs font-medium transition-all"
              style={{ color: 'var(--text-secondary)' }}
              onMouseEnter={e => {
                e.currentTarget.style.backgroundColor = 'var(--bg-elevated)';
                e.currentTarget.style.color = 'var(--text-primary)';
              }}
              onMouseLeave={e => {
                e.currentTarget.style.backgroundColor = 'transparent';
                e.currentTarget.style.color = 'var(--text-secondary)';
              }}>
              {lk.label}
            </Link>
          ))}
        </nav>
      </div>

    </aside>
  );
}