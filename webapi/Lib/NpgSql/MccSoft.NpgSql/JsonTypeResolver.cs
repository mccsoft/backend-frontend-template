using System;
using System.Text.Json;
using MccSoft.LowLevelPrimitives.Serialization.DateOnlyConverters;
using Npgsql;
using Npgsql.Internal;
using Npgsql.Internal.TypeHandlers;
using Npgsql.Internal.TypeHandling;

namespace MccSoft.NpgSql;

public static class PostgresSerialization
{
    public static void AdjustDateOnlySerialization()
    {
        // We need this to support DateOnly within jsonb types
        // https://github.com/npgsql/efcore.pg/issues/1107
        var options = new JsonSerializerOptions { };
        options.Converters.Add(new DateOnlyConverter());
        NpgsqlConnection.GlobalTypeMapper.AddTypeResolverFactory(
            new JsonOverrideTypeHandlerResolverFactory(options)
        );
    }
}
class JsonOverrideTypeHandlerResolverFactory : TypeHandlerResolverFactory
{
    private readonly JsonSerializerOptions _options;

    public JsonOverrideTypeHandlerResolverFactory(JsonSerializerOptions options) =>
        _options = options;

    public override TypeHandlerResolver Create(NpgsqlConnector connector) =>
        new JsonOverrideTypeHandlerResolver(connector, _options);

    public override string? GetDataTypeNameByClrType(Type clrType) => null;

    public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName) => null;

    class JsonOverrideTypeHandlerResolver : TypeHandlerResolver
    {
        readonly JsonHandler _jsonbHandler;

        internal JsonOverrideTypeHandlerResolver(
            NpgsqlConnector connector,
            JsonSerializerOptions options
        ) =>
            _jsonbHandler ??= new JsonHandler(
                connector.DatabaseInfo.GetPostgresTypeByName("jsonb"),
                connector.TextEncoding,
                isJsonb: true,
                options
            );

        public override NpgsqlTypeHandler? ResolveByDataTypeName(string typeName) =>
            typeName == "jsonb" ? _jsonbHandler : null;

        public override NpgsqlTypeHandler? ResolveByClrType(Type type)
            // You can add any user-defined CLR types which you want mapped to jsonb
            =>
            _jsonbHandler;

        public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName) => null; // Let the built-in resolver do this
    }
}
