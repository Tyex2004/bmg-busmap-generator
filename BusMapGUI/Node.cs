using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BusMapGenerator
{
    public class Node
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("coord")]
        public decimal[] Coord { get; set; } = new decimal[2];
    }
}
