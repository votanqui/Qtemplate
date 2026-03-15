import { useState, useEffect, useCallback } from 'react';
import { adminBannerApi } from '../../api/adminApi';
import { toAbsoluteUrl } from '../../api/client';
import {
  fmt, ActiveDot, Chip,
  PageHeader, FiltersBar, Card, Table, Pager,
  BtnPrimary, BtnSecondary, BtnDanger, ConfirmModal, Empty, Toast,
  trBase, tdBase,
} from '../../components/ui/AdminUI';
import BannerFormModal from '../../modals/admin/BannerFormModal';

export default function AdminBannersPage() {
  const [banners, setBanners] = useState([]);
  const [total,   setTotal]   = useState(0);
  const [loading, setLoading] = useState(true);
  const [page,    setPage]    = useState(1);
  const [form,    setForm]    = useState(null);   // null | {} | banner
  const [delId,   setDelId]   = useState(null);
  const [busy,    setBusy]    = useState(false);
  const [toast,   setToast]   = useState('');
  const pageSize = 20;

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await adminBannerApi.getList(page, pageSize);
      const d = r.data.data;
      setBanners(d?.items || d || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setBanners([]); }
    finally { setLoading(false); }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  const doDelete = async () => {
    setBusy(true);
    try {
      await adminBannerApi.delete(delId);
      setDelId(null); load();
      ok('🗑 Đã xoá banner');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <div>
      <Toast msg={toast} />

      <PageHeader
        title="Banners"
        sub={`${total} banner`}
        action={
          <BtnPrimary onClick={() => setForm({})}>
            + Tạo banner
          </BtnPrimary>
        }
      />

      <Card>
        <Table
          heads={['Ảnh', 'Tiêu đề', 'Vị trí', 'Thứ tự', 'Trạng thái', 'Thời gian', '']}
          loading={loading}
          colCount={7}
        >
          {banners.length === 0 && !loading
            ? <Empty msg="Chưa có banner nào." />
            : banners.map(b => (
              <tr key={b.id} className={trBase}>
                <td className={tdBase}>
                  <div className="w-20 h-12 rounded-xl overflow-hidden bg-slate-100 flex-shrink-0">
                    {b.imageUrl
                      ? <img src={toAbsoluteUrl(b.imageUrl)} alt="" className="w-full h-full object-cover" />
                      : <div className="w-full h-full flex items-center justify-center text-slate-300 text-lg">🖼</div>
                    }
                  </div>
                </td>
                <td className={tdBase}>
                  <div className="font-semibold text-slate-900">{b.title}</div>
                  {b.subTitle && <div className="text-[11px] text-slate-400 truncate max-w-[160px]">{b.subTitle}</div>}
                  {b.linkUrl && (
                    <a href={b.linkUrl} target="_blank" rel="noreferrer"
                      className="text-[11px] text-blue-500 hover:underline truncate block max-w-[160px]">
                      {b.linkUrl}
                    </a>
                  )}
                </td>
                <td className={tdBase}>
                  <Chip label={b.position} color="purple" />
                </td>
                <td className={`${tdBase} text-center font-semibold text-slate-700`}>
                  {b.sortOrder}
                </td>
                <td className={tdBase}>
                  <ActiveDot active={b.isActive} onLabel="Hiển thị" offLabel="Ẩn" />
                </td>
                <td className={`${tdBase} text-slate-400 text-[12px]`}>
                  {b.startAt || b.endAt
                    ? <>{fmt(b.startAt)} → {fmt(b.endAt)}</>
                    : '—'
                  }
                </td>
                <td className={tdBase} onClick={e => e.stopPropagation()}>
                  <div className="flex gap-1.5">
                    <BtnSecondary className="py-1 px-3 text-[12px]" onClick={() => setForm(b)}>
                      Sửa
                    </BtnSecondary>
                    <BtnDanger className="py-1 px-3 text-[12px]" onClick={() => setDelId(b.id)}>
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

      <BannerFormModal
        banner={form}
        onClose={() => setForm(null)}
        onRefresh={load}
      />

      <ConfirmModal
        open={!!delId}
        onClose={() => setDelId(null)}
        onConfirm={doDelete}
        busy={busy}
        msg="Xoá banner này? Hành động không thể hoàn tác."
      />
    </div>
  );
}