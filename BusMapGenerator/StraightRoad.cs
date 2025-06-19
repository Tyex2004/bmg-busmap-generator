using ExCSS;
using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusMapGenerator
{
    public class StraightRoad
    {
        public List<int> NodeIds { get; set; } = [];
        // 东西向路、斜路按 x 坐标向右排序，南北向路按 y 坐标向上排序
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
        public List<int> SortedRoadIds
        {
            get
            {
                //Debug.WriteLine($"正在计算道路 [{string.Join(", ", RoadIds)}] 在直线道路上的排序顺序");
                List<int> returnSet = [];
                for (int i = 0; i < NodeIds.Count - 1; i++)
                {
                    int nodeId = SortedNodeIds[i];
                    int roadId = Program.Nodes[nodeId].RoadsId[Direction];
                    returnSet.Add(roadId);
                }
                return returnSet;
            }
        }
        public List<Road> SortedRoads => [.. SortedRoadIds.Select(x => Program.Roads[x])];
        public decimal Length
        {
            get
            {
                decimal returnDecimal = 0;
                foreach (var roadId in SortedRoadIds)
                {
                    returnDecimal += Program.Roads[roadId].Length.Coefficient;
                }
                return returnDecimal;
            }
        }
        // LinearFunction: Ax + By + C = 0
        public decimal A
        {
            get
            {
                return Direction switch
                {
                    0 => 1,      // x = const
                    1 => 1,      // y = x + c
                    2 => 0,      // y = const
                    3 => 1,      // y = -x + c
                    _ => 0       // 默认安全处理
                };
            }
        }
        public decimal B
        {
            get
            {
                return Direction switch
                {
                    0 => 0,      // x = const
                    1 => -1,     // y = x + c → x - y + c = 0
                    2 => 1,      // y = const
                    3 => 1,      // y = -x + c → x + y + c = 0
                    _ => 0
                };
            }
        }
        public decimal C
        {
            get
            {
                if (NodeIds.Count != 0)
                {
                    decimal x = Program.Nodes[NodeIds[0]].JSONCoord[0];
                    decimal y = Program.Nodes[NodeIds[0]].JSONCoord[1];
                    return -A * x - B * y;
                }
                else return 0;
            }
        }

        public Dictionary<int /* Slot */, RoutePart[]> SlotMatrix
        {
            get
            {
                Dictionary<int, RoutePart[]> returnDict = [];
                // 工具方法：把篮子的内容横贴在卡槽矩阵中，从靠近中心线尝试贴，如果存在卡槽被覆盖，就往下一层卡槽粘贴
                void addBasketToReturnDict(List<RoutePart> basket)
                {
                    // 空篮子直接跳过
                    if (basket.Count == 0) return;

                    // 找出 basket 中首尾所在的道路在当前 StraightRoad 的索引（SortedRoadIds 是当前直线道路上所有 Road 的 ID 列表）
                    int segStart = SortedRoadIds.IndexOf(basket.First().Road.Id);
                    int segEnd = SortedRoadIds.IndexOf(basket.Last().Road.Id);

                    // 如果找不到就跳过，可能不是此直线道路的段
                    if (segStart == -1 || segEnd == -1)
                    {
                        Debug.WriteLine($"[Warning] 路线 {basket.First().Route.Id} 中的部分道路不在当前 StraightRoad 中");
                        return;
                    }

                    // 确保 segStart < segEnd，方便后续循环处理
                    if (segStart > segEnd) (segStart, segEnd) = (segEnd, segStart);

                    // 整个矩阵的宽度，即道路段数
                    int width = SortedRoadIds.Count;

                    // 判断路线方向，决定 slot 是正还是负
                    bool isForward = basket[0].IsForwardOnStraightRoad;
                    int slotSign = isForward ? 1 : -1;

                    // 找出所有已使用的同方向 slot，例如如果正在贴正向线，就找 >0 的 slot
                    var usedSlots = returnDict.Keys.Where(k => Math.Sign(k) == slotSign).ToList();

                    // 从靠近中线的 slot 开始尝试：1, 2, 3... 或 -1, -2, -3...
                    int nextTry = 1;

                    while (true)
                    {
                        int slotId = nextTry * slotSign;

                        // 尝试获取 slot，如果不存在就创建一个空数组
                        if (!returnDict.TryGetValue(slotId, out var slot))
                        {
                            slot = new RoutePart[width]; // 初始化一个空的卡槽列（矩阵一行）
                            returnDict[slotId] = slot;
                        }

                        // 检查该 slot 的 segStart ~ segEnd 区间是否已有内容（避免重叠）
                        bool conflict = false;
                        for (int i = segStart; i <= segEnd; i++)
                        {
                            if (slot[i] != null)
                            {
                                conflict = true;
                                break;
                            }
                        }

                        // 如果没有冲突，就可以把篮子中的 RoutePart 粘贴到 slot 上
                        if (!conflict)
                        {
                            for (int i = 0; i < basket.Count; i++)
                            {
                                slot[segStart + i] = basket[i];
                            }
                            return; // 粘贴成功，返回
                        }

                        // 否则换下一个 slot（例如从 slot 1 换到 slot 2）
                        nextTry++;
                    }
                }
                // 遍历所有线路
                foreach (Route route in Program.Routes.Values)
                {
                    Debug.WriteLine($"正在尝试将线路 {route.Id} 加入道路 {SortedRoadIds[0]} 所在的直线道路的卡槽矩阵中");
                    List<RoutePart> basket = [];
                    // 如果第一段道路就在此直线道路上，就可以开始放进篮子了
                    if (route[0].StraightRoad == this)
                    {
                        Debug.WriteLine($"第一段道路 {route[0].Road.Id} 就在此直线道路上，可以直接放进篮子");
                        basket.Add(route[0]);
                    }
                    for (int i = 1; i < route.Roads.Count; i++)
                    {
                        if (route[i].StraightRoad == this && route[i].IsForwardOnStraightRoad == route[i - 1].IsForwardOnStraightRoad)
                        {
                            // 符合条件都放进篮子
                            basket.Add(route[i]);
                            if (i == route.Roads.Count - 1)
                            {
                                // 如果此时已经是最后一条道路，就提交篮子
                                addBasketToReturnDict(basket);
                            }
                        }
                        // 方向改变也要提交篮子
                        else if (route[i].StraightRoad == this && route[i].IsForwardOnStraightRoad != route[i - 1].IsForwardOnStraightRoad)
                        {
                            addBasketToReturnDict(basket);
                            basket.Clear();
                            basket.Add(route[i]);
                            if (i == route.Roads.Count - 1)
                            {
                                addBasketToReturnDict(basket);
                            }
                        }
                        else
                        {
                            // 上述条件都不符合，但篮子有内容，提交篮子
                            if (basket.Count != 0)
                            {
                                addBasketToReturnDict(basket);
                                basket.Clear();
                            }
                        }
                    }
                }
                return returnDict;
            }
        }
        public decimal COfSlot(int slot)
        {
            if (slot == 0) return C;

            decimal distance = Math.Abs(slot) * Program.BoldOfRoutes;
            decimal shift = distance * (decimal)Math.Sqrt((double)(A * A + B * B));

            return C + Math.Sign(slot) * shift;
        }
    }
}