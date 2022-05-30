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
    public partial class Test
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("commands")]
        public List<Command> Commands { get; set; }
    }
}
