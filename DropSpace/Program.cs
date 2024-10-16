using DropSpace.Domain;
using DropSpace.Infrastructure;
using DropSpace.Infrastructure.Stores;
using DropSpace.Infrastructure.Stores.Interfaces;
using DropSpace.Logic.Events;
using DropSpace.Logic.Events.Events;
using DropSpace.Logic.Events.Handlers;
using DropSpace.Logic.Events.Interfaces;
using DropSpace.Logic.Files;
using DropSpace.Logic.Files.Interfaces;
using DropSpace.Logic.Jobs;
using DropSpace.Logic.Services;
using DropSpace.Logic.Services.Interfaces;
using DropSpace.Logic.Utils.Converters;
using DropSpace.Logic.Utils.Converters.Interfaces;
using DropSpace.WebApi.Controllers.Filters.MemberFilter;
using DropSpace.WebApi.Controllers.Filters.MemberFilter.Providers;
using DropSpace.WebApi.RPCServices;
using DropSpace.WebApi.SignalRHubs;
using DropSpace.WebApi.Utils.Cashe;
using DropSpace.WebApi.Utils.Interfaces;
using DropSpace.WebApi.Utils.JWTs;
using DropSpace.WebApi.Utils.Requirements;
using DropSpace.WebApi.Utils.RSAKeyProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Quartz;
using System.Security.Claims;
using Uploads;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<JWTFactory>();
builder.Services.AddHostedService<DataSeed>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddDbContext<ApplicationContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("Debug")));
builder.Services.AddScoped<IAuthorizationHandler, MemberRequirementAuthorizationHandler>();
builder.Services.AddScoped<IFileFlowCoordinator, FileFlowCoordinator>();
builder.Services.AddSingleton<IFileVault, FileVault>();
builder.Services.AddSingleton<IInviteCodeStore, CasheInviteCodeStore>();
builder.Services.AddScoped<ISessionStore, SessionStore>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFileStore, FileStore>();
builder.Services.AddScoped<IUploadStateStore, UploadStateStore>();
builder.Services.AddSingleton<IConnectionIdStore, CasheConnectionIdStore>();
builder.Services.AddSingleton<IEventTransmitter, EventTransmitter>();
builder.Services.AddScoped<IEventHandler<UserJoinedEvent>, UserJoinedEventHandler>();
builder.Services.AddScoped<IEventHandler<UserLeftEvent>, UserLeftEventHandler>();
builder.Services.AddScoped<IEventHandler<FileUpdatedEvent>, FileUpdatedEventHandler>();
builder.Services.AddScoped<IEventHandler<FileDeletedEvent>, FileDeletedEventHandler>();
builder.Services.AddScoped<IEventHandler<SessionExpiredEvent>, SessionExpiredEventHandler>();
builder.Services.AddSingleton(typeof(ISeparetedCache<>), typeof(SeparetedCashe<>));
builder.Services.AddSingleton<IRSAKeyProvider, RSAFromFileKeyProvider>();
builder.Services.AddScoped<IFileConverter, FileConverter>();
builder.Services.RegisterSessionIdProviders();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        b =>
        {
            b.WithOrigins(builder.Configuration.GetValue<string>("ClientAddress")!)
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials()
                       .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
        });
});

builder.Services.AddQuartz(q =>
{
    q.AddJob<DeleteExpiredSessionsJob>(opts =>
    opts.WithIdentity("DeleteExpiredSessions", "Expired")
        .RequestRecovery());

    q.AddJob<DeleteDeadUploadsJob>(opts =>
    opts.WithIdentity("DeleteDead", "Expired")
        .RequestRecovery());

    q.AddTrigger(trigger =>
    {
        trigger.ForJob("DeleteDead", "Expired")
        .WithSimpleSchedule(shedule =>
        {
            shedule
            .WithIntervalInSeconds(15)
            .RepeatForever();
        })
        .StartNow();
    });

    q.AddTrigger(trigger =>
    {
        trigger.ForJob("DeleteExpiredSessions", "Expired")
        .WithSimpleSchedule(shedule =>
        {
            shedule
            .WithIntervalInSeconds(60)
            .RepeatForever();
        })
        .StartNow();
    });
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "������� ����� � �������: Bearer {�����}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});


builder.Services.AddIdentity<IdentityUser, UserPlanRole>(options =>
{
    options.Lockout.AllowedForNewUsers = false;
})
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationContext>();

builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(builder.Configuration["KeysFolder"]!))
            .SetApplicationName("DropSpace");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        IssuerSigningKey = new RSAFromFileKeyProvider(builder.Configuration).GetOrCreateKey()
    };

    options.Events = new JwtBearerEvents()
    {
        OnMessageReceived = (context) =>
        {
            var path = context.HttpContext.Request.Path;
            if (path.Value.Contains("/hubs/"))
            {
                var accessToken = context.Request.Query["access_token"];

                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
}).AddJwtBearer("refresh", options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        IssuerSigningKey = new RSAFromFileKeyProvider(builder.Configuration).GetOrCreateKey()
    };

    options.Events = new JwtBearerEvents()
    {
        OnMessageReceived = (ctx) =>
        {
            var dataProtectionProvider = ctx.HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>();
            var protector = dataProtectionProvider.CreateProtector("refresh");

            if (!ctx.Request.Cookies.ContainsKey("refreshToken"))
                ctx.Fail(new NullReferenceException("����� �� �����!"));
            else
            {
                ctx.Token = protector.Unprotect(ctx.Request.Cookies["refreshToken"]);
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddMemoryCache(options =>
{
    options.TrackStatistics = true;
});

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim(ClaimTypes.NameIdentifier)
        .RequireClaim(ClaimTypes.Role)
        .RequireClaim("maxSessions")
        .RequireClaim("sessionDuration")
        .RequireClaim("maxFilesSize")
        .Build())
    .AddPolicy("refresh", pb =>
    {
        pb.AddAuthenticationSchemes("refresh");
        pb.RequireClaim("type", "refresh");
    });

builder.Services.AddSignalR(options => options.EnableDetailedErrors = true);

builder.Services.AddGrpc(options => {
    options.MaxReceiveMessageSize = 20 * 1024 * 1024;
    options.MaxSendMessageSize = 20 * 1024 * 1024;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DropSpace");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAllOrigins");

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseGrpcWeb();

app.MapHub<SessionsHub>("/hubs/Sessions");

app.MapGrpcService<UploadService>().EnableGrpcWeb();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
