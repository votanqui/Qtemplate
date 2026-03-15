import { useState, useEffect } from 'react';
import { adminTemplateApi } from '../../api/adminApi';
import { fmtMoney, Modal, Field, Input, BtnPrimary, BtnSecondary, BtnDanger, Toast, SearchInput, Chip } from '../../components/ui/AdminUI';

const statusColor = { Draft: 'slate', Published: 'green', Archived: 'orange' };

export default function BulkSaleModal({ open, onClose, onRefresh }) {
  const [step,       setStep]       = useState('select'); // 'select' | 'config'
  const [templates,  setTemplates]  = useState([]);
  const [loading,    setLoading]    = useState(false);
  const [search,     setSearch]     = useState('');
  const [selected,   setSelected]   = useState(new Set());

  const [mode,       setMode]       = useState('set');
  const [salePrice,  setSalePrice]  = useState('');
  const [startAt,    setStartAt]    = useState('');
  const [endAt,      setEndAt]      = useState('');
  const [busy,       setBusy]       = useState(false);
  const [toast,      setToast]      = useState('');

  const ok  = msg => { setToast(msg); setTimeout(() => setToast(''), 2800); };
  const err = msg => alert(msg);

  // Load tất cả templates khi mở modal
  useEffect(() => {
    if (!open) return;
    setStep('select');
    setSelected(new Set());
    setSalePrice(''); setStartAt(''); setEndAt('');
    setMode('set'); setSearch('');
    setLoading(true);
    adminTemplateApi.getList({ page: 1, pageSize: 200 })
      .then(r => {
        const items = r.data.data?.items || r.data.data || [];
        setTemplates(items);
      })
      .catch(() => setTemplates([]))
      .finally(() => setLoading(false));
  }, [open]);

  const filtered = templates.filter(t =>
    !search.trim() ||
    t.name.toLowerCase().includes(search.toLowerCase()) ||
    t.slug.toLowerCase().includes(search.toLowerCase())
  );

  // Chỉ hiện template có thể sale (không phải free)
  const paidOnly = filtered.filter(t => !t.isFree);

  const allChecked  = paidOnly.length > 0 && paidOnly.every(t => selected.has(t.id));
  const someChecked = paidOnly.some(t => selected.has(t.id));

  const toggleOne = id =>
    setSelected(prev => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });

  const toggleAll = () =>
    setSelected(allChecked ? new Set() : new Set(paidOnly.map(t => t.id)));

  const selectedList = templates.filter(t => selected.has(t.id));

  const doSave = async () => {
    if (selected.size === 0) return err('Chưa chọn template nào.');
    if (mode === 'set') {
      if (!salePrice || Number(salePrice) <= 0) return err('Nhập giá sale hợp lệ (> 0).');
      if (startAt && endAt && startAt >= endAt) return err('Ngày bắt đầu phải trước ngày kết thúc.');
    }
    setBusy(true);
    try {
      const r = await adminTemplateApi.bulkSetSale({
        templateIds: [...selected],
        salePrice:   mode === 'set' ? Number(salePrice) : null,
        saleStartAt: mode === 'set' && startAt ? new Date(startAt).toISOString() : null,
        saleEndAt:   mode === 'set' && endAt   ? new Date(endAt).toISOString()   : null,
      });
      ok(r.data?.message || '✅ Thành công');
      onRefresh();
      setTimeout(onClose, 800);
    } catch (e) {
      err(e?.response?.data?.message || 'Lỗi server.');
    } finally { setBusy(false); }
  };

  return (
    <Modal open={open} onClose={onClose} title="🔥 Sale hàng loạt" width={640}>
      <Toast msg={toast} />

      {/* Step tabs */}
      <div className="flex gap-0 mb-5 bg-slate-100 rounded-xl p-1">
        {[['select', `1. Chọn template${selected.size > 0 ? ` (${selected.size})` : ''}`], ['config', '2. Cấu hình sale']].map(([s, lbl]) => (
          <button
            key={s}
            onClick={() => s === 'config' && selected.size > 0 ? setStep('config') : setStep('select')}
            className={`flex-1 py-2 rounded-lg text-[12px] font-bold transition-all ${
              step === s ? 'bg-white shadow text-slate-900' : 'text-slate-500 hover:text-slate-700'
            }`}
          >
            {lbl}
          </button>
        ))}
      </div>

      {/* ── STEP 1: Chọn template ── */}
      {step === 'select' && (
        <>
          <div className="flex items-center gap-3 mb-3">
            <SearchInput
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder="Tìm tên, slug…"
              className="flex-1"
            />
            <button
              onClick={toggleAll}
              className="px-3 py-2 text-[12px] font-bold rounded-xl border border-slate-200 hover:border-slate-400 text-slate-600 whitespace-nowrap transition-colors"
            >
              {allChecked ? 'Bỏ tất cả' : 'Chọn tất cả'}
            </button>
          </div>

          {loading ? (
            <div className="text-center text-slate-400 py-10">Đang tải…</div>
          ) : (
            <div className="flex flex-col gap-1.5 max-h-[42vh] overflow-y-auto pr-1">
              {filtered.length === 0 && (
                <div className="text-center text-slate-400 py-8">Không tìm thấy template.</div>
              )}
              {filtered.map(t => {
                const isSelected = selected.has(t.id);
                const disabled   = t.isFree;
                return (
                  <div
                    key={t.id}
                    onClick={() => !disabled && toggleOne(t.id)}
                    className={`flex items-center gap-3 px-3 py-2.5 rounded-xl border transition-all ${
                      disabled
                        ? 'opacity-40 cursor-not-allowed border-slate-100 bg-slate-50'
                        : isSelected
                          ? 'border-red-300 bg-red-50/60 cursor-pointer'
                          : 'border-slate-100 bg-white hover:border-slate-300 cursor-pointer'
                    }`}
                  >
                    <input
                      type="checkbox"
                      checked={isSelected}
                      disabled={disabled}
                      onChange={() => !disabled && toggleOne(t.id)}
                      onClick={e => e.stopPropagation()}
                      className="w-4 h-4 accent-red-500 flex-shrink-0 cursor-pointer"
                    />
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 flex-wrap">
                        <span className="text-[13px] font-semibold text-slate-900 truncate">{t.name}</span>
                        <Chip label={t.status} color={statusColor[t.status] || 'slate'} />
                        {t.isFree && <Chip label="Miễn phí" color="green" />}
                        {t.salePrice && <Chip label="🔥 Sale" color="red" />}
                      </div>
                      <div className="text-[11px] text-slate-400 font-mono">{t.slug}</div>
                    </div>
                    <div className="text-right flex-shrink-0">
                      {t.isFree
                        ? <span className="text-[11px] text-green-600 font-bold">Miễn phí</span>
                        : <div>
                            <div className="text-[12px] font-bold text-slate-900">{fmtMoney(t.price)}</div>
                            {t.salePrice && <div className="text-[10px] text-red-500">Sale: {fmtMoney(t.salePrice)}</div>}
                          </div>
                      }
                    </div>
                  </div>
                );
              })}
            </div>
          )}

          <div className="flex justify-between items-center mt-4 pt-4 border-t border-slate-100">
            <span className="text-[12px] text-slate-500">
              {selected.size > 0
                ? <span className="font-bold text-red-500">{selected.size} template đã chọn</span>
                : 'Chưa chọn template nào'}
            </span>
            <div className="flex gap-2">
              <BtnSecondary onClick={onClose}>Huỷ</BtnSecondary>
              <BtnPrimary
                onClick={() => setStep('config')}
                disabled={selected.size === 0}
              >
                Tiếp theo →
              </BtnPrimary>
            </div>
          </div>
        </>
      )}

      {/* ── STEP 2: Cấu hình sale ── */}
      {step === 'config' && (
        <>
          {/* Summary chips */}
          <div className="flex flex-wrap gap-1.5 mb-4 p-3 bg-slate-50 rounded-xl border border-slate-100 max-h-20 overflow-y-auto">
            {selectedList.map(t => (
              <span key={t.id} className="text-[11px] font-semibold px-2.5 py-1 bg-white border border-slate-200 rounded-lg text-slate-700 truncate max-w-[180px]">
                {t.name}
              </span>
            ))}
          </div>

          {/* Mode toggle */}
          <div className="flex gap-2 mb-4">
            {[['set', '🔥 Đặt sale'], ['remove', '✕ Xóa sale']].map(([m, lbl]) => (
              <button
                key={m}
                onClick={() => setMode(m)}
                className={`flex-1 py-2.5 rounded-xl text-[13px] font-bold border transition-all ${
                  mode === m
                    ? m === 'set' ? 'bg-red-500 text-white border-red-500' : 'bg-slate-800 text-white border-slate-800'
                    : 'bg-white text-slate-600 border-slate-200 hover:border-slate-400'
                }`}
              >
                {lbl}
              </button>
            ))}
          </div>

          {mode === 'set' ? (
            <div className="flex flex-col gap-3">
              <Field label="Giá sale (VNĐ)" required>
                <Input
                  type="number" min={1} step={1000}
                  value={salePrice}
                  onChange={e => setSalePrice(e.target.value)}
                  placeholder="Ví dụ: 99000"
                  autoFocus
                />
                {salePrice && Number(salePrice) > 0 && (
                  <div className="text-[11px] text-red-500 font-semibold mt-1">→ {fmtMoney(Number(salePrice))}</div>
                )}
              </Field>
              <div className="grid grid-cols-2 gap-3">
                <Field label="Bắt đầu (tùy chọn)">
                  <Input type="datetime-local" value={startAt} onChange={e => setStartAt(e.target.value)} />
                </Field>
                <Field label="Kết thúc (tùy chọn)">
                  <Input type="datetime-local" value={endAt} onChange={e => setEndAt(e.target.value)} />
                </Field>
              </div>
              <p className="text-[11px] text-slate-400">
                • Template có giá gốc ≤ giá sale sẽ bị bỏ qua tự động.<br />
                • Để trống ngày kết thúc = sale vô thời hạn.
              </p>
            </div>
          ) : (
            <div className="p-4 bg-amber-50 border border-amber-200 rounded-xl text-[13px] text-amber-800">
              Xóa sale của <strong>{selected.size} template</strong> — giá sẽ trở về giá gốc.
            </div>
          )}

          <div className="flex justify-between items-center mt-5 pt-4 border-t border-slate-100">
            <button onClick={() => setStep('select')} className="text-[12px] text-slate-500 hover:text-slate-700 font-semibold">
              ← Quay lại
            </button>
            <div className="flex gap-2">
              <BtnSecondary onClick={onClose} disabled={busy}>Huỷ</BtnSecondary>
              {mode === 'set'
                ? <button
                    onClick={doSave} disabled={busy}
                    className="px-4 py-2 rounded-xl text-[13px] font-bold bg-red-500 hover:bg-red-600 text-white transition-colors disabled:opacity-50"
                  >
                    {busy ? 'Đang lưu…' : `🔥 Đặt sale ${selected.size} template`}
                  </button>
                : <BtnDanger onClick={doSave} disabled={busy}>
                    {busy ? 'Đang xử lý…' : `Xóa sale ${selected.size} template`}
                  </BtnDanger>
              }
            </div>
          </div>
        </>
      )}
    </Modal>
  );
}