using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace BusMapGenerator
{
    internal class ManagementTools
    {
        // 获取数据目录
        private static string GetDataDirectory(string mapName) => Path.Combine("data", mapName);
        private static string GetBackupDirectory(string mapName) => Path.Combine("data", "backup", mapName);

        // 备份数据
        public static void BackupData(string mapName, string toolName = "move_nodes")
        {
            string timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            string backupDir = GetBackupDirectory(mapName);
            string backupPath = Path.Combine(backupDir, $"data-{timestamp}-before-{toolName}");

            if (Directory.Exists(GetDataDirectory(mapName)))
            {
                Directory.CreateDirectory(backupPath);
                foreach (var file in Directory.GetFiles(GetDataDirectory(mapName)))
                {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(backupPath, fileName);
                    File.Copy(file, destFile, true);
                }
                Console.WriteLine($"[INFO] 数据已备份至: {backupPath}");
            }
            else
            {
                Console.WriteLine("[ERROR] 未找到 data 目录。");
            }
        }
        //public static List<Node> LoadNodes(string mapName)
        //{
        //    string nodesPath = Path.Combine(GetDataDirectory(mapName), "nodes.json");
        //    if (File.Exists(nodesPath))
        //    {
        //        string json = File.ReadAllText(nodesPath);
        //        return JsonSerializer.Deserialize<List<Node>>(json);
        //    }
        //    else
        //    {
        //        Console.WriteLine("[ERROR] 未找到 nodes.json 文件。");
        //        return null;
        //    }
        //}

        public static void SaveNodes(string mapName, List<Node> nodes)
        {
            string nodesPath = Path.Combine(GetDataDirectory(mapName), "nodes.json");
            string json = JsonSerializer.Serialize(nodes, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(nodesPath, json);
            Console.WriteLine($"[INFO] 已更新的节点已保存到 {nodesPath}");
        }

        public static bool InBox(Node node, double xMin, double yMin, double xMax, double yMax)
        {
            if (node.Coord == null || node.Coord.Count != 2)
                return false;
            double x = node.Coord[0];
            double y = node.Coord[1];
            return xMin <= x && x <= xMax && yMin <= y && y <= yMax;
        }

        public static void MoveNodes(string mapName, double x1, double y1, double x2, double y2, double dx, double dy)
        {
            double xMin = Math.Min(x1, x2);
            double xMax = Math.Max(x1, x2);
            double yMin = Math.Min(y1, y2);
            double yMax = Math.Max(y1, y2);

            // Load the nodes
            List<Node> nodes = LoadNodes(mapName);
            if (nodes == null) return;

            int movedCount = 0;
            foreach (Node node in nodes)
            {
                if (InBox(node, xMin, yMin, xMax, yMax))
                {
                    node.Coord[0] += dx;
                    node.Coord[1] += dy;
                    movedCount++;
                }
            }

            // Save the updated nodes
            SaveNodes(mapName, nodes);
            Console.WriteLine($"[INFO] 通过 ({dx}, {dy}) 移动了 {movedCount} 节点");
        }
    }
}
