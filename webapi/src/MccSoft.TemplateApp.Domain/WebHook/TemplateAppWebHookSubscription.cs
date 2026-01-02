using System;
using System.Collections.Generic;
using System.Net.Http;
using MccSoft.DomainHelpers;
using MccSoft.WebHooks.Domain;

namespace MccSoft.TemplateApp.Domain.WebHook;

/// <summary>
/// This is an example of overriden webhook (to add Tenant for example. You can use base entity from lib)
/// </summary>
public class TemplateAppWebHookSubscription : WebHookSubscription, ITenantEntity
{
    #region ITenantEntity

    public int TenantId { get; private set; }

    private Tenant? _tenant;

    public Tenant Tenant
    {
        get => _tenant.ThrowIfNotIncluded();
        private set => _tenant = value;
    }

    public void SetTenantIdUnsafe(int tenantId)
    {
        TenantId = tenantId;
    }

    #endregion

    /// <summary>
    /// Needed for Entity Framework, keep empty.
    /// </summary>
    public TemplateAppWebHookSubscription() { }

    public TemplateAppWebHookSubscription(
        string name,
        string url,
        string eventType,
        HttpMethod? method = null,
        Dictionary<string, string>? headers = null
    )
        : base(name, url, eventType, (method ?? HttpMethod.Post).Method, headers) { }

    #region Specification

    /// <summary>
    /// Creates a specification that is satisfied by a WebhookSubscription having the specified id.
    /// </summary>
    /// <param name="id">Subscription id.</param>
    /// <returns>The created specification.</returns>
    public static Specification<TemplateAppWebHookSubscription> HasId(Guid id) =>
        new(nameof(HasId), x => x.Id == id, id);

    #endregion
}
