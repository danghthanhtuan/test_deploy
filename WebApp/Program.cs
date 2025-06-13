using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using WebApp.Configs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // địa chỉ Redis
});
builder.Services.AddHttpClient();

builder.Services.Configure<ApiConfigs>(builder.Configuration.GetSection("ApiConfigs"));

// thêm dịch vụ authentication
builder.Services.AddAuthentication(option =>
{
    option.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
    .AddCookie("User", option =>
    {
        option.LoginPath = "/Loginclient/Loginclient"; // Đường dẫn khi user chưa đăng nhập
    })


    .AddCookie("AdminCookie", options =>
    {
        options.LoginPath = "/admin/LoginAdmin/Login"; // Đường dẫn khi admin chưa đăng nhập
        options.AccessDeniedPath = "/admin/phanquyen/Index"; // Đường dẫn khi admin không có quyền truy cập
        options.Cookie.Name = "AdminAuthCookie"; // Tên của cookie dành cho admin
    });

// Thêm chính sách "AdminPolicy"
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("AdminCookie");
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
    });

    options.AddPolicy("HanhChinhPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("AdminCookie");
        policy.RequireAuthenticatedUser();
        policy.RequireRole("HanhChinh");
    });

    options.AddPolicy("KyThuatPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("AdminCookie");
        policy.RequireAuthenticatedUser();
        policy.RequireRole("KyThuat");
    });
    options.AddPolicy("DirectorPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("AdminCookie");
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Director");
    });
});


builder.Services.AddScoped<AuthorizeTokenAttribute>();

// Thêm dịch vụ sử dụng Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
       pattern: "{controller=HomeGuest}/{action=Index}/{id?}");



app.Run();
