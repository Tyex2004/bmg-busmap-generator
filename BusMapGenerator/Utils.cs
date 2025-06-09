using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (!string.IsNullOrEmpty(Program.Map))
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string mapDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map);
                string backupDir = Path.Combine(mapDir, "backup");
                string backupPath = Path.Combine(backupDir, $"data-{timestamp}-before-{toolName}");

                Directory.CreateDirectory(backupPath);
                MoveData(mapDir, backupPath);
            }
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
            SKPoint skiaPoint = new()
            {
                X = (float)jsonPoint[0] - Program.PriorCenterX + (Program.PaperSizeX / 2),
                Y = Program.PriorCenterY - (float)jsonPoint[1] + (Program.PaperSizeY / 2)
            };
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

        // 计算点到直线距离
        public static double CalculatePointToLineDistance(Point p, Point a, Point b)
        {
            // 线段 ab 的平方长度
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double lengthSquared = dx * dx + dy * dy;

            if (lengthSquared == 0)
                return CalculatePointDistance(p, a); // a 和 b 是同一个点

            // 计算投影点在 ab 上的比例 t（限制在 [0,1] 范围）
            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSquared;
            t = Math.Max(0, Math.Min(1, t));

            // 找到投影点
            Point projection = new(a.X + t * dx, a.Y + t * dy);

            // 返回从 p 到投影点的距离
            return CalculatePointDistance(p, projection);
        }

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

        // 刷新数据
        public static void DataRefresher()
        {
            Program.Nodes = DataLoader.LoadNodes();
            Program.Roads = DataLoader.LoadRoads();
            Program.Stations = DataLoader.LoadStations();
            Program.Routes = DataLoader.LoadRoutes();
            Program.MtrStations = DataLoader.LoadMtrStations();
            DataLoader.BuildStraightRoads();
            Debug.WriteLine($"直路数量：{Program.StraightRoads.Count}");
        }

        // 设置颜色
        public static SvgColourServer SetColor(int r, int g, int b)
        {
            return new(System.Drawing.Color.FromArgb(r, g, b));
        }

        // 转换方向
        public static int SwapDirection(int dir)
        {
            if (dir >= 0 && dir <= 3)
                return dir + 4;
            else if (dir >= 4 && dir <= 7)
                return dir - 4;
            else
                return -1; // 或抛出异常 throw new ArgumentOutOfRangeException()
        }
        // 求相对位移
        public static (int direction, decimal distance) GetDirectionAndDistance(decimal[] a, decimal[] b)
        {
            if (a.Length != 2 || b.Length != 2)
                throw new ArgumentException("坐标数组必须为长度为 2 的 decimal[2]");

            decimal dx = b[0] - a[0]; // x轴，东为正
            decimal dy = b[1] - a[1]; // y轴，北为正

            // 若两点重合，方向设为北，距离为 0
            if (dx == 0 && dy == 0)
                return (0, 0);

            // 计算角度（以“北”为 0°，顺时针为正方向）
            double angle = Math.Atan2((double)dx, (double)dy); // 注意 dx 和 dy 的顺序是为了以“北”为0°
            if (angle < 0)
                angle += 2 * Math.PI;

            // 映射到 8 个方向：每个方向占 π/4 = 45°
            int direction = (int)Math.Floor((angle + Math.PI / 8) / (Math.PI / 4)) % 8;

            // 计算距离
            decimal absDx = Math.Abs(dx);
            decimal absDy = Math.Abs(dy);
            decimal distance = (direction % 2 == 1)  // 斜向（奇数）
                ? Math.Min(absDx, absDy)
                : (direction % 4 == 0 ? absDy : absDx); // 北南用 dy，东西用 dx

            return (direction, distance);
        }


        // 求目标坐标
        public static decimal[] GetTargetCoord(decimal[] origin, int direction, decimal distance)
        {
            if (origin.Length != 2)
                throw new ArgumentException("起点坐标必须是长度为 2 的 decimal[2]");
            if (direction < 0 || direction > 7)
                throw new ArgumentOutOfRangeException(nameof(direction), "方向必须在 0 到 7 之间");

            // 八个方向的单位向量（x, y）
            var directionVectors = new (int dx, int dy)[]
            {
            (0, 1),   // 0 北
            (1, 1),   // 1 东北
            (1, 0),   // 2 东
            (1, -1),  // 3 东南
            (0, -1),  // 4 南
            (-1, -1), // 5 西南
            (-1, 0),  // 6 西
            (-1, 1),  // 7 西北
            };

            var (dxUnit, dyUnit) = directionVectors[direction];

            decimal dx, dy;

            if (direction % 2 == 0)
            {
                // 正交方向（水平或垂直）
                dx = distance * dxUnit;
                dy = distance * dyUnit;
            }
            else
            {
                // 对角方向，按你的定义：斜向时距离仅代表 dx，dy 与 dx 相同
                dx = distance * dxUnit;
                dy = distance * dyUnit;
            }

            return
            [
            origin[0] + dx,
            origin[1] + dy
            ];
        }
        // 计算两道路交点坐标
        public static decimal[]? GetTwoRoadsIntersection(Road road1, Road road2)
        {
            // 获取两个端点坐标
            decimal[] p1 = road1.Nodes[0].JSONCoord;
            decimal[] p2 = road1.Nodes[1].JSONCoord;
            decimal[] q1 = road2.Nodes[0].JSONCoord;
            decimal[] q2 = road2.Nodes[1].JSONCoord;

            int d1 = road1.Direction;
            int d2 = road2.Direction;

            // 方向分组函数：返回 0=竖直，1=右上斜，2=水平，3=右下斜
            static int GetClass(int d)
            {
                return d switch
                {
                    0 or 4 => 0, // 竖直
                    1 or 5 => 1, // 右上
                    2 or 6 => 2, // 水平
                    3 or 7 => 3, // 右下
                    _ => -1
                };
            }

            static bool InRange(decimal v, decimal a, decimal b) =>
                v >= Math.Min(a, b) && v <= Math.Max(a, b);

            int c1 = GetClass(d1);
            int c2 = GetClass(d2);
            if (c1 == -1 || c2 == -1 || c1 == c2) return null;

            // 保证 c1 <= c2，这样六种组合只需考虑一次
            if (c1 > c2)
            {
                (p1, p2, q1, q2) = (q1, q2, p1, p2);
                (d1, d2) = (d2, d1);
                (c1, c2) = (c2, c1);
            }

            decimal x = 0, y = 0;

            if (c1 == 0 && c2 == 1) // 竖直 & 右上
            {
                x = p1[0];
                y = x - q1[0] + q1[1];
            }
            else if (c1 == 0 && c2 == 2) // 竖直 & 水平
            {
                x = p1[0];
                y = q1[1];
            }
            else if (c1 == 0 && c2 == 3) // 竖直 & 右下
            {
                x = p1[0];
                y = -x + q1[0] + q1[1];
            }
            else if (c1 == 1 && c2 == 2) // 右上 & 水平
            {
                y = q1[1];
                x = y - p1[1] + p1[0];
            }
            else if (c1 == 1 && c2 == 3) // 右上 & 右下
            {
                decimal A = p1[0] - p1[1]; // x - y（右上）
                decimal B = q1[0] + q1[1]; // x + y（右下）
                x = (A + B) / 2;
                y = (B - A) / 2;
            }
            else if (c1 == 2 && c2 == 3) // 水平 & 右下
            {
                y = p1[1]; // 水平线的 y
                x = q1[0] + q1[1] - y; // 解出 x
            }

            // 判断是否在两个线段范围内
            bool onP = InRange(x, p1[0], p2[0]) && InRange(y, p1[1], p2[1]);
            bool onQ = InRange(x, q1[0], q2[0]) && InRange(y, q1[1], q2[1]);

            if (onP && onQ)
                return [x, y];

            return null;
        }


        public static decimal[]? GetFootOfPerpendicular(decimal[] point, Road road)
        {
            // 点坐标
            decimal px = point[0], py = point[1];

            // 路段起点和终点
            decimal[] a = road.Nodes[0].JSONCoord;
            decimal[] b = road.Nodes[1].JSONCoord;

            decimal x1 = a[0], y1 = a[1];
            decimal x2 = b[0], y2 = b[1];

            decimal dx = x2 - x1;
            decimal dy = y2 - y1;

            if (dx == 0 && dy == 0)
            {
                // 路段起终点重合（退化为点），返回 null
                return null;
            }

            // 计算参数 t，表示垂足在线段 AB 上的相对位置：A + t*(B - A)
            decimal t = ((px - x1) * dx + (py - y1) * dy) / (dx * dx + dy * dy);

            // 若 t 不在 [0,1]，说明垂足不在线段上
            if (t < 0 || t > 1)
            {
                return null;
            }

            // 计算垂足坐标
            decimal footX = x1 + t * dx;
            decimal footY = y1 + t * dy;

            return [footX, footY];
        }

    }
    public readonly struct Distance
    {
        public decimal Coefficient { get; }
        public int Radicand { get; } // 只能为 1 或 2

        public Distance(decimal[] a, decimal[] b)
        {
            if (a.Length != 2 || b.Length != 2)
                throw new ArgumentException("坐标数组必须为长度为 2 的 decimal[2]");

            decimal dx = b[0] - a[0];
            decimal dy = b[1] - a[1];

            decimal absDx = Math.Abs(dx);
            decimal absDy = Math.Abs(dy);

            if (absDx != 0 && absDy == 0)
            {
                // 水平
                Coefficient = absDx;
                Radicand = 1;
            }
            else if (absDx == 0 && absDy != 0)
            {
                // 垂直
                Coefficient = absDy;
                Radicand = 1;
            }
            else if (absDx == absDy && absDx != 0)
            {
                // 对角
                Coefficient = absDx;
                Radicand = 2;
            }
            else
            {
                throw new InvalidOperationException("两点不在八方向（上、下、左、右、左上、右上、左下、右下）上，无法构造 Distance。");
            }
        }

        public readonly double Value => (double)Coefficient * Math.Sqrt(Radicand);

        public override readonly string ToString()
        {
            return $"{Coefficient}√{Radicand}";
        }
    }
}
