using DadBotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DadBotNet.Services
{
    public class ConfigService : IConfigService
    {
        Config config;

        public ConfigService()
        {
            config = new Config(File.ReadAllText("config.json"));
        }

        public string GetField(string field)
        {
            return config.GetField(field);
        }
    }
}
