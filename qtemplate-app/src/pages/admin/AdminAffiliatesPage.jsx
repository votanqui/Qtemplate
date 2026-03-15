import { useState, useEffect, useCallback } from 'react';
import { adminAffiliateApi } from '../../api/adminApi';
import {
  fmt, fmtMoney, Chip, ActiveDot,
  PageHeader, FiltersBar, Card, Table, Pager,
  BtnSecondary, Select, Empty,
  trBase, tdBase,
} from '../../components/ui/AdminUI';
import AffiliateDetailModal from '../../modals/admin/AffiliateDetailModal';

export default function AdminAffiliatesPage() {
  const [affiliates, setAffiliates] = useState([]);
  const [total,      setTotal]      = useState(0);
  const [loading,    setLoading]    = useState(true);
  const [page,       setPage]       = useState(1);
  const [status,     setStatus]     = useState('');
  const [sel,        setSel]        = useState(null);
  const pageSize = 20;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const p = { page, pageSize };
      if (status !== '') p.isActive = status;
      const r = await adminAffiliateApi.getList(p);
      const d = r.data.data;
      setAffiliates(d?.items || d || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setAffiliates([]); }
    finally { setLoading(false); }
  }, [page, status]);

  useEffect(() => { load(); }, [load]);

  return (
    <div>
      <PageHeader title="Affiliates" sub={`${total} tài khoản`} />

      <FiltersBar>
        <Select value={status} onChange={e => { setStatus(e.target.value); setPage(1); }}>
          <option value="">Tất cả</option>
          <option value="true">Đã duyệt</option>
          <option value="false">Chờ duyệt</option>
        </Select>
        <span className="ml-auto text-[12px] text-slate-400">
          {loading ? 'Đang tải…' : `${total} affiliates`}
        </span>
      </FiltersBar>

      <Card>
        <Table
          heads={['Người dùng', 'Mã affiliate', 'Tỷ lệ', 'Tổng kiếm', 'Chờ TT', 'Đã TT', 'Trạng thái', 'Ngày đăng ký', '']}
          loading={loading}
          colCount={9}
        >
          {affiliates.length === 0 && !loading
            ? <Empty msg="Chưa có affiliate nào." />
            : affiliates.map(a => (
              <tr key={a.id} className={trBase} onClick={() => setSel(a)}>
                <td className={tdBase}>
                  <div className="font-semibold text-slate-900">{a.userName || '—'}</div>
                  <div className="text-[11px] text-slate-400">{a.userEmail || '—'}</div>
                </td>
                <td className={tdBase}>
                  <span className="font-mono text-[12px] font-bold text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded-lg">
                    {a.affiliateCode}
                  </span>
                </td>
                <td className={`${tdBase} font-semibold text-slate-900`}>
                  {a.commissionRate}%
                </td>
                <td className={`${tdBase} font-extrabold text-slate-900`}>
                  {fmtMoney(a.totalEarned)}
                </td>
                <td className={`${tdBase} font-semibold text-amber-600`}>
                  {fmtMoney(a.pendingAmount)}
                </td>
                <td className={`${tdBase} text-slate-500`}>
                  {fmtMoney(a.paidAmount)}
                </td>
                <td className={tdBase}>
                  <ActiveDot active={a.isActive} onLabel="Active" offLabel="Chờ duyệt" />
                </td>
                <td className={`${tdBase} text-slate-400`}>{fmt(a.createdAt)}</td>
                <td className={tdBase} onClick={e => e.stopPropagation()}>
                  <BtnSecondary className="py-1 px-3 text-[12px]" onClick={() => setSel(a)}>
                    Mở
                  </BtnSecondary>
                </td>
              </tr>
            ))
          }
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>

      <AffiliateDetailModal
        affiliate={sel}
        onClose={() => setSel(null)}
        onRefresh={load}
      />
    </div>
  );
}