import { useState, useEffect } from 'react';
import { adminTemplateApi, adminCategoryApi, adminTagApi } from '../../api/adminApi';
import {
  Modal, Field, Input, Select, Textarea,
  BtnPrimary, BtnSecondary, Toast,
} from '../../components/ui/AdminUI';

const emptyForm = {
  name: '', slug: '', categoryId: '', shortDescription: '', description: '',
  price: '', techStack: '', compatibleWith: '', fileFormat: '', version: '1.0.0',
  isFeatured: false, isFree: false, tagIds: [], features: [],
};

function slugify(s) {
  return s.toLowerCase().trim()
    .replace(/[àáạảãâầấậẩẫăằắặẳẵ]/g, 'a')
    .replace(/[èéẹẻẽêềếệểễ]/g, 'e')
    .replace(/[ìíịỉĩ]/g, 'i')
    .replace(/[òóọỏõôồốộổỗơờớợởỡ]/g, 'o')
    .replace(/[ùúụủũưừứựửữ]/g, 'u')
    .replace(/[ỳýỵỷỹ]/g, 'y')
    .replace(/đ/g, 'd')
    .replace(/[^a-z0-9\s-]/g, '')
    .replace(/\s+/g, '-')
    .replace(/-+/g, '-');
}

export default function TemplateFormModal({ template, onClose, onRefresh }) {
  const isEdit = !!template?.id;
  const [form,       setForm]       = useState(emptyForm);
  const [categories, setCategories] = useState([]);
  const [tags,       setTags]       = useState([]);
  const [newFeature, setNewFeature] = useState('');
  const [busy,       setBusy]       = useState(false);
  const [toast,      setToast]      = useState('');

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };
  const set = (k, v) => setForm(f => ({ ...f, [k]: v }));

  // Load categories + tags
  useEffect(() => {
    adminCategoryApi.getAll().then(r => setCategories(r.data.data || [])).catch(() => {});
    adminTagApi.getAll().then(r => setTags(r.data.data || [])).catch(() => {});
  }, []);

  // Populate form when editing
  useEffect(() => {
    if (!template) return;
    if (template.id) {
      // Edit mode — load full detail by slug
      adminTemplateApi.getDetail(template.slug).then(r => {
        const d = r.data.data;
        setForm({
          name:             d.name || '',
          slug:             d.slug || '',
          categoryId:       d.category?.id || '',
          shortDescription: d.shortDescription || '',
          description:      d.description || '',
          price:            d.price ?? '',
          techStack:        d.techStack || '',
          compatibleWith:   d.compatibleWith || '',
          fileFormat:       d.fileFormat || '',
          version:          d.version || '1.0.0',
          isFeatured:       d.isFeatured ?? false,
          isFree:           d.isFree ?? false,
          tagIds:           [], // tags returned as strings — match by name below
          features:         d.features || [],
        });
        // Map tag names back to ids
        adminTagApi.getAll().then(tr => {
          const allTags = tr.data.data || [];
          const matchedIds = allTags
            .filter(t => d.tags?.includes(t.name))
            .map(t => t.id);
          setForm(f => ({ ...f, tagIds: matchedIds }));
        });
      }).catch(() => {});
    } else {
      setForm(emptyForm);
    }
  }, [template]);

  const toggleTag = id => {
    set('tagIds', form.tagIds.includes(id)
      ? form.tagIds.filter(t => t !== id)
      : [...form.tagIds, id]);
  };

  const addFeature = () => {
    const v = newFeature.trim();
    if (!v || form.features.includes(v)) return;
    set('features', [...form.features, v]);
    setNewFeature('');
  };

  const removeFeature = idx =>
    set('features', form.features.filter((_, i) => i !== idx));

  const validate = () => {
    if (!form.name.trim())   return 'Nhập tên template.';
    if (!form.slug.trim())   return 'Nhập slug.';
    if (!form.categoryId)    return 'Chọn danh mục.';
    if (!form.isFree && (form.price === '' || Number(form.price) < 0))
      return 'Nhập giá hợp lệ.';
    return null;
  };

  const doSave = async () => {
    const err = validate();
    if (err) return alert(err);
    setBusy(true);
    try {
      const payload = {
        name:             form.name.trim(),
        slug:             form.slug.trim(),
        categoryId:       Number(form.categoryId),
        shortDescription: form.shortDescription.trim() || undefined,
        description:      form.description.trim() || undefined,
        price:            form.isFree ? 0 : Number(form.price),
        techStack:        form.techStack.trim() || undefined,
        compatibleWith:   form.compatibleWith.trim() || undefined,
        fileFormat:       form.fileFormat.trim() || undefined,
        version:          form.version.trim() || undefined,
        isFeatured:       form.isFeatured,
        isFree:           form.isFree,
        tagIds:           form.tagIds,
        features:         form.features,
      };
      if (isEdit) {
        await adminTemplateApi.update(template.id, payload);
        ok('✅ Đã cập nhật template');
      } else {
        await adminTemplateApi.create(payload);
        ok('✅ Đã tạo template');
      }
      onRefresh();
      setTimeout(onClose, 600);
    } catch (e) {
      alert(e?.response?.data?.message || 'Lỗi server.');
    } finally {
      setBusy(false);
    }
  };

  return (
    <Modal
      open={!!template}
      onClose={onClose}
      title={isEdit ? `Sửa: ${template?.name || ''}` : 'Tạo Template mới'}
      width={680}
    >
      <Toast msg={toast} />

      <div className="flex flex-col gap-4 max-h-[72vh] overflow-y-auto pr-1">
        {/* Basic info */}
        <div className="p-4 rounded-2xl border border-slate-100 bg-slate-50/50">
          <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-3">Thông tin cơ bản</div>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Tên template" required className="col-span-2">
              <Input
                value={form.name}
                onChange={e => {
                  set('name', e.target.value);
                  if (!isEdit) set('slug', slugify(e.target.value));
                }}
                placeholder="Ví dụ: Landing Page Pro"
              />
            </Field>
            <Field label="Slug (URL)" required className="col-span-2">
              <Input
                value={form.slug}
                onChange={e => set('slug', slugify(e.target.value))}
                placeholder="landing-page-pro"
                className="font-mono text-[12px]"
              />
            </Field>
            <Field label="Danh mục" required className="col-span-1">
              <Select
                value={form.categoryId}
                onChange={e => set('categoryId', e.target.value)}
                className="w-full"
              >
                <option value="">-- Chọn danh mục --</option>
                {(() => {
                  const opts = [];
                  const flatten = (list, depth = 0) => {
                    list.forEach(c => {
                      opts.push(
                        <option key={c.id} value={c.id}>
                          {'　'.repeat(depth)}{depth > 0 ? '└ ' : ''}{c.name}
                        </option>
                      );
                      if (c.children?.length) flatten(c.children, depth + 1);
                    });
                  };
                  flatten(categories);
                  return opts;
                })()}
              </Select>
            </Field>
            <Field label="Phiên bản" className="col-span-1">
              <Input
                value={form.version}
                onChange={e => set('version', e.target.value)}
                placeholder="1.0.0"
              />
            </Field>
            <Field label="Mô tả ngắn" className="col-span-2">
              <Textarea
                value={form.shortDescription}
                onChange={e => set('shortDescription', e.target.value)}
                rows={2}
                placeholder="Mô tả ngắn hiển thị trên danh sách…"
              />
            </Field>
            <Field label="Mô tả đầy đủ" className="col-span-2">
              <Textarea
                value={form.description}
                onChange={e => set('description', e.target.value)}
                rows={4}
                placeholder="Mô tả chi tiết template…"
              />
            </Field>
          </div>
        </div>

        {/* Pricing */}
        <div className="p-4 rounded-2xl border border-slate-100 bg-slate-50/50">
          <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-3">Giá bán</div>
          <div className="flex items-center gap-4 mb-3">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={form.isFree}
                onChange={e => set('isFree', e.target.checked)}
                className="w-4 h-4 accent-slate-600"
              />
              <span className="text-[13px] font-semibold text-slate-700">Template miễn phí</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={form.isFeatured}
                onChange={e => set('isFeatured', e.target.checked)}
                className="w-4 h-4 accent-slate-600"
              />
              <span className="text-[13px] font-semibold text-slate-700">Nổi bật (Featured)</span>
            </label>
          </div>
          {!form.isFree && (
            <Field label="Giá (VNĐ)" required>
              <Input
                type="number"
                min={0}
                step={1000}
                value={form.price}
                onChange={e => set('price', e.target.value)}
                placeholder="199000"
              />
            </Field>
          )}
        </div>

        {/* Tech info */}
        <div className="p-4 rounded-2xl border border-slate-100 bg-slate-50/50">
          <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-3">Thông tin kỹ thuật</div>
          <div className="grid grid-cols-3 gap-3">
            <Field label="Tech Stack">
              <Input value={form.techStack} onChange={e => set('techStack', e.target.value)} placeholder="React, TailwindCSS" />
            </Field>
            <Field label="Tương thích">
              <Input value={form.compatibleWith} onChange={e => set('compatibleWith', e.target.value)} placeholder="Windows, macOS" />
            </Field>
            <Field label="Định dạng file">
              <Input value={form.fileFormat} onChange={e => set('fileFormat', e.target.value)} placeholder=".zip, .html" />
            </Field>
          </div>
        </div>

        {/* Tags */}
        {tags.length > 0 && (
          <div className="p-4 rounded-2xl border border-slate-100 bg-slate-50/50">
            <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-3">Tags</div>
            <div className="flex flex-wrap gap-2">
              {tags.map(t => (
                <button
                  key={t.id}
                  type="button"
                  onClick={() => toggleTag(t.id)}
                  className={`px-3 py-1 rounded-full text-[12px] font-semibold border transition-all ${
                    form.tagIds.includes(t.id)
                      ? 'bg-slate-900 text-white border-slate-900'
                      : 'bg-white text-slate-600 border-slate-200 hover:border-slate-400'
                  }`}
                >
                  {t.name}
                </button>
              ))}
            </div>
          </div>
        )}

        {/* Features */}
        <div className="p-4 rounded-2xl border border-slate-100 bg-slate-50/50">
          <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-3">
            Tính năng ({form.features.length})
          </div>
          <div className="flex gap-2 mb-3">
            <Input
              value={newFeature}
              onChange={e => setNewFeature(e.target.value)}
              onKeyDown={e => e.key === 'Enter' && (e.preventDefault(), addFeature())}
              placeholder="Nhập tính năng rồi Enter…"
              className="flex-1"
            />
            <BtnSecondary onClick={addFeature} className="px-4 flex-shrink-0">+ Thêm</BtnSecondary>
          </div>
          {form.features.length > 0 && (
            <div className="flex flex-col gap-1.5">
              {form.features.map((f, i) => (
                <div key={i} className="flex items-center justify-between bg-white px-3 py-2 rounded-xl border border-slate-100">
                  <span className="text-[13px] text-slate-700">✓ {f}</span>
                  <button
                    onClick={() => removeFeature(i)}
                    className="text-slate-300 hover:text-red-400 transition-colors text-[16px] leading-none"
                  >×</button>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      <div className="flex justify-end gap-2 mt-4 pt-4 border-t border-slate-100">
        <BtnSecondary onClick={onClose}>Huỷ</BtnSecondary>
        <BtnPrimary onClick={doSave} disabled={busy}>
          {busy ? 'Đang lưu…' : isEdit ? 'Lưu thay đổi' : 'Tạo template'}
        </BtnPrimary>
      </div>
    </Modal>
  );
}