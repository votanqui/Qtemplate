// src/pages/admin/AdminCommunityPage.jsx

import { useState, useEffect, useCallback } from 'react';
import { adminCommunityApi } from '../../api/adminApi';
import { toAbsoluteUrl } from '../../api/client';
import {
  fmt, Chip, PageHeader, FiltersBar, Card, Table, Pager,
  BtnSecondary, BtnDanger, ConfirmModal, Empty, Toast,
  Input, Select, trBase, tdBase,
} from '../../components/ui/AdminUI';

/* ─── Helpers ───────────────────────────────────────────────── */
const fmtDate = d =>
  d ? new Date(d).toLocaleString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : '—';

const truncate = (s, n = 80) => s && s.length > n ? s.slice(0, n) + '…' : s;

/* ─── Tab component ─────────────────────────────────────────── */
function TabBar({ tab, onChange }) {
  const TABS = [
    { id: 'posts',    icon: '📝', label: 'Bài viết'  },
    { id: 'comments', icon: '💬', label: 'Bình luận' },
  ];
  return (
    <div className="flex gap-2 mb-5">
      {TABS.map(({ id, icon, label }) => (
        <button
          key={id}
          onClick={() => onChange(id)}
          className="flex items-center gap-1.5 px-4 py-2 rounded-xl text-sm font-semibold transition-all"
          style={{
            background: tab === id ? '#7c3aed' : 'var(--bg-elevated)',
            color: tab === id ? '#fff' : 'var(--text-muted)',
            border: '1px solid var(--border)',
          }}
        >
          {icon} {label}
        </button>
      ))}
    </div>
  );
}

