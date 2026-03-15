import { useState, useEffect, useCallback } from 'react';
import { adminIpBlacklistApi } from '../../api/adminApi';
import {
  fmt, Chip, ActiveDot,
  PageHeader, Card, Table, Pager,
  BtnPrimary, BtnSecondary, BtnDanger,
  ConfirmModal, Empty, Toast, StatCard,
  trBase, tdBase,
} from '../../components/ui/AdminUI';
import IpBlacklistAddModal from '../../modals/admin/IpBlacklistAddModal';

export default function AdminIpBlacklistPage() {
  const [ips,      setIps]      = useState([]);
  const [total,    setTotal]    = useState(0);
  const [loading,  setLoading]  = useState(true);
  const [page,     setPage]     = useState(1);
  const [showAdd,  setShowAdd]  = useState(false);
  const [delId,    setDelId]    = useState(null);
  const [busy,     setBusy]     = useState(false);
  const [toast,    setToast]    = useState('');
  const pageSize = 20;

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await adminIpBlacklistApi.getList(page, pageSize);
      const d = r.data.data;
      setIps(d?.items || d || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setIps([]); }
    finally { setLoading(false); }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  const doToggle = async (id, currentActive) => {
    try {
      await adminIpBlacklistApi.toggle(id);
      ok(currentActive ? '✅ Đã bỏ chặn IP' : '🚫 Đã kích hoạt chặn IP');
      load();
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
  };

  const doDelete = async () => {
    setBusy(true);
    try {
      await adminIpBlacklistApi.delete(delId);
      setDelId(null); load();
      ok('🗑 Đã xoá IP khỏi blacklist');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const activeCount  = ips.filter(i => i.isActive).length;
  const expiredCount = ips.filter(i => i.expiredAt && new Date(i.expiredAt) < new Date()).length;

  return (
    <div>
      <Toast msg={toast} />

      <PageHeader
        title="IP Blacklist"
        sub={`${total} địa chỉ`}
        action={
          <BtnDanger onClick={() => setShowAdd(true)}>
            + Thêm IP
          </BtnDanger>
        }
      />

      <div className="grid grid-cols-3 gap-3 mb-5">
        <StatCard label="Tổng IP"    value={total}        />
        <StatCard label="Đang chặn"  value={activeCount}  />
        <StatCard label="Đã hết hạn" value={expiredCount} />
      </div>

      <Card>
        <Table
          heads={['Địa chỉ IP', 'Loại', 'Lý do', 'Blocked by', 'Trạng thái', 'Hết hạn', 'Ngày chặn', '']}
          loading={loading}
          colCount={8}
        >
          {ips.length === 0 && !loading
            ? <Empty msg="Blacklist trống." />
            : ips.map(ip => {
              const isExpired = ip.expiredAt && new Date(ip.expiredAt) < new Date();
              return (
                <tr key={ip.id} className={trBase}>
                  <td className={tdBase}>
                    <span className="font-mono text-[13px] font-bold text-slate-900 bg-slate-100 px-2.5 py-1 rounded-lg">
                      {ip.ipAddress}
                    </span>
                  </td>

                  <td className={tdBase}>
                    <Chip
                      label={ip.type || 'Manual'}
                      color={ip.type === 'Auto' ? 'orange' : 'blue'}
                    />
                  </td>

                  <td className={`${tdBase} max-w-[160px]`}>
                    <div className="text-[12px] text-slate-600 truncate">
                      {ip.reason || <span className="text-slate-300 italic">—</span>}
                    </div>
                  </td>

                  <td className={`${tdBase} text-slate-500 text-[12px]`}>
                    {ip.blockedBy || '—'}
                  </td>

                  <td className={tdBase}>
                    {isExpired
                      ? <Chip label="Hết hạn" color="slate" />
                      : <ActiveDot active={ip.isActive} onLabel="Đang chặn" offLabel="Tạm bỏ" />
                    }
                  </td>

                  <td className={`${tdBase} text-[12px]`}>
                    {ip.expiredAt
                      ? <span className={isExpired ? 'text-slate-400 line-through' : 'text-amber-600 font-semibold'}>
                          {fmt(ip.expiredAt)}
                        </span>
                      : <span className="text-red-500 font-semibold text-[11px]">Vĩnh viễn</span>
                    }
                  </td>

                  <td className={`${tdBase} text-slate-400 text-[12px]`}>
                    {fmt(ip.blockedAt)}
                  </td>

                  <td className={tdBase} onClick={e => e.stopPropagation()}>
                    <div className="flex gap-1.5">
                      {ip.isActive
                        ? (
                          <BtnSecondary className="py-1 px-2.5 text-[11px]" onClick={() => doToggle(ip.id, true)}>
                            Bỏ chặn
                          </BtnSecondary>
                        ) : (
                          <BtnDanger className="py-1 px-2.5 text-[11px]" onClick={() => doToggle(ip.id, false)}>
                            Chặn lại
                          </BtnDanger>
                        )
                      }
                      <BtnDanger className="py-1 px-2.5 text-[11px]" onClick={() => setDelId(ip.id)}>
                        Xoá
                      </BtnDanger>
                    </div>
                  </td>
                </tr>
              );
            })
          }
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>

      <IpBlacklistAddModal
        open={showAdd}
        onClose={() => setShowAdd(false)}
        onRefresh={() => { load(); ok('🚫 Đã thêm IP vào blacklist'); }}
      />

      <ConfirmModal
        open={!!delId}
        onClose={() => setDelId(null)}
        onConfirm={doDelete}
        busy={busy}
        msg="Xoá IP này khỏi blacklist? Hành động không thể hoàn tác."
      />
    </div>
  );
}