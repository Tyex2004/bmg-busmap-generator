using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace BusMapGenerator
{
    public partial class MtrStationManagerWindow : Window
    {
        // 字段：view model 列表
        private ObservableCollection<MtrStationViewModel> viewModelList = [];

        // 构造函数
        public MtrStationManagerWindow()
        {
            InitializeComponent();
            LoadData();
        }

        // 加载数据
        private void LoadData()
        {
            // model 映射至 view model
            var list = Program.MtrStations;
            viewModelList = new ObservableCollection<MtrStationViewModel>(
                list.Select(m => new MtrStationViewModel
                {
                    Name = m.Name,
                    Routes = m.Routes
                }));
            // view 绑定 view model
            MtrStationGrid.ItemsSource = viewModelList;
        }

        // 保存数据
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var invalidItems = viewModelList.Where(x => string.IsNullOrWhiteSpace(x.Name)).ToList();
            foreach (var item in invalidItems)
                viewModelList.Remove(item);
            // 清除 viewModelList 中 Name 为空或空白的项
            viewModelList = new ObservableCollection<MtrStationViewModel>(
                viewModelList.Where(x => !string.IsNullOrWhiteSpace(x.Name)));

            // 检查重复站名（忽略大小写和空格）
            List<string> duplicates = viewModelList.GroupBy(x => x.Name.Trim())
                                  .Where(g => g.Count() > 1)
                                  .Select(g => g.Key)
                                  .ToList();

            if (duplicates.Count != 0)
            {
                MessageBox.Show("发现重复站名：" + string.Join(", ", duplicates) + "。\n请处理后再进行保存", "提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 映射为 MtrStation
            Program.MtrStations = viewModelList
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new MtrStation
                {
                    Name = x.Name.Trim(),
                    Routes = string.IsNullOrWhiteSpace(x.RoutesString)
                             ? []
                             : x.RoutesString.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(s => s.Trim())
                                             .Where(s => !string.IsNullOrWhiteSpace(s))
                                             .ToList()
                })
                .ToList();

            DataSaver.Save();
        }


        private static readonly char[] separator = [',', '，'];

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string keyword = SearchBox.Text.Trim();
            if (string.IsNullOrEmpty(keyword)) return;

            IEnumerable<MtrStationViewModel>? items = MtrStationGrid.ItemsSource as IEnumerable<MtrStationViewModel>;
            MtrStationViewModel? found = items?.FirstOrDefault(p => p.Name == keyword); // 按 Name 查找

            if (found != null)
            {
                MtrStationGrid.SelectedItem = found;
                MtrStationGrid.ScrollIntoView(found); // 定位到那一行
            }
            else
            {
                MessageBox.Show("未找到匹配项");
            }
        }

        private void Exportor(object sender, RoutedEventArgs e)
        {

        }

        private void Importor(object sender, RoutedEventArgs e)
        {

        }
    }

    public class MtrStationViewModel
    {
        public string Name { get; set; } = "";
        public List<string> Routes { get; set; } = [];

        public string RoutesString
        {
            get => Routes == null ? "" : string.Join(", ", Routes);
            set => Routes = string.IsNullOrWhiteSpace(value)
                ? []
                : value.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim())
                       .ToList();
        }


        private static readonly char[] separator = [',', '，'];
    }
}
