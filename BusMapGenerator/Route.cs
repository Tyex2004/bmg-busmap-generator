using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusMapGenerator
{
    internal class Route
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("color")]
        public int[] Color { get; set; } = new int[3];

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("via_nodes")]
        public List<int> ViaNodes { get; set; } = [];

        [JsonProperty("skip_stations")]
        public List<List<int>> SkipStations { get; set; } = [];

        [JsonProperty("end")]
        public int End { get; set; }
    }
}
