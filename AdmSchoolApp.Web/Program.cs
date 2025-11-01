using AdmSchoolApp.Web.Providers;
using AdmSchoolApp.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Serviços
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAuthorizationCore();
builder.Services.AddProtectedBrowserStorage();

// HttpClient configurado para a API
var baseUrl = builder.Configuration["BackApi:BaseUrl"];
ArgumentException.ThrowIfNullOrEmpty(baseUrl);

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IApiService, ApiService>();

// Authentication
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(baseUrl)
});

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// importante: servir arquivos estáticos antes do routing/fallback
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();

// fallback para a página _Host (garante que o wrapper com <head> seja servido)
app.MapFallbackToPage("/_Host");

await app.RunAsync();