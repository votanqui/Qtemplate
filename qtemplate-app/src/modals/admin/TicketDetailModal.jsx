import { useState, useEffect } from 'react';
import { adminTicketApi } from '../../api/adminApi';
import {
  Modal, Tabs, Chip, Field, Input, Select, Textarea,
  BtnPrimary, BtnSecondary, BtnDanger, BtnSuccess, Toast,
  fmtFull,
} from '../../components/ui/AdminUI';

const statusColor  = { Open: 'blue', InProgress: 'yellow', Closed: 'green' };
const priorityColor = { Low: 'slate', Normal: 'blue', High: 'orange', Urgent: 'red' };

export default function TicketDetailModal({ ticketId, onClose, onRefresh }) {
  const [ticket,  setTicket]  = useState(null);
  const [tab,     setTab]     = useState('thread');
  const [loading, setLoading] = useState(true);
  const [busy,    setBusy]    = useState(false);
  const [toast,   setToast]   = useState('');

  // reply
  const [msg, setMsg] = useState('');
  // actions
  const [newStatus,   setNewStatus]   = useState('');
  const [newPriority, setNewPriority] = useState('');
  const [assignTo,    setAssignTo]    = useState('');

  const ok = m => { setToast(m); setTimeout(() => setToast(''), 2400); };

  useEffect(() => {
    if (!ticketId) return;
    setLoading(true); setTab('thread'); setMsg('');
    adminTicketApi.getDetail(ticketId)
      .then(r => {
        const d = r.data.data;
        setTicket(d);
        setNewStatus(d?.status || '');
        setNewPriority(d?.priority || '');
        setAssignTo(d?.assignedTo || '');
      })
      .catch(() => setTicket(null))
      .finally(() => setLoading(false));
  }, [ticketId]);

  const doReply = async () => {
    if (!msg.trim()) return alert('Nhập nội dung trả lời.');
    setBusy(true);
    try {
      await adminTicketApi.reply(ticketId, msg.trim());
      setMsg('');
      // reload ticket để có reply mới
      const r = await adminTicketApi.getDetail(ticketId);
      setTicket(r.data.data);
      onRefresh(); ok('💬 Đã gửi reply');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doChangeStatus = async () => {
    if (newStatus === ticket.status) return;
    setBusy(true);
    try {
      await adminTicketApi.changeStatus(ticketId, newStatus);
      setTicket(t => ({ ...t, status: newStatus }));
      onRefresh(); ok('✅ Đã đổi status');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doChangePriority = async () => {
    if (newPriority === ticket.priority) return;
    setBusy(true);
    try {
      await adminTicketApi.changePriority(ticketId, newPriority);
      setTicket(t => ({ ...t, priority: newPriority }));
      onRefresh(); ok('✅ Đã đổi priority');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doAssign = async () => {
    if (!assignTo.trim()) return alert('Nhập Admin ID.');
    setBusy(true);
    try {
      await adminTicketApi.assign(ticketId, assignTo.trim());
      setTicket(t => ({ ...t, assignedTo: assignTo.trim() }));
      onRefresh(); ok('✅ Đã assign ticket');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <Modal open={!!ticketId} onClose={onClose} title="Chi tiết Ticket" width={680}>
      <Toast msg={toast} />
      {loading ? (
        <div className="text-center text-slate-400 py-12">Đang tải...</div>
      ) : !ticket ? (
        <div className="text-center text-red-500 py-8">Không tìm thấy ticket.</div>
      ) : (
        <>
          {/* Header */}
          <div className="p-4 bg-slate-50 rounded-2xl mb-5">
            <div className="flex items-start justify-between gap-3 mb-2">
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 flex-wrap mb-1">
                  <span className="font-mono text-[12px] font-bold text-slate-500 bg-white border border-slate-200 px-2 py-0.5 rounded-lg">
                    {ticket.ticketCode}
                  </span>
                  <Chip label={ticket.status}   color={statusColor[ticket.status]   || 'slate'} />
                  <Chip label={ticket.priority} color={priorityColor[ticket.priority] || 'slate'} />
                </div>
                <div className="text-[14px] font-bold text-slate-900 truncate">{ticket.subject}</div>
              </div>
              <div className="text-right flex-shrink-0 text-[11px] text-slate-400">
                <div>{ticket.userName}</div>
                <div>{fmtFull(ticket.createdAt)}</div>
              </div>
            </div>
            {ticket.aiPriorityReason && (
              <div className="text-[11px] text-amber-600 bg-amber-50 px-3 py-1.5 rounded-lg mt-2">
                🤖 AI: {ticket.aiPriorityReason}
              </div>
            )}
          </div>

          <Tabs
            tabs={[
              ['thread', `Hội thoại (${ticket.replies?.length ?? 0})`],
              ['actions', 'Thao tác'],
            ]}
            active={tab} onChange={setTab}
          />

          {/* THREAD */}
          {tab === 'thread' && (
            <div className="flex flex-col gap-3">
              {/* Original message */}
              <div className="p-3 bg-blue-50 border border-blue-100 rounded-xl">
                <div className="flex items-center gap-2 mb-2">
                  <span className="text-[11px] font-bold text-blue-600 uppercase tracking-wide">Tin nhắn gốc</span>
                  <span className="text-[11px] text-slate-400">{fmtFull(ticket.createdAt)}</span>
                </div>
                <p className="text-[13px] text-slate-700 leading-relaxed whitespace-pre-wrap">{ticket.message}</p>
              </div>

              {/* Replies */}
              {ticket.replies?.map(r => (
                <div
                  key={r.id}
                  className={`p-3 rounded-xl border ${
                    r.isFromAdmin
                      ? 'bg-violet-50 border-violet-100 ml-4'
                      : 'bg-white border-slate-100'
                  }`}
                >
                  <div className="flex items-center gap-2 mb-2">
                    <span className={`text-[11px] font-bold uppercase tracking-wide ${r.isFromAdmin ? 'text-violet-600' : 'text-slate-500'}`}>
                      {r.isFromAdmin ? '🛡 Admin' : r.userName || 'User'}
                    </span>
                    <span className="text-[11px] text-slate-400">{fmtFull(r.createdAt)}</span>
                  </div>
                  <p className="text-[13px] text-slate-700 leading-relaxed whitespace-pre-wrap">{r.message}</p>
                  {r.attachmentUrl && (
                    <a href={r.attachmentUrl} target="_blank" rel="noreferrer"
                      className="text-[11px] text-blue-500 hover:underline mt-1 block">
                      📎 Đính kèm
                    </a>
                  )}
                </div>
              ))}

              {/* Reply box */}
              {ticket.status !== 'Closed' ? (
                <div className="mt-2 pt-4 border-t border-slate-100">
                  <Field label="Trả lời">
                    <Textarea
                      value={msg}
                      onChange={e => setMsg(e.target.value)}
                      rows={3}
                      placeholder="Nhập nội dung trả lời…"
                    />
                  </Field>
                  <BtnPrimary onClick={doReply} disabled={busy}>
                    {busy ? '…' : '📨 Gửi trả lời'}
                  </BtnPrimary>
                </div>
              ) : (
                <div className="p-3 bg-slate-100 rounded-xl text-center text-[12px] text-slate-400 font-semibold">
                  Ticket đã đóng — không thể reply.
                </div>
              )}
            </div>
          )}

          {/* ACTIONS */}
          {tab === 'actions' && (
            <div className="flex flex-col gap-4">
              {/* Status */}
              <div className="p-4 rounded-2xl border border-slate-200">
                <div className="text-[13px] font-bold text-slate-900 mb-3">🔄 Đổi Status</div>
                <div className="flex gap-2">
                  <Select value={newStatus} onChange={e => setNewStatus(e.target.value)} className="flex-1">
                    <option value="Open">Open</option>
                    <option value="InProgress">InProgress</option>
                    <option value="Closed">Closed</option>
                  </Select>
                  <BtnPrimary onClick={doChangeStatus} disabled={busy || newStatus === ticket.status}>
                    {busy ? '…' : 'Lưu'}
                  </BtnPrimary>
                </div>
              </div>

              {/* Priority */}
              <div className="p-4 rounded-2xl border border-slate-200">
                <div className="text-[13px] font-bold text-slate-900 mb-3">⚡ Đổi Priority</div>
                <div className="flex gap-2">
                  <Select value={newPriority} onChange={e => setNewPriority(e.target.value)} className="flex-1">
                    <option value="Low">Low</option>
                    <option value="Normal">Normal</option>
                    <option value="High">High</option>
                    <option value="Urgent">Urgent</option>
                  </Select>
                  <BtnPrimary onClick={doChangePriority} disabled={busy || newPriority === ticket.priority}>
                    {busy ? '…' : 'Lưu'}
                  </BtnPrimary>
                </div>
              </div>

              {/* Assign */}
              <div className="p-4 rounded-2xl border border-slate-200">
                <div className="text-[13px] font-bold text-slate-900 mb-3">👤 Assign cho Admin</div>
                {ticket.assignedTo && (
                  <div className="text-[11px] text-slate-400 mb-2">Đang assign: <span className="font-mono">{ticket.assignedTo}</span></div>
                )}
                <div className="flex gap-2">
                  <Input
                    value={assignTo}
                    onChange={e => setAssignTo(e.target.value)}
                    placeholder="Admin User ID (Guid)…"
                    className="flex-1"
                  />
                  <BtnSecondary onClick={doAssign} disabled={busy}>
                    {busy ? '…' : 'Assign'}
                  </BtnSecondary>
                </div>
              </div>
            </div>
          )}
        </>
      )}
    </Modal>
  );
}