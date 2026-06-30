using Microsoft.EntityFrameworkCore;
using PraeferenzRoO.Application.Common.Interfaces;
using PraeferenzRoO.Domain.Common;
using PraeferenzRoO.Domain.Entities;

namespace PraeferenzRoO.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService tenantService)
        : base(options) => _tenantService = tenantService;

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<HsCode> HsCodes => Set<HsCode>();
    public DbSet<TradeAgreement> TradeAgreements => Set<TradeAgreement>();
    public DbSet<ProductRule> ProductRules => Set<ProductRule>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<FinishedProduct> FinishedProducts => Set<FinishedProduct>();
    public DbSet<ProductMaterial> ProductMaterials => Set<ProductMaterial>();
    public DbSet<OriginCalculation> OriginCalculations => Set<OriginCalculation>();
    public DbSet<OriginCalculationDetail> OriginCalculationDetails => Set<OriginCalculationDetail>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.Ignore<DomainEvent>();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.Entity<Country>().HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenantService.CurrentTenantId);
        modelBuilder.Entity<HsCode>().HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenantService.CurrentTenantId);
        modelBuilder.Entity<TradeAgreement>().HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenantService.CurrentTenantId);
        modelBuilder.Entity<ProductRule>().HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenantService.CurrentTenantId);
        modelBuilder.Entity<Material>().HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenantService.CurrentTenantId);
        modelBuilder.Entity<FinishedProduct>().HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenantService.CurrentTenantId);
        modelBuilder.Entity<ProductMaterial>().HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenantService.CurrentTenantId);
        modelBuilder.Entity<OriginCalculation>().HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenantService.CurrentTenantId);
        modelBuilder.Entity<OriginCalculationDetail>().HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenantService.CurrentTenantId);
        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenantService.CurrentTenantId);

        base.OnModelCreating(modelBuilder);
    }
}
