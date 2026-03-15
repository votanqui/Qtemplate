import { useState, useEffect } from 'react';
import { userApi } from '../../api/services';
import { extractError } from '../../api/client';
import { StarRating, FormField, Spinner, useToast, Portal } from '../../components/ui'; // 👈 thêm Portal

export const ReviewEditModal = ({ review, onClose, onSaved }) => {
  const toast = useToast();
  const [form, setForm] = useState({ rating: 5, title: '', comment: '' });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (review) {
      setForm({ rating: review.rating, title: review.title || '', comment: review.comment || '' });
    }
  }, [review]);

  if (!review) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await userApi.updateReview(review.id, form);
      onSaved();
    } catch (err) {
      toast.error(extractError(err), 'Cập nhật thất bại');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Portal> {/* 👈 render thẳng vào body, thoát khỏi mọi container */}
      {/* Backdrop — phủ toàn màn hình */}
      <div
        className="fixed inset-0 bg-black/50 backdrop-blur-sm"
        style={{ zIndex: 200 }}
        onClick={onClose}
      />

      {/* Modal */}
      <div
        className="fixed inset-0 flex items-start justify-center p-4 pt-20 lg:items-center lg:pt-4 pointer-events-none"
        style={{ zIndex: 201 }}
      >
        <div
          className="relative w-full max-w-md rounded-3xl shadow-2xl animate-fade-in pointer-events-auto flex flex-col max-h-[80vh]"
          style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}
          onClick={e => e.stopPropagation()}
        >
          {/* Header */}
          <div className="flex items-center justify-between px-6 py-5 shrink-0"
            style={{ borderBottom: '1px solid var(--border)' }}>
            <div>
              <h3 className="font-black text-base tracking-tight" style={{ color: 'var(--text-primary)' }}>
                Sửa đánh giá
              </h3>
              <p className="text-xs mt-0.5 truncate max-w-[220px]" style={{ color: 'var(--text-muted)' }}>
                {review.templateName}
              </p>
            </div>
            <button
              onClick={onClose}
              className="w-8 h-8 rounded-lg flex items-center justify-center text-xl leading-none transition-all"
              style={{ backgroundColor: 'var(--bg-elevated)', color: 'var(--text-secondary)' }}
            >×</button>
          </div>

          {/* Body */}
          <div className="p-6 overflow-y-auto">
            <form onSubmit={handleSubmit} className="space-y-4">
              <FormField label="Đánh giá sao" required>
                <StarRating value={form.rating} onChange={v => setForm(f => ({ ...f, rating: v }))} />
              </FormField>

              <FormField label="Tiêu đề">
                <input
                  className="w-full px-4 py-3 rounded-xl text-sm transition-all focus:outline-none"
                  style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-primary)' }}
                  value={form.title}
                  onChange={e => setForm(f => ({ ...f, title: e.target.value }))}
                  placeholder="Tiêu đề đánh giá"
                  onFocus={e => e.target.style.borderColor = '#0ea5e9'}
                  onBlur={e => e.target.style.borderColor = 'var(--border)'}
                />
              </FormField>

              <FormField label="Nhận xét">
                <textarea
                  className="w-full px-4 py-3 rounded-xl text-sm transition-all focus:outline-none resize-none"
                  style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-primary)' }}
                  rows={4}
                  value={form.comment}
                  onChange={e => setForm(f => ({ ...f, comment: e.target.value }))}
                  placeholder="Nhận xét của bạn..."
                  onFocus={e => e.target.style.borderColor = '#0ea5e9'}
                  onBlur={e => e.target.style.borderColor = 'var(--border)'}
                />
              </FormField>

              <div className="flex gap-2 justify-end pt-1">
                <button type="button" onClick={onClose}
                  className="px-4 py-2 rounded-xl text-sm font-semibold transition-all"
                  style={{ border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
                  Hủy
                </button>
                <button type="submit" disabled={loading}
                  className="flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-bold transition-all disabled:opacity-50"
                  style={{ backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)' }}>
                  {loading ? <><Spinner /> Đang lưu...</> : 'Lưu thay đổi'}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </Portal>
  );
};