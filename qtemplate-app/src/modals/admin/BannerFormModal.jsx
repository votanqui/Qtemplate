import { useState, useEffect, useRef } from 'react';
import { adminBannerApi } from '../../api/adminApi';
import { toAbsoluteUrl } from '../../api/client';
import {
  Modal, Field, Input, Select, BtnPrimary, BtnSecondary, Toast,
} from '../../components/ui/AdminUI';

const empty = {
  title: '', subTitle: '', imageUrl: '', linkUrl: '',
  position: 'Home', sortOrder: 0, isActive: true,
  startAt: '', endAt: '',
};

export default function BannerFormModal({ banner, onClose, onRefresh }) {
  const isEdit = !!banner?.id;
  const [form,      setForm]      = useState(empty);
  const [imageFile, setImageFile] = useState(null);
  const [preview,   setPreview]   = useState(null);
  const [busy,      setBusy]      = useState(false);
  const [toast,     setToast]     = useState('');
  const fileRef = useRef();

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  useEffect(() => {
    if (banner?.id) {
      setForm({
        title:     banner.title     || '',
        subTitle:  banner.subTitle  || '',
        imageUrl:  banner.imageUrl  || '',
        linkUrl:   banner.linkUrl   || '',
        position:  banner.position  || 'Home',
        sortOrder: banner.sortOrder ?? 0,
        isActive:  banner.isActive  ?? true,
        startAt:   banner.startAt   ? banner.startAt.slice(0, 16)  : '',
        endAt:     banner.endAt     ? banner.endAt.slice(0, 16)    : '',
      });
      setPreview(banner.imageUrl ? toAbsoluteUrl(banner.imageUrl) : null);
    } else {
      setForm(empty);
      setPreview(null);
    }
    setImageFile(null);
  }, [banner]);

  const set = (k, v) => setForm(f => ({ ...f, [k]: v }));

  const onFileChange = e => {
    const f = e.target.files?.[0];
    if (!f) return;
    setImageFile(f);
    setPreview(URL.createObjectURL(f));
  };

  const doSave = async () => {
    if (!form.title.trim()) return alert('Nhập tiêu đề banner.');
    if (!imageFile && !form.imageUrl) return alert('Chọn ảnh hoặc nhập URL ảnh.');
    setBusy(true);
    try {
      const params = {
        title:     form.title.trim(),
        subTitle:  form.subTitle.trim() || undefined,
        imageUrl:  form.imageUrl.trim() || undefined,
        linkUrl:   form.linkUrl.trim()  || undefined,
        position:  form.position,
        sortOrder: Number(form.sortOrder),
        isActive:  form.isActive,
        startAt:   form.startAt ? new Date(form.startAt).toISOString() : undefined,
        endAt:     form.endAt   ? new Date(form.endAt).toISOString()   : undefined,
      };
      if (isEdit) {
        await adminBannerApi.update(banner.id, params, imageFile || null);
        ok('✅ Đã cập nhật banner');
      } else {
        await adminBannerApi.create(params, imageFile || null);
        ok('✅ Đã tạo banner');
      }
      onRefresh();
      setTimeout(onClose, 600);
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <Modal
      open={!!banner}
      onClose={onClose}
      title={isEdit ? 'Chỉnh sửa Banner' : 'Tạo Banner mới'}
      width={560}
    >
      <Toast msg={toast} />

      {/* Image preview */}
      <div
        className="relative w-full h-36 bg-slate-100 rounded-2xl overflow-hidden mb-2 cursor-pointer border-2 border-dashed border-slate-200 hover:border-slate-400 transition-colors flex items-center justify-center"
        onClick={() => fileRef.current?.click()}
      >
        {preview
          ? <img src={preview} alt="" className="w-full h-full object-cover" />
          : <div className="text-center text-slate-400">
              <div className="text-3xl mb-1">🖼</div>
              <div className="text-[12px]">Click để chọn ảnh</div>
            </div>
        }
        <input ref={fileRef} type="file" accept="image/*" className="hidden" onChange={onFileChange} />
        {preview && (
          <div className="absolute inset-0 bg-black/0 hover:bg-black/20 transition-colors flex items-center justify-center">
            <span className="text-white text-[12px] font-semibold opacity-0 hover:opacity-100 bg-black/50 px-3 py-1 rounded-full">
              Đổi ảnh
            </span>
          </div>
        )}
      </div>

      {/* Show current image source info */}
      {imageFile ? (
        <div className="mb-4 px-3 py-2 bg-green-50 border border-green-100 rounded-xl text-[11px] text-green-700 font-semibold">
          📁 File: {imageFile.name} · {(imageFile.size / 1024).toFixed(0)} KB
        </div>
      ) : form.imageUrl ? (
        <div className="mb-4 px-3 py-2 bg-blue-50 border border-blue-100 rounded-xl text-[11px] text-blue-600 break-all">
          🔗 {toAbsoluteUrl(form.imageUrl)}
        </div>
      ) : (
        <div className="mb-4" />
      )}

      <div className="grid grid-cols-2 gap-3">
        <Field label="Tiêu đề" required className="col-span-2">
          <Input value={form.title} onChange={e => set('title', e.target.value)} placeholder="Tiêu đề banner…" />
        </Field>

        <Field label="Phụ đề" className="col-span-2">
          <Input value={form.subTitle} onChange={e => set('subTitle', e.target.value)} placeholder="Mô tả ngắn…" />
        </Field>

        <Field label="URL ảnh (nếu không upload)" className="col-span-2">
          <Input value={form.imageUrl} onChange={e => set('imageUrl', e.target.value)} placeholder="https://…" />
        </Field>

        <Field label="Link khi click" className="col-span-2">
          <Input value={form.linkUrl} onChange={e => set('linkUrl', e.target.value)} placeholder="https://…" />
        </Field>

        <Field label="Vị trí">
          <Select value={form.position} onChange={e => set('position', e.target.value)} className="w-full">
            <option value="Home">Home</option>
            <option value="Sidebar">Sidebar</option>
            <option value="Templates">Templates</option>
          </Select>
        </Field>

        <Field label="Thứ tự hiển thị">
          <Input type="number" min={0} value={form.sortOrder} onChange={e => set('sortOrder', e.target.value)} />
        </Field>

        <Field label="Trạng thái">
          <Select value={String(form.isActive)} onChange={e => set('isActive', e.target.value === 'true')} className="w-full">
            <option value="true">Hiển thị</option>
            <option value="false">Ẩn</option>
          </Select>
        </Field>

        <Field label="Bắt đầu">
          <Input type="datetime-local" value={form.startAt} onChange={e => set('startAt', e.target.value)} />
        </Field>

        <Field label="Kết thúc">
          <Input type="datetime-local" value={form.endAt} onChange={e => set('endAt', e.target.value)} />
        </Field>
      </div>

      <div className="flex justify-end gap-2 mt-4 pt-4 border-t border-slate-100">
        <BtnSecondary onClick={onClose}>Huỷ</BtnSecondary>
        <BtnPrimary onClick={doSave} disabled={busy}>
          {busy ? '…' : isEdit ? 'Lưu thay đổi' : 'Tạo banner'}
        </BtnPrimary>
      </div>
    </Modal>
  );
}