import api from './client';

// ══════════════════════════════════════
// AUTH
// ══════════════════════════════════════
export const authApi = {
  login: (data) => api.post('/api/auth/login', data),
  register: (data) => api.post('/api/auth/register', data),
  logout: (refreshToken) => api.post('/api/auth/logout', { refreshToken }),
  refreshToken: (refreshToken) => api.post('/api/auth/refreshtoken', { refreshToken }),
  forgotPassword: (email) => api.post('/api/auth/forgotpassword', { email }),
  resetPassword: (data) => api.post('/api/auth/resetpassword', data),
  changePassword: (data) => api.post('/api/auth/changepassword', data),
  verifyEmail: (token) => api.get(`/api/auth/verifyemail?token=${token}`),
  resendVerifyEmail: (email) => api.post('/api/auth/resendverifyemail', { email }),
};

// ══════════════════════════════════════
// USER
// ══════════════════════════════════════
export const userApi = {
  getProfile: () => api.get('/api/user/profile'),
  updateProfile: (data) => api.put('/api/user/profile', data),
  updateAvatar: (file) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.put('/api/user/avatar', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  deleteAccount: (password) => api.delete('/api/user/account', { data: { password } }),
  getPurchases: (page = 1, pageSize = 10) =>
    api.get(`/api/user/purchases?page=${page}&pageSize=${pageSize}`),
  getDownloads: (page = 1, pageSize = 10) =>
    api.get(`/api/user/downloads?page=${page}&pageSize=${pageSize}`),
  getWishlist: (page = 1, pageSize = 12) =>
    api.get(`/api/user?page=${page}&pageSize=${pageSize}`),
  toggleWishlist: (templateId) =>
    api.post(`/api/user/wishlist/${templateId}`),
  getNotifications: (page = 1, pageSize = 20, unreadOnly = null) => {
    let url = `/api/user/notifications?page=${page}&pageSize=${pageSize}`;
    if (unreadOnly !== null) url += `&unreadOnly=${unreadOnly}`;
    return api.get(url);
  },
  markNotificationRead: (id) => api.patch(`/api/user/notifications/${id}/read`),
  markAllNotificationsRead: () => api.patch('/api/user/notifications/read-all'),
  getMyReviews: () => api.get('/api/user/reviews'),
  updateReview: (id, data) => api.put(`/api/user/reviews/${id}`, data),
  deleteReview: (id) => api.delete(`/api/user/reviews/${id}`),
};

// ══════════════════════════════════════
// TEMPLATES
// ══════════════════════════════════════
export const templateApi = {
  getList: (params = {}) => api.get('/api/templates', { params }),
  // Trang Săn Sale: chỉ lấy templates đang sale hợp lệ, kèm countdown & % giảm
  // Params: { search?, categorySlug?, page?, pageSize? }
  getOnSaleList: (params = {}) => api.get('/api/templates/on-sale', { params }),
  getDetail: (slug) => api.get(`/api/templates/${slug}`),
  download: (slug) => api.get(`/api/templates/${slug}/download`, { responseType: 'blob' }),
  getReviews: (slug, page = 1, pageSize = 10) =>
    api.get(`/api/templates/${slug}/reviews?page=${page}&pageSize=${pageSize}`),
  createReview: (slug, data) => api.post(`/api/templates/${slug}/reviews`, data),
  getPreviewUrl: (templateId) =>
    api.get(`/api/preview/${templateId}`, {
      responseType: 'text',
      transformResponse: [(data) => data],
    }),
};

// ══════════════════════════════════════
// ORDERS
// ══════════════════════════════════════
export const orderApi = {
  // data: { templateIds, couponCode?, affiliateCode?, note? }
  create: (data) => api.post('/api/orders', data),
  getMyOrders: (page = 1, pageSize = 10, status = null) => {
    let url = `/api/user/purchases?page=${page}&pageSize=${pageSize}`;
    if (status) url += `&status=${status}`;
    return api.get(url);
  },
  getDetail: (id) => api.get(`/api/orders/${id}`),
  getDetailByCode: (code) => api.get(`/api/orders/code/${code}`),
  applyCoupon: (data) => api.post('/api/orders/apply-coupon', data),
  createPayment: (id) => api.post(`/api/orders/${id}/payment`),
  cancel: (id, reason) => api.post(`/api/orders/${id}/cancel`, { reason }),
  getPaymentStatus: (id) => api.get(`/api/orders/${id}/payment-status`),
};

// ══════════════════════════════════════
// TICKETS
// ══════════════════════════════════════
export const ticketApi = {
  getList: (page = 1, pageSize = 10) =>
    api.get(`/api/tickets?page=${page}&pageSize=${pageSize}`),
  getDetail: (id) => api.get(`/api/tickets/${id}`),
  create: (data) => api.post('/api/tickets', data),
  reply: (id, data) => api.post(`/api/tickets/${id}/reply`, data),
};

// ══════════════════════════════════════
// COUPONS
// ══════════════════════════════════════
export const couponApi = {
  // Lấy danh sách coupon public đang active (user xem để copy mã)
  getPublicList: () => api.get('/api/coupons'),
  // Preview discount trước khi tạo đơn (dùng lại /api/orders/apply-coupon)
  preview: (couponCode, templateIds) =>
    api.post('/api/orders/apply-coupon', { couponCode, templateIds }),
};

// ══════════════════════════════════════
// AFFILIATE
// ══════════════════════════════════════
export const affiliateApi = {
  register: () => api.post('/api/affiliate/register'),
  getStats: () => api.get('/api/affiliate/stats'),
    getTransactions: (params = {}) =>
    api.get('/api/affiliate/transactions', { params }),
};

// ══════════════════════════════════════
// PUBLIC
// ══════════════════════════════════════
export const publicApi = {
  getBanners: (position = null) => {
    const url = position ? `/api/banners?position=${position}` : '/api/banners';
    return api.get(url);
  },
  getCategories: () => api.get('/api/categories'),
  getTags: () => api.get('/api/tags'),
  trackPageview: (data) => api.post('/api/analytics/track', data),
  updateTimeOnPage: (data) => api.patch('/api/analytics/time-on-page', data),
};