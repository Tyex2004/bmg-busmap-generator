using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BusMapGenerator
{
    public partial class SetStationNameWindow : Window
    {
        // 用 required 修饰，外部必须在初始化时赋值
        private readonly Station _station;

        public SetStationNameWindow(Station station)
        {
            _station = station;
            InitializeComponent();

            Loaded += SetStationNameWindow_Loaded;

            // 绑定按键事件，处理 Enter 和 Tab
            NameTextBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            EnNameTextBox.PreviewKeyDown += TextBox_PreviewKeyDown;
        }

        private void SetStationNameWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化文本框内容
            NameTextBox.Text = _station.Name ?? string.Empty;
            EnNameTextBox.Text = _station.EnName ?? string.Empty;

            // 光标选中中文名全文
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        }

        // 按下 Enter 或 Tab 的处理
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SaveAndClose();
            }
            else if (e.Key == Key.Tab)
            {
                e.Handled = true;

                if (sender == NameTextBox)
                {
                    EnNameTextBox.Focus();
                    EnNameTextBox.SelectAll();
                }
                else if (sender == EnNameTextBox)
                {
                    NameTextBox.Focus();
                    NameTextBox.SelectAll();
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAndClose();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveAndClose()
        {
            _station.Name = NameTextBox.Text;
            _station.EnName = EnNameTextBox.Text;

            DialogResult = true;
            Close();
        }
    }
}
