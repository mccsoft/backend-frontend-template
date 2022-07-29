# Multi tenancy implementation
We include multi-tenancy implementation in the template via [DbContextQueryFilterExtensions](webapi/Lib/PersistenceHelpers/MccSoft.PersistenceHelpers/DbContextQueryFilterExtensions.cs) and [PostProcessEntitiesOnSaveInterceptor](webapi/Lib/PersistenceHelpers/MccSoft.PersistenceHelpers/PostProcessEntitiesOnSaveInterceptor.cs).

[Multi tenancy](https://docs.microsoft.com/en-us/ef/core/miscellaneous/multitenancy) roughly means that different groups of users see different data. It could be that:
1. Every user has his own set of data (e.g. ToDo app)
2. Every user belongs to some Organisation (tenant), and users within Organisation see the same data, but user in different Organisations never share the data (e.g. different Organisations in AzureDevOps)

EFCore has support for such kind of data sharing out of the box via [Query Filters](https://docs.microsoft.com/en-us/ef/core/miscellaneous/multitenancy#an-example-solution-single-database).

## Implementaion details
Usually it's implemented so that EVERY database Entity has a special column, which describes the Tenant entity belongs to (e.g. `OwnerId` for 1st scenario, or `TenantId` for 2nd scenario).

EFCore is then configured with a QueryFilter so that EF only returns entities with the same `OwnerId` (or `TenantId`) as the currently logged-in user has.

In [Template](webapi/src/MccSoft.TemplateApp.Persistence/TemplateAppDbContext.cs) it's integrated like this:
```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
  builder.SetupQueryFilter<IOwnedEntity>(
      (x) => CurrentOwnerId == null || x.OwnerId == CurrentOwnerId
  );
}
```

Also it makes sense to automatically set this `OwnerId` (or `TenantId`) field whenever an entity is saved to the DB.

It could be done with help of [PostProcessEntitiesOnSaveInterceptor](webapi/Lib/PersistenceHelpers/MccSoft.PersistenceHelpers/PostProcessEntitiesOnSaveInterceptor.cs):
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    base.OnConfiguring(optionsBuilder);

    optionsBuilder.AddInterceptors(
        new PostProcessEntitiesOnSaveInterceptor<IOwnedEntity, TemplateAppDbContext>(
            (entity, context) =>
            {
                entity.SetOwnerIdUnsafe(context.CurrentOwnerId);
            }
        )
    );
}
```
it will call the passed action when `IOwnedEntity` is saved.
