# Changelog
### 2022-11-19 .NET 7
Upgrade to .NET 7

### 2022-11-16 System.Text.Json
Use System.Text.Json for everything
(the only references to Newtonsoft.Json are in tests and via NSwag)

### 2022-11-13 Web Hooks
Web Hooks functionality. See further [documentation](./details/Webhooks.md).

### 2022-11-01 StyledAutocomplete update
StyledAutocomplete updates to better handle FreeSolo and Search scenarios

### 2022-24-10 TestBase simplified
Simplify inheritance hierarchy for tests (i.e. there's a single common [TestBase](../webapi/Lib/Testing/MccSoft.Testing/TestBase.cs) in `MccSoft.Lib.Testing`, and project-specific base classes [AppServiceTestBase](../webapi/tests/MccSoft.TemplateApp.App.Tests/AppServiceTestBase.cs) and [ComponentTestBase](../webapi/tests/MccSoft.TemplateApp.ComponentTests/ComponentTestBase.cs))
