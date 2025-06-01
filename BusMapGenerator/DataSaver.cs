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
            if (Program.CurrentMap != null)
            {
                var nodesList = Program.Nodes.Values.ToList();
                string json = JsonConvert.SerializeObject(nodesList, Formatting.Indented);
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.CurrentMap, "nodes.json"), json);
            }
        }
        public static void SaveRoads()
        {
            if (Program.CurrentMap != null)
            {
                var roadsList = Program.Roads.Values.ToList();
                string json = JsonConvert.SerializeObject(roadsList, Formatting.Indented);
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.CurrentMap, "roads.json"), json);
            }
        }
        public static void SaveStations()
        {
            if (Program.CurrentMap != null)
            {
                var stationsList = Program.Stations.Values.ToList();
                string json = JsonConvert.SerializeObject(stationsList, Formatting.Indented);
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.CurrentMap, "stations.json"), json);
            }
        }
        public static void SaveRoutes()
        {
            if (Program.CurrentMap != null)
            {
                var routesList = Program.Routes.Values.ToList();
                string json = JsonConvert.SerializeObject(routesList, Formatting.Indented);
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.CurrentMap, "routes.json"), json);
            }
        }
        public static void SaveMtrStations()
        {
            if (Program.CurrentMap != null)
            {
                string json = JsonConvert.SerializeObject(Program.MtrStations, Formatting.Indented);
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.CurrentMap, "mtr_stations.json"), json);
            }
        }
        public static void Save()
        {
            SaveNodes();
            SaveRoads();
            SaveStations();
            SaveRoutes();
            SaveMtrStations();
        }
    }
}
