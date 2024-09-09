using DropSpace;
using DropSpace.DataManagers;
using DropSpace.Models.Data;
using DropSpace.Providers;
using DropSpace.Requirements;
using DropSpace.Services;
using DropSpace.SignalRHubs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ClaimsFactory>();

builder.Services.AddHostedService<DataSeed>();

builder.Services.AddScoped<SessionService>();

builder.Services.AddDbContext<ApplicationContext>(options => options.UseInMemoryDatabase("InMemory"));

builder.Services.AddScoped<IAuthorizationHandler, MemberRequirementAuthorizationHandler>();

builder.Services.AddSingleton<IInviteCodeProvider, InMemoryInviteCodeProvider>();

builder.Services.AddIdentity<IdentityUser, UserPlanRole>(options =>
{
    options.Lockout.AllowedForNewUsers = false;
})
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationContext>();

builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys\"))
            .SetApplicationName("DropSpace");

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ForwardSignIn = CookieAuthenticationDefaults.AuthenticationScheme;
});

builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 8;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueLimit = 2;
    }));


builder.Services.AddAuthentication(
        options =>
        {
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }
    )
    .AddScheme<CookieAuthenticationOptions, ChooseAuthenticationHandler>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
    });


builder.Services.AddSignalR();

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim(ClaimTypes.NameIdentifier)
        .RequireClaim(ClaimTypes.Role)
        .RequireClaim("maxSessions")
        .RequireClaim("sessionDuration")
        .RequireClaim("maxFilesSize")
        .Build();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();

app.MapHub<SessionsHub>("/Session/Subscribe");

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}").RequireRateLimiting("fixed");

app.Run();
