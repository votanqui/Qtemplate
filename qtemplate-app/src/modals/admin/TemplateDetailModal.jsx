import { useState, useEffect, useRef } from 'react';
import { adminTemplateApi } from '../../api/adminApi';
import { toAbsoluteUrl } from '../../api/client';
import {
  Modal, Tabs, Chip, Field, Input, Textarea,
  BtnPrimary, BtnSecondary, BtnDanger, BtnSuccess, Toast,
  fmtFull, fmtMoney, fmt, ActiveDot,
} from '../../components/ui/AdminUI';

const statusColor = {
  Draft: 'slate', Published: 'green', Archived: 'orange', Rejected: 'red',
};

// ── Upload progress bar ────────────────────────────────────
function ProgressBar({ pct }) {
  if (pct == null) return null;
  return (
    <div className="w-full bg-slate-100 rounded-full h-1.5 mt-2 overflow-hidden">
      <div
        className="h-full bg-slate-800 rounded-full transition-all duration-200"
        style={{ width: `${pct}%` }}
      />
    </div>
  );
}

// ── Section wrapper ────────────────────────────────────────
function Section({ title, children }) {
  return (
    <div className="p-4 rounded-2xl border border-slate-100 bg-slate-50/40">
      <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-3">{title}</div>
      {children}
    </div>
  );
}

