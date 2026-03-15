import { useState, useEffect, useCallback } from 'react';
import { adminTemplateApi } from '../../api/adminApi';
import { toAbsoluteUrl } from '../../api/client';
import {
  fmt, fmtMoney, Chip,
  PageHeader, FiltersBar, Card, Table, Pager,
  BtnPrimary, BtnSecondary, SearchInput, Select, Empty, Toast,
  trBase, tdBase,
} from '../../components/ui/AdminUI';
import TemplateFormModal   from '../../modals/admin/TemplateFormModal';
import TemplateDetailModal from '../../modals/admin/TemplateDetailModal';
import BulkSaleModal       from '../../modals/admin/BulkSaleModal';

const statusColor = { Draft: 'slate', Published: 'green', Archived: 'orange' };

export default function AdminTemplatesPage() {
  const [templates,    setTemplates]    = useState([]);
  const [total,        setTotal]        = useState(0);
  const [loading,      setLoading]      = useState(true);
  const [page,         setPage]         = useState(1);
  const [search,       setSearch]       = useState('');
  const [status,       setStatus]       = useState('');
  const [toast,        setToast]        = useState('');
  const [formTarget,   setFormTarget]   = useState(null);
  const [detailTarget, setDetailTarget] = useState(null);
  const [bulkSaleOpen, setBulkSaleOpen] = useState(false);

  const pageSize = 20;
  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const p = { page, pageSize };
      if (search.trim()) p.search = search.trim();
      if (status)        p.status = status;
      const r = await adminTemplateApi.getList(p);
      const d = r.data.data;
      setTemplates(d?.items || d || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setTemplates([]); }
    finally { setLoading(false); }
  }, [page, search, status]);

  useEffect(() => { load(); }, [load]);

  const doQuickPublish = async (e, t) => {
    e.stopPropagation();
    if (!window.confirm(`Publish "${t.name}"?`)) return;
    try {
      await adminTemplateApi.publish(t.id);
      ok(`🚀 Đã publish "${t.name}"`);
      load();
    } catch (ex) { alert(ex?.response?.data?.message || 'Lỗi.'); }
  };

  return (
    <div>
      <Toast msg={toast} />

      <PageHeader
        title="Templates"
        sub={`${total} template`}
        action={
          <div className="flex items-center gap-2">
            <BtnSecondary onClick={() => setBulkSaleOpen(true)}>
              🔥 Sale hàng loạt
            </BtnSecondary>
            <BtnPrimary onClick={() => setFormTarget({})}>
              + Tạo template
            </BtnPrimary>
          </div>
        }
      />

      <FiltersBar>
        <SearchInput
          value={search}
          onChange={e => { setSearch(e.target.value); setPage(1); }}
          placeholder="Tìm tên, slug…"
          className="w-56"
        />
        <Select value={status} onChange={e => { setStatus(e.target.value); setPage(1); }}>
          <option value="">Tất cả status</option>
          <option value="Draft">Draft</option>
          <option value="Published">Published</option>
          <option value="Archived">Archived</option>
        </Select>
        <span className="ml-auto text-[12px] text-slate-400">
          {loading ? 'Đang tải…' : `${total} templates`}
        </span>
      </FiltersBar>

      <Card>
        <Table
          heads={['', 'Tên template', 'Status', 'Giá', 'Danh mục', 'Bán', '⭐', 'Ngày tạo', '']}
          loading={loading}
          colCount={9}
        >
          {templates.length === 0 && !loading
            ? <Empty msg="Chưa có template nào." />
            : templates.map(t => (
              <tr key={t.id} className={trBase} onClick={() => setDetailTarget(t)}>
                <td className="px-3 py-2 w-14">
                  <div className="w-12 h-9 rounded-lg overflow-hidden bg-slate-100 flex-shrink-0">
                    {t.thumbnailUrl
                      ? <img src={toAbsoluteUrl(t.thumbnailUrl)} alt="" className="w-full h-full object-cover" />
                      : <div className="w-full h-full flex items-center justify-center text-slate-300 text-base">🖼</div>
                    }
                  </div>
                </td>
                <td className={tdBase}>
                  <div className="flex items-center gap-2 flex-wrap">
                    <span className="font-bold text-slate-900">{t.name}</span>
                    {t.isFeatured && <Chip label="⭐" color="yellow" />}
                    {t.salePrice  && <Chip label="🔥 Sale" color="red" />}
                  </div>
                  <div className="text-[11px] text-slate-400 font-mono mt-0.5 truncate max-w-[200px]">{t.slug}</div>
                </td>
                <td className={tdBase}>
                  <Chip label={t.status} color={statusColor[t.status] || 'slate'} />
                </td>
                <td className={tdBase}>
                  {t.isFree
                    ? <span className="text-green-600 font-bold text-[12px]">Miễn phí</span>
                    : <div>
                        <div className="font-bold text-slate-900">{fmtMoney(t.salePrice ?? t.price)}</div>
                        {t.salePrice && <div className="text-[10px] text-slate-400 line-through">{fmtMoney(t.price)}</div>}
                      </div>
                  }
                </td>
                <td className={`${tdBase} text-slate-500`}>{t.categoryName || '—'}</td>
                <td className={`${tdBase} text-center font-semibold text-slate-700`}>{t.salesCount}</td>
                <td className={`${tdBase} text-slate-500`}>{t.averageRating > 0 ? t.averageRating.toFixed(1) : '—'}</td>
                <td className={`${tdBase} text-slate-400 text-[12px]`}>{fmt(t.publishedAt || t.createdAt)}</td>
                <td className={tdBase} onClick={e => e.stopPropagation()}>
                  <div className="flex gap-1.5">
                    {t.status === 'Draft' && (
                      <BtnPrimary className="py-1 px-2.5 text-[11px]" onClick={e => doQuickPublish(e, t)}>
                        Publish
                      </BtnPrimary>
                    )}
                    <BtnSecondary className="py-1 px-2.5 text-[11px]" onClick={e => { e.stopPropagation(); setFormTarget(t); }}>
                      Sửa
                    </BtnSecondary>
                    <BtnSecondary className="py-1 px-2.5 text-[11px]" onClick={e => { e.stopPropagation(); setDetailTarget(t); }}>
                      Media
                    </BtnSecondary>
                  </div>
                </td>
              </tr>
            ))
          }
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>

      <TemplateFormModal   template={formTarget}   onClose={() => setFormTarget(null)}   onRefresh={load} />
      <TemplateDetailModal template={detailTarget} onClose={() => setDetailTarget(null)} onRefresh={load} />
      <BulkSaleModal       open={bulkSaleOpen}     onClose={() => setBulkSaleOpen(false)} onRefresh={load} />
    </div>
  );
}