using Newtonsoft.Json.Linq;

namespace DadBotNet.Models
{
    internal class Config
    {
        string jsonContent;

        JObject jsonObject;

        public Config(string fileContent)
        {
            jsonContent = fileContent;

            jsonObject = JObject.Parse(jsonContent);
        }

        public string GetField(string field)
        {
            if(!jsonObject.ContainsKey(field))
            {
                throw new ArgumentException("Field does not exist in config file");
            }

            return jsonObject.GetValue(field).ToString();
        }
    }
}
