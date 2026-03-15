import axios from 'axios';

// Base URL – đổi thành domain thực của bạn
const BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7217';

const api = axios.create({
  baseURL: BASE_URL,
  withCredentials: true, // Bắt buộc để gửi/nhận Cookie httpOnly
  headers: { 'Content-Type': 'application/json' },
});

// ── Response Interceptor: tự động refresh token khi 401 ──
let isRefreshing = false;
let failedQueue = [];

const processQueue = (error) => {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error);
    else resolve();
  });
  failedQueue = [];
};

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then(() => api.request(originalRequest))
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      const refreshToken = localStorage.getItem('refreshToken');
      if (!refreshToken) {
        isRefreshing = false;
        localStorage.clear();
        window.location.href = '/login';
        return Promise.reject(error);
      }

      try {
        const res = await api.post('/api/auth/refreshtoken', { refreshToken });
        const newRefreshToken = res.data?.data?.refreshToken;
        if (newRefreshToken) {
          localStorage.setItem('refreshToken', newRefreshToken);
        }
        processQueue(null);
        return api.request(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError);
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        window.location.href = '/login';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export default api;

// ── Helper để trích lỗi từ API response ──
export const extractError = (error) => {
  const data = error?.response?.data;
  if (!data) return error?.message || 'Đã xảy ra lỗi';
  if (data.errors && data.errors.length > 0) return data.errors.join('\n');
  return data.message || 'Đã xảy ra lỗi';
};
export const toAbsoluteUrl = (url) => {
  if (!url) return null;
  if (url.startsWith('http')) return url;
  return `${BASE_URL}${url}`;  // ← dùng BASE_URL thay vì import.meta.env
};