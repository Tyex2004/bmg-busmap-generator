import os
import svgwrite
from src.loader import Node, Road, Station, Route
from src.utils import get_papersize
from src.utils import coord_shifter


# 生成道路预览画布
def generate_road_previewer(node_dict: dict[int, Node]):
    paper_size = get_papersize(node_dict)
    return svgwrite.Drawing(filename=os.path.join("..", "output", "道路预览.svg"), size=paper_size)


# 生成线路图画布
def generate_mapper(node_dict: dict[int, Node]):
    paper_size = get_papersize(node_dict)
    return svgwrite.Drawing(filename=os.path.join("..", "output", "线路图.svg"), size=paper_size)


# 转化道路节点坐标
def shift_nodes_coord(node_dict: dict[int, Node], paper_size: tuple[float, float], origin_center_coord: tuple[float, float]):
    for node_id, node in node_dict.items():
        node.geo_coord = coord_shifter(node.coord, paper_size, origin_center_coord)


# 绘制预览道路
def draw_preview_roads(road_previewer: svgwrite.Drawing, road_dict: dict[int, Road], node_dict: dict[int, Node]):
    for road_id, road in road_dict.items():
        road.start = node_dict[road.nodes[0]].geo_coord
        road.end = node_dict[road.nodes[1]].geo_coord
        road_previewer.add(
            road_previewer.line(
                start=road.start,
                end=road.end,
                stroke='rgb(200, 0, 200)',
                stroke_width=2
            )
        )
        road_previewer.add(
            road_previewer.circle(
                center=road.start,
                r=1,
                fill='rgb(200, 0, 200)',
                stroke='none'
            )
        )
        road_previewer.add(
            road_previewer.circle(
                center=road.end,
                r=1,
                fill='rgb(200, 0, 200)',
                stroke='none'
            )
        )
        print(f"在 道路预览.svg 绘制了道路 {road.name} 和它的端点，图上的起点坐标：{road.start}, 终点坐标：{road.end}")


# 绘制预览车站
def draw_preview_stations(road_previewer: svgwrite.Drawing, road_dict: dict[int, Road], station_dict: dict[int, Station]):
    for station_id, station in station_dict.items():
        x0, y0 = road_dict[station.road_id].start
        x1, y1 = road_dict[station.road_id].end
        ratio = station.on_road_pos
        station.geo_coord = x0 + (x1 - x0) * ratio, y0 + (y1 - y0) * ratio
        road_previewer.add(
            road_previewer.circle(
                center=station.geo_coord,
                r=2,
                fill='yellow',
                stroke='black',
                stroke_width=0.8
            )
        )
        print(f"在 道路预览.svg 绘制了站点 {station.name}，图上坐标：{station.geo_coord}")