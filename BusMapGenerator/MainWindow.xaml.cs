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
using System.Windows.Shapes;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using Svg.Skia;
using System.Diagnostics;
using AvalonDock.Layout;

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
                ResetZoom();
                Program.CurrentMap = selectWindow.SelectedMap;            // 赋值：当前地图名称
                Program.CurrentSkiaSVG = null;                            // 清空当前 SkiaSVG
                SkiaCanvas.InvalidateVisual();                            // 刷新重绘：对 SVG 主要进行“加载”和“绘制”两步
                MessageBox.Show($"你打开了地图：{Program.CurrentMap}");   // 弹出消息框
            }
        }

        // 初次显示、刷新请求、内容变化、尺寸变化时执行
        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)  // e 包含了很多关于 SkiaSharp 绘图画布的属性
        {
            // 1. 获取 SkiaSharp 的绘图画布并清空，设置背景为白色。
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            // 条件控制：当前地图为 null 时跳出执行
            if (string.IsNullOrEmpty(Program.CurrentMap))
            {
                return;
            }

            // 条件控制：当 SVG 未加载时，加载 SVG ，并记录其边界
            if (Program.CurrentSkiaSVG == null)
            {
                Program.CurrentSkiaSVG = new SKSvg();
                Program.CurrentSkiaSVG.Load(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output", Program.CurrentMap, "道路预览.svg"));
                //Program.SkiaSvgBounds = Program.CurrentSkiaSVG.Picture.CullRect;
                if (Program.CurrentSkiaSVG.Picture != null)
                {
                    Program.SkiaSvgBounds = Program.CurrentSkiaSVG.Picture.CullRect;
                }
            }

            // 2. 应用缩放和平移
            canvas.Translate(Program.ZoomCenter.X, Program.ZoomCenter.Y);
            canvas.Scale(Program.Zoom);

            // 3. 绘制 SVG
            canvas.DrawPicture(Program.CurrentSkiaSVG.Picture);

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
        }

        // 按下鼠标时执行
        private void SkiaCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(Program.CurrentMap))
            {
                return;
            }
            if (e.ChangedButton == MouseButton.Left)
            {
                Program.WPFStartPoint = e.GetPosition(SkiaCanvas);
                Program.SkiaStartPoint = Utils.CoordWPFToSkia(Program.WPFStartPoint, SkiaCanvas);
                Program.JSONStartPoint = Utils.CoordSkiaToJSON(Program.SkiaStartPoint);
                Program.IsDragging = true;
            }
            if (e.ChangedButton == MouseButton.Middle)
            {
                Program.IsPanning = true;
                Program.LastMousePosition = e.GetPosition(SkiaCanvas);
                SkiaCanvas.CaptureMouse();
            }
        }

        // 移动鼠标时执行
        private void SkiaCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(Program.CurrentMap))
            {
                return;
            }
            if (Program.IsDragging)
            {
                Program.WPFEndPoint = e.GetPosition(SkiaCanvas);
                Program.SkiaEndPoint = Utils.CoordWPFToSkia(Program.WPFEndPoint, SkiaCanvas);
                Program.JSONEndPoint = Utils.CoordSkiaToJSON(Program.SkiaEndPoint);
                SkiaCanvas.InvalidateVisual();
            }
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

        // 抬起鼠标时执行
        private void SkiaCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(Program.CurrentMap))
            {
                return;
            }
            if (e.ChangedButton == MouseButton.Left)
            {
                if (!Program.IsDragging) return;
                Program.IsDragging = false;
                Program.WPFEndPoint = e.GetPosition(SkiaCanvas);
                Program.SkiaEndPoint = Utils.CoordWPFToSkia(Program.WPFEndPoint, SkiaCanvas);
                Program.JSONEndPoint = Utils.CoordSkiaToJSON(Program.SkiaEndPoint);
                ManagementTools.SelectNodes();  // 执行选择工具
                textBox3.Text = $"选择了{string.Join(",", Program.SelectedNodesIds)}";
                SkiaCanvas.InvalidateVisual();
            }
            if (e.ChangedButton == MouseButton.Middle)
            {
                Program.IsPanning = false;
                SkiaCanvas.ReleaseMouseCapture();
                return; // 避免继续走框选逻辑
            }
        }

        // 滚动鼠标时执行
        private void SkiaCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (string.IsNullOrEmpty(Program.CurrentMap))
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
            float oldZoom = Program.Zoom;
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
        private void RunPython(object sender, RoutedEventArgs e)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C run.exe demo-fongtsuen",
                    WorkingDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonScripts"),
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行渲染失败，报错内容: {ex.Message}");
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