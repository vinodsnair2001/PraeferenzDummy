using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PraeferenzRoO.Application.Common.Interfaces;
using PraeferenzRoO.Persistence.Context;
using PraeferenzRoO.Persistence.Interceptors;

namespace PraeferenzRoO.Persistence;

// NOTE: The app_user PostgreSQL role must NOT have DELETE privilege on any table.
// This is enforced at the database level in the seeding scripts (T23).
public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention()
                   .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
        });

        services.AddScoped<IDapperContext, DapperContext>();

        return services;
    }
}
