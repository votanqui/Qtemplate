QTemplate - Nền tảng bán mã nguồn (Source Code Marketplace)
📋 Tổng quan
QTemplate là một nền tảng thương mại điện tử chuyên bán các mã nguồn, template và theme được xây dựng trên nền tảng .NET 8 với kiến trúc Clean Architecture. Dự án cung cấp giải pháp hoàn chỉnh cho việc kinh doanh và phân phối sản phẩm số.

🎯 Tính năng chính
Quản lý sản phẩm: Upload, quản lý phiên bản, hình ảnh, mô tả cho các template

Hệ thống người dùng: Đăng ký, đăng nhập, xác thực email, phân quyền (Admin/User)

Giỏ hàng & Thanh toán: Tích hợp thanh toán qua nhiều cổng thanh toán

Mã giảm giá: Hệ thống coupon linh hoạt

Đánh giá & Bình luận: Cho phép người dùng đánh giá sản phẩm

Yêu thích: Tính năng wishlist cho người dùng

Thống kê & Báo cáo: Dashboard quản trị chi tiết

Hỗ trợ khách hàng: Hệ thống ticket hỗ trợ

AI Moderation: Tích hợp AI để kiểm duyệt nội dung

Middleware: IP Blacklist, Analytics, Request Logging

🏗️ Kiến trúc
Dự án được tổ chức theo Clean Architecture với 4 layers chính:

text
QTemplate.sln
├── Qtemplate (Presentation Layer) - Web API
├── Qtemplate.Application (Application Layer) - Use cases, DTOs, Features
├── Qtemplate.Domain (Domain Layer) - Entities, Enums, Interfaces
└── Qtemplate.Infrastructure (Infrastructure Layer) - Data, Repositories, Services
🧩 Công nghệ sử dụng
.NET 8 - Framework chính

Entity Framework Core 8 - ORM

SQL Server - Database

MediatR - CQRS Pattern

FluentValidation - Validation

AutoMapper - Object mapping

JWT Bearer - Authentication

BCrypt - Password hashing

MassTransit + RabbitMQ - Message bus

Swagger/OpenAPI - API documentation

AspNetCoreRateLimit - Rate limiting
