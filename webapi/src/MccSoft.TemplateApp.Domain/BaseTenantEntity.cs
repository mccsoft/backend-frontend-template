namespace MccSoft.TemplateApp.Domain;

public class BaseTenantEntity : BaseEntity, ITenantEntity
{
    public int TenantId { get; private set; }
    public Tenant Tenant { get; } = null!;

    public void SetTenantIdUnsafe(int tenantId)
    {
        TenantId = tenantId;
    }
}
