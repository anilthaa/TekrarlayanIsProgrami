using TekrarlayanIsProgrami.Components;
using Radzen;
using TekrarlayanIsProgrami.Services;
using Microsoft.AspNetCore.Components.Authorization;
using TekrarlayanIsProgrami.Repository;

var builder = WebApplication.CreateBuilder(args);
var jwtService= new JwtService(); //JwtServive nesnesi oluştur.
string token = jwtService.GenerateToken("User1"); //token oluştur.
Console.WriteLine("Oluşturulan Token:");//Console ekranında oluşturulan token i yazcak.
Console.WriteLine(token);

bool Valid=jwtService.ValidateToken(token); //Token i doğrula.
Console.WriteLine("\n Token geçerli mi?" +Valid);


// Add services to the container.
builder.Services.AddRazorComponents()
     .AddInteractiveServerComponents();

builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddScoped<TooltipService>(); 
builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
 builder.Services.AddScoped<UserRepository>();//UserRrepository servisini çağırdık.
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
