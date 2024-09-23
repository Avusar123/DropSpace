using DropSpace;
using DropSpace.DataManagers;
using DropSpace.Events;
using DropSpace.Events.Events;
using DropSpace.Events.Handlers;
using DropSpace.Events.Interfaces;
using DropSpace.Files;
using DropSpace.Files.Interfaces;
using DropSpace.Jobs;
using DropSpace.Models.Data;
using DropSpace.Providers;
using DropSpace.Requirements;
using DropSpace.Services;
using DropSpace.Services.Interfaces;
using DropSpace.SignalRHubs;
using DropSpace.Stores;
using DropSpace.Stores.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ClaimsFactory>();
builder.Services.AddHostedService<DataSeed>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddDbContext<ApplicationContext>(options => options.UseInMemoryDatabase("InMemory"));
builder.Services.AddScoped<IAuthorizationHandler, MemberRequirementAuthorizationHandler>();
builder.Services.AddScoped<IFileFlowCoordinator, FileFlowCoordinator>();
builder.Services.AddSingleton<IFileVault, FileVault>();
builder.Services.AddSingleton<IInviteCodeStore, InMemoryInviteCodeStore>();
builder.Services.AddScoped<ISessionStore, SessionStore>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFileStore, FileStore>();
builder.Services.AddScoped<IPendingUploadStore, PendingUploadStore>();
builder.Services.AddSingleton<IConnectionIdStore, InMemoryConnectionIdStore>();
builder.Services.AddSingleton<IEventTransmitter, EventTransmitter>();
builder.Services.AddScoped<IEventHandler<UserJoinedEvent>, UserJoinedEventHandler>();
builder.Services.AddScoped<IEventHandler<FileListChangedEvent>, FileListChangedEventHandler>();
builder.Services.AddScoped<IEventHandler<UserLeftEvent>, UserLeftEventHandler>();
builder.Services.AddScoped<IEventHandler<SessionExpiredEvent>, SessionExpiredEventHandler>();
builder.Services.AddScoped<IEventHandler<NewChunkUploadedEvent>, NewChunkUploadedEventHandler>();

builder.Services.AddQuartz(q =>
{
    q.AddJob<DeleteExpiredSessionsJob>(opts =>
    opts.WithIdentity("DeleteExpired", "Session")
        .RequestRecovery());

    q.AddTrigger(trigger =>
    {
        trigger.ForJob("DeleteExpired", "Session")
        .WithSimpleSchedule(shedule =>
        {
            shedule
            .WithIntervalInSeconds(60)
            .WithMisfireHandlingInstructionNextWithRemainingCount()
            .RepeatForever();
        })
        .StartNow();
    });

    q.AddJob<DeleteTimeoutUploadsJob>(opts =>
    opts.WithIdentity("DeleteUpload", "Session")
        .RequestRecovery());

    q.AddTrigger(trigger =>
    {
        trigger.ForJob("DeleteUpload", "Session")
        .WithSimpleSchedule(shedule =>
        {
            shedule
            .WithIntervalInSeconds(15)
            .WithMisfireHandlingInstructionNextWithRemainingCount()
            .RepeatForever();
        })
        .StartNow();
    });

    q.AddJob<DeleteFilesWithoutReferencesJob>(opts =>
    opts.WithIdentity("DeleteFiles", "Files")
        .RequestRecovery());

    q.AddTrigger(trigger =>
    {
        trigger.ForJob("DeleteFiles", "Files")
        .WithSimpleSchedule(shedule =>
        {
            shedule
            .WithIntervalInSeconds(60)
            .WithMisfireHandlingInstructionNextWithRemainingCount()
            .RepeatForever();
        })
        .StartNow();
    });
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

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
        options.Window = TimeSpan.FromSeconds(15);
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
