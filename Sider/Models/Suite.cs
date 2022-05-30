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
    public partial class Suite
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("persistSession")]
        public bool PersistSession { get; set; }

        [JsonProperty("parallel")]
        public bool Parallel { get; set; }

        [JsonProperty("timeout")]
        public long Timeout { get; set; }

        [JsonProperty("tests")]
        public List<Guid> Tests { get; set; }
    }
}
