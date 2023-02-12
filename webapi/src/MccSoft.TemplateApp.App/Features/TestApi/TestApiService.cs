using MccSoft.LowLevelPrimitives.Exceptions;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.TemplateApp.App.Features.TestApi;

public class TestApiService
{
    private readonly TemplateAppDbContext _dbContext;
    private readonly UserManager<User> _userManager;

    public TestApiService(TemplateAppDbContext dbContext, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
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
        await _dbContext.Products.ExecuteDeleteAsync();
        await _dbContext.AuditLogs.ExecuteDeleteAsync();
    }

    /// <summary>
    /// Creates a test tenant to be used in UI Tests
    /// </summary>
    public async Task<CreateTestTenantResponseDto> CreateTestTenant(CreateTestTenantDto dto)
    {
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(
            x => x.Email == dto.UserEmail
        );
        if (existingUser != null)
            throw new ValidationException(nameof(dto.UserEmail), "User exists");

        var tenant = new Tenant();
        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync();

        using var setCustomTenant = _dbContext.UserAccessor.SetCustomTenantId(tenant.Id);
        var user = new User(dto.UserEmail);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        await _userManager.AddPasswordAsync(user, dto.UserPassword);
        return new CreateTestTenantResponseDto(tenant.Id);
    }
}
