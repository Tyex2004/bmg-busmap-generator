using Newtonsoft.Json;
using SkiaSharp;
using Svg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BusMapGenerator
{
    public class Road
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("nodes")]
        public int[] NodesId { get; set; } = new int[2];

        [JsonIgnore]
        public Node[] Nodes => NodesId.Select(nodesId => Program.Nodes[nodesId]).ToArray();

        [JsonIgnore]
        public StraightRoad StraightRoad
        {
            get
            {
                foreach (StraightRoad straightRoad in Program.StraightRoads)
                {
                    if (straightRoad.RoadIds.Contains(Id)) return straightRoad;
                }
                return new();
            }
        }

        [JsonIgnore]
        public decimal[] JSONCoordStart => Nodes[0].JSONCoord;

        [JsonIgnore]
        public decimal[] JSONCoordEnd => Nodes[1].JSONCoord;

        [JsonIgnore]
        public SvgPoint SvgCoordStart => Nodes[0].SvgCoord;

        [JsonIgnore]
        public SvgPoint SvgCoordEnd => Nodes[1].SvgCoord;

        [JsonIgnore]
        public SKPoint SKiaCoordStart => Nodes[0].SkiaCoord;

        [JsonIgnore]
        public SKPoint SKiaCoordEnd => Nodes[1].SkiaCoord;

        [JsonIgnore]
        public Point WPFCoordStart => Nodes[0].WPFCoord;

        [JsonIgnore]
        public Point WPFCoordEnd => Nodes[1].WPFCoord;

        [JsonIgnore]
        public int Direction  // 从零向北开始顺时针
        {
            get
            {
                decimal dx = JSONCoordEnd[0] - JSONCoordStart[0]; // 东西方向分量
                decimal dy = JSONCoordEnd[1] - JSONCoordStart[1]; // 南北方向分量（上北）

                // 计算 atan2(Δy, Δx)，返回角度（单位：度）
                double angleRad = Math.Atan2((double)dy, (double)dx);  // [-π, π]
                double angleDeg = angleRad * (180.0 / Math.PI);        // 转换为角度

                // 将角度转换为从正东开始顺时针的角度
                // 例如：正东=0，正北=90，正西=180/-180，正南=-90
                double clockwiseFromEast = (90 - angleDeg + 360) % 360;

                // 以每45度为一个方向区间，四舍五入后再模8，得到方向编号
                int direction = (int)Math.Round(clockwiseFromEast / 45.0) % 8;

                return direction; // 0=北，1=东北，2=东，3=东南，4=南，5=西南，6=西，7=西北
            }
        }

        [JsonIgnore]
        public Distance Length => new(JSONCoordStart, JSONCoordEnd);

        [JsonIgnore]
        public static int NextNewId
        {
            get
            {
                HashSet<int> existingIds = new(Program.Roads.Keys);
                int i = 1;
                while (existingIds.Contains(i))
                {
                    i++;
                }
                return i;
            }
        }

        [JsonIgnore]
        public int[] CanMoveDirections => [(Direction + 2) % 8, (Direction + 6) % 8];

        [JsonIgnore]
        public Dictionary<Node, decimal[]> DisplacementWhileMovingTargetNodes
        {
            // 道路节点 : 位移
            get
            {
                Dictionary<Node, decimal[]> returnDictionary = [];
                decimal distance = 0m;
                // 横路
                if (StraightRoad.Direction == 2)
                {
                    // 只允许向上、向下移动
                    if (Program.JSONMove.Item1 is 0 or 7 or 1) distance = Program.JSONMove.Item2;
                    else if (Program.JSONMove.Item1 is 4 or 5 or 3) distance = -Program.JSONMove.Item2;
                    else return [];
                    // 遍历这条路上所有的道路节点，给它们添加进字典，并判断位移阈值
                    foreach (Node node in StraightRoad.NodeIds.Select(nodeId => Program.Nodes[nodeId]))
                    {
                        // 1 级道路节点（非相交路）
                        if (node.Level == 1) returnDictionary[node] = [0m, 1m];
                        // 2 级道路节点（双相交路，如转弯、十字路口和丁字路口）
                        if (node.Level == 2)
                        {
                            // 和纵路相交
                            if (node.RoadsId[0] != -1 || node.RoadsId[4] != -1)
                            {
                                returnDictionary[node] = [0m, 1m];
                                if (distance > 0 && node.RoadMovingCanMoveDistance[0] != -1) distance = Math.Min(distance, node.RoadMovingCanMoveDistance[0]);
                                else if (distance < 0 && node.RoadMovingCanMoveDistance[4] != -1) distance = Math.Max(distance, -node.RoadMovingCanMoveDistance[4]);
                            }
                            // 和上斜路相交
                            else if (node.RoadsId[1] != -1 || node.RoadsId[5] != -1)
                            {
                                returnDictionary[node] = [1m, 1m];
                                if (distance > 0 && node.RoadMovingCanMoveDistance[1] != -1) distance = Math.Min(distance, node.RoadMovingCanMoveDistance[1]);
                                else if (distance < 0 && node.RoadMovingCanMoveDistance[5] != -1) distance = Math.Max(distance, -node.RoadMovingCanMoveDistance[5]);
                            }
                            // 和下斜路相交
                            else if (node.RoadsId[3] != -1 || node.RoadsId[7] != -1)
                            {
                                returnDictionary[node] = [-1m, 1m];
                                if (distance > 0 && node.RoadMovingCanMoveDistance[7] != -1) distance = Math.Min(distance, node.RoadMovingCanMoveDistance[7]);
                                else if (distance < 0 && node.RoadMovingCanMoveDistance[3] != -1) distance = Math.Max(distance, -node.RoadMovingCanMoveDistance[3]);
                            }
                        }
                        // 3 级道路节点（复杂路口）
                        else
                        {
                            StraightRoad straightRoad1 = node.StraightRoads[1];  // 相交的上斜路
                            StraightRoad straightRoad2 = node.StraightRoads[3];  // 相交的下斜路
                            returnDictionary[node] = [0m, 1m];
                            // 和纵路相交
                            if (node.RoadsId[0] != -1 || node.RoadsId[4] != -1)
                            {
                                if (distance > 0 && node.RoadMovingCanMoveDistance[0] != -1) distance = Math.Min(distance, node.RoadMovingCanMoveDistance[0]);
                                else if (distance < 0 && node.RoadMovingCanMoveDistance[4] != -1) distance = Math.Max(distance, -node.RoadMovingCanMoveDistance[4]);
                            }
                            // 和上斜路相交，带动上斜路上移或下移
                            if (node.RoadsId[1] != -1 || node.RoadsId[5] != -1)
                            {
                                // 遍历相交上斜路上的道路节点
                                foreach (Node node1 in straightRoad1.NodeIds.Select(nodeId => Program.Nodes[nodeId]))
                                {
                                    // 1 级道路节点
                                    if (node1.Level == 1) returnDictionary[node1] = [0m, 1m];
                                    // 2 级道路节点
                                    else if (node1.Level == 2)
                                    {
                                        // 和横路相交
                                        if (node1.RoadsId[2] != -1 || node1.RoadsId[6] != -1)
                                        {
                                            returnDictionary[node1] = [-1m, 0m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[6] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[6]);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[2] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[2]);
                                        }
                                        // 和纵路相交
                                        if (node1.RoadsId[0] != -1 || node1.RoadsId[4] != -1)
                                        {
                                            returnDictionary[node1] = [0m, 1m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[0] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[0]);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[4] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[4]);
                                        }
                                        // 和下斜路相交
                                        else if (node1.RoadsId[3] != -1 || node1.RoadsId[7] != -1)
                                        {
                                            returnDictionary[node1] = [-0.5m, 0.5m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[7] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[7] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[3] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[3] * 2m);
                                        }
                                    }
                                    // 3 级道路节点，不允许移动
                                    else if (node1.Id != node.Id) return [];
                                }
                            }
                            // 和下斜路相交，带动下斜路上移或下移
                            if (node.RoadsId[3] != -1 || node.RoadsId[7] != -1)
                            {
                                // 遍历相交下斜路上的道路节点
                                foreach (Node node1 in straightRoad2.NodeIds.Select(nodeId => Program.Nodes[nodeId]))
                                {
                                    // 1 级道路节点
                                    if (node1.Level == 1) returnDictionary[node1] = [0m, 1m];
                                    // 2 级道路节点
                                    else if (node1.Level == 2)
                                    {
                                        // 和横路相交
                                        if (node1.RoadsId[2] != -1 || node1.RoadsId[6] != -1)
                                        {
                                            returnDictionary[node1] = [1m, 0m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[2] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[2]);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[6] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[6]);
                                        }
                                        // 和纵路相交
                                        if (node1.RoadsId[0] != -1 || node1.RoadsId[4] != -1)
                                        {
                                            returnDictionary[node1] = [0m, 1m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[0] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[0]);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[4] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[4]);
                                        }
                                        // 和上斜路相交
                                        else if (node1.RoadsId[1] != -1 || node1.RoadsId[5] != -1)
                                        {
                                            returnDictionary[node1] = [0.5m, 0.5m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[1] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[1] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[5] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[5] * 2m);
                                        }
                                    }
                                    // 3 级道路节点，不允许移动
                                    else if (node1.Id != node.Id) 
                                    {
                                        Debug.WriteLine($"前批次的 3 级道路节点 {node.Id} 联动的后批次 3 级道路节点 {node1.Id} 阻止道路移动。");
                                        return [];
                                    }
                                }
                            }
                        }
                    }
                }
                // 纵路
                if (StraightRoad.Direction == 0)
                {
                    // 只允许向左、向右移动
                    if (Program.JSONMove.Item1 is 2 or 1 or 3) distance = Program.JSONMove.Item2;
                    else if (Program.JSONMove.Item1 is 6 or 5 or 7) distance = -Program.JSONMove.Item2;
                    else return [];
                    // 遍历这条路上所有的道路节点，给它们添加进字典，并判断位移阈值
                    foreach (Node node in StraightRoad.NodeIds.Select(nodeId => Program.Nodes[nodeId]))
                    {
                        // 1 级道路节点（非相交路）
                        if (node.Level == 1) returnDictionary[node] = [1m, 0m];
                        // 2 级道路节点（双相交路，如转弯、十字路口和丁字路口）
                        if (node.Level == 2)
                        {
                            // 和横路相交
                            if (node.RoadsId[2] != -1 || node.RoadsId[6] != -1)
                            {
                                returnDictionary[node] = [1m, 0m];
                                if (distance > 0 && node.RoadMovingCanMoveDistance[2] != -1) distance = Math.Min(distance, node.RoadMovingCanMoveDistance[2]);
                                else if (distance < 0 && node.RoadMovingCanMoveDistance[6] != -1) distance = Math.Max(distance, -node.RoadMovingCanMoveDistance[6]);
                            }
                            // 和上斜路相交
                            else if (node.RoadsId[1] != -1 || node.RoadsId[5] != -1)
                            {
                                returnDictionary[node] = [1m, 1m];
                                if (distance > 0 && node.RoadMovingCanMoveDistance[1] != -1) distance = Math.Min(distance, node.RoadMovingCanMoveDistance[1]);
                                else if (distance < 0 && node.RoadMovingCanMoveDistance[5] != -1) distance = Math.Max(distance, -node.RoadMovingCanMoveDistance[5]);
                            }
                            // 和下斜路相交
                            else if (node.RoadsId[3] != -1 || node.RoadsId[7] != -1)
                            {
                                returnDictionary[node] = [1m, -1m];
                                if (distance > 0 && node.RoadMovingCanMoveDistance[3] != -1) distance = Math.Min(distance, node.RoadMovingCanMoveDistance[3]);
                                else if (distance < 0 && node.RoadMovingCanMoveDistance[7] != -1) distance = Math.Max(distance, -node.RoadMovingCanMoveDistance[7]);
                            }
                        }
                        // 3 级道路节点（复杂路口）
                        else
                        {
                            StraightRoad straightRoad1 = node.StraightRoads[1];  // 相交的上斜路
                            StraightRoad straightRoad2 = node.StraightRoads[3];  // 相交的下斜路
                            returnDictionary[node] = [1m, 0m];
                            // 和横路相交
                            if (node.RoadsId[2] != -1 || node.RoadsId[6] != -1)
                            {
                                if (distance > 0 && node.RoadMovingCanMoveDistance[2] != -1) distance = Math.Min(distance, node.RoadMovingCanMoveDistance[2]);
                                else if (distance < 0 && node.RoadMovingCanMoveDistance[6] != -1) distance = Math.Max(distance, -node.RoadMovingCanMoveDistance[6]);
                            }
                            // 和上斜路相交，带动上斜路左移或右移
                            if (node.RoadsId[1] != -1 || node.RoadsId[5] != -1)
                            {
                                // 遍历相交上斜路上的道路节点
                                foreach (Node node1 in straightRoad1.NodeIds.Select(nodeId => Program.Nodes[nodeId]))
                                {
                                    // 1 级道路节点
                                    if (node1.Level == 1) returnDictionary[node1] = [1m, 0m];
                                    // 2 级道路节点
                                    else if (node1.Level == 2)
                                    {
                                        // 和横路相交
                                        if (node1.RoadsId[2] != -1 || node1.RoadsId[6] != -1)
                                        {
                                            returnDictionary[node1] = [1m, 0m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[2] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[2]);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[6] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[6]);
                                        }
                                        // 和纵路相交
                                        if (node1.RoadsId[0] != -1 || node1.RoadsId[4] != -1)
                                        {
                                            returnDictionary[node1] = [0m, -1m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[4] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[4]);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[0] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[0]);
                                        }
                                        // 和下斜路相交
                                        else if (node1.RoadsId[3] != -1 || node1.RoadsId[7] != -1)
                                        {
                                            returnDictionary[node1] = [0.5m, -0.5m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[3] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[3] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[7] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[7] * 2m);
                                        }
                                    }
                                    // 3 级道路节点，不允许移动
                                    else if (node1.Id != node.Id) return [];
                                }
                            }
                            // 和下斜路相交，带动下斜路左移或右移
                            if (node.RoadsId[3] != -1 || node.RoadsId[7] != -1)
                            {
                                // 遍历相交下斜路上的道路节点
                                foreach (Node node1 in straightRoad2.NodeIds.Select(nodeId => Program.Nodes[nodeId]))
                                {
                                    // 1 级道路节点
                                    if (node1.Level == 1) returnDictionary[node1] = [1m, 0m];
                                    // 2 级道路节点
                                    else if (node1.Level == 2)
                                    {
                                        // 和横路相交
                                        if (node1.RoadsId[2] != -1 || node1.RoadsId[6] != -1)
                                        {
                                            returnDictionary[node1] = [1m, 0m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[2] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[2]);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[6] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[6]);
                                        }
                                        // 和纵路相交
                                        if (node1.RoadsId[0] != -1 || node1.RoadsId[4] != -1)
                                        {
                                            returnDictionary[node1] = [0m, 1m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[0] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[0]);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[4] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[4]);
                                        }
                                        // 和上斜路相交
                                        else if (node1.RoadsId[1] != -1 || node1.RoadsId[5] != -1)
                                        {
                                            returnDictionary[node1] = [0.5m, 0.5m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[1] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[1] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[5] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[5] * 2m);
                                        }
                                    }
                                    // 3 级道路节点，不允许移动
                                    else if (node1.Id != node.Id) return [];
                                }
                            }
                        }
                    }
                }
                // 上斜路
                if (StraightRoad.Direction == 1)
                {
                    // 只允许向右下、左上移动
                    if (Program.JSONMove.Item1 == 3) distance = Program.JSONMove.Item2 * 2;
                    else if (Program.JSONMove.Item1 is 2 or 4) distance = Program.JSONMove.Item2;
                    else if (Program.JSONMove.Item1 == 7) distance = -Program.JSONMove.Item2 * 2;
                    else if (Program.JSONMove.Item1 is 6 or 0) distance = -Program.JSONMove.Item2;
                    else return [];
                    // 遍历这条路上所有的道路节点，给它们添加进字典，并判断位移阈值
                    foreach (Node node in StraightRoad.NodeIds.Select(nodeId => Program.Nodes[nodeId]))
                    {
                        // 1 级道路节点（非相交路）
                        if (node.Level == 1) returnDictionary[node] = [0.5m, -0.5m];
                        // 2 级道路节点（双相交路，如转弯、十字路口和丁字路口）
                        if (node.Level == 2)
                        {
                            // 和下斜路相交
                            if (node.RoadsId[3] != -1 || node.RoadsId[7] != -1)
                            {
                                returnDictionary[node] = [0.5m, -0.5m];
                                if (distance > 0 && node.RoadMovingCanMoveDistance[3] != -1) distance = Math.Min(distance, node.RoadMovingCanMoveDistance[3] * 2m);
                                else if (distance < 0 && node.RoadMovingCanMoveDistance[7] != -1) distance = Math.Max(distance, -node.RoadMovingCanMoveDistance[7] * 2m);
                            }
                            // 和横路相交
                            else if (node.RoadsId[2] != -1 || node.RoadsId[6] != -1)
                            {
                                returnDictionary[node] = [1m, 0m];
                                if (distance > 0 && node.RoadMovingCanMoveDistance[2] != -1) distance = Math.Min(distance, node.RoadMovingCanMoveDistance[2]);
                                else if (distance < 0 && node.RoadMovingCanMoveDistance[6] != -1) distance = Math.Max(distance, -node.RoadMovingCanMoveDistance[6]);
                            }
                            // 和纵路相交
                            else if (node.RoadsId[0] != -1 || node.RoadsId[4] != -1)
                            {
                                returnDictionary[node] = [0m, -1m];
                                if (distance > 0 && node.RoadMovingCanMoveDistance[4] != -1) distance = Math.Min(distance, node.RoadMovingCanMoveDistance[4]);
                                else if (distance < 0 && node.RoadMovingCanMoveDistance[0] != -1) distance = Math.Max(distance, -node.RoadMovingCanMoveDistance[0]);
                            }
                        }
                        // 3 级道路节点（复杂路口）
                        else
                        {
                            StraightRoad straightRoad1 = node.StraightRoads[2];  // 相交的横路
                            StraightRoad straightRoad2 = node.StraightRoads[0];  // 相交的纵路
                            returnDictionary[node] = [0.5m, -0.5m];
                            // 和横路相交，带动横路下移或上移
                            if (node.RoadsId[2] != -1 || node.RoadsId[6] != -1)
                            {
                                // 遍历相交横路上的道路节点
                                foreach (Node node1 in straightRoad1.NodeIds.Select(nodeId => Program.Nodes[nodeId]))
                                {
                                    // 1 级道路节点
                                    if (node1.Level == 1) returnDictionary[node1] = [0m, -0.5m];
                                    // 2 级道路节点
                                    else if (node1.Level == 2)
                                    {
                                        // 和纵路相交
                                        if (node1.RoadsId[0] != -1 || node1.RoadsId[4] != -1)
                                        {
                                            returnDictionary[node1] = [0m, -0.5m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[4] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[4] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[0] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[0] * 2m);
                                        }
                                        // 和下斜路相交
                                        if (node1.RoadsId[3] != -1 || node1.RoadsId[7] != -1)
                                        {
                                            returnDictionary[node1] = [0.5m, -0.5m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[3] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[3] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[7] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[7] * 2m);
                                        }
                                        // 和上斜路相交
                                        else if (node1.RoadsId[1] != -1 || node1.RoadsId[5] != -1)
                                        {
                                            returnDictionary[node1] = [-0.5m, -0.5m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[5] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[5] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[1] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[1] * 2m);
                                        }
                                    }
                                    // 3 级道路节点，不允许移动
                                    else if (node1.Id != node.Id) return [];
                                }
                            }
                            // 和纵路相交，带动纵路右移或左移
                            if (node.RoadsId[0] != -1 || node.RoadsId[4] != -1)
                            {
                                // 遍历相交纵路上的道路节点
                                foreach (Node node1 in straightRoad2.NodeIds.Select(nodeId => Program.Nodes[nodeId]))
                                {
                                    // 1 级道路节点
                                    if (node1.Level == 1) returnDictionary[node1] = [0.5m, 0m];
                                    // 2 级道路节点
                                    else if (node1.Level == 2)
                                    {
                                        // 和横路相交
                                        if (node1.RoadsId[2] != -1 || node1.RoadsId[6] != -1)
                                        {
                                            returnDictionary[node1] = [0.5m, 0m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[2] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[2] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[6] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[6] * 2m);
                                        }
                                        // 和下斜路相交
                                        if (node1.RoadsId[3] != -1 || node1.RoadsId[7] != -1)
                                        {
                                            returnDictionary[node1] = [0.5m, -0.5m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[3] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[3] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[7] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[7] * 2m);
                                        }
                                        // 和上斜路相交
                                        else if (node1.RoadsId[1] != -1 || node1.RoadsId[5] != -1)
                                        {
                                            returnDictionary[node1] = [0.5m, 0.5m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[1] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[1] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[5] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[5] * 2m);
                                        }
                                    }
                                    // 3 级道路节点，不允许移动
                                    else if (node1.Id != node.Id) return [];
                                }
                            }
                        }
                    }
                }
                // 下斜路
                if (StraightRoad.Direction == 3)
                {
                    // 只允许向右上、左下移动
                    if (Program.JSONMove.Item1 == 1) distance = Program.JSONMove.Item2 * 2;
                    else if (Program.JSONMove.Item1 is 2 or 0) distance = Program.JSONMove.Item2;
                    else if (Program.JSONMove.Item1 == 5) distance = -Program.JSONMove.Item2 * 2;
                    else if (Program.JSONMove.Item1 is 4 or 6) distance = -Program.JSONMove.Item2;
                    else return [];
                    // 遍历这条路上所有的道路节点，给它们添加进字典，并判断位移阈值
                    foreach (Node node in StraightRoad.NodeIds.Select(nodeId => Program.Nodes[nodeId]))
                    {
                        // 1 级道路节点（非相交路）
                        if (node.Level == 1) returnDictionary[node] = [0.5m, 0.5m];
                        // 2 级道路节点（双相交路，如转弯、十字路口和丁字路口）
                        if (node.Level == 2)
                        {
                            // 和上斜路相交
                            if (node.RoadsId[1] != -1 || node.RoadsId[5] != -1)
                            {
                                returnDictionary[node] = [0.5m, 0.5m];
                                if (distance > 0 && node.RoadMovingCanMoveDistance[1] != -1) distance = Math.Min(distance, node.RoadMovingCanMoveDistance[1] * 2m);
                                else if (distance < 0 && node.RoadMovingCanMoveDistance[5] != -1) distance = Math.Max(distance, -node.RoadMovingCanMoveDistance[5] * 2m);
                            }
                            // 和横路相交
                            else if (node.RoadsId[2] != -1 || node.RoadsId[6] != -1)
                            {
                                returnDictionary[node] = [1m, 0m];
                                if (distance > 0 && node.RoadMovingCanMoveDistance[2] != -1) distance = Math.Min(distance, node.RoadMovingCanMoveDistance[2]);
                                else if (distance < 0 && node.RoadMovingCanMoveDistance[6] != -1) distance = Math.Max(distance, -node.RoadMovingCanMoveDistance[6]);
                            }
                            // 和纵路相交
                            else if (node.RoadsId[0] != -1 || node.RoadsId[4] != -1)
                            {
                                returnDictionary[node] = [0m, 1m];
                                if (distance > 0 && node.RoadMovingCanMoveDistance[0] != -1) distance = Math.Min(distance, node.RoadMovingCanMoveDistance[0]);
                                else if (distance < 0 && node.RoadMovingCanMoveDistance[4] != -1) distance = Math.Max(distance, -node.RoadMovingCanMoveDistance[4]);
                            }
                        }
                        // 3 级道路节点（复杂路口）
                        else
                        {
                            StraightRoad straightRoad1 = node.StraightRoads[2];  // 相交的横路
                            StraightRoad straightRoad2 = node.StraightRoads[0];  // 相交的纵路
                            returnDictionary[node] = [0.5m, 0.5m];
                            // 和横路相交，带动横路上移或下移
                            if (node.RoadsId[2] != -1 || node.RoadsId[6] != -1)
                            {
                                // 遍历相交横路上的道路节点
                                foreach (Node node1 in straightRoad1.NodeIds.Select(nodeId => Program.Nodes[nodeId]))
                                {
                                    // 1 级道路节点
                                    if (node1.Level == 1) returnDictionary[node1] = [0m, 0.5m];
                                    // 2 级道路节点
                                    else if (node1.Level == 2)
                                    {
                                        // 和纵路相交
                                        if (node1.RoadsId[0] != -1 || node1.RoadsId[4] != -1)
                                        {
                                            returnDictionary[node1] = [0m, 0.5m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[0] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[0] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[4] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[4] * 2m);
                                        }
                                        // 和下斜路相交
                                        if (node1.RoadsId[3] != -1 || node1.RoadsId[7] != -1)
                                        {
                                            returnDictionary[node1] = [-0.5m, 0.5m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[7] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[7] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[3] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[3] * 2m);
                                        }
                                        // 和上斜路相交
                                        else if (node1.RoadsId[1] != -1 || node1.RoadsId[5] != -1)
                                        {
                                            returnDictionary[node1] = [0.5m, 0.5m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[1] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[1] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[5] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[5] * 2m);
                                        }
                                    }
                                    // 3 级道路节点，不允许移动
                                    else if (node1.Id != node.Id) return [];
                                }
                            }
                            // 和纵路相交，带动纵路右移或左移
                            if (node.RoadsId[0] != -1 || node.RoadsId[4] != -1)
                            {
                                // 遍历相交纵路上的道路节点
                                foreach (Node node1 in straightRoad2.NodeIds.Select(nodeId => Program.Nodes[nodeId]))
                                {
                                    // 1 级道路节点
                                    if (node1.Level == 1) returnDictionary[node1] = [0.5m, 0m];
                                    // 2 级道路节点
                                    else if (node1.Level == 2)
                                    {
                                        // 和横路相交
                                        if (node1.RoadsId[2] != -1 || node1.RoadsId[6] != -1)
                                        {
                                            returnDictionary[node1] = [0.5m, 0m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[2] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[2] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[6] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[6] * 2m);
                                        }
                                        // 和下斜路相交
                                        if (node1.RoadsId[3] != -1 || node1.RoadsId[7] != -1)
                                        {
                                            returnDictionary[node1] = [0.5m, -0.5m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[3] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[3] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[7] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[7] * 2m);
                                        }
                                        // 和上斜路相交
                                        else if (node1.RoadsId[1] != -1 || node1.RoadsId[5] != -1)
                                        {
                                            returnDictionary[node1] = [0.5m, 0.5m];
                                            if (distance > 0 && node1.RoadMovingCanMoveDistance[1] != -1) distance = Math.Min(distance, node1.RoadMovingCanMoveDistance[1] * 2m);
                                            else if (distance < 0 && node1.RoadMovingCanMoveDistance[5] != -1) distance = Math.Max(distance, -node1.RoadMovingCanMoveDistance[5] * 2m);
                                        }
                                    }
                                    // 3 级道路节点，不允许移动
                                    else if (node1.Id != node.Id) return [];
                                }
                            }
                        }
                    }
                }
                foreach (var displacement in returnDictionary.Values)
                {
                    displacement[0] *= distance;
                    displacement[1] *= distance;
                }
                // 保护一条路上的节点顺序
                Dictionary<StraightRoad, List<int>> sortedNodeIdsAfterMovingRoads = Program.StraightRoads.ToDictionary(
                    w => w,
                    w => (w.Direction is 1 or 2 or 3)
                        ? [.. w.NodeIds.OrderBy(x =>
                        {
                            if (returnDictionary.TryGetValue(Program.Nodes[x], out decimal[]? val))
                                return Program.Nodes[x].JSONCoord[0] + val[0];
                            else
                                return Program.Nodes[x].JSONCoord[0];
                        })]
                        : w.NodeIds.OrderBy(x =>
                        {
                            if (returnDictionary.TryGetValue(Program.Nodes[x], out decimal[]? val))
                                return Program.Nodes[x].JSONCoord[1] + val[1];
                            else
                                return Program.Nodes[x].JSONCoord[1];
                        }).ToList()
                );
                foreach (var straightRoad in Program.StraightRoads)
                {
                    if (!straightRoad.SortedNodeIds.SequenceEqual(sortedNodeIdsAfterMovingRoads[straightRoad])) return [];
                }
                return returnDictionary;
            }
        }

        [JsonIgnore]
        public SvgLine RPGraph => new()
        {
            StartX = SvgCoordStart.X,
            StartY = SvgCoordStart.Y,
            EndX = SvgCoordEnd.X,
            EndY = SvgCoordEnd.Y,
            Stroke = Utils.SetColor(200, 0, 200),
            StrokeWidth = 3
        };
    }
}
