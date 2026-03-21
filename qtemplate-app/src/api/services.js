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
  getProfile: () => api.get('/api/User/profile'),
  updateProfile: (data) => api.put('/api/user/profile', data),
  updateAvatar: (file) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.put('/api/User/avatar', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  deleteAccount: (password) => api.delete('/api/User/account', { data: { password } }),
  getPurchases: (page = 1, pageSize = 10) =>
    api.get(`/api/User/purchases?page=${page}&pageSize=${pageSize}`),
  getDownloads: (page = 1, pageSize = 10) =>
    api.get(`/api/User/downloads?page=${page}&pageSize=${pageSize}`),
  getWishlist: (page = 1, pageSize = 12) =>
    api.get(`/api/User?page=${page}&pageSize=${pageSize}`),
  toggleWishlist: (templateId) =>
    api.post(`/api/User/wishlist/${templateId}`),
  getNotifications: (page = 1, pageSize = 20, unreadOnly = null) => {
    let url = `/api/User/notifications?page=${page}&pageSize=${pageSize}`;
    if (unreadOnly !== null) url += `&unreadOnly=${unreadOnly}`;
    return api.get(url);
  },
  markNotificationRead: (id) => api.patch(`/api/User/notifications/${id}/read`),
  markAllNotificationsRead: () => api.patch('/api/User/notifications/read-all'),
  getMyReviews: () => api.get('/api/User/reviews'),
  updateReview: (id, data) => api.put(`/api/User/reviews/${id}`, data),
  deleteReview: (id) => api.delete(`/api/User/reviews/${id}`),
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
export const postApi = {
  getList: (params = {}) => api.get('/api/posts', { params }),
  getBySlug: (slug) => api.get(`/api/posts/${slug}`),
};
export const communityApi = {
  // Feed
  getFeed: (page = 1, pageSize = 20, sortBy = 'hot') =>
    api.get('/api/community/feed', { params: { page, pageSize, sortBy } }),

  // Posts — ảnh upload qua multipart, text qua query params (đúng với backend)
  createPost: (content, imageFile = null) => {
    const fd = new FormData();
    if (imageFile) fd.append('imageFile', imageFile);
    return api.post('/api/community/posts', fd, {
      params: { content },
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
 
  updatePost: (id, content, imageFile = null, imageUrl = null) => {
    const fd = new FormData();
    if (imageFile) fd.append('imageFile', imageFile);
    return api.put(`/api/community/posts/${id}`, fd, {
      params: {
        content,
        ...(imageUrl !== undefined && imageUrl !== null ? { imageUrl } : {}),
      },
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
 
  deletePost: (id) => api.delete(`/api/community/posts/${id}`),
 
  toggleLike: (postId) => api.post(`/api/community/posts/${postId}/like`),
 
  // Comments
  getComments: (postId, page = 1, pageSize = 30) =>
    api.get(`/api/community/posts/${postId}/comments`, { params: { page, pageSize } }),
 
  createComment: (postId, content, parentId = null) =>
    api.post(`/api/community/posts/${postId}/comments`, {
      content,
      ...(parentId !== null ? { parentId } : {}),
    }),
 
  updateComment: (commentId, content) =>
    api.put(`/api/community/comments/${commentId}`, { content }),
 
  deleteComment: (commentId) =>
    api.delete(`/api/community/comments/${commentId}`),
};