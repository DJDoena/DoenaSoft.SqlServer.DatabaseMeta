using DoenaSoft.SqlServerDatabaseMeta;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

var reader = new MetaReader();

const string Database = "MasterData";

//var meta = reader.Read($"mes-sql-prod-1.basf.net", Database, "VI", "VI-Sol2019!");
var meta = reader.Read($"mes-proxy.basf.net", Database, "sa", "VI-Sol2019!");

//var functions = reader.ReadScalarFunctions($"mes-sql-prod-1.basf.net", Database, "VI", "VI-Sol2019!");
var functions = reader.ReadScalarFunctions($"mes-proxy.basf.net", Database, "sa", "VI-Sol2019!");

var serializer = JsonSerializer.Create(new JsonSerializerSettings
{
    ContractResolver = new BackReferenceIgnoringResolver(),
    Formatting = Formatting.Indented,
});

//using var streamWriter = new StreamWriter("prod.json");
using var streamWriter = new StreamWriter("proxy.json");

serializer.Serialize(streamWriter, new { Tables = meta, ScalarFunctions = functions });

sealed class BackReferenceIgnoringResolver : DefaultContractResolver
{
    private static readonly HashSet<string> _ignored = new(StringComparer.Ordinal)
    {
        nameof(IMetaBase.MetaId),
        nameof(IColumnMeta.Table),
        nameof(IColumnMeta.TableType),
        nameof(IIndexMeta.Table),
        nameof(ICheckMeta.Table),
        nameof(IForeignKeyMeta.SourceTable),
        nameof(IForeignKeyMeta.TargetTable),
    };

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (_ignored.Contains(property.PropertyName))
        {
            property.Ignored = true;
        }

        return property;
    }
}

