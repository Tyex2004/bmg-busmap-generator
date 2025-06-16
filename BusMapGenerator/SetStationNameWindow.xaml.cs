using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BusMapGenerator
{
    public partial class SetStationNameWindow : Window
    {
        private readonly Station _station;

        public SetStationNameWindow(Station station)
        {
            _station = station;
            InitializeComponent();

            Loaded += SetStationNameWindow_Loaded;

            // 绑定按键事件，处理 Enter 和 Tab
            NameTextBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            EnNameTextBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            SetMtrSta.PreviewKeyDown += TextBox_PreviewKeyDown;
            SetNote.PreviewKeyDown += TextBox_PreviewKeyDown;
        }

        private void SetStationNameWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化文本框内容
            NameTextBox.Text = _station.Name ?? string.Empty;
            EnNameTextBox.Text = _station.EnName ?? string.Empty;
            SetMtrSta.Text = string.Join(", ", _station.ConnectsMtr);
            SetNote.Text = string.Join(", ", _station.Note);

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
                    SetMtrSta.Focus();
                    SetMtrSta.SelectAll();
                }
                else if (sender == SetMtrSta)
                {
                    SetNote.Focus();
                    SetNote.SelectAll();
                }
                else if (sender == SetNote)
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
            _station.ConnectsMtr = Utils.SmartSplit(SetMtrSta.Text);
            _station.Note = Utils.SmartSplit(SetNote.Text);

            DialogResult = true;
            Close();
        }
    }
}
