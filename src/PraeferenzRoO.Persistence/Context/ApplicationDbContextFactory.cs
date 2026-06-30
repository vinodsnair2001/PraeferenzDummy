using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PraeferenzRoO.Application.Common.Interfaces;

namespace PraeferenzRoO.Persistence.Context;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=praeferenz_dev;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention()
            .Options;

        return new ApplicationDbContext(options, new DesignTimeTenantService());
    }

    private sealed class DesignTimeTenantService : ITenantService
    {
        public Guid CurrentTenantId => Guid.Empty;
        public void SetCurrentTenant(Guid tenantId) { }
    }
}
