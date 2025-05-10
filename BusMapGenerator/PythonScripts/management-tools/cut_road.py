from pathlib import Path
from src.loader import load_nodes, load_roads, load_stations
from utils import save_nodes, save_roads, save_stations
import argparse
import shutil
from datetime import datetime


def cut_road(data_dir: Path, road_id: int, cut_ratio: float):
    assert 0 < cut_ratio < 1, "cut_ratio 必须在 0 到 1 之间"

    nodes = load_nodes(data_dir / "nodes.json")
    roads = load_roads(data_dir / "roads.json")
    stations = load_stations(data_dir / "stations.json")

    road = next((r for r in roads if r.id == road_id), None)
    if not road:
        raise ValueError(f"未找到 ID 为 {road_id} 的道路")

    node_start = next(n for n in nodes if n.id == road.nodes[0])
    node_end = next(n for n in nodes if n.id == road.nodes[1])

    new_node_id = max(n.id for n in nodes) + 1
    new_coord = [
        node_start.coord[0] + cut_ratio * (node_end.coord[0] - node_start.coord[0]),
        node_start.coord[1] + cut_ratio * (node_end.coord[1] - node_start.coord[1])
    ]
    nodes.append(type(node_start)(id=new_node_id, name=f"切点_{road.id}", coord=new_coord))

    roads = [r for r in roads if r.id != road_id]
    new_road_id_1 = max(r.id for r in roads) + 1
    new_road_id_2 = new_road_id_1 + 1
    roads.append(type(road)(id=new_road_id_1, name=f"{road.name}_A", nodes=[road.nodes[0], new_node_id]))
    roads.append(type(road)(id=new_road_id_2, name=f"{road.name}_B", nodes=[new_node_id, road.nodes[1]]))

    for station in stations:
        if station.road_id == road_id:
            if station.on_road_pos < cut_ratio:
                station.road_id = new_road_id_1
                station.on_road_pos /= cut_ratio
            else:
                station.road_id = new_road_id_2
                station.on_road_pos = (station.on_road_pos - cut_ratio) / (1 - cut_ratio)

    save_nodes(data_dir / "nodes.json", nodes)
    save_roads(data_dir / "roads.json", roads)
    save_stations(data_dir / "stations.json", stations)
    print(f"✅ 道路 {road_id} 成功切断，新节点 ID：{new_node_id}，新道路 ID：{new_road_id_1}, {new_road_id_2}")


def backup_data(data_dir: Path):
    timestamp = datetime.now().strftime("%y%m%d%H%M%S")
    backup_dir = data_dir.parent / f"data-backup-{timestamp}"
    shutil.copytree(data_dir, backup_dir)
    print(f"🗂️ 数据已备份至：{backup_dir}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="🚧 将指定道路按比例切断")
    parser.add_argument("road_id", type=int, help="要切断的道路 ID")
    parser.add_argument("cut_ratio", type=float, help="切断位置比例，范围 (0,1)")
    parser.add_argument("--data_dir", type=str, default="data", help="数据目录，默认为 data/")
    args = parser.parse_args()

    data_path = Path(args.data_dir).resolve()
    backup_data(data_path)
    cut_road(data_path, args.road_id, args.cut_ratio)
