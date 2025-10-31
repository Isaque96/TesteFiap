using AdmSchoolApp.Domain.Interfaces;
using AdmSchoolApp.Infrastructure.Contexts;
using AdmSchoolApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdmSchoolApp.Infrastructure;

public static class InfrastructureServiceCollection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string? connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<AdmSchoolDbContext>(opt =>
        {
            opt.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(3);
            });
        });

        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
        
        return services;
    }
}