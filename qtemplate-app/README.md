# Qtemplate – Frontend

React + Vite + Tailwind CSS frontend cho Qtemplate API.

## Cài đặt

```bash
npm install
cp .env.example .env
# Sửa VITE_API_URL trong .env
npm run dev
```

## Cấu trúc

```
src/
├── api/
│   ├── client.js        # Axios instance + interceptors (auto refresh token)
│   └── services.js      # Tất cả API calls (auth, user, templates, orders, tickets, affiliate, public)
├── components/
│   ├── auth/            # ProtectedRoute
│   ├── layout/          # Layout + Sidebar
│   └── ui/              # Spinner, Alert, Modal, Badge, Pagination, StarRating...
├── context/
│   └── AuthContext.jsx  # Auth state + login/register/logout
└── pages/
    ├── LoginPage.jsx
    ├── RegisterPage.jsx
    ├── AuthPages.jsx        # ForgotPassword, ResetPassword, VerifyEmail
    ├── TemplatesPage.jsx    # Danh sách + lọc/tìm kiếm
    ├── TemplateDetailPage.jsx # Chi tiết + mua + wishlist + review
    ├── ProfilePage.jsx      # Hồ sơ + avatar
    ├── SecurityPage.jsx     # Đổi mật khẩu + xóa tài khoản
    ├── PurchasePages.jsx    # Purchases + Downloads + Wishlist
    ├── NotificationsPage.jsx
    ├── ReviewsPage.jsx
    ├── TicketPages.jsx      # List + Detail + Create + Reply
    ├── AffiliatePage.jsx
    └── OrderDetailPage.jsx  # Chi tiết đơn + thanh toán QR + polling

```

## Tính năng đầy đủ

### Auth
- ✅ Đăng nhập / Đăng ký
- ✅ Quên mật khẩu / Đặt lại mật khẩu
- ✅ Xác minh email / Gửi lại email xác minh
- ✅ Auto refresh token (Axios interceptor)
- ✅ Logout

### User
- ✅ Xem và cập nhật hồ sơ
- ✅ Upload avatar (multipart/form-data)
- ✅ Đổi mật khẩu
- ✅ Xóa tài khoản

### Templates
- ✅ Danh sách với lọc/tìm/phân trang
- ✅ Chi tiết template
- ✅ Toggle wishlist
- ✅ Download (external + stream)
- ✅ Preview iframe
- ✅ Reviews (đọc + tạo + sửa + xóa)

### Orders
- ✅ Tạo đơn hàng
- ✅ Apply coupon
- ✅ Thanh toán chuyển khoản (QR code)
- ✅ Polling trạng thái thanh toán
- ✅ Hủy đơn

### Tickets
- ✅ Danh sách và chi tiết ticket
- ✅ Tạo ticket mới
- ✅ Reply ticket

### Affiliate
- ✅ Đăng ký affiliate
- ✅ Xem stats và lịch sử hoa hồng
- ✅ Copy link affiliate

### Notifications
- ✅ Danh sách thông báo
- ✅ Đánh dấu đã đọc (1 hoặc tất cả)
- ✅ Filter chưa đọc
