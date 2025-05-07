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
    class DataSaver  // 输入 ( <数据字典> , <地图名称> ) ，保存数据到文件
    {
        public static void SaveNodes(Dictionary<int, Node> nodesDict, string mapName)
        {
            var nodesList = nodesDict.Values.ToList();
            string json = JsonConvert.SerializeObject(nodesList, Formatting.Indented);
            File.WriteAllText(Path.Combine("data", mapName, "nodes.json"), json);
        }
    }
}
