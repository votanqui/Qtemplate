import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { orderApi } from '../../api/services';
import { extractError, toAbsoluteUrl } from '../../api/client';
import { LoadingPage, Pagination, Price, StatusBadge, EmptyState, useToast } from '../../components/ui';
import { useLang } from '../../context/Langcontext';
const STATUS_TABS = [
  { value: null,        label: 'Tất cả' },
  { value: 'Pending',   label: 'Chờ xử lý' },
  { value: 'Paid',      label: 'Đã thanh toán' },
  { value: 'Completed', label: 'Hoàn thành' },
  { value: 'Cancelled', label: 'Đã hủy' },
];

export default function PurchasesPage() {
  const toast = useToast();
  const [data, setData] = useState(null);
  const [page, setPage] = useState(1);
  const [status, setStatus] = useState(null);
  const [loading, setLoading] = useState(true);
const { t } = useLang();
  useEffect(() => {
    setLoading(true);
    orderApi.getMyOrders(page, 10, status)
      .then(res => setData(res.data.data))
      .catch(err => toast.error(extractError(err), 'Không thể tải đơn hàng'))
      .finally(() => setLoading(false));
  }, [page, status]);

  const handleTabChange = (val) => { setStatus(val); setPage(1); };

  return (
    <div className="animate-fade-in ">

      {/* Header */}
      <div className="mb-6">
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-semibold uppercase tracking-wider mb-3"
          style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
          🛍️ Đơn hàng
        </div>
        <h1 className="text-2xl font-black tracking-tight" style={{ color: 'var(--text-primary)' }}>
          Đơn mua
        </h1>
        <p className="text-sm mt-1" style={{ color: 'var(--text-muted)' }}>
          Tổng <span className="font-bold" style={{ color: 'var(--text-secondary)' }}>{data?.totalCount || 0}</span> đơn hàng
        </p>
      </div>

      {/* Status tabs */}
      <div className="flex items-center gap-1.5 flex-wrap mb-5">
        {STATUS_TABS.map(tab => (
          <button
            key={String(tab.value)}
            onClick={() => handleTabChange(tab.value)}
            className="px-3 py-1.5 rounded-xl text-xs font-semibold transition-all border"
            style={status === tab.value ? {
              backgroundColor: 'var(--sidebar-active-bg)',
              color: 'var(--sidebar-active-text)',
              borderColor: 'transparent',
            } : {
              backgroundColor: 'var(--bg-elevated)',
              color: 'var(--text-secondary)',
              borderColor: 'var(--border)',
            }}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {loading ? <LoadingPage /> : !data?.items?.length ? (
        <EmptyState
          icon="🛍️"
          title="Chưa có đơn hàng nào"
          description={status ? 'Không có đơn với trạng thái này.' : undefined}
        />
      ) : (
        <div className="space-y-3">
          {data.items.map(order => (
            <div
              key={order.orderId}
              className="rounded-2xl transition-colors"
              style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}
            >
              {/* Order header */}
              <div className="flex items-start justify-between flex-wrap gap-3 px-5 py-4"
                style={{ borderBottom: '1px solid var(--border)' }}>
                <div>
                  <p className="font-mono text-sm font-bold" style={{ color: 'var(--text-primary)' }}>
                    {order.orderCode}
                  </p>
                  <p className="text-xs mt-0.5" style={{ color: 'var(--text-muted)' }}>
                    {new Date(order.createdAt).toLocaleString('vi-VN')}
                  </p>
                </div>
                <div className="flex items-center gap-2 flex-wrap">
                  <StatusBadge status={order.status} />
                  {order.paymentStatus && <StatusBadge status={order.paymentStatus} />}
                </div>
              </div>

              {/* Items */}
              <div className="px-5 py-3 space-y-1">
                {order.items?.map(item => (
                  <Link
                    key={item.id}
                    to={`/templates/${item.templateSlug}`}
                    className="flex items-center gap-3 py-2 px-2 -mx-2 rounded-xl transition-colors"
                    onMouseEnter={e => e.currentTarget.style.backgroundColor = 'var(--bg-elevated)'}
                    onMouseLeave={e => e.currentTarget.style.backgroundColor = 'transparent'}
                  >
                    {item.thumbnailUrl ? (
                      <img
                        src={toAbsoluteUrl(item.thumbnailUrl)}
                        alt={item.templateName}
                        className="w-12 h-9 rounded-lg object-cover shrink-0"
                        style={{ border: '1px solid var(--border)' }}
                      />
                    ) : (
                      <div className="w-12 h-9 rounded-lg shrink-0 flex items-center justify-center text-lg"
                        style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)' }}>
                        🗂️
                      </div>
                    )}
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-semibold truncate" style={{ color: 'var(--text-primary)' }}>
                        {item.templateName}
                      </p>
                      {item.originalPrice !== item.price && (
                        <p className="text-xs line-through" style={{ color: 'var(--text-muted)' }}>
                          <Price amount={item.originalPrice} />
                        </p>
                      )}
                    </div>
                    <Price amount={item.price} className="text-sm font-bold shrink-0"
                      style={{ color: 'var(--text-primary)' }} />
                  </Link>
                ))}
              </div>

              {/* Footer */}
              <div className="flex items-center justify-between px-5 py-3"
                style={{ borderTop: '1px solid var(--border)' }}>
                <div>
                  {order.couponCode && (
                    <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium"
                      style={{ border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
                      🏷️ {order.couponCode}
                      {order.discountAmount > 0 && <> −<Price amount={order.discountAmount} /></>}
                    </span>
                  )}
                </div>
                <div className="flex items-center gap-4">
                  <div className="text-right">
                    <p className="text-xs" style={{ color: 'var(--text-muted)' }}>Tổng cộng</p>
                    <p className="text-base font-black" style={{ color: 'var(--text-primary)' }}>
                      <Price amount={order.finalAmount} />
                    </p>
                  </div>
                  <Link
                    to={`/dashboard/orders/${order.orderId}`}
                    className="px-3 py-1.5 rounded-xl text-xs font-semibold transition-all"
                    style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-primary)' }}
                    onClick={e => e.stopPropagation()}
                  >
                    Chi tiết →
                  </Link>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <Pagination page={page} totalPages={data?.totalPages || 1} onPageChange={setPage} />
    </div>
  );
}