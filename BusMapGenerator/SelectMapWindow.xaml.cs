using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace BusMapGenerator
{
    public partial class SelectMapWindow : Window
    {
        public string? SelectedMap { get; private set; } = null;

        public SelectMapWindow()
        {
            InitializeComponent();

            MapComboBox.Items.Add("<无>");
            MapComboBox.SelectedIndex = 0;

            // 获取应用路径下的 data 文件夹下所有子文件夹
            string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            if (Directory.Exists(dataPath))
            {
                var dirs = Directory.GetDirectories(dataPath);
                foreach (var dir in dirs)
                { 
                    MapComboBox.Items.Add(Path.GetFileName(dir));
                }
            }
        }

        private void MapComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = MapComboBox.SelectedItem;

            if (selectedItem is ComboBoxItem comboBoxItem)
            {
                OkButton.IsEnabled = comboBoxItem.Content?.ToString() != "<无>";
            }
            else if (selectedItem != null)
            {
                OkButton.IsEnabled = selectedItem.ToString() != "<无>";
            }
            else
            {
                OkButton.IsEnabled = false;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = MapComboBox.SelectedItem;

            if (selectedItem is ComboBoxItem comboBoxItem)
            {
                SelectedMap = comboBoxItem.Content.ToString();
            }
            else if (selectedItem != null)
            {
                SelectedMap = selectedItem.ToString();
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
