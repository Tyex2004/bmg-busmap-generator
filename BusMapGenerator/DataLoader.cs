﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BusMapGenerator
{
    internal class DataLoader  // 输入 <地图名称> ，输出 <数据字典>
    {
        public static Dictionary<int, Node> LoadNodes()
        {
            string json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.CurrentMap, "nodes.json"));
            List<Node> nodesList = JsonConvert.DeserializeObject<List<Node>>(json) ?? [];
            Dictionary<int, Node> nodesDict = nodesList.ToDictionary(node => node.Id, node => node);
            return nodesDict;
        }
    }
}
