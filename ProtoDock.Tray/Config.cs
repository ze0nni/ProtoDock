namespace ProtoDock.Tray
{
    class Config
    {
        public bool OnlyPinned { get; set; } = false;

        public string Write()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }

        public static Config Read(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return new Config();
            }

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<Config>(data) ?? new Config();
            }
            catch
            {
                return new Config();
            }
        }
    }
}
