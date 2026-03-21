// src/pages/public/NewsDetailPage.jsx

import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { postApi } from '../../api/services';
import { toAbsoluteUrl } from '../../api/client';
import { useLang } from '../../context/Langcontext';

/* ─── Reading progress ──────────────────────────────────────── */
function ProgressBar() {
  const [pct, setPct] = useState(0);
  useEffect(() => {
    const onScroll = () => {
      const doc = document.documentElement;
      const total = doc.scrollHeight - doc.clientHeight;
      setPct(total > 0 ? Math.min(100, (doc.scrollTop / total) * 100) : 0);
    };
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, []);
  return (
    <div className="fixed top-0 left-0 right-0 z-50 h-0.5"
      style={{ background: 'var(--bg-elevated)' }}>
      <div className="h-full transition-[width] duration-100"
        style={{ width: `${pct}%`, background: 'linear-gradient(90deg,#7c3aed,#0ea5e9)' }} />
    </div>
  );
}

/* ─── Skeleton ──────────────────────────────────────────────── */
function Skeleton() {
  return (
    <div className="max-w-3xl mx-auto px-4 sm:px-6 py-10 animate-pulse space-y-5">
      <div className="h-3 rounded-full w-1/4" style={{ background: 'var(--bg-elevated)' }} />
      <div className="h-8 rounded-2xl w-5/6"  style={{ background: 'var(--bg-elevated)' }} />
      <div className="h-3 rounded-full w-1/3" style={{ background: 'var(--bg-elevated)' }} />
      <div className="h-56 rounded-2xl"       style={{ background: 'var(--bg-elevated)' }} />
      {Array.from({ length: 8 }).map((_, i) => (
        <div key={i} className={`h-3 rounded-full ${i % 3 === 2 ? 'w-2/3' : 'w-full'}`}
          style={{ background: 'var(--bg-elevated)' }} />
      ))}
    </div>
  );
}

/* ─── Main page ─────────────────────────────────────────────── */
export default function NewsDetailPage() {
  const { slug }         = useParams();
  const { t, lang }      = useLang();
  const [post,    setPost]     = useState(null);
  const [loading, setLoading]  = useState(true);
  const [notFound,setNotFound] = useState(false);

  const fmtDate = d =>
    d ? new Date(d).toLocaleDateString(lang === 'vi' ? 'vi-VN' : 'en-US', {
      weekday: 'long', day: 'numeric', month: 'long', year: 'numeric',
    }) : '';

  const readTime = (content = '') =>
    Math.max(1, Math.ceil(content.replace(/<[^>]+>/g, '').split(/\s+/).filter(Boolean).length / 200));

  useEffect(() => {
    setLoading(true);
    setNotFound(false);
    postApi.getBySlug(slug)
      .then(r => {
        const d = r.data?.data ?? r.data;
        if (!d) { setNotFound(true); return; }
        setPost(d);
      })
      .catch(() => setNotFound(true))
      .finally(() => setLoading(false));
  }, [slug]);

  if (loading) return <><ProgressBar /><Skeleton /></>;

  if (notFound) return (
    <div className="text-center py-24 px-4">
      <div className="text-6xl mb-4 select-none">😕</div>
      <h2 className="text-xl font-bold mb-3" style={{ color: 'var(--text-primary)' }}>
        {t('news.not_found_title')}
      </h2>
      <p className="text-sm mb-6" style={{ color: 'var(--text-muted)' }}>
        {t('news.not_found_desc')}
      </p>
      <Link to="/tin-tuc"
        className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-bold text-white no-underline transition-all hover:-translate-y-0.5"
        style={{ background: 'linear-gradient(135deg,#7c3aed,#0ea5e9)' }}>
        {t('news.back_to_list')}
      </Link>
    </div>
  );

  const tags = (post.tags || '').split(',').map(tag => tag.trim()).filter(Boolean);

  return (
    <>
      <ProgressBar />

      <article className="max-w-3xl mx-auto px-4 sm:px-6 py-10">

        {/* Back link */}
        <Link to="/tin-tuc"
          className="inline-flex items-center gap-1.5 text-sm font-semibold mb-7 no-underline transition-colors hover:text-violet-500"
          style={{ color: 'var(--text-muted)' }}>
          {t('news.back')}
        </Link>

        {/* Tags */}
        {tags.length > 0 && (
          <div className="flex flex-wrap gap-2 mb-4">
            {tags.map(tag => (
              <span key={tag}
                className="px-2.5 py-1 rounded-full text-[11px] font-semibold"
                style={{ background: 'rgba(124,58,237,.1)', color: '#7c3aed' }}>
                {tag}
              </span>
            ))}
          </div>
        )}

        {/* Title */}
        <h1 className="text-2xl sm:text-3xl font-black leading-tight tracking-tight mb-4"
          style={{ color: 'var(--text-primary)', fontFamily: 'Syne, sans-serif' }}>
          {post.isFeatured && <span className="mr-2">⭐</span>}
          {post.title}
        </h1>

        {/* Excerpt */}
        {post.excerpt && (
          <p className="text-base leading-relaxed font-medium mb-5"
            style={{ color: 'var(--text-secondary)' }}>
            {post.excerpt}
          </p>
        )}

        {/* Meta row */}
        <div className="flex flex-wrap items-center gap-3 text-xs mb-7 pb-6"
          style={{ color: 'var(--text-muted)', borderBottom: '1.5px solid var(--border)' }}>
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 rounded-full flex items-center justify-center text-white text-[11px] font-bold shrink-0"
              style={{ background: 'linear-gradient(135deg,#7c3aed,#0ea5e9)' }}>
              {post.authorName?.[0]?.toUpperCase() || 'A'}
            </div>
            <span className="font-semibold" style={{ color: 'var(--text-secondary)' }}>
              {post.authorName}
            </span>
          </div>
          <span>·</span>
          <span>{fmtDate(post.publishedAt || post.createdAt)}</span>
          <span>·</span>
          <span>⏱ ~{readTime(post.content)} {t('news.read_time')}</span>
          <span>·</span>
          <span>👁 {post.viewCount ?? 0} {t('news.views')}</span>
        </div>

        {/* Thumbnail */}
        {post.thumbnailUrl && (
          <div className="rounded-2xl overflow-hidden mb-8 shadow-lg">
            <img
              src={toAbsoluteUrl(post.thumbnailUrl)}
              alt={post.title}
              className="w-full object-cover"
              style={{ maxHeight: 480 }}
            />
          </div>
        )}

        {/* Content */}
        <div
          className="post-prose"
          style={{ fontSize: 15, lineHeight: 1.85, color: 'var(--text-primary)' }}
          dangerouslySetInnerHTML={{ __html: post.content }}
        />

        {/* Prose styles — thuần inline, không cần @tailwind/typography */}
        <style>{`
          .post-prose h1 {
            font-size:1.75rem; font-weight:800; line-height:1.2;
            margin:1.5em 0 .5em; color:var(--text-primary);
            font-family:Syne,sans-serif;
          }
          .post-prose h2 {
            font-size:1.35rem; font-weight:700; line-height:1.3;
            margin:1.3em 0 .4em; color:var(--text-primary);
            font-family:Syne,sans-serif;
          }
          .post-prose h3 {
            font-size:1.1rem; font-weight:600; line-height:1.4;
            margin:1.1em 0 .3em; color:var(--text-primary);
          }
          .post-prose p  { margin:.75em 0; }
          .post-prose ul { list-style:disc;    padding-left:1.5em; margin:.6em 0; }
          .post-prose ol { list-style:decimal; padding-left:1.5em; margin:.6em 0; }
          .post-prose li { margin:.3em 0; }
          .post-prose a  { color:#7c3aed; text-decoration:underline; text-underline-offset:3px; }
          .post-prose a:hover { opacity:.75; }
          .post-prose strong { font-weight:700; }
          .post-prose em     { font-style:italic; }
          .post-prose s      { text-decoration:line-through; opacity:.6; }
          .post-prose u      { text-decoration:underline; text-underline-offset:3px; }
          .post-prose blockquote {
            border-left:4px solid #7c3aed;
            padding:.6em 1.2em; margin:1em 0;
            border-radius:0 12px 12px 0;
            background:rgba(124,58,237,.06);
            color:var(--text-secondary); font-style:italic;
          }
          .post-prose pre {
            background:#0f172a; color:#e2e8f0;
            padding:1em 1.25em; border-radius:12px;
            font-family:'Fira Code',monospace; font-size:.85em;
            overflow-x:auto; margin:1em 0; white-space:pre-wrap;
          }
          .post-prose code:not(pre code) {
            background:rgba(124,58,237,.1); color:#7c3aed;
            padding:.1em .4em; border-radius:5px;
            font-size:.88em; font-family:monospace;
          }
          .post-prose img {
            max-width:100%; height:auto; border-radius:12px;
            margin:1em auto; display:block;
            box-shadow:0 4px 24px rgba(0,0,0,.1);
          }
          .post-prose hr {
            border:none; border-top:2px solid var(--border); margin:1.5em 0;
          }
          .post-prose table {
            width:100%; border-collapse:collapse; margin:1em 0; font-size:.9em;
          }
          .post-prose th, .post-prose td {
            border:1px solid var(--border); padding:.5em .85em; text-align:left;
          }
          .post-prose th { background:var(--bg-elevated); font-weight:700; }
          .post-prose tr:hover td { background:var(--bg-elevated); }
        `}</style>

        {/* Footer */}
        <div className="mt-10 pt-6" style={{ borderTop: '1.5px solid var(--border)' }}>
          <div className="flex flex-wrap items-center justify-between gap-4">
            {tags.length > 0 && (
              <div className="flex flex-wrap items-center gap-2">
                <span className="text-xs font-semibold" style={{ color: 'var(--text-muted)' }}>
                  {t('news.tags_label')}
                </span>
                {tags.map(tag => (
                  <span key={tag}
                    className="px-2.5 py-1 rounded-full text-[11px] font-semibold"
                    style={{ background: 'var(--bg-elevated)', color: 'var(--text-secondary)', border: '1px solid var(--border)' }}>
                    {tag}
                  </span>
                ))}
              </div>
            )}
            <Link to="/tin-tuc"
              className="inline-flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-semibold no-underline transition-all hover:-translate-y-0.5"
              style={{ background: 'var(--bg-elevated)', border: '1.5px solid var(--border)', color: 'var(--text-primary)' }}>
              {t('news.other_articles')}
            </Link>
          </div>
        </div>
      </article>
    </>
  );
}