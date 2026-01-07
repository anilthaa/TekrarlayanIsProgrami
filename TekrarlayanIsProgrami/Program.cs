using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor.Markdown;
using System.ComponentModel;
using TekrarlayanIsProgrami.Components;
using TekrarlayanIsProgrami.Repository;
using TekrarlayanIsProgrami.Services;
var builder = WebApplication.CreateBuilder(args);
Console.OutputEncoding = System.Text.Encoding.UTF8;

builder.Services.AddSingleton<JwtService>();

var connectionString = builder.Configuration.GetConnectionString("PostgreSqlConnection"); 

builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application", options =>
    {
        options.LoginPath = "/GirisYap";
        options.AccessDeniedPath = "/AccessDenied";
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }
            context.Response.Redirect(options.LoginPath);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            }
            context.Response.Redirect(options.AccessDeniedPath);
            return Task.CompletedTask;
        };
    });
builder.Services.AddRazorComponents()
     .AddInteractiveServerComponents();
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});



builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddScoped<TooltipService>(); 
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<KullaniciTipRepository>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(serviceProvider =>
    serviceProvider.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddScoped<EPostaService>();
builder.Services.AddSingleton<UrlSigningService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<DataService>(serviceProvider =>
    new DataService(builder.Configuration.GetConnectionString("PostgreSqlConnection"),
    stateProvider:serviceProvider.GetRequiredService<AuthenticationStateProvider>()
    ));
builder.Services.AddControllers();





var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

