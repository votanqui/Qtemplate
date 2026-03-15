import { useState, useEffect } from 'react';
import { adminTagApi } from '../../api/adminApi';
import {
  Modal, Field, Input, BtnPrimary, BtnSecondary, Toast,
} from '../../components/ui/AdminUI';

function slugify(str) {
  return str
    .toLowerCase()
    .normalize('NFD').replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/g, 'd')
    .replace(/[^a-z0-9\s-]/g, '')
    .trim()
    .replace(/\s+/g, '-');
}

export default function TagFormModal({ tag, onClose, onRefresh }) {
  const isEdit = !!tag?.id;
  const [form,       setForm]       = useState({ name: '', slug: '' });
  const [busy,       setBusy]       = useState(false);
  const [toast,      setToast]      = useState('');
  const [slugManual, setSlugManual] = useState(false);

  const ok  = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };
  const set = (k, v) => setForm(f => ({ ...f, [k]: v }));

  useEffect(() => {
    if (tag?.id) {
      setForm({ name: tag.name || '', slug: tag.slug || '' });
      setSlugManual(true);
    } else {
      setForm({ name: '', slug: '' });
      setSlugManual(false);
    }
  }, [tag]);

  const handleName = (v) => {
    set('name', v);
    if (!slugManual) set('slug', slugify(v));
  };

  const handleSlug = (v) => {
    set('slug', v);
    setSlugManual(true);
  };

  const doSave = async () => {
    if (!form.name.trim()) return alert('Nhập tên tag.');
    if (!form.slug.trim()) return alert('Nhập slug.');
    setBusy(true);
    try {
      const payload = { name: form.name.trim(), slug: form.slug.trim() };
      if (isEdit) {
        await adminTagApi.update(tag.id, payload);
        ok('✅ Đã cập nhật tag');
      } else {
        await adminTagApi.create(payload);
        ok('✅ Đã tạo tag');
      }
      onRefresh();
      setTimeout(onClose, 600);
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <Modal
      open={!!tag}
      onClose={onClose}
      title={isEdit ? 'Chỉnh sửa Tag' : 'Tạo Tag mới'}
      width={400}
    >
      <Toast msg={toast} />

      <div className="flex flex-col gap-3">
        <Field label="Tên tag" required>
          <Input
            value={form.name}
            onChange={e => handleName(e.target.value)}
            placeholder="React, Tailwind CSS, Dashboard…"
            autoFocus
          />
        </Field>

        <Field label="Slug" required>
          <Input
            value={form.slug}
            onChange={e => handleSlug(e.target.value)}
            placeholder="react, tailwind-css…"
            className="font-mono text-[12px]"
          />
        </Field>
      </div>

      <div className="flex justify-end gap-2 mt-4 pt-4 border-t border-slate-100">
        <BtnSecondary onClick={onClose}>Huỷ</BtnSecondary>
        <BtnPrimary onClick={doSave} disabled={busy}>
          {busy ? '…' : isEdit ? 'Lưu thay đổi' : 'Tạo tag'}
        </BtnPrimary>
      </div>
    </Modal>
  );
}