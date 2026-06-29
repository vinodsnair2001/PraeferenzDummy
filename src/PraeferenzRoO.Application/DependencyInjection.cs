using Microsoft.Extensions.DependencyInjection;

namespace PraeferenzRoO.Application;

/// <summary>
/// Registers Application layer services with the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application layer services: MediatR, FluentValidation, AutoMapper.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
