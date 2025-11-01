using AdmSchoolApp.Application.Services;
using AdmSchoolApp.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AdmSchoolApp.Application;

public static class ApplicationServiceCollection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<ITokenService, TokenService>();
        
        return services;
    }
}