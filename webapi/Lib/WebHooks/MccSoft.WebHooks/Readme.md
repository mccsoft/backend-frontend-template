# WebHook Infrastructure for ASP.NET Core

A lightweight, extensible WebHook processing library designed for ASP.NET Core applications.

This library provides everything you need to publish and reliably deliver WebHook events using Hangfire, with Polly-powered retry strategies, interceptors, and clean EF Core integration.

## ✨ Features

-  EF Core-based persistence of WebHooks and subscriptions
-  Automatic retry strategy using [Polly](https://github.com/App-vNext/Polly)
-  Integration via interceptors for pre/post execution and failure handling
-  Integration with [Hangfire](https://www.hangfire.io/) for background processing
-  Minimal configuration required — works out of the box 📦

---

## 🚀 Getting Started

### 1. Install via Git Submodule or Copy to Your Template

This library is already available in the [feature/webhooks](https://github.com/mav10/backend-frontend-template/tree/feature/webhooks) branch of the [backend-frontend-template](https://github.com/mav10/backend-frontend-template) repository.

> Alternatively, you can copy the contents of the `WebHooks` folder directly into your own project or turn it into a shared package.

---

### 2. Add WebHook Entities to Your `DbContext`

In your `DbContext`, register the WebHook entities:

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
        /*
            Demonstrates how to register WebHook entities using a custom subscription type.
            If the default <see cref="WebHookSubscription"/> entity meets your requirements,
            you can simply register it as shown below:

            builder.AddWebHookEntities<WebHookSubscription>(GetType());

            Otherwise, provide your own type that inherits from WebHookSubscription,
            e.g. <see cref="TemplateWebHookSubscription"/>, to extend or override behavior.
        */
        builder.AddWebHookEntities<TemplateWebHookSubscription>(GetType());

        /*
            [Optional]
            We recommend placing WebHook-related entities in a separate schema
            (e.g. "webhooks") to keep them isolated from your main business logic tables.
        */
        builder.Entity<WebHook<TemplateWebHookSubscription>>().Metadata.SetSchema("webhooks");
        builder.Entity<TemplateWebHookSubscription>().Metadata.SetSchema("webhooks");
}
```

### 3. Register WebHook Services in DI Container

Add the following line in Startup.cs or wherever you configure services:
```csharp
services.AddWebHooks<TemplateWebHookSubscription>(builder =>
{
    // Optional: customize retry logic, interceptors, hangfire retry delays, etc.
    builder.ResilienceOptions.MaxRetryAttempts = 3;
    builder.HangfireDelayInMinutes = new[] { 1, 5, 15 }; // Customize retry delays
});
```

> You must provide a configured Hangfire instance. We assume it’s already registered in your application.
----
## 🧠 Concepts
### WebHookPublisher
Call IWebHookEventPublisher.PublishEvent to publish a WebHook:

```csharp
await _publisher.PublishEvent("user.created", new { UserId = 123 });
```
This stores the WebHook in the DB and schedules delivery via Hangfire.

### WebHookProcessor

Delivery jobs are executed by WebHookProcessor, which:
- Applies Polly retry and timeout policies
- Invokes pre- and post-processing interceptors
- Logs success or failure of each execution
- Removes failed bindings if delivery permanently fails

---

## 🔄 Retry Strategy

Retries are implemented using Polly. The default settings:
- Retry: up to 5 times
- Backoff: exponential with jitter
- Timeout per call: 30 seconds

You can override these via builder.ResilienceOptions.

Delivery retries are managed via Hangfire and controlled by HangfireDelayInMinutes.

----

## 🪝 Interceptors
Customize the execution pipeline using IWebHookInterceptors<TSub>:
```csharp
builder.WebHookInterceptors = new WebHookInterceptors<TemplateWebHookSubscription>
{
    BeforeExecution = id => Console.WriteLine($"Sending webhook #{id}"),
    ExecutionSucceeded = id => Console.WriteLine($"Webhook #{id} succeeded"),
    AfterAllAttemptsFailed = id => Console.WriteLine($"Webhook #{id} failed permanently"),
};
```

----
## 🧱 Custom Subscription Entity

You can extend the default WebHookSubscription to include things like:
- Custom metadata
- Filtering by tenant, tag, topic, etc.

Example:
```csharp
public class TemplateWebHookSubscription : WebHookSubscription
{
    public string? TenantName { get; set; }
}
```