/* ─── Main page ─────────────────────────────────────────────── */
export default function AdminCommunityPage() {
  const [tab,      setTab]      = useState('posts');
  const [items,    setItems]    = useState([]);
  const [total,    setTotal]    = useState(0);
  const [loading,  setLoading]  = useState(true);
  const [page,     setPage]     = useState(1);
  const [search,   setSearch]   = useState('');
  const [hidden,   setHidden]   = useState('');      // '' | 'true' | 'false'
  const [delId,    setDelId]    = useState(null);
  const [busy,     setBusy]     = useState(false);
  const [toast,    setToast]    = useState('');
  const pageSize = 20;

  const ok  = msg => { setToast(msg); setTimeout(() => setToast(''), 2800); };

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const isHiddenParam = hidden === '' ? null : hidden === 'true';
      let r;
      if (tab === 'posts') {
        r = await adminCommunityApi.getPosts(page, pageSize, search, isHiddenParam);
      } else {
        r = await adminCommunityApi.getComments(page, pageSize, isHiddenParam);
      }
      const d = r.data?.data ?? r.data;
      setItems(d?.items || []);
      setTotal(d?.totalCount ?? 0);
    } catch {
      setItems([]);
    } finally {
      setLoading(false);
    }
  }, [tab, page, search, hidden]);

  useEffect(() => { setPage(1); }, [tab, search, hidden]);
  useEffect(() => { load(); }, [load]);

  /* ── Hide / unhide ─────────────────────────────────────────── */
  const doHide = async (id, isHidden) => {
    try {
      if (tab === 'posts')
        await adminCommunityApi.hidePost(id, isHidden);
      else
        await adminCommunityApi.hideComment(id, isHidden);
      ok(isHidden ? '🙈 Đã ẩn' : '👁 Đã hiện lại');
      load();
    } catch { alert('Thao tác thất bại'); }
  };

  /* ── Delete ────────────────────────────────────────────────── */
  const doDelete = async () => {
    setBusy(true);
    try {
      if (tab === 'posts')
        await adminCommunityApi.deletePost(delId);
      else
        await adminCommunityApi.deleteComment(delId);
      setDelId(null);
      ok('🗑 Đã xóa vĩnh viễn');
      load();
    } catch { alert('Xóa thất bại'); }
    finally { setBusy(false); }
  };

  return (
    <div>
      <Toast msg={toast} />

      <PageHeader
        title="👥 Cộng đồng"
        sub={`${total.toLocaleString('vi-VN')} ${tab === 'posts' ? 'bài viết' : 'bình luận'}`}
      />

      <TabBar tab={tab} onChange={t => setTab(t)} />

      {/* Filters */}
      <FiltersBar>
        {tab === 'posts' && (
          <Input
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder="🔍 Tìm nội dung, tên tác giả..."
            className="w-64"
          />
        )}
        <Select value={hidden} onChange={e => setHidden(e.target.value)}>
          <option value="">Tất cả trạng thái</option>
          <option value="false">✅ Đang hiển thị</option>
          <option value="true">🙈 Đã ẩn</option>
        </Select>
        <BtnSecondary onClick={load}>↻ Làm mới</BtnSecondary>
      </FiltersBar>

      {/* ── Posts table ─────────────────────────────────────── */}
      {tab === 'posts' && (
        <Card>
          <Table
            heads={['Tác giả', 'Nội dung', '❤️ Likes', '💬 Cmts', 'Trạng thái', 'Ngày đăng', '']}
            loading={loading}
            colCount={7}
          >
            {!loading && items.length === 0 && <Empty msg="Không có bài viết nào." />}
            {items.map(p => (
              <tr key={p.id} className={trBase}>
                {/* Tác giả */}
                <td className={tdBase}>
                  <div className="flex items-center gap-2 min-w-0">
                    {p.authorAvatar ? (
                      <img src={toAbsoluteUrl(p.authorAvatar)} alt=""
                        className="w-7 h-7 rounded-full object-cover flex-shrink-0" />
                    ) : (
                      <div className="w-7 h-7 rounded-full flex-shrink-0 flex items-center justify-center text-white text-[11px] font-bold"
                        style={{ background: 'linear-gradient(135deg,#7c3aed,#0ea5e9)' }}>
                        {p.authorName?.[0]?.toUpperCase()}
                      </div>
                    )}
                    <div className="min-w-0">
                      <div className="text-[13px] font-semibold truncate max-w-[120px]"
                        style={{ color: 'var(--text-primary)' }}>
                        {p.authorName}
                      </div>
                      <div className="text-[10px] truncate max-w-[120px]"
                        style={{ color: 'var(--text-muted)' }}>
                        {p.authorEmail}
                      </div>
                    </div>
                  </div>
                </td>

                {/* Nội dung */}
                <td className={tdBase} style={{ maxWidth: 300 }}>
                  {p.content && (
                    <p className="text-[13px] leading-snug"
                      style={{ color: 'var(--text-primary)' }}>
                      {truncate(p.content)}
                    </p>
                  )}
                  {p.imageUrl && (
                    <img
                      src={toAbsoluteUrl(p.imageUrl)} alt=""
                      className="mt-1.5 w-14 h-10 rounded-lg object-cover border cursor-pointer hover:opacity-80 transition-opacity"
                      style={{ borderColor: 'var(--border)' }}
                      onClick={() => window.open(toAbsoluteUrl(p.imageUrl), '_blank')}
                    />
                  )}
                </td>

                {/* Likes */}
                <td className={`${tdBase} text-center`}>
                  <span className="text-sm font-semibold" style={{ color: 'var(--text-secondary)' }}>
                    {p.likeCount}
                  </span>
                </td>

                {/* Comments */}
                <td className={`${tdBase} text-center`}>
                  <span className="text-sm font-semibold" style={{ color: 'var(--text-secondary)' }}>
                    {p.commentCount}
                  </span>
                </td>

                {/* Trạng thái */}
                <td className={tdBase}>
                  <Chip
                    label={p.isHidden ? '🙈 Ẩn' : '✅ Hiển thị'}
                    color={p.isHidden ? 'yellow' : 'green'}
                  />
                  {p.isHidden && p.hideReason && (
                    <div className="text-[10px] mt-0.5 max-w-[120px] truncate"
                      style={{ color: 'var(--text-muted)' }}>
                      {p.hideReason}
                    </div>
                  )}
                </td>

                {/* Ngày */}
                <td className={`${tdBase} text-[12px]`} style={{ color: 'var(--text-muted)', whiteSpace: 'nowrap' }}>
                  {fmtDate(p.createdAt)}
                </td>

                {/* Actions */}
                <td className={tdBase} onClick={e => e.stopPropagation()}>
                  <div className="flex gap-1.5">
                    <BtnSecondary
                      className="py-1 px-2.5 text-[12px]"
                      onClick={() => doHide(p.id, !p.isHidden)}
                    >
                      {p.isHidden ? '👁 Hiện' : '🙈 Ẩn'}
                    </BtnSecondary>
                    <BtnDanger
                      className="py-1 px-2.5 text-[12px]"
                      onClick={() => setDelId(p.id)}
                    >
                      Xóa
                    </BtnDanger>
                  </div>
                </td>
              </tr>
            ))}
          </Table>
          <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
        </Card>
      )}

      {/* ── Comments table ───────────────────────────────────── */}
      {tab === 'comments' && (
        <Card>
          <Table
            heads={['Tác giả', 'Nội dung', 'Bài viết #', 'Loại', 'Trạng thái', 'Ngày', '']}
            loading={loading}
            colCount={7}
          >
            {!loading && items.length === 0 && <Empty msg="Không có bình luận nào." />}
            {items.map(c => (
              <tr key={c.id} className={trBase}>
                {/* Tác giả */}
                <td className={tdBase}>
                  <div className="flex items-center gap-2">
                    {c.authorAvatar ? (
                      <img src={toAbsoluteUrl(c.authorAvatar)} alt=""
                        className="w-7 h-7 rounded-full object-cover flex-shrink-0" />
                    ) : (
                      <div className="w-7 h-7 rounded-full flex-shrink-0 flex items-center justify-center text-white text-[11px] font-bold"
                        style={{ background: 'linear-gradient(135deg,#7c3aed,#0ea5e9)' }}>
                        {c.authorName?.[0]?.toUpperCase()}
                      </div>
                    )}
                    <span className="text-[13px] font-semibold truncate max-w-[110px]"
                      style={{ color: 'var(--text-primary)' }}>
                      {c.authorName}
                    </span>
                  </div>
                </td>

                {/* Nội dung */}
                <td className={tdBase} style={{ maxWidth: 320 }}>
                  <p className="text-[13px] leading-snug" style={{ color: 'var(--text-primary)' }}>
                    {truncate(c.content, 100)}
                  </p>
                </td>

                {/* Post ID */}
                <td className={`${tdBase} text-center`}>
                  <span className="text-sm font-mono font-semibold"
                    style={{ color: 'var(--text-muted)' }}>
                    #{c.postId}
                  </span>
                </td>

                {/* Loại: reply hay top-level */}
                <td className={tdBase}>
                  {c.parentId ? (
                    <Chip label="↳ Reply" color="blue" />
                  ) : (
                    <Chip label="Comment" color="slate" />
                  )}
                </td>

                {/* Trạng thái */}
                <td className={tdBase}>
                  <Chip
                    label={c.isHidden ? '🙈 Ẩn' : '✅ Hiển thị'}
                    color={c.isHidden ? 'yellow' : 'green'}
                  />
                </td>

                {/* Ngày */}
                <td className={`${tdBase} text-[12px]`} style={{ color: 'var(--text-muted)', whiteSpace: 'nowrap' }}>
                  {fmtDate(c.createdAt)}
                </td>

                {/* Actions */}
                <td className={tdBase} onClick={e => e.stopPropagation()}>
                  <div className="flex gap-1.5">
                    <BtnSecondary
                      className="py-1 px-2.5 text-[12px]"
                      onClick={() => doHide(c.id, !c.isHidden)}
                    >
                      {c.isHidden ? '👁 Hiện' : '🙈 Ẩn'}
                    </BtnSecondary>
                    <BtnDanger
                      className="py-1 px-2.5 text-[12px]"
                      onClick={() => setDelId(c.id)}
                    >
                      Xóa
                    </BtnDanger>
                  </div>
                </td>
              </tr>
            ))}
          </Table>
          <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
        </Card>
      )}

      {/* Confirm delete */}
      <ConfirmModal
        open={!!delId}
        onClose={() => setDelId(null)}
        onConfirm={doDelete}
        busy={busy}
        msg={
          tab === 'posts'
            ? 'Xóa vĩnh viễn bài viết này? Toàn bộ bình luận, lượt thích và ảnh đính kèm sẽ bị xóa theo.'
            : 'Xóa vĩnh viễn bình luận này?'
        }
      />
    </div>
  );
}