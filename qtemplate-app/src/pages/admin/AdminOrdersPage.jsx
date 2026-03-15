import { useState, useEffect, useCallback } from 'react';
import { adminOrderApi } from '../../api/adminApi';
import {
  fmt, fmtMoney, Chip,
  PageHeader, FiltersBar, Card, Table, Pager,
  BtnSecondary, SearchInput, Select, Empty,
  trBase, tdBase,
} from '../../components/ui/AdminUI';
import OrderDetailModal from '../../modals/admin/OrderDetailModal';

const statusColor = {
  Pending: 'yellow', Paid: 'green', Cancelled: 'red', Refunded: 'orange',
};

export default function AdminOrdersPage() {
  const [orders,  setOrders]  = useState([]);
  const [total,   setTotal]   = useState(0);
  const [loading, setLoading] = useState(true);
  const [page,    setPage]    = useState(1);
  const [rawQ,    setRawQ]    = useState('');
  const [search,  setSearch]  = useState('');
  const [status,  setStatus]  = useState('');
  const [sel,     setSel]     = useState(null);
  const pageSize = 20;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const p = { page, pageSize };
      if (search) p.search = search;
      if (status) p.status = status;
      const r = await adminOrderApi.getList(p);
      const d = r.data.data;
      setOrders(d?.items || d || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setOrders([]); }
    finally { setLoading(false); }
  }, [page, search, status]);

  useEffect(() => { load(); }, [load]);

  useEffect(() => {
    const t = setTimeout(() => { setSearch(rawQ); setPage(1); }, 380);
    return () => clearTimeout(t);
  }, [rawQ]);

  return (
    <div>
      <PageHeader title="Đơn hàng" sub={`${total} đơn`} />

      <FiltersBar>
        <SearchInput
          value={rawQ}
          onChange={e => setRawQ(e.target.value)}
          placeholder="Mã đơn, email người mua…"
          className="flex-1 min-w-[180px] max-w-xs"
        />
        <Select value={status} onChange={e => { setStatus(e.target.value); setPage(1); }}>
          <option value="">Tất cả trạng thái</option>
          <option value="Pending">Pending</option>
          <option value="Paid">Paid</option>
          <option value="Cancelled">Cancelled</option>
          <option value="Refunded">Refunded</option>
        </Select>
        <span className="ml-auto text-[12px] text-slate-400">
          {loading ? 'Đang tải…' : `${total} đơn hàng`}
        </span>
      </FiltersBar>

      <Card>
        <Table
          heads={['Mã đơn', 'Người mua', 'Sản phẩm', 'Tổng tiền', 'Trạng thái', 'Ngày tạo', '']}
          loading={loading}
          colCount={7}
        >
          {orders.length === 0 && !loading
            ? <Empty msg="Không tìm thấy đơn hàng nào." />
            : orders.map(o => (
              <tr key={o.id} className={trBase} onClick={() => setSel(o.id)}>
                <td className={tdBase}>
                  <span className="font-mono text-[12px] font-bold text-slate-700">
                    #{o.orderCode || o.id?.slice(0, 10)}
                  </span>
                </td>
                <td className={tdBase}>
                  <div className="font-semibold text-slate-900">{o.userName || '—'}</div>
                  <div className="text-[11px] text-slate-400">{o.userEmail || '—'}</div>
                </td>
                <td className={`${tdBase} text-slate-500`}>{o.items?.length ?? 0} sản phẩm</td>
                <td className={tdBase}>
                  <span className="font-extrabold text-slate-900">{fmtMoney(o.totalAmount)}</span>
                  {o.discountAmount > 0 && (
                    <div className="text-[11px] text-green-600">-{fmtMoney(o.discountAmount)}</div>
                  )}
                </td>
                <td className={tdBase}>
                  <Chip label={o.status} color={statusColor[o.status] || 'slate'} />
                </td>
                <td className={`${tdBase} text-slate-400`}>{fmt(o.createdAt)}</td>
                <td className={tdBase} onClick={e => e.stopPropagation()}>
                  <BtnSecondary className="py-1 px-3 text-[12px]" onClick={() => setSel(o.id)}>
                    Mở
                  </BtnSecondary>
                </td>
              </tr>
            ))
          }
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>

      <OrderDetailModal orderId={sel} onClose={() => setSel(null)} onRefresh={load} />
    </div>
  );
}