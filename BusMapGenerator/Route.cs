using HarfBuzzSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup.Localizer;
using Svg;
using Svg.Pathing;
using System.Drawing;
using System.Diagnostics;

namespace BusMapGenerator
{
    public class Route
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

        [JsonIgnore]
        public List<Node> Nodes => [.. ViaNodes.Select(x => Program.Nodes[x])];

        [JsonIgnore]
        public List<(Road, bool IsForward)> Roads
        {
            get
            {
                List<(Road, bool)> returnList = [];
                // 没有经过道路节点
                if (ViaNodes.Count == 0)
                {
                    if (Program.Stations[Start].RoadId == Program.Stations[End].RoadId)
                    {
                        Debug.WriteLine($"线路 {Id} 起点站和终点站相同，且未经过任何道路节点");
                        return [];
                    }
                    else
                    {
                        if (Program.Stations[Start].OnRoadPos < Program.Stations[End].OnRoadPos)
                        {
                            returnList.Add((Program.Stations[Start].Road, true));
                        }
                        else
                        {
                            returnList.Add((Program.Stations[End].Road, false));
                        }
                    }
                }
                // 会经过道路节点
                else
                {
                    // 起点站所在道路
                    int roadId = Program.Stations[Start].RoadId;
                    Road road = Program.Roads[roadId];
                    if (Program.Nodes[ViaNodes[0]].RoadsId.Contains(roadId))
                    {
                        if (road.NodesId[1] == ViaNodes[0]) returnList.Add((road, true));
                        else returnList.Add((road, false));
                    }
                    else
                    {
                        Debug.WriteLine($"线路 {Id} 起点站 {Start} 所在道路 {roadId} 不经过道路节点 {ViaNodes[0]}");
                        return [];
                    }
                    // 中间段道路
                    for (int i = 0; i < ViaNodes.Count - 1; i++)
                    {
                        roadId = Program.Nodes[ViaNodes[i]].RoadsId.FirstOrDefault(x => Program.Nodes[ViaNodes[i + 1]].RoadsId.Contains(x) && (x != -1));
                        if (roadId == 0) 
                        { 
                            Debug.WriteLine($"道路节点 {ViaNodes[i]} 和道路节点 {ViaNodes[i + 1]} 找不到他们共同的道路，因为\n" +
                                $"道路节点 {ViaNodes[i]} 道路列表：{string.Join(", ", Program.Nodes[ViaNodes[i]].RoadsId)}\n" +
                                $"道路节点 {ViaNodes[i + 1]} 道路列表：{string.Join(", ", Program.Nodes[ViaNodes[i + 1]].RoadsId)}");
                            return []; 
                        }
                        road = Program.Roads[roadId];
                        if (roadId != 0)
                        {
                            if (Program.Nodes[ViaNodes[i]].Id == road.NodesId[0]) returnList.Add((road, true));
                            else returnList.Add((road, false));
                        }
                        else return [];
                    }
                    // 终点站所在道路
                    roadId = Program.Stations[End].RoadId;
                    road = Program.Roads[roadId];
                    if (Program.Nodes[ViaNodes[^1]].RoadsId.Contains(roadId))
                    {
                        if (road.NodesId[0] == ViaNodes[^1]) returnList.Add((road, true));
                        else returnList.Add((road, false));
                    }
                    else
                    {
                        Debug.WriteLine($"线路 {Id} 终点站 {End} 所在道路 {roadId} 不经过道路节点 {ViaNodes[^1]}");
                        return [];
                    }
                }
                return returnList;
            }
        }

        [JsonIgnore]
        public List<int> NodesOfRoads
        {
            get
            {
                List<int> returnList = [];
                if (Roads.Count == 1)
                {
                    decimal onRoadPosA = Program.Stations[Start].OnRoadPos;
                    decimal onRoadPosB = Program.Stations[End].OnRoadPos;
                    if (onRoadPosA < onRoadPosB)
                    {
                        returnList.Add(Program.Stations[Start].Road.NodesId[0]);
                        returnList.Add(Program.Stations[Start].Road.NodesId[1]);
                    }
                    else
                    {
                        returnList.Add(Program.Stations[End].Road.NodesId[0]);
                        returnList.Add(Program.Stations[End].Road.NodesId[1]);
                    }
                }
                else if (Roads.Count > 1)
                {
                    int nodeStartId = Roads[0].Item1.NodesId.FirstOrDefault(x => x != ViaNodes[0]);
                    int nodeEndId = Roads[^1].Item1.NodesId.FirstOrDefault(x => x != ViaNodes[^1]);
                    returnList.Add(nodeStartId);
                    foreach (var vianode in ViaNodes)
                    {
                        returnList.Add(vianode);
                    }
                    returnList.Add(nodeEndId);
                }
                return returnList;
            }
        }

