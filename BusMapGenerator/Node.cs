using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Navigation;
using SkiaSharp;

namespace BusMapGenerator
{
    public class Node
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        private decimal[] _coord = new decimal[2];

        [JsonProperty("coord")]
        public decimal[] Coord
        {
            get => _coord;
            set
            {
                if (value == null || value.Length != 2)
                    _coord = new decimal[2];
                else
                    _coord = value;
            }
        }

        public SKPoint GeoCoord => Utils.CoordJSONToSkia(Coord);
    }
}
