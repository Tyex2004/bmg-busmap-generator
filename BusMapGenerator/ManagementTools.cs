using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using System.Net.Security;
using OpenTK.Graphics.OpenGL;

namespace BusMapGenerator
{
    internal class ManagementTools
    {
        // 选择工具
        public static void SelectNodes()
        {
            decimal[] startPoint = Program.JSONStartPoint;
            decimal[] endPoint = Program.JSONEndPoint;
            decimal x1 = startPoint[0];
            decimal y1 = startPoint[1];
            decimal x2 = endPoint[0];
            decimal y2 = endPoint[1];
            decimal xMin = Math.Min(x1, x2);
            decimal xMax = Math.Max(x1, x2);
            decimal yMin = Math.Min(y1, y2);
            decimal yMax = Math.Max(y1, y2);
            List<int> selectedNodes = [];
            foreach (Node node in Program.Nodes.Values)
            {
                decimal x = node.Coord[0];
                decimal y = node.Coord[1];
                if (xMin <= x && x <= xMax && yMin <= y && y <= yMax)
                {
                    selectedNodes.Add(node.Id);
                }
            }
            Program.SelectedNodesIds = selectedNodes;
        }

        // 道路节点移动工具：输入移动的 x 值和移动的 y值
        public static void MoveNodes(decimal dx, decimal dy)
        {
            // 备份
            Utils.BackupData("MoveNodes");
            // 移动
            foreach (KeyValuePair<int, Node> node in Program.Nodes)
            {
                if (Program.SelectedNodesIds.Contains(node.Key))
                {
                    node.Value.Coord[0] += dx;
                    node.Value.Coord[1] += dy;
                }
            }
            // 保存
            DataSaver.SaveNodes();
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
}
