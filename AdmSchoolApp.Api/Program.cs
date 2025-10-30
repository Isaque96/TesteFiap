using System.Text;
using AdmSchoolApp.Endpoints.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddPolicy("Front",
    p => p.WithOrigins("https://localhost:7200", "http://localhost:5200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-change";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "your-issuer";
var audience = builder.Configuration["Jwt:Audience"] ?? "api";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("Front");
app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api");

var v1 = api.MapGroup("/v1").WithTags("v1");
v1.MapAuthEndpoints();

await app.RunAsync();