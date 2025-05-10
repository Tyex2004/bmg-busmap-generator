using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace BusMapGenerator
{
    partial class Utils
    {
        // 备份数据：输入 ( <地图名称> , <工具名称> )，执行备份
        public static void BackupData(string mapName, string toolName)
        {
            string timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            string mapDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", mapName);
            string backupDir = Path.Combine(mapDir, "backup");
            string backupPath = Path.Combine(backupDir, $"data-{timestamp}-before-{toolName}");

            Directory.CreateDirectory(backupPath);
            MoveData(mapDir, backupPath);
        }

        // 判断道路节点是否在选择框里：输入 ( <节点对象> , [ <框选矩形对角坐标组> ] ，输出布尔值
        public static bool IsNodeInBox(Node node, decimal x1, decimal y1, decimal x2, decimal y2)
        {
            decimal xMin = Math.Min(x1, x2);
            decimal xMax = Math.Max(x1, x2);
            decimal yMin = Math.Min(y1, y2);
            decimal yMax = Math.Max(y1, y2);
            decimal x = node.Coord[0];
            decimal y = node.Coord[1];
            return xMin <= x && x <= xMax && yMin <= y && y <= yMax;
        }

        // 解析备份文件夹名中的 timestamp：输入 ( <文件夹名> )，输出 DateTime?
        public static DateTime? ExtractTimestampOfBackup(string folderName)
        {
            // 使用正则表达式匹配文件夹名中的 timestamp
            var match = DataRegex1().Match(folderName);
            if (match.Success)
            {
                // 尝试将匹配到的 timestamp 转换为 DateTime
                if (DateTime.TryParseExact(match.Groups[1].Value, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime timestamp))
                {
                    return timestamp;
                }
            }
            return null;
        }

        // 解析已撤回文件夹名中的 timestamp：输入 ( <文件夹名> )，输出 DateTime?
        public static DateTime? ExtractTimestampOfUndone(string folderName)
        {
            // 使用正则表达式匹配文件夹名中的 timestamp
            var match = DataRegex().Match(folderName);
            if (match.Success)
            {
                // 尝试将匹配到的 timestamp 转换为 DateTime
                if (DateTime.TryParseExact(match.Groups[1].Value, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime timestamp))
                {
                    return timestamp;
                }
            }
            return null;
        }

        // 移动数据：输入（ <源文件夹> , <目标文件夹> ），执行移动
        public static void MoveData(string sourceDir, string destDir)
        {
            string[] filesToMove = ["nodes.json", "roads.json", "stations.json", "routes.json", "mtr_stations.json"];
            foreach (string file in filesToMove)
            {
                File.Move(Path.Combine(sourceDir, file), Path.Combine(destDir, file), true);
            }
        }

        [GeneratedRegex(@"data-(\d{14})-.*")]
        private static partial Regex DataRegex();

        [GeneratedRegex(@"data-(\d{14})-before-.*")]
        private static partial Regex DataRegex1();
    }
}
