using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PraeferenzRoO.Persistence;

/// <summary>
/// Registers Persistence layer services with the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Persistence layer services: EF Core DbContext, Dapper connection factory, repositories.
    /// </summary>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }
}
