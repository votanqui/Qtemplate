import { useState } from 'react';
import { adminIpBlacklistApi } from '../../api/adminApi';
import {
  Modal, Field, Input, BtnDanger, BtnSecondary,
} from '../../components/ui/AdminUI';

export default function IpBlacklistAddModal({ open, onClose, onRefresh }) {
  const [form, setForm] = useState({ ipAddress: '', reason: '', expiredAt: '' });
  const [busy, setBusy] = useState(false);
  const set = (k, v) => setForm(f => ({ ...f, [k]: v }));

  const doAdd = async () => {
    if (!form.ipAddress.trim()) return alert('Nhập địa chỉ IP.');
    setBusy(true);
    try {
      await adminIpBlacklistApi.add(
        form.ipAddress.trim(),
        form.reason.trim() || null,
        form.expiredAt ? new Date(form.expiredAt).toISOString() : null,
      );
      setForm({ ipAddress: '', reason: '', expiredAt: '' });
      onRefresh(); onClose();
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <Modal open={open} onClose={onClose} title="Thêm IP vào Blacklist" width={440}>
      <div className="flex flex-col gap-3">
        <Field label="Địa chỉ IP" required>
          <Input
            value={form.ipAddress}
            onChange={e => set('ipAddress', e.target.value)}
            placeholder="192.168.1.1  hoặc  10.0.0.0/24"
            className="font-mono"
          />
          <p className="text-[11px] text-slate-400 mt-1">Hỗ trợ IPv4, IPv6 và CIDR range.</p>
        </Field>
        <Field label="Lý do chặn">
          <Input
            value={form.reason}
            onChange={e => set('reason', e.target.value)}
            placeholder="Spam, brute force, scraping…"
          />
        </Field>
        <Field label="Hết hạn (để trống = vĩnh viễn)">
          <Input
            type="datetime-local"
            value={form.expiredAt}
            onChange={e => set('expiredAt', e.target.value)}
          />
        </Field>
        <div className="flex justify-end gap-2 pt-2">
          <BtnSecondary onClick={onClose}>Huỷ</BtnSecondary>
          <BtnDanger onClick={doAdd} disabled={busy}>
            {busy ? '…' : '🚫 Thêm vào blacklist'}
          </BtnDanger>
        </div>
      </div>
    </Modal>
  );
}