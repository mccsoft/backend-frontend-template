# Changelog

### 2023-02-06 Playwright with Storybook testing

End-to-end tests using Playwright were introduced to the project (check out [e2e](./e2e) folder). These tests also include screenshot testing for all Storybook stories.

### 2023-01-16 Storybook

Storybook was added to the project, giving the ability to play with ui-kit components in an isolated environment

### 2022-12-04 [Log] attribute

Implement logging of starting/finishing the method with AOP `[Log]` attribute.

### 2022-12-03 Multi-tenancy

Support multi-tenant out of the box

### 2022-11-19 .NET 7

Upgrade to .NET 7

#### Manual steps (you need to do that if manually updating from previous versions via `yarn pull-changes-from-template`)

1. Replace `<TargetFramework>net6.0</TargetFramework>` with `<TargetFramework>net7.0</TargetFramework>`
1. Remove `NJsonSchema` from .App.csproj, .ComponentTests.csproj
1. Remove `NSwag.Generation`, `NSwag.CodeGeneration` from ComponentTests.csproj
1. Change first line of your Dockerfile to `FROM mcr.microsoft.com/dotnet/aspnet:7.0`

### 2022-11-16 System.Text.Json

Use System.Text.Json for everything (the only references to Newtonsoft.Json are in tests and via NSwag)

### 2022-11-13 Web Hooks

Web Hooks functionality. See further [documentation](./details/Webhooks.md).

#### Manual steps (you need to do that if manually updating from previous versions via `yarn pull-changes-from-template`)

1. Call `builder.Services.AddWebHooks();` from [Program.cs](../../webapi/src/MccSoft.TemplateApp.App/Program.cs)
2. Call `builder.AddWebHookEntities(this.GetType());` from `OnModelCreating` method of your [DbContext](../../webapi/src/MccSoft.TemplateApp.Persistence/TemplateAppDbContext.cs)

### 2022-11-01 StyledAutocomplete update

StyledAutocomplete updates to better handle FreeSolo and Search scenarios

### 2022-24-10 TestBase simplified

Simplify inheritance hierarchy for tests (i.e. there's a single common [TestBase](../webapi/Lib/Testing/MccSoft.Testing/TestBase.cs) in `MccSoft.Lib.Testing`, and project-specific base classes [AppServiceTestBase](../webapi/tests/MccSoft.TemplateApp.App.Tests/AppServiceTestBase.cs) and [ComponentTestBase](../webapi/tests/MccSoft.TemplateApp.ComponentTests/ComponentTestBase.cs))
