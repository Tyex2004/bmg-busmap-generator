using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BusMapGenerator
{
    internal class DataLoader
    {
        public static Dictionary<int, Node> LoadNodes(string json)
        {
            List<Node> nodesList = JsonConvert.DeserializeObject<List<Node>>(json);
            Dictionary<int, Node> nodesDict = new Dictionary<int, Node>();
            foreach (Node node in nodesList)
            {
                nodesDict[node.Id] = node;
            }
            return nodesDict;
        }
    }
}
