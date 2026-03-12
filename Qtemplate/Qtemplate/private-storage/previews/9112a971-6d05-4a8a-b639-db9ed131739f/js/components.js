/* ============================================================
   LUXE STORE — components.js
   Nav + Footer HTML templates, inject vào DOM ngay khi load.
   Script nằm cuối <body> nên DOM đã sẵn sàng — không cần
   DOMContentLoaded. app.js (load sau) xử lý toàn bộ events.
   ============================================================ */

/* ── Navigation HTML ── */
const NAV_HTML = `
<div class="topbar">
  ✦ MIỄN PHÍ VẬN CHUYỂN CHO ĐƠN HÀNG TRÊN 500K ✦ ĐỔI TRẢ TRONG 30 NGÀY ✦
</div>
<nav>
  <div class="nav-inner">
    <a class="logo" href="#" data-page="home">LUXE</a>
    <ul class="nav-links">
      <li><a href="#" data-page="home">Trang Chủ</a></li>
      <li><a href="#" data-page="shop">Sản Phẩm</a></li>
      <li><a href="#" data-page="product-detail">Chi Tiết</a></li>
      <li><a href="#" data-page="wishlist">Yêu Thích</a></li>
      <li><a href="#" data-page="cart">Giỏ Hàng</a></li>
      <li><a href="#" data-page="checkout">Thanh Toán</a></li>
      <li><a href="#" data-page="account">Tài Khoản</a></li>
    </ul>
    <div class="nav-icons">
      <button title="Tìm kiếm">🔍</button>
      <button title="Yêu thích" data-page="wishlist">♡</button>
      <button title="Giỏ hàng" data-page="cart">
        🛒<span class="cart-badge">3</span>
      </button>
      <button title="Tài khoản" data-page="account">👤</button>
    </div>
    <button class="mobile-menu-btn" aria-label="Menu">
      <span></span><span></span><span></span>
    </button>
  </div>
</nav>
`;

/* ── Footer HTML ── */
const FOOTER_HTML = `
<footer>
  <div class="footer-grid">
    <div class="footer-brand">
      <a class="logo" href="#" data-page="home">LUXE</a>
      <p>Điểm đến thời trang cao cấp hàng đầu Việt Nam. Chúng tôi mang đến sản phẩm chính hãng từ các thương hiệu danh tiếng thế giới.</p>
      <div class="social-links">
        <a href="#" aria-label="Facebook">f</a>
        <a href="#" aria-label="Instagram">ig</a>
        <a href="#" aria-label="YouTube">yt</a>
        <a href="#" aria-label="TikTok">tk</a>
      </div>
    </div>
    <div class="footer-col">
      <h4>Sản Phẩm</h4>
      <ul>
        <li><a href="#" data-page="shop">Thời Trang Nữ</a></li>
        <li><a href="#" data-page="shop">Thời Trang Nam</a></li>
        <li><a href="#" data-page="shop">Túi Xách</a></li>
        <li><a href="#" data-page="shop">Giày Dép</a></li>
        <li><a href="#" data-page="shop">Trang Sức</a></li>
      </ul>
    </div>
    <div class="footer-col">
      <h4>Hỗ Trợ</h4>
      <ul>
        <li><a href="#">Câu Hỏi Thường Gặp</a></li>
        <li><a href="#">Hướng Dẫn Mua Hàng</a></li>
        <li><a href="#">Chính Sách Đổi Trả</a></li>
        <li><a href="#">Theo Dõi Đơn Hàng</a></li>
        <li><a href="#">Liên Hệ</a></li>
      </ul>
    </div>
    <div class="footer-col">
      <h4>Về Chúng Tôi</h4>
      <ul>
        <li><a href="#">Câu Chuyện Thương Hiệu</a></li>
        <li><a href="#">Blog Thời Trang</a></li>
        <li><a href="#">Chương Trình Thành Viên</a></li>
        <li><a href="#">Tuyển Dụng</a></li>
      </ul>
    </div>
  </div>
  <div class="footer-bottom">
    <p>© 2025 LUXE Store. Tất cả quyền được bảo lưu.</p>
    <div class="payment-icons">
      <span class="payment-icon">VISA</span>
      <span class="payment-icon">MC</span>
      <span class="payment-icon">MOMO</span>
      <span class="payment-icon">VNPAY</span>
      <span class="payment-icon">COD</span>
    </div>
  </div>
</footer>
`;

/* ── Inject ngay — script ở cuối <body> nên DOM đã có ── */
(function mount() {
  const header = document.getElementById('site-header');
  if (header) header.innerHTML = NAV_HTML;

  document.querySelectorAll('.footer-slot').forEach(slot => {
    slot.innerHTML = FOOTER_HTML;
  });
})();
