using AspNetCore.ReCaptcha;
using FreshFarmMarket.Data;
using FreshFarmMarket.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FreshFarmMarket.Helpers;
using Microsoft.AspNetCore.Identity.UI.Services;
using FreshFarmMarket.Middleware;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configure Forwarded Headers (For Reverse Proxy & Load Balancers)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Register the default processing strategy
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// ✅ Register DbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthConnectionString")));

// ✅ Register Identity with Enhanced Security
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.SignIn.RequireConfirmedAccount = true;

    // ✅ Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = true; // ✅ Ensure email confirmation
});

// ✅ Configure Authentication & Cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// ✅ Register Google ReCaptcha
builder.Services.AddReCaptcha(builder.Configuration.GetSection("GoogleReCaptcha"));

// ✅ Enable CSRF Protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

// ✅ Add Razor Pages with AntiForgery Protection
builder.Services.AddRazorPages(options =>
{
    options.Conventions.ConfigureFilter(new AutoValidateAntiforgeryTokenAttribute());
});

// ✅ Register QR Code Generator
builder.Services.AddSingleton<QRCoder.QRCodeGenerator>();

// ✅ Configure Session Middleware
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// ✅ Load SMTP settings from appsettings.json
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// ✅ Register EmailSender Service
builder.Services.AddTransient<IEmailSender, EmailSender>();
EncryptionHelper.Configure(builder.Configuration);

// ✅ Add Memory Cache for Rate Limiting
builder.Services.AddMemoryCache();

// ✅ Configure IP Rate Limiting
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*:/login",
            Limit = 5,
            Period = "1m" // Max 5 login attempts per minute
        }
    };
});
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

var app = builder.Build();

// ✅ Use Forwarded Headers
app.UseForwardedHeaders();

// ✅ Configure Error Handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Errors/500"); // ✅ Redirect to custom error page
    app.UseHsts();
}

// ✅ Secure Response Headers (Prevent XSS, Clickjacking)
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self' https://www.google.com/recaptcha/; style-src 'self' 'unsafe-inline'; img-src 'self' data:;";
    await next();
});

// ✅ Middleware Pipeline Configuration
app.UseHttpsRedirection();
app.UseMiddleware<SessionMiddleware>();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery(); // ✅ Ensure CSRF Protection is Enabled

// ✅ Handle Custom Error Pages
app.UseStatusCodePagesWithReExecute("/Errors/{0}");

// ✅ Use IP Rate Limiting Middleware
app.UseIpRateLimiting();

app.MapRazorPages();

app.Run();
