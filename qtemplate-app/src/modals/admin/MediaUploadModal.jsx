import { useState, useEffect, useRef } from 'react';
import { adminMediaApi, adminTemplateApi } from '../../api/adminApi';
import {
  Modal, Tabs, Field, Input, Select,
  BtnPrimary, BtnSecondary, Toast,
} from '../../components/ui/AdminUI';

export default function MediaUploadModal({ open, onClose, onRefresh }) {
  const [tab,        setTab]        = useState('upload');
  const [busy,       setBusy]       = useState(false);
  const [toast,      setToast]      = useState('');
  const [templates,  setTemplates]  = useState([]);
  const [templateId, setTemplateId] = useState('');
  const [linkForm,   setLinkForm]   = useState({
    url: '', originalName: '', storageType: 'GoogleDrive', externalId: '',
  });
  const fileRef = useRef();

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  useEffect(() => {
    if (!open) return;
    adminTemplateApi.getList({ pageSize: 200 })
      .then(r => setTemplates(r.data.data?.items || []))
      .catch(() => {});
  }, [open]);

  const doUpload = async () => {
    const file = fileRef.current?.files?.[0];
    if (!file) return alert('Chọn file để upload.');
    setBusy(true);
    try {
      await adminMediaApi.upload(file, templateId || null);
      ok('✅ Upload thành công');
      if (fileRef.current) fileRef.current.value = '';
      setTemplateId('');
      onRefresh();
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi upload.'); }
    finally { setBusy(false); }
  };

  const setLink = (k, v) => setLinkForm(f => ({ ...f, [k]: v }));

  const doLink = async () => {
    if (!linkForm.url.trim())          return alert('Nhập URL file.');
    if (!linkForm.originalName.trim()) return alert('Nhập tên file.');
    setBusy(true);
    try {
      await adminMediaApi.link({
        url:          linkForm.url.trim(),
        originalName: linkForm.originalName.trim(),
        storageType:  linkForm.storageType,
        externalId:   linkForm.externalId.trim() || undefined,
        templateId:   templateId || undefined,
      });
      ok('✅ Đã liên kết file');
      setLinkForm({ url: '', originalName: '', storageType: 'GoogleDrive', externalId: '' });
      setTemplateId('');
      onRefresh();
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <Modal open={open} onClose={onClose} title="Thêm Media" width={520}>
      <Toast msg={toast} />

      <Tabs
        tabs={[['upload', '📁 Upload file'], ['link', '🔗 External link']]}
        active={tab}
        onChange={setTab}
      />

      <Field label="Gắn với template (tuỳ chọn)" className="mb-4">
        <Select value={templateId} onChange={e => setTemplateId(e.target.value)} className="w-full">
          <option value="">-- Không gắn template --</option>
          {templates.map(t => (
            <option key={t.id} value={t.id}>{t.name}</option>
          ))}
        </Select>
      </Field>

      {tab === 'upload' && (
        <div className="flex flex-col gap-3">
          <Field label="Chọn file (tối đa 500MB)">
            <input
              ref={fileRef}
              type="file"
              className="block w-full text-[12px] text-slate-600 file:mr-3 file:py-2 file:px-4 file:rounded-xl file:border-0 file:font-semibold file:bg-slate-900 file:text-white hover:file:bg-slate-700 cursor-pointer"
            />
          </Field>
          <div className="flex justify-end gap-2 pt-2">
            <BtnSecondary onClick={onClose}>Huỷ</BtnSecondary>
            <BtnPrimary onClick={doUpload} disabled={busy}>
              {busy ? 'Đang upload…' : '📤 Upload'}
            </BtnPrimary>
          </div>
        </div>
      )}

      {tab === 'link' && (
        <div className="flex flex-col gap-3">
          <Field label="URL file" required>
            <Input
              value={linkForm.url}
              onChange={e => setLink('url', e.target.value)}
              placeholder="https://drive.google.com/file/…"
            />
          </Field>
          <Field label="Tên file" required>
            <Input
              value={linkForm.originalName}
              onChange={e => setLink('originalName', e.target.value)}
              placeholder="template-v1.0.0.zip"
            />
          </Field>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Storage type">
              <Select value={linkForm.storageType} onChange={e => setLink('storageType', e.target.value)} className="w-full">
                <option value="GoogleDrive">Google Drive</option>
                <option value="S3">Amazon S3</option>
                <option value="R2">Cloudflare R2</option>
                <option value="Local">Local</option>
              </Select>
            </Field>
            <Field label="External ID (tuỳ chọn)">
              <Input
                value={linkForm.externalId}
                onChange={e => setLink('externalId', e.target.value)}
                placeholder="file_id, key…"
              />
            </Field>
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <BtnSecondary onClick={onClose}>Huỷ</BtnSecondary>
            <BtnPrimary onClick={doLink} disabled={busy}>
              {busy ? '…' : '🔗 Liên kết'}
            </BtnPrimary>
          </div>
        </div>
      )}
    </Modal>
  );
}