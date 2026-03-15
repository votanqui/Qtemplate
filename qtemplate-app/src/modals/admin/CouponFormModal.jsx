import { useState, useEffect } from 'react';
import { adminCouponApi } from '../../api/adminApi';
import {
  Modal, Field, Input, Select, BtnPrimary, BtnSecondary, Toast,
} from '../../components/ui/AdminUI';

const empty = {
  code: '', type: 'Percentage', value: '',
  minOrderAmount: '', maxDiscountAmount: '',
  usageLimit: '', startAt: '', expiredAt: '',
};

export default function CouponFormModal({ coupon, onClose, onRefresh }) {
  const isEdit = !!coupon?.id;
  const [form, setForm] = useState(empty);
  const [busy, setBusy] = useState(false);
  const [toast, setToast] = useState('');

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  useEffect(() => {
    if (coupon?.id) {
      setForm({
        code:               coupon.code || '',
        type:               coupon.type || 'Percentage',
        value:              coupon.value ?? '',
        minOrderAmount:     coupon.minOrderAmount ?? '',
        maxDiscountAmount:  coupon.maxDiscountAmount ?? '',
        usageLimit:         coupon.usageLimit ?? '',
        startAt:            coupon.startAt ? coupon.startAt.slice(0, 16) : '',
        expiredAt:          coupon.expiredAt ? coupon.expiredAt.slice(0, 16) : '',
        isActive:           coupon.isActive ?? true,
      });
    } else {
      setForm(empty);
    }
  }, [coupon]);

  const set = (k, v) => setForm(f => ({ ...f, [k]: v }));

  const doSave = async () => {
    if (!form.code.trim()) return alert('Nhập mã coupon.');
    if (!form.value) return alert('Nhập giá trị giảm.');
    setBusy(true);
    try {
      const payload = {
        code:             form.code.trim().toUpperCase(),
        type:             form.type,
        value:            Number(form.value),
        ...(form.minOrderAmount    && { minOrderAmount:    Number(form.minOrderAmount) }),
        ...(form.maxDiscountAmount && { maxDiscountAmount: Number(form.maxDiscountAmount) }),
        ...(form.usageLimit        && { usageLimit:        Number(form.usageLimit) }),
        ...(form.startAt           && { startAt:           new Date(form.startAt).toISOString() }),
        ...(form.expiredAt         && { expiredAt:         new Date(form.expiredAt).toISOString() }),
      };
      if (isEdit) {
        await adminCouponApi.update(coupon.id, { ...payload, isActive: form.isActive ?? true });
        ok('✅ Đã cập nhật coupon');
      } else {
        await adminCouponApi.create(payload);
        ok('✅ Đã tạo coupon');
      }
      onRefresh();
      setTimeout(onClose, 600);
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <Modal
      open={!!coupon}
      onClose={onClose}
      title={isEdit ? 'Chỉnh sửa Coupon' : 'Tạo Coupon mới'}
      width={520}
    >
      <Toast msg={toast} />

      <div className="grid grid-cols-2 gap-3">
        <Field label="Mã Coupon" required className="col-span-2">
          <Input
            value={form.code}
            onChange={e => set('code', e.target.value.toUpperCase())}
            placeholder="SALE20, FREESHIP…"
            disabled={isEdit}
            className={isEdit ? 'opacity-60' : ''}
          />
        </Field>

        <Field label="Loại giảm" required>
          <Select value={form.type} onChange={e => set('type', e.target.value)} className="w-full">
            <option value="Percentage">Phần trăm (%)</option>
            <option value="Fixed">Số tiền cố định (₫)</option>
          </Select>
        </Field>

        <Field label={form.type === 'Percentage' ? 'Giá trị (%)' : 'Giá trị (₫)'} required>
          <Input
            type="number" min={0}
            value={form.value}
            onChange={e => set('value', e.target.value)}
            placeholder={form.type === 'Percentage' ? '20' : '50000'}
          />
        </Field>

        <Field label="Đơn tối thiểu (₫)">
          <Input
            type="number" min={0}
            value={form.minOrderAmount}
            onChange={e => set('minOrderAmount', e.target.value)}
            placeholder="0"
          />
        </Field>

        <Field label="Giảm tối đa (₫)">
          <Input
            type="number" min={0}
            value={form.maxDiscountAmount}
            onChange={e => set('maxDiscountAmount', e.target.value)}
            placeholder="Không giới hạn"
          />
        </Field>

        <Field label="Giới hạn dùng">
          <Input
            type="number" min={0}
            value={form.usageLimit}
            onChange={e => set('usageLimit', e.target.value)}
            placeholder="Không giới hạn"
          />
        </Field>

        {isEdit && (
          <Field label="Kích hoạt">
            <Select value={String(form.isActive)} onChange={e => set('isActive', e.target.value === 'true')} className="w-full">
              <option value="true">Đang hoạt động</option>
              <option value="false">Tắt</option>
            </Select>
          </Field>
        )}

        <Field label="Bắt đầu">
          <Input
            type="datetime-local"
            value={form.startAt}
            onChange={e => set('startAt', e.target.value)}
          />
        </Field>

        <Field label="Hết hạn">
          <Input
            type="datetime-local"
            value={form.expiredAt}
            onChange={e => set('expiredAt', e.target.value)}
          />
        </Field>
      </div>

      <div className="flex justify-end gap-2 mt-4 pt-4 border-t border-slate-100">
        <BtnSecondary onClick={onClose}>Huỷ</BtnSecondary>
        <BtnPrimary onClick={doSave} disabled={busy}>
          {busy ? '…' : isEdit ? 'Lưu thay đổi' : 'Tạo coupon'}
        </BtnPrimary>
      </div>
    </Modal>
  );
}