import { useState, useEffect } from 'react';
import { adminMediaApi, adminTemplateApi } from '../../api/adminApi';
import {
  Modal, Field, Select, BtnPrimary, BtnSecondary,
} from '../../components/ui/AdminUI';

function mimeIcon(mime) {
  if (!mime) return '📄';
  if (mime.startsWith('image/')) return '🖼';
  if (mime.includes('zip'))      return '📦';
  if (mime.includes('pdf'))      return '📕';
  if (mime.includes('video'))    return '🎬';
  return '📄';
}

export default function MediaSetDownloadModal({ open, onClose, onRefresh, files }) {
  const [templateId, setTemplateId] = useState('');
  const [mediaId,    setMediaId]    = useState('');
  const [busy,       setBusy]       = useState(false);
  const [templates,  setTemplates]  = useState([]);

  useEffect(() => {
    if (!open) return;
    adminTemplateApi.getList({ pageSize: 200 })
      .then(r => setTemplates(r.data.data?.items || []))
      .catch(() => {});
  }, [open]);

  const doSet = async () => {
    if (!templateId) return alert('Chọn template.');
    if (!mediaId)    return alert('Chọn file media.');
    setBusy(true);
    try {
      await adminMediaApi.setDownload(templateId, Number(mediaId));
      alert('✅ Đã set file download cho template.');
      setTemplateId(''); setMediaId('');
      onClose(); onRefresh();
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <Modal open={open} onClose={onClose} title="Set file download cho Template" width={480}>
      <div className="flex flex-col gap-3">
        <Field label="Template" required>
          <Select value={templateId} onChange={e => setTemplateId(e.target.value)} className="w-full">
            <option value="">-- Chọn template --</option>
            {templates.map(t => (
              <option key={t.id} value={t.id}>{t.name}</option>
            ))}
          </Select>
        </Field>
        <Field label="File media" required>
          <Select value={mediaId} onChange={e => setMediaId(e.target.value)} className="w-full">
            <option value="">-- Chọn file --</option>
            {files.map(f => (
              <option key={f.id} value={f.id}>
                {mimeIcon(f.mimeType)} {f.originalName} ({f.fileSizeText})
              </option>
            ))}
          </Select>
        </Field>
        <div className="flex justify-end gap-2 pt-2">
          <BtnSecondary onClick={onClose}>Huỷ</BtnSecondary>
          <BtnPrimary onClick={doSet} disabled={busy}>
            {busy ? '…' : '✓ Set download'}
          </BtnPrimary>
        </div>
      </div>
    </Modal>
  );
}