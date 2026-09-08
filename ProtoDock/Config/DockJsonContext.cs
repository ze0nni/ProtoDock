using System.Text.Json.Serialization;

namespace ProtoDock.Config
{
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        UseStringEnumConverter = true,
        PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(DockConfig))]
    [JsonSerializable(typeof(DockSkin))]
    internal partial class DockJsonContext : JsonSerializerContext
    {
    }
}
