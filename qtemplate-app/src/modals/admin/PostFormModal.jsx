// src/modals/admin/PostFormModal.jsx

import { useState, useEffect, useRef, useCallback } from 'react';
import { adminPostApi } from '../../api/adminApi';
import { toAbsoluteUrl } from '../../api/client';
import {
  Modal, Field, Input, Select, BtnPrimary, BtnSecondary, Toast,
} from '../../components/ui/AdminUI';

/* ─── Slug generator ────────────────────────────────────────── */
function toSlug(str) {
  const map = { đ:'d',Đ:'d',á:'a',à:'a',ả:'a',ã:'a',ạ:'a',ă:'a',ắ:'a',ặ:'a',ằ:'a',ẵ:'a',ẳ:'a',â:'a',ấ:'a',ậ:'a',ầ:'a',ẫ:'a',ẩ:'a',é:'e',è:'e',ẻ:'e',ẽ:'e',ẹ:'e',ê:'e',ế:'e',ệ:'e',ề:'e',ễ:'e',ể:'e',í:'i',ì:'i',ỉ:'i',ĩ:'i',ị:'i',ó:'o',ò:'o',ỏ:'o',õ:'o',ọ:'o',ô:'o',ố:'o',ộ:'o',ồ:'o',ổ:'o',ỗ:'o',ơ:'o',ớ:'o',ợ:'o',ờ:'o',ỡ:'o',ở:'o',ú:'u',ù:'u',ủ:'u',ũ:'u',ụ:'u',ư:'u',ứ:'u',ự:'u',ừ:'u',ữ:'u',ử:'u',ý:'y',ỳ:'y',ỷ:'y',ỹ:'y',ỵ:'y' };
  return str.toLowerCase().split('').map(c => map[c] ?? c).replace(/[^a-z0-9]+/g,'-').replace(/^-+|-+$/g,'');
}

