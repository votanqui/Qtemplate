const AFFILIATE_KEY = 'aff_code';
const AFFILIATE_EXPIRE_DAYS = 30;

/**
 * Lưu affiliate code vào cookie 30 ngày
 */
export function saveAffiliateCode(code) {
  if (!code) return;
  const expires = new Date();
  expires.setDate(expires.getDate() + AFFILIATE_EXPIRE_DAYS);
  document.cookie = `${AFFILIATE_KEY}=${encodeURIComponent(code)};expires=${expires.toUTCString()};path=/;SameSite=Lax`;
}

/**
 * Đọc affiliate code từ cookie
 */
export function getAffiliateCode() {
  const match = document.cookie.match(new RegExp(`(^| )${AFFILIATE_KEY}=([^;]+)`));
  return match ? decodeURIComponent(match[2]) : null;
}

/**
 * Xóa affiliate code sau khi đã dùng
 */
export function clearAffiliateCode() {
  document.cookie = `${AFFILIATE_KEY}=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/`;
}