        [JsonIgnore]
        public RoutePart this[int index] => Program.RouteParts.First(x => x.Route == this && x.Index == index);

        [JsonIgnore]
        public List<decimal[]> JSONCoords
        {
            get
            {
                decimal bold = Program.BoldOfRoutes;
                List<decimal[]> returnList = [];
                if (Roads.Count == 0) { Debug.WriteLine($"线路 {Id} 的道路列表为空"); return []; }
                decimal[] JSONStartCoord = Roads[0].Item1.StraightRoad.Direction switch
                {
                    0 => [Program.Stations[Start].JSONCoord[0] + this[0].Slot * bold, Program.Stations[Start].JSONCoord[1]],
                    1 => [Program.Stations[Start].JSONCoord[0] + 0.5m * this[0].Slot * bold, Program.Stations[Start].JSONCoord[1] - 0.5m * this[0].Slot * bold],
                    2 => [Program.Stations[Start].JSONCoord[0], Program.Stations[Start].JSONCoord[1] + this[0].Slot * bold],
                    _ /* 3 */ => [Program.Stations[Start].JSONCoord[0] - 0.5m * this[0].Slot * bold, Program.Stations[Start].JSONCoord[1] - 0.5m * this[0].Slot * bold],
                };
                decimal[] JSONEndCoord = Roads[^1].Item1.StraightRoad.Direction switch
                {
                    0 => [Program.Stations[End].JSONCoord[0] + this[0].Slot * bold, Program.Stations[End].JSONCoord[1]],
                    1 => [Program.Stations[End].JSONCoord[0] + 0.5m * this[0].Slot * bold, Program.Stations[End].JSONCoord[1] - 0.5m * this[0].Slot * bold],
                    2 => [Program.Stations[End].JSONCoord[0], Program.Stations[End].JSONCoord[1] + this[0].Slot * bold],
                    _ /* 3 */ => [Program.Stations[End].JSONCoord[0] - 0.5m * this[0].Slot * bold, Program.Stations[End].JSONCoord[1] - 0.5m * this[0].Slot * bold],
                };
                if (Roads.Count == 1)
                {
                    returnList = [JSONStartCoord, JSONEndCoord];
                }
                else if (Roads.Count > 1)
                {
                    returnList.Add(JSONStartCoord);
                    // 遍历所有 RoutePart
                    for (int i = 1; i < Roads.Count; i++)
                    {
                        RoutePart thisRoutePart = this[i];
                        RoutePart previousRoutePart = this[i - 1];
                        if (thisRoutePart.StraightRoad != previousRoutePart.StraightRoad)
                        {
                            decimal[]? intersection = Utils.GetIntersectionPoint(previousRoutePart.A, previousRoutePart.B, previousRoutePart.C,
                                thisRoutePart.A, thisRoutePart.B, thisRoutePart.C);
                            if (intersection != null) returnList.Add(intersection);
                        }
                        else
                        {
                            if (thisRoutePart.IsForwardOnStraightRoad != previousRoutePart.IsForwardOnStraightRoad)
                            {
                                decimal[] coord1 = thisRoutePart.Direction switch
                                {
                                    0 => [Nodes[i].JSONCoord[0] + this[i - 1].Slot * bold, Nodes[i].JSONCoord[1]],
                                    1 => [Nodes[i].JSONCoord[0] + 0.5m * this[i - 1].Slot * bold, Nodes[i].JSONCoord[1] - 0.5m * this[i - 1].Slot * bold],
                                    2 => [Nodes[i].JSONCoord[0], Nodes[i].JSONCoord[1] + this[i - 1].Slot * bold],
                                    _ /* 3 */ => [Nodes[i].JSONCoord[0] - 0.5m * this[i - 1].Slot * bold, Nodes[i].JSONCoord[1] - 0.5m * this[i - 1].Slot * bold],
                                };
                                decimal[] coord2 = thisRoutePart.Direction switch
                                {
                                    0 => [Nodes[i].JSONCoord[0] + this[i].Slot * bold, Nodes[i].JSONCoord[1]],
                                    1 => [Nodes[i].JSONCoord[0] + 0.5m * this[i].Slot * bold, Nodes[i].JSONCoord[1] - 0.5m * this[i].Slot * bold],
                                    2 => [Nodes[i].JSONCoord[0], Nodes[i].JSONCoord[1] + this[i].Slot * bold],
                                    _ /* 3 */ => [Nodes[i].JSONCoord[0] - 0.5m * this[i].Slot * bold, Nodes[i].JSONCoord[1] - 0.5m * this[i].Slot * bold],
                                };
                                returnList.Add(coord1);
                                returnList.Add(coord2);
                            }
                        }
                    }
                    returnList.Add(JSONEndCoord);
                }
                return returnList;
            }
        }

