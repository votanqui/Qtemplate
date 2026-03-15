import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { userApi } from '../../api/services';
import { extractError } from '../../api/client';
import { LoadingPage, Alert, Pagination, EmptyState, Spinner } from '../../components/ui';
import { useNotification } from '../../context/NotificationContext';
import { useLang } from '../../context/Langcontext';
const typeConfig = {
  Success: { icon: '✅', color: '#10b981' },
  Warning: { icon: '⚠️', color: '#f59e0b' },
  Info:    { icon: '🔔', color: '#3b82f6' },
  Error:   { icon: '❌', color: '#ef4444' },
  OrderPaid:      { icon: '💳', color: '#10b981' },
  OrderCancelled: { icon: '🚫', color: '#ef4444' },
  TicketReply:    { icon: '💬', color: '#8b5cf6' },
};
const getConf = (type) => typeConfig[type] || { icon: '📢', color: '#64748b' };

export default function NotificationsPage() {
    const { t } = useLang();   
  const navigate = useNavigate();
  const { decrementUnread, clearUnread } = useNotification();
  const [data, setData] = useState(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [unreadOnly, setUnreadOnly] = useState(false);
  const [markingAll, setMarkingAll] = useState(false);
  const [markingOne, setMarkingOne] = useState({});

  const fetchData = () => {
    setLoading(true);
    userApi.getNotifications(page, 20, unreadOnly || null)
      .then(res => setData(res.data.data))
      .catch(err => setError(extractError(err)))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchData(); }, [page, unreadOnly]);


const handleMarkOne = async (id) => {
  setMarkingOne(m => ({ ...m, [id]: true }));
  try {
    await userApi.markNotificationRead(id);
    setData(d => ({ ...d, items: d.items.map(n => n.id === id ? { ...n, isRead: true } : n) }));
    decrementUnread(1); // ← cập nhật badge
  } catch {}
  finally { setMarkingOne(m => ({ ...m, [id]: false })); }
};

const handleMarkAll = async () => {
  setMarkingAll(true);
  try {
    await userApi.markAllNotificationsRead();
    clearUnread(); // ← reset badge về 0
    fetchData();
  } catch {}
  finally { setMarkingAll(false); }
};
  const handleClick = async (notif) => {
    if (!notif.isRead) await handleMarkOne(notif.id);
    if (notif.redirectUrl) navigate(notif.redirectUrl);
  };

  const unreadCount = data?.items?.filter(n => !n.isRead).length || 0;
  if (loading && !data) return <LoadingPage />;

  return (
  <div className="animate-fade-in pb-6">
      <div className="flex items-start justify-between mb-6 gap-4">
        <div>
          <h1 className="text-2xl font-black tracking-tight mb-1" style={{ color: 'var(--text-primary)' }}>
            {t('notif.title')}
          </h1>
          <p className="text-sm font-medium" style={{ color: 'var(--text-secondary)' }}>
            {data?.totalCount || 0} {t('notif.total')}
            {unreadCount > 0 && (
              <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-bold"
                style={{ backgroundColor: 'rgba(59,130,246,0.15)', color: '#60a5fa', border: '1px solid rgba(59,130,246,0.25)' }}>
              {unreadCount} {t('notif.unread_count')}
              </span>
            )}
          </p>
        </div>
        <div className="flex items-center gap-2 shrink-0">
<label
  className="flex items-center gap-2 text-sm font-medium cursor-pointer select-none"
  style={{ color: 'var(--text-secondary)' }}
>
  <input
    type="checkbox"
    checked={unreadOnly}
    onChange={e => { setUnreadOnly(e.target.checked); setPage(1); }}
    className="w-4 h-4 rounded"
  />
  {t('notif.unread_only')}
</label>
          <button onClick={handleMarkAll} disabled={markingAll}
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold border transition-all disabled:opacity-50"
            style={{ backgroundColor: 'var(--bg-elevated)', borderColor: 'var(--border)', color: 'var(--text-primary)' }}>
          {markingAll ? <Spinner /> : <>{t('notif.read_all_btn')}</>}
          </button>
        </div>
      </div>

      {error && <Alert type="error">{error}</Alert>}

      {loading ? <LoadingPage /> : data?.items?.length === 0 ? (
      <EmptyState icon="🔔" title={t('notif.empty_title')}
  description={unreadOnly ? t('notif.empty_unread') : t('notif.empty_all')} />
      ) : (
        <div className="space-y-2">
          {data.items.map(notif => {
            const conf = getConf(notif.type);
            return (
              <div key={notif.id} onClick={() => handleClick(notif)}
                className="rounded-2xl border transition-all cursor-pointer group"
                style={{
                  backgroundColor: notif.isRead ? 'var(--bg-card)' : 'var(--bg-elevated)',
                  borderColor: notif.isRead ? 'var(--border)' : `${conf.color}44`,
                  padding: '14px 16px',
                }}>
                <div className="flex items-start gap-3">
                  <div className="w-9 h-9 rounded-xl flex items-center justify-center text-base shrink-0"
                    style={{ backgroundColor: `${conf.color}18`, border: `1px solid ${conf.color}30` }}>
                    {conf.icon}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-start justify-between gap-2 mb-1">
                      <p className="text-sm font-bold leading-snug"
                        style={{ color: notif.isRead ? 'var(--text-secondary)' : 'var(--text-primary)' }}>
                        {notif.title}
                      </p>
                      {!notif.isRead && (
                        <span className="w-2 h-2 rounded-full shrink-0 mt-1.5"
                          style={{ backgroundColor: conf.color }} />
                      )}
                    </div>
                    <p className="text-sm leading-relaxed"
                      style={{ color: notif.isRead ? 'var(--text-muted)' : 'var(--text-secondary)' }}>
                      {notif.message}
                    </p>
                    <div className="flex items-center gap-3 mt-2">
                      <span className="text-xs" style={{ color: 'var(--text-muted)' }}>
                        {new Date(notif.createdAt).toLocaleString('vi-VN', {
                          day: '2-digit', month: '2-digit', year: 'numeric',
                          hour: '2-digit', minute: '2-digit',
                        })}
                      </span>
                      {!notif.isRead && (
                        <button onClick={e => { e.stopPropagation(); handleMarkOne(notif.id); }}
                          disabled={markingOne[notif.id]}
                          className="text-xs font-semibold transition-opacity hover:opacity-70"
                          style={{ color: conf.color }}>
                         {markingOne[notif.id] ? <Spinner /> : t('notif.mark_one_read')}
                        </button>
                      )}
                      {notif.redirectUrl && (
                        <span className="text-xs font-semibold opacity-0 group-hover:opacity-60 transition-opacity"
                          style={{ color: conf.color }}>
                          {t('notif.view_detail')}
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
      <Pagination page={page} totalPages={data?.totalPages || 1} onPageChange={setPage} />
    </div>
  );
}