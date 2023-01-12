# Webhooks
Webhooks is a way to notify any 3rd party app/service about the events that happened in your app/service.

E.g.: some 3rd party wants to subscribe to the ProductAdded event (i.e. be notified when product is added).

So, 3rd party should create a WebHook, that when product is added to our App, we should call some URL (e.g. POST https://our-client.com/product-added).

# MccSoft.Webhooks library
Library MccSoft.Webhooks helps with sending out web hook calls and handle retries if they are needed.

I.e. if 3rd party server is not answering (e.g. timeout) or is answering with an error (e.g. 500 Internal Server Error, 503 Service Unavailable, etc.) we should retry the call after some time.

Retry intervals are configurable (via `services.AddWebHooks(options => {options.SendingDelaysInMinutes = new [] {10, 20, 100};}`))

## How to add
1. Call `builder.Services.AddWebHooks();` from [Program.cs](../../webapi/src/MccSoft.TemplateApp.App/Program.cs)
2. Call `builder.AddWebHookEntities(this.GetType());` from `OnModelCreating` method of your [DbContext](../../webapi/src/MccSoft.TemplateApp.Persistence/TemplateAppDbContext.cs)

## How to use
