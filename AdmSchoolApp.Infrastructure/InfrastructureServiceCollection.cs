using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdmSchoolApp.Infrastructure;

public static class InfrastructureServiceCollection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string? connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

        /*services.AddDbContext<AdmSchoolDbContext>(opt =>
        {
            opt.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(3);
            });
        });*/
        
        return services;
    }
}