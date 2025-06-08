using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusMapGenerator
{
    public class StraightRoad
    {
        public List<int> NodeIds { get; set; } = [];
        public List<int> SortedNodeIds
        {
            get
            {
                List<int> returnList = [];
                if (Direction is 1 or 2 or 3)
                {
                    returnList = [.. NodeIds.OrderBy(x => Program.Nodes[x].JSONCoord[0])];
                }
                else
                {
                    returnList = [.. NodeIds.OrderBy(x => Program.Nodes[x].JSONCoord[1])];
                }
                return returnList;
            }
        }
        public int Direction { get; set; }
        public List<int> RoadIds
        {
            get
            {
                List<int> returnList = [];
                foreach (var road in Program.Roads.Values)
                {
                    foreach (int nodeId in NodeIds)
                    {
                        if (Program.Nodes[nodeId].RoadsId[Direction] == road.Id)
                        {
                            if (!returnList.Contains(road.Id))
                            {
                                returnList.Add(road.Id);
                            }
                        }
                        if (Program.Nodes[nodeId].RoadsId[Utils.SwapDirection(Direction)] == road.Id)
                        {
                            if (!returnList.Contains(road.Id))
                            {
                                returnList.Add(road.Id);
                            }
                        }
                    }
                }
                return returnList;
            }
        }
    }
}
