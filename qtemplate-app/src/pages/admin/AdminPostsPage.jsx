// src/pages/admin/AdminPostsPage.jsx

import { useState, useEffect, useCallback } from 'react';
import { adminPostApi } from '../../api/adminApi';
import { toAbsoluteUrl } from '../../api/client';
import {
  fmt, Chip, PageHeader, FiltersBar, Card, Table, Pager,
  BtnPrimary, BtnSecondary, BtnDanger, ConfirmModal, Empty, Toast,
  Input, Select, trBase, tdBase,
} from '../../components/ui/AdminUI';
import PostFormModal from '../../modals/admin/PostFormModal';

const STATUS_MAP = {
  Published: { label: '✅ Published', color: 'green'  },
  Draft:     { label: '📝 Draft',     color: 'slate'  },
  Archived:  { label: '📦 Archived',  color: 'yellow' },
};

export default function AdminPostsPage() {
  const [posts,   setPosts]   = useState([]);
  const [total,   setTotal]   = useState(0);
  const [loading, setLoading] = useState(true);
  const [page,    setPage]    = useState(1);
  const [search,  setSearch]  = useState('');
  const [status,  setStatus]  = useState('');
  // ✅ FIX: form = null (đóng) | {} (tạo mới) | { id:... } (sửa)
  const [form,    setForm]    = useState(null);
  const [delId,   setDelId]   = useState(null);
  const [busy,    setBusy]    = useState(false);
  const [toast,   setToast]   = useState('');
  const pageSize = 15;

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2600); };

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await adminPostApi.getList(page, pageSize, search, status);
      const d = r.data?.data ?? r.data;
      setPosts(d?.items || []);
      setTotal(d?.totalCount ?? 0);
    } catch {
      setPosts([]);
    } finally {
      setLoading(false);
    }
  }, [page, search, status]);

  useEffect(() => { load(); }, [load]);
  useEffect(() => { setPage(1); }, [search, status]);

  const doDelete = async () => {
    setBusy(true);
    try {
      await adminPostApi.delete(delId);
      setDelId(null);
      load();
      ok('🗑 Đã xóa bài viết');
    } catch (e) {
      alert(e?.response?.data?.message || 'Lỗi khi xóa.');
    } finally {
      setBusy(false);
    }
  };

  const wordCount = html =>
    Math.max(1, Math.ceil((html || '').replace(/<[^>]+>/g,'').split(/\s+/).filter(Boolean).length / 200));

  return (
    <div>
      <Toast msg={toast} />

      <PageHeader
        title="📰 Bảng tin"
        sub={`${total} bài viết`}
        action={<BtnPrimary onClick={() => setForm({})}>+ Tạo bài viết</BtnPrimary>}
      />

      <FiltersBar>
        <Input
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="🔍 Tìm tiêu đề, tác giả..."
          className="w-60"
        />
        <Select value={status} onChange={e => setStatus(e.target.value)}>
          <option value="">Tất cả trạng thái</option>
          <option value="Published">✅ Published</option>
          <option value="Draft">📝 Draft</option>
          <option value="Archived">📦 Archived</option>
        </Select>
        <BtnSecondary className="text-[12px] py-1.5 px-3" onClick={load}>↻ Làm mới</BtnSecondary>
      </FiltersBar>

      <Card>
        <Table
          heads={['Thumbnail', 'Tiêu đề', 'Tác giả', 'Trạng thái', 'Tags', 'Lượt xem', 'Ngày', '']}
          loading={loading}
          colCount={8}
        >
          {posts.length === 0 && !loading
            ? <Empty msg="Chưa có bài viết nào." />
            : posts.map(p => {
                const s = STATUS_MAP[p.status] || STATUS_MAP.Draft;
                return (
                  <tr key={p.id} className={trBase} onClick={() => setForm(p)}>
                    <td className={tdBase}>
                      <div className="w-16 h-11 rounded-lg overflow-hidden" style={{ background: 'var(--bg-elevated)', flexShrink: 0 }}>
                        {p.thumbnailUrl
                          ? <img src={toAbsoluteUrl(p.thumbnailUrl)} alt="" className="w-full h-full object-cover" />
                          : <div className="w-full h-full flex items-center justify-center text-xl">📄</div>
                        }
                      </div>
                    </td>

                    <td className={tdBase} style={{ maxWidth: 240 }}>
                      <div className="flex items-center gap-1.5">
                        {p.isFeatured && <span title="Nổi bật">⭐</span>}
                        <span className="font-semibold text-[13px] truncate" style={{ color: 'var(--text-primary)' }}>
                          {p.title}
                        </span>
                      </div>
                      <div className="font-mono text-[10px] truncate mt-0.5" style={{ color: 'var(--text-muted)' }}>
                        /{p.slug}
                      </div>
                      {p.excerpt && (
                        <div className="text-[11px] truncate mt-0.5" style={{ color: 'var(--text-muted)' }}>
                          {p.excerpt}
                        </div>
                      )}
                    </td>

                    <td className={`${tdBase} text-[12px]`} style={{ color: 'var(--text-secondary)' }}>
                      {p.authorName}
                    </td>

                    <td className={tdBase}>
                      <Chip label={s.label} color={s.color} />
                    </td>

                    <td className={`${tdBase} max-w-[120px]`}>
                      {p.tags
                        ? p.tags.split(',').slice(0,3).map(t => (
                          <span key={t}
                            className="inline-block mr-1 mb-0.5 px-1.5 py-0.5 rounded-full text-[10px] font-semibold"
                            style={{ background: 'var(--bg-elevated)', color: 'var(--text-secondary)', border: '1px solid var(--border)' }}
                          >
                            {t.trim()}
                          </span>
                        ))
                        : <span style={{ color: 'var(--text-muted)' }}>—</span>
                      }
                    </td>

                    <td className={`${tdBase} text-[12px]`} style={{ color: 'var(--text-muted)' }}>
                      <div>👁 {p.viewCount ?? 0}</div>
                      <div>~{wordCount(p.content)}ph</div>
                    </td>

                    <td className={`${tdBase} text-[12px]`} style={{ color: 'var(--text-muted)' }}>
                      {fmt(p.publishedAt || p.createdAt)}
                      {p.publishedAt && <div className="text-[10px]">Published</div>}
                    </td>

                    <td className={tdBase} onClick={e => e.stopPropagation()}>
                      <div className="flex gap-1.5">
                        <BtnSecondary className="py-1 px-2.5 text-[12px]" onClick={() => setForm(p)}>Sửa</BtnSecondary>
                        <BtnDanger    className="py-1 px-2.5 text-[12px]" onClick={() => setDelId(p.id)}>Xoá</BtnDanger>
                      </div>
                    </td>
                  </tr>
                );
              })
          }
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>

      {/* ✅ FIX: Luôn render modal, truyền post=form (null=đóng, {}=tạo mới, {...}=sửa) */}
      {/* Modal tự xử lý open={!!post} bên trong */}
      <PostFormModal
        post={form}
        onClose={() => setForm(null)}
        onRefresh={load}
      />

      {/* ✅ FIX: ConfirmModal dùng open + onClose (không phải onCancel) */}
      <ConfirmModal
        open={!!delId}
        onClose={() => setDelId(null)}
        onConfirm={doDelete}
        busy={busy}
        msg="Xóa bài viết này? Hành động không thể hoàn tác."
      />
    </div>
  );
}