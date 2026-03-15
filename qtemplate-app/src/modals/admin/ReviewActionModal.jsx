import { useState } from 'react';
import { adminReviewApi } from '../../api/adminApi';
import { toAbsoluteUrl } from '../../api/client';
import {
  Modal, Field, Textarea, BtnPrimary, BtnSecondary, BtnDanger, BtnSuccess, Toast, Chip, fmtFull,
} from '../../components/ui/AdminUI';

export default function ReviewActionModal({ review, onClose, onRefresh }) {
  const [reply,  setReply]  = useState(review?.adminReply || '');
  const [busy,   setBusy]   = useState(false);
  const [toast,  setToast]  = useState('');

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  const doApprove = async (approved) => {
    setBusy(true);
    try {
      await adminReviewApi.approve(review.id, approved);
      onRefresh();
      ok(approved ? '✅ Đã duyệt review' : '🚫 Đã từ chối review');
      setTimeout(onClose, 600);
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doReply = async () => {
    if (!reply.trim()) return alert('Nhập nội dung trả lời.');
    setBusy(true);
    try {
      await adminReviewApi.reply(review.id, reply.trim());
      onRefresh();
      ok('💬 Đã gửi trả lời');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doDelete = async () => {
    if (!window.confirm('Xoá review này?')) return;
    setBusy(true);
    try {
      await adminReviewApi.delete(review.id);
      onRefresh();
      ok('🗑 Đã xoá review');
      setTimeout(onClose, 500);
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const stars = n => '★'.repeat(n) + '☆'.repeat(5 - n);

  return (
    <Modal open={!!review} onClose={onClose} title="Chi tiết Review" width={580}>
      <Toast msg={toast} />
      {review && (
        <>
          {/* Review header */}
          <div className="p-4 bg-slate-50 rounded-2xl mb-5">
            <div className="flex items-start justify-between gap-3 mb-3">
              <div>
                <div className="font-bold text-slate-900 text-[14px]">
                  {review.userName || 'Ẩn danh'}
                </div>
                <div className="text-[11px] text-slate-400">{review.userEmail}</div>
              </div>
              <div className="text-right flex-shrink-0">
                <div className="text-amber-400 text-[16px] font-bold tracking-tight">
                  {stars(review.rating)}
                </div>
                <div className="text-[11px] text-slate-400 mt-0.5">{fmtFull(review.createdAt)}</div>
              </div>
            </div>

            <div className="text-[13px] text-slate-700 mb-2 leading-relaxed">{review.comment}</div>

            {review.templateName && (
              <div className="flex items-center gap-2 mt-3 pt-3 border-t border-slate-200">
                <Chip label="Template" color="purple" />
                <span className="text-[12px] text-slate-600 font-semibold">{review.templateName}</span>
              </div>
            )}
          </div>

          {/* Status & approve */}
          <div className="p-4 rounded-2xl border border-slate-200 mb-4">
            <div className="flex items-center justify-between mb-3">
              <span className="text-[13px] font-bold text-slate-900">Trạng thái duyệt</span>
              <Chip
                label={review.isApproved ? 'Đã duyệt' : 'Chờ duyệt'}
                color={review.isApproved ? 'green' : 'yellow'}
              />
            </div>
            <div className="flex gap-2">
              {!review.isApproved && (
                <BtnSuccess onClick={() => doApprove(true)} disabled={busy} className="flex-1">
                  ✓ Duyệt review
                </BtnSuccess>
              )}
              {review.isApproved && (
                <BtnSecondary onClick={() => doApprove(false)} disabled={busy} className="flex-1">
                  Bỏ duyệt
                </BtnSecondary>
              )}
              <BtnDanger onClick={doDelete} disabled={busy}>
                Xoá
              </BtnDanger>
            </div>
          </div>

          {/* Reply */}
          <div className="p-4 rounded-2xl border border-blue-100 bg-blue-50/30">
            <div className="text-[13px] font-bold text-slate-900 mb-3">💬 Trả lời của Admin</div>
            {review.adminReply && (
              <div className="text-[12px] text-slate-600 bg-white p-3 rounded-xl border border-blue-100 mb-3 leading-relaxed">
                {review.adminReply}
              </div>
            )}
            <Field label={review.adminReply ? 'Cập nhật trả lời' : 'Nhập trả lời'}>
              <Textarea
                value={reply}
                onChange={e => setReply(e.target.value)}
                rows={3}
                placeholder="Cảm ơn bạn đã đánh giá…"
              />
            </Field>
            <BtnPrimary onClick={doReply} disabled={busy}>
              {busy ? '…' : review.adminReply ? 'Cập nhật trả lời' : 'Gửi trả lời'}
            </BtnPrimary>
          </div>
        </>
      )}
    </Modal>
  );
}