# WebHook Infrastructure for ASP.NET Core

A lightweight, extensible WebHook processing library designed for ASP.NET Core applications.

This library provides everything you need to publish and reliably deliver WebHook events using Hangfire, with Polly-powered retry strategies, interceptors, and clean EF Core integration.

## ✨ Features

-  EF Core-based persistence of WebHooks and subscriptions
-  Automatic retry strategy using [Polly](https://github.com/App-vNext/Polly)
-  Integration via interceptors for pre/post execution and failure handling
-  Integration with [Hangfire](https://www.hangfire.io/) for background processing
-  Minimal configuration required — works out of the box 📦
-  Signing (sent webhooks payload) support with secret encryption
-  Custom HTTP headers
-  Fluent and strongly typed configuration

---

## 🚀 Getting Started

### 1. Install via Git Submodule or Copy to Your Template

This library is already available in the [feature/webhooks](https://github.com/mav10/backend-frontend-template/tree/feature/webhooks) branch of the [backend-frontend-template](https://github.com/mav10/backend-frontend-template) repository.

> ℹ️ Prerequisite: Your project must have [Hangfire](https://www.hangfire.io/) configured.
> This library schedules background jobs via Hangfire for reliable delivery.


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

### 4. Publish event with WebHookPublisher
Call IWebHookEventPublisher.PublishEvent to publish a WebHook:

```csharp
await _publisher.PublishEvent("user.created", new { UserId = 123 });
```
This stores the WebHook in the DB and schedules delivery via Hangfire.


### 5. Rely on WebHookProcessor

Delivery jobs are executed by WebHookProcessor, which:
- Applies Polly retry and timeout policies
- Invokes pre- and post-processing interceptors
- Logs success or failure of each execution
- Removes failed bindings if delivery permanently fails


----
## 🧠 Concepts

### What Are WebHooks?

WebHooks are a powerful mechanism for real-time notifications that allow your application to inform external systems about specific events as they happen. Instead of requiring other systems to poll your API repeatedly, WebHooks provide an efficient push-based model where the external system receives an HTTP POST as soon as an event is triggered.

This complements your existing integration logic and is particularly useful when external systems need to react to events like record creation, status changes, or time-based expirations. This library ensures delivery reliability using Hangfire and Polly, even under unstable network conditions.

---

## 🔄 Retry Strategy

Retries are implemented using Polly. The default settings:
- Retry: up to 5 times
- Backoff: exponential with jitter
- Timeout per call: 30 seconds

You can override these via `builder.ResilienceOptions`.

Delivery retries are managed via Hangfire and controlled by HangfireDelayInMinutes.

⚠️ Retry attempts are executed via Hangfire. Make sure Hangfire server is running, otherwise WebHooks will not be delivered.

----

## 🪝 Interceptors
Customize the execution pipeline using IWebHookInterceptors<TSub>:
```csharp
builder.WebHookInterceptors = new WebHookInterceptors<TemplateWebHookSubscription>
{
    BeforeExecution = webhook => Console.WriteLine($"Sending webhook #{webhook.id}"),
    ExecutionSucceeded = webhook => Console.WriteLine($"Webhook #{webhook.id} succeeded"),
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

---
## 🔐 Signing & Secrets

You can enable automatic signing of WebHook payloads using a shared secret, signed with HMAC SHA-256.

When `UseSigning = true`, the system:
- Generates a secret for every subscription (`SignatureSecret`)
- Encrypts it using AES (via `IWebHookSignatureService`)
- Automatically adds an `X-Signature` (or configured name) header to outgoing WebHook requests

### Setup

1. Add a strong encryption key in `appsettings.json`:
```json
"WebHooks": {
  "EncryptionKey": "base64-encoded-32-byte-key"
}
```

2. Register services in `Program.cs`:
```csharp
builder.Services.AddSingleton<ISecretEncryptor>(
  new AesSecretEncryptor(keyBase64: config["WebHooks:EncryptionKey"], ivBase64: "..."));

builder.Services.AddSingleton<IWebHookSignatureService, WebHookSignatureService>();
```

3. Enable signing in options:
```csharp
builder.Services.AddWebHooks<MySubscription>(options =>
{
    options.UseSigning = true;
    options.EncryptionKey = configuration["WebHooks:EncryptionKey"];
    options.WebHookSignatureHeaderName = "X-Signature"; // optional
});
```
⚠️ If signing is enabled but the encryption key is missing, the application will throw during startup.

### Rotation

To rotate a secret:
```csharp
var newSecret = await webHookService.RotateSecret(subscriptionId);
```

The new secret will be encrypted and stored, and used for signing from now on.

## Fluent Configuration API

All configuration is done via the `IWebHookOptionBuilder<TSub>` interface with a fluent API:

```csharp
services.AddWebHooks<CustomSubscription>(opt => opt
    .WithInterceptors(new MyInterceptors())
    .WithHangfireDelays([10, 30])
    .WithSigning(encryptionKey: "base64-key")
);
```

---

## 🧠 Best Practices

To build reliable, secure, and maintainable integrations using WebHooks, consider following these best practices:

- **Use polling as a fallback**: While WebHooks push data in real-time, combining them with periodic polling can help detect and recover from missed notifications and ensure data consistency.
- **Verify payloads**: Always authenticate incoming WebHook requests—e.g., using HMAC signatures or API tokens.
- **Respond quickly**: Return a 2xx HTTP status code as soon as possible. Defer long-running tasks to background workers.
- **Ensure idempotency**: Design your handlers to safely process the same event more than once, since retries are expected.
- **Log and monitor**: Track delivery attempts, failures, and response times to help diagnose issues.
- **Implement retry awareness**: Understand the library's retry strategy and prepare for multiple delivery attempts per event.