/* ─── Rich Text Editor ──────────────────────────────────────── */
function RichEditor({ value, onChange, onImageInsert }) {
  const editorRef   = useRef(null);
  const isComposing = useRef(false);
  const onChangeRef = useRef(onChange);

  // Luôn giữ ref mới nhất để tránh stale closure
  useEffect(() => { onChangeRef.current = onChange; }, [onChange]);

  // Sync value → DOM (chỉ khi editor không đang focus)
  useEffect(() => {
    const el = editorRef.current;
    if (!el || document.activeElement === el) return;
    if (el.innerHTML !== (value || '')) el.innerHTML = value || '';
  }, [value]);

  const syncOut = useCallback(() => {
    if (editorRef.current) onChangeRef.current(editorRef.current.innerHTML);
  }, []);

  const exec = useCallback((cmd, val = null) => {
    editorRef.current?.focus();
    document.execCommand(cmd, false, val);
    setTimeout(() => {
      if (editorRef.current) onChangeRef.current(editorRef.current.innerHTML);
    }, 0);
  }, []);

  const insertColor = useCallback((color) => {
    editorRef.current?.focus();
    document.execCommand('foreColor', false, color);
    setTimeout(() => {
      if (editorRef.current) onChangeRef.current(editorRef.current.innerHTML);
    }, 0);
  }, []);

  const COLORS = ['#0f172a','#dc2626','#16a34a','#2563eb','#9333ea','#ea580c','#0891b2','#be185d','#ca8a04','#ffffff'];

  const ToolBtn = ({ title, onClick, children }) => (
    <button
      type="button"
      title={title}
      onClick={onClick}
      className="flex items-center justify-center w-7 h-7 rounded-lg text-[13px] font-semibold transition-all select-none hover:opacity-80"
      style={{ background: 'var(--bg-elevated)', color: 'var(--text-secondary)', border: '1px solid var(--border)' }}
      onMouseDown={e => e.preventDefault()}
    >
      {children}
    </button>
  );

  return (
    <div className="rounded-xl overflow-hidden" style={{ border: '1px solid var(--input-border)' }}>
      {/* Toolbar */}
      <div className="flex flex-wrap gap-1 p-2" style={{ background: 'var(--bg-elevated)', borderBottom: '1px solid var(--border)' }}>
        <select
          onChange={e => { exec('formatBlock', e.target.value); e.target.value = 'p'; }}
          className="h-7 px-2 rounded-lg text-[12px] cursor-pointer"
          style={{ background: 'var(--input-bg)', border: '1px solid var(--border)', color: 'var(--text-primary)' }}
          onMouseDown={e => e.preventDefault()}
        >
          <option value="p">Đoạn văn</option>
          <option value="h1">Tiêu đề 1</option>
          <option value="h2">Tiêu đề 2</option>
          <option value="h3">Tiêu đề 3</option>
        </select>

        <span className="w-px self-stretch mx-0.5" style={{ background: 'var(--border)' }} />
        <ToolBtn title="In đậm (Ctrl+B)"    onClick={() => exec('bold')}><b>B</b></ToolBtn>
        <ToolBtn title="In nghiêng (Ctrl+I)" onClick={() => exec('italic')}><i>I</i></ToolBtn>
        <ToolBtn title="Gạch dưới (Ctrl+U)" onClick={() => exec('underline')}><u>U</u></ToolBtn>
        <ToolBtn title="Gạch ngang"         onClick={() => exec('strikeThrough')}><s>S</s></ToolBtn>

        <span className="w-px self-stretch mx-0.5" style={{ background: 'var(--border)' }} />
        <ToolBtn title="Căn trái"  onClick={() => exec('justifyLeft')}>◀</ToolBtn>
        <ToolBtn title="Căn giữa"  onClick={() => exec('justifyCenter')}>▬</ToolBtn>
        <ToolBtn title="Căn phải"  onClick={() => exec('justifyRight')}>▶</ToolBtn>

        <span className="w-px self-stretch mx-0.5" style={{ background: 'var(--border)' }} />
        <ToolBtn title="Bullet list"  onClick={() => exec('insertUnorderedList')}>≡•</ToolBtn>
        <ToolBtn title="Number list"  onClick={() => exec('insertOrderedList')}>1≡</ToolBtn>

        <span className="w-px self-stretch mx-0.5" style={{ background: 'var(--border)' }} />
        <ToolBtn title="Trích dẫn"       onClick={() => exec('formatBlock', 'blockquote')}>❝</ToolBtn>
        <ToolBtn title="Code"            onClick={() => exec('formatBlock', 'pre')}>&lt;/&gt;</ToolBtn>
        <ToolBtn title="Đường kẻ ngang"  onClick={() => exec('insertHorizontalRule')}>—</ToolBtn>

        <span className="w-px self-stretch mx-0.5" style={{ background: 'var(--border)' }} />
        <ToolBtn title="Chèn link" onClick={() => { const u = prompt('Nhập URL:'); if (u) exec('createLink', u); }}>🔗</ToolBtn>
        <ToolBtn title="Chèn ảnh" onClick={onImageInsert}>🖼</ToolBtn>

        <span className="w-px self-stretch mx-0.5" style={{ background: 'var(--border)' }} />
        {COLORS.map(c => (
          <button
            key={c}
            type="button"
            title={`Màu ${c}`}
            onMouseDown={e => e.preventDefault()}
            onClick={() => insertColor(c)}
            className="w-5 h-5 rounded-full flex-shrink-0 transition-transform hover:scale-125"
            style={{ background: c, outline: '2px solid var(--border)', outlineOffset: 1 }}
          />
        ))}

        <span className="w-px self-stretch mx-0.5" style={{ background: 'var(--border)' }} />
        <ToolBtn title="Hoàn tác"      onClick={() => exec('undo')}>↩</ToolBtn>
        <ToolBtn title="Làm lại"       onClick={() => exec('redo')}>↪</ToolBtn>
        <ToolBtn title="Xoá định dạng" onClick={() => exec('removeFormat')}>✕</ToolBtn>
      </div>

      {/* Vùng soạn thảo */}
      <div
        ref={editorRef}
        contentEditable
        suppressContentEditableWarning
        onCompositionStart={() => { isComposing.current = true; }}
        onCompositionEnd={() => { isComposing.current = false; syncOut(); }}
        onInput={() => { if (!isComposing.current) syncOut(); }}
        onKeyDown={e => {
          if (e.key === 'Tab') { e.preventDefault(); exec('insertText', '\u00a0\u00a0\u00a0\u00a0'); }
        }}
        className="min-h-[300px] max-h-[480px] overflow-y-auto p-4 outline-none text-[14px] leading-relaxed"
        style={{ color: 'var(--text-primary)', background: 'var(--input-bg)', caretColor: '#0ea5e9' }}
        data-placeholder="Nhập nội dung bài viết..."
      />

      <style>{`
        [contenteditable][data-placeholder]:empty:before {
          content: attr(data-placeholder);
          color: var(--text-muted); pointer-events: none;
        }
        [contenteditable] h1 { font-size:1.75em; font-weight:800; margin:.6em 0 .3em; }
        [contenteditable] h2 { font-size:1.35em; font-weight:700; margin:.5em 0 .3em; }
        [contenteditable] h3 { font-size:1.1em;  font-weight:600; margin:.4em 0 .2em; }
        [contenteditable] blockquote {
          border-left:4px solid #0ea5e9; padding:.5em 1em; margin:.6em 0;
          background:rgba(14,165,233,.07); border-radius:0 8px 8px 0;
          color:var(--text-secondary); font-style:italic;
        }
        [contenteditable] pre {
          background:#0f172a; color:#e2e8f0; padding:.75em 1em;
          border-radius:8px; font-family:monospace; font-size:.85em;
          margin:.6em 0; white-space:pre-wrap;
        }
        [contenteditable] ul { list-style:disc;    padding-left:1.5em; margin:.3em 0; }
        [contenteditable] ol { list-style:decimal; padding-left:1.5em; margin:.3em 0; }
        [contenteditable] a  { color:#0ea5e9; text-decoration:underline; }
        [contenteditable] hr { border:none; border-top:2px solid var(--border); margin:.75em 0; }
        [contenteditable] img { max-width:100%; border-radius:8px; margin:.5em 0; }
      `}</style>
    </div>
  );
}

