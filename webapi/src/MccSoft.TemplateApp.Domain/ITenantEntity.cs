public interface ITenantEntity
{
    public int TenantId { get; }

    public Tenant Tenant { get; }

    /// <summary>
    /// Isn't meant to be called from Application code.
    /// Should be called by infrastructure (e.g. SaveChanges interceptor)
    /// </summary>
    void SetTenantIdUnsafe(int tenantId);
}
