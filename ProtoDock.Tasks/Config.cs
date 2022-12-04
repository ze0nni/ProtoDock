using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoDock.Tasks
{
    class Config
    {
        public bool OnlyMinimised { get; set; } = false;

        public string Write()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }

        public static Config Read(string data)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<Config>(data);
            }
            catch
            {
                return new Config();
            }
        }
    }
}
