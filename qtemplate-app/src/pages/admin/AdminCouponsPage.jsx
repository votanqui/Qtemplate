import { useState, useEffect, useCallback } from 'react';
import { adminCouponApi } from '../../api/adminApi';
import {
  fmt, fmtMoney, Chip, ActiveDot,
  PageHeader, FiltersBar, Card, Table, Pager,
  BtnPrimary, BtnSecondary, BtnDanger, Select,
  ConfirmModal, Empty, Toast,
  trBase, tdBase,
} from '../../components/ui/AdminUI';
import CouponFormModal from '../../modals/admin/CouponFormModal';

const isPercent = type => type === 'Percent' || type === 'Percentage';

export default function AdminCouponsPage() {
  const [coupons, setCoupons] = useState([]);
  const [total,   setTotal]   = useState(0);
  const [loading, setLoading] = useState(true);
  const [page,    setPage]    = useState(1);
  const [status,  setStatus]  = useState('');
  const [form,    setForm]    = useState(null);
  const [delId,   setDelId]   = useState(null);
  const [busy,    setBusy]    = useState(false);
  const [toast,   setToast]   = useState('');
  const pageSize = 20;

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const p = { page, pageSize };
      if (status !== '') p.isActive = status;
      const r = await adminCouponApi.getList(p);
      const d = r.data.data;
      setCoupons(d?.items || d || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setCoupons([]); }
    finally { setLoading(false); }
  }, [page, status]);

  useEffect(() => { load(); }, [load]);

  const doDelete = async () => {
    setBusy(true);
    try {
      await adminCouponApi.delete(delId);
      setDelId(null); load();
      ok('🗑 Đã xoá coupon');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <div>
      <Toast msg={toast} />

      <PageHeader
        title="Coupon"
        sub={`${total} mã`}
        action={<BtnPrimary onClick={() => setForm({})}>+ Tạo coupon</BtnPrimary>}
      />

      <FiltersBar>
        <Select value={status} onChange={e => { setStatus(e.target.value); setPage(1); }}>
          <option value="">Tất cả</option>
          <option value="true">Đang hoạt động</option>
          <option value="false">Đã tắt</option>
        </Select>
        <span className="ml-auto text-[12px] text-slate-400">
          {loading ? 'Đang tải…' : `${total} coupon`}
        </span>
      </FiltersBar>

      <Card>
        <Table
          heads={['Mã code', 'Loại', 'Giá trị', 'Đơn tối thiểu', 'Giảm tối đa', 'Đã dùng / Giới hạn', 'Hết hạn', 'Trạng thái', '']}
          loading={loading}
          colCount={9}
        >
          {coupons.length === 0 && !loading
            ? <Empty msg="Chưa có coupon nào." />
            : coupons.map(c => (
              <tr key={c.id} className={trBase}>
                <td className={tdBase}>
                  <span className="font-mono font-bold bg-slate-100 px-2 py-0.5 rounded-lg text-[12px]" style={{ color: 'var(--text-primary)' }}>
                    {c.code}
                  </span>
                </td>
                <td className={tdBase}>
                  <Chip
                    label={isPercent(c.type) ? 'Phần trăm' : 'Cố định'}
                    color={isPercent(c.type) ? 'purple' : 'blue'}
                  />
                </td>
                <td className={`${tdBase} font-extrabold`} style={{ color: 'var(--text-primary)' }}>
                  {isPercent(c.type) ? `${c.value}%` : fmtMoney(c.value)}
                </td>
                <td className={`${tdBase} text-slate-500`}>
                  {c.minOrderAmount ? fmtMoney(c.minOrderAmount) : '—'}
                </td>
                <td className={`${tdBase} text-slate-500`}>
                  {c.maxDiscountAmount ? fmtMoney(c.maxDiscountAmount) : '—'}
                </td>
                <td className={tdBase}>
                  <span className={`text-[13px] font-semibold ${c.usageLimit && c.usedCount >= c.usageLimit ? 'text-red-500' : ''}`} style={{ color: c.usageLimit && c.usedCount >= c.usageLimit ? undefined : 'var(--text-primary)' }}>
                    {c.usedCount ?? 0}
                    {c.usageLimit ? ` / ${c.usageLimit}` : ' / ∞'}
                  </span>
                </td>
                <td className={`${tdBase} text-[12px]`} style={{ color: 'var(--text-muted)' }}>
                  {c.expiredAt ? fmt(c.expiredAt) : '—'}
                </td>
                <td className={tdBase}>
                  <ActiveDot active={c.isActive} onLabel="Hoạt động" offLabel="Tắt" />
                </td>
                <td className={tdBase} onClick={e => e.stopPropagation()}>
                  <div className="flex gap-1.5">
                    <BtnSecondary className="py-1 px-3 text-[12px]" onClick={() => setForm(c)}>Sửa</BtnSecondary>
                    <BtnDanger    className="py-1 px-3 text-[12px]" onClick={() => setDelId(c.id)}>Xoá</BtnDanger>
                  </div>
                </td>
              </tr>
            ))
          }
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>

      <CouponFormModal coupon={form} onClose={() => setForm(null)} onRefresh={load} />
      <ConfirmModal open={!!delId} onClose={() => setDelId(null)} onConfirm={doDelete} busy={busy} msg="Xoá coupon này? Hành động không thể hoàn tác." />
    </div>
  );
}