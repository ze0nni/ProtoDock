using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProtoDock.Tray
{
    class Config
    {
        public bool OnlyPinned { get; set; } = false;

        public string Write()
        {
            return JsonSerializer.Serialize(this, ConfigJsonContext.Default.Config);
        }

        public static Config Read(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return new Config();
            }

            try
            {
                return JsonSerializer.Deserialize(data, ConfigJsonContext.Default.Config) ?? new Config();
            }
            catch
            {
                return new Config();
            }
        }
    }

    [JsonSerializable(typeof(Config))]
    internal partial class ConfigJsonContext : JsonSerializerContext
    {
    }
}
