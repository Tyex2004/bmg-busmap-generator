using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using System.Windows;
using System.Net.Security;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace BusMapGenerator
{
    internal class ManagementTools
    {
        // 选择工具：拖拽松手后，获取 JSON 选择框执行
        public static void SelectNodes()
        {
            decimal x1 = Program.JSONStartPoint[0];
            decimal y1 = Program.JSONStartPoint[1];
            decimal x2 = Program.JSONEndPoint[0];
            decimal y2 = Program.JSONEndPoint[1];
            decimal xMin = Math.Min(x1, x2);
            decimal xMax = Math.Max(x1, x2);
            decimal yMin = Math.Min(y1, y2);
            decimal yMax = Math.Max(y1, y2);
            List<int> selectedNodes = [];
            foreach (Node node in Program.Nodes.Values)
            {
                decimal x = node.JSONCoord[0];
                decimal y = node.JSONCoord[1];
                if (xMin <= x && x <= xMax && yMin <= y && y <= yMax)
                {
                    selectedNodes.Add(node.Id);
                }
            }
            Program.SelectedNodesIds = selectedNodes;
        }

        // 道路节点移动工具：输入移动的道路节点编号，根据 Program.JSONMove 进行移动，移动距离不超过各方向可移动的最大距离
        public static void MoveNode(int nodeId)
        {
            Node node = Program.Nodes[nodeId];
            int moveDirection = Program.JSONMove.Item1;
            decimal moveDistance = Program.JSONMove.Item2;
            if (node.NodeMovingCanMoveDistance[moveDirection] != -1)
            {
                moveDistance = Math.Min(Program.JSONMove.Item2, node.NodeMovingCanMoveDistance[moveDirection]);
            }
            if (moveDistance != 0)
            {
                if (Program.Map != null) Utils.ClearDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "undone"));
                decimal[] targetCoord = Utils.GetTargetCoord(node.JSONCoord, moveDirection, moveDistance);
                Debug.WriteLine($"node.JSONCoord = [{node.JSONCoord[0]}, {node.JSONCoord[1]}], targetCoord = [{targetCoord[0]}, {targetCoord[1]}]");
                Program.Nodes[nodeId].JSONCoord = targetCoord;
                Utils.BackupData("MoveNode");
                DataSaver.Save();
            }
        }

        // 道路拉出工具：输入拉出道路的开始的道路节点编号
        public static void PullRoad(int nodeId)
        {
            Node node = Program.Nodes[nodeId];
            int moveDirection = Program.JSONMove.Item1;
            decimal moveDistance = Program.JSONMove.Item2;
            if (node.RoadsId[moveDirection] != -1) moveDistance = 0;
            if (moveDistance > 3)
            {
                if (Program.MouseNearElement.Type == BMGElementTypes.Node && Program.MouseNearElement.Id != nodeId)
                {
                    decimal[] targetcoord = Program.Nodes[Program.MouseNearElement.Id].JSONCoord;
                    if (Utils.AreOnDirectionOfEachOther(node.JSONCoord, targetcoord))
                    {
                        int nextRoadNewId = Road.NextNewId;
                        Program.Roads[nextRoadNewId] = new() { Id = nextRoadNewId, NodesId = [node.Id, Program.MouseNearElement.Id] };
                    }
                }
                else
                {
                    decimal[] targetCoord = Utils.GetTargetCoord(node.JSONCoord, moveDirection, moveDistance);
                    int nextNodeNewId = Node.NextNewId;
                    int nextRoadNewId = Road.NextNewId;
                    Node nextNode = new() { Id = nextNodeNewId, JSONCoord = targetCoord };
                    Program.Nodes[nextNodeNewId] = nextNode;
                    Program.Roads[nextRoadNewId] = new() { Id = nextRoadNewId, NodesId = [node.Id, nextNode.Id] };
                }
                if (Program.Map != null) Utils.ClearDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "undone"));
                Utils.BackupData("PullRoad");
                DataSaver.Save();
            }
        }

        // 道路节点删除工具：输入删除的道路节点编号
        public static void DeleteNode(int nodeId)
        {
            Node node = Program.Nodes[nodeId];
            if (node.IsRoadStartOrEnd)
            {
                int roadId = node.RoadsId.First(id => id != -1);
                foreach (int stationId in Program.Roads[roadId].StationsId)
                {
                    Program.Stations.Remove(stationId);
                }
                Program.Roads.Remove(roadId);
                Program.Nodes.Remove(nodeId);
                if (Program.Map != null) Utils.ClearDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "undone"));
                Utils.BackupData("DeleteNode");
                DataSaver.Save();
            }
            else if (node.IsStraight)
            {
                Road[] roads = node.RoadsId.Where(id => id != -1).Take(2).Select(id => Program.Roads[id]).ToArray();
                int roadsIdToRemove0 = roads[0].Id;
                int roadsIdToRemove1 = roads[1].Id;
                Dictionary<int, decimal> stations1OnRoadPos = roads[0].StationsId.ToDictionary(id => id, id => Program.Stations[id].OnRoadPos);
                Dictionary<int, decimal> stations2OnRoadPos = roads[1].StationsId.ToDictionary(id => id, id => Program.Stations[id].OnRoadPos);
                decimal nodePos = Program.Roads[roadsIdToRemove0].Length.Coefficient / (Program.Roads[roadsIdToRemove0].Length.Coefficient + Program.Roads[roadsIdToRemove1].Length.Coefficient);
                int nodeId1 = roads[0].NodesId.First(id => id != nodeId);
                int nodeId2 = roads[1].NodesId.First(id => id != nodeId);
                if (node.Id != Program.Roads[roadsIdToRemove0].NodesId[1]) foreach (var kvp in stations1OnRoadPos)
                {
                    stations1OnRoadPos[kvp.Key] = 1 - kvp.Value;
                    Program.Stations[kvp.Key].ChangeSide();
                }
                if (node.Id != Program.Roads[roadsIdToRemove1].NodesId[0]) foreach (var kvp in stations2OnRoadPos)
                {
                    stations2OnRoadPos[kvp.Key] = 1 - kvp.Value;
                    Program.Stations[kvp.Key].ChangeSide();
                }
                foreach (var kvp in stations1OnRoadPos)
                {
                    stations1OnRoadPos[kvp.Key] *= nodePos;
                }
                foreach (var kvp in stations2OnRoadPos)
                {
                    stations2OnRoadPos[kvp.Key] = nodePos + stations2OnRoadPos[kvp.Key] * (1 - nodePos);
                }
                int newRoadId = Road.NextNewId;
                Program.Roads[newRoadId] = new() { Id = newRoadId, NodesId = [nodeId1, nodeId2] };
                foreach (var kv in stations1OnRoadPos.Concat(stations2OnRoadPos))
                {
                    Program.Stations[kv.Key].RoadId = newRoadId;
                    Program.Stations[kv.Key].OnRoadPos = kv.Value;
                }
                Program.Roads.Remove(roadsIdToRemove0);
                Program.Roads.Remove(roadsIdToRemove1);
                Program.Nodes.Remove(nodeId);
                if (Program.Map != null) Utils.ClearDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "undone"));
                Utils.BackupData("DeleteNode");
                DataSaver.Save();
            }
            else if (node.Level == 1)
            {
                Program.Nodes.Remove(nodeId);
                if (Program.Map != null) Utils.ClearDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "undone"));
                Utils.BackupData("DeleteNode");
                DataSaver.Save();
            }
        }

        // 道路移动工具：输入移动的道路编号，通过移动关联道路节点和同一条直线上所有道路的关联道路节点实现移动
        public static void MoveRoad(int roadId)
        {
            Road road = Program.Roads[roadId];
            Dictionary<Node, decimal[]> moveDict = road.DisplacementWhileMovingTargetNodes;
            if (moveDict.Count != 0)
            {
                foreach (KeyValuePair<Node, decimal[]> movePair in moveDict)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        movePair.Key.JSONCoord[i] += movePair.Value[i];
                    }
                }
                if (Program.Map != null) Utils.ClearDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "undone"));
                Utils.BackupData("MoveRoad");
                DataSaver.Save();
            }
        }

        // 道路节点插入工具（重载1）：输入两个道路编号，给它们的交点创建新的道路节点
        public static void InsertNode(int roadId1, int roadId2)
        {
            Road road1 = Program.Roads[roadId1];
            Road road2 = Program.Roads[roadId2];
            Node node11 = Program.Nodes[road1.NodesId[0]];
            Node node12 = Program.Nodes[road1.NodesId[1]];
            Node node21 = Program.Nodes[road2.NodesId[0]];
            Node node22 = Program.Nodes[road2.NodesId[1]];

            decimal[]? intersection = Utils.GetTwoRoadsIntersection(road1, road2);
            if (intersection == null) return;

            // 已存在交点则跳过
            if (Program.Nodes.Values.Any(n => n.JSONCoord.SequenceEqual(intersection)))
                return;

            // 创建新交点节点
            Node newNode = new() { Id = Node.NextNewId, JSONCoord = intersection };
            Program.Nodes[newNode.Id] = newNode;

            // 计算比例 nodePos
            decimal dist11 = new Distance(intersection, node11.JSONCoord).Coefficient;
            decimal dist12 = new Distance(intersection, node12.JSONCoord).Coefficient;
            decimal dist21 = new Distance(intersection, node21.JSONCoord).Coefficient;
            decimal dist22 = new Distance(intersection, node22.JSONCoord).Coefficient;
            decimal nodePos1 = dist11 + dist12 == 0 ? 0.5m : dist11 / (dist11 + dist12);
            decimal nodePos2 = dist21 + dist22 == 0 ? 0.5m : dist21 / (dist21 + dist22);

            // 保存原路的站点
            var stationGroup = Program.Stations
                .Where(s => s.Value.RoadId == roadId1 || s.Value.RoadId == roadId2)
                .Select(s => s.Value)
                .ToList();

            // 删除旧道路
            Program.Roads.Remove(roadId1);
            Program.Roads.Remove(roadId2);

            // 创建新道路并映射旧道路与新道路的拆分关系
            Dictionary<int, (int lowRoadId, int highRoadId, decimal splitPos)> splitMap = [];

            (int oldId, Node start, Node end, decimal pos)[] roadSplits =
            [
                (roadId1, node11, node12, nodePos1),
                (roadId2, node21, node22, nodePos2)
            ];

            foreach (var (oldId, startNode, endNode, splitPos) in roadSplits)
            {
                int lowRoadId = Road.NextNewId;
                Road roadLow = new() { Id = lowRoadId, NodesId = [startNode.Id, newNode.Id] };
                Program.Roads[lowRoadId] = roadLow;

                int highRoadId = Road.NextNewId;
                Road roadHigh = new() { Id = highRoadId, NodesId = [newNode.Id, endNode.Id] };
                Program.Roads[highRoadId] = roadHigh;

                splitMap[oldId] = (lowRoadId, highRoadId, splitPos);
            }

            // 重新分配站点
            foreach (var s in stationGroup)
            {
                var (lowId, highId, splitPos) = splitMap[s.RoadId];

                if (s.OnRoadPos <= splitPos)
                {
                    s.RoadId = lowId;
                    s.OnRoadPos = splitPos == 0 ? 0 : s.OnRoadPos / splitPos;
                }
                else
                {
                    s.RoadId = highId;
                    s.OnRoadPos = (s.OnRoadPos - splitPos) / (1 - splitPos);
                }
            }

            if (Program.Map != null)
            {
                Utils.ClearDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "undone"));
            }

            Utils.BackupData("InsertNode");
            DataSaver.Save();
        }



        // 道路节点插入工具（重载2）：输入一个道路编号，计算新的道路节点
        public static void InsertNode(int roadId)
        {
            Road road = Program.Roads[roadId];
            decimal[]? foot = Utils.GetFootOfPerpendicular(Program.JSONEndPoint, road);
            if (foot != null)
            {
                // 防止节点重复
                foreach (var node in Program.Nodes.Values)
                {
                    if (node.JSONCoord.SequenceEqual(foot)) return;
                }
                Node newNode = new() { Id = Node.NextNewId, JSONCoord = foot };  // 创建新道路节点
                Program.Nodes[Node.NextNewId] = newNode;  // 放入字典
                int nodeA = road.NodesId[0];
                int nodeB = road.NodesId[1];
                // 拷贝原始站点
                Dictionary<int, Station> stations = Program.Stations
                    .Where(p => p.Value.RoadId == roadId)
                    .ToDictionary(p => p.Key, p => p.Value);
                Program.Roads.Remove(roadId);  // 删除原有道路
                // 创建前段道路
                int roadId1 = Road.NextNewId;
                Road road1 = new() { Id = roadId1, NodesId = [nodeA, newNode.Id] };
                Program.Roads[roadId1] = road1;

                // 创建后段道路
                int roadId2 = Road.NextNewId;
                Road road2 = new() { Id = roadId2, NodesId = [newNode.Id, nodeB] };
                Program.Roads[roadId2] = road2;

                // 计算 nodePos
                decimal distance1 = new Distance(foot, Program.Nodes[nodeA].JSONCoord).Coefficient;
                decimal distance2 = new Distance(foot, Program.Nodes[nodeB].JSONCoord).Coefficient;
                decimal nodePos = distance1 / (distance1 + distance2);

                foreach (var station in stations.Values)
                {
                    if (station.OnRoadPos <= nodePos)
                    {
                        station.RoadId = road1.Id;
                        station.OnRoadPos /= nodePos;
                    }
                    else
                    {
                        station.RoadId = road2.Id;
                        station.OnRoadPos = (station.OnRoadPos - nodePos) / (1 - nodePos);
                    }
                }
                if (Program.Map != null) Utils.ClearDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "undone"));
                Utils.BackupData("InsertNode");
                DataSaver.Save();
            }
        }

        // 道路删除工具：输入一个道路编号，从字典移除它
        public static void DeleteRoad(int roadId)
        {
            List<int> IdOfNodeNeedToRemove = [];
            List<int> IdOfStationNeedToRemove = [];
            foreach (Node node in Program.Roads[roadId].Nodes) if (node.RoadsId.Where(id => id != -1).Count() == 1) IdOfNodeNeedToRemove.Add(node.Id);
            foreach (int nodeId in IdOfNodeNeedToRemove) Program.Nodes.Remove(nodeId);
            foreach (Station station in Program.Stations.Values) if (station.RoadId == roadId) IdOfStationNeedToRemove.Add(station.Id);
            foreach (int stationId in IdOfStationNeedToRemove) Program.Stations.Remove(stationId);
            Program.Roads.Remove(roadId);
            if (Program.Map != null) Utils.ClearDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "undone"));
            Utils.BackupData("DeleteRoad");
            DataSaver.Save();
        }

        // 车站移动工具：输入移动的车站编号，进行移动
        public static void MoveStation(int stationId)
        {
            Station station = Program.Stations[stationId];
            if (Program.WPFMoveDistance > 4)
            {
                station.JSONCoord = Program.JSONEndPoint;
                if (Program.Map != null) Utils.ClearDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "undone"));
                Utils.BackupData("MoveStation");
                DataSaver.Save();
            }
        }

        // 站名设置工具：输入车站编号，弹窗设置站名
        public static void SetStationName(int stationId)
        {
            if (!Program.Stations.TryGetValue(stationId, out var station)) return;
            SetStationNameWindow window = new(station)
            {
                Owner = Application.Current.MainWindow
            };
            bool? result = window.ShowDialog();
            if (result == true)
            {
                // 已在窗口内部完成赋值，这里可选再保存一次
                if (Program.Map != null) Utils.ClearDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "undone"));
                Utils.BackupData("MoveStation");
                DataSaver.Save();
            }
        }

        // 站点删除工具：输入车站编号，从字典移除它
        public static void DeleteStation(int stationId)
        {
            Program.Stations.Remove(stationId);
            if (Program.Map != null) Utils.ClearDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "undone"));
            Utils.BackupData("DeleteStation");
            DataSaver.Save();
        }

        // 站点添加工具：输入道路编号，在相应位置创建新的站点，并弹窗设置站名
        public static void AddStation(int roadId)
        {
            int stationId = Station.NextNewId;
            Station station = new() { Id = stationId, Name = "新站点", EnName = "New Station", RoadId = roadId, JSONCoord = Program.JSONEndPoint };
            Program.Stations[stationId] = station;
            SetStationNameWindow window = new(station)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            // 已在窗口内部完成赋值，这里可选再保存一次
            if (Program.Map != null) Utils.ClearDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "undone"));
            Utils.BackupData("AddStation");
            DataSaver.Save();
        }

        // 站名标注方向改变工具：输入车站编号，station.Side 在 left 和 right 之间切换
        public static void ChangeStationMarkerSide(int stationId)
        {
            if (!Program.Stations.TryGetValue(stationId, out var station)) return;
            if (station.Side == "left")
            {
                station.Side = "right";
            }
            else
            {
                station.Side = "left";
            }
            if (Program.Map != null) Utils.ClearDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "undone"));
            Utils.BackupData("ChangeStationMarkerSide");
            DataSaver.Save();
        }

        // 撤销工具：把 mapDir 的数据移动到 undonePath，把 backupPath 的数据移动到 mapDir
        public static void Undo()
        {
            if (Program.Map != null)
            {
                Debug.WriteLine("执行撤销");
                string mapName = Program.Map;
                // 加载路径
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string mapDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", mapName);      // 已经确保存在
                string backupDir = Path.Combine(mapDir, "backup");  // 已经确保存在
                string undoneDir = Path.Combine(mapDir, "undone");  // 计算并创建 undonePath
                string[] directories = Directory.GetDirectories(backupDir);
                // 先确保 backupDir 内部不为空
                if (directories.Length > 0)
                {
                    // backupDir 排序
                    List<string> sortedDirectories =
                        (
                            from directory in directories
                            let timestamper = Utils.ExtractTimestampOfBackup(Path.GetFileName(directory))
                            where timestamper != null
                            orderby timestamper descending
                            select directory
                        )
                        .ToList();
                    string targetDirectory = sortedDirectories[0];
                    string toolName = targetDirectory.Split('-')[^1];  // toolName 从最新的 backupPath 获取
                    string undonePath = Path.Combine(undoneDir, $"data-{timestamp}-{toolName}");
                    Directory.CreateDirectory(undonePath);
                    // 计算 backupPath
                    string backupPath = Path.Combine(backupDir, targetDirectory);
                    // 执行移动
                    Utils.MoveData(mapDir, undonePath);
                    Utils.MoveData(backupPath, mapDir);
                    Directory.Delete(backupPath, true);
                }
            }
        }

        // 重做工具：把 mapDir 的数据移动到 backupPath，把 undonePath 的数据移动到 mapDir
        public static void Redo()
        {
            if (Program.Map != null)
            {
                Debug.WriteLine("重做刚才撤销的工具");
                string mapName = Program.Map;
                // 加载路径
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string mapDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", mapName);       // 已经确保存在
                string backupDir = Path.Combine(mapDir, "backup");   // 已经确保存在
                string undoneDir = Path.Combine(mapDir, "undone");   // 计算并创建 backupPath
                string[] directories = Directory.GetDirectories(undoneDir);
                if (directories.Length > 0)
                {
                    List<string> sortedDirectories =
                        (
                            from directory in directories
                            let timestamper = Utils.ExtractTimestampOfUndone(Path.GetFileName(directory))
                            where timestamper != null
                            orderby timestamper descending
                            select directory
                        )
                        .ToList();
                    string targetDirectory = sortedDirectories[0];
                    string toolName = targetDirectory.Split('-')[^1];  // toolName 从最新的 undonePath 获取
                    string backupPath = Path.Combine(backupDir, $"data-{timestamp}-before-{toolName}");
                    Directory.CreateDirectory(backupPath);
                    // 计算 undonePath
                    string undonePath = Path.Combine(undoneDir, targetDirectory);
                    // 执行复制
                    Utils.MoveData(mapDir, backupPath);
                    Utils.MoveData(undonePath, mapDir);
                    Directory.Delete(undonePath, true);
                }
            }
        }
    }
    enum ManagementTool
    {
        None,
        MoveNode,
        PullRoad,
        DeleteNode,
        MoveRoad,
        InsertNode,
        DeleteRoad,
        MoveStation,
        SetStationsName,
        DeleteStation,
        AddStation,
        ChangeStationMarkerSide
    }
}
