using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using SkiaSharp;
using Svg;
using System.Threading.Tasks.Dataflow;
using System.Reflection;

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
        public StraightRoad[] StraightRoads
        {
            get
            {
                StraightRoad[] returnArray = new StraightRoad[4];
                foreach (var straightRoad in Program.StraightRoads)
                {
                    if (straightRoad.NodeIds.Contains(Id)) returnArray[straightRoad.Direction] = straightRoad;
                }
                return returnArray;
            }
        }

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
        public int[] NeighbourNodesId
        {
            get
            {
                int[] returnArrary = [-1, -1, -1, -1, -1, -1, -1, -1];
                int i = 0;
                foreach (var roadId in RoadsId)
                {
                    if (roadId != -1)
                    {
                        returnArrary[i] = Program.Roads[roadId].NodesId.First(x => x!= Id);
                    }
                    i++;
                }
                return returnArrary;
            }
        }

        [JsonIgnore]
        public List<int>[] SameStraightRoadNodesId8Directs
        {
            get
            {
                List<int>[] returnArray = [[], [], [], [], [], [], [], []];
                int i = 0;
                foreach (var neighbourNodeId in NeighbourNodesId)
                {
                    if (neighbourNodeId != -1)
                    {
                        int nodeId = Id;
                        while (true)
                        {
                            int targetNeighbourId = Program.Nodes[nodeId].NeighbourNodesId[i];
                            if (targetNeighbourId != -1)
                            {
                                returnArray[i].Add(targetNeighbourId);
                                nodeId = targetNeighbourId;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    i++;
                }
                return returnArray;
            }
        }

        [JsonIgnore]
        public List<int>[] SameStraightRoadNodesId4Directis
        {
            get
            {
                List<int>[] returnArray = [[], [], [], []];
                for (int i = 0; i <= 3; i++)
                    returnArray[i] = [.. SameStraightRoadNodesId8Directs[i], .. SameStraightRoadNodesId8Directs[i + 4]];
                return returnArray;
            }
        }

        [JsonIgnore]
        public bool IsRoadStartOrEnd => RoadsId.Count(x => x != -1) == 1;

        [JsonIgnore]
        public bool IsStraight
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
        public bool IsCross
        {
            get
            {
                List<int[]> roadIdPairs = [[RoadsId[0], RoadsId[4]], [RoadsId[1], RoadsId[5]], [RoadsId[2], RoadsId[6]], [RoadsId[3], RoadsId[7]]];
                if (RoadsId.Count(x => x == -1) == 4)
                {
                    if (roadIdPairs.Count(x => x.SequenceEqual([-1, -1])) == 2)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [JsonIgnore]
        public bool IsTurn
        {
            get
            {
                int[] indices = RoadsId
                    .Select((value, index) => new { value, index })
                    .Where(x => x.value != -1)
                    .Select(x => x.index)
                    .ToArray();
                if (indices.Length == 2) return Math.Abs(indices[1] - indices[0]) != 4;
                return false;
            }
        }

        [JsonIgnore]
        public bool IsTShape
        {
            get
            {
                List<int[]> roadIdPairs = [[RoadsId[0], RoadsId[4]], [RoadsId[1], RoadsId[5]], [RoadsId[2], RoadsId[6]], [RoadsId[3], RoadsId[7]]];
                if (RoadsId.Count(x => x == -1) == 5)
                {
                    if (roadIdPairs.Count(x => x.SequenceEqual([-1, -1])) == 2)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [JsonIgnore]
        public bool IsComplex
        {
            get
            {
                bool direction0HasRoad = RoadsId[0] != -1 || RoadsId[4] != -1;
                bool direction1HasRoad = RoadsId[1] != -1 || RoadsId[5] != -1;
                bool direction2HasRoad = RoadsId[2] != -1 || RoadsId[6] != -1;
                bool direction3HasRoad = RoadsId[3] != -1 || RoadsId[7] != -1;
                int trueCount = 0;
                if (direction0HasRoad) trueCount++;
                if (direction1HasRoad) trueCount++;
                if (direction2HasRoad) trueCount++;
                if (direction3HasRoad) trueCount++;
                return trueCount >= 3;
            }
        }

        [JsonIgnore]
        public int Level
        {
            get
            {
                if (IsComplex) return 3;
                else if (IsTShape || IsCross || IsTurn) return 2;
                else return 1;
            }
        }

        [JsonIgnore]
        public decimal[] NodeMovingCanMoveDistance
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
                else if (IsStraight)
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
        public decimal[] RoadMovingCanMoveDistance
        {
            get
            {
                var returnArray = new decimal[8];
                int i = 0;
                foreach (var roadId in RoadsId)
                {
                    if (roadId != -1)
                    {
                        Road road = Program.Roads[roadId];
                        returnArray[i] = Math.Max(0, road.Length.Coefficient - 3);
                    }
                    else
                    {
                        returnArray[i] = -1;  // 不限制移动长度
                    }
                    i++;
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
