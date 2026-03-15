import { useState, useEffect, useCallback } from 'react';
import { adminCategoryApi } from '../../api/adminApi';
import {
  PageHeader, Card,
  BtnPrimary, BtnSecondary, BtnDanger,
  ActiveDot, ConfirmModal, Empty, Toast,
} from '../../components/ui/AdminUI';
import CategoryFormModal from '../../modals/admin/CategoryFormModal';

// ── Category row (recursive) ──────────────────────────────────
function CategoryRow({ cat, depth, onEdit, onDelete, allCats }) {
  const [expanded, setExpanded] = useState(true);
  const hasChildren = cat.children?.length > 0;

  return (
    <>
      <tr className="border-b border-slate-50 hover:bg-slate-50/70 transition-colors">
        {/* Tên + indent */}
        <td className="px-4 py-3">
          <div className="flex items-center gap-2" style={{ paddingLeft: depth * 24 }}>
            {hasChildren ? (
              <button
                onClick={() => setExpanded(v => !v)}
                className="w-5 h-5 rounded-md bg-slate-100 hover:bg-slate-200 flex items-center justify-center text-slate-500 text-[10px] flex-shrink-0 transition-colors"
              >
                {expanded ? '▾' : '▸'}
              </button>
            ) : (
              <span className="w-5 h-5 flex items-center justify-center text-slate-200 text-[10px] flex-shrink-0">—</span>
            )}
            {cat.iconUrl && (
              <img src={cat.iconUrl} alt="" className="w-5 h-5 object-contain flex-shrink-0" />
            )}
            <div>
              <div className="text-[13px] font-semibold text-slate-900">{cat.name}</div>
              <div className="text-[11px] font-mono text-slate-400">{cat.slug}</div>
            </div>
          </div>
        </td>

        {/* Mô tả */}
        <td className="px-4 py-3 max-w-[200px]">
          <div className="text-[12px] text-slate-500 truncate">{cat.description || '—'}</div>
        </td>

        {/* sortOrder */}
        <td className="px-4 py-3 text-center">
          <span className="text-[12px] font-semibold text-slate-600 font-mono">{cat.sortOrder}</span>
        </td>

        {/* Trạng thái */}
        <td className="px-4 py-3">
          <ActiveDot active={cat.isActive} onLabel="Hiển thị" offLabel="Ẩn" />
        </td>

        {/* Actions */}
        <td className="px-4 py-3" onClick={e => e.stopPropagation()}>
          <div className="flex gap-1.5">
            <BtnSecondary className="py-1 px-2.5 text-[11px]" onClick={() => onEdit(cat)}>
              Sửa
            </BtnSecondary>
            <BtnDanger className="py-1 px-2.5 text-[11px]" onClick={() => onDelete(cat.id)}>
              Xoá
            </BtnDanger>
          </div>
        </td>
      </tr>

      {/* Children */}
      {hasChildren && expanded && cat.children.map(child => (
        <CategoryRow
          key={child.id}
          cat={child}
          depth={depth + 1}
          onEdit={onEdit}
          onDelete={onDelete}
          allCats={allCats}
        />
      ))}
    </>
  );
}

// ── Main page ─────────────────────────────────────────────────
export default function AdminCategoriesPage() {
  const [categories, setCategories] = useState([]);
  const [loading,    setLoading]    = useState(true);
  const [form,       setForm]       = useState(null);   // null | {} | category
  const [delId,      setDelId]      = useState(null);
  const [busy,       setBusy]       = useState(false);
  const [toast,      setToast]      = useState('');

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await adminCategoryApi.getAll();
      setCategories(r.data.data || []);
    } catch { setCategories([]); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  // Đếm tổng (bao gồm children)
  const countAll = (list) => list.reduce((s, c) => s + 1 + countAll(c.children || []), 0);
  const total = countAll(categories);

  const doDelete = async () => {
    setBusy(true);
    try {
      await adminCategoryApi.delete(delId);
      setDelId(null); load();
      ok('🗑 Đã xoá danh mục');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi xoá — có thể danh mục đang có template hoặc con.'); }
    finally { setBusy(false); }
  };

  return (
    <div>
      <Toast msg={toast} />

      <PageHeader
        title="Danh mục"
        sub={`${total} danh mục`}
        action={
          <BtnPrimary onClick={() => setForm({})}>
            + Tạo danh mục
          </BtnPrimary>
        }
      />

      <Card>
        {loading ? (
          <div className="text-center text-slate-400 py-16">Đang tải…</div>
        ) : categories.length === 0 ? (
          <Empty msg="Chưa có danh mục nào." />
        ) : (
          <table className="w-full">
            <thead>
              <tr className="border-b border-slate-100">
                {['Tên / Slug', 'Mô tả', 'Thứ tự', 'Trạng thái', ''].map(h => (
                  <th key={h} className="px-4 py-2.5 text-left text-[11px] font-bold text-slate-400 uppercase tracking-wider">
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {categories.map(cat => (
                <CategoryRow
                  key={cat.id}
                  cat={cat}
                  depth={0}
                  onEdit={setForm}
                  onDelete={setDelId}
                  allCats={categories}
                />
              ))}
            </tbody>
          </table>
        )}
      </Card>

      <CategoryFormModal
        category={form}
        onClose={() => setForm(null)}
        onRefresh={() => { load(); ok(form?.id ? '✅ Đã cập nhật danh mục' : '✅ Đã tạo danh mục'); }}
        categories={categories}
      />

      <ConfirmModal
        open={!!delId}
        onClose={() => setDelId(null)}
        onConfirm={doDelete}
        busy={busy}
        msg="Xoá danh mục này? Nếu có danh mục con hoặc template thuộc danh mục này, thao tác sẽ thất bại."
      />
    </div>
  );
}