using Blazored.LocalStorage;
using DropSpace.FrontEnd;
using DropSpace.FrontEnd.Extensions;
using DropSpace.FrontEnd.HttpHandlers;
using DropSpace.FrontEnd.Services;
using DropSpace.FrontEnd.Utils;
using DropSpace.FrontEnd.Utils.ErrorHandlers;
using DropSpace.FrontEnd.Utils.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Refit;
using System.Net;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var cookieContainer = new CookieContainer();
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Configuration.AddJsonFile("./appsettings.json");
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddSingleton<EventTransmitter>();
builder.Services.AddScoped<AuthManager>();
builder.Services.AddScoped<TokenHttpHandler>();
builder.Services.AddScoped<CookieHttpHandler>();
builder.Services.AddScoped<IHubConnectionProvider, HubConnectionProvider>();
builder.Services.AddBlazorBootstrap();
builder.Services.AddRefitClient<IAuthService>()
    .WithConfiguration(builder.Configuration)
    .AddHttpMessageHandler<CookieHttpHandler>();
builder.Services.AddRefitClient<ISessionService>()
    .WithConfiguration(builder.Configuration)
    .AddHttpMessageHandler<TokenHttpHandler>();
ErrorHandler.Initialize(builder.Services.BuildServiceProvider());
var app = builder.Build();

await app.RunAsync();
