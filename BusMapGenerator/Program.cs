using System;
using System.Collections.Generic;
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
    internal class Program
    {
        public static string? CurrentMap { get; set; } = null;  // 当前地图

        public static SKSvg? CurrentSkiaSVG { get; set; } = null;  // 当前的 SkiaSVG

        public static SKRect SkiaSvgBounds { get; set; } = new SKRect();  // SkiaSVG 画布边界，有 Width 和 Height 属性

        public static Point WPFStartPoint { get; set; } = new Point();  // Skia 起点坐标

        public static Point WPFEndPoint { get; set; } = new Point();  // Skia 终点坐标

        public static SKPoint SkiaStartPoint { get; set; } = new SKPoint();  // Skia 起点坐标

        public static SKPoint SkiaEndPoint { get; set; } = new SKPoint();  // Skia 终点坐标

        public static Point? SVGStartPoint { get; set; } = null;  // SVG 起点坐标

        public static Point? SVGEndPoint { get; set; } = null;  // SVG 终点坐标

        public static bool IsDragging { get; set; } = false;
    }
}
