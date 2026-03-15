import { useState } from 'react';
import { adminSettingApi } from '../../api/adminApi';
import {
  Modal, Field, Input, Select, Textarea,
  BtnPrimary, BtnSecondary,
} from '../../components/ui/AdminUI';

const GROUPS = ['General', 'Payment', 'Email', 'Upload', 'SEO', 'Security', 'Affiliate'];

export default function SettingCreateModal({ open, onClose, onRefresh }) {
  const [form, setForm] = useState({
    key: '', value: '', group: 'General', description: '',
  });
  const [busy, setBusy] = useState(false);
  const set = (k, v) => setForm(f => ({ ...f, [k]: v }));

  const doCreate = async () => {
    if (!form.key.trim()) return alert('Nhập key setting.');
    setBusy(true);
    try {
      await adminSettingApi.create({
        key:         form.key.trim(),
        value:       form.value.trim() || null,
        group:       form.group,
        description: form.description.trim() || null,
      });
      setForm({ key: '', value: '', group: 'General', description: '' });
      onRefresh(); onClose();
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <Modal open={open} onClose={onClose} title="Tạo Setting mới" width={460}>
      <div className="grid grid-cols-2 gap-3">
        <Field label="Key" required className="col-span-2">
          <Input
            value={form.key}
            onChange={e => set('key', e.target.value)}
            placeholder="siteName, maxUploadSize…"
            className="font-mono"
          />
        </Field>
        <Field label="Group">
          <Select value={form.group} onChange={e => set('group', e.target.value)} className="w-full">
            {GROUPS.map(g => <option key={g} value={g}>{g}</option>)}
          </Select>
        </Field>
        <Field label="Mô tả">
          <Input
            value={form.description}
            onChange={e => set('description', e.target.value)}
            placeholder="Mô tả ngắn"
          />
        </Field>
        <Field label="Giá trị" className="col-span-2">
          <Textarea
            value={form.value}
            onChange={e => set('value', e.target.value)}
            rows={2}
            placeholder="Nhập giá trị…"
            className="font-mono"
          />
        </Field>
      </div>
      <div className="flex justify-end gap-2 mt-4 pt-4 border-t border-slate-100">
        <BtnSecondary onClick={onClose}>Huỷ</BtnSecondary>
        <BtnPrimary onClick={doCreate} disabled={busy}>
          {busy ? '…' : 'Tạo setting'}
        </BtnPrimary>
      </div>
    </Modal>
  );
}