/* ─── Default form ──────────────────────────────────────────── */
const empty = {
  title:'', slug:'', excerpt:'', content:'',
  thumbnailUrl:'', status:'Draft', isFeatured:false,
  sortOrder:0, tags:'', metaTitle:'', metaDescription:'', publishedAt:'',
};

/* ─── Main Modal ────────────────────────────────────────────── */
export default function PostFormModal({ post, onClose, onRefresh }) {
  const isEdit = !!post?.id;

  const [form,          setForm]          = useState(empty);
  const [thumbnailFile, setThumbnailFile] = useState(null);
  const [previewThumb,  setPreviewThumb]  = useState(null);
  const [busy,          setBusy]          = useState(false);
  const [toast,         setToast]         = useState('');
  const [activeTab,     setActiveTab]     = useState('content');

  const thumbRef  = useRef();
  const inlineRef = useRef();

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2600); };

  useEffect(() => {
    if (post?.id) {
      setForm({
        title:           post.title           || '',
        slug:            post.slug            || '',
        excerpt:         post.excerpt         || '',
        content:         post.content         || '',
        thumbnailUrl:    post.thumbnailUrl    || '',
        status:          post.status          || 'Draft',
        isFeatured:      post.isFeatured      ?? false,
        sortOrder:       post.sortOrder       ?? 0,
        tags:            post.tags            || '',
        metaTitle:       post.metaTitle       || '',
        metaDescription: post.metaDescription || '',
        publishedAt:     post.publishedAt ? post.publishedAt.slice(0,16) : '',
      });
      setPreviewThumb(post.thumbnailUrl ? toAbsoluteUrl(post.thumbnailUrl) : null);
    } else {
      setForm(empty);
      setPreviewThumb(null);
    }
    setThumbnailFile(null);
    setActiveTab('content');
  }, [post]);

  const set = (k, v) => setForm(f => ({ ...f, [k]: v }));

  const onTitleChange = v => {
    set('title', v);
    if (!isEdit) set('slug', toSlug(v));
  };

  const onThumbChange = e => {
    const f = e.target.files?.[0];
    if (!f) return;
    setThumbnailFile(f);
    setPreviewThumb(URL.createObjectURL(f));
    set('thumbnailUrl', '');
  };

  const onInlineFileChange = e => {
    const f = e.target.files?.[0];
    if (!f) return;
    const url = URL.createObjectURL(f);
    // Tìm editor trong cùng modal wrapper
    const wrapper = inlineRef.current?.closest('[data-post-modal]');
    const el = wrapper?.querySelector('[contenteditable]');
    if (el) {
      el.focus();
      document.execCommand('insertImage', false, url);
      set('content', el.innerHTML);
    }
    e.target.value = '';
  };

  const doSave = async () => {
    if (!form.title.trim()) { alert('Nhập tiêu đề bài viết.'); return; }
    if (form.content.replace(/<[^>]+>/g,'').trim().length < 10) {
      alert('Nội dung bài viết quá ngắn.'); return;
    }
    setBusy(true);
    try {
      const params = {
        title:           form.title.trim(),
        slug:            form.slug.trim()            || undefined,
        excerpt:         form.excerpt.trim()         || undefined,
        content:         form.content,
        thumbnailUrl:    form.thumbnailUrl.trim()    || undefined,
        status:          form.status,
        isFeatured:      form.isFeatured,
        sortOrder:       Number(form.sortOrder),
        tags:            form.tags.trim()            || undefined,
        metaTitle:       form.metaTitle.trim()       || undefined,
        metaDescription: form.metaDescription.trim() || undefined,
        publishedAt:     form.publishedAt ? new Date(form.publishedAt).toISOString() : undefined,
      };
      if (isEdit) {
        await adminPostApi.update(post.id, params, thumbnailFile || null);
        ok('✅ Đã cập nhật bài viết');
      } else {
        await adminPostApi.create(params, thumbnailFile || null);
        ok('✅ Đã tạo bài viết');
      }
      onRefresh();
      setTimeout(onClose, 700);
    } catch (e) {
      alert(e?.response?.data?.message || e?.message || 'Có lỗi xảy ra.');
    } finally {
      setBusy(false);
    }
  };

  const TAB = ({ id, label }) => (
    <button
      type="button"
      onClick={() => setActiveTab(id)}
      className="px-4 py-2 text-[13px] font-semibold rounded-xl transition-all"
      style={{ background: activeTab === id ? '#0ea5e9' : 'transparent', color: activeTab === id ? '#fff' : 'var(--text-muted)' }}
    >
      {label}
    </button>
  );

  return (
    // ✅ FIX 1: open={!!post}  →  mở khi post = {} (tạo mới) hoặc { id: N } (sửa)
    // ✅ FIX 2: width=900 (số, không phải string "max-w-5xl")
    <Modal open={!!post} onClose={onClose} title={isEdit ? '✏️ Sửa bài viết' : '📝 Tạo bài viết mới'} width={900}>
      <Toast msg={toast} />

      <div data-post-modal>
        <input ref={thumbRef}  type="file" accept="image/*" className="hidden" onChange={onThumbChange} />
        <input ref={inlineRef} type="file" accept="image/*" className="hidden" onChange={onInlineFileChange} />

        {/* Tabs */}
        <div className="flex gap-1 mb-5 p-1 rounded-2xl" style={{ background: 'var(--bg-elevated)' }}>
          <TAB id="content"  label="📄 Nội dung" />
          <TAB id="seo"      label="🔍 SEO & Meta" />
          <TAB id="settings" label="⚙️ Cài đặt" />
        </div>

        {/* ── Nội dung ─────────────────────────────────────────── */}
        {activeTab === 'content' && (
          <div className="space-y-4">
            <Field label="Tiêu đề" required>
              <Input value={form.title} onChange={e => onTitleChange(e.target.value)} placeholder="Tiêu đề bài viết..." />
            </Field>

            <Field label="Slug (URL)">
              <div className="flex items-center gap-2">
                <span className="text-[12px] font-mono shrink-0" style={{ color: 'var(--text-muted)' }}>/tin-tuc/</span>
                <Input value={form.slug} onChange={e => set('slug', toSlug(e.target.value))} placeholder="tu-dong-tao-tu-tieu-de" className="font-mono text-[13px]" />
              </div>
            </Field>

            <Field label="Tóm tắt">
              <textarea
                rows={2} value={form.excerpt} onChange={e => set('excerpt', e.target.value)}
                placeholder="Mô tả ngắn hiển thị trong danh sách..."
                className="w-full px-3 py-2 rounded-xl text-[13px] resize-none focus:outline-none"
                style={{ background: 'var(--input-bg)', border: '1px solid var(--input-border)', color: 'var(--input-text)' }}
                onFocus={e => { e.target.style.borderColor='#0ea5e9'; e.target.style.boxShadow='0 0 0 3px rgba(14,165,233,.15)'; }}
                onBlur={e  => { e.target.style.borderColor='var(--input-border)'; e.target.style.boxShadow='none'; }}
              />
            </Field>

            <Field label="Ảnh đại diện (Thumbnail)">
              <div className="flex gap-3 items-start">
                <div
                  className="w-28 h-20 rounded-xl overflow-hidden shrink-0 cursor-pointer flex items-center justify-center text-3xl hover:opacity-80 transition-opacity"
                  style={{ background: 'var(--bg-elevated)', border: '2px dashed var(--border)' }}
                  onClick={() => thumbRef.current?.click()}
                >
                  {previewThumb ? <img src={previewThumb} alt="" className="w-full h-full object-cover" /> : '📷'}
                </div>
                <div className="flex-1 space-y-2">
                  <BtnSecondary className="text-[12px] py-1.5" onClick={() => thumbRef.current?.click()}>
                    📁 Chọn ảnh từ máy
                  </BtnSecondary>
                  <p className="text-[11px]" style={{ color: 'var(--text-muted)' }}>Hoặc nhập URL ảnh</p>
                  <Input
                    value={form.thumbnailUrl}
                    onChange={e => { set('thumbnailUrl', e.target.value); setPreviewThumb(e.target.value || null); setThumbnailFile(null); }}
                    placeholder="https://..." className="text-[12px]"
                  />
                </div>
              </div>
            </Field>

            <Field label="Nội dung" required>
              <RichEditor
                value={form.content}
                onChange={v => set('content', v)}
                onImageInsert={() => inlineRef.current?.click()}
              />
              <p className="text-[11px] mt-1.5" style={{ color: 'var(--text-muted)' }}>
                <b>Ctrl+B</b> in đậm · <b>Ctrl+I</b> nghiêng · <b>Ctrl+U</b> gạch dưới · 🖼 chèn ảnh · Bấm chấm màu để đổi màu chữ
              </p>
            </Field>

            <Field label="Tags (cách nhau bằng dấu phẩy)">
              <Input value={form.tags} onChange={e => set('tags', e.target.value)} placeholder="react, nextjs, design..." />
            </Field>
          </div>
        )}

        {/* ── SEO ──────────────────────────────────────────────── */}
        {activeTab === 'seo' && (
          <div className="space-y-4">
            <Field label="Meta Title">
              <Input value={form.metaTitle} onChange={e => set('metaTitle', e.target.value)} placeholder="Để trống → dùng tiêu đề bài viết" />
              <p className="text-[11px] mt-1" style={{ color: form.metaTitle.length > 60 ? '#ef4444' : 'var(--text-muted)' }}>
                {form.metaTitle.length}/60 ký tự
              </p>
            </Field>

            <Field label="Meta Description">
              <textarea
                rows={3} value={form.metaDescription} onChange={e => set('metaDescription', e.target.value)}
                placeholder="Mô tả xuất hiện trên Google..."
                className="w-full px-3 py-2 rounded-xl text-[13px] resize-none focus:outline-none"
                style={{ background: 'var(--input-bg)', border: '1px solid var(--input-border)', color: 'var(--input-text)' }}
                onFocus={e => { e.target.style.borderColor='#0ea5e9'; e.target.style.boxShadow='0 0 0 3px rgba(14,165,233,.15)'; }}
                onBlur={e  => { e.target.style.borderColor='var(--input-border)'; e.target.style.boxShadow='none'; }}
              />
              <p className="text-[11px] mt-1" style={{ color: form.metaDescription.length > 160 ? '#ef4444' : 'var(--text-muted)' }}>
                {form.metaDescription.length}/160 ký tự
              </p>
            </Field>

            <div className="rounded-xl p-4" style={{ background: 'var(--bg-elevated)', border: '1px solid var(--border)' }}>
              <p className="text-[10px] font-bold uppercase tracking-widest mb-3" style={{ color: 'var(--text-muted)' }}>Xem trước Google</p>
              <p className="text-[14px] font-semibold text-blue-500 truncate">{form.metaTitle || form.title || 'Tiêu đề bài viết'}</p>
              <p className="text-[11px] text-green-600 truncate">{window.location.origin}/tin-tuc/{form.slug || 'slug-bai-viet'}</p>
              <p className="text-[12px] mt-1 leading-relaxed" style={{ color: 'var(--text-secondary)' }}>
                {(form.metaDescription || form.excerpt || 'Mô tả bài viết sẽ xuất hiện ở đây...').slice(0,160)}
              </p>
            </div>
          </div>
        )}

        {/* ── Cài đặt ──────────────────────────────────────────── */}
        {activeTab === 'settings' && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <Field label="Trạng thái">
                <Select value={form.status} onChange={e => set('status', e.target.value)} className="w-full">
                  <option value="Draft">📝 Draft — Bản nháp</option>
                  <option value="Published">✅ Published — Công khai</option>
                  <option value="Archived">📦 Archived — Lưu trữ</option>
                </Select>
              </Field>
              <Field label="Thứ tự sắp xếp">
                <Input type="number" min={0} value={form.sortOrder} onChange={e => set('sortOrder', e.target.value)} />
              </Field>
            </div>

            <Field label="Ngày xuất bản">
              <Input type="datetime-local" value={form.publishedAt} onChange={e => set('publishedAt', e.target.value)} />
              <p className="text-[11px] mt-1" style={{ color: 'var(--text-muted)' }}>Để trống → lấy thời điểm chuyển Published</p>
            </Field>

            <Field label="Bài viết nổi bật">
              <label className="flex items-center gap-3 cursor-pointer select-none">
                <div
                  className="relative w-11 h-6 rounded-full transition-colors duration-300"
                  style={{ background: form.isFeatured ? '#0ea5e9' : 'var(--bg-elevated)', border: '1px solid var(--border)' }}
                  onClick={() => set('isFeatured', !form.isFeatured)}
                >
                  <div
                    className="absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-all duration-300"
                    style={{ left: form.isFeatured ? 22 : 2 }}
                  />
                </div>
                <span className="text-[13px]" style={{ color: 'var(--text-secondary)' }}>
                  {form.isFeatured ? '⭐ Hiển thị nổi bật lên đầu danh sách' : 'Không nổi bật'}
                </span>
              </label>
            </Field>
          </div>
        )}

        {/* Footer */}
        <div className="flex justify-between items-center mt-6 pt-4" style={{ borderTop: '1px solid var(--border)' }}>
          <p className="text-[12px]" style={{ color: 'var(--text-muted)' }}>
            {form.content
              ? `~${Math.max(1, Math.ceil(form.content.replace(/<[^>]+>/g,'').split(/\s+/).filter(Boolean).length / 200))} phút đọc · ${form.content.replace(/<[^>]+>/g,'').length} ký tự`
              : 'Chưa có nội dung'
            }
          </p>
          <div className="flex gap-2">
            <BtnSecondary onClick={onClose} disabled={busy}>Huỷ</BtnSecondary>
            <BtnPrimary onClick={doSave} disabled={busy}>
              {busy ? '⏳ Đang lưu...' : isEdit ? '💾 Cập nhật' : '🚀 Tạo bài viết'}
            </BtnPrimary>
          </div>
        </div>
      </div>
    </Modal>
  );
}