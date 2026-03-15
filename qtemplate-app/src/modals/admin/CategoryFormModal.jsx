import { useState, useEffect } from 'react';
import { adminCategoryApi } from '../../api/adminApi';
import {
  Modal, Field, Input, Select, BtnPrimary, BtnSecondary, Toast, ActiveDot,
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

const empty = {
  parentId: '', name: '', slug: '', description: '',
  iconUrl: '', sortOrder: 0, isActive: true,
};

export default function CategoryFormModal({ category, onClose, onRefresh, categories }) {
  const isEdit = !!category?.id;
  const [form, setForm] = useState(empty);
  const [busy, setBusy] = useState(false);
  const [toast, setToast] = useState('');
  const [slugManual, setSlugManual] = useState(false);

  const ok  = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };
  const set = (k, v) => setForm(f => ({ ...f, [k]: v }));

  useEffect(() => {
    if (category?.id) {
      setForm({
        parentId:    category.parentId ?? '',
        name:        category.name        || '',
        slug:        category.slug        || '',
        description: category.description || '',
        iconUrl:     category.iconUrl     || '',
        sortOrder:   category.sortOrder   ?? 0,
        isActive:    category.isActive    ?? true,
      });
      setSlugManual(true);
    } else {
      setForm(empty);
      setSlugManual(false);
    }
  }, [category]);

  const handleName = (v) => {
    set('name', v);
    if (!slugManual) set('slug', slugify(v));
  };

  const handleSlug = (v) => {
    set('slug', v);
    setSlugManual(true);
  };

  // Flatten categories for parent select (loại bỏ chính nó khi edit)
  const flatCats = [];
  const flatten = (list, depth = 0) => {
    list.forEach(c => {
      if (isEdit && c.id === category.id) return;
      flatCats.push({ ...c, depth });
      if (c.children?.length) flatten(c.children, depth + 1);
    });
  };
  flatten(categories || []);

  const doSave = async () => {
    if (!form.name.trim()) return alert('Nhập tên danh mục.');
    if (!form.slug.trim()) return alert('Nhập slug.');
    setBusy(true);
    try {
      const payload = {
        parentId:    form.parentId ? Number(form.parentId) : null,
        name:        form.name.trim(),
        slug:        form.slug.trim(),
        description: form.description.trim() || null,
        iconUrl:     form.iconUrl.trim()     || null,
        sortOrder:   Number(form.sortOrder)  || 0,
        isActive:    form.isActive,
      };
      if (isEdit) {
        await adminCategoryApi.update(category.id, payload);
        ok('✅ Đã cập nhật danh mục');
      } else {
        await adminCategoryApi.create(payload);
        ok('✅ Đã tạo danh mục');
      }
      onRefresh();
      setTimeout(onClose, 600);
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <Modal
      open={!!category}
      onClose={onClose}
      title={isEdit ? 'Chỉnh sửa danh mục' : 'Tạo danh mục mới'}
      width={500}
    >
      <Toast msg={toast} />

      <div className="grid grid-cols-2 gap-3">
        <Field label="Danh mục cha">
          <Select value={form.parentId} onChange={e => set('parentId', e.target.value)} className="w-full">
            <option value="">— Danh mục gốc —</option>
            {flatCats.map(c => (
              <option key={c.id} value={c.id}>
                {'　'.repeat(c.depth)}{c.depth > 0 ? '└ ' : ''}{c.name}
              </option>
            ))}
          </Select>
        </Field>

        <Field label="Thứ tự (sortOrder)">
          <Input
            type="number" min={0}
            value={form.sortOrder}
            onChange={e => set('sortOrder', e.target.value)}
          />
        </Field>

        <Field label="Tên danh mục" required className="col-span-2">
          <Input
            value={form.name}
            onChange={e => handleName(e.target.value)}
            placeholder="Frontend Templates, UI Kits…"
            autoFocus
          />
        </Field>

        <Field label="Slug" required className="col-span-2">
          <Input
            value={form.slug}
            onChange={e => handleSlug(e.target.value)}
            placeholder="frontend-templates"
            className="font-mono text-[12px]"
          />
        </Field>

        <Field label="Mô tả" className="col-span-2">
          <Input
            value={form.description}
            onChange={e => set('description', e.target.value)}
            placeholder="Mô tả ngắn về danh mục…"
          />
        </Field>

        <Field label="Icon URL">
          <Input
            value={form.iconUrl}
            onChange={e => set('iconUrl', e.target.value)}
            placeholder="https://…/icon.svg"
          />
        </Field>

        <Field label="Trạng thái">
          <Select value={String(form.isActive)} onChange={e => set('isActive', e.target.value === 'true')} className="w-full">
            <option value="true">Hiển thị</option>
            <option value="false">Ẩn</option>
          </Select>
        </Field>
      </div>

      <div className="flex justify-end gap-2 mt-4 pt-4 border-t border-slate-100">
        <BtnSecondary onClick={onClose}>Huỷ</BtnSecondary>
        <BtnPrimary onClick={doSave} disabled={busy}>
          {busy ? '…' : isEdit ? 'Lưu thay đổi' : 'Tạo danh mục'}
        </BtnPrimary>
      </div>
    </Modal>
  );
}