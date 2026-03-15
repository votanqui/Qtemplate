// OrderCancelModal.js
import { Spinner } from '../../components/ui';
import { createPortal } from 'react-dom';

export default function OrderCancelModal({ open, onClose, reason, onReasonChange, onConfirm, loading }) {
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
            Hủy đơn hàng
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
          <div className="space-y-4">
            <div className="px-4 py-3 rounded-xl border text-sm"
              style={{ backgroundColor: 'rgba(245,158,11,0.08)', borderColor: 'rgba(245,158,11,0.25)', color: '#f59e0b' }}>
              ⚠️ Sau khi hủy, đơn hàng không thể khôi phục.
            </div>

            <textarea
              rows={3}
              placeholder="Lý do hủy (không bắt buộc)..."
              value={reason}
              onChange={e => onReasonChange(e.target.value)}
              className="w-full px-4 py-3 rounded-xl border text-sm resize-none focus:outline-none"
              style={{ backgroundColor: 'var(--input-bg)', borderColor: 'var(--input-border)', color: 'var(--input-text)' }}
            />

            <div className="flex gap-2 justify-end">
              <button onClick={onClose}
                className="px-4 py-2 rounded-xl border text-sm font-semibold transition-all"
                style={{ borderColor: 'var(--border)', color: 'var(--text-secondary)', backgroundColor: 'var(--bg-elevated)' }}>
                Đóng
              </button>
              <button onClick={onConfirm} disabled={loading}
                className="flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-bold text-white transition-all disabled:opacity-50"
                style={{ backgroundColor: '#ef4444' }}>
                {loading ? <><Spinner /> Đang hủy...</> : 'Xác nhận hủy'}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>,
    document.body
  );
}