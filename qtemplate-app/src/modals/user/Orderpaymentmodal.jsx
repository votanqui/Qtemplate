// OrderPaymentModal.js
import { Price, Spinner } from '../../components/ui';
import { createPortal } from 'react-dom';

export default function OrderPaymentModal({ open, onClose, paymentData, polling, pollResult }) {
  if (!open) return null;

  return createPortal(
    <div className="fixed inset-0 z-[99999] flex items-center justify-center bg-black/50 backdrop-blur-sm">
      
      {/* Modal content */}
      <div className="w-full max-w-md mx-4 rounded-2xl border shadow-2xl animate-fade-in"
        style={{ backgroundColor: 'var(--bg-card)', borderColor: 'var(--border)' }}>
        
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b"
          style={{ borderColor: 'var(--border)' }}>
          <h3 className="text-lg font-black" style={{ color: 'var(--text-primary)' }}>
            Thanh toán đơn hàng
          </h3>
          <button
            onClick={onClose}
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl border text-xs font-bold transition-all"
            style={{ borderColor: 'var(--border)', color: 'var(--text-secondary)' }}
          >
            <span>✕</span> Đóng
          </button>
        </div>

        {/* Body */}
        <div className="p-5">
          {pollResult?.isPaid ? (
            <div className="text-center py-6">
              <div className="w-16 h-16 rounded-2xl flex items-center justify-center text-3xl mx-auto mb-4"
                style={{ backgroundColor: 'rgba(16,185,129,0.1)', border: '1px solid rgba(16,185,129,0.3)' }}>
                ✅
              </div>
              <p className="font-black text-lg mb-1" style={{ color: 'var(--text-primary)' }}>
                Thanh toán thành công!
              </p>
              <p className="text-sm" style={{ color: 'var(--text-secondary)' }}>
                Đơn hàng đã được xác nhận
              </p>
            </div>
          ) : (
            <div className="space-y-4">
              <div className="px-4 py-3 rounded-xl border text-sm"
                style={{ backgroundColor: 'rgba(139,92,246,0.08)', borderColor: 'rgba(139,92,246,0.2)', color: '#a78bfa' }}>
                ℹ️ Chuyển khoản đúng nội dung và số tiền để đơn được xác nhận tự động.
              </div>

              {paymentData?.qrUrl && (
                <div className="flex justify-center">
                  <div className="p-3 rounded-2xl border"
                    style={{ backgroundColor: 'var(--bg-elevated)', borderColor: 'var(--border)' }}>
                    <img src={paymentData.qrUrl} alt="QR" className="rounded-xl w-48 h-48 object-contain" />
                  </div>
                </div>
              )}

              <div className="rounded-xl border overflow-hidden" style={{ borderColor: 'var(--border)' }}>
                {[
                  { label: 'Ngân hàng', value: paymentData?.bankCode },
                  { label: 'Số tài khoản', value: <span className="font-mono font-bold">{paymentData?.accountNumber}</span> },
                  { label: 'Số tiền', value: <Price amount={paymentData?.amount} className="font-black" style={{ color: '#10b981' }} /> },
                  { label: 'Nội dung CK', value: (
                    <span className="font-mono font-black px-2 py-0.5 rounded text-xs"
                      style={{ backgroundColor: 'rgba(139,92,246,0.12)', color: '#8b5cf6' }}>
                      {paymentData?.transferContent}
                    </span>
                  )},
                ].map(({ label, value }) => (
                  <div key={label} className="flex items-center justify-between gap-3 px-4 py-3 text-sm border-b last:border-0"
                    style={{ borderColor: 'var(--border)' }}>
                    <span style={{ color: 'var(--text-muted)' }}>{label}</span>
                    <span style={{ color: 'var(--text-primary)' }}>{value}</span>
                  </div>
                ))}
              </div>

              {polling && (
                <div className="flex items-center justify-center gap-2 text-sm py-1"
                  style={{ color: 'var(--text-secondary)' }}>
                  <Spinner />
                  Đang chờ xác nhận thanh toán...
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>,
    document.body
  );
}