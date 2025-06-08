using System.ComponentModel;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using Svg;
using Svg.Skia;
using System.Diagnostics;
using AvalonDock.Layout;
using System.Linq.Expressions;
using static System.Formats.Asn1.AsnWriter;

namespace BusMapGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            LayoutDocumentPane.Children.Remove(RoadPreviewer);
            LayoutDocumentPane.Children.Remove(Mapper);
            Program.RPSkiaElement = SkiaCanvas;
            // 注册全局的 KeyDown 和 KeyUp 事件监听
            InputManager.Current.PreNotifyInput += (sender, e) =>
            {
                if (e.StagingItem.Input is KeyEventArgs keyArgs)
                {
                    if (keyArgs.Key == Key.LeftShift || keyArgs.Key == Key.RightShift ||
                        keyArgs.Key == Key.LeftCtrl || keyArgs.Key == Key.RightCtrl)
                    {
                        if (Program.Map != null && Program.SkiaSVG!= null)
                        {
                            SkiaCanvas.InvalidateVisual();
                        }
                    }
                }
            };
        }

        // 点击“打开”时执行
        private void OpenMap(object sender, RoutedEventArgs e)
        {
            // 弹窗，返回是否选了地图
            var selectWindow = new SelectMapWindow
            {
                Owner = Application.Current.MainWindow
            };
            bool? result = selectWindow.ShowDialog();
            // 如果选了地图执行下列，从而打开了地图
            if (result == true && !string.IsNullOrEmpty(selectWindow.SelectedMap))
            {
                LayoutDocumentPane.Children.Remove(Opener);
                if (!LayoutDocumentPane.Children.Contains(RoadPreviewer))
                { 
                    LayoutDocumentPane.Children.Add(RoadPreviewer);
                }
                if (!LayoutDocumentPane.Children.Contains(Mapper))
                {
                    LayoutDocumentPane.Children.Add(Mapper);
                }
                Program.Map = selectWindow.SelectedMap;            // 赋值：当前地图名称
                Program.SkiaSVG = null;                            // 清空当前 SkiaSVG
                Utils.DataRefresher();                                    // 刷新数据
                Generate();                                               // 生成 SVG
                ResetZoom();                                              // 重置缩放
                SkiaCanvas.InvalidateVisual();                            // 刷新重绘：对 SVG 主要进行“加载”和“绘制”两步
                MessageBox.Show($"你打开了地图：{Program.Map}");   // 弹出消息框
            }
        }

        // 初次显示、刷新请求、内容变化、尺寸变化时执行
        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)  // e 包含了很多关于 SkiaSharp 绘图画布的属性
        {
            if (!string.IsNullOrEmpty(Program.Map))
            {
                // 1. 获取 SkiaSharp 的绘图画布，清空 -> 背景设为白色 -> 加载 SVG
                var canvas = e.Surface.Canvas;
                canvas.Clear(SKColors.White);
                LoadSvg();

                // 2. 应用缩放和平移
                canvas.Translate(Program.ZoomCenter.X, Program.ZoomCenter.Y);
                canvas.Scale(Program.Zoom);

                // 3. 绘制 SVG
                if (Program.SkiaSVG != null)
                { 
                    canvas.DrawPicture(Program.SkiaSVG.Picture);
                }

                // 条件控制：如果正在框选，画矩形
                if (Program.IsDragging)
                {
                    using var paint = new SKPaint
                    {
                        Color = new SKColor(0, 0, 220),
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 1.2f / Program.Zoom,
                        PathEffect = SKPathEffect.CreateDash([9 / Program.Zoom, 5 / Program.Zoom], 0)
                    };

                    var rect = SKRect.Create(
                        Math.Min(Program.SkiaStartPoint.X, Program.SkiaEndPoint.X),
                        Math.Min(Program.SkiaStartPoint.Y, Program.SkiaEndPoint.Y),
                        Math.Abs(Program.SkiaEndPoint.X - Program.SkiaStartPoint.X),
                        Math.Abs(Program.SkiaEndPoint.Y - Program.SkiaStartPoint.Y));

                    canvas.DrawRect(rect, paint);
                }

                // 条件控制：鼠标靠近道路节点，道路节点有矩形框
                if (Program.MouseButtonNearElement.Type == BMGElementTypes.Node)
                {
                    SKPoint rectCenter = Program.Nodes[Program.MouseButtonNearElement.Id].SkiaCoord;
                    using var paint1 = new SKPaint
                    {
                        Color = new SKColor(0, 0, 240),
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 1f / Program.Zoom,
                    };
                    var rect1 = SKRect.Create(rectCenter.X - 2f - 4.7f / Program.Zoom, rectCenter.Y - 2f - 4.7f / Program.Zoom, 4f + 9.4f / Program.Zoom, 4f + 9.4f / Program.Zoom);
                    canvas.DrawRect(rect1, paint1);
                    using var paint2 = new SKPaint
                    {
                        Color = new SKColor(200, 200, 230),
                        Style = SKPaintStyle.Fill
                    };
                    var rect2 = SKRect.Create(rectCenter.X - 2f - 4.7f / Program.Zoom - 4f / Program.Zoom, rectCenter.Y - 2f - 4.7f / Program.Zoom - 4f / Program.Zoom, 8f / Program.Zoom, 8f / Program.Zoom);
                    var rect3 = SKRect.Create(rectCenter.X + 2f + 4.7f / Program.Zoom - 4f / Program.Zoom, rectCenter.Y - 2f - 4.7f / Program.Zoom - 4f / Program.Zoom, 8f / Program.Zoom, 8f / Program.Zoom);
                    var rect4 = SKRect.Create(rectCenter.X - 2f - 4.7f / Program.Zoom - 4f / Program.Zoom, rectCenter.Y + 2f + 4.7f / Program.Zoom - 4f / Program.Zoom, 8f / Program.Zoom, 8f / Program.Zoom);
                    var rect5 = SKRect.Create(rectCenter.X + 2f + 4.7f / Program.Zoom - 4f / Program.Zoom, rectCenter.Y + 2f + 4.7f / Program.Zoom - 4f / Program.Zoom, 8f / Program.Zoom, 8f / Program.Zoom);
                    canvas.DrawRect(rect2, paint2);
                    canvas.DrawRect(rect3, paint2);
                    canvas.DrawRect(rect4, paint2);
                    canvas.DrawRect(rect5, paint2);
                    canvas.DrawRect(rect2, paint1);
                    canvas.DrawRect(rect3, paint1);
                    canvas.DrawRect(rect4, paint1);
                    canvas.DrawRect(rect5, paint1);
                    NearElementText.Text = $"靠近节点：{Program.MouseButtonNearElement.Id}";
                }
                // 条件控制：鼠标靠近道路，道路会有中心蓝线
                if (Program.MouseButtonNearElement.Type == BMGElementTypes.Road)
                {
                    SKPoint roadStart = Program.Roads[Program.MouseButtonNearElement.Id].SKiaCoordStart;
                    SKPoint roadEnd = Program.Roads[Program.MouseButtonNearElement.Id].SKiaCoordEnd;
                    using var paint1 = new SKPaint
                    {
                        Color = new SKColor(0, 180, 230),
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 2f / Program.Zoom,
                    };
                    canvas.DrawLine(roadStart, roadEnd, paint1);
                    NearElementText.Text = $"靠近道路：{Program.MouseButtonNearElement.Id}";
                }
                // 条件控制：鼠标没有靠近的物体，NearElementText 显示“无靠近的元素”
                if (Program.MouseButtonNearElement.Type == BMGElementTypes.None)
                {
                    NearElementText.Text = "无靠近的元素";
                }
                // 条件控制：使用道路节点移动工具时，在预计的目标位置画一个黄点
                if (Program.CurrentManagementTool == ManagementTool.MoveNode)
                {
                    if (Program.SelectedElement.Type == BMGElementTypes.Node)
                    {
                        Node node = Program.Nodes[Program.SelectedElement.Id];
                        int moveDirection = Program.JSONMove.Item1;
                        decimal moveDistance = Program.JSONMove.Item2;
                        if (node.NodeMovingCanMoveDistance[moveDirection] != -1)
                        {
                            moveDistance = Math.Min(Program.JSONMove.Item2, node.NodeMovingCanMoveDistance[moveDirection]);
                        }
                        decimal[] jsonTargetCoord = Utils.GetTargetCoord(node.JSONCoord, moveDirection, moveDistance);
                        SKPoint skiaTargetCoord = Utils.CoordJSONToSkia(jsonTargetCoord);
                        using var paint = new SKPaint
                        {
                            Color = new SKColor(0, 255, 255),
                            Style = SKPaintStyle.StrokeAndFill,
                            StrokeWidth = 1f / Program.Zoom,
                        };
                        canvas.DrawCircle(skiaTargetCoord.X, skiaTargetCoord.Y, 6f / Program.Zoom, paint);
                    }
                }

                // 条件控制：使用道路拉出工具时，先画出拉出的线段，然后画出目标位置的节点
                if (Program.CurrentManagementTool == ManagementTool.PullRoad)
                {
                    if (Program.SelectedElement.Type == BMGElementTypes.Node)
                    {
                        Node node = Program.Nodes[Program.SelectedElement.Id];
                        int moveDirection = Program.JSONMove.Item1;
                        decimal moveDistance = Program.JSONMove.Item2;
                        if (node.RoadsId[moveDirection] != -1) moveDistance = 0;
                        if (moveDistance > 3)
                        {
                            decimal[] targetCoord = Utils.GetTargetCoord(node.JSONCoord, moveDirection, moveDistance);
                            SKPoint skiaTargetCoord = Utils.CoordJSONToSkia(targetCoord);
                            using var roadpaint = new SKPaint
                            {
                                Color = new SKColor(200, 0, 200, 130),
                                Style = SKPaintStyle.Stroke,
                                StrokeWidth = 12f / Program.Zoom,
                            };
                            canvas.DrawLine(node.SkiaCoord.X, node.SkiaCoord.Y, skiaTargetCoord.X, skiaTargetCoord.Y, roadpaint);
                            using var paint = new SKPaint
                            {
                                Color = new SKColor(0, 255, 255),
                                Style = SKPaintStyle.StrokeAndFill,
                                StrokeWidth = 1f / Program.Zoom,
                            };
                            canvas.DrawCircle(skiaTargetCoord.X, skiaTargetCoord.Y, 6f / Program.Zoom, paint);
                        }
                    }
                }

                // 条件控制：使用道路移动工具时，画出受影响的道路和道路节点
                if (Program.CurrentManagementTool == ManagementTool.MoveRoad)
                {
                    if (Program.SelectedElement.Type == BMGElementTypes.Road)
                    {
                        Road road = Program.Roads[Program.SelectedElement.Id];
                        Dictionary<Node, decimal[]> moveDict = road.DisplacementWhileMovingTargetNodes;
                        using var paint = new SKPaint
                        {
                            Color = new SKColor(0, 255, 255),
                            Style = SKPaintStyle.StrokeAndFill,
                            StrokeWidth = 1f / Program.Zoom,
                        };
                        foreach (KeyValuePair<Node, decimal[]> movePair in moveDict)
                        {
                            decimal[] targetCoord = [movePair.Key.JSONCoord[0] + movePair.Value[0], movePair.Key.JSONCoord[1] + movePair.Value[1]];
                            SKPoint skiaTargetCoord = Utils.CoordJSONToSkia(targetCoord);
                            canvas.DrawCircle(skiaTargetCoord.X, skiaTargetCoord.Y, 6f / Program.Zoom, paint);
                        }
                    }
                }

                // 条件控制：如果选择出东西，画矩形
                if (Program.SelectedNodesIds.Count > 0)
                {
                    // 对选中的内容画选择框
                    using var paint1 = new SKPaint
                    {
                        Color = new SKColor(0, 0, 240),
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 1f / Program.Zoom,
                    };
                    var rect1 = SKRect.Create(
                        Program.SelectedMinX - 2f, Program.SelectedMinY - 2f,
                        Program.SelectedMaxX - Program.SelectedMinX + 4f, Program.SelectedMaxY - Program.SelectedMinY + 4f);
                    canvas.DrawRect(rect1, paint1);
                    using var paint2 = new SKPaint
                    {
                        Color = new SKColor(200, 200, 230),
                        Style = SKPaintStyle.Fill
                    };
                    var rect2 = SKRect.Create(Program.SelectedMinX - 2f - 4f / Program.Zoom, Program.SelectedMinY - 2f - 4f / Program.Zoom, 8f / Program.Zoom, 8f / Program.Zoom);
                    var rect3 = SKRect.Create(Program.SelectedMaxX + 2f - 4f / Program.Zoom, Program.SelectedMinY - 2f - 4f / Program.Zoom, 8f / Program.Zoom, 8f / Program.Zoom);
                    var rect4 = SKRect.Create(Program.SelectedMinX - 2f - 4f / Program.Zoom, Program.SelectedMaxY + 2f - 4f / Program.Zoom, 8f / Program.Zoom, 8f / Program.Zoom);
                    var rect5 = SKRect.Create(Program.SelectedMaxX + 2f - 4f / Program.Zoom, Program.SelectedMaxY + 2f - 4f / Program.Zoom, 8f / Program.Zoom, 8f / Program.Zoom);
                    canvas.DrawRect(rect2, paint2);
                    canvas.DrawRect(rect3, paint2);
                    canvas.DrawRect(rect4, paint2);
                    canvas.DrawRect(rect5, paint2);
                    canvas.DrawRect(rect2, paint1);
                    canvas.DrawRect(rect3, paint1);
                    canvas.DrawRect(rect4, paint1);
                    canvas.DrawRect(rect5, paint1);
                }
            }
        }

        // 按下鼠标时执行
        private void SkiaCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(Program.Map))
            {
                // 左键执行
                if (e.ChangedButton == MouseButton.Left)
                {
                    Program.WPFStartPoint = e.GetPosition(SkiaCanvas);
                    Program.SkiaStartPoint = Utils.CoordWPFToSkia(Program.WPFStartPoint, SkiaCanvas);
                    Program.JSONStartPoint = Utils.CoordSkiaToJSON(Program.SkiaStartPoint);
                    // 赋值选择的元素
                    Program.SelectedElement = Program.MouseButtonNearElement;
                }
                // 关于平移
                if (e.ChangedButton == MouseButton.Middle)
                {
                    Program.IsPanning = true;
                    Program.LastMousePosition = e.GetPosition(SkiaCanvas);
                    SkiaCanvas.CaptureMouse();
                }
            }
        }

        // 移动鼠标时执行
        private void SkiaCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Program.RPSkiaElement = SkiaCanvas;
            Program.MousePosition = e.GetPosition(SkiaCanvas);
            if (!string.IsNullOrEmpty(Program.Map))
            {
                SkiaCanvas.InvalidateVisual();
                Program.WPFEndPoint = e.GetPosition(SkiaCanvas);
                Program.SkiaEndPoint = Utils.CoordWPFToSkia(Program.WPFEndPoint, SkiaCanvas);
                Program.JSONEndPoint = Utils.CoordSkiaToJSON(Program.SkiaEndPoint);
                // 关于平移
                if (Program.IsPanning)
                {
                    Point currentPosition = e.GetPosition(SkiaCanvas);
                    Vector delta = currentPosition - Program.LastMousePosition;
                    Program.LastMousePosition = currentPosition;

                    // 获取当前鼠标点对应的 Skia 坐标
                    var mouseBefore = Utils.CoordWPFToSkia(currentPosition - delta, SkiaCanvas);
                    var mouseAfter = Utils.CoordWPFToSkia(currentPosition, SkiaCanvas);

                    // 平移等价于让画布内容在 Skia 空间“跟着鼠标差异移动”
                    var deltaSkia = mouseAfter - mouseBefore;

                    deltaSkia.X *= Program.Zoom;
                    deltaSkia.Y *= Program.Zoom;

                    Program.ZoomCenter += deltaSkia;

                    SkiaCanvas.InvalidateVisual();
                    return;
                }
            }
        }

        // 抬起鼠标时执行
        private void SkiaCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(Program.Map))
            {
                // 左键执行
                if (e.ChangedButton == MouseButton.Left)
                {
                    // 道路编辑模式下
                    if (Program.Mode == Mode.EditRoads)
                    {
                        // 使用道路节点移动工具时
                        if (Program.CurrentManagementTool == ManagementTool.MoveNode)
                        {
                            Debug.WriteLine("执行节点移动");
                            ManagementTools.MoveNode(Program.SelectedElement.Id);
                            Generate();
                            Utils.DataRefresher();
                        }
                        // 使用道路拉出工具时
                        else if (Program.CurrentManagementTool == ManagementTool.PullRoad)
                        {
                            Debug.WriteLine("执行道路拉出");
                            ManagementTools.PullRoad(Program.SelectedElement.Id);
                            Generate();
                            Utils.DataRefresher();
                        }
                        // 使用道路节点删除工具时
                        else if (Program.CurrentManagementTool == ManagementTool.DeleteNode)
                        {
                            Debug.WriteLine("执行节点删除");
                            ManagementTools.DeleteNode(Program.SelectedElement.Id);
                            Generate();
                            Utils.DataRefresher();
                        }
                        // 使用道路移动工具时
                        else if (Program.CurrentManagementTool == ManagementTool.MoveRoad)
                        {
                            Debug.WriteLine("执行道路移动");
                            ManagementTools.MoveRoad(Program.SelectedElement.Id);
                            Generate();
                            Utils.DataRefresher();
                        }
                        Program.SelectedElement = new();
                        SkiaCanvas.InvalidateVisual();
                    }
                }
                // 关于缩放和平移
                if (e.ChangedButton == MouseButton.Middle)
                {
                    Program.IsPanning = false;
                    SkiaCanvas.ReleaseMouseCapture();
                    return;
                }
            }
        }

        // 关于缩放和平移
        private void SkiaCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (string.IsNullOrEmpty(Program.Map))
            {
                return;
            }

            // 获取鼠标在控件中的位置
            var wpfMousePos = e.GetPosition(SkiaCanvas);

            // 将鼠标点转换为 Skia 坐标（变换前的逻辑坐标）
            var matrix = PresentationSource.FromVisual(SkiaCanvas)?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
            double dpiX = matrix.M11;
            double dpiY = matrix.M22;
            float rawX = (float)(wpfMousePos.X * dpiX);
            float rawY = (float)(wpfMousePos.Y * dpiY);

            // 当前鼠标位置对应的世界坐标（变换前）
            var logicalMousePos = new SKPoint(
                (rawX - Program.CanvasOffset.X - Program.ZoomCenter.X) / Program.Zoom,
                (rawY - Program.CanvasOffset.Y - Program.ZoomCenter.Y) / Program.Zoom
            );

            // 更新缩放比例
            const float zoomFactor = 1.1f;
            if (e.Delta > 0)
                Program.Zoom *= zoomFactor;
            else
                Program.Zoom /= zoomFactor;

            // 缩放后，重新计算 ZoomCenter，使得鼠标位置保持在同一逻辑坐标点
            Program.ZoomCenter = new SKPoint(
                rawX - Program.CanvasOffset.X - logicalMousePos.X * Program.Zoom,
                rawY - Program.CanvasOffset.Y - logicalMousePos.Y * Program.Zoom
            );

            SkiaCanvas.InvalidateVisual();
        }

        // 执行生成
        private static void Generate()
        {
            if (Program.Map != null)
            {
                SvgDocument rp = new() { Width = Program.PaperSizeX, Height = Program.PaperSizeY };
                foreach (KeyValuePair<int, Road> road in Program.Roads)
                {
                    rp.Children.Add(road.Value.RPGraph);
                }
                foreach (KeyValuePair<int, Node> node in Program.Nodes)
                {
                    rp.Children.Add(node.Value.RPGraph);
                }
                foreach (KeyValuePair<int, Station> station in Program.Stations)
                {
                    rp.Children.Add(station.Value.RPGraph);
                    rp.Children.Add(station.Value.RPText);
                }
                if (Path.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output", Program.Map)) == false)
                {
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output", Program.Map));
                }
                rp.Write(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output", Program.Map, "道路预览.svg"));
            }
        }

        // 加载
        private static void LoadSvg()
        {
            if (Program.Map != null)
            {
                Program.SkiaSVG = new SKSvg();
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output", Program.Map, "道路预览.svg")))
                {
                    Program.SkiaSVG.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output", Program.Map, "道路预览.svg"));
                }
            }
        }

        // 重置缩放
        private void ResetZoom()
        {
            Program.Zoom = 1f;
            Program.ZoomCenter = new SKPoint(0, 0);
            SkiaCanvas.InvalidateVisual();
        }
    }
}