using MccSoft.LowLevelPrimitives.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MccSoft.TemplateApp.App.Features.TestApi;

/// <summary>
/// Test API Controller that is only enabled on test and development stages.
/// Could be enabled/disabled by setting `TestApiEnabled` env. variable
/// </summary>
public class TestApiController : Controller
{
    private readonly TestApiService _testApiService;
    private readonly bool _isTestApiEnabled;

    public TestApiController(TestApiService testApiService, IConfiguration configuration)
    {
        _testApiService = testApiService;
        _isTestApiEnabled = configuration.GetValue<bool>("TestApiEnabled");
    }

    /// <summary>
    /// Called before any action method is invoked.
    /// </summary>
    /// <param name="context">The action executing context.</param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!_isTestApiEnabled)
        {
            throw new ValidationException("Test API is disabled for this stage.");
        }
    }

    /// <summary>
    /// Resets tenant to a default state.
    /// Resetting in practice is usually faster then creating new tenant
    /// (in cases when creating  a tenant involves seeding the data).
    ///
    /// Also resetting allows to reuse the browser session in UI Tests without re-login in every test.
    /// </summary>
    public async Task ResetTenant()
    {
        await _testApiService.ResetTenant();
    }

    /// <summary>
    /// Creates a test tenant to be used in UI Tests
    /// </summary>
    [AllowAnonymous]
    public async Task CreateTestTenant(CreateTestTenantDto dto)
    {
        await _testApiService.CreateTestTenant(dto);
    }
}
