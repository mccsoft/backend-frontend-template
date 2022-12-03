using System;

public class WebHookSubscription
{
    public int Id { get; private set; }

    public WebHookEventType EventType { get; set; } = null!;

    public DateTime? LastSent { get; set; }

    /// <summary>
    /// Webhook secret
    /// </summary>
    public string? Secret { get; set; }

    /// <summary>
    /// Is subscription active
    /// </summary>
    public bool IsActive { get; set; }
}
