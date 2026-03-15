import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { userApi } from '../../api/services';
import { toAbsoluteUrl } from '../../api/client';
import { LoadingPage, Pagination, Price, EmptyState } from '../../components/ui';
import { useLang } from '../../context/Langcontext';

export default function WishlistPage() {
  const { t } = useLang();
  const [data, setData] = useState(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [removing, setRemoving] = useState({});

  const fetchWishlist = () => {
    setLoading(true);
    userApi.getWishlist(page)
      .then(res => setData(res.data.data))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchWishlist(); }, [page]);

  const handleRemove = async (e, templateId) => {
    e.preventDefault();
    e.stopPropagation();
    setRemoving(r => ({ ...r, [templateId]: true }));
    try {
      await userApi.toggleWishlist(templateId);
      fetchWishlist();
    } finally {
      setRemoving(r => ({ ...r, [templateId]: false }));
    }
  };

  if (loading) return <LoadingPage />;

  return (
    <div className="animate-fade-in">

      {/* Header */}
      <div className="mb-7">
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-semibold uppercase tracking-wider mb-3"
          style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
          ❤️ {t('wish.title')}
        </div>
        <h1 className="text-2xl font-black tracking-tight" style={{ color: 'var(--text-primary)' }}>
          {t('wish.list_title')}
        </h1>
        <p className="text-sm mt-1" style={{ color: 'var(--text-muted)' }}>
          <span className="font-bold" style={{ color: 'var(--text-secondary)' }}>{data?.totalCount || 0}</span>{' '}
          {t('wish.total')}
        </p>
      </div>

      {!data?.items?.length ? (
        <EmptyState
          icon="❤️"
          title={t('wish.empty')}
          description={
            <Link to="/templates" className="text-violet-600 font-semibold hover:underline">
              {t('wish.explore_btn')}
            </Link>
          }
        />
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {data.items.map(item => (
            <Link
              key={item.id}
              to={`/templates/${item.templateSlug}`}
              className="border rounded-2xl overflow-hidden shadow-sm hover:shadow-md transition-all group block"
              style={{ backgroundColor: 'var(--bg-card)', borderColor: 'var(--border)' }}
              onMouseEnter={e => e.currentTarget.style.borderColor = 'var(--border-light, #94a3b8)'}
              onMouseLeave={e => e.currentTarget.style.borderColor = 'var(--border)'}
            >
              {/* Thumbnail */}
              <div className="aspect-video overflow-hidden relative"
                style={{ backgroundColor: 'var(--bg-elevated)' }}>
                {item.thumbnailUrl
                  ? <img
                      src={toAbsoluteUrl(item.thumbnailUrl)}
                      alt={item.templateName}
                      className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                    />
                  : <div className="w-full h-full flex items-center justify-center text-4xl" style={{ color: 'var(--text-muted)' }}>🖼️</div>
                }
                {/* Hover overlay */}
                <div className="absolute inset-0 bg-black/0 group-hover:bg-black/10 transition-colors duration-200 flex items-center justify-center pointer-events-none">
                  <span className="opacity-0 group-hover:opacity-100 transition-opacity duration-200 bg-white/90 backdrop-blur-sm text-gray-900 text-xs font-bold px-3 py-1.5 rounded-full shadow-md">
                    {t('wish.view_detail')}
                  </span>
                </div>
              </div>

              {/* Card body */}
              <div className="p-4">
                <p className="font-bold text-sm mb-3 leading-snug line-clamp-2 transition-colors"
                  style={{ color: 'var(--text-primary)' }}>
                  {item.templateName}
                </p>

                <div className="flex items-center justify-between">
                  {/* Price */}
                  <div>
                    {item.isFree ? (
                      <span className="text-emerald-500 text-sm font-bold">{t('tpl.free_label')}</span>
                    ) : item.salePrice ? (
                      <div className="flex items-center gap-1.5">
                        <Price amount={item.salePrice} className="font-bold text-sm" style={{ color: 'var(--text-primary)' }} />
                        <Price amount={item.price} className="text-xs line-through" style={{ color: 'var(--text-muted)' }} />
                      </div>
                    ) : (
                      <Price amount={item.price} className="font-bold text-sm" style={{ color: 'var(--text-primary)' }} />
                    )}
                  </div>

                  {/* Remove button */}
                  <button
                    onClick={(e) => handleRemove(e, item.templateId)}
                    disabled={removing[item.templateId]}
                    className="flex items-center gap-1 px-2.5 py-1 rounded-lg border border-red-200 text-xs font-semibold text-red-500 hover:bg-red-50 hover:border-red-400 transition-all disabled:opacity-50 relative z-10"
                  >
                    {removing[item.templateId]
                      ? <span className="inline-block w-3 h-3 border border-red-400 border-t-transparent rounded-full animate-spin" />
                      : t('wish.remove_btn')}
                  </button>
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}

      <Pagination page={page} totalPages={data?.totalPages || 1} onPageChange={setPage} />
    </div>
  );
}