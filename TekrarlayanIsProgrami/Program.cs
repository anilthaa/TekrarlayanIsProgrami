using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor.Markdown;
using System.ComponentModel;
using TekrarlayanIsProgrami.Components;
using TekrarlayanIsProgrami.Repository;
using TekrarlayanIsProgrami.Services;

var builder = WebApplication.CreateBuilder(args);
var jwtService= new JwtService(); //JwtServive nesnesi oluştur.
string token = jwtService.GenerateToken("Kullanici1"); //token oluştur.
Console.WriteLine("Oluşturulan Token:");//Console ekranında oluşturulan token i yazcak.
Console.WriteLine(token);

bool Valid=jwtService.ValidateToken(token); //Token i doğrula.
Console.WriteLine("\n Token geçerli mi?" +Valid);

var connectionString = builder.Configuration.GetConnectionString("PostgreSqlConnection"); // ConnectionString.

// Add services to the container.
builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application"); // scheme ismi önemli
builder.Services.AddRazorComponents()
     .AddInteractiveServerComponents();
builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7014/"); // API adresin
});


builder.Services.AddControllers();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddScoped<TooltipService>(); 
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<UserRepository>();//UserRrepository servisini çağırdık.
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<GorevLoglariRepository>();
builder.Services.AddScoped<SifreService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DataService>(serviceProvider =>
    new DataService(builder.Configuration.GetConnectionString("PostgreSqlConnection")));
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

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

