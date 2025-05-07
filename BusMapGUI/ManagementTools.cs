using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using System.Net.Security;

namespace BusMapGenerator
{
    internal class ManagementTools
    {
        // 道路节点移动工具
        public static void MoveNodes(string mapName, decimal x1, decimal y1, decimal x2, decimal y2, decimal dx, decimal dy)
        {
            // 备份
            Utils.BackupData(mapName, "MoveNodes");
            // 加载
            Dictionary<int, Node> nodes = DataLoader.LoadNodes(mapName);
            if (nodes == null) return;
            // 移动
            int movedCount = 0;
            foreach (Node node in nodes.Values)
            {
                if (Utils.IsNodeInBox(node, x1, y1, x2, y2))
                {
                    node.Coord[0] += dx;
                    node.Coord[1] += dy;
                    movedCount++;
                }
            }
            // 保存
            DataSaver.SaveNodes(nodes, mapName);
            Console.WriteLine($"[INFO] 移动了 {movedCount} 个道路节点，偏移量 ({dx}, {dy}) ");
        }

        // 撤销工具：把 mapDir 的数据复制到 undonePath，把 backupPath 的数据复制到 mapDir
        public static void Undo(string mapName)
        {
            // 加载路径
            string timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            string mapDir = Path.Combine("data", mapName);      // 已经确保存在
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
            // 执行复制
            Utils.CopyData(mapDir, undonePath);
            Utils.CopyData(backupPath, mapDir);
        }

        // 重做工具：把 mapDir 的数据复制到 backupPath，把 undonePath 的数据复制到 mapDir
        public static void Redo(string mapName)
        {
            // 加载路径
            string timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            string mapDir = Path.Combine("data", mapName);       // 已经确保存在
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
            Utils.CopyData(mapDir, backupPath);
            Utils.CopyData(undonePath, mapDir);
        }
    }
}
