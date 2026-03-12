/* ============================================================
   LUXE STORE — app.js
   Pure event delegation — hoạt động trong iframe.
   ============================================================ */

/* ── Router ── */
const Router = (() => {
  function navigate(pageId) {
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    const target = document.getElementById(pageId);
    if (!target) return;
    target.classList.add('active');
    target.scrollIntoView({ block: 'start', behavior: 'smooth' });
    document.querySelectorAll('.nav-links [data-page]').forEach(el => {
      el.classList.toggle('active', el.dataset.page === pageId);
    });
  }

  // Capture phase — bắt được mọi click kể cả element inject sau
  document.addEventListener('click', e => {
    const el = e.target.closest('[data-page]');
    if (!el) return;
    if (el.dataset.action) return; // bỏ qua nếu là action button
    e.preventDefault();
    navigate(el.dataset.page);
  }, true);

  return { navigate };
})();

/* ── Toast ── */
const Toast = (() => {
  let timer = null;
  function show(msg) {
    const el = document.getElementById('toast');
    if (!el) return;
    el.textContent = msg;
    el.classList.add('show');
    clearTimeout(timer);
    timer = setTimeout(() => el.classList.remove('show'), 2800);
  }
  return { show };
})();

/* ── Cart ── */
const Cart = (() => {
  let count = 3;
  function updateBadge() {
    document.querySelectorAll('.cart-badge').forEach(b => b.textContent = count);
  }
  function add() { count++; updateBadge(); Toast.show('🛒 Đã thêm vào giỏ hàng!'); }
  return { add, updateBadge };
})();

/* ── Action buttons (add-to-cart, wishlist, dec, inc) ── */
document.addEventListener('click', e => {
  const btn = e.target.closest('[data-action]');
  if (!btn) return;
  const action = btn.dataset.action;

  if (action === 'add-to-cart') { e.stopPropagation(); Cart.add(); return; }
  if (action === 'wishlist')    { e.stopPropagation(); Toast.show('♡ Đã thêm vào yêu thích!'); return; }

  if (action === 'dec') {
    const d = btn.closest('.qty-control')?.querySelector('.qty-value');
    if (d && parseInt(d.textContent) > 1) d.textContent = parseInt(d.textContent) - 1;
    return;
  }
  if (action === 'inc') {
    const d = btn.closest('.qty-control')?.querySelector('.qty-value');
    if (d) d.textContent = parseInt(d.textContent) + 1;
    return;
  }
});

/* ── Color swatches ── */
document.addEventListener('click', e => {
  const sw = e.target.closest('.color-swatch, .color-dot');
  if (!sw) return;
  const g = sw.closest('.color-options, .color-swatches');
  if (!g) return;
  g.querySelectorAll('.color-swatch, .color-dot').forEach(s => s.classList.remove('active'));
  sw.classList.add('active');
  const lbl = sw.closest('.option-group')?.querySelector('.selected-color');
  if (lbl && sw.dataset.color) lbl.textContent = sw.dataset.color;
});

/* ── Size buttons ── */
document.addEventListener('click', e => {
  const btn = e.target.closest('.size-btn');
  if (!btn) return;
  btn.closest('.size-options')?.querySelectorAll('.size-btn').forEach(b => b.classList.remove('active'));
  btn.classList.add('active');
});

/* ── Tabs ── */
document.addEventListener('click', e => {
  const h = e.target.closest('.tab-header');
  if (!h) return;
  const group   = h.closest('.detail-tabs');
  if (!group) return;
  const headers = group.querySelectorAll('.tab-header');
  const panels  = group.querySelectorAll('.tab-panel');
  const idx     = Array.from(headers).indexOf(h);
  headers.forEach(x => x.classList.remove('active'));
  panels.forEach(x => x.classList.remove('active'));
  h.classList.add('active');
  if (panels[idx]) panels[idx].classList.add('active');
});

/* ── Thumbnails ── */
document.addEventListener('click', e => {
  const t = e.target.closest('.thumb');
  if (!t) return;
  t.closest('.thumb-row')?.querySelectorAll('.thumb').forEach(x => x.classList.remove('active'));
  t.classList.add('active');
});

/* ── Payment options ── */
document.addEventListener('click', e => {
  const opt = e.target.closest('.payment-option');
  if (!opt) return;
  const g = opt.closest('.payment-options');
  if (!g) return;
  g.querySelectorAll('.payment-option').forEach(o => o.classList.remove('selected'));
  opt.classList.add('selected');
  const r = opt.querySelector('input[type="radio"]');
  if (r) r.checked = true;
});

/* ── Remove cart row ── */
document.addEventListener('click', e => {
  const btn = e.target.closest('.remove-btn');
  if (!btn) return;
  const row = btn.closest('tr');
  if (!row) return;
  row.style.cssText = 'opacity:0;transition:opacity .3s';
  setTimeout(() => row.remove(), 300);
  Toast.show('🗑 Đã xóa sản phẩm');
});

/* ── Remove wishlist card ── */
document.addEventListener('click', e => {
  const btn = e.target.closest('.wishlist-remove');
  if (!btn) return;
  const card = btn.closest('.wishlist-card');
  if (!card) return;
  card.style.cssText = 'opacity:0;transition:opacity .3s';
  setTimeout(() => card.remove(), 300);
  Toast.show('✕ Đã xóa khỏi yêu thích');
});

/* ── Newsletter ── */
document.addEventListener('click', e => {
  if (!e.target.closest('.newsletter-form button')) return;
  const inp = e.target.closest('.newsletter-form')?.querySelector('input');
  if (inp?.value.trim()) { Toast.show('✉ Đăng ký thành công!'); inp.value = ''; }
  else Toast.show('⚠ Vui lòng nhập email');
});

/* ── Checkout submit ── */
document.addEventListener('click', e => {
  if (e.target.closest('.checkout-submit')) Toast.show('✅ Đặt hàng thành công!');
});

/* ── Cancel order ── */
document.addEventListener('click', e => {
  const btn = e.target.closest('.btn-cancel');
  if (!btn) return;
  const card = btn.closest('.order-card');
  if (!card) return;
  card.style.cssText = 'opacity:0;transition:opacity .3s';
  setTimeout(() => card.remove(), 300);
  Toast.show('✕ Đã hủy đơn hàng');
});

/* ── View toggle ── */
document.addEventListener('click', e => {
  const btn = e.target.closest('.view-btn');
  if (!btn) return;
  btn.closest('.shop-controls')?.querySelectorAll('.view-btn').forEach(b => b.classList.remove('active'));
  btn.classList.add('active');
});

/* ── Init ── */
Cart.updateBadge();
