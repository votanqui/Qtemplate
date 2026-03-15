import { useState } from 'react';
import { ticketApi } from '../../api/services';
import { extractError } from '../../api/client';
import { Spinner, FormField, useToast, Portal } from '../../components/ui';

export const CreateTicketModal = ({ open, onClose, onCreated }) => {
  const toast = useToast();
  const [form, setForm] = useState({ subject: '', message: '', templateId: '' });
  const [loading, setLoading] = useState(false);

  if (!open) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const payload = { subject: form.subject, message: form.message };
      if (form.templateId) payload.templateId = form.templateId;
      const res = await ticketApi.create(payload);
      onCreated(res.data.data.id);
    } catch (err) {
      toast.error(extractError(err), 'Tạo ticket thất bại');
    } finally {
      setLoading(false);
    }
  };

  const inputStyle = {
    backgroundColor: 'var(--bg-elevated)',
    border: '1px solid var(--border)',
    color: 'var(--text-primary)',
  };

  return (
    <Portal>
      {/* Backdrop */}
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
          className="relative w-full max-w-lg rounded-3xl shadow-2xl animate-fade-in pointer-events-auto flex flex-col max-h-[85vh]"
          style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}
          onClick={e => e.stopPropagation()}
        >
          {/* Header */}
          <div className="flex items-center justify-between px-6 py-5 shrink-0"
            style={{ borderBottom: '1px solid var(--border)' }}>
            <div>
              <h3 className="font-black text-base tracking-tight" style={{ color: 'var(--text-primary)' }}>
                Tạo ticket hỗ trợ
              </h3>
              <p className="text-xs mt-0.5" style={{ color: 'var(--text-muted)' }}>
                Đội ngũ sẽ phản hồi sớm nhất có thể
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

              <FormField label="Tiêu đề" required>
                <input
                  className="w-full px-4 py-3 rounded-xl text-sm focus:outline-none transition-all"
                  style={inputStyle}
                  placeholder="Không tải được file template..."
                  value={form.subject}
                  onChange={e => setForm(f => ({ ...f, subject: e.target.value }))}
                  required
                  onFocus={e => e.target.style.borderColor = '#0ea5e9'}
                  onBlur={e => e.target.style.borderColor = 'var(--border)'}
                />
              </FormField>

              <FormField label="Template ID (tùy chọn)">
                <input
                  className="w-full px-4 py-3 rounded-xl text-sm focus:outline-none transition-all"
                  style={inputStyle}
                  placeholder="UUID của template (nếu có)"
                  value={form.templateId}
                  onChange={e => setForm(f => ({ ...f, templateId: e.target.value }))}
                  onFocus={e => e.target.style.borderColor = '#0ea5e9'}
                  onBlur={e => e.target.style.borderColor = 'var(--border)'}
                />
              </FormField>

              <FormField label="Mô tả chi tiết" required>
                <textarea
                  className="w-full px-4 py-3 rounded-xl text-sm focus:outline-none transition-all resize-none"
                  style={inputStyle}
                  rows={5}
                  placeholder="Mô tả vấn đề bạn gặp phải..."
                  value={form.message}
                  onChange={e => setForm(f => ({ ...f, message: e.target.value }))}
                  required
                  onFocus={e => e.target.style.borderColor = '#0ea5e9'}
                  onBlur={e => e.target.style.borderColor = 'var(--border)'}
                />
              </FormField>

              <div className="flex gap-2 justify-end pt-1">
                <button
                  type="button"
                  onClick={onClose}
                  className="px-4 py-2 rounded-xl text-sm font-semibold transition-all"
                  style={{ border: '1px solid var(--border)', color: 'var(--text-secondary)' }}
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  disabled={loading}
                  className="flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-bold transition-all disabled:opacity-50"
                  style={{ backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)' }}
                >
                  {loading ? <><Spinner /> Đang gửi...</> : '📨 Tạo ticket'}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </Portal>
  );
};