export default function TemplateDetailModal({ template, onClose, onRefresh }) {
  const [tab,      setTab]      = useState('media');
  const [detail,   setDetail]   = useState(null);
  const [versions, setVersions] = useState([]);
  const [loading,  setLoading]  = useState(true);
  const [busy,     setBusy]     = useState(false);
  const [toast,    setToast]    = useState('');

  // Upload progress
  const [thumbPct,   setThumbPct]   = useState(null);
  const [previewPct, setPreviewPct] = useState(null);
  const [versionPct, setVersionPct] = useState(null);

  // Pricing form
  const [pricingForm, setPricingForm] = useState({ isFree: false, price: '' });
  // Sale form
  const [saleForm, setSaleForm] = useState({ salePrice: '', saleStartAt: '', saleEndAt: '' });
  // Preview URL form
  const [previewUrl, setPreviewUrl] = useState('');
  // Version form
  const [verForm, setVerForm] = useState({
    mode: 'file', version: '', changeLog: '', externalUrl: '', storageType: 'GoogleDrive',
  });

  const thumbRef   = useRef();
  const previewRef = useRef();
  const versionRef = useRef();
  const imageRef   = useRef();

  const ok  = msg => { setToast(msg); setTimeout(() => setToast(''), 2800); };
  const err = msg => alert(msg);

  const loadDetail = async () => {
    if (!template?.slug) return;
    setLoading(true);
    try {
      const r  = await adminTemplateApi.getDetail(template.slug);
      const d  = r.data.data;
      setDetail(d);
      setPricingForm({ isFree: d.isFree, price: d.price ?? '' });
      setSaleForm({
        salePrice:  d.salePrice  ?? '',
        saleStartAt: d.saleStartAt ? d.saleStartAt.slice(0, 16) : '',
        saleEndAt:   d.saleEndAt   ? d.saleEndAt.slice(0, 16)   : '',
      });
      setPreviewUrl(d.previewUrl || '');
      const vr = await adminTemplateApi.getVersions(d.id);
      setVersions(vr.data.data || []);
    } catch { setDetail(null); }
    finally { setLoading(false); }
  };

  useEffect(() => {
    if (template) { setTab('media'); loadDetail(); }
  }, [template?.slug]);

  if (!template) return null;

  /* ── helpers ── */
  const doPublish = async () => {
    setBusy(true);
    try {
      await adminTemplateApi.publish(detail.id);
      ok('🚀 Đã publish template');
      loadDetail(); onRefresh();
    } catch (e) { err(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doChangeStatus = async (status) => {
    setBusy(true);
    try {
      await adminTemplateApi.changeStatus(detail.id, status);
      ok(`✅ Đã chuyển sang ${status}`);
      loadDetail(); onRefresh();
    } catch (e) { err(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doDelete = async () => {
    if (!window.confirm(`Xoá template "${detail.name}"? Không thể hoàn tác!`)) return;
    setBusy(true);
    try {
      await adminTemplateApi.delete(detail.id);
      ok('🗑 Đã xoá template');
      onRefresh(); onClose();
    } catch (e) { err(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  /* ── thumbnail ── */
  const doUploadThumb = async (file) => {
    setBusy(true); setThumbPct(0);
    try {
      await adminTemplateApi.uploadThumbnail(detail.id, file);
      ok('✅ Đã cập nhật thumbnail');
      loadDetail();
    } catch (e) { err(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); setThumbPct(null); }
  };

  const doDeleteThumb = async () => {
    if (!window.confirm('Xoá thumbnail này?')) return;
    setBusy(true);
    try {
      await adminTemplateApi.deleteThumbnail(detail.id);
      ok('🗑 Đã xoá thumbnail');
      loadDetail();
    } catch (e) { err(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  /* ── preview zip ── */
  const doUploadPreview = async (file) => {
    setBusy(true); setPreviewPct(0);
    try {
      await adminTemplateApi.uploadPreview(detail.id, file, e => {
        if (e.total) setPreviewPct(Math.round(e.loaded / e.total * 100));
      });
      ok('✅ Đã upload preview ZIP');
      loadDetail();
    } catch (e) { err(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); setPreviewPct(null); }
  };

  const doDeletePreview = async () => {
    if (!window.confirm('Xoá preview?')) return;
    setBusy(true);
    try {
      await adminTemplateApi.deletePreview(detail.id);
      ok('🗑 Đã xoá preview');
      loadDetail();
    } catch (e) { err(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doSetPreviewUrl = async () => {
    setBusy(true);
    try {
      await adminTemplateApi.setPreviewUrl(detail.id, previewUrl.trim() || null);
      ok('✅ Đã lưu Preview URL');
      loadDetail();
    } catch (e) { err(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  /* ── gallery images ── */
  const doAddImage = async (file) => {
    setBusy(true);
    try {
      await adminTemplateApi.addImage(detail.id, file, 'Screenshot', detail.images?.length ?? 0);
      ok('✅ Đã thêm ảnh gallery');
      loadDetail();
    } catch (e) { err(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doDeleteImage = async (imageId) => {
    setBusy(true);
    try {
      await adminTemplateApi.deleteImage(imageId);
      ok('🗑 Đã xoá ảnh');
      loadDetail();
    } catch (e) { err(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  /* ── versions ── */
  const doAddVersion = async () => {
    if (!verForm.version.trim()) return err('Nhập số phiên bản.');
    setBusy(true); setVersionPct(0);
    try {
      if (verForm.mode === 'link') {
        if (!verForm.externalUrl.trim()) return err('Nhập External URL.');
        await adminTemplateApi.addVersionLink(detail.id, {
          version:     verForm.version.trim(),
          changeLog:   verForm.changeLog.trim() || undefined,
          externalUrl: verForm.externalUrl.trim(),
          storageType: verForm.storageType,
        });
      } else {
        const file = versionRef.current?.files?.[0];
        if (!file) return err('Chọn file ZIP để upload.');
        await adminTemplateApi.addVersion(detail.id, file, verForm.version.trim(), verForm.changeLog.trim() || null, e => {
          if (e.total) setVersionPct(Math.round(e.loaded / e.total * 100));
        });
      }
      ok('✅ Đã thêm version mới');
      setVerForm(f => ({ ...f, version: '', changeLog: '', externalUrl: '' }));
      if (versionRef.current) versionRef.current.value = '';
      const vr = await adminTemplateApi.getVersions(detail.id);
      setVersions(vr.data.data || []);
    } catch (e) { err(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); setVersionPct(null); }
  };

  const doDeleteVersion = async (version) => {
    if (!window.confirm(`Xoá version ${version}?`)) return;
    setBusy(true);
    try {
      await adminTemplateApi.deleteVersion(detail.id, version);
      ok(`🗑 Đã xoá version ${version}`);
      const vr = await adminTemplateApi.getVersions(detail.id);
      setVersions(vr.data.data || []);
    } catch (e) { err(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  /* ── pricing ── */
  const doSavePricing = async () => {
    setBusy(true);
    try {
      await adminTemplateApi.changePricing(detail.id, {
        isFree: pricingForm.isFree,
        price:  pricingForm.isFree ? 0 : Number(pricingForm.price),
      });
      ok('✅ Đã cập nhật giá');
      loadDetail(); onRefresh();
    } catch (e) { err(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doSaveSale = async () => {
    setBusy(true);
    try {
      await adminTemplateApi.setSale(detail.id, {
        salePrice:  saleForm.salePrice !== '' ? Number(saleForm.salePrice) : null,
        saleStartAt: saleForm.saleStartAt ? new Date(saleForm.saleStartAt).toISOString() : null,
        saleEndAt:   saleForm.saleEndAt   ? new Date(saleForm.saleEndAt).toISOString()   : null,
      });
      ok('✅ Đã cập nhật sale');
      loadDetail(); onRefresh();
    } catch (e) { err(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  /* ── render ── */
  return (
    <Modal
      open={!!template}
      onClose={onClose}
      title={detail?.name || template?.name || 'Quản lý Template'}
      width={720}
    >
      <Toast msg={toast} />

      {loading ? (
        <div className="text-center text-slate-400 py-16">Đang tải…</div>
      ) : !detail ? (
        <div className="text-center text-red-500 py-8">Không tải được thông tin template.</div>
      ) : (
        <>
          {/* Header summary */}
          <div className="flex items-center gap-4 p-4 bg-slate-50 rounded-2xl mb-5">
            <div className="w-16 h-12 rounded-xl overflow-hidden bg-slate-200 flex-shrink-0">
              {detail.thumbnailUrl
                ? <img src={toAbsoluteUrl(detail.thumbnailUrl)} alt="" className="w-full h-full object-cover" />
                : <div className="w-full h-full flex items-center justify-center text-slate-400 text-xl">🖼</div>
              }
            </div>
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 flex-wrap mb-1">
                <Chip label={detail.status} color={statusColor[detail.status] || 'slate'} />
                {detail.isFree && <Chip label="Miễn phí" color="green" />}
                {detail.isFeatured && <Chip label="⭐ Featured" color="yellow" />}
                {detail.salePrice && <Chip label="🔥 On Sale" color="red" />}
              </div>
              <div className="text-[12px] text-slate-400 font-mono">{detail.slug}</div>
            </div>
            <div className="text-right flex-shrink-0">
              {detail.isFree
                ? <div className="text-[16px] font-extrabold text-green-600">Miễn phí</div>
                : <>
                    <div className="text-[17px] font-extrabold text-slate-900">{fmtMoney(detail.salePrice ?? detail.price)}</div>
                    {detail.salePrice && <div className="text-[11px] text-slate-400 line-through">{fmtMoney(detail.price)}</div>}
                  </>
              }
              <div className="text-[11px] text-slate-400 mt-1">{detail.salesCount} lượt mua · ⭐ {detail.averageRating?.toFixed(1) || '0'}</div>
            </div>
          </div>

          <Tabs
            tabs={[
              ['media',    '🖼 Media'],
              ['versions', `📦 Versions (${versions.length})`],
              ['pricing',  '💰 Giá & Sale'],
              ['actions',  '⚙️ Thao tác'],
            ]}
            active={tab} onChange={setTab}
          />

          <div className="max-h-[52vh] overflow-y-auto pr-1 flex flex-col gap-4">

            {/* ── MEDIA TAB ── */}
            {tab === 'media' && (
              <>
                {/* Thumbnail */}
                <Section title="Thumbnail (ảnh đại diện)">
                  <div className="flex gap-4 items-start">
                    <div
                      className="w-32 h-24 rounded-xl overflow-hidden bg-slate-100 border-2 border-dashed border-slate-200 flex-shrink-0 cursor-pointer hover:border-slate-400 transition-colors flex items-center justify-center"
                      onClick={() => thumbRef.current?.click()}
                    >
                      {detail.thumbnailUrl
                        ? <img src={toAbsoluteUrl(detail.thumbnailUrl)} alt="" className="w-full h-full object-cover" />
                        : <span className="text-3xl">🖼</span>
                      }
                    </div>
                    <div className="flex-1">
                      <input ref={thumbRef} type="file" accept="image/*" className="hidden"
                        onChange={e => { const f = e.target.files?.[0]; if (f) doUploadThumb(f); }} />
                      <div className="flex gap-2 flex-wrap">
                        <BtnSecondary onClick={() => thumbRef.current?.click()} disabled={busy} className="text-[12px] py-1.5 px-3">
                          📁 Upload ảnh mới
                        </BtnSecondary>
                        {detail.thumbnailUrl && (
                          <BtnDanger onClick={doDeleteThumb} disabled={busy} className="text-[12px] py-1.5 px-3">
                            🗑 Xoá
                          </BtnDanger>
                        )}
                      </div>
                      <p className="text-[11px] text-slate-400 mt-2">Tối đa 5MB · JPG, PNG, WebP</p>
                      <ProgressBar pct={thumbPct} />
                    </div>
                  </div>
                </Section>

                {/* Preview */}
                <Section title="Preview (ZIP HTML hoặc URL)">
                  {/* Preview URL */}
                  <div className="mb-3">
                    <Field label="Preview URL (iframe / external link)">
                      <div className="flex gap-2">
                        <Input
                          value={previewUrl}
                          onChange={e => setPreviewUrl(e.target.value)}
                          placeholder="https://preview.example.com/template"
                          className="flex-1"
                        />
                        <BtnSecondary onClick={doSetPreviewUrl} disabled={busy} className="flex-shrink-0 text-[12px] py-1.5 px-3">
                          Lưu URL
                        </BtnSecondary>
                      </div>
                    </Field>
                  </div>

                  {/* Preview ZIP */}
                  <div>
                    <div className="text-[11px] text-slate-500 font-semibold mb-2">hoặc upload file ZIP preview (max 50MB):</div>
                    <div className="flex gap-2 flex-wrap">
                      <input ref={previewRef} type="file" accept=".zip" className="hidden"
                        onChange={e => { const f = e.target.files?.[0]; if (f) doUploadPreview(f); }} />
                      <BtnSecondary onClick={() => previewRef.current?.click()} disabled={busy} className="text-[12px] py-1.5 px-3">
                        📦 Upload ZIP
                      </BtnSecondary>
                      {detail.previewFolder && (
                        <BtnDanger onClick={doDeletePreview} disabled={busy} className="text-[12px] py-1.5 px-3">
                          🗑 Xoá preview
                        </BtnDanger>
                      )}
                    </div>
                    {detail.previewFolder && (
                      <div className="mt-2 text-[11px] text-green-600 font-semibold">
                        ✓ Đã có preview: <span className="font-mono">{detail.previewFolder}</span>
                      </div>
                    )}
                    <ProgressBar pct={previewPct} />
                  </div>
                </Section>

                {/* Gallery images */}
                <Section title={`Ảnh gallery (${detail.images?.length ?? 0})`}>
                  <div className="grid grid-cols-3 gap-2 mb-3">
                    {detail.images?.map((img, i) => (
                      <div key={i} className="relative group rounded-xl overflow-hidden bg-slate-100 aspect-video">
                        <img src={toAbsoluteUrl(img.imageUrl)} alt={img.altText || ''} className="w-full h-full object-cover" />
                        <div className="absolute inset-0 bg-black/0 group-hover:bg-black/40 transition-colors flex items-center justify-center">
                          <button
                            onClick={() => doDeleteImage(img.id)}
                            disabled={busy}
                            className="opacity-0 group-hover:opacity-100 bg-red-500 text-white text-[11px] font-bold px-2.5 py-1 rounded-lg transition-opacity"
                          >
                            Xoá
                          </button>
                        </div>
                        <div className="absolute bottom-1 left-1 bg-black/50 text-white text-[9px] px-1.5 py-0.5 rounded">
                          {img.type} · #{img.sortOrder}
                        </div>
                      </div>
                    ))}

                    {/* Add image button */}
                    <div
                      className="rounded-xl border-2 border-dashed border-slate-200 hover:border-slate-400 transition-colors aspect-video flex flex-col items-center justify-center cursor-pointer text-slate-400 hover:text-slate-600"
                      onClick={() => imageRef.current?.click()}
                    >
                      <span className="text-2xl">+</span>
                      <span className="text-[11px] font-semibold mt-1">Thêm ảnh</span>
                    </div>
                  </div>
                  <input ref={imageRef} type="file" accept="image/*" multiple className="hidden"
                    onChange={async e => {
                      for (const f of Array.from(e.target.files || [])) await doAddImage(f);
                      if (imageRef.current) imageRef.current.value = '';
                    }} />
                  <p className="text-[11px] text-slate-400">Tối đa 5MB / ảnh · Loại: Screenshot</p>
                </Section>
              </>
            )}

            {/* ── VERSIONS TAB ── */}
            {tab === 'versions' && (
              <>
                {/* Version list */}
                {versions.length === 0
                  ? <div className="text-center text-slate-400 py-6">Chưa có version nào.</div>
                  : (
                    <div className="flex flex-col gap-2">
                      {versions.map(v => (
                        <div key={v.id} className="flex items-center justify-between p-3 bg-slate-50 rounded-xl border border-slate-100">
                          <div className="flex-1 min-w-0">
                            <div className="flex items-center gap-2">
                              <span className="font-mono text-[13px] font-bold text-slate-900">v{v.version}</span>
                              {v.isLatest && <Chip label="Latest" color="green" />}
                            </div>
                            {v.changeLog && <p className="text-[11px] text-slate-500 mt-0.5 truncate">{v.changeLog}</p>}
                            <p className="text-[10px] text-slate-400 mt-0.5">{fmtFull(v.createdAt)}</p>
                          </div>
                          <BtnDanger
                            onClick={() => doDeleteVersion(v.version)}
                            disabled={busy}
                            className="text-[11px] py-1 px-2.5 ml-3 flex-shrink-0"
                          >
                            Xoá
                          </BtnDanger>
                        </div>
                      ))}
                    </div>
                  )
                }

                {/* Add version form */}
                <Section title="Thêm version mới">
                  {/* Mode toggle */}
                  <div className="flex gap-3 mb-4">
                    {[['file', '📁 Upload file'], ['link', '🔗 External link']].map(([m, lbl]) => (
                      <button
                        key={m}
                        onClick={() => setVerForm(f => ({ ...f, mode: m }))}
                        className={`flex-1 py-2 rounded-xl text-[12px] font-semibold border transition-all ${
                          verForm.mode === m
                            ? 'bg-slate-900 text-white border-slate-900'
                            : 'bg-white text-slate-600 border-slate-200 hover:border-slate-400'
                        }`}
                      >
                        {lbl}
                      </button>
                    ))}
                  </div>

                  <div className="grid grid-cols-2 gap-3">
                    <Field label="Số phiên bản" required>
                      <Input
                        value={verForm.version}
                        onChange={e => setVerForm(f => ({ ...f, version: e.target.value }))}
                        placeholder="1.0.1"
                      />
                    </Field>
                    {verForm.mode === 'link' && (
                      <Field label="Storage type">
                        <select
                          value={verForm.storageType}
                          onChange={e => setVerForm(f => ({ ...f, storageType: e.target.value }))}
                          className="w-full px-3 py-2 rounded-xl border border-slate-200 text-[13px] bg-white"
                        >
                          <option value="GoogleDrive">Google Drive</option>
                          <option value="S3">S3</option>
                          <option value="R2">Cloudflare R2</option>
                        </select>
                      </Field>
                    )}
                    <Field label="Ghi chú (changelog)" className="col-span-2">
                      <Input
                        value={verForm.changeLog}
                        onChange={e => setVerForm(f => ({ ...f, changeLog: e.target.value }))}
                        placeholder="Sửa lỗi responsive, thêm dark mode…"
                      />
                    </Field>
                    {verForm.mode === 'link' ? (
                      <Field label="External URL" required className="col-span-2">
                        <Input
                          value={verForm.externalUrl}
                          onChange={e => setVerForm(f => ({ ...f, externalUrl: e.target.value }))}
                          placeholder="https://drive.google.com/file/…"
                        />
                      </Field>
                    ) : (
                      <Field label="File ZIP (max 50MB)" className="col-span-2">
                        <input
                          ref={versionRef}
                          type="file"
                          accept=".zip"
                          className="block w-full text-[12px] text-slate-600 file:mr-3 file:py-1.5 file:px-3 file:rounded-lg file:border-0 file:text-[12px] file:font-semibold file:bg-slate-900 file:text-white hover:file:bg-slate-700 cursor-pointer"
                        />
                        <ProgressBar pct={versionPct} />
                      </Field>
                    )}
                  </div>
                  <div className="mt-3">
                    <BtnPrimary onClick={doAddVersion} disabled={busy}>
                      {busy ? 'Đang upload…' : '+ Thêm version'}
                    </BtnPrimary>
                  </div>
                </Section>
              </>
            )}

            {/* ── PRICING TAB ── */}
            {tab === 'pricing' && (
              <>
                {/* Pricing */}
                <Section title="Giá bán">
                  <div className="flex items-center gap-4 mb-3">
                    <label className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={pricingForm.isFree}
                        onChange={e => setPricingForm(f => ({ ...f, isFree: e.target.checked }))}
                        className="w-4 h-4 accent-slate-600"
                      />
                      <span className="text-[13px] font-semibold text-slate-700">Miễn phí</span>
                    </label>
                  </div>
                  {!pricingForm.isFree && (
                    <Field label="Giá (VNĐ)" required>
                      <Input
                        type="number"
                        min={0}
                        step={1000}
                        value={pricingForm.price}
                        onChange={e => setPricingForm(f => ({ ...f, price: e.target.value }))}
                        placeholder="199000"
                      />
                    </Field>
                  )}
                  <div className="mt-3">
                    <BtnPrimary onClick={doSavePricing} disabled={busy}>
                      {busy ? '…' : 'Lưu giá'}
                    </BtnPrimary>
                  </div>
                </Section>

                {/* Sale */}
                <Section title="🔥 Cài đặt Sale">
                  <div className="grid grid-cols-3 gap-3">
                    <Field label="Giá sale (VNĐ)">
                      <Input
                        type="number"
                        min={0}
                        step={1000}
                        value={saleForm.salePrice}
                        onChange={e => setSaleForm(f => ({ ...f, salePrice: e.target.value }))}
                        placeholder="99000 (để trống = xoá sale)"
                      />
                    </Field>
                    <Field label="Bắt đầu">
                      <Input
                        type="datetime-local"
                        value={saleForm.saleStartAt}
                        onChange={e => setSaleForm(f => ({ ...f, saleStartAt: e.target.value }))}
                      />
                    </Field>
                    <Field label="Kết thúc">
                      <Input
                        type="datetime-local"
                        value={saleForm.saleEndAt}
                        onChange={e => setSaleForm(f => ({ ...f, saleEndAt: e.target.value }))}
                      />
                    </Field>
                  </div>
                  <p className="text-[11px] text-slate-400 mt-2">Để giá sale trống → xoá sale hiện tại. Để ngày kết thúc trống → sale vô thời hạn.</p>
                  <div className="mt-3">
                    <BtnPrimary onClick={doSaveSale} disabled={busy}>
                      {busy ? '…' : '💾 Lưu sale'}
                    </BtnPrimary>
                  </div>
                </Section>

                {/* Current pricing info */}
                <div className="grid grid-cols-3 gap-2">
                  {[
                    ['Giá gốc',    fmtMoney(detail.price)],
                    ['Giá sale',   detail.salePrice ? fmtMoney(detail.salePrice) : '—'],
                    ['Trạng thái', detail.isFree ? 'Miễn phí' : (detail.salePrice ? '🔥 On Sale' : 'Bình thường')],
                    ['Sale từ',   fmt(detail.saleStartAt)],
                    ['Sale đến',  fmt(detail.saleEndAt)],
                    ['Đã bán',    detail.salesCount + ' lượt'],
                  ].map(([lbl, val]) => (
                    <div key={lbl} className="p-3 bg-slate-50 rounded-xl">
                      <div className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">{lbl}</div>
                      <div className="text-[12px] font-bold text-slate-900">{val}</div>
                    </div>
                  ))}
                </div>
              </>
            )}

            {/* ── ACTIONS TAB ── */}
            {tab === 'actions' && (
              <>
                {/* Status info */}
                <div className="grid grid-cols-2 gap-2 mb-2">
                  {[
                    ['Trạng thái',  <Chip label={detail.status} color={statusColor[detail.status] || 'slate'} />],
                    ['Lượt xem',    detail.viewCount],
                    ['Lượt mua',    detail.salesCount],
                    ['Đánh giá',   `${detail.averageRating?.toFixed(1) || '0'} ⭐ (${detail.reviewCount} reviews)`],
                    ['Ngày tạo',   fmtFull(detail.createdAt)],
                    ['Publish lúc', fmtFull(detail.publishedAt)],
                  ].map(([lbl, val]) => (
                    <div key={lbl} className="p-3 bg-slate-50 rounded-xl">
                      <div className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">{lbl}</div>
                      <div className="text-[12px] font-bold text-slate-900">{val}</div>
                    </div>
                  ))}
                </div>

                {/* Publish */}
                {detail.status === 'Draft' && (
                  <div className="p-4 rounded-2xl border border-green-200 bg-green-50/40">
                    <div className="text-[13px] font-bold text-slate-900 mb-1">🚀 Publish template</div>
                    <p className="text-[12px] text-slate-500 mb-3">
                      Sau khi publish, template sẽ hiển thị công khai cho người dùng.
                    </p>
                    <BtnSuccess onClick={doPublish} disabled={busy}>
                      {busy ? '…' : '✓ Publish ngay'}
                    </BtnSuccess>
                  </div>
                )}

                {/* Change status */}
                <Section title="Đổi trạng thái">
                  <div className="flex gap-2 flex-wrap">
                    {['Draft', 'Published', 'Archived'].filter(s => s !== detail.status).map(s => (
                      <BtnSecondary
                        key={s}
                        onClick={() => doChangeStatus(s)}
                        disabled={busy}
                        className="text-[12px] py-1.5 px-3"
                      >
                        → {s}
                      </BtnSecondary>
                    ))}
                  </div>
                </Section>

                {/* Danger zone */}
                <div className="p-4 rounded-2xl border border-red-200 bg-red-50/40">
                  <div className="text-[13px] font-bold text-red-600 mb-1">🗑 Xoá template</div>
                  <p className="text-[12px] text-slate-500 mb-3">
                    Xoá toàn bộ dữ liệu template bao gồm ảnh, file, versions. Không thể hoàn tác.
                  </p>
                  <BtnDanger onClick={doDelete} disabled={busy}>
                    {busy ? '…' : 'Xoá vĩnh viễn'}
                  </BtnDanger>
                </div>
              </>
            )}

          </div>
        </>
      )}
    </Modal>
  );
}