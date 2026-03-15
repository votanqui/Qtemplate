import { useState, useEffect, useCallback } from 'react';
import { adminAffiliateApi } from '../../api/adminApi';
import {
  Modal, Tabs, Chip, BtnSuccess, BtnSecondary, BtnDanger, BtnPrimary,
  Toast, fmtFull, fmtMoney, fmt,
  Select, Pager, Empty,
} from '../../components/ui/AdminUI';

const txStatusColor = { Pending: 'yellow', Paid: 'green' };

// ── Transactions Tab (API-driven) ─────────────────────────────
function TransactionsTab({ affiliateId, onPayout, busy }) {
  const [items,    setItems]    = useState([]);
  const [total,    setTotal]    = useState(0);
  const [loading,  setLoading]  = useState(true);
  const [page,     setPage]     = useState(1);
  const [status,   setStatus]   = useState('');
  const [summary,  setSummary]  = useState(null);
  const pageSize = 20;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const params = { page, pageSize };
      if (status) params.status = status;
      const r = await adminAffiliateApi.getTransactions(affiliateId, params);
      const d = r.data.data;
      // API trả về AdminAffiliateTransactionsDto
      const txData = d?.transactions ?? d;
      setItems(txData?.items || []);
      setTotal(txData?.totalCount ?? 0);
      // Lưu summary để hiện tổng hoa hồng
      if (d?.totalEarned != null) setSummary(d);
    } catch { setItems([]); }
    finally { setLoading(false); }
  }, [affiliateId, page, status]);

  useEffect(() => { setPage(1); }, [status]);
  useEffect(() => { load(); }, [load]);

  // Khi payout xong, reload lại
  const handlePayout = async (txId) => {
    await onPayout(txId);
    load();
  };

  const pendingTotal = items
    .filter(t => t.status === 'Pending')
    .reduce((s, t) => s + (t.commission || 0), 0);

  return (
    <div className="flex flex-col gap-3">
      {/* Summary bar */}
      {summary && (
        <div className="grid grid-cols-3 gap-2">
          {[
            ['Tổng kiếm', fmtMoney(summary.totalEarned), 'text-slate-900'],
            ['Chờ TT',    fmtMoney(summary.pendingAmount), 'text-amber-600'],
            ['Đã TT',     fmtMoney(summary.paidAmount), 'text-green-600'],
          ].map(([lbl, val, cls]) => (
            <div key={lbl} className="bg-slate-50 rounded-xl px-3 py-2 text-center">
              <div className="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-0.5">{lbl}</div>
              <div className={`text-[13px] font-extrabold ${cls}`}>{val}</div>
            </div>
          ))}
        </div>
      )}

      {/* Filters */}
      <div className="flex items-center gap-2">
        <Select value={status} onChange={e => setStatus(e.target.value)} className="text-[12px]">
          <option value="">Tất cả trạng thái</option>
          <option value="Pending">Pending</option>
          <option value="Paid">Paid</option>
        </Select>
        <span className="ml-auto text-[12px] text-slate-400">{total} giao dịch</span>
      </div>

      {/* Payout all pending button */}
      {items.filter(t => t.status === 'Pending').length > 0 && (
        <div className="flex items-center justify-between bg-amber-50 border border-amber-200 rounded-xl px-3 py-2">
          <div>
            <span className="text-[12px] font-bold text-amber-700">
              {items.filter(t => t.status === 'Pending').length} giao dịch pending
            </span>
            <span className="text-[12px] text-amber-600 ml-2">· {fmtMoney(pendingTotal)}</span>
          </div>
          <BtnPrimary
            className="py-1 px-3 text-[12px]"
            disabled={busy}
            onClick={async () => {
              const pending = items.filter(t => t.status === 'Pending');
              for (const tx of pending) await handlePayout(tx.id);
            }}
          >
            {busy ? '…' : 'Thanh toán tất cả'}
          </BtnPrimary>
        </div>
      )}

      {/* List */}
      {loading ? (
        <div className="text-center text-slate-400 py-8 text-[13px]">Đang tải…</div>
      ) : items.length === 0 ? (
        <Empty msg="Chưa có giao dịch nào." />
      ) : (
        <div className="flex flex-col gap-2">
          {items.map(tx => (
            <div key={tx.id} className="flex items-center justify-between p-3 bg-slate-50 rounded-xl gap-3">
              <div className="flex-1 min-w-0">
                <div className="text-[12px] font-bold text-slate-700">
                  {tx.orderCode || tx.orderId?.toString().slice(0, 8)}
                </div>
                <div className="text-[11px] text-slate-400 mt-0.5">{fmt(tx.createdAt)}</div>
                {tx.paidAt && (
                  <div className="text-[10px] text-green-500 mt-0.5">Đã TT: {fmt(tx.paidAt)}</div>
                )}
              </div>
              <div className="text-right">
                <div className="text-[13px] font-extrabold text-slate-900">{fmtMoney(tx.commission)}</div>
                <div className="text-[11px] text-slate-400">/ {fmtMoney(tx.orderAmount)}</div>
              </div>
              <div className="flex items-center gap-2 flex-shrink-0">
                <Chip label={tx.status} color={txStatusColor[tx.status] || 'slate'} />
                {tx.status === 'Pending' && (
                  <BtnSuccess
                    className="py-1 px-2.5 text-[11px]"
                    onClick={() => handlePayout(tx.id)}
                    disabled={busy}
                  >
                    Trả tiền
                  </BtnSuccess>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {total > pageSize && (
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      )}
    </div>
  );
}

// ── Main modal ────────────────────────────────────────────────
export default function AffiliateDetailModal({ affiliate, onClose, onRefresh }) {
  const [tab,  setTab]  = useState('info');
  const [busy, setBusy] = useState(false);
  const [toast,setToast]= useState('');

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  if (!affiliate) return null;

  const doToggle = async (activate) => {
    setBusy(true);
    try {
      await adminAffiliateApi.approve(affiliate.id, activate);
      onRefresh();
      ok(activate ? '✅ Đã duyệt affiliate' : '🚫 Đã vô hiệu hóa');
      setTimeout(onClose, 600);
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doPayout = async (txId) => {
    if (!window.confirm('Xác nhận đã thanh toán hoa hồng này?')) return;
    setBusy(true);
    try {
      await adminAffiliateApi.payout(txId);
      onRefresh();
      ok('💰 Đã thanh toán hoa hồng');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <Modal open={!!affiliate} onClose={onClose} title="Chi tiết Affiliate" width={640}>
      <Toast msg={toast} />

      {/* Header */}
      <div className="flex items-center gap-4 p-4 bg-slate-50 rounded-2xl mb-5">
        <div className="w-11 h-11 rounded-2xl bg-gradient-to-br from-emerald-400 to-teal-500 flex items-center justify-center text-white text-lg font-extrabold flex-shrink-0">
          {affiliate.userName?.[0]?.toUpperCase() || '?'}
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 mb-1 flex-wrap">
            <span className="text-[15px] font-extrabold text-slate-900">{affiliate.userName}</span>
            <span className="font-mono text-[12px] font-bold text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded-lg">
              {affiliate.affiliateCode}
            </span>
            <Chip
              label={affiliate.isActive ? 'Đã duyệt' : 'Chờ duyệt'}
              color={affiliate.isActive ? 'green' : 'yellow'}
            />
          </div>
          <div className="text-[12px] text-slate-500">{affiliate.userEmail}</div>
        </div>
        <div className="text-right flex-shrink-0">
          <div className="text-[18px] font-extrabold text-slate-900">{fmtMoney(affiliate.totalEarned)}</div>
          <div className="text-[11px] text-slate-400">Tổng hoa hồng</div>
        </div>
      </div>

      <Tabs
        tabs={[
          ['info',         'Thông tin'],
          ['transactions', 'Giao dịch'],
          ['actions',      'Thao tác'],
        ]}
        active={tab} onChange={setTab}
      />

      {/* INFO */}
      {tab === 'info' && (
        <div className="grid grid-cols-2 gap-3">
          {[
            ['Mã affiliate',          affiliate.affiliateCode],
            ['Tỷ lệ hoa hồng',        `${affiliate.commissionRate}%`],
            ['Tổng kiếm được',        fmtMoney(affiliate.totalEarned)],
            ['Đang chờ thanh toán',   fmtMoney(affiliate.pendingAmount)],
            ['Đã thanh toán',         fmtMoney(affiliate.paidAmount)],
            ['Trạng thái',            <Chip key="s" label={affiliate.isActive ? 'Active' : 'Pending'} color={affiliate.isActive ? 'green' : 'yellow'} />],
            ['Ngày đăng ký',          fmtFull(affiliate.createdAt)],
          ].map(([lbl, val]) => (
            <div key={lbl} className="p-3 bg-slate-50 rounded-xl">
              <div className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">{lbl}</div>
              <div className="text-[13px] font-semibold text-slate-900">{val}</div>
            </div>
          ))}
        </div>
      )}

      {/* TRANSACTIONS — API-driven */}
      {tab === 'transactions' && (
        <TransactionsTab
          affiliateId={affiliate.id}
          onPayout={doPayout}
          busy={busy}
        />
      )}

      {/* ACTIONS */}
      {tab === 'actions' && (
        <div className="flex flex-col gap-4">
          {/* Approve / Deactivate */}
          <div className={`p-4 rounded-2xl border ${affiliate.isActive ? 'border-red-200 bg-red-50/40' : 'border-green-200 bg-green-50/40'}`}>
            <div className="text-[13px] font-bold text-slate-900 mb-3">
              {affiliate.isActive ? '🚫 Vô hiệu hóa affiliate' : '✅ Duyệt affiliate'}
            </div>
            <div className="text-[12px] text-slate-500 mb-3">
              {affiliate.isActive
                ? 'Tạm ngưng tài khoản affiliate này. User sẽ không tạo được hoa hồng mới.'
                : 'Duyệt tài khoản affiliate. User sẽ bắt đầu nhận hoa hồng khi có đơn hàng qua link của họ.'
              }
            </div>
            {affiliate.isActive
              ? <BtnDanger onClick={() => doToggle(false)} disabled={busy}>
                  {busy ? '…' : 'Vô hiệu hóa'}
                </BtnDanger>
              : <BtnSuccess onClick={() => doToggle(true)} disabled={busy}>
                  {busy ? '…' : '✓ Duyệt ngay'}
                </BtnSuccess>
            }
          </div>

          {/* Payout pending — quick action */}
          {affiliate.pendingAmount > 0 && (
            <div className="p-4 rounded-2xl border border-amber-200 bg-amber-50/40">
              <div className="text-[13px] font-bold text-slate-900 mb-1">
                💰 Có hoa hồng chờ thanh toán
              </div>
              <div className="text-[12px] text-slate-500 mb-3">
                Tổng chờ TT: <span className="font-bold text-amber-600">{fmtMoney(affiliate.pendingAmount)}</span>
                {' '}— Vào tab <strong>Giao dịch</strong> để thanh toán từng khoản.
              </div>
              <BtnSecondary onClick={() => setTab('transactions')}>
                Xem giao dịch pending →
              </BtnSecondary>
            </div>
          )}
        </div>
      )}
    </Modal>
  );
}