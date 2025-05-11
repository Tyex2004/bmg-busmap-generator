using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using SkiaSharp.Views.WPF;
using System.Windows;
using SkiaSharp;

namespace BusMapGenerator
{
    partial class Utils
    {
        // 正则表达式定义

        [GeneratedRegex(@"data-(\d{14})-.*")]
        private static partial Regex DataRegex();

        [GeneratedRegex(@"data-(\d{14})-before-.*")]
        private static partial Regex DataRegex1();

        // 工具方法

        // 备份数据：输入 ( <工具名称> )，执行备份
        public static void BackupData(string toolName)
        {
            string timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            string mapDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.CurrentMap);
            string backupDir = Path.Combine(mapDir, "backup");
            string backupPath = Path.Combine(backupDir, $"data-{timestamp}-before-{toolName}");

            Directory.CreateDirectory(backupPath);
            MoveData(mapDir, backupPath);
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

        // WPF 坐标 → Skia 坐标
        public static SKPoint CoordWPFToSkia(Point wpfPoint, SKElement skElement)
        {
            var matrix = PresentationSource.FromVisual(skElement)?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
            double dpiX = matrix.M11;
            double dpiY = matrix.M22;

            float rawX = (float)(wpfPoint.X * dpiX);
            float rawY = (float)(wpfPoint.Y * dpiY);

            float skiaX = (rawX - Program.CanvasOffset.X - Program.ZoomCenter.X) / Program.Zoom;
            float skiaY = (rawY - Program.CanvasOffset.Y - Program.ZoomCenter.Y) / Program.Zoom;

            return new SKPoint(skiaX, skiaY);
        }

        // Skia 坐标 → JSON 坐标
        public static decimal[] CoordSkiaToJSON(SKPoint skiaPoint)
        {
            decimal[] decimalArray = [0, 0];
            decimalArray[0] = (decimal)skiaPoint.X;
            decimalArray[1] = (decimal)skiaPoint.Y;
            decimalArray[0] = decimalArray[0] + Program.PriorCenterX - (Program.PaperSizeX / 2);
            decimalArray[1] = Program.PriorCenterY + (Program.PaperSizeY / 2) - decimalArray[1];
            return decimalArray;
        }

        // JSON 坐标 → Skia 坐标
        public static SKPoint CoordJSONToSkia(decimal[] jsonPoint)
        {
            SKPoint skiaPoint = new SKPoint();
            skiaPoint.X = (float)(jsonPoint[0] - Program.PriorCenterX + (Program.PaperSizeX / 2));
            skiaPoint.Y = (float)(Program.PriorCenterY - jsonPoint[1] + (Program.PaperSizeY / 2));
            return skiaPoint;
        }
    }
}
