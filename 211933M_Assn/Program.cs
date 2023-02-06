using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using _211933M_Assn.Models;
using _211933M_Assn;
using _211933M_Assn.Services;
using SendGrid.Extensions.DependencyInjection;
using AspNetCore.ReCaptcha;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;

var builder = WebApplication.CreateBuilder(args);
var provider =builder.Services.BuildServiceProvider();
var configuration = provider.GetRequiredService<IConfiguration>();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<MyDbContext>();
//builder.Services.AddDefaultIdentity<Usere>(options => { options.SignIn.RequireConfirmedAccount = false; options.Password.RequiredLength = 12; }).AddEntityFrameworkStores<MyDbContext>().AddDefaultTokenProviders();
builder.Services.AddIdentity<User, IdentityRole>(options => { 
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Password.RequiredLength = 12;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(30);
})
    .AddEntityFrameworkStores<MyDbContext>().AddDefaultTokenProviders();
builder.Services.AddAntiforgery(options => options.HeaderName = "XSRF-TOKEN");
builder.Services.AddSendGrid(options =>
    options.ApiKey = configuration["SendGrid:Key"]
                     ?? throw new Exception("The 'SendGridApiKey' is not configured")
);
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);
builder.Services.AddControllersWithViews();

//builder.Services.Configure<GoogleCaptcha>(builder.Configuration.GetSection("ReCaptcha"));
builder.Services.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"));

builder.Services.AddDataProtection();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache(); //save session in memory
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options=>
{
options.Cookie.Name = "MyCookieAuth";
}).AddGoogle(googleOptions =>
{
    googleOptions.ClientId = configuration["Google:ClientId"];
    googleOptions.ClientSecret = configuration["Google:ClientSecret"];
    googleOptions.ClaimActions.MapJsonKey("image", "picture", "url");
    googleOptions.Scope.Add("https://www.googleapis.com/auth/user.birthday.read");
    googleOptions.SaveTokens = true;
});

builder.Services.ConfigureApplicationCookie(options =>{
    options.LoginPath= "/login";
    options.LogoutPath= "/logout";
    options.ExpireTimeSpan= TimeSpan.FromMinutes(10);
    options.SlidingExpiration= true;
});

builder.Services.AddScoped<EncodingService>();
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddDataProtection();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithRedirects("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.Lax
});

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.MapRazorPages();

app.Run();
