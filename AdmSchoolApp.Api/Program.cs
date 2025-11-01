using System.Text;
using AdmSchoolApp.Application;
using AdmSchoolApp.Application.Utils;
using AdmSchoolApp.Domain.Models;
using AdmSchoolApp.Endpoints.V1;
using AdmSchoolApp.Extensions;
using AdmSchoolApp.Infrastructure;
using AdmSchoolApp.Infrastructure.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using AppJsonContext = AdmSchoolApp.Domain.Models.Responses.AppJsonContext;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        theme: AnsiConsoleTheme.Code,
        outputTemplate: Util.SerilogTemplate
    )
    .WriteTo.File(
        path: "Logs/App-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: Util.SerilogTemplate
    )
    .CreateLogger();

var builder = WebApplication.CreateSlimBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: Util.SerilogTemplate);
});

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
    o.SerializerOptions.Converters.Add(new InternalCodesJsonConverter());
});
builder.Services.Configure<RouteOptions>(options =>
{
    options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddPolicy("Front",
    p => p.WithOrigins("https://localhost:7200", "http://localhost:5200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
));

var jwtSettings = builder.Configuration
                      .GetSection(JwtSettings.SectionName)
                      .Get<JwtSettings>()
                  ?? throw new InvalidOperationException($"Missing '{JwtSettings.SectionName}' configuration section.");

// Validações
if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
    throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long.");

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false; // Em produção: true
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

var cs = builder.Configuration.GetConnectionString("Default")
         ?? throw new InvalidOperationException("ConnectionStrings:Default is missing.");

builder.Services.AddInfrastructureServices(cs);
builder.Services.AddApplicationServices();

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("Default")!,
        name: "sqlserver",
        tags: ["db", "sql"]
    );

var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000} ms";
    options.EnrichDiagnosticContext = (diagnostic, http) =>
    {
        diagnostic.Set("ClientIP", http.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
        diagnostic.Set("UserAgent", http.Request.Headers.UserAgent.ToString());
        diagnostic.Set("CorrelationId", http.TraceIdentifier);
    };
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler("/error");

app.UseHttpsRedirection();
app.UseCors("Front");
app.UseAuthentication();
app.UseAuthorization();

app.MapErrorEndpoint();
app.MapHealthChecks("/health");
app.MapAppHealthChecks();

var api = app.MapGroup("/api").RequireAuthorization();

var v1 = api.MapGroup("/v1").WithTags("v1");
v1.MapAuthEndpoints();

await app.RunAsync();