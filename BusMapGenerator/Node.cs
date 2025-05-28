using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using SkiaSharp;
using Svg;

namespace BusMapGenerator
{
    public class Node
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("coord")]
        public decimal[] JSONCoord = new decimal[2];

        [JsonIgnore]
        public SvgPoint SvgCoord => Utils.CoordJSONToSvg(JSONCoord);

        [JsonIgnore]
        public SKPoint SkiaCoord => Utils.CoordJSONToSkia(JSONCoord);

        [JsonIgnore]
        public Point WPFCoord => Utils.CoordSkiaToWPF(SkiaCoord, Program.CurrentSkiaElement);

        [JsonIgnore]
        public int[] RoadsId  // 从北侧开始顺时针
        {
            get
            {
                int[] roads = [-1, -1, -1, -1, -1, -1, -1, -1];
                foreach (KeyValuePair<int, Road> road in Program.Roads)
                {
                    if (road.Value.NodesId[0] == Id)
                    {
                        roads[road.Value.Direction] = road.Key;
                    }
                    else if (road.Value.NodesId[1] == Id)
                    {
                        roads[(road.Value.Direction + 4) % 8] = road.Key;
                    }
                }
                return roads;
            }
        }

        [JsonIgnore]
        public SvgCircle RPGraph => new()
        {
            CenterX = SvgCoord.X,
            CenterY = SvgCoord.Y,
            Radius = 1.5f,
            Fill = new SvgColourServer(System.Drawing.Color.FromArgb(100, 246, 255)),
            Stroke = null
        };
    }
}
