using DoenaSoft.SqlServerDatabaseMeta;
using Newtonsoft.Json;

var meta = (new MetaReader()).Read($"server", "database", "user", "password");

var serializer = JsonSerializer.Create(new JsonSerializerSettings
{
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    Formatting = Formatting.Indented,
});

using var streamWriter = new StreamWriter("meta.json");

serializer.Serialize(streamWriter, meta);