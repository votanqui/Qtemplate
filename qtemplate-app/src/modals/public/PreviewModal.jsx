import { useEffect, useRef } from 'react';
import { createPortal } from 'react-dom';

export default function PreviewModal({ open, onClose, title, templateId }) {
  const iframeRef = useRef(null);

  // Chặn mọi click có href tuyệt đối trong iframe thoát ra ngoài website chính.
  //
  // Vấn đề:
  //   Template có link <a href="/"> hay <a href="https://..."> → browser điều hướng
  //   tab chính ra khỏi website, iframe không đóng.
  //
  // Giải pháp:
  //   Lắng nghe message từ script inject trong backend (hoặc từ iframe trực tiếp).
  //   Đồng thời dùng sandbox không có allow-top-navigation → link tuyệt đối bị chặn
  //   hoàn toàn, chỉ allow-top-navigation-by-user-activation cho popup/modal nội bộ.
  //
  useEffect(() => {
    if (!open) return;

    const handleMessage = (e) => {
      // Nhận message từ script inject trong ServePreviewFileQueryHandler
      if (e.data?.type === 'PREVIEW_NAVIGATE') {
        // Template muốn navigate → giữ trong iframe, không làm gì thêm
        // (script trong handler đã patch history API bên trong iframe)
      }
    };

    window.addEventListener('message', handleMessage);
    return () => window.removeEventListener('message', handleMessage);
  }, [open]);

  if (!open) return null;

  return createPortal(
    <div className="fixed inset-0 z-[99999] flex flex-col bg-white">

      {/* Header */}
      <div className="flex items-center justify-between px-5 h-12 bg-white border-b border-gray-200 shadow-sm shrink-0">
        <div className="flex items-center gap-2.5">
          <div className="w-6 h-6 rounded-lg bg-gray-900 flex items-center justify-center text-white text-[10px] font-black">
            Q
          </div>
          <div className="flex items-center gap-2">
            <span className="text-xs font-bold text-gray-400 uppercase tracking-widest">Preview</span>
            <span className="text-gray-300">·</span>
            <span className="text-sm font-bold text-gray-800 truncate max-w-[200px] sm:max-w-xs">{title}</span>
          </div>
        </div>

        <button
          onClick={onClose}
          className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl border border-gray-200 text-xs font-bold text-gray-600 hover:border-gray-400 hover:text-gray-900 transition-all"
        >
          <span>✕</span> Đóng
        </button>
      </div>

      {/* iframe */}
      <div className="flex-1 overflow-hidden bg-gray-50">
        <iframe
          ref={iframeRef}
          key={templateId}
          src={`/api/preview/${templateId}`}
          className="w-full h-full border-0 block"
          title={title}
          // sandbox:
          //   allow-scripts        → JS chạy được
          //   allow-same-origin    → fetch/cookie hoạt động (cùng origin)
          //   allow-forms          → form submit được
          //   allow-popups         → window.open được (một số template dùng)
          //   allow-modals         → alert/confirm được
          //   allow-pointer-lock   → game/canvas template
          //
          //   KHÔNG có allow-top-navigation → link <a href="/"> hay window.top.location
          //   sẽ bị chặn, KHÔNG thoát ra ngoài website chính ✅
          //
          //   KHÔNG có allow-popups-to-escape-sandbox → popup cũng bị sandbox ✅
          sandbox="allow-scripts allow-same-origin allow-forms allow-popups allow-modals allow-pointer-lock"
        />
      </div>
    </div>,
    document.body
  );
}