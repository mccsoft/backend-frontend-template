using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Audit.Core;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using MccSoft.LowLevelPrimitives;
using MccSoft.TemplateApp.App.Setup;
using MccSoft.TemplateApp.App.Utils.Localization;
using MccSoft.TemplateApp.Domain;
using MccSoft.TemplateApp.Persistence;
using MccSoft.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace MccSoft.TemplateApp.App.Tests;

/// <summary>
/// The base class for application service test classes.
/// </summary>
public class AppServiceTestBase : TestBase<TemplateAppDbContext>
{
    protected User _defaultUser;
    public override bool InsertLoggerInEf => true;

    protected AppServiceTestBase(
        ITestOutputHelper outputHelper,
        DatabaseType? testDatabaseType = DatabaseType.Postgres
    )
        : base(
            outputHelper,
            testDatabaseType,
            adjustNpgsqlDataSource: builder =>
                TemplateAppDbContext.MapEnums(builder.EnableDynamicJson())
        )
    {
        Configuration.AuditDisabled = true;

        // initialize some variables to be available in all tests
        if (testDatabaseType != null)
        {
            TaskUtils.RunSynchronously(async () =>
            {
                await WithDbContext(async db =>
                {
                    using var _ = CustomTenantIdAccessor.IgnoreTenantIdQueryFilter();
                    _defaultUser = await db.Users.FirstAsync(x => x.Email == "default@test.test");
                });
                _userAccessorMock.Setup(x => x.GetUserId()).Returns(_defaultUser.Id);
                _userAccessorMock.Setup(x => x.GetTenantId()).Returns(_defaultUser.TenantId);
            });
        }
    }

    /// <summary>
    /// Insert some data that you want to be available in every test.
    ///
    /// Note, that this method is executed only once, when template database is initially created.
    /// !!IT IS NOT CALLED IN EACH TEST!!!
    /// (though, created data is available in each test by backing up
    /// and restoring a DB from template for each test)
    /// </summary>
    protected override DatabaseSeedingOptions<TemplateAppDbContext> SeedDatabase() =>
        new DatabaseSeedingOptions<TemplateAppDbContext>(
            nameof(TemplateAppDbContext) + "AppServiceTest",
            async (db) =>
            {
                var tenant = new Tenant();
                db.Tenants.Add(tenant);
                await db.SaveChangesAsync();

                var user = new User("default@test.test");
                db.Users.Add(user);
                user.SetTenantIdUnsafe(tenant.Id);
                await db.SaveChangesAsync();
            }
        );

    protected override void RegisterServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment
    )
    {
        SetupServices.AddServices(services, configuration, environment);

        services
            .AddDefaultIdentity<User>()
            .AddRoles<IdentityRole>()
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddEntityFrameworkStores<TemplateAppDbContext>();

        // Here you could override type registration (e.g. mock http clients that call other microservices).
        // Most probably you'd need to remove existing registration before registering new one.
        // You could remove the registration by calling:
        // services.RemoveRegistration<TService>();
    }

    /// <summary>
    /// Creates a NEW DbContext.
    /// Resolving it from ServiceProvider is not enough, because we will get the same DbContext every time.
    /// </summary>
    protected override TemplateAppDbContext CreateDbContext(IServiceProvider serviceProvider) =>
        SetupDatabase.CreateDbContext(serviceProvider);

    #region Validation

    public record ValidationResult(string MemberNames, string ErrorMessage);

    /// <summary>
    /// Returns list of validation result for DataAnnotation errors assertion.
    /// </summary>
    protected IList<ValidationResult> ValidateModel(object model)
    {
        var provider = new MetadataTranslationProvider(
            CreateService<IStringLocalizer<MetadataTranslationProvider.DataAnnotationLocalizer>>()
        );
        var attributes = GetValidationAttributes(model.GetType());
        provider.CreateValidationMetadata(attributes);

        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults
            .Select(x => new ValidationResult(string.Join(",", x.MemberNames), x.ErrorMessage))
            .ToList();
    }

    private List<object> GetValidationAttributes(Type type)
    {
        var properties = TypeDescriptor.GetProperties(type);
        var attributes = new List<object>();

        for (var i = 0; i < properties.Count; i++)
        {
            var property = properties[i];
            for (var j = 0; j < property.Attributes.Count; j++)
            {
                attributes.Add(property.Attributes[j]);
            }
        }

        return attributes;
    }
    #endregion
}
