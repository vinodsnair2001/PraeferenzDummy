using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using PraeferenzRoO.Application.Common.Interfaces;
using PraeferenzRoO.Domain.Entities;
using PraeferenzRoO.Persistence.Context;
using PraeferenzRoO.Persistence.Interceptors;

namespace PraeferenzRoO.Persistence.Tests.Interceptors;

public sealed class AuditableEntityInterceptorTests
{
    private static ApplicationDbContext BuildContext(
        AuditableEntityInterceptor interceptor,
        ITenantService tenantService)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return new ApplicationDbContext(options, tenantService);
    }

    [Fact]
    public async Task SavingChangesAsync_OnAdded_SetsCreatedByAndCreatedDate()
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(u => u.UserId).Returns("test-user");

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(h => h.HttpContext).Returns((HttpContext?)null);

        var tenantId = Guid.NewGuid();
        var tenantService = new Mock<ITenantService>();
        tenantService.Setup(t => t.CurrentTenantId).Returns(tenantId);

        var interceptor = new AuditableEntityInterceptor(currentUser.Object, httpContextAccessor.Object);
        await using var context = BuildContext(interceptor, tenantService.Object);

        var country = new Country
        {
            TenantId = tenantId,
            Name = "Germany",
            IsoCode2 = "DE",
            IsoCode3 = "DEU"
        };

        var before = DateTime.UtcNow;
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        Assert.Equal("test-user", country.CreatedBy);
        Assert.True(country.CreatedDate >= before);
        Assert.Null(country.UpdatedBy);
        Assert.Null(country.ModifiedDate);
    }

    [Fact]
    public async Task SavingChangesAsync_OnModified_SetsUpdatedByAndModifiedDate()
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(u => u.UserId).Returns("updater");

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(h => h.HttpContext).Returns((HttpContext?)null);

        var tenantId = Guid.NewGuid();
        var tenantService = new Mock<ITenantService>();
        tenantService.Setup(t => t.CurrentTenantId).Returns(tenantId);

        var interceptor = new AuditableEntityInterceptor(currentUser.Object, httpContextAccessor.Object);
        await using var context = BuildContext(interceptor, tenantService.Object);

        var country = new Country
        {
            TenantId = tenantId,
            Name = "France",
            IsoCode2 = "FR",
            IsoCode3 = "FRA",
            CreatedBy = "creator",
            CreatedDate = DateTime.UtcNow.AddMinutes(-1)
        };

        context.Countries.Add(country);
        await context.SaveChangesAsync();

        var savedCreatedBy = country.CreatedBy;
        var savedCreatedDate = country.CreatedDate;

        country.Name = "France (updated)";
        var before = DateTime.UtcNow;
        await context.SaveChangesAsync();

        Assert.Equal("updater", country.UpdatedBy);
        Assert.True(country.ModifiedDate >= before);
        Assert.Equal(savedCreatedBy, country.CreatedBy);
        Assert.Equal(savedCreatedDate, country.CreatedDate);
    }

    [Fact]
    public async Task SavingChangesAsync_OnModified_DoesNotOverwriteCreatedBy()
    {
        // Use a sequence: first call returns "creator", subsequent calls return "updater"
        var callCount = 0;
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(u => u.UserId).Returns(() => callCount++ == 0 ? "creator" : "updater");

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(h => h.HttpContext).Returns((HttpContext?)null);

        var tenantId = Guid.NewGuid();
        var tenantService = new Mock<ITenantService>();
        tenantService.Setup(t => t.CurrentTenantId).Returns(tenantId);

        var interceptor = new AuditableEntityInterceptor(currentUser.Object, httpContextAccessor.Object);
        await using var context = BuildContext(interceptor, tenantService.Object);

        var country = new Country
        {
            TenantId = tenantId,
            Name = "Spain",
            IsoCode2 = "ES",
            IsoCode3 = "ESP"
        };

        context.Countries.Add(country);
        await context.SaveChangesAsync();  // interceptor sets CreatedBy = "creator"

        country.Name = "España";
        await context.SaveChangesAsync();  // interceptor sets UpdatedBy = "updater", must NOT change CreatedBy

        Assert.Equal("creator", country.CreatedBy);
        Assert.Equal("updater", country.UpdatedBy);
    }
}
