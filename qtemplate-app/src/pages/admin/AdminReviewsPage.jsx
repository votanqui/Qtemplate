import { useState, useEffect, useCallback } from 'react';
import { adminReviewApi } from '../../api/adminApi';
import {
  fmt, Chip,
  PageHeader, FiltersBar, Card, Table, Pager,
  BtnSecondary, Select, Empty,
  trBase, tdBase,
} from '../../components/ui/AdminUI';
import ReviewActionModal from '../../modals/admin/ReviewActionModal';

export default function AdminReviewsPage() {
  const [reviews, setReviews] = useState([]);
  const [total,   setTotal]   = useState(0);
  const [loading, setLoading] = useState(true);
  const [page,    setPage]    = useState(1);
  const [status,  setStatus]  = useState('');
  const [sel,     setSel]     = useState(null);
  const pageSize = 20;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const p = { page, pageSize };
      if (status !== '') p.status = status;
      const r = await adminReviewApi.getList(p);
      const d = r.data.data;
      setReviews(d?.items || d || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setReviews([]); }
    finally { setLoading(false); }
  }, [page, status]);

  useEffect(() => { load(); }, [load]);

  const stars = n => '★'.repeat(n) + '☆'.repeat(5 - n);

  return (
    <div>
      <PageHeader title="Reviews" sub={`${total} đánh giá`} />

      <FiltersBar>
        <Select value={status} onChange={e => { setStatus(e.target.value); setPage(1); }}>
          <option value="">Tất cả</option>
          <option value="pending">Chờ duyệt</option>
          <option value="approved">Đã duyệt</option>
        </Select>
        <span className="ml-auto text-[12px] text-slate-400">
          {loading ? 'Đang tải…' : `${total} reviews`}
        </span>
      </FiltersBar>

      <Card>
        <Table
          heads={['Người dùng', 'Template', 'Rating', 'Nội dung', 'Trạng thái', 'Ngày tạo', '']}
          loading={loading}
          colCount={7}
        >
          {reviews.length === 0 && !loading
            ? <Empty msg="Không tìm thấy review nào." />
            : reviews.map(r => (
              <tr key={r.id} className={trBase} onClick={() => setSel(r)}>
                <td className={tdBase}>
                  <div className="font-semibold text-slate-900">{r.userName || '—'}</div>
                  <div className="text-[11px] text-slate-400">{r.userEmail || '—'}</div>
                </td>
                <td className={`${tdBase} text-slate-600 text-[12px] max-w-[140px] truncate`}>
                  {r.templateName || '—'}
                </td>
                <td className={tdBase}>
                  <span className="text-amber-400 font-bold tracking-tight text-[14px]">
                    {stars(r.rating)}
                  </span>
                  <div className="text-[11px] text-slate-400">{r.rating}/5</div>
                </td>
                <td className={`${tdBase} max-w-[180px]`}>
                  <p className="text-[12px] text-slate-600 line-clamp-2 leading-relaxed">
                    {r.comment}
                  </p>
                  {r.adminReply && (
                    <span className="text-[10px] text-blue-600 font-semibold mt-0.5 block">💬 Đã trả lời</span>
                  )}
                </td>
                <td className={tdBase}>
                  <Chip
                    label={r.isApproved ? 'Đã duyệt' : 'Chờ duyệt'}
                    color={r.isApproved ? 'green' : 'yellow'}
                  />
                </td>
                <td className={`${tdBase} text-slate-400`}>{fmt(r.createdAt)}</td>
                <td className={tdBase} onClick={e => e.stopPropagation()}>
                  <BtnSecondary className="py-1 px-3 text-[12px]" onClick={() => setSel(r)}>
                    Xem
                  </BtnSecondary>
                </td>
              </tr>
            ))
          }
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>

      <ReviewActionModal review={sel} onClose={() => setSel(null)} onRefresh={load} />
    </div>
  );
}