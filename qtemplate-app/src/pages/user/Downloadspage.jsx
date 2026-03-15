import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { userApi } from '../../api/services';
import { extractError, toAbsoluteUrl } from '../../api/client';
import { LoadingPage, Pagination, EmptyState, useToast } from '../../components/ui';
import { useLang } from '../../context/Langcontext';

export default function DownloadsPage() {
  const { t } = useLang();
  const toast = useToast();
  const [data, setData] = useState(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    setLoading(true);
    userApi.getDownloads(page)
      .then(res => setData(res.data.data))
      .catch(err => toast.error(extractError(err)))
      .finally(() => setLoading(false));
  }, [page]);

  if (loading) return <LoadingPage />;

  return (
    <div className="animate-fade-in">

      {/* Header */}
      <div className="mb-7">
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-semibold uppercase tracking-wider mb-3"
          style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
          ⬇️ {t('dl.title')}
        </div>
        <h1 className="text-2xl font-black tracking-tight" style={{ color: 'var(--text-primary)' }}>
          {t('dl.history_title')}
        </h1>
        <p className="text-sm mt-1" style={{ color: 'var(--text-muted)' }}>
          {t('dl.total')}{' '}
          <span className="font-bold" style={{ color: 'var(--text-secondary)' }}>
            {data?.totalCount || 0}
          </span>{' '}
          {t('dl.total_suffix')}
        </p>
      </div>

      {!data?.items?.length ? (
        <EmptyState
          icon="⬇️"
          title={t('dl.empty')}
          description={t('dl.empty_desc')}
        />
      ) : (
        <div className="rounded-2xl shadow-sm overflow-hidden"
          style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>

          {/* Table header */}
          <div className="grid grid-cols-[1fr_80px_150px_110px] gap-4 px-5 py-3"
            style={{ backgroundColor: 'var(--bg-elevated)', borderBottom: '1px solid var(--border)' }}>
            {[
              ['Template',           ''],
              [t('dl.col_count'),    'text-center'],
              [t('dl.col_last'),     'text-right'],
              [t('dl.col_action'),   'text-right'],
            ].map(([h, cls]) => (
              <span key={h} className={`text-xs font-bold uppercase tracking-widest ${cls}`}
                style={{ color: 'var(--text-muted)' }}>{h}</span>
            ))}
          </div>

          {/* Rows */}
          <div>
            {data.items.map((item, idx) => (
              <div key={item.templateId}>
                {idx > 0 && <div style={{ borderTop: '1px solid var(--border)' }} />}
                <div
                  className="grid grid-cols-[1fr_80px_150px_110px] gap-4 items-center px-5 py-4 transition-colors"
                  onMouseEnter={e => e.currentTarget.style.backgroundColor = 'var(--bg-elevated)'}
                  onMouseLeave={e => e.currentTarget.style.backgroundColor = ''}
                >
                  {/* Template info */}
                  <div className="flex items-center gap-3 min-w-0">
                    <div className="w-14 h-10 rounded-xl overflow-hidden shrink-0 flex items-center justify-center"
                      style={{ border: '1px solid var(--border)', backgroundColor: 'var(--bg-elevated)' }}>
                      {item.thumbnailUrl
                        ? <img src={toAbsoluteUrl(item.thumbnailUrl)} alt={item.templateName}
                            className="w-full h-full object-cover" />
                        : <span className="text-base">🖼️</span>
                      }
                    </div>
                    <div className="min-w-0">
                      <p className="font-semibold text-sm truncate" style={{ color: 'var(--text-primary)' }}>
                        {item.templateName}
                      </p>
                      <p className="text-[11px] mt-0.5 font-mono" style={{ color: 'var(--text-muted)' }}>
                        {item.templateId?.toString().slice(0, 8)}...
                      </p>
                    </div>
                  </div>

                  {/* Download count */}
                  <div className="text-center">
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold"
                      style={{ backgroundColor: 'rgba(139,92,246,0.1)', border: '1px solid rgba(139,92,246,0.2)', color: '#7c3aed' }}>
                      {item.downloadCount}×
                    </span>
                  </div>

                  {/* Last download date */}
                  <div className="text-right text-xs whitespace-nowrap" style={{ color: 'var(--text-muted)' }}>
                    {item.lastDownloadAt
                      ? new Date(item.lastDownloadAt).toLocaleString('vi-VN')
                      : '—'}
                  </div>

                  {/* Action */}
                  <div className="flex justify-end">
                    {item.slug ? (
                      <button
                        onClick={() => navigate(`/templates/${item.slug}`)}
                        className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-bold transition-all"
                        style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }}
                        onMouseEnter={e => { e.currentTarget.style.borderColor = '#7c3aed'; e.currentTarget.style.color = '#7c3aed'; }}
                        onMouseLeave={e => { e.currentTarget.style.borderColor = 'var(--border)'; e.currentTarget.style.color = 'var(--text-secondary)'; }}
                      >
                        {t('dl.view_btn')}
                      </button>
                    ) : (
                      <span className="text-xs" style={{ color: 'var(--text-muted)' }}>—</span>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      <Pagination page={page} totalPages={data?.totalPages || 1} onPageChange={setPage} />
    </div>
  );
}