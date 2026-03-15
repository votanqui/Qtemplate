import { useState } from 'react';
import { adminNotificationApi } from '../../api/adminApi';
import {
  Modal, Field, Input, Select, Textarea,
  BtnPrimary, BtnSecondary, Toast,
} from '../../components/ui/AdminUI';

export default function NotificationSendModal({ open, onClose }) {
  const [form, setForm] = useState({
    userId: '', title: '', message: '', type: 'Info', redirectUrl: '',
  });
  const [busy,  setBusy]  = useState(false);
  const [toast, setToast] = useState('');

  const ok  = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };
  const set = (k, v) => setForm(f => ({ ...f, [k]: v }));

  const doSend = async () => {
    if (!form.title.trim())   return alert('Nhập tiêu đề thông báo.');
    if (!form.message.trim()) return alert('Nhập nội dung thông báo.');
    setBusy(true);
    try {
      await adminNotificationApi.send({
        userId:      form.userId.trim() || null,
        title:       form.title.trim(),
        message:     form.message.trim(),
        type:        form.type,
        redirectUrl: form.redirectUrl.trim() || null,
      });
      ok(form.userId ? '✅ Đã gửi thông báo đến user' : '📢 Đã broadcast đến tất cả user');
      setForm({ userId: '', title: '', message: '', type: 'Info', redirectUrl: '' });
      setTimeout(onClose, 800);
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi gửi thông báo.'); }
    finally { setBusy(false); }
  };

  const isBroadcast = !form.userId.trim();

  return (
    <Modal open={open} onClose={onClose} title="Gửi thông báo" width={500}>
      <Toast msg={toast} />

      <div className="flex flex-col gap-3">
        <Field label="User ID (để trống = broadcast tất cả)">
          <Input
            value={form.userId}
            onChange={e => set('userId', e.target.value)}
            placeholder="UUID user — để trống để gửi tất cả"
            className="font-mono text-[12px]"
          />
          {isBroadcast && (
            <p className="text-[11px] text-amber-500 mt-1 font-semibold">
              ⚠️ Sẽ broadcast đến TẤT CẢ user đang online
            </p>
          )}
        </Field>

        <div className="grid grid-cols-2 gap-3">
          <Field label="Tiêu đề" required>
            <Input
              value={form.title}
              onChange={e => set('title', e.target.value)}
              placeholder="Thông báo hệ thống"
            />
          </Field>
          <Field label="Loại">
            <Select value={form.type} onChange={e => set('type', e.target.value)} className="w-full">
              <option value="Info">ℹ️ Info</option>
              <option value="Success">✅ Success</option>
              <option value="Warning">⚠️ Warning</option>
            </Select>
          </Field>
        </div>

        <Field label="Nội dung" required>
          <Textarea
            value={form.message}
            onChange={e => set('message', e.target.value)}
            rows={3}
            placeholder="Nội dung thông báo…"
          />
        </Field>

        <Field label="Redirect URL (tuỳ chọn)">
          <Input
            value={form.redirectUrl}
            onChange={e => set('redirectUrl', e.target.value)}
            placeholder="/dashboard/orders, /templates/…"
          />
        </Field>

        <div className="flex justify-end gap-2 pt-2 border-t border-slate-100 mt-1">
          <BtnSecondary onClick={onClose}>Huỷ</BtnSecondary>
          <BtnPrimary onClick={doSend} disabled={busy}>
            {busy ? '…' : isBroadcast ? '📢 Broadcast' : '📨 Gửi'}
          </BtnPrimary>
        </div>
      </div>
    </Modal>
  );
}