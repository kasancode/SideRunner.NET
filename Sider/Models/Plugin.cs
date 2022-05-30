using Newtonsoft.Json;

/// <summary>
/// base on 
/// http://json2csharp.com/
/// https://quicktype.io/
/// </summary>
namespace Sider.Models
{
    public partial class Plugin
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
