# Why
We are using Postgrtes Enums for enum properties, so that it is easier to understand what '0', '1', '2' etc. means. It's very important for people who use database directly, write SQL queries against database manually and/or use some BI tools.

# How
Here's the documentation how Postgres enums are integrated into EFCore: https://www.npgsql.org/efcore/mapping/enum.html?tabs=tabid-1.
So, for every new enum that you are adding to your domain entities you should also add it in DbContext:
1. To `OnModelCreating` override, something like `builder.HasPostgresEnum<YOUR_ENUM>();`
1. To static constructor of DbContext like `NpgsqlConnection.GlobalTypeMapper.MapEnum<ProductType>();`

# Migration of existing data
If you forgot to add enum in constructor, or decided to use Postgres Enums later (shortly, if you already have some data in the table, which column you'd like to convert to enum), you'd need to adjust your generated migration manually (if you were storing enums as INTs previously).

You'd have the following generated automatically:
```csharp
            migrationBuilder.AlterColumn<MergeFieldQueryType>(
                name: "YOUR_FIELD",
                table: "YOUR_TABLE",
                type: "ENUM_TYPE",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true
            );
```
You'd need to replace it with the following SQL:
```csharp
migrationBuilder.Sql(@"ALTER TABLE ""YOUR_TABLE"" ALTER COLUMN ""YOUR_FIELD"" TYPE ENUM_TYPEusing (enum_range(null::ENUM_TYPE))[""YOUR_FIELD""::int + 1];");
```
Otherwise you will get an exception when running this migration (`cannot cast INT to YOUR_ENUM`).

P.S. If your enum columns have default values in DB before migration, you need to DROP them and recreate. So your overall SQL for migration will be like:
```csharp
migrationBuilder.Sql(
	@"ALTER TABLE ""YOUR_TABLE"" ALTER COLUMN ""YOUR_FIELD"" DROP DEFAULT;"
);
migrationBuilder.Sql(
	@"ALTER TABLE ""YOUR_TABLE"" ALTER COLUMN ""YOUR_FIELD"" TYPE ENUM_TYPEusing (enum_range(null::ENUM_TYPE))[""YOUR_FIELD""::int + 1];"
);
migrationBuilder.Sql(
	@"ALTER TABLE ""YOUR_TABLE"" ALTER COLUMN ""YOUR_FIELD"" SET DEFAULT DEFAULT_ENUM_TYPE;"
);
```