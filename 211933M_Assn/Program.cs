using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using _211933M_Assn.Models;
using _211933M_Assn;
using _211933M_Assn.Services;
using SendGrid.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<MyDbContext>();
//builder.Services.AddDefaultIdentity<Usere>(options => { options.SignIn.RequireConfirmedAccount = false; options.Password.RequiredLength = 12; }).AddEntityFrameworkStores<MyDbContext>().AddDefaultTokenProviders();
builder.Services.AddIdentity<User, IdentityRole>(options => { options.SignIn.RequireConfirmedAccount = false; options.Password.RequiredLength = 12; }).AddEntityFrameworkStores<MyDbContext>().AddDefaultTokenProviders();
builder.Services.AddAntiforgery(options => options.HeaderName = "XSRF-TOKEN");
builder.Services.AddSendGrid(options =>
    options.ApiKey = "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"
                     ?? throw new Exception("The 'SendGridApiKey' is not configured")
);
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);
builder.Services.AddControllersWithViews();

builder.Services.AddDataProtection();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache(); //save session in memory
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options
=>
{
	options.Cookie.Name = "MyCookieAuth";
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

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
