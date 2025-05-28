using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusMapGenerator
{
    internal class MtrStation
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("routes")]
        public List<string> Routes { get; set; } = [];
    }
}
