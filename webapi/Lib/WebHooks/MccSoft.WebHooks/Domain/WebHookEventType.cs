using System.Collections.Generic;

public class WebHookEventType
{
    public int Id { get; private set; }

    public string Type { get; set; } = null!;

    public List<WebHookSubscription>? Subscriptions { get; set; }
}
