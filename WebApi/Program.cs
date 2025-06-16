using AutoMapper;
using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebApi.Helper;
using WebApi.Models;
using WebApi.Service.Admin;
using WebApi.Service.Client;
using WebApi.Service.Introduce;
using WebApi.Configs;

var builder = WebApplication.CreateBuilder(args);

// Thêm dịch vụ Controller
builder.Services.AddControllers();

// Cấu hình Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ApiConfigs>(builder.Configuration.GetSection("ApiConfigs"));

// Đăng ký DbContext
builder.Services.AddDbContext<ManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ManagementDbContext")),
    ServiceLifetime.Transient);

// đăng ký AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? config["OpenAI:ApiKey"];
    return new ChatbotService(apiKey);
});


//});
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

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

    // Chính sách cho bộ phận Hành chính
    options.AddPolicy("HanhChinhPolicy", policy => policy.RequireRole("HanhChinh"));

    // Chính sách cho bộ phận Kỹ thuật
    options.AddPolicy("KyThuatPolicy", policy => policy.RequireRole("KyThuat"));
    // Chính sách cho User
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
    options.AddPolicy("DirectorPolicy", policy => policy.RequireRole("Director"));

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
builder.Services.AddSignalR();
builder.Services.AddCors(options => {
    options.AddPolicy("CORSPolicy", builder => builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed((hosts) => true));
});

// Đăng ký IHttpContextAccessor để tránh lỗi thiếu dependency
builder.Services.AddHttpContextAccessor();


// Đăng ký các dịch vụ
builder.Services.AddScoped<IMapper, Mapper>();
builder.Services.AddScoped<RegisterService>();
builder.Services.AddTransient<HomeService>();
builder.Services.AddTransient<JwtService>();
builder.Services.AddTransient<CRegisterService>();
builder.Services.AddTransient<LoginService>();
builder.Services.AddTransient<RequirementService>();
builder.Services.AddTransient<RequestService>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<RegulationsService>();
builder.Services.AddTransient<AccountService>();
builder.Services.AddTransient<ContractsManagementService>();

builder.Services.AddTransient<StaffService>();
builder.Services.AddTransient<PdfService>();
builder.Services.AddTransient<SeeContract_SignService>();
builder.Services.AddTransient<PaymentService>();

builder.Services.AddTransient<TransactionService>();
builder.Services.AddTransient<ContactService>();
builder.Services.AddTransient<IAdminContactService, AdminContactService>();

builder.Services.AddTransient<INotificationService, NotificationService>();

builder.Services.AddTransient<ServiceGuest>();


//builder.Services.AddTransient<TextLocationStrategy>();

builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddTransient<IPaymentService, PaymentService>();

builder.Services.AddMemoryCache(); // Cho IMemoryCache

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("CORSPolicy");

app.UseAuthentication();  // ✅ Xác thực trước middleware JWT
//app.UseMiddleware<TokenValidationMiddleware>(); // Đăng ký Middleware
app.UseAuthorization();   // ✅ Ủy quyền sau khi đã xác thực

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    app.MapHub<NotificationHub>("/notificationHub");
});

app.Run();