// src/pages/public/NewsPage.jsx

import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { postApi } from '../../api/services';
import { toAbsoluteUrl } from '../../api/client';
import { useLang } from '../../context/Langcontext';

/* ─── Helpers ───────────────────────────────────────────────── */
const fmtDate = (d, lang) =>
  d ? new Date(d).toLocaleDateString(lang === 'vi' ? 'vi-VN' : 'en-US', {
    day: '2-digit', month: 'short', year: 'numeric',
  }) : '';

const readTime = (content = '') =>
  Math.max(1, Math.ceil(content.replace(/<[^>]+>/g, '').split(/\s+/).filter(Boolean).length / 200));

/* ─── Skeleton card ─────────────────────────────────────────── */
function SkeletonCard() {
  return (
    <div className="rounded-2xl overflow-hidden animate-pulse"
      style={{ background: 'var(--bg-card)', border: '1.5px solid var(--border)' }}>
      <div className="h-44" style={{ background: 'var(--bg-elevated)' }} />
      <div className="p-5 space-y-3">
        <div className="h-2.5 rounded-full w-1/3" style={{ background: 'var(--bg-elevated)' }} />
        <div className="h-4 rounded-full w-4/5"   style={{ background: 'var(--bg-elevated)' }} />
        <div className="h-3 rounded-full"          style={{ background: 'var(--bg-elevated)' }} />
        <div className="h-3 rounded-full w-2/3"   style={{ background: 'var(--bg-elevated)' }} />
      </div>
    </div>
  );
}

/* ─── Post card ─────────────────────────────────────────────── */
function PostCard({ post, large = false, lang, t }) {
  const tags = (post.tags || '').split(',').map(tag => tag.trim()).filter(Boolean);

  return (
    <Link
      to={`/tin-tuc/${post.slug}`}
      className="group no-underline block"
      style={{ textDecoration: 'none' }}
    >
      <article
        className={`rounded-2xl overflow-hidden transition-all duration-300 hover:-translate-y-1 hover:shadow-xl ${large ? 'md:flex' : 'flex flex-col'}`}
        style={{ background: 'var(--bg-card)', border: '1.5px solid var(--border)', height: '100%' }}
        onMouseEnter={e => { e.currentTarget.style.borderColor = 'rgba(124,58,237,0.35)'; }}
        onMouseLeave={e => { e.currentTarget.style.borderColor = 'var(--border)'; }}
      >
        {/* Thumbnail */}
        <div
          className={`relative overflow-hidden flex-shrink-0 ${large ? 'md:w-5/12 h-52 md:h-auto' : 'h-44 w-full'}`}
          style={{ background: 'linear-gradient(135deg,rgba(124,58,237,.06),rgba(14,165,233,.06))' }}
        >
          {post.thumbnailUrl ? (
            <img
              src={toAbsoluteUrl(post.thumbnailUrl)}
              alt={post.title}
              className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105"
              loading="lazy"
            />
          ) : (
            <div className="w-full h-full flex items-center justify-center text-5xl select-none">📰</div>
          )}
          {post.isFeatured && (
            <span
              className="absolute top-3 left-3 px-2.5 py-1 rounded-full text-[10px] font-bold text-white"
              style={{ background: 'linear-gradient(90deg,#f59e0b,#ef4444)' }}
            >
              {t('news.featured_badge')}
            </span>
          )}
        </div>

        {/* Body */}
        <div className={`flex flex-col gap-2 p-5 flex-1 min-w-0 ${large ? '' : ''}`}>
          {/* Tags */}
          {tags.length > 0 && (
            <div className="flex flex-wrap gap-1.5">
              {tags.slice(0, 3).map(tag => (
                <span key={tag}
                  className="px-2 py-0.5 rounded-full text-[10px] font-semibold"
                  style={{ background: 'rgba(124,58,237,.1)', color: '#7c3aed' }}>
                  {tag}
                </span>
              ))}
            </div>
          )}

          {/* Title */}
          <h2
            className={`font-bold leading-snug group-hover:text-violet-500 transition-colors line-clamp-2 ${large ? 'text-lg sm:text-xl' : 'text-[15px]'}`}
            style={{ color: 'var(--text-primary)', margin: 0 }}
          >
            {post.title}
          </h2>

          {/* Excerpt */}
          {post.excerpt && (
            <p className="text-[13px] leading-relaxed line-clamp-2 flex-shrink-0"
              style={{ color: 'var(--text-secondary)', margin: 0 }}>
              {post.excerpt}
            </p>
          )}

          {/* Meta */}
          <div
            className="flex flex-wrap items-center gap-x-3 gap-y-1 text-[11px] mt-auto pt-3"
            style={{ color: 'var(--text-muted)', borderTop: '1px solid var(--border)' }}
          >
            <span className="flex items-center gap-1">
              <span>✍️</span>
              <span className="truncate max-w-[120px]">{post.authorName}</span>
            </span>
            <span>·</span>
            <span>{fmtDate(post.publishedAt || post.createdAt, lang)}</span>
            <span>·</span>
            <span>👁 {post.viewCount ?? 0}</span>
            <span>·</span>
            <span>⏱ ~{readTime(post.content)} {t('news.read_time')}</span>
          </div>
        </div>
      </article>
    </Link>
  );
}

