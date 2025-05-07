using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebApi.Helper;
using WebApi.Models;
using WebApi.Service.Admin;
using WebApi.Service.Client;

var builder = WebApplication.CreateBuilder(args);

// Thêm dịch vụ Controller
builder.Services.AddControllers();

// Cấu hình Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Đăng ký DbContext
builder.Services.AddDbContext<ManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ManagementDbContext")),
    ServiceLifetime.Transient);

// đăng ký AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
//đăng ký redis để restart cũng không mất mã otp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // Redis chạy trên localhost
    options.InstanceName = "SampleInstance";

});
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));



//// đăng ký dịch vụ JWT
//builder.Services.AddAuthentication(option =>
//{
//    // Đặt mặc định schema xác thực là JwtBearer.Điều này đảm bảo mọi yêu cầu sẽ sử dụng JWT để xác thực.
//    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    // Đặt mặc định schema thách thức là JwtBearer. Điều này được sử dụng khi xác thực thất bại(ví dụ: token không hợp lệ hoặc không được cung cấp).Hệ thống sẽ thách thức client bằng cách trả về mã 401 Unauthorized.
//    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    // Đặt mặc định schema chính, áp dụng cho cả xác thực và thách thức.
//    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//}).AddJwtBearer(option =>
//{
//    // Cho phép API hoạt động qua HTTP(không yêu cầu HTTPS).Điều này hữu ích khi phát triển hoặc debug, nhưng bạn nên bật HTTPS trong môi trường sản xuất.
//    option.RequireHttpsMetadata = false;
//    // Lưu token đã xác thực vào HttpContext. Điều này có thể hữu ích nếu bạn cần sử dụng lại token trong quá trình xử lý.
//    option.SaveToken = true;
//    option.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
//        ValidAudience = builder.Configuration["JwtConfig:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"]!)),
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = false,
//        ValidateIssuerSigningKey = true,
//    };
//    option.Events = new JwtBearerEvents
//    {
//        OnAuthenticationFailed = context =>
//        {
//            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
//            {
//                context.Response.Headers.Add("Token-Expired", "true");
//            }
//            return Task.CompletedTask;
//        }
//    };


//});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
            ValidAudience = builder.Configuration["JwtConfig:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"]!)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true, // 🔥 Sửa thành true để kiểm tra token hết hạn
            ValidateIssuerSigningKey = true,
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                // Nếu token được gửi qua query string (cho WebSocket, SignalR)
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                if (!context.Response.Headers.ContainsKey("Token-Expired"))
                {
                    context.Response.Headers.Add("Token-Expired", "false");
                }
                await Task.CompletedTask;
            }
        };
    });



builder.Services.AddAuthorization(options =>
{
    // Chính sách cho Admin
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    // Chính sách cho User
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
});

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token in the input field."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// Đăng ký IHttpContextAccessor để tránh lỗi thiếu dependency
builder.Services.AddHttpContextAccessor();


// Đăng ký các dịch vụ
// Đăng ký dịch vụ cho IMapper
builder.Services.AddScoped<IMapper, Mapper>();
builder.Services.AddScoped<RegisterService>();
builder.Services.AddTransient<HomeService>();
builder.Services.AddTransient<JwtService>();
builder.Services.AddTransient<AccountService>();
builder.Services.AddTransient<CRegisterService>();
builder.Services.AddTransient<LoginService>();
builder.Services.AddTransient<RequirementService>();
builder.Services.AddTransient<RequestService>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<ContractService>();
builder.Services.AddTransient<RegulationsService>();
builder.Services.AddTransient<StaffService>();


builder.Services.AddTransient<IEmailService, EmailService>(); // Thay EmailService bằng class thực tế của bạn


//var app = builder.Build();

//// Cấu hình middleware
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();

//}

//app.UseHttpsRedirection();

//app.UseStaticFiles();
//app.UseRouting();

//app.UseAuthentication();
//app.UseMiddleware<JwtMiddleware>();

//app.UseAuthorization();


//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//});
//app.Run();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.UseSwaggerUI(c =>
    //{
    //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
    //    c.RoutePrefix = string.Empty; // Để Swagger UI mở ngay tại `/`
    //});
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();  // ✅ Xác thực trước middleware JWT
//app.UseMiddleware<TokenValidationMiddleware>(); // Đăng ký Middleware
app.UseAuthorization();   // ✅ Ủy quyền sau khi đã xác thực

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
