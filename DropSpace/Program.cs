using DropSpace;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Services.AddOidcAuthentication(options =>
//{
//    options.ProviderOptions.ClientId = "test_mvc";
//    options.ProviderOptions.Authority = "https://localhost:7237/";
//    options.ProviderOptions.ResponseType = "code";

//    // Note: response_mode=fragment is the best option for a SPA. Unfortunately, the Blazor WASM
//    // authentication stack is impacted by a bug that prevents it from correctly extracting
//    // authorization error responses (e.g error=access_denied responses) from the URL fragment.
//    // For more information about this bug, visit https://github.com/dotnet/aspnetcore/issues/28344.
//    //
//    options.ProviderOptions.ResponseMode = "query";
//    options.AuthenticationPaths.RemoteRegisterPath = "https://localhost:7237/Identity/Account/Register";

//    // Add the "roles" (OpenIddictConstants.Scopes.Roles) scope and the "role" (OpenIddictConstants.Claims.Role) claim
//    // (the same ones used in the Startup class of the Server) in order for the roles to be validated.
//    // See the Counter component for an example of how to use the Authorize attribute with roles
//    options.ProviderOptions.DefaultScopes.Add("roles");
//    options.UserOptions.RoleClaim = "role";
//});
builder.Services.AddHostedService<DataSeedService>();
builder.Services.AddDbContext<ApplicationContext>(
    options =>
    {
        options.UseSqlite("Data Source=DbContext.db");
        options.UseOpenIddict();
    }
);
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Home", configure => configure.RequireClaim("scope", "home_api").Build());
});
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationContext>();
    })
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("connect/authorize")
                       .SetLogoutEndpointUris("connect/logout")
                       .SetTokenEndpointUris("connect/token")
                       .SetUserinfoEndpointUris("connect/userinfo");

        options.AllowClientCredentialsFlow().AllowAuthorizationCodeFlow().AllowRefreshTokenFlow();

        //options.AddDevelopmentEncryptionCertificate();

        options.DisableAccessTokenEncryption();

        options.AddDevelopmentSigningCertificate()
        .AddEphemeralEncryptionKey();

        options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableLogoutEndpointPassthrough()
                       .EnableStatusCodePagesIntegration()
                       .EnableTokenEndpointPassthrough();

        options.RegisterScopes("home_api");

    }).AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    }).AddClient(options =>
    {
        // Note: this sample uses the code flow, but you can enable the other flows if necessary.
        options.AllowAuthorizationCodeFlow()
        .AllowClientCredentialsFlow()
        .AllowRefreshTokenFlow();

        // Register the signing and encryption credentials used to protect
        // sensitive data like the state tokens produced by OpenIddict.
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
                       .EnableStatusCodePagesIntegration()
                       .EnableRedirectionEndpointPassthrough();

        options.UseSystemNetHttp();
    })
;
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


app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
