using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Qtemplate.Application;
using Qtemplate.Infrastructure;
using Qtemplate.Infrastructure.Hubs;
using Qtemplate.Infrastructure.Services.AuditLog;
using Qtemplate.Middleware;
using Qtemplate.Middleware.Analyticc;
using Qtemplate.Middleware.RequestLogs;

var builder = WebApplication.CreateBuilder(args);


ThreadPool.SetMinThreads(200, 200);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();


builder.Services.AddMemoryCache();


builder.Services.AddResponseCompression(opts =>
{
    opts.EnableForHttps = true;
    opts.Providers.Add<BrotliCompressionProvider>();
    opts.Providers.Add<GzipCompressionProvider>();
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "application/problem+json",
    });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o =>
    o.Level = CompressionLevel.Fastest); 
builder.Services.Configure<GzipCompressionProviderOptions>(o =>
    o.Level = CompressionLevel.Fastest);

builder.Services.AddOutputCache(opts =>
{
    opts.AddPolicy("templates", b => b
        .Expire(TimeSpan.FromSeconds(30))
        .SetVaryByQuery("page", "pageSize", "search", "categorySlug", "tagSlug",
                        "isFree", "minPrice", "maxPrice", "sortBy",
                        "onSale", "isFeatured", "isNew", "techStack")
        .Tag("templates")); 

    opts.AddPolicy("template-detail", b => b
        .Expire(TimeSpan.FromSeconds(60))
        .SetVaryByRouteValue("slug")
        .Tag("templates"));

    opts.AddPolicy("categories", b => b
        .Expire(TimeSpan.FromSeconds(300))
        .Tag("categories"));
});

builder.Services.AddSignalR();

builder.Services.AddSingleton<RequestLogQueue>();
builder.Services.AddHostedService<RequestLogDrainService>();

builder.Services.AddSingleton<AnalyticsQueue>();
builder.Services.AddHostedService<AnalyticsDrainService>();

builder.Services.AddSingleton<AuditLogQueue>();
builder.Services.AddHostedService<AuditLogDrainService>();

builder.Services.Configure<IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.QuotaExceededResponse = new QuotaExceededResponse
    {
        Content = "{{ \"success\": false, \"message\": \"Bạn đã gửi quá nhiều yêu cầu, vui lòng thử lại sau {1} giây.\" }}",
        ContentType = "application/json",
        StatusCode = 429
    };
});


var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey chưa được cấu hình");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer chưa được cấu hình");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience chưa được cấu hình");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("accessToken"))
                    context.Token = context.Request.Cookies["accessToken"];

                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken)
                    && path.StartsWithSegments("/hubs/notifications"))
                    context.Token = accessToken;

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(
                    "{\"success\":false,\"message\":\"Bạn chưa đăng nhập hoặc token không hợp lệ.\"}");
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(
                    "{\"success\":false,\"message\":\"Bạn không có quyền truy cập tài nguyên này.\"}");
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireCustomer", policy => policy.RequireRole("Customer"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseResponseCompression();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseStaticFiles();

app.UseAnalyticsTracking();
app.UseIpBlacklist();
app.UseRequestLogging();
app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();


app.UseOutputCache();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();