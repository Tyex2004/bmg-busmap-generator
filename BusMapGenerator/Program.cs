using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Svg.Skia;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace BusMapGenerator
{
    static internal class Program  // 全局变量列表
    {
        // 关于地图信息
        public static string? Map { get; set; } = null;                 // 当前地图
        public static SKSvg? SkiaSVG { get; set; } = null;              // 当前的 SkiaSVG
        public static SKElement RPSkiaElement { get; set; } = new();    // 当前的 SkiaElement

        // 关于鼠标信息
        public static Point MousePosition { get; set; } = new Point();  // 当前鼠标位置
        public static BMGElementId MouseNearElement               // 当前鼠标靠近
        {
            get
            {
                if (Mode == Mode.EditRoads)
                {
                    foreach (KeyValuePair<int, Node> node in Nodes)
                    {
                        if (Utils.CalculatePointDistance(MousePosition, node.Value.WPFCoord) < 7)
                        {
                            return new(BMGElementTypes.Node, node.Key);
                        }
                    }
                    foreach (KeyValuePair<int, Road> road in Roads)
                    {
                        if (Utils.CalculatePointToLineDistance(MousePosition, road.Value.WPFCoordStart, road.Value.WPFCoordEnd) < 6)
                        {
                            return new(BMGElementTypes.Road, road.Key);
                        }
                    }
                }
                return new();
            }
        }
        public static BMGElementId[] MouseTwoNearestRoads
        {
            get
            {
                if (Mode == Mode.EditRoads)
                {
                    var nearRoads = new List<(double distance, int roadId)>();

                    foreach (KeyValuePair<int, Road> road in Roads)
                    {
                        double distance = Utils.CalculatePointToLineDistance(MousePosition, road.Value.WPFCoordStart, road.Value.WPFCoordEnd);
                        if (distance < 6)
                        {
                            nearRoads.Add((distance, road.Key));
                        }
                    }

                    // 按照距离升序排列
                    nearRoads.Sort((a, b) => a.distance.CompareTo(b.distance));

                    // 取前两个最近的道路
                    BMGElementId[] result = new BMGElementId[2];
                    for (int i = 0; i < Math.Min(2, nearRoads.Count); i++)
                    {
                        result[i] = new BMGElementId(BMGElementTypes.Road, nearRoads[i].roadId);
                    }

                    return result;
                }

                // 如果不是编辑道路模式或没有足够靠近的道路，返回空数组
                return new BMGElementId[2];
            }
        }
        public static bool IsDragging { get; set; } = false;            // 是否在画布按住拖拽

        // 关于模式、数据管理工具使用信息
        public static Mode Mode = Mode.EditRoads;                       // 当前模式
        public static BMGElementId SelectedElement { get; set; }        // 选中的元素类型和编号
        public static ManagementTool CurrentManagementTool              // 当前数据管理工具
        {
            get
            {
                // 编辑道路模式
                if (Mode == Mode.EditRoads)
                {
                    // 选择了道路节点
                    if (SelectedElement.Type == BMGElementTypes.Node)
                    {
                        if (KeyStatus == KeyStatus.None)
                        {
                            return ManagementTool.MoveNode;
                        }
                        else if (KeyStatus == KeyStatus.Shift)
                        {
                            return ManagementTool.PullRoad;
                        }
                        else if (KeyStatus == KeyStatus.Ctrl)
                        {
                            return ManagementTool.DeleteNode;
                        }
                        else return ManagementTool.None;
                    }
                    // 选择了道路
                    else if (SelectedElement.Type == BMGElementTypes.Road)
                    {
                        if (KeyStatus == KeyStatus.None)
                        {
                            return ManagementTool.MoveRoad;
                        }
                        else if (KeyStatus == KeyStatus.Shift)
                        {
                            return ManagementTool.InsertNode;
                        }
                        else if (KeyStatus == KeyStatus.Ctrl)
                        {
                            return ManagementTool.DeleteRoad;
                        }
                        else return ManagementTool.None;
                    }
                    // 其它情况
                    else return ManagementTool.None;
                }
                // 非编辑道路模式
                else return ManagementTool.None;
            }
        }

        // 关于画布平移信息
        public static bool IsPanning = false;                           // 是否正在中键平移
        public static Point LastMousePosition;                          // 上一次鼠标位置（WPF坐标）
        public static float Zoom = 1f;                                  // 当前缩放比例
        public static SKPoint CanvasOffset = new(0, 0);                 // 当前画布偏移
        public static SKPoint ZoomCenter = new(0, 0);                   // 缩放中心

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
        public static decimal[] JSONStartPoint { get; set; } = new decimal[2];           // JSON 起点坐标
        public static decimal[] JSONEndPoint { get; set; } = new decimal[2];             // JSON 终点坐标
        public static (int, decimal) JSONMove => (WPFEndPoint - WPFStartPoint).Length > 7 ? Utils.GetDirectionAndDistance(JSONStartPoint, JSONEndPoint) : (0, 0);  // JSON 移动方向和距离

        // 关于键盘行为信息
        public static KeyStatus KeyStatus
        {
            get
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    return KeyStatus.Ctrl;
                }
                else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    return KeyStatus.Shift;
                }
                else
                {
                    return KeyStatus.None;
                }
            }
        }

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

        // 属性数据
        public static List<StraightRoad> StraightRoads = [];    // 直线道路列表
    }
    enum KeyStatus
    {
        None,
        Shift,
        Ctrl
    }
    enum Mode
    {
        EditRoads,
        EditStations,
    }
    enum BMGElementTypes
    {
        None,
        Node,
        Road,
        Station,
        Route,
        MtrStation
    }
    record struct BMGElementId(BMGElementTypes Type = BMGElementTypes.None, int Id = -1);
}
