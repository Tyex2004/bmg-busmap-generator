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
        public Point WPFCoord => Utils.CoordSkiaToWPF(SkiaCoord, Program.RPSkiaElement);

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
        public bool IsRoadStartOrEnd => RoadsId.Count(x => x != -1) == 1;

        [JsonIgnore]
        public bool IsNotRoadEnterence
        {
            get
            {
                int[] indices = RoadsId
                    .Select((value, index) => new { value, index })
                    .Where(x => x.value!= -1)
                    .Select(x => x.index)
                    .ToArray();
                if (indices.Length == 2) return Math.Abs(indices[1] - indices[0]) == 4;
                return false;
            }
        }

        [JsonIgnore]
        public decimal[] CanMoveDistance
        {
            get
            {
                var returnArray = new decimal[8];
                if (IsRoadStartOrEnd)
                {
                    int i = 0;
                    foreach (var roadId in RoadsId)
                    {
                        if (roadId != -1)
                        {
                            Road road = Program.Roads[roadId];
                            returnArray[i] = Math.Max(0, road.Length.Coefficient - 3);
                            returnArray[Utils.SwapDirection(i)] = -1;  // 不限制移动长度
                        }
                        i++;
                    }
                }
                else if (IsNotRoadEnterence)
                {
                    int i = 0;
                    foreach (var roadId in RoadsId)
                    {
                        if (roadId != -1)
                        {
                            Road road = Program.Roads[roadId];
                            returnArray[i] = Math.Max(0, road.Length.Coefficient - 3);
                        }
                        i++;
                    }
                }
                return returnArray;
            }
        }

        [JsonIgnore]
        public static int NextNewId
        {
            get
            {
                HashSet<int> existingIds = new(Program.Nodes.Keys);
                int i = 1;
                while (existingIds.Contains(i))
                {
                    i++;
                }
                return i;
            }
        }

        [JsonIgnore]
        public SvgCircle RPGraph => new()
        {
            CenterX = SvgCoord.X,
            CenterY = SvgCoord.Y,
            Radius = 1.5f,
            Fill = Utils.SetColor(100, 246, 255),
            Stroke = null
        };
    }
}
