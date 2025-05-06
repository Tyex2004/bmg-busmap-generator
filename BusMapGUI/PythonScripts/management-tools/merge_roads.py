from pathlib import Path
from src.loader import load_nodes, load_roads, load_stations
from utils import save_roads, save_stations


def merge_roads(data_dir, road_id1, road_id2):
    roads = load_roads(data_dir / "roads.json")
    stations = load_stations(data_dir / "stations.json")

    r1 = next((r for r in roads if r.id == road_id1), None)
    r2 = next((r for r in roads if r.id == road_id2), None)
    if not r1 or not r2:
        raise ValueError("未找到目标道路")

    # 判断连接关系
    if r1.nodes[1] != r2.nodes[0]:
        raise ValueError("两个道路不连续，不能合并")

    new_road_id = max(r.id for r in roads if r.id not in [road_id1, road_id2]) + 1
    new_road = type(r1)(id=new_road_id, name=f"{r1.name}_{r2.name}", nodes=[r1.nodes[0], r2.nodes[1]])

    roads = [r for r in roads if r.id not in [road_id1, road_id2]]
    roads.append(new_road)

    # 更新站点
    for s in stations:
        if s.road_id == road_id1:
            s.road_id = new_road_id
            s.on_road_pos *= 0.5
        elif s.road_id == road_id2:
            s.road_id = new_road_id
            s.on_road_pos = 0.5 + s.on_road_pos * 0.5

    save_roads(data_dir / "roads.json", roads)
    save_stations(data_dir / "stations.json", stations)
    print(f"道路 {road_id1} 与 {road_id2} 合并为新道路 {new_road_id}")