/* ─── Pager button ──────────────────────────────────────────── */
function PBtn({ children, active, disabled, onClick }) {
  return (
    <button
      onClick={onClick}
      disabled={disabled}
      className="min-w-[36px] h-9 px-2 rounded-xl text-[13px] font-semibold transition-all disabled:opacity-30 disabled:cursor-not-allowed"
      style={{
        background: active ? '#7c3aed' : 'var(--bg-card)',
        color: active ? '#fff' : 'var(--text-primary)',
        border: '1.5px solid var(--border)',
      }}
    >
      {children}
    </button>
  );
}

/* ─── Main component ────────────────────────────────────────── */
export default function NewsPage() {
  const { t, lang } = useLang();
  const [posts,   setPosts]   = useState([]);
  const [total,   setTotal]   = useState(0);
  const [loading, setLoading] = useState(true);
  const [page,    setPage]    = useState(1);
  const [search,  setSearch]  = useState('');
  const pageSize = 9;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await postApi.getList({ page, pageSize, ...(search && { search }) });
      const d = r.data?.data ?? r.data;
      setPosts(d?.items || []);
      setTotal(d?.totalCount ?? 0);
    } catch {
      setPosts([]);
    } finally {
      setLoading(false);
    }
  }, [page, search]);

  useEffect(() => { load(); }, [load]);
  useEffect(() => { setPage(1); }, [search]);

  const totalPages = Math.ceil(total / pageSize);

  // Tính pager range
  const pagerPages = () => {
    const all = Array.from({ length: totalPages }, (_, i) => i + 1);
    return all.filter(p => p === 1 || p === totalPages || Math.abs(p - page) <= 1)
      .reduce((acc, p, idx, arr) => {
        if (idx > 0 && arr[idx - 1] !== p - 1) acc.push('…');
        acc.push(p);
        return acc;
      }, []);
  };

  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 py-8 sm:py-12">

      {/* ── Page header ──────────────────────────────────────── */}
      <div className="mb-8">
        <p className="text-[10px] font-black uppercase tracking-widest mb-2"
          style={{ color: '#7c3aed' }}>
          {t('news.page_eyebrow')}
        </p>
        <h1 className="text-2xl sm:text-3xl font-black tracking-tight mb-2"
          style={{ color: 'var(--text-primary)', fontFamily: 'Syne, sans-serif' }}>
          {t('news.page_title')}
        </h1>
        <p className="text-sm" style={{ color: 'var(--text-muted)' }}>
          {t('news.page_subtitle')}
        </p>
      </div>

      {/* ── Search ───────────────────────────────────────────── */}
      <div className="mb-7">
        <div className="relative max-w-sm">
          <span className="absolute left-3.5 top-1/2 -translate-y-1/2 pointer-events-none text-base">🔍</span>
          <input
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder={t('news.search_ph')}
            className="w-full pl-10 pr-4 py-2.5 rounded-xl text-sm focus:outline-none transition-all"
            style={{
              background: 'var(--input-bg)',
              border: '1.5px solid var(--input-border)',
              color: 'var(--input-text)',
            }}
            onFocus={e => { e.target.style.borderColor='#7c3aed'; e.target.style.boxShadow='0 0 0 3px rgba(124,58,237,.12)'; }}
            onBlur={e  => { e.target.style.borderColor='var(--input-border)'; e.target.style.boxShadow='none'; }}
          />
        </div>
      </div>

      {/* ── Grid ─────────────────────────────────────────────── */}
      {loading ? (
        /* Skeleton */
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
          {Array.from({ length: 6 }).map((_, i) => <SkeletonCard key={i} />)}
        </div>
      ) : posts.length === 0 ? (
        /* Empty */
        <div className="text-center py-20">
          <div className="text-5xl mb-3 select-none">📭</div>
          <p className="font-semibold text-sm mb-3" style={{ color: 'var(--text-primary)' }}>
            {t('news.empty_title')}
          </p>
          {search && (
            <button
              className="text-sm font-semibold transition-colors hover:opacity-75"
              style={{ color: '#7c3aed' }}
              onClick={() => setSearch('')}
            >
              {t('news.clear_search')}
            </button>
          )}
        </div>
      ) : (
        /*
         * Layout grid:
         * - Mobile: 1 col, mọi card đều xếp chồng
         * - Tablet (sm): 2 col
         * - Desktop (lg): 3 col
         * - Bài đầu tiên trang 1 (large=true) chiếm cả hàng đầu trên lg
         */
        <>
          {page === 1 && !search && posts.length > 0 && (
            <div className="mb-5">
              <PostCard post={posts[0]} large t={t} lang={lang} />
            </div>
          )}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
            {(page === 1 && !search ? posts.slice(1) : posts).map(p => (
              <PostCard key={p.id} post={p} t={t} lang={lang} />
            ))}
          </div>
        </>
      )}

      {/* ── Pager ────────────────────────────────────────────── */}
      {totalPages > 1 && (
        <div className="flex justify-center items-center gap-2 mt-10 flex-wrap">
          <PBtn disabled={page <= 1} onClick={() => setPage(p => p - 1)}>
            {t('news.prev')}
          </PBtn>

          {pagerPages().map((p, i) =>
            p === '…' ? (
              <span key={`e${i}`} className="w-9 text-center text-sm select-none"
                style={{ color: 'var(--text-muted)' }}>…</span>
            ) : (
              <PBtn key={p} active={p === page} onClick={() => setPage(p)}>{p}</PBtn>
            )
          )}

          <PBtn disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>
            {t('news.next')}
          </PBtn>
        </div>
      )}
    </div>
  );
}