import json
from pathlib import Path
from src.model import Node, Road, Station, Route


# 道路节点包含：ID、名称、坐标，其中坐标是包含横坐标和纵坐标的元组
def load_nodes(path: Path) -> dict[int, Node]:
    with path.open(encoding='utf-8') as f:
        data = json.load(f)
        return {
            d['id']: Node(id=d['id'], name=d['name'], coord=(d['coord'][0], d['coord'][1]))
            for d in data
        }


# 道路包含：ID、名称、几何信息，其中几何信息是包含起点和终点的元组
def load_roads(path: Path) -> dict[int, Road]:
    with path.open(encoding='utf-8') as f:
        data = json.load(f)
        return {
            d['id']: Road(id=d['id'], name=d['name'], nodes=(d['nodes'][0], d['nodes'][1]))
            for d in data
        }


# 站点包含：ID、名称、英文名称、位置、标注在哪一侧、连接地铁站、备注
def load_stations(path: Path) -> dict[int, Station]:
    with path.open(encoding='utf-8') as f:
        data = json.load(f)
        return {
            d['id']: Station(
                id=d['id'],
                name=d['name'],
                en_name=d['en_name'],
                road_id=d['road_id'],
                on_road_pos=d['on_road_pos'],
                side=d['side'],
                connects_mtr=d['connects_mtr'],
                note=d['note']
            ) for d in data
        }


# 线路包含：ID、名称、颜色、起点、终点、路径，其中路径是包含道路ID和站点ID的列表
def load_routes(path: Path) -> dict[int, Route]:
    with path.open(encoding='utf-8') as f:
        data = json.load(f)
        return {
            d['id']: Route(
                id=d['id'],
                name=d['name'],
                color=d['color'],
                start=d['start'],
                path=[(seg[0], seg[1]) for seg in d['path']],
                end=d['end']
            ) for d in data
        }


# 地铁站包含：名称、途径线路列表
def load_mtr_stations(path: Path) -> dict[str, list[str]]:
    with path.open(encoding='utf-8') as f:
        data = json.load(f)
        return {d['name']: d['routes'] for d in data}
