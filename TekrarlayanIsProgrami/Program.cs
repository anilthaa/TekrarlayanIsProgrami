using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor.Markdown;
using System.ComponentModel;
using TekrarlayanIsProgrami.Components;
using TekrarlayanIsProgrami.Repository;
using TekrarlayanIsProgrami.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
var builder = WebApplication.CreateBuilder(args);
Console.OutputEncoding = System.Text.Encoding.UTF8;
//var jwtService= new JwtService(); //JwtServive nesnesi oluştur.
//string token = jwtService.GenerateToken("Kullanici1"); //token oluştur.
//Console.WriteLine("Oluşturulan Token:");//Console ekranında oluşturulan token i yazcak.
//Console.WriteLine(token);

//bool Valid=jwtService.ValidateToken(token); //Token i doğrula.
//Console.WriteLine("\n Token geçerli mi?" +Valid);

builder.Services.AddSingleton<JwtService>();

var connectionString = builder.Configuration.GetConnectionString("PostgreSqlConnection"); // ConnectionString.

// Add services to the container.
builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application", options =>
    {
        options.LoginPath = "/GirisYap";
        options.AccessDeniedPath = "/AccessDenied";
        // Sadece API istekleri için 401 dön, Blazor sayfaları için normal davran
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }
            // Blazor için normal redirect yap ama ReturnUrl ekleme
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
builder.Services.AddScoped<UserRepository>();//UserRrepository servisini çağırdık.
builder.Services.AddScoped<KullaniciTipRepository>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(serviceProvider =>
    serviceProvider.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddScoped<GorevLoglariRepository>();
builder.Services.AddScoped<SifreService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EPostaService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<DataService>(serviceProvider =>
    new DataService(builder.Configuration.GetConnectionString("PostgreSqlConnection"),
    stateProvider:serviceProvider.GetRequiredService<AuthenticationStateProvider>()
    ));
builder.Services.AddControllers();





var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

