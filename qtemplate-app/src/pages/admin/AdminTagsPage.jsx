import { useState, useEffect, useCallback } from 'react';
import { adminTagApi } from '../../api/adminApi';
import {
  PageHeader, Card, Table,
  BtnPrimary, BtnSecondary, BtnDanger,
  SearchInput, ConfirmModal, Empty, Toast,
  trBase, tdBase,
} from '../../components/ui/AdminUI';
import TagFormModal from '../../modals/admin/TagFormModal';

export default function AdminTagsPage() {
  const [tags,    setTags]    = useState([]);
  const [all,     setAll]     = useState([]);    // full list để filter
  const [loading, setLoading] = useState(true);
  const [search,  setSearch]  = useState('');
  const [form,    setForm]    = useState(null);  // null | {} | tag
  const [delId,   setDelId]   = useState(null);
  const [busy,    setBusy]    = useState(false);
  const [toast,   setToast]   = useState('');

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await adminTagApi.getAll();
      const data = r.data.data || [];
      setAll(data);
    } catch { setAll([]); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  // Filter client-side
  useEffect(() => {
    if (!search.trim()) {
      setTags(all);
    } else {
      const q = search.toLowerCase();
      setTags(all.filter(t =>
        t.name.toLowerCase().includes(q) || t.slug.toLowerCase().includes(q)
      ));
    }
  }, [search, all]);

  const doDelete = async () => {
    setBusy(true);
    try {
      await adminTagApi.delete(delId);
      setDelId(null); load();
      ok('🗑 Đã xoá tag');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi xoá — tag có thể đang được dùng.'); }
    finally { setBusy(false); }
  };

  return (
    <div>
      <Toast msg={toast} />

      <PageHeader
        title="Tags"
        sub={`${all.length} tags`}
        action={
          <BtnPrimary onClick={() => setForm({})}>
            + Tạo tag
          </BtnPrimary>
        }
      />

      <div className="flex items-center gap-3 mb-4">
        <SearchInput
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Tìm tag theo tên hoặc slug…"
          className="w-72"
        />
        <span className="text-[12px] text-slate-400">{tags.length} kết quả</span>
      </div>

      {/* Tag cloud view */}
      {!loading && tags.length > 0 && (
        <Card className="mb-4">
          <div className="px-4 py-3">
            <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-3">Tag cloud</div>
            <div className="flex flex-wrap gap-2">
              {tags.map(t => (
                <span
                  key={t.id}
                  className="px-3 py-1.5 rounded-xl bg-slate-100 text-slate-700 text-[12px] font-semibold cursor-pointer hover:bg-slate-900 hover:text-white transition-colors"
                  onClick={() => setForm(t)}
                  title={`/${t.slug}`}
                >
                  {t.name}
                </span>
              ))}
            </div>
          </div>
        </Card>
      )}

      {/* Table */}
      <Card>
        <Table
          heads={['ID', 'Tên', 'Slug', '']}
          loading={loading}
          colCount={4}
        >
          {tags.length === 0 && !loading
            ? <Empty msg="Chưa có tag nào." />
            : tags.map(t => (
              <tr key={t.id} className={trBase} onClick={() => setForm(t)}>
                <td className={`${tdBase} text-slate-400 font-mono text-[12px] w-12`}>#{t.id}</td>
                <td className={tdBase}>
                  <span className="font-semibold text-slate-900">{t.name}</span>
                </td>
                <td className={tdBase}>
                  <span className="font-mono text-[12px] text-slate-400">{t.slug}</span>
                </td>
                <td className={tdBase} onClick={e => e.stopPropagation()}>
                  <div className="flex gap-1.5">
                    <BtnSecondary className="py-1 px-2.5 text-[11px]" onClick={() => setForm(t)}>
                      Sửa
                    </BtnSecondary>
                    <BtnDanger className="py-1 px-2.5 text-[11px]" onClick={() => setDelId(t.id)}>
                      Xoá
                    </BtnDanger>
                  </div>
                </td>
              </tr>
            ))
          }
        </Table>
      </Card>

      <TagFormModal
        tag={form}
        onClose={() => setForm(null)}
        onRefresh={() => { load(); ok(form?.id ? '✅ Đã cập nhật tag' : '✅ Đã tạo tag'); }}
      />

      <ConfirmModal
        open={!!delId}
        onClose={() => setDelId(null)}
        onConfirm={doDelete}
        busy={busy}
        msg="Xoá tag này? Tag đang được gắn với template sẽ bị gỡ liên kết."
      />
    </div>
  );
}