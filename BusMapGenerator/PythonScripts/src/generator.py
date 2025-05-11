import os
import svgwrite
from src.loader import Node, Road, Station, Route
from src.utils import get_papersize


# 生成道路预览画布
def generate_road_previewer(node_dict: dict[int, Node], map_name: str):
    if not os.path.exists(os.path.join("..", "output", map_name)):
        os.makedirs(os.path.join("..", "output", map_name))
    paper_size = get_papersize(node_dict)
    return svgwrite.Drawing(filename=os.path.join("..", "output", map_name, "道路预览.svg"), size=paper_size)


# 生成线路图画布
def generate_mapper(node_dict: dict[int, Node]):
    paper_size = get_papersize(node_dict)
    return svgwrite.Drawing(filename=os.path.join("..", "output", "线路图.svg"), size=paper_size)


# 绘制预览道路
def draw_preview_roads(road_previewer: svgwrite.Drawing,
                       nodes: dict[int, Node],
                       roads: dict[int, Road],
                       paper_size: tuple[float, float],
                       prior_center: tuple[float, float]):
    for road_id, road in roads.items():
        road_previewer.add(
            road_previewer.line(
                start=road.start(nodes, paper_size, prior_center),
                end=road.end(nodes, paper_size, prior_center),
                stroke='rgb(200, 0, 200)',
                stroke_width=3
            )
        )
        print(f"在 道路预览.svg 绘制了道路 {road.name} 和它的端点，"
              f"图上的起点坐标：{road.start(nodes, paper_size, prior_center)}, "
              f"终点坐标：{road.end(nodes, paper_size, prior_center)}")


# 绘制预览道路节点
def draw_preview_nodes(road_previewer: svgwrite.Drawing,
                       nodes: dict[int, Node],
                       paper_size: tuple[float, float],
                       prior_center: tuple[float, float]):
    for node_id, node in nodes.items():
        road_previewer.add(
            road_previewer.circle(
                center=node.geo_coord(paper_size, prior_center),
                r=1.5,
                fill='rgb(100, 246, 255)',
                stroke='none'
            )
        )
        print(f"在 道路预览.svg 绘制了道路节点 {node.name}，图上坐标：{node.geo_coord(paper_size, prior_center)}")


# 绘制预览车站和站点标注
def draw_preview_stations(road_previewer: svgwrite.Drawing,
                          nodes: dict[int, Node],
                          roads: dict[int, Road],
                          stations: dict[int, Station],
                          paper_size: tuple[float, float],
                          prior_center: tuple[float, float]):
    for station_id, station in stations.items():
        road_previewer.add(
            road_previewer.circle(
                center=station.geo_coord(nodes, roads, paper_size, prior_center),
                r=2,
                fill='yellow',
                stroke='black',
                stroke_width=0.8
            )
        )
        text_to_add = road_previewer.text(
            '',
            insert=station.annotate_coord_in_previewer(nodes, roads, paper_size, prior_center),
            text_anchor=station.text_anchor(nodes, roads, paper_size, prior_center)
        )
        text_to_add.add(road_previewer.tspan(station.name, font_size=4.7, fill='black', font_family='SimHei',
                                             x=[station.annotate_coord_in_previewer
                                                (nodes, roads, paper_size, prior_center)[0]], dy=['0em']))
        text_to_add.add(road_previewer.tspan(station.en_name, font_size=3, fill='black', font_family='Arial',
                                             x=[station.annotate_coord_in_previewer
                                                (nodes, roads, paper_size, prior_center)[0]], dy=['1.2em']))
        road_previewer.add(text_to_add)
        print(f"在 道路预览.svg 绘制了站点 {station.name}，"
              f"图上坐标：{station.geo_coord(nodes, roads, paper_size, prior_center)}")