        [JsonIgnore]
        public List<SvgPoint> SvgCoords => [.. JSONCoords.Select(Utils.CoordJSONToSvg)];

        [JsonIgnore]
        public SvgPath MPGraph
        {
            get
            {
                const float offset = 4f;
                var coords = SvgCoords;
                var path = new SvgPath();
                var segments = new SvgPathSegmentList();

                if (coords.Count < 2)
                    return path;

                static PointF ToPointF(SvgPoint p) => new((float)p.X.Value, (float)p.Y.Value);

                // 向量运算函数
                static PointF Normalize(PointF v)
                {
                    float len = MathF.Sqrt(v.X * v.X + v.Y * v.Y);
                    return len == 0 ? PointF.Empty : new(v.X / len, v.Y / len);
                }

                static PointF Scale(PointF v, float s) => new(v.X * s, v.Y * s);
                static PointF Add(PointF a, PointF b) => new(a.X + b.X, a.Y + b.Y);
                static PointF Sub(PointF a, PointF b) => new(a.X - b.X, a.Y - b.Y);

                var pts = coords.Select(ToPointF).ToList();
                var drawPoints = new List<PointF> { pts[0] }; // 起点保留

                for (int i = 1; i < pts.Count - 1; i++)
                {
                    PointF prev = pts[i - 1];
                    PointF curr = pts[i];
                    PointF next = pts[i + 1];

                    // 方向向量
                    var v1 = Normalize(Sub(curr, prev));
                    var v2 = Normalize(Sub(next, curr));

                    // 生成偏移点
                    var p1 = Add(curr, Scale(v1, -offset));
                    var p2 = Add(curr, Scale(v2, offset));

                    drawPoints.Add(p1);
                    drawPoints.Add(p2);
                }

                drawPoints.Add(pts[^1]); // 终点保留

                // 开始组装 SvgPathSegmentList
                segments.Add(new SvgMoveToSegment(false, drawPoints[0]));

                for (int i = 1; i < drawPoints.Count; i++)
                {
                    if (i % 2 == 0 && i >= 2)
                    {
                        // 每两个偏移点之间加圆角
                        PointF ctrl1 = drawPoints[i - 2]; // 前线段终点
                        PointF ctrl2 = drawPoints[i];     // 当前线段起点
                        PointF mid = pts[i / 2];          // 原始折点作为中控点

                        // 用中点代替精准贝塞尔（可升级）
                        segments.Add(new SvgCubicCurveSegment(false, ctrl1, ctrl2, ctrl2));
                    }
                    else
                    {
                        // 普通折线段
                        segments.Add(new SvgLineSegment(false, drawPoints[i]));
                    }
                }

                path.PathData = segments;
                return path;
            }
        }
    }

    public class RoutePart(Route route, int index)
    {
        public Route Route { get; init; } = route;
        public int Index { get; init; } = index;
        public Road Road => Route.Roads[Index].Item1;
        public bool IsForwardOnStraightRoad => Route.Roads[Index].IsForward == Road.IsForwardOnStraightRoad;
        public StraightRoad StraightRoad => Road.StraightRoad;
        public int Slot
        {
            get
            {
                foreach (var kv in StraightRoad.SlotMatrix)
                {
                    if (kv.Value.Contains(this))
                        return kv.Key;
                }

                // 输出调试信息
                Debug.WriteLine($"[Slot Error] 当前 RoutePart {Route.Id}[{Index}] 未找到卡槽");
                Debug.WriteLine($"当前 SlotMatrix 在道路此侧（${(IsForwardOnStraightRoad ? "右" : "左")}侧）: 共 {StraightRoad.SlotMatrix.Where(x => IsForwardOnStraightRoad ? x.Key > 0 : x.Key < 0).Count()} 个 slot");

                throw new KeyNotFoundException("在 SlotMatrix 中找不到当前 RoutePart。");
            }
        }
        public int Direction => StraightRoad.Direction;
        public decimal A => StraightRoad.A;
        public decimal B => StraightRoad.B;
        public decimal C => StraightRoad.COfSlot(Slot);
    }
}
