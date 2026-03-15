import { useState, useEffect } from 'react';
import { adminOrderApi } from '../../api/adminApi';
import { toAbsoluteUrl } from '../../api/client';
import {
  Modal, Tabs, Chip, Field, Textarea,
  BtnDanger, BtnSecondary, Toast, fmtFull, fmtMoney,
} from '../../components/ui/AdminUI';

const statusColor = {
  Pending: 'yellow', Paid: 'green', Cancelled: 'red', Refunded: 'orange', Completed: 'blue',
};

export default function OrderDetailModal({ orderId, onClose, onRefresh }) {
  const [order,      setOrder]      = useState(null);
  const [tab,        setTab]        = useState('info');
  const [loading,    setLoading]    = useState(true);
  const [busy,       setBusy]       = useState(false);
  const [reason,     setReason]     = useState('');
  const [newStatus,  setNewStatus]  = useState('');
  const [statusNote, setStatusNote] = useState('');
  const [toast,      setToast]      = useState('');

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  useEffect(() => {
    if (!orderId) return;
    setLoading(true); setTab('info'); setReason(''); setNewStatus(''); setStatusNote('');
    adminOrderApi.getDetail(orderId)
      .then(r => setOrder(r.data.data))
      .catch(() => setOrder(null))
      .finally(() => setLoading(false));
  }, [orderId]);

  const doUpdateStatus = async () => {
    if (!newStatus) return alert('Chọn trạng thái mới.');
    if (!window.confirm(`Xác nhận đổi trạng thái đơn sang "${newStatus}"?`)) return;
    setBusy(true);
    try {
      await adminOrderApi.updateStatus(orderId, newStatus, statusNote.trim() || null);
      setOrder(o => ({ ...o, status: newStatus }));
      setNewStatus(''); setStatusNote('');
      onRefresh();
      ok(`✅ Đã cập nhật trạng thái → ${newStatus}`);
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doCancel = async () => {
    if (!reason.trim()) return alert('Nhập lý do huỷ.');
    setBusy(true);
    try {
      await adminOrderApi.cancel(orderId, reason.trim());
      setOrder(o => ({ ...o, status: 'Cancelled' }));
      setReason(''); onRefresh();
      ok('✅ Đã huỷ đơn hàng');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <Modal open={!!orderId} onClose={onClose} title="Chi tiết đơn hàng" width={640}>
      <Toast msg={toast} />
      {loading ? (
        <div className="text-center text-slate-400 py-12">Đang tải...</div>
      ) : !order ? (
        <div className="text-center text-red-500 py-8">Không tìm thấy đơn hàng.</div>
      ) : (
        <>
          {/* Header */}
          <div className="flex items-center justify-between p-4 bg-slate-50 rounded-2xl mb-5">
            <div>
              <div className="text-[15px] font-extrabold text-slate-900">
                #{order.orderCode || order.id?.slice(0, 10)}
              </div>
              <div className="text-[12px] text-slate-400 mt-0.5">{fmtFull(order.createdAt)}</div>
            </div>
            <div className="text-right">
              <div className="text-[20px] font-extrabold text-slate-900">{fmtMoney(order.totalAmount)}</div>
              <Chip label={order.status} color={statusColor[order.status] || 'slate'} />
            </div>
          </div>

          <Tabs
            tabs={[['info', 'Thông tin'], ['items', `Sản phẩm (${order.items?.length ?? 0})`], ['actions', 'Thao tác']]}
            active={tab} onChange={setTab}
          />

          {/* INFO */}
          {tab === 'info' && (
            <div className="flex flex-col gap-3">
              <div className="grid grid-cols-2 gap-3">
                {[
                  ['Mã đơn',     order.orderCode || '—'],
                  ['Người mua',  order.userName || order.userId],
                  ['Email',      order.userEmail || '—'],
                  ['Trạng thái', <Chip label={order.status} color={statusColor[order.status] || 'slate'} />],
                  ['Tổng tiền',  fmtMoney(order.totalAmount)],
                  ['Giảm giá',   order.discountAmount > 0 ? `- ${fmtMoney(order.discountAmount)}` : '—'],
                  ['Thành tiền', fmtMoney(order.finalAmount)],
                  ['Coupon',     order.couponCode || '—'],
                  ['Ngày tạo',   fmtFull(order.createdAt)],
                  ['Ghi chú',    order.note || '—'],
                  ...(order.cancelReason ? [['Lý do huỷ', order.cancelReason]] : []),
                ].map(([lbl, val]) => (
                  <div key={lbl} className="p-3 bg-slate-50 rounded-xl">
                    <div className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">{lbl}</div>
                    <div className="text-[13px] font-semibold text-slate-900 break-all">{val}</div>
                  </div>
                ))}
              </div>

              {(order.paymentStatus || order.bankCode) && (
                <div className="p-4 bg-blue-50 border border-blue-100 rounded-2xl">
                  <div className="text-[11px] font-bold text-blue-600 uppercase tracking-widest mb-3">
                    💳 Thông tin thanh toán
                  </div>
                  <div className="grid grid-cols-2 gap-2">
                    {[
                      ['Trạng thái TT',   order.paymentStatus || '—'],
                      ['Ngân hàng',        order.bankCode || '—'],
                      ['Số tiền TT',       order.paymentAmount != null ? fmtMoney(order.paymentAmount) : '—'],
                      ['Nội dung CK',      order.transferContent || '—'],
                      ['Mã Sepay',         order.sepayCode || '—'],
                      ['Ngày thanh toán',  fmtFull(order.paidAt)],
                      ...(order.failReason ? [['Lý do lỗi', order.failReason]] : []),
                    ].map(([lbl, val]) => (
                      <div key={lbl} className="p-2 bg-white rounded-xl">
                        <div className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-0.5">{lbl}</div>
                        <div className="text-[12px] font-semibold text-slate-900 break-all">{val}</div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}

          {/* ITEMS */}
          {tab === 'items' && (
            <div className="flex flex-col gap-2">
              {(!order.items || order.items.length === 0)
                ? <div className="text-center text-slate-400 py-8">Không có sản phẩm.</div>
                : order.items.map((item, i) => (
                  <div key={item.id || i} className="flex items-center gap-3 p-3 bg-slate-50 rounded-xl">
                    {item.thumbnailUrl
                      ? <img src={toAbsoluteUrl(item.thumbnailUrl)} alt="" className="w-12 h-10 rounded-lg object-cover flex-shrink-0" />
                      : <div className="w-12 h-10 rounded-lg bg-slate-200 flex items-center justify-center text-slate-400 flex-shrink-0 text-lg">🖼</div>
                    }
                    <div className="flex-1 min-w-0">
                      <div className="text-[13px] font-bold text-slate-900 truncate">
                        {item.templateName || item.templateId}
                      </div>
                      {item.templateSlug && (
                        <div className="text-[11px] text-slate-400 mt-0.5">{item.templateSlug}</div>
                      )}
                    </div>
                    <div className="text-right flex-shrink-0">
                      <div className="text-[14px] font-extrabold text-slate-900">{fmtMoney(item.price)}</div>
                      {item.originalPrice !== item.price && (
                        <div className="text-[11px] text-slate-400 line-through">{fmtMoney(item.originalPrice)}</div>
                      )}
                    </div>
                  </div>
                ))
              }
            </div>
          )}

          {/* ACTIONS */}
          {tab === 'actions' && (
            <div className="flex flex-col gap-4">

              {/* Đổi trạng thái — chỉ hiện khi còn có thể đổi */}
              {['Pending', 'Paid'].includes(order.status) && (
                <div className="p-4 rounded-2xl border border-blue-200 bg-blue-50/40">
                  <div className="text-[13px] font-bold text-slate-900 mb-1">🔄 Cập nhật trạng thái đơn</div>
                  <div className="text-[12px] text-slate-500 mb-3">
                    Hiện tại: <strong>{order.status}</strong>
                  </div>
                  <div className="flex flex-col gap-2 mb-3">
                    <select
                      value={newStatus}
                      onChange={e => setNewStatus(e.target.value)}
                      className="w-full px-3 py-2 rounded-xl border border-blue-200 text-[13px] bg-white text-slate-800 focus:outline-none focus:ring-2 focus:ring-blue-300"
                    >
                      <option value="">— Chọn trạng thái mới —</option>
                      {order.status === 'Pending' && <option value="Paid">Paid — Đã thanh toán</option>}
                      {['Pending', 'Paid'].includes(order.status) && <option value="Completed">Completed — Hoàn tất</option>}
                    </select>
                    <Textarea
                      value={statusNote}
                      onChange={e => setStatusNote(e.target.value)}
                      rows={2}
                      placeholder="Ghi chú (tuỳ chọn)…"
                      className="border-blue-200"
                    />
                  </div>
                  <BtnSecondary onClick={doUpdateStatus} disabled={busy || !newStatus}>
                    {busy ? '…' : 'Xác nhận đổi trạng thái'}
                  </BtnSecondary>
                </div>
              )}

              {/* Huỷ đơn */}
              {order.status !== 'Cancelled' ? (
                <div className="p-4 rounded-2xl border border-red-200 bg-red-50/40">
                  <div className="text-[13px] font-bold text-slate-900 mb-3">🚫 Huỷ đơn hàng</div>
                  <Field label="Lý do huỷ" required>
                    <Textarea
                      value={reason}
                      onChange={e => setReason(e.target.value)}
                      rows={3}
                      placeholder="Nhập lý do huỷ đơn…"
                      className="border-red-200"
                    />
                  </Field>
                  <BtnDanger onClick={doCancel} disabled={busy}>
                    {busy ? '…' : 'Huỷ đơn hàng'}
                  </BtnDanger>
                </div>
              ) : (
                <div className="p-4 bg-red-50 rounded-2xl text-center text-[13px] text-red-500 font-semibold">
                  Đơn hàng đã bị huỷ.
                </div>
              )}

            </div>
          )}
        </>
      )}
    </Modal>
  );
}