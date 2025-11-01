using System.Reflection;
using AdmSchoolApp.Application.Services;
using AdmSchoolApp.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AdmSchoolApp.Application;

public static class ApplicationServiceCollection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidators();
        services.AddTransient<ITokenService, TokenService>();
        services.AddScoped<StudentService>();
        services.AddScoped<ClassService>();
        services.AddScoped<EnrollmentService>();
        services.AddScoped<UserService>();
        
        return services;
    }

    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        var domainAssembly = Assembly.Load("AdmSchoolApp.Application");
        services.AddValidatorsFromAssembly(domainAssembly);

        return services;
    }
}