import { useState, useEffect, useRef } from 'react';
import { useParams, Link } from 'react-router-dom';
import { orderApi } from '../../api/services';
import { extractError, toAbsoluteUrl } from '../../api/client';
import { LoadingPage, Price, StatusBadge, Spinner, useToast } from '../../components/ui';
import OrderPaymentModal from '../../modals/user/Orderpaymentmodal';
import OrderCancelModal from '../../modals/user/Ordercancelmodal';
import { useLang } from '../../context/Langcontext';

export default function OrderDetailPage() {
  const { t } = useLang();
  const { id } = useParams();
  const toast = useToast();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);

  const [paymentModal, setPaymentModal] = useState(false);
  const [paymentData, setPaymentData] = useState(null);
  const [paymentLoading, setPaymentLoading] = useState(false);
  const [polling, setPolling] = useState(false);
  const [pollResult, setPollResult] = useState(null);
  const pollRef = useRef(null);

  const [cancelModal, setCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [cancelLoading, setCancelLoading] = useState(false);

  const fetchOrder = () => {
    orderApi.getDetail(id)
      .then(res => setOrder(res.data.data))
      .catch(err => toast.error(extractError(err), t('order.load_err')))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchOrder(); }, [id]);
  useEffect(() => () => clearInterval(pollRef.current), []);

  const handleCreatePayment = async () => {
    setPaymentLoading(true);
    try {
      const res = await orderApi.createPayment(id);
      setPaymentData(res.data.data);
      setPaymentModal(true);
      startPolling();
    } catch (err) {
      toast.error(extractError(err), t('order.pay_err'));
    } finally { setPaymentLoading(false); }
  };

  const startPolling = () => {
    setPolling(true);
    let count = 0;
    pollRef.current = setInterval(async () => {
      count++;
      try {
        const res = await orderApi.getPaymentStatus(id);
        if (res.data.data.isPaid) {
          clearInterval(pollRef.current);
          setPolling(false);
          setPollResult(res.data.data);
          fetchOrder();
          toast.success(t('order.pay_ok'));
        }
      } catch {}
      if (count >= 60) { clearInterval(pollRef.current); setPolling(false); }
    }, 5000);
  };

  const handleCancel = async () => {
    setCancelLoading(true);
    try {
      await orderApi.cancel(id, cancelReason);
      setCancelModal(false);
      fetchOrder();
      toast.success(t('order.cancel_ok'));
    } catch (err) {
      toast.error(extractError(err), t('order.cancel_err'));
    } finally { setCancelLoading(false); }
  };

  if (loading) return <LoadingPage />;
  if (!order) return null;

  const isPending   = order.status === 'Pending';
  const isPaid      = order.status === 'Paid' || order.status === 'Completed';
  const isCancelled = order.status === 'Cancelled';

  // Payment info rows — labels translated
  const paymentRows = [
    order.bankCode        && { label: t('order.bank'),             value: order.bankCode },
    order.transferContent && { label: t('order.transfer_content'), value: (
      <span className="font-mono font-black px-2 py-0.5 rounded text-xs"
        style={{ backgroundColor: 'rgba(139,92,246,0.12)', color: '#8b5cf6' }}>
        {order.transferContent}
      </span>
    )},
    order.paymentAmount   && { label: t('order.payment_amount'),   value: <Price amount={order.paymentAmount} className="font-bold" style={{ color: '#10b981' }} /> },
    order.paidAt          && { label: t('order.paid_at'),          value: new Date(order.paidAt).toLocaleString('vi-VN') },
    order.sepayCode       && { label: t('order.sepay_code'),       value: <span className="font-mono text-xs">{order.sepayCode}</span> },
    order.failReason      && { label: t('order.fail_reason'),      value: <span style={{ color: '#ef4444' }}>{order.failReason}</span> },
  ].filter(Boolean);

  return (
    <div className="animate-fade-in">

      {/* Back */}
      <Link to="/dashboard/purchases"
        className="inline-flex items-center gap-1.5 text-sm font-medium mb-5 transition-colors"
        style={{ color: 'var(--text-secondary)' }}
        onMouseEnter={e => e.currentTarget.style.color = 'var(--text-primary)'}
        onMouseLeave={e => e.currentTarget.style.color = 'var(--text-secondary)'}
      >
        <span className="w-6 h-6 rounded-lg border flex items-center justify-center text-xs"
          style={{ backgroundColor: 'var(--bg-elevated)', borderColor: 'var(--border)' }}>←</span>
        {t('order.back')}
      </Link>

      <div className="rounded-2xl mb-4"
        style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>

        {/* Header */}
        <div className="flex items-start justify-between flex-wrap gap-3 px-5 py-4"
          style={{ borderBottom: '1px solid var(--border)' }}>
          <div>
            <p className="font-mono text-sm font-black" style={{ color: 'var(--text-primary)' }}>
              {order.orderCode}
            </p>
            <p className="text-xs mt-0.5" style={{ color: 'var(--text-muted)' }}>
              {new Date(order.createdAt).toLocaleString('vi-VN')}
            </p>
          </div>
          <div className="flex gap-2 flex-wrap">
            <StatusBadge status={order.status} />
            {order.paymentStatus && <StatusBadge status={order.paymentStatus} />}
          </div>
        </div>

        {/* Items */}
        <div className="px-5 py-4" style={{ borderBottom: '1px solid var(--border)' }}>
          <p className="text-xs font-bold uppercase tracking-widest mb-3" style={{ color: 'var(--text-muted)' }}>
            {t('order.items_title')} ({order.items?.length})
          </p>
          <div className="space-y-3">
            {order.items?.map(item => (
              <div key={item.id} className="flex items-center gap-3">
                <div className="w-14 h-11 rounded-xl overflow-hidden shrink-0"
                  style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)' }}>
                  {item.thumbnailUrl
                    ? <img src={toAbsoluteUrl(item.thumbnailUrl)} alt="" className="w-full h-full object-cover" />
                    : <div className="w-full h-full flex items-center justify-center text-xl">🗂️</div>}
                </div>
                <div className="flex-1 min-w-0">
                  <Link to={`/templates/${item.templateSlug}`}
                    className="text-sm font-semibold truncate block hover:underline"
                    style={{ color: 'var(--text-primary)' }}>
                    {item.templateName}
                  </Link>
                  <div className="flex items-center gap-2 mt-0.5">
                    {item.originalPrice !== item.price && (
                      <span className="text-xs line-through" style={{ color: 'var(--text-muted)' }}>
                        <Price amount={item.originalPrice} />
                      </span>
                    )}
                    <Price amount={item.price} className="text-xs font-bold" style={{ color: 'var(--text-secondary)' }} />
                  </div>
                </div>
                {item.downloadUrl ? (
                  <a href={toAbsoluteUrl(item.downloadUrl)} target="_blank" rel="noreferrer"
                    onClick={e => e.stopPropagation()}
                    className="flex items-center gap-1 px-2.5 py-1.5 rounded-xl text-xs font-bold shrink-0 transition-all"
                    style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-primary)' }}>
                    {t('order.download_btn')}
                  </a>
                ) : isPaid && (
                  <Link to={`/templates/${item.templateSlug}`}
                    className="flex items-center gap-1 px-2.5 py-1.5 rounded-xl text-xs font-semibold shrink-0"
                    style={{ border: '1px solid var(--border)', color: 'var(--text-muted)' }}>
                    {t('common.view')}
                  </Link>
                )}
              </div>
            ))}
          </div>
        </div>

        {/* Summary */}
        <div className="px-5 py-4" style={{ borderBottom: '1px solid var(--border)' }}>
          <p className="text-xs font-bold uppercase tracking-widest mb-3" style={{ color: 'var(--text-muted)' }}>
            {t('order.summary')}
          </p>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between">
              <span style={{ color: 'var(--text-secondary)' }}>{t('order.subtotal')}</span>
              <Price amount={order.totalAmount} className="font-medium" style={{ color: 'var(--text-primary)' }} />
            </div>
            {order.discountAmount > 0 && (
              <div className="flex justify-between">
                <span style={{ color: 'var(--text-secondary)' }}>
                  {t('order.discount')} {order.couponCode && (
                    <span className="ml-1 px-1.5 py-0.5 rounded text-xs font-bold"
                      style={{ backgroundColor: 'rgba(16,185,129,0.1)', color: '#10b981' }}>
                      {order.couponCode}
                    </span>
                  )}
                </span>
                <span className="font-semibold" style={{ color: '#10b981' }}>
                  −<Price amount={order.discountAmount} />
                </span>
              </div>
            )}
            <div className="flex justify-between font-black text-base pt-2"
              style={{ borderTop: '1px solid var(--border)', color: 'var(--text-primary)' }}>
              <span>{t('order.total')}</span>
              <Price amount={order.finalAmount} />
            </div>
          </div>
        </div>

        {/* Payment Info */}
        {paymentRows.length > 0 && (
          <div className="px-5 py-4" style={{ borderBottom: '1px solid var(--border)' }}>
            <p className="text-xs font-bold uppercase tracking-widest mb-3" style={{ color: 'var(--text-muted)' }}>
              {t('order.payment_info')}
            </p>
            <div className="rounded-xl overflow-hidden" style={{ border: '1px solid var(--border)' }}>
              {paymentRows.map(({ label, value }) => (
                <div key={label} className="flex items-center justify-between gap-3 px-4 py-3 text-sm"
                  style={{ borderBottom: '1px solid var(--border)' }}>
                  <span style={{ color: 'var(--text-muted)' }}>{label}</span>
                  <span className="text-right" style={{ color: 'var(--text-primary)' }}>{value}</span>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Cancel Reason */}
        {isCancelled && order.cancelReason && (
          <div className="px-5 py-4" style={{ borderBottom: '1px solid var(--border)' }}>
            <p className="text-xs font-bold uppercase tracking-widest mb-2" style={{ color: 'var(--text-muted)' }}>
              {t('order.cancel_reason')}
            </p>
            <div className="px-4 py-3 rounded-xl"
              style={{ backgroundColor: 'rgba(239,68,68,0.05)', border: '1px solid rgba(239,68,68,0.2)' }}>
              <p className="text-sm" style={{ color: '#ef4444' }}>{order.cancelReason}</p>
            </div>
          </div>
        )}

        {/* Note */}
        {order.note && (
          <div className="px-5 py-4" style={{ borderBottom: '1px solid var(--border)' }}>
            <p className="text-xs font-bold uppercase tracking-widest mb-2" style={{ color: 'var(--text-muted)' }}>
              {t('order.note')}
            </p>
            <p className="text-sm" style={{ color: 'var(--text-secondary)' }}>{order.note}</p>
          </div>
        )}

        {/* Actions */}
        {isPending && (
          <div className="px-5 py-4 flex gap-3 flex-wrap">
            <button onClick={handleCreatePayment} disabled={paymentLoading}
              className="flex items-center gap-2 px-5 py-2.5 rounded-xl font-bold text-sm transition-all disabled:opacity-50"
              style={{ backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)' }}>
              {paymentLoading
                ? <><Spinner /> {t('order.pay_creating')}</>
                : t('order.pay_btn')}
            </button>
            <button onClick={() => setCancelModal(true)}
              className="flex items-center gap-2 px-5 py-2.5 rounded-xl font-bold text-sm transition-all"
              style={{ border: '1px solid rgba(239,68,68,0.4)', color: '#ef4444', backgroundColor: 'rgba(239,68,68,0.05)' }}>
              {t('order.cancel_btn')}
            </button>
          </div>
        )}
      </div>

      <OrderPaymentModal
        open={paymentModal}
        onClose={() => setPaymentModal(false)}
        paymentData={paymentData}
        polling={polling}
        pollResult={pollResult}
      />

      <OrderCancelModal
        open={cancelModal}
        onClose={() => setCancelModal(false)}
        reason={cancelReason}
        onReasonChange={setCancelReason}
        onConfirm={handleCancel}
        loading={cancelLoading}
      />
    </div>
  );
}