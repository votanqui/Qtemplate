// src/pages/public/CommunityPage.jsx

import { useState, useEffect, useCallback, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { communityApi } from '../../api/services';
import { toAbsoluteUrl, extractError } from '../../api/client';
import { useAuth } from '../../context/AuthContext';
import { useLang } from '../../context/Langcontext';
import { useToast } from '../../components/ui';
import {
  startCommunityHub,
  useCommunityFeed,
  useCommunityComments,
} from '../../hooks/useCommunityHub';

/* ══════════════════════════════════════════════════════════════
   HELPERS
══════════════════════════════════════════════════════════════ */
const fmtTime = (d, lang, t) => {
  if (!d) return '';
  const diff = Date.now() - new Date(d).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 1)  return t('community.time_just_now');
  if (mins < 60) return `${mins} ${t('community.time_mins')}`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24)  return `${hrs} ${t('community.time_hours')}`;
  const days = Math.floor(hrs / 24);
  if (days < 7)  return `${days} ${t('community.time_days')}`;
  return new Date(d).toLocaleDateString(lang === 'vi' ? 'vi-VN' : 'en-US', {
    day: '2-digit', month: 'short', year: 'numeric',
  });
};

/* ══════════════════════════════════════════════════════════════
   AVATAR
══════════════════════════════════════════════════════════════ */
function Avatar({ name, avatarUrl, size = 9 }) {
  const sMap = { 6:'w-6 h-6', 7:'w-7 h-7', 8:'w-8 h-8', 9:'w-9 h-9' };
  const dim  = sMap[size] || `w-${size} h-${size}`;
  if (avatarUrl) return (
    <img src={toAbsoluteUrl(avatarUrl)} alt={name}
      className={`${dim} rounded-full object-cover flex-shrink-0`} />
  );
  return (
    <div className={`${dim} rounded-full flex-shrink-0 flex items-center justify-center text-white font-bold text-sm select-none`}
      style={{ background: 'linear-gradient(135deg,#7c3aed,#0ea5e9)', fontSize: size <= 6 ? 10 : 13 }}>
      {name?.[0]?.toUpperCase() || 'U'}
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════
   POST COMPOSER
══════════════════════════════════════════════════════════════ */
function PostComposer({ user, onPosted, editPost = null, onCancelEdit }) {
  const { t }    = useLang();
  const toast    = useToast();
  const [content, setContent] = useState(editPost?.content || '');
  const [imgFile, setImgFile] = useState(null);
  const [imgPreview, setImgP] = useState(
    editPost?.imageUrl ? toAbsoluteUrl(editPost.imageUrl) : null
  );
  const [busy, setBusy] = useState(false);
  const fileRef = useRef();
  const textRef = useRef();
  const isEdit  = !!editPost;

  useEffect(() => { if (isEdit) textRef.current?.focus(); }, [isEdit]);

  const onFile = e => {
    const f = e.target.files?.[0];
    if (!f) return;
    if (f.size > 5 * 1024 * 1024) { toast.error(t('community.img_too_large')); return; }
    setImgFile(f);
    setImgP(URL.createObjectURL(f));
  };
  const removeImg = () => { setImgFile(null); setImgP(null); if (fileRef.current) fileRef.current.value = ''; };

  const submit = async () => {
    const trimmed = content.trim();
    if (!trimmed && !imgPreview) return;
    if (trimmed.length > 3000) { toast.error(t('community.content_too_long')); return; }
    setBusy(true);
    try {
      if (isEdit) {
        const keepUrl = imgPreview && !imgFile ? editPost.imageUrl : null;
        await communityApi.updatePost(editPost.id, trimmed, imgFile || null, keepUrl);
        toast.success(t('community.post_updated_ok'));
        onCancelEdit?.();
      } else {
        await communityApi.createPost(trimmed, imgFile || null);
        setContent(''); removeImg();
        toast.success(t('community.post_created_ok'));
      }
      onPosted();
    } catch (err) { toast.error(extractError(err)); }
    finally { setBusy(false); }
  };

  const remaining = 3000 - content.length;

  return (
    <div className="rounded-2xl p-4" style={{ background:'var(--bg-card)', border:'1.5px solid var(--border)' }}>
      <div className="flex gap-3">
        <Avatar name={user?.fullName} avatarUrl={user?.avatarUrl} />
        <div className="flex-1 min-w-0">
          <textarea ref={textRef} rows={isEdit ? 4 : 3} value={content}
            onChange={e => setContent(e.target.value)}
            onKeyDown={e => { if (e.key==='Enter' && (e.ctrlKey||e.metaKey)) submit(); }}
            placeholder={t('community.composer_ph')}
            className="w-full px-3.5 py-2.5 rounded-xl text-sm resize-none focus:outline-none transition-all"
            style={{ background:'var(--input-bg)', border:'1.5px solid var(--input-border)', color:'var(--input-text)' }}
            onFocus={e=>{ e.target.style.borderColor='#7c3aed'; e.target.style.boxShadow='0 0 0 3px rgba(124,58,237,.1)'; }}
            onBlur={e =>{ e.target.style.borderColor='var(--input-border)'; e.target.style.boxShadow='none'; }}
          />
          {imgPreview && (
            <div className="relative mt-2 inline-block">
              <img src={imgPreview} alt="" className="max-h-52 rounded-xl object-cover"
                style={{ border:'1px solid var(--border)' }} />
              <button onClick={removeImg}
                className="absolute top-1.5 right-1.5 w-6 h-6 rounded-full flex items-center justify-center text-white text-xs font-bold hover:opacity-80"
                style={{ background:'rgba(0,0,0,.65)' }}>✕</button>
            </div>
          )}
          <div className="flex items-center justify-between mt-2.5 pt-2.5" style={{ borderTop:'1px solid var(--border)' }}>
            <div>
              <input ref={fileRef} type="file" accept="image/*" className="hidden" onChange={onFile} />
              <button onClick={()=>fileRef.current?.click()}
                className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold hover:opacity-80 transition-opacity"
                style={{ background:'var(--bg-elevated)', color:'var(--text-secondary)', border:'1px solid var(--border)' }}>
                {t('community.btn_image')}
              </button>
            </div>
            <div className="flex items-center gap-3">
              <span className="text-[11px] tabular-nums"
                style={{ color: remaining<100?'#ef4444': remaining<300?'#f59e0b':'var(--text-muted)' }}>
                {remaining.toLocaleString()}
              </span>
              {isEdit && (
                <button onClick={onCancelEdit}
                  className="px-3 py-1.5 rounded-xl text-sm font-semibold hover:opacity-80"
                  style={{ background:'var(--bg-elevated)', color:'var(--text-muted)', border:'1px solid var(--border)' }}>
                  {t('community.btn_cancel')}
                </button>
              )}
              <button onClick={submit} disabled={busy || (!content.trim() && !imgPreview)}
                className="px-4 py-1.5 rounded-xl text-sm font-bold text-white hover:-translate-y-0.5 transition-all disabled:opacity-40 disabled:translate-y-0 disabled:cursor-not-allowed"
                style={{ background:'linear-gradient(135deg,#7c3aed,#0ea5e9)' }}>
                {busy ? '⏳' : isEdit ? t('community.btn_save') : t('community.btn_post')}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════
   GUEST PROMPT
══════════════════════════════════════════════════════════════ */
function GuestPrompt() {
  const { t }    = useLang();
  const navigate = useNavigate();
  return (
    <div className="rounded-2xl p-5 text-center" style={{ background:'var(--bg-card)', border:'1.5px solid var(--border)' }}>
      <div className="text-3xl mb-2">👥</div>
      <p className="text-sm font-medium mb-4" style={{ color:'var(--text-secondary)' }}>
        {t('community.guest_title')}
      </p>
      <div className="flex justify-center gap-3">
        <button onClick={()=>navigate('/login')}
          className="px-5 py-2 rounded-xl text-sm font-bold text-white hover:-translate-y-0.5 transition-all"
          style={{ background:'linear-gradient(135deg,#7c3aed,#0ea5e9)' }}>
          {t('community.btn_login')}
        </button>
        <button onClick={()=>navigate('/register')}
          className="px-5 py-2 rounded-xl text-sm font-semibold hover:opacity-80 transition-opacity"
          style={{ background:'var(--bg-elevated)', color:'var(--text-primary)', border:'1.5px solid var(--border)' }}>
          {t('community.btn_register')}
        </button>
      </div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════
   REPLY INPUT
══════════════════════════════════════════════════════════════ */
function ReplyInput({ targetName, currentUser, onSubmit, onCancel }) {
  const { t }    = useLang();
  const [text, setText] = useState('');
  const [busy, setBusy] = useState(false);
  const ref = useRef();
  useEffect(() => { ref.current?.focus(); }, []);

  const submit = async () => {
    const v = text.trim();
    if (!v) return;
    setBusy(true);
    try { await onSubmit(v); setText(''); onCancel(); }
    catch {}
    finally { setBusy(false); }
  };

  return (
    <div className="flex gap-2 mt-2 items-center">
      <Avatar name={currentUser?.fullName} avatarUrl={currentUser?.avatarUrl} size={6} />
      <div className="flex gap-2 flex-1">
        <input ref={ref} value={text} onChange={e=>setText(e.target.value)}
          placeholder={`${t('community.reply_ph')} ${targetName}...`}
          className="flex-1 px-3 py-1.5 rounded-xl text-sm focus:outline-none"
          style={{ background:'var(--input-bg)', border:'1.5px solid var(--input-border)', color:'var(--input-text)' }}
          onFocus={e=>{ e.target.style.borderColor='#7c3aed'; }}
          onBlur={e =>{ e.target.style.borderColor='var(--input-border)'; }}
          onKeyDown={e=>{ if(e.key==='Enter') submit(); if(e.key==='Escape') onCancel(); }}
        />
        <button onClick={submit} disabled={busy||!text.trim()}
          className="px-3.5 py-1.5 rounded-xl text-sm font-bold text-white disabled:opacity-40"
          style={{ background:'#7c3aed' }}>
          {busy ? '...' : t('community.btn_send')}
        </button>
        <button onClick={onCancel}
          className="px-2.5 py-1.5 rounded-xl text-sm hover:opacity-80"
          style={{ background:'var(--bg-elevated)', color:'var(--text-muted)', border:'1px solid var(--border)' }}>
          ✕
        </button>
      </div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════
   COMMENT ITEM
══════════════════════════════════════════════════════════════ */
const REPLY_SHOW_INIT = 3;

function CommentItem({ comment, postId, topLevelId, currentUser, lang, onRefresh }) {
  const { t }  = useLang();
  const toast  = useToast();
  const [editing,  setEditing]  = useState(false);
  const [editText, setEditText] = useState(comment.content);
  const [replying, setReplying] = useState(false);
  const [busy,     setBusy]     = useState(false);
  const [showAllReplies, setShowAllReplies] = useState(false);

  const isTopLevel = !comment.parentId;
  const canEdit    = comment.isOwner;
  const canDelete  = comment.isOwner;
  const canReply   = !!currentUser;

  const allReplies   = comment.replies || [];
  const shownReplies = showAllReplies ? allReplies : allReplies.slice(0, REPLY_SHOW_INIT);
  const hiddenCount  = allReplies.length - shownReplies.length;

  const doEdit = async () => {
    const v = editText.trim();
    if (!v) return;
    setBusy(true);
    try { await communityApi.updateComment(comment.id, v); setEditing(false); onRefresh(); }
    catch (err) { toast.error(extractError(err)); }
    finally { setBusy(false); }
  };

  const doDelete = async () => {
    if (!confirm(t('community.comment_delete_confirm'))) return;
    try { await communityApi.deleteComment(comment.id); onRefresh(); }
    catch (err) { toast.error(extractError(err)); }
  };

  const doReply = async (text) => {
    const parentId = isTopLevel ? comment.id : topLevelId;
    await communityApi.createComment(postId, text, parentId);
    onRefresh();
  };

  return (
    <div className={isTopLevel ? 'mt-3' : 'mt-2 pl-9'}>
      <div className="flex gap-2">
        <Avatar name={comment.authorName} avatarUrl={comment.authorAvatar} size={isTopLevel ? 7 : 6} />
        <div className="flex-1 min-w-0">
          <div className="inline-block max-w-full rounded-2xl px-3.5 py-2"
            style={{ background:'var(--bg-elevated)' }}>
            <div className="flex items-baseline gap-2 flex-wrap mb-0.5">
              <span className="text-[13px] font-bold" style={{ color:'var(--text-primary)' }}>
                {comment.authorName}
              </span>
              <span className="text-[10px]" style={{ color:'var(--text-muted)' }}>
                {fmtTime(comment.createdAt, lang, t)}
              </span>
            </div>
            {editing ? (
              <div>
                <textarea rows={2} value={editText} onChange={e=>setEditText(e.target.value)}
                  className="w-full px-2.5 py-1.5 rounded-xl text-sm resize-none focus:outline-none"
                  style={{ background:'var(--input-bg)', border:'1px solid #7c3aed', color:'var(--input-text)', minWidth:200 }}
                  onKeyDown={e=>{ if(e.key==='Enter'&&!e.shiftKey){ e.preventDefault(); doEdit(); } }}
                />
                <div className="flex gap-1.5 mt-1.5">
                  <button onClick={doEdit} disabled={busy}
                    className="px-3 py-1 rounded-lg text-xs font-bold text-white disabled:opacity-40"
                    style={{ background:'#7c3aed' }}>
                    {busy ? '...' : t('community.btn_save')}
                  </button>
                  <button onClick={()=>{ setEditing(false); setEditText(comment.content); }}
                    className="px-3 py-1 rounded-lg text-xs font-semibold"
                    style={{ background:'var(--bg-card)', color:'var(--text-muted)', border:'1px solid var(--border)' }}>
                    {t('community.btn_cancel')}
                  </button>
                </div>
              </div>
            ) : (
              <p className="text-sm leading-relaxed whitespace-pre-wrap break-words"
                style={{ color:'var(--text-primary)' }}>
                {comment.content}
              </p>
            )}
          </div>

          {!editing && (
            <div className="flex items-center gap-3 mt-1 ml-1">
              {canReply && (
                <button onClick={()=>setReplying(v=>!v)}
                  className="text-[11px] font-semibold hover:opacity-70 transition-opacity"
                  style={{ color: replying ? '#7c3aed' : 'var(--text-muted)' }}>
                  {replying ? t('community.btn_reply_close') : t('community.btn_reply')}
                </button>
              )}
              {canEdit && (
                <button onClick={()=>setEditing(true)}
                  className="text-[11px] font-semibold hover:opacity-70 transition-opacity"
                  style={{ color:'var(--text-muted)' }}>
                  {t('community.btn_edit')}
                </button>
              )}
              {canDelete && (
                <button onClick={doDelete}
                  className="text-[11px] font-semibold hover:opacity-70 transition-opacity text-red-400">
                  {t('community.btn_delete')}
                </button>
              )}
            </div>
          )}

          {replying && (
            <ReplyInput targetName={comment.authorName} currentUser={currentUser}
              onSubmit={doReply} onCancel={()=>setReplying(false)} />
          )}

          {isTopLevel && allReplies.length > 0 && (
            <div className="mt-1.5">
              {shownReplies.map(r => (
                <CommentItem key={r.id} comment={r} postId={postId} topLevelId={comment.id}
                  currentUser={currentUser} lang={lang} onRefresh={onRefresh} />
              ))}
              {!showAllReplies && hiddenCount > 0 && (
                <button onClick={()=>setShowAllReplies(true)}
                  className="mt-1.5 ml-9 text-[11px] font-semibold hover:opacity-70 transition-opacity"
                  style={{ color:'#7c3aed' }}>
                  ↓ {t('community.comment_show_more')} {hiddenCount} {t('community.comment_replies')}
                </button>
              )}
              {showAllReplies && allReplies.length > REPLY_SHOW_INIT && (
                <button onClick={()=>setShowAllReplies(false)}
                  className="mt-1.5 ml-9 text-[11px] font-semibold hover:opacity-70 transition-opacity"
                  style={{ color:'var(--text-muted)' }}>
                  ↑ {t('community.btn_cancel')}
                </button>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════
   COMMENT SECTION
══════════════════════════════════════════════════════════════ */
const COMMENT_SHOW_INIT = 5;

function CommentSection({ post, currentUser, lang, likeButton }) {
  const { t }  = useLang();
  const toast  = useToast();
  const [open,     setOpen]     = useState(false);
  const [comments, setComments] = useState([]);
  const [newText,  setNewText]  = useState('');
  const [loading,  setLoading]  = useState(false);
  const [busy,     setBusy]     = useState(false);
  const [count,    setCount]    = useState(post.commentCount);
  const [showAll,  setShowAll]  = useState(false);

  const loadComments = useCallback(async () => {
    setLoading(true);
    try {
      const r = await communityApi.getComments(post.id);
      const d = r.data?.data ?? r.data;
      const items = d?.items || [];
      setComments(items);
      setCount(items.reduce((s, c) => s + 1 + (c.replies?.length || 0), 0));
    } catch (err) { toast.error(extractError(err)); }
    finally { setLoading(false); }
  }, [post.id]);

  useEffect(() => { if (open) loadComments(); }, [open]);

  // ── Realtime comments ─────────────────────────────────────
  useCommunityComments(post.id, open, {
    onNewComment: (commentDto) => {
      if (commentDto.postId !== post.id) return;
      if (!open) { setCount(c => c + 1); return; }
      setComments(prev => {
        if (commentDto.parentId) {
          return prev.map(c => {
            if (c.id !== commentDto.parentId) return c;
            if (c.replies?.find(r => r.id === commentDto.id)) return c;
            return { ...c, replies: [...(c.replies || []), commentDto] };
          });
        }
        if (prev.find(c => c.id === commentDto.id)) return prev;
        return [commentDto, ...prev];
      });
      setCount(c => c + 1);
    },
    onCommentUpdated: (commentDto) => {
      if (commentDto.postId !== post.id || !open) return;
      setComments(prev => prev.map(c => {
        if (c.id === commentDto.id)
          return { ...c, content: commentDto.content, updatedAt: commentDto.updatedAt };
        return {
          ...c,
          replies: c.replies?.map(r =>
            r.id === commentDto.id ? { ...r, content: commentDto.content } : r
          ) || [],
        };
      }));
    },
    onCommentDeleted: (commentId, postId) => {
      if (postId !== post.id) return;
      setComments(prev =>
        prev
          .filter(c => c.id !== commentId)
          .map(c => ({ ...c, replies: c.replies?.filter(r => r.id !== commentId) || [] }))
      );
      setCount(c => Math.max(0, c - 1));
    },
  });

  const doComment = async () => {
    const v = newText.trim();
    if (!v) return;
    setBusy(true);
    try {
      await communityApi.createComment(post.id, v);
      setNewText('');
      setShowAll(true);
      loadComments();
    } catch (err) { toast.error(extractError(err)); }
    finally { setBusy(false); }
  };

  const shownComments = showAll ? comments : comments.slice(0, COMMENT_SHOW_INIT);
  const hiddenCount   = comments.length - shownComments.length;

  return (
    <div>
      {/* Action bar */}
      <div className="flex items-center gap-5 pt-3" style={{ borderTop:'1px solid var(--border)' }}>
        {likeButton}
        <button onClick={()=>setOpen(v=>!v)}
          className="flex items-center gap-1.5 text-sm font-semibold hover:opacity-70 transition-opacity"
          style={{ color: open ? '#7c3aed' : 'var(--text-muted)' }}>
          <span>💬</span>
          <span>{count > 0 ? `${count} ${t('community.comment_count')}` : t('community.comment_btn')}</span>
        </button>
      </div>

      {open && (
        <div className="mt-4">
          {currentUser ? (
            <div className="flex gap-2.5 items-center mb-3">
              <Avatar name={currentUser.fullName} avatarUrl={currentUser.avatarUrl} size={7} />
              <div className="flex gap-2 flex-1">
                <input value={newText} onChange={e=>setNewText(e.target.value)}
                  placeholder={t('community.comment_ph')}
                  className="flex-1 px-3.5 py-2 rounded-xl text-sm focus:outline-none"
                  style={{ background:'var(--input-bg)', border:'1.5px solid var(--input-border)', color:'var(--input-text)' }}
                  onFocus={e=>{ e.target.style.borderColor='#7c3aed'; e.target.style.boxShadow='0 0 0 3px rgba(124,58,237,.1)'; }}
                  onBlur={e =>{ e.target.style.borderColor='var(--input-border)'; e.target.style.boxShadow='none'; }}
                  onKeyDown={e=>{ if(e.key==='Enter') doComment(); }}
                />
                <button onClick={doComment} disabled={busy||!newText.trim()}
                  className="px-4 py-2 rounded-xl text-sm font-bold text-white hover:-translate-y-0.5 transition-all disabled:opacity-40 disabled:translate-y-0"
                  style={{ background:'#7c3aed' }}>
                  {busy ? '...' : t('community.btn_send')}
                </button>
              </div>
            </div>
          ) : (
            <Link to="/login"
              className="block text-center text-sm py-2.5 rounded-xl mb-3 no-underline font-semibold hover:opacity-80 transition-opacity"
              style={{ background:'var(--bg-elevated)', color:'#7c3aed', border:'1px solid var(--border)' }}>
              {t('community.comment_login')}
            </Link>
          )}

          {loading ? (
            <div className="text-center py-5 text-sm" style={{ color:'var(--text-muted)' }}>
              {t('community.comment_loading')}
            </div>
          ) : comments.length === 0 ? (
            <div className="text-center py-5 text-sm" style={{ color:'var(--text-muted)' }}>
              {t('community.comment_empty')}
            </div>
          ) : (
            <>
              {shownComments.map(c => (
                <CommentItem key={c.id} comment={c} postId={post.id} topLevelId={c.id}
                  currentUser={currentUser} lang={lang} onRefresh={loadComments} />
              ))}
              {!showAll && hiddenCount > 0 && (
                <button onClick={()=>setShowAll(true)}
                  className="mt-3 text-sm font-semibold hover:opacity-70 transition-opacity"
                  style={{ color:'#7c3aed' }}>
                  ↓ {t('community.comment_more')} {hiddenCount} {t('community.comment_more_suffix')}
                </button>
              )}
              {showAll && comments.length > COMMENT_SHOW_INIT && (
                <button onClick={()=>setShowAll(false)}
                  className="mt-3 text-sm font-semibold hover:opacity-70 transition-opacity"
                  style={{ color:'var(--text-muted)' }}>
                  {t('community.comment_collapse')}
                </button>
              )}
            </>
          )}
        </div>
      )}
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════
   POST CARD
══════════════════════════════════════════════════════════════ */
function PostCard({ post, currentUser, lang, onRefresh }) {
  const { t }  = useLang();
  const toast  = useToast();

  const [liked,     setLiked]     = useState(post.isLikedByMe);
  const [likeCount, setLikeCount] = useState(post.likeCount);
  const [editing,   setEditing]   = useState(false);

  // ── KEY FIX: sync likeCount khi parent cập nhật qua onLikeUpdated ──
  const prevLikeRef = useRef(post.likeCount);
  useEffect(() => {
    if (post.likeCount !== prevLikeRef.current) {
      prevLikeRef.current = post.likeCount;
      setLikeCount(post.likeCount);
    }
  }, [post.likeCount]);

  const canEdit   = post.isOwner;
  const canDelete = post.isOwner;

  const doLike = async () => {
    if (!currentUser) { toast.warning(t('community.like_login_warn')); return; }
    const prev = liked;
    setLiked(!prev);
    setLikeCount(c => prev ? c - 1 : c + 1);
    try { await communityApi.toggleLike(post.id); }
    catch { setLiked(prev); setLikeCount(c => prev ? c + 1 : c - 1); }
  };

  const doDelete = async () => {
    if (!confirm(t('community.post_delete_confirm'))) return;
    try { await communityApi.deletePost(post.id); toast.success(t('community.post_deleted_ok')); onRefresh(); }
    catch (err) { toast.error(extractError(err)); }
  };

  const likeBtn = (
    <button onClick={doLike}
      className="flex items-center gap-1.5 text-sm font-semibold transition-all hover:scale-105 active:scale-95"
      style={{ color: liked ? '#ef4444' : 'var(--text-muted)' }}>
      <span className={`text-base transition-all duration-150 ${liked ? 'scale-110' : ''}`}>
        {liked ? '❤️' : '🤍'}
      </span>
      {likeCount > 0 && <span>{likeCount}</span>}
    </button>
  );

  return (
    <div className="rounded-2xl p-5" style={{ background:'var(--bg-card)', border:'1.5px solid var(--border)' }}>
      <div className="flex items-start justify-between mb-3">
        <div className="flex gap-2.5 items-center min-w-0">
          <Avatar name={post.authorName} avatarUrl={post.authorAvatar} />
          <div className="min-w-0">
            <div className="text-sm font-bold truncate" style={{ color:'var(--text-primary)' }}>
              {post.authorName}
            </div>
            <div className="text-[11px]" style={{ color:'var(--text-muted)' }}>
              {fmtTime(post.createdAt, lang, t)}
              {post.updatedAt && post.updatedAt !== post.createdAt && (
                <span className="ml-1 italic">{t('community.post_edited')}</span>
              )}
            </div>
          </div>
        </div>

        {(canEdit || canDelete) && !editing && (
          <div className="flex gap-1.5 flex-shrink-0 ml-2">
            {canEdit && (
              <button onClick={()=>setEditing(true)}
                className="px-2.5 py-1 rounded-lg text-[11px] font-semibold hover:opacity-80 transition-opacity"
                style={{ background:'var(--bg-elevated)', color:'var(--text-muted)', border:'1px solid var(--border)' }}>
                {t('community.post_btn_edit')}
              </button>
            )}
            {canDelete && (
              <button onClick={doDelete}
                className="px-2.5 py-1 rounded-lg text-[11px] font-semibold hover:opacity-80 transition-opacity text-red-400"
                style={{ background:'var(--bg-elevated)', border:'1px solid var(--border)' }}>
                {t('community.post_btn_delete')}
              </button>
            )}
          </div>
        )}
      </div>

      {editing ? (
        <PostComposer user={currentUser} editPost={post}
          onPosted={()=>{ setEditing(false); onRefresh(); }}
          onCancelEdit={()=>setEditing(false)} />
      ) : (
        <>
          {post.content && (
            <p className="text-sm leading-relaxed whitespace-pre-wrap break-words mb-3"
              style={{ color:'var(--text-primary)' }}>
              {post.content}
            </p>
          )}
          {post.imageUrl && (
            <div className="rounded-2xl overflow-hidden mb-3 cursor-zoom-in"
              onClick={()=>window.open(toAbsoluteUrl(post.imageUrl),'_blank')}>
              <img src={toAbsoluteUrl(post.imageUrl)} alt=""
                className="w-full object-cover hover:scale-[1.01] transition-transform duration-300"
                style={{ maxHeight:520 }} />
            </div>
          )}
          <CommentSection post={post} currentUser={currentUser} lang={lang} likeButton={likeBtn} />
        </>
      )}
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════
   SKELETON
══════════════════════════════════════════════════════════════ */
function PostSkeleton() {
  return (
    <div className="rounded-2xl p-5 animate-pulse" style={{ background:'var(--bg-card)', border:'1.5px solid var(--border)' }}>
      <div className="flex gap-2.5 items-center mb-4">
        <div className="w-9 h-9 rounded-full" style={{ background:'var(--bg-elevated)' }} />
        <div className="space-y-1.5">
          <div className="h-3 w-28 rounded-full" style={{ background:'var(--bg-elevated)' }} />
          <div className="h-2.5 w-16 rounded-full" style={{ background:'var(--bg-elevated)' }} />
        </div>
      </div>
      <div className="space-y-2 mb-4">
        <div className="h-3.5 rounded-full" style={{ background:'var(--bg-elevated)' }} />
        <div className="h-3.5 rounded-full w-5/6" style={{ background:'var(--bg-elevated)' }} />
        <div className="h-3.5 rounded-full w-3/4" style={{ background:'var(--bg-elevated)' }} />
      </div>
      <div className="flex gap-4 pt-3" style={{ borderTop:'1px solid var(--border)' }}>
        <div className="h-3 w-12 rounded-full" style={{ background:'var(--bg-elevated)' }} />
        <div className="h-3 w-16 rounded-full" style={{ background:'var(--bg-elevated)' }} />
      </div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════
   BACK TO TOP
══════════════════════════════════════════════════════════════ */
function BackToTop() {
  const [show, setShow] = useState(false);
  useEffect(() => {
    const fn = () => setShow(window.scrollY > 600);
    window.addEventListener('scroll', fn, { passive: true });
    return () => window.removeEventListener('scroll', fn);
  }, []);
  if (!show) return null;
  return (
    <button onClick={()=>window.scrollTo({ top:0, behavior:'smooth' })}
      className="fixed bottom-6 right-6 z-40 w-10 h-10 rounded-full flex items-center justify-center text-white text-lg shadow-lg hover:scale-110 transition-transform"
      style={{ background:'linear-gradient(135deg,#7c3aed,#0ea5e9)' }}>
      ↑
    </button>
  );
}

/* ══════════════════════════════════════════════════════════════
   NEW POST BANNER
══════════════════════════════════════════════════════════════ */
function NewPostBanner({ count, onClick }) {
  const { t } = useLang();
  if (count === 0) return null;
  return (
    <button onClick={onClick}
      className="fixed top-4 left-1/2 -translate-x-1/2 z-50 flex items-center gap-2 px-5 py-2.5 rounded-2xl text-sm font-bold text-white shadow-xl hover:scale-105 transition-transform"
      style={{ background: 'linear-gradient(135deg,#7c3aed,#0ea5e9)' }}>
      ↑ {count} {t('community.feed_new_banner')}
    </button>
  );
}


/* ══════════════════════════════════════════════════════════════
   SORT BAR
══════════════════════════════════════════════════════════════ */
function SortBar({ value, onChange }) {
  const { t } = useLang();
  const TABS = [
    { id: 'hot', icon: '🔥', label: t('community.sort_hot') },
    { id: 'new', icon: '🆕', label: t('community.sort_new') },
    { id: 'top', icon: '⭐', label: t('community.sort_top') },
  ];
  return (
    <div className="flex gap-2 mb-5">
      {TABS.map(tab => (
        <button key={tab.id} onClick={() => onChange(tab.id)}
          className="flex items-center gap-1.5 px-3.5 py-1.5 rounded-xl text-sm font-semibold transition-all"
          style={{
            background: value === tab.id ? '#7c3aed' : 'var(--bg-elevated)',
            color: value === tab.id ? '#fff' : 'var(--text-muted)',
            border: '1.5px solid ' + (value === tab.id ? '#7c3aed' : 'var(--border)'),
          }}>
          <span>{tab.icon}</span>
          <span>{tab.label}</span>
        </button>
      ))}
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════
   INNER PAGE  (hooks cần nằm bên trong Provider nếu dùng Context,
   nhưng vì dùng module singleton thì không cần thiết)
══════════════════════════════════════════════════════════════ */
function CommunityPageInner() {
  const { user, isAuth } = useAuth();
  const { lang, t }      = useLang();
  const toast            = useToast();

  const [posts,      setPosts]      = useState([]);
  const [total,      setTotal]      = useState(0);
  const [loading,    setLoading]    = useState(true);
  const [page,       setPage]       = useState(1);
  const [pendingNew, setPendingNew] = useState([]);
  const [sortBy,     setSortBy]     = useState('hot');
  const pageSize = 20;

  const load = useCallback(async (p) => {
    const pg = p ?? page;
    setLoading(true);
    try {
      const r = await communityApi.getFeed(pg, pageSize, sortBy);
      const d = r.data?.data ?? r.data;
      const items = d?.items || [];
      if (pg === 1) setPosts(items);
      else setPosts(prev => {
        const ids = new Set(prev.map(x => x.id));
        return [...prev, ...items.filter(x => !ids.has(x.id))];
      });
      setTotal(d?.totalCount ?? 0);
    } catch (err) { toast.error(extractError(err)); }
    finally { setLoading(false); }
  }, [page, sortBy]);

  useEffect(() => { load(page); }, [page]);
  // Reset về trang 1 khi đổi sort
  useEffect(() => { setPage(1); setPendingNew([]); }, [sortBy]);

  // Khởi động WebSocket singleton khi page mount, cleanup khi unmount
  useEffect(() => startCommunityHub(), []);

  const reload = useCallback(() => {
    setPendingNew([]);
    if (page === 1) load(1);
    else setPage(1);
  }, [page, load]);

  const flushPending = useCallback(() => {
    if (pendingNew.length === 0) return;
    setPosts(prev => {
      const ids = new Set(prev.map(x => x.id));
      return [...pendingNew.filter(x => !ids.has(x.id)), ...prev];
    });
    setTotal(t => t + pendingNew.length);
    setPendingNew([]);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }, [pendingNew]);

  // ── Realtime feed ─────────────────────────────────────────
  useCommunityFeed({
    onNewPost: (postDto) => {
      if (postDto.isOwner) return;
      setPendingNew(prev => {
        if (prev.find(p => p.id === postDto.id)) return prev;
        return [postDto, ...prev];
      });
    },
    onPostUpdated: (postDto) => {
      setPosts(prev => prev.map(p => p.id === postDto.id ? { ...p, ...postDto } : p));
    },
    onPostDeleted: (postId) => {
      setPosts(prev => prev.filter(p => p.id !== postId));
      setTotal(v => Math.max(0, v - 1));
    },
    // ── KEY: cập nhật likeCount trên posts array → PostCard sync qua useEffect
    onLikeUpdated: (postId, likeCount) => {
      setPosts(prev => prev.map(p => p.id === postId ? { ...p, likeCount } : p));
    },
  });

  const hasMore = posts.length < total;

  return (
    <>
      <NewPostBanner count={pendingNew.length} onClick={flushPending} />

      <div className="max-w-2xl mx-auto px-4 sm:px-6 py-8 animate-fade-in">
        <div className="mb-6">
          <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold uppercase tracking-widest mb-3"
            style={{ background:'var(--bg-elevated)', border:'1px solid var(--border)', color:'var(--text-muted)' }}>
            👥 {t('community.badge')}
          </div>
          <h1 className="text-2xl sm:text-3xl font-black tracking-tight mb-1"
            style={{ color:'var(--text-primary)', fontFamily:'Syne, sans-serif' }}>
            {t('community.page_title')}
          </h1>
          <p className="text-sm" style={{ color:'var(--text-muted)' }}>
            {t('community.page_subtitle')}
          </p>
        </div>

        <SortBar value={sortBy} onChange={setSortBy} />

        <div className="mb-5">
          {isAuth ? <PostComposer user={user} onPosted={reload} /> : <GuestPrompt />}
        </div>

        {loading && posts.length === 0 ? (
          <div className="space-y-4">
            {Array.from({ length: 4 }).map((_, i) => <PostSkeleton key={i} />)}
          </div>
        ) : posts.length === 0 ? (
          <div className="text-center py-20">
            <div className="text-5xl mb-3 select-none">🌱</div>
            <p className="font-bold text-base mb-1" style={{ color:'var(--text-primary)' }}>
              {t('community.feed_empty_title')}
            </p>
            <p className="text-sm" style={{ color:'var(--text-muted)' }}>
              {t('community.feed_empty_desc')}
            </p>
          </div>
        ) : (
          <div className="space-y-4">
            {posts.map(p => (
              <PostCard key={p.id} post={p} currentUser={user} lang={lang} onRefresh={reload} />
            ))}
            {hasMore ? (
              <div className="text-center pt-2 pb-4">
                <button onClick={()=>setPage(v=>v+1)} disabled={loading}
                  className="px-7 py-2.5 rounded-xl text-sm font-semibold hover:-translate-y-0.5 transition-all disabled:opacity-40 disabled:translate-y-0"
                  style={{ background:'var(--bg-card)', border:'1.5px solid var(--border)', color:'var(--text-primary)' }}>
                  {loading ? t('community.feed_loading') : t('community.feed_load_more')}
                </button>
              </div>
            ) : (
              <div className="text-center py-6 text-sm" style={{ color:'var(--text-muted)' }}>
                — {t('community.feed_end')} {total} {t('community.feed_end_suffix')} —
              </div>
            )}
          </div>
        )}
      </div>

      <BackToTop />
    </>
  );
}

/* ══════════════════════════════════════════════════════════════
   EXPORT DEFAULT
══════════════════════════════════════════════════════════════ */
export default function CommunityPage() {
  return <CommunityPageInner />;
}