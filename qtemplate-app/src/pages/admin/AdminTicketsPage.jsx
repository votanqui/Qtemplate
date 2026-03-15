import { useState, useEffect, useCallback } from 'react';
import { adminTicketApi } from '../../api/adminApi';
import {
  fmt, Chip,
  PageHeader, FiltersBar, Card, Table, Pager,
  BtnSecondary, SearchInput, Select, Empty,
  trBase, tdBase,
} from '../../components/ui/AdminUI';
import TicketDetailModal from '../../modals/admin/TicketDetailModal';

const statusColor   = { Open: 'blue', InProgress: 'yellow', Closed: 'green' };
const priorityColor = { Low: 'slate', Normal: 'blue', High: 'orange', Urgent: 'red' };

export default function AdminTicketsPage() {
  const [tickets, setTickets] = useState([]);
  const [total,   setTotal]   = useState(0);
  const [loading, setLoading] = useState(true);
  const [page,    setPage]    = useState(1);
  const [status,  setStatus]  = useState('');
  const [priority,setPriority]= useState('');
  const [sel,     setSel]     = useState(null);
  const pageSize = 20;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const p = { page, pageSize };
      if (status)   p.status   = status;
      if (priority) p.priority = priority;
      const r = await adminTicketApi.getList(p);
      const d = r.data.data;
      setTickets(d?.items || d || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setTickets([]); }
    finally { setLoading(false); }
  }, [page, status, priority]);

  useEffect(() => { load(); }, [load]);

  return (
    <div>
      <PageHeader title="Tickets hỗ trợ" sub={`${total} ticket`} />

      <FiltersBar>
        <Select value={status} onChange={e => { setStatus(e.target.value); setPage(1); }}>
          <option value="">Tất cả status</option>
          <option value="Open">Open</option>
          <option value="InProgress">In Progress</option>
          <option value="Closed">Closed</option>
        </Select>
        <Select value={priority} onChange={e => { setPriority(e.target.value); setPage(1); }}>
          <option value="">Tất cả priority</option>
          <option value="Urgent">Urgent</option>
          <option value="High">High</option>
          <option value="Normal">Normal</option>
          <option value="Low">Low</option>
        </Select>
        <span className="ml-auto text-[12px] text-slate-400">
          {loading ? 'Đang tải…' : `${total} tickets`}
        </span>
      </FiltersBar>

      <Card>
        <Table
          heads={['Mã ticket', 'Người gửi', 'Chủ đề', 'Priority', 'Status', 'Replies', 'Ngày tạo', '']}
          loading={loading}
          colCount={8}
        >
          {tickets.length === 0 && !loading
            ? <Empty msg="Không có ticket nào." />
            : tickets.map(t => (
              <tr key={t.id} className={trBase} onClick={() => setSel(t.id)}>
                <td className={tdBase}>
                  <span className="font-mono text-[11px] font-bold text-slate-500 bg-slate-100 px-2 py-0.5 rounded-lg">
                    {t.ticketCode}
                  </span>
                </td>
                <td className={tdBase}>
                  <div className="font-semibold text-slate-900">{t.userName || '—'}</div>
                  <div className="text-[11px] text-slate-400">{t.userEmail || '—'}</div>
                </td>
                <td className={`${tdBase} max-w-[200px]`}>
                  <div className="text-[13px] font-semibold text-slate-900 truncate">{t.subject}</div>
                  {t.templateName && (
                    <div className="text-[11px] text-slate-400 truncate">📄 {t.templateName}</div>
                  )}
                </td>
                <td className={tdBase}>
                  <Chip label={t.priority} color={priorityColor[t.priority] || 'slate'} />
                </td>
                <td className={tdBase}>
                  <Chip label={t.status} color={statusColor[t.status] || 'slate'} />
                </td>
                <td className={`${tdBase} text-center font-semibold text-slate-700`}>
                  {t.replyCount ?? 0}
                </td>
                <td className={`${tdBase} text-slate-400`}>{fmt(t.createdAt)}</td>
                <td className={tdBase} onClick={e => e.stopPropagation()}>
                  <BtnSecondary className="py-1 px-3 text-[12px]" onClick={() => setSel(t.id)}>
                    Mở
                  </BtnSecondary>
                </td>
              </tr>
            ))
          }
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>

      <TicketDetailModal ticketId={sel} onClose={() => setSel(null)} onRefresh={load} />
    </div>
  );
}