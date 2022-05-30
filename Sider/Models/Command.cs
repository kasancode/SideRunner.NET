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
    public partial class Command
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("command")]
        public string CommandName { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("targets")]
        public List<List<string>> Targets { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("opensWindow", NullValueHandling = NullValueHandling.Ignore)]
        public bool? OpensWindow { get; set; }

        [JsonProperty("windowHandleName", NullValueHandling = NullValueHandling.Ignore)]
        public string? WindowHandleName { get; set; }

        [JsonProperty("windowTimeout", NullValueHandling = NullValueHandling.Ignore)]
        public long? WindowTimeout { get; set; }

        [JsonProperty("skip", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Skip { get; set; }
    }
}
