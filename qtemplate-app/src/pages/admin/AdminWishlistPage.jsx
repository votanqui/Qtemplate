import { useState, useEffect, useCallback } from 'react';
import { adminWishlistApi } from '../../api/adminApi';
import {
  fmt, fmtMoney,
  PageHeader, FiltersBar, Card, Table, Pager,
  Empty, Toast, Tabs, StatCard,
  trBase, tdBase,
} from '../../components/ui/AdminUI';

// ── All Wishlists tab ─────────────────────────────────────────
function AllWishlistsTab() {
  const [items,   setItems]   = useState([]);
  const [total,   setTotal]   = useState(0);
  const [loading, setLoading] = useState(true);
  const [page,    setPage]    = useState(1);
  const pageSize = 20;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await adminWishlistApi.getAll({ page, pageSize });
      const d = r.data.data;
      setItems(d?.items || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setItems([]); }
    finally { setLoading(false); }
  }, [page]);

  useEffect(() => { load(); }, [load]);

  return (
    <>
      <FiltersBar>
        <span className="text-[12px] text-slate-400">{total} lượt yêu thích</span>
      </FiltersBar>

      <Card>
        <Table
          heads={['User', 'Template', 'Ngày thêm']}
          loading={loading}
          colCount={3}
        >
          {items.length === 0 && !loading
            ? <Empty msg="Chưa có wishlist nào." />
            : items.map(w => (
              <tr key={w.id} className={trBase}>
                <td className={tdBase}>
                  <div className="text-[13px] font-semibold text-slate-800">{w.userEmail}</div>
                  <div className="text-[10px] font-mono text-slate-400">{String(w.userId).slice(0, 16)}…</div>
                </td>
                <td className={tdBase}>
                  <div className="text-[13px] font-semibold text-slate-800">{w.templateName}</div>
                  <div className="text-[10px] font-mono text-slate-400">{String(w.templateId).slice(0, 16)}…</div>
                </td>
                <td className={`${tdBase} text-[12px] text-slate-400`}>
                  {fmt(w.createdAt)}
                </td>
              </tr>
            ))
          }
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>
    </>
  );
}

// ── Top Wishlisted tab ────────────────────────────────────────
function TopWishlistedTab() {
  const [items,   setItems]   = useState([]);
  const [loading, setLoading] = useState(true);
  const [top,     setTop]     = useState(10);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await adminWishlistApi.getTop(top);
      setItems(r.data.data || []);
    } catch { setItems([]); }
    finally { setLoading(false); }
  }, [top]);

  useEffect(() => { load(); }, [load]);

  const maxCount = items[0]?.count ?? 1;

  return (
    <>
      <FiltersBar>
        <div className="flex items-center gap-2">
          <span className="text-[12px] text-slate-500">Hiển thị top</span>
          {[10, 20, 50].map(n => (
            <button
              key={n}
              onClick={() => setTop(n)}
              className={`px-3 py-1 rounded-lg text-[12px] font-bold transition-colors ${
                top === n ? 'bg-slate-900 text-white' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
              }`}
            >
              {n}
            </button>
          ))}
        </div>
      </FiltersBar>

      {loading ? (
        <div className="text-center text-slate-400 py-16">Đang tải…</div>
      ) : items.length === 0 ? (
        <div className="text-center text-slate-400 py-16">
          <div className="text-4xl mb-3">💔</div>
          <div className="font-semibold">Chưa có dữ liệu wishlist.</div>
        </div>
      ) : (
        <Card>
          <div className="px-4 py-2">
            {items.map((item, idx) => (
              <div key={String(item.templateId)} className="py-3 border-b border-slate-50 last:border-0">
                <div className="flex items-center gap-3">
                  {/* Rank */}
                  <div className={`flex-shrink-0 w-8 h-8 rounded-xl flex items-center justify-center font-extrabold text-[13px] ${
                    idx === 0 ? 'bg-amber-400 text-white' :
                    idx === 1 ? 'bg-slate-300 text-white' :
                    idx === 2 ? 'bg-orange-400 text-white' :
                    'bg-slate-100 text-slate-500'
                  }`}>
                    {idx + 1}
                  </div>

                  {/* Name */}
                  <div className="flex-1 min-w-0">
                    <div className="text-[13px] font-semibold text-slate-900 truncate">
                      {item.templateName}
                    </div>
                    <div className="text-[10px] font-mono text-slate-400 truncate">
                      {String(item.templateId)}
                    </div>
                  </div>

                  {/* Count */}
                  <div className="flex-shrink-0 text-right">
                    <div className="text-[16px] font-extrabold text-slate-900">{item.count}</div>
                    <div className="text-[10px] text-slate-400">lượt ❤️</div>
                  </div>
                </div>

                {/* Bar */}
                <div className="mt-2 h-1.5 bg-slate-100 rounded-full overflow-hidden">
                  <div
                    className="h-full rounded-full bg-gradient-to-r from-slate-700 to-slate-500 transition-all duration-500"
                    style={{ width: `${(item.count / maxCount) * 100}%` }}
                  />
                </div>
              </div>
            ))}
          </div>
        </Card>
      )}
    </>
  );
}

// ── Main page ─────────────────────────────────────────────────
export default function AdminWishlistPage() {
  const [tab, setTab] = useState('all');

  return (
    <div>
      <PageHeader title="Danh sách yêu thích" sub="Wishlist của tất cả user" />

      <Tabs
        tabs={[
          ['all', '❤️ Tất cả Wishlist'],
          ['top', '🏆 Top Template'],
        ]}
        active={tab}
        onChange={setTab}
      />

      {tab === 'all' && <AllWishlistsTab />}
      {tab === 'top' && <TopWishlistedTab />}
    </div>
  );
}