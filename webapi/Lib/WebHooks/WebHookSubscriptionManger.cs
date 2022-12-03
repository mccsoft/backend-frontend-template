public class WebHookSubscriptionManager
{
    private readonly DbContext _dbContext;

    public WebHookSubscriptionManger(DbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    /// <summary>
    /// Adds a subscription to a certain eventType in a tenant.
    /// When event occurs (i.e. someone calls <see cref="TriggerEvent" /> method) the passed url will be called.
    /// </summary>
    /// <param name="tenantId">Id of the tenant</param>
    /// <param name="eventType">Type of an event</param>
    /// <param name="url">URL to call when event occurs</param>
    /// <param name="method">HTTP method to use for the call (e.g. 'POST', 'PUT', etc.)</param>
    public async Task<int> AddSubscription(
        int tenantId,
        string eventType,
        string url,
        string method
    ) { }

    /// <summary>
    /// Removes the previously created subscription to event.
    /// </summary>
    /// <param name="tenantId">Id of the tenant</param>
    /// <param name="eventType">Type of an event</param>
    /// <param name="url">Target URL for which subscription was configured</param>
    /// <returns></returns>
    public async Task RemoveSubscription(int tenantId, string eventType, string url) { }

    public IQueryable<WebHookSubscription> GetSubscriptions(int tenantId, string? eventType) =>
        _dbContext
            .WebHooks()
            .Where(x => x.TenantId == tenantId && (eventType == null || x.Type == eventType));

    /// <summary>
    /// Triggers the event for passed tenant
    /// </summary>
    /// <param name="tenantId">Id of the tenant</param>
    /// <param name="eventType">Type of an event</param>
    /// <param name="data">Body of POST request that will be sent</param>
    /// <returns></returns>
    public async Task TriggerEvent(int tenantId, string eventType, JsonDocument? data) { }
}
