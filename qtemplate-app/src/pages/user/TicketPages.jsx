import { useState, useEffect } from 'react';
import { Link, useParams, useNavigate } from 'react-router-dom';
import { ticketApi } from '../../api/services';
import { extractError } from '../../api/client';
import { LoadingPage, Pagination, StatusBadge, PriorityBadge, EmptyState, Spinner, useToast } from '../../components/ui';
import { CreateTicketModal } from '../../modals/user/CreateTicketModal';
import { useLang } from '../../context/Langcontext';

// ── TicketsPage ──────────────────────────────
export function TicketsPage() {
  const { t } = useLang();
  const toast = useToast();
  const [data, setData] = useState(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [createModal, setCreateModal] = useState(false);
  const navigate = useNavigate();

  const fetchTickets = () => {
    setLoading(true);
    ticketApi.getList(page)
      .then(res => setData(res.data.data))
      .catch(err => toast.error(extractError(err), t('ticket.load_err')))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchTickets(); }, [page]);

  if (loading && !data) return <LoadingPage />;

  return (
    <div className="animate-fade-in max-w-6xl">

      {/* Header */}
      <div className="flex items-start justify-between mb-7">
        <div>
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-semibold uppercase tracking-wider mb-3"
            style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
            🎫 {t('ticket.badge_support')}
          </div>
          <h1 className="text-2xl font-black tracking-tight" style={{ color: 'var(--text-primary)' }}>
            {t('ticket.list_title')}
          </h1>
          <p className="text-sm mt-1" style={{ color: 'var(--text-muted)' }}>{data?.totalCount || 0} {t('ticket.count_suffix')}</p>
        </div>
        <button
          onClick={() => setCreateModal(true)}
          className="flex items-center gap-2 px-4 py-2.5 rounded-xl font-bold text-sm transition-all shadow-md mt-1"
          style={{ backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)' }}
        >
          {t('ticket.new_btn')}
        </button>
      </div>

      {!data?.items?.length ? (
        <EmptyState icon="🎫" title={t('ticket.empty')} description={t('ticket.empty_desc')} />
      ) : (
        <div className="rounded-2xl shadow-sm overflow-hidden"
          style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>

          {/* Table head */}
          <div className="grid grid-cols-[1fr_auto_auto_auto] gap-4 px-5 py-3"
            style={{ backgroundColor: 'var(--bg-elevated)', borderBottom: '1px solid var(--border)' }}>
            {[t('ticket.col_ticket'), t('ticket.col_priority'), t('ticket.col_status'), t('ticket.col_created')].map((h, i) => (
              <span key={h} className={`text-xs font-bold uppercase tracking-widest ${i === 3 ? 'text-right' : ''}`}
                style={{ color: 'var(--text-muted)' }}>{h}</span>
            ))}
          </div>

          {/* Rows */}
          <div style={{ borderColor: 'var(--border)' }} className="divide-y">
            {data.items.map(ticket => (
              <div
                key={ticket.id}
                className="grid grid-cols-[1fr_auto_auto_auto] gap-4 items-center px-5 py-4 cursor-pointer transition-colors"
                style={{ borderColor: 'var(--border)' }}
                onClick={() => navigate(`/dashboard/tickets/${ticket.id}`)}
                onMouseEnter={e => e.currentTarget.style.backgroundColor = 'var(--bg-elevated)'}
                onMouseLeave={e => e.currentTarget.style.backgroundColor = ''}
              >
                <div className="min-w-0">
                  <p className="font-mono text-xs font-bold text-violet-500 mb-0.5">{ticket.ticketCode}</p>
                  <p className="font-semibold text-sm truncate" style={{ color: 'var(--text-primary)' }}>{ticket.subject}</p>
                  {ticket.templateName && (
                    <p className="text-xs truncate mt-0.5" style={{ color: 'var(--text-muted)' }}>{ticket.templateName}</p>
                  )}
                </div>
                <div><PriorityBadge priority={ticket.priority} /></div>
                <div><StatusBadge status={ticket.status} /></div>
                <div className="text-xs text-right whitespace-nowrap" style={{ color: 'var(--text-muted)' }}>
                  {new Date(ticket.createdAt).toLocaleDateString('vi-VN')}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      <Pagination page={page} totalPages={data?.totalPages || 1} onPageChange={setPage} />

      <CreateTicketModal
        open={createModal}
        onClose={() => setCreateModal(false)}
        onCreated={(id) => {
          setCreateModal(false);
          fetchTickets();
          toast.success(t('ticket.create_ok'));
          navigate(`/dashboard/tickets/${id}`);
        }}
      />
    </div>
  );
}

// ── TicketDetailPage ─────────────────────────
export function TicketDetailPage() {
  const { id } = useParams();
  const { t } = useLang();
  const toast = useToast();
  const [ticket, setTicket] = useState(null);
  const [loading, setLoading] = useState(true);
  const [replyMsg, setReplyMsg] = useState('');
  const [replyAttach, setReplyAttach] = useState('');
  const [replyLoading, setReplyLoading] = useState(false);

  const fetchTicket = () => {
    ticketApi.getDetail(id)
      .then(res => setTicket(res.data.data))
      .catch(err => toast.error(extractError(err), t('ticket.detail_load_err')))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchTicket(); }, [id]);

  const handleReply = async (e) => {
    e.preventDefault();
    if (!replyMsg.trim()) return;
    setReplyLoading(true);
    try {
      const payload = { message: replyMsg };
      if (replyAttach) payload.attachmentUrl = replyAttach;
      await ticketApi.reply(id, payload);
      setReplyMsg('');
      setReplyAttach('');
      fetchTicket();
      toast.success(t('ticket.reply_ok'));
    } catch (err) {
      toast.error(extractError(err), t('ticket.reply_err'));
    } finally {
      setReplyLoading(false);
    }
  };

  if (loading) return <LoadingPage />;

  return (
    <div className="animate-fade-in ">

      {/* Back */}
      <Link
        to="/dashboard/tickets"
        className="inline-flex items-center gap-1.5 text-sm font-medium mb-5 group transition-colors"
        style={{ color: 'var(--text-muted)' }}
        onMouseEnter={e => e.currentTarget.style.color = 'var(--text-primary)'}
        onMouseLeave={e => e.currentTarget.style.color = 'var(--text-muted)'}
      >
        <span className="w-6 h-6 rounded-lg flex items-center justify-center text-xs transition-colors"
          style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>←</span>
        {t('ticket.back')}
      </Link>

      {/* Ticket header card */}
      <div className="rounded-2xl p-5 shadow-sm mb-4"
        style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>
        <div className="flex items-start justify-between flex-wrap gap-3 mb-4">
          <div>
            <p className="font-mono text-xs font-bold text-violet-500 mb-1">{ticket.ticketCode}</p>
            <h1 className="font-black text-xl leading-tight" style={{ color: 'var(--text-primary)' }}>{ticket.subject}</h1>
          </div>
          <div className="flex gap-2 flex-wrap">
            <PriorityBadge priority={ticket.priority} />
            <StatusBadge status={ticket.status} />
          </div>
        </div>

        {/* Original message */}
        <div className="rounded-xl p-4" style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)' }}>
          <p className="text-sm leading-relaxed whitespace-pre-wrap" style={{ color: 'var(--text-secondary)' }}>
            {ticket.message}
          </p>
          <p className="text-xs mt-2.5 font-medium" style={{ color: 'var(--text-muted)' }}>
            {new Date(ticket.createdAt).toLocaleString('vi-VN')}
          </p>
        </div>
      </div>

      {/* Replies */}
      {ticket.replies?.length > 0 && (
        <div className="space-y-3 mb-4">
          {ticket.replies.map(reply => (
            <div
              key={reply.id}
              className="rounded-2xl p-5 shadow-sm"
              style={{
                backgroundColor: reply.isFromAdmin ? 'rgba(139,92,246,0.06)' : 'var(--bg-card)',
                border: `1px solid ${reply.isFromAdmin ? 'rgba(139,92,246,0.2)' : 'var(--border)'}`,
              }}
            >
              <div className="flex items-center gap-2.5 mb-3">
                <div className={`w-8 h-8 rounded-xl flex items-center justify-center text-xs font-black shrink-0 ${
                  reply.isFromAdmin ? 'bg-violet-600 text-white' : ''
                }`}
                  style={!reply.isFromAdmin ? { backgroundColor: 'var(--bg-elevated)', color: 'var(--text-secondary)' } : {}}>
                  {reply.userName?.[0]?.toUpperCase() || '?'}
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-bold" style={{ color: 'var(--text-primary)' }}>{reply.userName}</span>
                    {reply.isFromAdmin && (
                      <span className="px-2 py-0.5 rounded-full bg-violet-600 text-white text-[10px] font-bold uppercase tracking-wide">
                        Admin
                      </span>
                    )}
                  </div>
                </div>
                <span className="text-xs whitespace-nowrap" style={{ color: 'var(--text-muted)' }}>
                  {new Date(reply.createdAt).toLocaleString('vi-VN')}
                </span>
              </div>

              <p className="text-sm leading-relaxed whitespace-pre-wrap" style={{ color: 'var(--text-secondary)' }}>
                {reply.message}
              </p>

              {reply.attachmentUrl && (
                <a href={reply.attachmentUrl} target="_blank" rel="noopener noreferrer"
                  className="inline-flex items-center gap-1.5 mt-2.5 text-xs font-semibold text-violet-500 hover:underline">
                  {t('ticket.attach')}
                </a>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Reply form */}
      {ticket.status !== 'Closed' ? (
        <div className="rounded-2xl p-5 shadow-sm"
          style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>
          <h3 className="text-xs font-bold uppercase tracking-widest mb-4" style={{ color: 'var(--text-muted)' }}>
            {t('ticket.reply_title')}
          </h3>
          <form onSubmit={handleReply} className="space-y-3">
            <textarea
              className="w-full px-4 py-3 rounded-xl text-sm transition-all focus:outline-none resize-none"
              style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-primary)' }}
              rows={4}
              placeholder={t('ticket.reply_ph')}
              value={replyMsg}
              onChange={e => setReplyMsg(e.target.value)}
              required
              onFocus={e => e.target.style.borderColor = '#0ea5e9'}
              onBlur={e => e.target.style.borderColor = 'var(--border)'}
            />
            <input
              className="w-full px-4 py-3 rounded-xl text-sm transition-all focus:outline-none"
              style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-primary)' }}
              placeholder={t('ticket.attach_ph')}
              value={replyAttach}
              onChange={e => setReplyAttach(e.target.value)}
              onFocus={e => e.target.style.borderColor = '#0ea5e9'}
              onBlur={e => e.target.style.borderColor = 'var(--border)'}
            />
            <button
              type="submit"
              disabled={replyLoading || !replyMsg.trim()}
              className="flex items-center gap-2 px-5 py-2.5 rounded-xl font-bold text-sm transition-all shadow-md disabled:opacity-50"
              style={{ backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)' }}
            >
              {replyLoading ? <><Spinner /> {t('ticket.reply_sending')}</> : t('ticket.reply_btn')}
            </button>
          </form>
        </div>
      ) : (
        <div className="flex items-center gap-3 px-4 py-3.5 rounded-2xl"
          style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)' }}>
          <span className="text-lg">🔒</span>
          <p className="text-sm font-medium" style={{ color: 'var(--text-muted)' }}>
            {t('ticket.closed_msg')}
          </p>
        </div>
      )}
    </div>
  );
}