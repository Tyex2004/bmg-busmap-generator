using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Svg.Skia;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace BusMapGenerator
{
    internal class Program  // 全局变量列表
    {
        // 关于地图信息
        public static string? CurrentMap { get; set; } = null;      // 当前地图
        public static SKSvg? CurrentSkiaSVG { get; set; } = null;   // 当前的 SkiaSVG

        // 关于操作信息
        public static bool IsPanning = false;                       // 是否正在中键平移
        public static bool IsDragging { get; set; } = false;        // 是否在画布按住拖拽
        public static Point LastMousePosition;                      // 上一次鼠标位置（WPF坐标）

        // 关于画布信息
        public static float Zoom = 1f;                              // 当前缩放比例
        public static SKPoint CanvasOffset = new(0, 0);             // 当前画布偏移
        public static SKPoint ZoomCenter = new(0, 0);               // 缩放中心
        public static SKRect SkiaSvgBounds { get; set; } = new SKRect();  // SkiaSVG 画布边界，有 Width 和 Height 属性

        // 关于坐标系变换参数信息
        public static string PaperSizeXString => CurrentMap != null ? File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", CurrentMap, "paper_size_x.txt")) : "";
        public static string PaperSizeYString => CurrentMap != null ? File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", CurrentMap, "paper_size_y.txt")) : "";
        public static string PriorCenterXString => CurrentMap != null ? File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", CurrentMap, "prior_center_x.txt")) : "";
        public static string PriorCenterYString => CurrentMap != null ? File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", CurrentMap, "prior_center_y.txt")) : "";
        public static decimal PaperSizeX => decimal.Parse(PaperSizeXString);
        public static decimal PaperSizeY => decimal.Parse(PaperSizeYString);
        public static decimal PriorCenterX => decimal.Parse(PriorCenterXString);
        public static decimal PriorCenterY => decimal.Parse(PriorCenterYString);


        // 关于坐标信息
        public static Point WPFStartPoint { get; set; } = new Point();          // WPF 起点坐标
        public static Point WPFEndPoint { get; set; } = new Point();            // WPF 终点坐标
        public static SKPoint SkiaStartPoint { get; set; } = new SKPoint();     // Skia 起点坐标
        public static SKPoint SkiaEndPoint { get; set; } = new SKPoint();       // Skia 终点坐标
        public static decimal[] SVGStartPoint { get; set; } = [];            // SVG 起点坐标
        public static decimal[] SVGEndPoint { get; set; } = [];              // SVG 终点坐标
        public static decimal[] JSONStartPoint { get; set; } = [];           // JSON 起点坐标
        public static decimal[] JSONEndPoint { get; set; } = [];             // JSON 终点坐标

        // 关于工具调用信息
        public static List<int> SelectedNodesIds { get; set; } = [];            // 选中的道路节点编号列表

        // 关于数据信息
        public static Dictionary<int, Node> Nodes  // 道路节点字典
        {
            get
            {
                if (CurrentMap != null)
                {
                    return DataLoader.LoadNodes();
                }
                return [];
            }
        }
    }
}
