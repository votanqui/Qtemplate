import api from './client';

// ══════════════════════════════════════════════════════════════
// ADMIN — Stats
// GET /api/admin/stats/dashboard?from=&to=
// GET /api/admin/stats/orders?from=&to=
// GET /api/admin/stats/payments?from=&to=
// GET /api/admin/stats/coupons
// GET /api/admin/stats/analytics?from=&to=
// GET /api/admin/stats/media
// GET /api/admin/stats/security?from=&to=
// GET /api/admin/stats/ip-blacklist-stats
// GET /api/admin/stats/request-log-stats?from=&to=
// GET /api/admin/stats/email-log-stats
// GET /api/admin/stats/refresh-token-stats
// ══════════════════════════════════════════════════════════════
export const adminStatsApi = {
  getDashboard: (from = null, to = null) => {
    const params = {};
    if (from) params.from = from;
    if (to)   params.to   = to;
    return api.get('/api/admin/stats/dashboard', { params });
  },
  getOrderStats: (from = null, to = null) => {
    const params = {};
    if (from) params.from = from;
    if (to)   params.to   = to;
    return api.get('/api/admin/stats/orders', { params });
  },
  getPaymentStats: (from = null, to = null) => {
    const params = {};
    if (from) params.from = from;
    if (to)   params.to   = to;
    return api.get('/api/admin/stats/payments', { params });
  },
  getCouponStats:       () => api.get('/api/admin/stats/coupons'),
  getAnalytics: (from = null, to = null) => {
    const params = {};
    if (from) params.from = from;
    if (to)   params.to   = to;
    return api.get('/api/admin/stats/analytics', { params });
  },
  getMediaStats:         () => api.get('/api/admin/stats/media'),
  getSecurity: (from = null, to = null) => {
    const params = {};
    if (from) params.from = from;
    if (to)   params.to   = to;
    return api.get('/api/admin/stats/security', { params });
  },
  getIpBlacklistStats:   () => api.get('/api/admin/stats/ip-blacklist-stats'),
  getRequestLogStats: (from = null, to = null) => {
    const params = {};
    if (from) params.from = from;
    if (to)   params.to   = to;
    return api.get('/api/admin/stats/request-log-stats', { params });
  },
  getEmailLogStats:      () => api.get('/api/admin/stats/email-log-stats'),
  getRefreshTokenStats:  () => api.get('/api/admin/stats/refresh-token-stats'),
  getDailyStats: (period = 'daily', from = null, to = null) => {
  const params = { period };
  if (from) params.from = from;
  if (to)   params.to   = to;
  return api.get('/api/admin/stats/daily', { params });
},
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Templates
// GET    /api/admin/templates?search=&status=&page=&pageSize=
// POST   /api/admin/templates                          { ...CreateTemplateDto }
// PUT    /api/admin/templates/:id                      { ...UpdateTemplateDto }
// DELETE /api/admin/templates/:id
// PATCH  /api/admin/templates/:id/publish
// PATCH  /api/admin/templates/:id/status               { status }
// PATCH  /api/admin/templates/:id/pricing              { isFree, price }
// PATCH  /api/admin/templates/:id/sale                 { salePrice, saleStartAt, saleEndAt }
// PATCH  /api/admin/templates/:id/preview-url          { previewUrl }
// GET    /api/admin/templates/:slug                    (detail by slug)
// POST   /api/admin/templates/:id/thumbnail            FormData(file)
// DELETE /api/admin/templates/:id/thumbnail
// POST   /api/admin/templates/:id/preview              FormData(file)  max 50MB
// DELETE /api/admin/templates/:id/preview
// POST   /api/admin/templates/:id/images?type=Screenshot&sortOrder=0  FormData(file)
// DELETE /api/admin/templates/images/:imageId
// POST   /api/admin/templates/:id/versions?version=&changeLog=        FormData(file)  max 50MB
// POST   /api/admin/templates/:id/versions/link        { version, changeLog, externalUrl, storageType }
// GET    /api/admin/templates/:id/versions
// DELETE /api/admin/templates/:id/versions/:version
// ══════════════════════════════════════════════════════════════
export const adminTemplateApi = {
  getList: (params = {}) =>
    api.get('/api/admin/templates', { params }),

  getDetail: (slug) =>
    api.get(`/api/admin/templates/${slug}`),

  create: (data) =>
    api.post('/api/admin/templates', data),

  update: (id, data) =>
    api.put(`/api/admin/templates/${id}`, data),

  delete: (id) =>
    api.delete(`/api/admin/templates/${id}`),

  publish: (id) =>
    api.patch(`/api/admin/templates/${id}/publish`),

  changeStatus: (id, status) =>
    api.patch(`/api/admin/templates/${id}/status`, { status }),

  changePricing: (id, data) =>
    // data: { isFree, price }
    api.patch(`/api/admin/templates/${id}/pricing`, data),

  setSale: (id, data) =>
    // data: { salePrice, saleStartAt, saleEndAt }
    api.patch(`/api/admin/templates/${id}/sale`, data),
bulkSetSale: (data) =>
    // data: { templateIds: Guid[], salePrice?, saleStartAt?, saleEndAt? }
    // salePrice = null → xóa sale hàng loạt
    api.post('/api/admin/templates/bulk-sale', data),
    
  setPreviewUrl: (id, previewUrl) =>
    api.patch(`/api/admin/templates/${id}/preview-url`, { previewUrl }),

  // Thumbnail
  uploadThumbnail: (id, file) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post(`/api/admin/templates/${id}/thumbnail`, fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  deleteThumbnail: (id) =>
    api.delete(`/api/admin/templates/${id}/thumbnail`),

  // Preview (zip HTML)
  uploadPreview: (id, file, onProgress = null) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post(`/api/admin/templates/${id}/preview`, fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: onProgress,
    });
  },
  deletePreview: (id) =>
    api.delete(`/api/admin/templates/${id}/preview`),

  // Images (screenshot/gallery)
  addImage: (id, file, type = 'Screenshot', sortOrder = 0, altText = null) => {
    const fd = new FormData();
    fd.append('file', file);
    const params = { type, sortOrder };
    if (altText) params.altText = altText;
    return api.post(`/api/admin/templates/${id}/images`, fd, {
      params,
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  deleteImage: (imageId) =>
    api.delete(`/api/admin/templates/images/${imageId}`),

  // Versions
  getVersions: (id) =>
    api.get(`/api/admin/templates/${id}/versions`),

  addVersion: (id, file, version, changeLog = null, onProgress = null) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post(`/api/admin/templates/${id}/versions`, fd, {
      params: { version, ...(changeLog && { changeLog }) },
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: onProgress,
    });
  },
  addVersionLink: (id, data) =>
    // data: { version, changeLog?, externalUrl, storageType }
    api.post(`/api/admin/templates/${id}/versions/link`, data),

  deleteVersion: (id, version) =>
    api.delete(`/api/admin/templates/${id}/versions/${version}`),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Users
// GET   /api/admin/users?search=&role=&isActive=&page=&pageSize=
// GET   /api/admin/users/:id
// PATCH /api/admin/users/:id/status   { isActive, reason }
// PATCH /api/admin/users/:id/role     { role }
// GET   /api/admin/users/:id/orders?page=&pageSize=
// ══════════════════════════════════════════════════════════════
export const adminUserApi = {
  getList: (params = {}) =>
    api.get('/api/admin/users', { params }),

  getDetail: (id) =>
    api.get(`/api/admin/users/${id}`),

  changeStatus: (id, isActive, reason = null) =>
    api.patch(`/api/admin/users/${id}/status`, { isActive, reason }),

  changeRole: (id, role) =>
    api.patch(`/api/admin/users/${id}/role`, { role }),

  getUserOrders: (id, page = 1, pageSize = 10) =>
    api.get(`/api/admin/users/${id}/orders`, { params: { page, pageSize } }),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Orders
// GET  /api/admin/orders?status=&search=&page=&pageSize=
// GET  /api/admin/orders/:id
// POST /api/admin/orders/:id/cancel   { reason }
// ══════════════════════════════════════════════════════════════
export const adminOrderApi = {
  getList: (params = {}) =>
    api.get('/api/admin/orders', { params }),

  getDetail: (id) =>
    api.get(`/api/admin/orders/${id}`),

  cancel: (id, reason) =>
    api.post(`/api/admin/orders/${id}/cancel`, { reason }),
  updateStatus: (id, newStatus, note = null) =>
  api.patch(`/api/admin/orders/${id}/status`, { newStatus, note }),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Banners
// GET    /api/admin/banners?page=&pageSize=
// POST   /api/admin/banners?title=&subTitle=&imageUrl=&linkUrl=&position=&sortOrder=&isActive=&startAt=&endAt=  + FormData(imageFile?)
// PUT    /api/admin/banners/:id  (same query params + FormData?)
// DELETE /api/admin/banners/:id
// ══════════════════════════════════════════════════════════════
export const adminBannerApi = {
  getList: (page = 1, pageSize = 20) =>
    api.get('/api/admin/banners', { params: { page, pageSize } }),

  create: (params, imageFile = null) => {
    const fd = new FormData();
    if (imageFile) fd.append('imageFile', imageFile);
    return api.post('/api/admin/banners', fd, {
      params,
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },

  update: (id, params, imageFile = null) => {
    const fd = new FormData();
    if (imageFile) fd.append('imageFile', imageFile);
    return api.put(`/api/admin/banners/${id}`, fd, {
      params,
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },

  delete: (id) =>
    api.delete(`/api/admin/banners/${id}`),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Categories
// GET    /api/admin/categories
// POST   /api/admin/categories   { name, slug, description, iconUrl, isActive, sortOrder }
// PUT    /api/admin/categories/:id
// DELETE /api/admin/categories/:id
// ══════════════════════════════════════════════════════════════
export const adminCategoryApi = {
  getAll: () =>
    api.get('/api/admin/categories'),

  create: (data) =>
    api.post('/api/admin/categories', data),

  update: (id, data) =>
    api.put(`/api/admin/categories/${id}`, data),

  delete: (id) =>
    api.delete(`/api/admin/categories/${id}`),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Tags
// GET    /api/admin/tags
// POST   /api/admin/tags   { name, slug }
// PUT    /api/admin/tags/:id
// DELETE /api/admin/tags/:id
// ══════════════════════════════════════════════════════════════
export const adminTagApi = {
  getAll: () =>
    api.get('/api/admin/tags'),

  create: (data) =>
    api.post('/api/admin/tags', data),

  update: (id, data) =>
    api.put(`/api/admin/tags/${id}`, data),

  delete: (id) =>
    api.delete(`/api/admin/tags/${id}`),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Coupons
// GET    /api/admin/coupons?isActive=&page=&pageSize=
// POST   /api/admin/coupons   { code, type, value, minOrderAmount?, maxDiscountAmount?, usageLimit?, startAt?, expiredAt? }
// PUT    /api/admin/coupons/:id   { value, minOrderAmount?, maxDiscountAmount?, usageLimit?, isActive, startAt?, expiredAt? }
// DELETE /api/admin/coupons/:id
// ══════════════════════════════════════════════════════════════
export const adminCouponApi = {
  getList: (params = {}) =>
    api.get('/api/admin/coupons', { params }),

  create: (data) =>
    api.post('/api/admin/coupons', data),

  update: (id, data) =>
    api.put(`/api/admin/coupons/${id}`, data),

  delete: (id) =>
    api.delete(`/api/admin/coupons/${id}`),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Reviews
// GET    /api/admin/reviews?status=&page=&pageSize=
// PATCH  /api/admin/reviews/:id/approve   { isApproved }
// PATCH  /api/admin/reviews/:id/reply     { reply }
// DELETE /api/admin/reviews/:id
// ══════════════════════════════════════════════════════════════
export const adminReviewApi = {
  getList: (params = {}) =>
    api.get('/api/admin/reviews', { params }),

  approve: (id, isApproved) =>
    api.patch(`/api/admin/reviews/${id}/approve`, { isApproved }),

  reply: (id, reply) =>
    api.patch(`/api/admin/reviews/${id}/reply`, { reply }),

  delete: (id) =>
    api.delete(`/api/admin/reviews/${id}`),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Tickets
// GET    /api/admin/tickets?status=&priority=&page=&pageSize=
// GET    /api/admin/tickets/:id
// POST   /api/admin/tickets/:id/reply        { message, attachmentUrl? }
// PATCH  /api/admin/tickets/:id/status       { status }
// PATCH  /api/admin/tickets/:id/assign       { assignedTo }
// PATCH  /api/admin/tickets/:id/priority     { priority }
// ══════════════════════════════════════════════════════════════
export const adminTicketApi = {
  getList: (params = {}) =>
    api.get('/api/admin/tickets', { params }),

  getDetail: (id) =>
    api.get(`/api/admin/tickets/${id}`),

  reply: (id, message, attachmentUrl = null) =>
    api.post(`/api/admin/tickets/${id}/reply`, { message, attachmentUrl }),

  changeStatus: (id, status) =>
    api.patch(`/api/admin/tickets/${id}/status`, { status }),

  assign: (id, assignedTo) =>
    api.patch(`/api/admin/tickets/${id}/assign`, { assignedTo }),

  changePriority: (id, priority) =>
    api.patch(`/api/admin/tickets/${id}/priority`, { priority }),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Affiliates
// GET   /api/admin/affiliates?isActive=&page=&pageSize=
// PATCH /api/admin/affiliates/:id/approve         { isActive }
// PATCH /api/admin/affiliates/transactions/:id/pay
// ══════════════════════════════════════════════════════════════
export const adminAffiliateApi = {
  getList: (params = {}) =>
    api.get('/api/admin/affiliates', { params }),

  approve: (id, isActive) =>
    api.patch(`/api/admin/affiliates/${id}/approve`, { isActive }),

  payout: (transactionId) =>
    api.patch(`/api/admin/affiliates/transactions/${transactionId}/pay`),
  getTransactions: (affiliateId, params = {}) =>
  api.get(`/api/admin/affiliates/${affiliateId}/transactions`, { params }),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Media
// GET  /api/admin/media?templateId=&page=&pageSize=
// POST /api/admin/media/upload   FormData { file, templateId? }
// POST /api/admin/media/link     { url, originalName, storageType, externalId?, templateId? }
// PUT  /api/admin/media/templates/:templateId/set-download   { mediaFileId }
// DELETE /api/admin/media/:id
// ══════════════════════════════════════════════════════════════
export const adminMediaApi = {
  getList: (templateId = null, page = 1, pageSize = 20) => {
    const params = { page, pageSize };
    if (templateId) params.templateId = templateId;
    return api.get('/api/admin/media', { params });
  },

  upload: (file, templateId = null) => {
    const fd = new FormData();
    fd.append('file', file);
    if (templateId) fd.append('templateId', templateId);
    return api.post('/api/admin/media/upload', fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },

  link: (data) =>
    // data: { url, originalName, storageType, externalId?, templateId? }
    api.post('/api/admin/media/link', data),

  setDownload: (templateId, mediaFileId) =>
    api.put(`/api/admin/media/templates/${templateId}/set-download`, { mediaFileId }),

  delete: (id) =>
    api.delete(`/api/admin/media/${id}`),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Settings
// GET   /api/admin/settings?group=
// GET   /api/admin/settings/detail?group=
// POST  /api/admin/settings          { key, value, group, description }
// PUT   /api/admin/settings          [ { key, value } ]  (bulk)
// PATCH /api/admin/settings/:key     { value }
// ══════════════════════════════════════════════════════════════
export const adminSettingApi = {
  getAll: (group = null) =>
    api.get('/api/admin/settings', { params: group ? { group } : {} }),

  getDetail: (group = null) =>
    api.get('/api/admin/settings/detail', { params: group ? { group } : {} }),

  create: (data) =>
    // data: { key, value, group, description }
    api.post('/api/admin/settings', data),

  bulkUpdate: (items) =>
    // items: [{ key, value }, ...]
    api.put('/api/admin/settings', items),

  updateOne: (key, value) =>
    api.patch(`/api/admin/settings/${key}`, { value }),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — IP Blacklist
// GET    /api/admin/ip-blacklist?page=&pageSize=
// POST   /api/admin/ip-blacklist   { ipAddress, reason, expiredAt? }
// PATCH  /api/admin/ip-blacklist/:id/toggle
// DELETE /api/admin/ip-blacklist/:id
// ══════════════════════════════════════════════════════════════
export const adminIpBlacklistApi = {
  getList: (page = 1, pageSize = 20) =>
    api.get('/api/admin/ip-blacklist', { params: { page, pageSize } }),

  add: (ipAddress, reason, expiredAt = null) =>
    api.post('/api/admin/ip-blacklist', { ipAddress, reason, expiredAt }),

  toggle: (id) =>
    api.patch(`/api/admin/ip-blacklist/${id}/toggle`),

  delete: (id) =>
    api.delete(`/api/admin/ip-blacklist/${id}`),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Logs
// GET /api/admin/request-logs?ip=&userId=&endpoint=&statusCode=&page=&pageSize=
// GET /api/admin/email-logs?status=&template=&page=&pageSize=
// GET /api/admin/refresh-tokens?userId=&isActive=&page=&pageSize=
// ══════════════════════════════════════════════════════════════
export const adminLogApi = {
  getRequestLogs: (params = {}) =>
    api.get('/api/admin/request-logs', { params }),

  getEmailLogs: (params = {}) =>
    api.get('/api/admin/email-logs', { params }),

  getRefreshTokens: (params = {}) =>
    api.get('/api/admin/refresh-tokens', { params }),
    getAuditLogs: (params = {}) =>
    api.get('/api/admin/audit-logs', { params }),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Notifications (gửi thông báo realtime)
// POST /api/admin/notifications/send
//   { userId?, title, message, type?, url? }
//   userId = null → broadcast tất cả
// ══════════════════════════════════════════════════════════════
export const adminNotificationApi = {
  send: (data) =>
    // data: { userId?, title, message, type?, url? }
    api.post('/api/admin/notifications/send', data),
      getHistory: (params = {}) =>
    api.get('/api/admin/notifications', { params }),
};

// ══════════════════════════════════════════════════════════════
// ADMIN — Wishlists
// GET /api/admin/wishlists?page=&pageSize=
// GET /api/admin/wishlists/top?top=10
// ══════════════════════════════════════════════════════════════
export const adminWishlistApi = {
  getAll: (params = {}) =>
    api.get('/api/admin/wishlists', { params }),

  getTop: (top = 10) =>
    api.get('/api/admin/wishlists/top', { params: { top } }),
};