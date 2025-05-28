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
using Svg;
using System.Runtime.Serialization;

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

        // Skia 坐标 → WPF 坐标
        public static Point CoordSkiaToWPF(SKPoint skiaPoint, SKElement skElement)
        {
            var matrix = PresentationSource.FromVisual(skElement)?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;

            // Skia → 设备像素
            float rawX = skiaPoint.X * Program.Zoom + Program.CanvasOffset.X + Program.ZoomCenter.X;
            float rawY = skiaPoint.Y * Program.Zoom + Program.CanvasOffset.Y + Program.ZoomCenter.Y;

            // 设备像素 → WPF（逻辑像素）
            var devicePoint = new Point(rawX, rawY);
            var wpfPoint = matrix.Transform(devicePoint);

            return wpfPoint;
        }

        // Skia 坐标 → JSON 坐标
        public static decimal[] CoordSkiaToJSON(SKPoint skiaPoint)
        {
            decimal[] decimalArray = [0, 0];
            decimalArray[0] = (decimal)skiaPoint.X;
            decimalArray[1] = (decimal)skiaPoint.Y;
            decimalArray[0] = decimalArray[0] + (decimal)Program.PriorCenterX - (decimal)(Program.PaperSizeX / 2);
            decimalArray[1] = (decimal)Program.PriorCenterY + (decimal)(Program.PaperSizeY / 2) - decimalArray[1];
            return decimalArray;
        }

        // JSON 坐标 → Skia 坐标
        public static SKPoint CoordJSONToSkia(decimal[] jsonPoint)
        {
            SKPoint skiaPoint = new SKPoint();
            skiaPoint.X = (float)jsonPoint[0] - Program.PriorCenterX + (Program.PaperSizeX / 2);
            skiaPoint.Y = Program.PriorCenterY - (float)jsonPoint[1] + (Program.PaperSizeY / 2);
            return skiaPoint;
        }

        // 获取框选后最小的 x
        public static decimal GetSelectedMinX()
        {
            List<decimal> xs = Program.SelectedNodesIds.Select(id => Program.Nodes[id].JSONCoord[0]).ToList();
            return xs.Min();
        }

        // 计算两个 Point 的距离
        public static double CalculatePointDistance(Point p1, Point p2) => (p2 - p1).Length;

        // 判断点是否靠近道路
        public static bool IsNodeNearRoad(decimal[] road_coord1, decimal[] road_coord2, decimal[] node_coord)
        {
            decimal x1 = road_coord1[0], y1 = road_coord1[1];
            decimal x2 = road_coord2[0], y2 = road_coord2[1];
            decimal x0 = node_coord[0], y0 = node_coord[1];

            // 将所有坐标转换为 double 以便使用 Math.Sqrt
            double dx = (double)(x2 - x1);
            double dy = (double)(y2 - y1);
            double lengthSquared = dx * dx + dy * dy;

            double px = (double)(x0 - x1);
            double py = (double)(y0 - y1);

            double t = lengthSquared == 0 ? 0 : (px * dx + py * dy) / lengthSquared;
            t = Math.Max(0, Math.Min(1, t)); // 限制 t 在 [0,1] 范围内

            double projX = (double)x1 + t * dx;
            double projY = (double)y1 + t * dy;

            double dist = Math.Sqrt((projX - (double)x0) * (projX - (double)x0) +
                                    (projY - (double)y0) * (projY - (double)y0));

            return dist <= 0.6;
        }

        // JSON 坐标 → SVG 坐标
        public static SvgPoint CoordJSONToSvg(decimal[] jsonCoord)
        {
            decimal x = jsonCoord[0];
            decimal y = jsonCoord[1];
            return new SvgPoint((float)x - Program.PriorCenterX + (Program.PaperSizeX / 2), Program.PriorCenterY - (float)y + (Program.PaperSizeY / 2));
        }

        // JSONEndPoint 投影至相对于 JSONStartPoint 的标准方向
        public static decimal[] DecimalEndPointProject(decimal[] StartPoint, decimal[] EndPoint)
        {
            decimal dx = EndPoint[0] - StartPoint[0];
            decimal dy = EndPoint[1] - StartPoint[1];

            // 使用整数比例的方向向量，避免浮点误差
            (decimal dx, decimal dy)[] directions =
            [
                (1m, 0m),
                (1m, 1m),
                (0m, 1m),
                (-1m, 1m),
                (-1m, 0m),
                (-1m, -1m),
                (0m, -1m),
                (1m, -1m)
            ];

            decimal maxDot = decimal.MinValue;
            (decimal dx, decimal dy) bestDir = (0m, 0m);

            foreach (var dir in directions)
            {
                decimal dot = dx * dir.dx + dy * dir.dy;
                if (dot > maxDot)
                {
                    maxDot = dot;
                    bestDir = dir;
                }
            }

            // 投影长度除以方向向量的模长平方
            decimal normSquared = bestDir.dx * bestDir.dx + bestDir.dy * bestDir.dy; // 1 或 2
            decimal scale = maxDot / normSquared;

            decimal x3 = StartPoint[0] + scale * bestDir.dx;
            decimal y3 = StartPoint[1] + scale * bestDir.dy;

            return [x3, y3];
        }
        public static void DataRefresher()
        {
            Program.Nodes = DataLoader.LoadNodes();
            Program.Roads = DataLoader.LoadRoads();
            Program.Stations = DataLoader.LoadStations();
        }
    }
}
