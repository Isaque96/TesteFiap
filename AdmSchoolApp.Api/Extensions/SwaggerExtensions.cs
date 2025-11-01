using System.Globalization;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AdmSchoolApp.Extensions;

public static class SwaggerExtensions
{
    public const string JsonContentType = "application/json";
    
    public static IServiceCollection AddSwaggerFullConfig(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // Documento único (v1) – você já agrupa via /api/v1
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "FIAP Admin School API",
                Version = "v1",
                Description = "API RESTful administrativa para gestão de Alunos, Turmas e Matrículas. Autenticação via JWT (role Admin).",
                Contact = new OpenApiContact { Name = "Equipe", Email = "contato@fiap.com.br" }
            });

            // Segurança Bearer
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Informe o token JWT no formato: Bearer {seu_token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };
            options.AddSecurityDefinition("Bearer", securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });

            // XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

            // Filtros para respostas padrão e ProblemDetails
            options.OperationFilter<DefaultResponsesOperationFilter>();
            options.OperationFilter<ProblemDetailsResponseOperationFilter>();
            options.SchemaFilter<RequiredNullableSchemaFilter>();

            // Tag por recurso (para Minimal APIs usa-se normalmente o primeiro segmento após /api/v1)
            options.TagActionsBy(api =>
            {
                // /api/v1/alunos -> tag "alunos"; /api/v1/auth -> "auth"
                var segments = api.RelativePath?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
                // Espera-se ["api","v1","alunos", ...]
                var tag = segments.Length >= 3 ? segments[2] : "misc";
                return [CultureInfo.InvariantCulture.TextInfo.ToTitleCase(tag)];
            });
            options.DocInclusionPredicate((_, _) => true);
        });
        
        return services;
    }
}

public class RequiredNullableSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null) return;

        var props = context.Type.GetProperties();
        foreach (var prop in props)
        {
            var propName = schema.Properties.Keys
                .FirstOrDefault(k => string.Equals(k, prop.Name, StringComparison.OrdinalIgnoreCase));
            if (propName is null) continue;

            var isNullable = Nullable.GetUnderlyingType(prop.PropertyType) != null;
            if (!isNullable)
                schema.Required.Add(propName);
        }
    }
}

public class DefaultResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // 401/403/500 padrão em todas as rotas protegidas
        if (!operation.Responses.ContainsKey("401"))
            operation.Responses.Add("401", new OpenApiResponse
            {
                Description = "Unauthorized - Token ausente ou inválido",
                Content = ProblemContent()
            });

        if (!operation.Responses.ContainsKey("403"))
            operation.Responses.Add("403", new OpenApiResponse
            {
                Description = "Forbidden - Requer role Admin",
                Content = ProblemContent()
            });

        if (!operation.Responses.ContainsKey("500"))
            operation.Responses.Add("500", new OpenApiResponse
            {
                Description = "Internal Server Error",
                Content = ProblemContent()
            });
    }

    private static IDictionary<string, OpenApiMediaType> ProblemContent()
    {
        return new Dictionary<string, OpenApiMediaType>
        {
            ["application/problem+json"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "ProblemDetails"
                    }
                }
            }
        };
    }
}

public class ProblemDetailsResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.ApiDescription.HttpMethod?.ToUpperInvariant();
        if (method is not ("POST" or "PUT" or "PATCH")) return;
        if (!operation.Responses.ContainsKey("400"))
            operation.Responses.Add("400", new OpenApiResponse
            {
                Description = "Bad Request - Erros de validação de domínio ou de entrada",
                Content = ProblemContent()
            });

        if (!operation.Responses.ContainsKey("409"))
            operation.Responses.Add("409", new OpenApiResponse
            {
                Description = "Conflict - Regras de unicidade (CPF/Email, matrícula duplicada, etc.)",
                Content = ProblemContent()
            });

        if (!operation.Responses.ContainsKey("422"))
            operation.Responses.Add("422", new OpenApiResponse
            {
                Description = "Unprocessable Entity - Erros de validação de formato",
                Content = ProblemContent()
            });
    }

    private static IDictionary<string, OpenApiMediaType> ProblemContent()
    {
        return new Dictionary<string, OpenApiMediaType>
        {
            ["application/problem+json"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "ProblemDetails"
                    }
                }
            }
        };
    }
}