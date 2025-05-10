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
                return; // 如果没有地图路径，则不在画布上加载 SVG
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

            // 2. 绘制 SVG
            canvas.DrawPicture(Program.CurrentSkiaSVG.Picture);

            // 条件控制：如果正在框选，画矩形
            if (Program.IsDragging)
            {
                using var paint = new SKPaint
                {
                    Color = new SKColor(200, 0, 255),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2
                };

                var rect = SKRect.Create(
                    Math.Min(Program.SkiaStartPoint.X, Program.SkiaEndPoint.X),
                    Math.Min(Program.SkiaStartPoint.Y, Program.SkiaEndPoint.Y),
                    Math.Abs(Program.SkiaEndPoint.X - Program.SkiaStartPoint.X),
                    Math.Abs(Program.SkiaEndPoint.Y - Program.SkiaStartPoint.Y));

                canvas.DrawRect(rect, paint);
            }
        }

        private void SkiaCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Program.WPFStartPoint = e.GetPosition(SkiaCanvas);
            Program.SkiaStartPoint = CoordWPFToSkia(Program.WPFStartPoint, SkiaCanvas);
            Program.IsDragging = true;
        }

        private void SkiaCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (Program.IsDragging)
            {
                Program.WPFEndPoint = e.GetPosition(SkiaCanvas);
                Program.SkiaEndPoint = CoordWPFToSkia(Program.WPFEndPoint, SkiaCanvas);
                SkiaCanvas.InvalidateVisual();
            }
        }

        private void SkiaCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!Program.IsDragging) return;

            Program.IsDragging = false;
            Program.WPFEndPoint = e.GetPosition(SkiaCanvas);
            Program.SkiaEndPoint = CoordWPFToSkia(Program.WPFEndPoint, SkiaCanvas);
            SkiaCanvas.InvalidateVisual();
        }

        // WPF 坐标转 Skia 坐标
        public static SKPoint CoordWPFToSkia(Point wpfPoint, SKElement skElement)
        {
            // 获取 WPF 控件的 DPI 缩放系数
            var matrix = PresentationSource.FromVisual(skElement)?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
            double dpiX = matrix.M11;
            double dpiY = matrix.M22;

            // 将 WPF 坐标（单位 DIP）转换为物理像素坐标（用于 Skia）
            float skiaX = (float)(wpfPoint.X * dpiX);
            float skiaY = (float)(wpfPoint.Y * dpiY);

            return new SKPoint(skiaX, skiaY);
        }
    }
}