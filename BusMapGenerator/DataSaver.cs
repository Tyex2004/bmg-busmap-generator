using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BusMapGenerator
{
    class DataSaver  // 保存数据到文件
    {
        public static void SaveNodes()
        {
            var nodesList = Program.Nodes.Values.ToList();
            string json = JsonConvert.SerializeObject(nodesList, Formatting.Indented);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.CurrentMap, "nodes.json"), json);
        }
    }
}
