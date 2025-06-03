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
            if (node.CanMoveDistance[moveDirection] != -1)
            {
                moveDistance = Math.Min(Program.JSONMove.Item2, node.CanMoveDistance[moveDirection]);
            }
            decimal[] targetCoord = Utils.GetTargetCoord(node.JSONCoord, moveDirection, moveDistance);
            Debug.WriteLine($"node.JSONCoord = [{node.JSONCoord[0]}, {node.JSONCoord[1]}], targetCoord = [{targetCoord[0]}, {targetCoord[1]}]");
            Program.Nodes[nodeId].JSONCoord = targetCoord;
            Utils.BackupData("MoveNode");
            DataSaver.Save();
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
                Debug.WriteLine($"使用道路拉出工具，nodeId = {nodeId}");
                decimal[] targetCoord = Utils.GetTargetCoord(node.JSONCoord, moveDirection, moveDistance);
                Node nextNode = new() { Id = Node.NextNewId, JSONCoord = targetCoord };
                Program.Nodes[Node.NextNewId] = nextNode;
                Program.Roads[Road.NextNewId] = new() { Id = Road.NextNewId, NodesId = [node.Id, nextNode.Id] };
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
                Program.Roads.Remove(roadId);
                Program.Nodes.Remove(nodeId);
                Utils.BackupData("DeleteNode");
                DataSaver.Save();
            }
            else if (node.IsNotRoadEnterence)
            {
                Road[] roads = node.RoadsId.Where(id => id != -1).Take(2).Select(id => Program.Roads[id]).ToArray();
                int nodeId1 = roads[0].NodesId.First(id => id != nodeId);
                int nodeId2 = roads[1].NodesId.First(id => id != nodeId);
                Program.Roads[Road.NextNewId] = new() { Id = Road.NextNewId, NodesId = [nodeId1, nodeId2] };
                Program.Roads.Remove(roads[0].Id);
                Program.Roads.Remove(roads[1].Id);
                Program.Nodes.Remove(nodeId);
            }
        }

        // 撤销工具：把 mapDir 的数据移动到 undonePath，把 backupPath 的数据移动到 mapDir
        public static void Undo(string mapName)
        {
            // 加载路径
            string timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            string mapDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", mapName);      // 已经确保存在
            string backupDir = Path.Combine(mapDir, "backup");  // 已经确保存在
            string undoneDir = Path.Combine(mapDir, "undone");  // 已经确保存在
            // 计算并创建 undonePath
            string[]? directories = Directory.GetDirectories(backupDir);
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
            string toolName = targetDirectory.Split('-')[3];  // toolName 从最新的 backupPath 获取
            string undonePath = Path.Combine(undoneDir, $"data-{timestamp}-{toolName}");
            Directory.CreateDirectory(undonePath);
            // 计算 backupPath
            string backupPath = Path.Combine(backupDir, targetDirectory);
            // 执行移动
            Utils.MoveData(mapDir, undonePath);
            Utils.MoveData(backupPath, mapDir);
            Directory.Delete(backupPath, true);
        }

        // 重做工具：把 mapDir 的数据移动到 backupPath，把 undonePath 的数据移动到 mapDir
        public static void Redo(string mapName)
        {
            // 加载路径
            string timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            string mapDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", mapName);       // 已经确保存在
            string backupDir = Path.Combine(mapDir, "backup");   // 已经确保存在
            string undoneDir = Path.Combine(mapDir, "undone");   // 已经确保存在
            // 计算并创建 backupPath
            string[]? directories = Directory.GetDirectories(undoneDir);
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
            string toolName = targetDirectory.Split('-')[4];  // toolName 从最新的 undonePath 获取
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
    enum ManagementTool
    {
        None,
        MoveNode,
        PullRoad,
        DeleteNode
    }
}
