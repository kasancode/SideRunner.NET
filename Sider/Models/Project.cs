using Newtonsoft.Json;
using System;
using System.Collections.Generic;

/// <summary>
/// base on 
/// http://json2csharp.com/
/// https://quicktype.io/
/// </summary>
namespace Sider.Models
{
    public partial class Project
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("tests")]
        public List<Test> Tests { get; set; }

        [JsonProperty("suites")]
        public List<Suite> Suites { get; set; }

        [JsonProperty("urls")]
        public List<Uri> Urls { get; set; }

        [JsonProperty("plugins")]
        public List<Plugin> Plugins { get; set; }
    }
}
