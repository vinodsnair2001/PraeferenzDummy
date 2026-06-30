namespace PraeferenzRoO.Application.Common.Interfaces;

public interface ITenantService
{
    Guid CurrentTenantId { get; }
    void SetCurrentTenant(Guid tenantId);
}
