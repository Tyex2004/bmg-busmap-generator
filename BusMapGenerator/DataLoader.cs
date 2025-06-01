using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BusMapGenerator
{
    internal class DataLoader
    {
        public static Dictionary<int, Node> LoadNodes()
        {
            if (Program.CurrentMap != null)
            {
                string json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.CurrentMap, "nodes.json"));
                List<Node> nodesList = JsonConvert.DeserializeObject<List<Node>>(json) ?? [];
                Dictionary<int, Node> nodesDict = nodesList.ToDictionary(node => node.Id, node => node);
                return nodesDict;
            }
            else
            {
                return [];
            }
        }
        public static Dictionary<int, Road> LoadRoads()
        {
            if (Program.CurrentMap != null)
            {
                string json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.CurrentMap, "roads.json"));
                List<Road> roadsList = JsonConvert.DeserializeObject<List<Road>>(json) ?? [];
                Dictionary<int, Road> roadsDict = roadsList.ToDictionary(road => road.Id, road => road);
                return roadsDict;
            }
            else
            {
                return [];
            }
        }
        public static Dictionary<int, Station> LoadStations()
        {
            if (Program.CurrentMap != null)
            {
                string json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.CurrentMap, "stations.json"));
                List<Station> stationsList = JsonConvert.DeserializeObject<List<Station>>(json) ?? [];
                Dictionary<int, Station> stationsDict = stationsList.ToDictionary(station => station.Id, station => station);
                return stationsDict;
            }
            else
            {
                return [];
            }
        }
        public static Dictionary<int, Route> LoadRoutes()
        {
            if (Program.CurrentMap != null)
            {
                string json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.CurrentMap, "routes.json"));
                List<Route> routesList = JsonConvert.DeserializeObject<List<Route>>(json) ?? [];
                Dictionary<int, Route> routesDict = routesList.ToDictionary(route => route.Id, route => route);
                return routesDict;
            }
            else
            {
                return [];
            }
        }
        public static List<MtrStation> LoadMtrStations()
        {
            if (Program.CurrentMap != null)
            {
                string json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Program.CurrentMap, "mtr_stations.json"));
                List<MtrStation> mtrStationsList = JsonConvert.DeserializeObject<List<MtrStation>>(json) ?? [];
                return mtrStationsList;
            }
            else
            {
                return [];
            }
        }
    }
}
