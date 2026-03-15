import { useState, useEffect, useCallback } from 'react';
import { adminMediaApi } from '../../api/adminApi';
import { toAbsoluteUrl } from '../../api/client';
import {
  fmt, Chip,
  PageHeader, Card, Table, Pager,
  BtnPrimary, BtnSecondary, BtnDanger,
  ConfirmModal, Empty, Toast,
  trBase, tdBase,
} from '../../components/ui/AdminUI';
import MediaUploadModal    from '../../modals/admin/MediaUploadModal';
import MediaSetDownloadModal from '../../modals/admin/MediaSetDownloadModal';

const storageColor = { Local: 'slate', GoogleDrive: 'blue', S3: 'orange', R2: 'purple' };

function mimeIcon(mime) {
  if (!mime) return '📄';
  if (mime.startsWith('image/')) return '🖼';
  if (mime.includes('zip'))      return '📦';
  if (mime.includes('pdf'))      return '📕';
  if (mime.includes('video'))    return '🎬';
  return '📄';
}

export default function AdminMediaPage() {
  const [files,       setFiles]       = useState([]);
  const [total,       setTotal]       = useState(0);
  const [loading,     setLoading]     = useState(true);
  const [page,        setPage]        = useState(1);
  const [showUpload,  setShowUpload]  = useState(false);
  const [showSetDl,   setShowSetDl]   = useState(false);
  const [delId,       setDelId]       = useState(null);
  const [busy,        setBusy]        = useState(false);
  const [toast,       setToast]       = useState('');
  const pageSize = 20;

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await adminMediaApi.getList(null, page, pageSize);
      const d = r.data.data;
      setFiles(d?.items || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setFiles([]); }
    finally { setLoading(false); }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  const doDelete = async () => {
    setBusy(true);
    try {
      await adminMediaApi.delete(delId);
      setDelId(null); load();
      ok('🗑 Đã xoá file');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <div>
      <Toast msg={toast} />

      <PageHeader
        title="Media"
        sub={`${total} file`}
        action={
          <div className="flex gap-2">
            <BtnSecondary onClick={() => setShowSetDl(true)}>
              ⬇ Set Download
            </BtnSecondary>
            <BtnPrimary onClick={() => setShowUpload(true)}>
              + Upload / Link
            </BtnPrimary>
          </div>
        }
      />

      <Card>
        <Table
          heads={['', 'Tên file', 'Storage', 'Template', 'Kích thước', 'MIME', 'Ngày tạo', '']}
          loading={loading}
          colCount={8}
        >
          {files.length === 0 && !loading
            ? <Empty msg="Chưa có file media nào." />
            : files.map(f => (
              <tr key={f.id} className={trBase}>
                <td className="px-3 py-3 w-10 text-xl text-center">
                  {mimeIcon(f.mimeType)}
                </td>

                <td className={tdBase}>
                  <div className="font-semibold text-slate-900 truncate max-w-[200px]">
                    {f.originalName}
                  </div>
                  <div className="text-[10px] text-slate-400 font-mono truncate max-w-[200px] mt-0.5">
                    {f.url?.startsWith('http') ? f.url : toAbsoluteUrl(f.url)}
                  </div>
                </td>

                <td className={tdBase}>
                  <Chip label={f.storageType} color={storageColor[f.storageType] || 'slate'} />
                </td>

                <td className={tdBase}>
                  {f.templateName
                    ? <Chip label={f.templateName} color="purple" />
                    : <span className="text-slate-300 text-[12px]">—</span>
                  }
                </td>

                <td className={`${tdBase} text-slate-500 font-mono text-[12px]`}>
                  {f.fileSizeText}
                </td>

                <td className={`${tdBase} text-slate-400 text-[11px] font-mono`}>
                  {f.mimeType || '—'}
                </td>

                <td className={`${tdBase} text-slate-400 text-[12px]`}>
                  {fmt(f.createdAt)}
                </td>

                <td className={tdBase} onClick={e => e.stopPropagation()}>
                  <div className="flex gap-1.5">
                    <a
                      href={f.url?.startsWith('http') ? f.url : toAbsoluteUrl(f.url)}
                      target="_blank"
                      rel="noreferrer"
                      className="inline-flex items-center px-2.5 py-1 rounded-lg bg-slate-100 text-slate-600 text-[11px] font-semibold hover:bg-slate-200 transition-colors"
                    >
                      Mở
                    </a>
                    <BtnDanger className="py-1 px-2.5 text-[11px]" onClick={() => setDelId(f.id)}>
                      Xoá
                    </BtnDanger>
                  </div>
                </td>
              </tr>
            ))
          }
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>

      <MediaUploadModal
        open={showUpload}
        onClose={() => setShowUpload(false)}
        onRefresh={() => { load(); ok('✅ Đã thêm file media'); }}
      />

      <MediaSetDownloadModal
        open={showSetDl}
        onClose={() => setShowSetDl(false)}
        onRefresh={load}
        files={files}
      />

      <ConfirmModal
        open={!!delId}
        onClose={() => setDelId(null)}
        onConfirm={doDelete}
        busy={busy}
        msg="Xoá file này? Hành động không thể hoàn tác."
      />
    </div>
  );
}