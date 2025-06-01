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
        // 鼠标位置
        public static Point MousePosition { get; set; } = new Point();

        // 关于地图信息
        public static string? CurrentMap { get; set; } = null;      // 当前地图
        public static SKSvg? CurrentSkiaSVG { get; set; } = null;   // 当前的 SkiaSVG
        public static SKElement RPSkiaElement { get; set; } = new();  // 当前的 SkiaElement

        // 关于模式信息
        public static bool IsEditingRoads = true;                   // 是否正在编辑道路模式，可以使用工具：拉出道路、移动道路、插入道路节点、删除道路节点
        public static bool IsEditingStations = false;               // 是否正在编辑站点模式，可以使用工具：设置站点、移动站点、删除站点

        // 关于鼠标悬停信息
        public static int MouseButtonNearNodeId
        {
            get
            {
                if (IsEditingRoads)
                {
                    foreach (KeyValuePair<int, Node> node in Nodes)
                    {
                        if (Utils.CalculatePointDistance(MousePosition, node.Value.WPFCoord) < 7)
                        {
                            return node.Key;
                        }
                    }
                    return -1;
                }
                else
                {
                    return -1;
                }
            }
        }

        // 关于数据管理工具使用信息
        public static int SelectedNodeId { get; set; } = -1;        // 选中的道路节点编号
        public static bool IsMovingNode = false;                    // 是否正在使用移动节点工具

        // 关于操作信息
        public static bool IsPanning = false;                       // 是否正在中键平移
        public static bool IsDragging { get; set; } = false;        // 是否在画布按住拖拽
        public static Point LastMousePosition;                      // 上一次鼠标位置（WPF坐标）

        // 关于画布信息
        public static float Zoom = 1f;                              // 当前缩放比例
        public static SKPoint CanvasOffset = new(0, 0);             // 当前画布偏移
        public static SKPoint ZoomCenter = new(0, 0);               // 缩放中心

        // 关于坐标系变换参数信息
        public static float PaperSizeX => (float)(Nodes.Values.Max(node => node.JSONCoord[0]) - Nodes.Values.Min(node => node.JSONCoord[0])) + 60;
        public static float PaperSizeY => (float)(Nodes.Values.Max(node => node.JSONCoord[1]) - Nodes.Values.Min(node => node.JSONCoord[1])) + 60;
        public static float PriorCenterX => (float)(Nodes.Values.Max(node => node.JSONCoord[0]) + Nodes.Values.Min(node => node.JSONCoord[0])) / 2;
        public static float PriorCenterY => (float)(Nodes.Values.Max(node => node.JSONCoord[1]) + Nodes.Values.Min(node => node.JSONCoord[1])) / 2;

        // 关于坐标信息
        public static Point WPFStartPoint { get; set; } = new Point();          // WPF 起点坐标
        public static Point WPFEndPoint { get; set; } = new Point();            // WPF 终点坐标
        public static SKPoint SkiaStartPoint { get; set; } = new SKPoint();     // Skia 起点坐标
        public static SKPoint SkiaEndPoint { get; set; } = new SKPoint();       // Skia 终点坐标
        public static decimal[] JSONStartPoint { get; set; } = [];           // JSON 起点坐标
        public static decimal[] JSONEndPoint { get; set; } = [];             // JSON 终点坐标
        public static (int, decimal) JSONMove => Utils.GetDirectionAndDistance(JSONStartPoint, JSONEndPoint);  // JSON 移动方向和距离

        // 关于框选工具（暂时弃用）
        public static List<int> SelectedNodesIds { get; set; } = [];            // 选中的道路节点编号列表
        public static float SelectedMinX => SelectedNodesIds.Select(id => Nodes[id].SkiaCoord.X).ToList().Min();
        public static float SelectedMaxX => SelectedNodesIds.Select(id => Nodes[id].SkiaCoord.X).ToList().Max();
        public static float SelectedMinY => SelectedNodesIds.Select(id => Nodes[id].SkiaCoord.Y).ToList().Min();
        public static float SelectedMaxY => SelectedNodesIds.Select(id => Nodes[id].SkiaCoord.Y).ToList().Max();

        // 关于数据信息
        public static Dictionary<int, Node> Nodes = [];         // 道路节点字典
        public static Dictionary<int, Road> Roads = [];         // 道路字典
        public static Dictionary<int, Station> Stations = [];   // 站点字典
        public static Dictionary<int, Route> Routes = [];       // 线路字典
        public static List<MtrStation> MtrStations = [];        // 地铁站列表
    }
}
