using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PraeferenzRoO.Infrastructure;

/// <summary>
/// Registers Infrastructure layer services with the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services: email, file storage, background jobs.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }
}
