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
            if (Program.Map != null)
            {
                var nodesList = Program.Nodes.Values.ToList();
                string json = JsonConvert.SerializeObject(nodesList, Formatting.Indented);
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "nodes.json"), json);
            }
        }
        public static void SaveRoads()
        {
            if (Program.Map != null)
            {
                var roadsList = Program.Roads.Values.ToList();
                string json = JsonConvert.SerializeObject(roadsList, Formatting.Indented);
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "roads.json"), json);
            }
        }
        public static void SaveStations()
        {
            if (Program.Map != null)
            {
                var stationsList = Program.Stations.Values.ToList();
                string json = JsonConvert.SerializeObject(stationsList, Formatting.Indented);
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "stations.json"), json);
            }
        }
        public static void SaveRoutes()
        {
            if (Program.Map != null)
            {
                var routesList = Program.Routes.Values.ToList();
                string json = JsonConvert.SerializeObject(routesList, Formatting.Indented);
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "routes.json"), json);
            }
        }
        public static void SaveMtrStations()
        {
            if (Program.Map != null)
            {
                string json = JsonConvert.SerializeObject(Program.MtrStations, Formatting.Indented);
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.Map, "mtr_stations.json"), json);
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
