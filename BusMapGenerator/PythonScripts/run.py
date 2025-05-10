import os
import sys
from pathlib import Path
from src.loader import load_nodes, load_roads, load_stations, load_routes, load_mtr_stations
from src.generator import (generate_road_previewer, get_papersize, shift_nodes_coord,
                           draw_preview_roads, draw_preview_stations)
from src.utils import get_origin_center_coord

# 读取数据
map_name = sys.argv[1]
data_dir = Path(os.path.join("..", "data", map_name))             # 数据文件夹
nodes = load_nodes(data_dir / "nodes.json")             # 道路节点字典（id: 对象）
roads = load_roads(data_dir / "roads.json")             # 道路字典（id: 对象）
stations = load_stations(data_dir / "stations.json")    # 站点字典（id: 对象）
routes = load_routes(data_dir / "routes.json")          # 线路字典（id: 对象）
mtr_stations = load_mtr_stations(data_dir / "mtr_stations.json")  # 地铁站字典（站名: [**线路]）

print(f"共加载节点 {len(nodes)} 个，道路 {len(roads)} 条，站点 {len(stations)} 个，\
路线 {len(routes)} 条，地铁站 {len(mtr_stations)} 个。")

# 画布与基本参数
road_previewer = generate_road_previewer(nodes, map_name)         # 道路预览画布
paper_size = get_papersize(nodes)                       # 纸张尺寸
prior_center = get_origin_center_coord(nodes)           # 节点原几何中心

# 基本参数导出 txt
for var in ["paper_size", "prior_center"]:
    for key, value in {1: "x", 2: "y"}.items():
        with open(data_dir / f"{var}_{value}.txt", "w") as f:
            f.write(str(eval(var)[key - 1]))

# 绘制预览道路
shift_nodes_coord(nodes, paper_size, prior_center)      # 移动道路节点坐标
draw_preview_roads(road_previewer, roads, nodes)        # 绘制道路
draw_preview_stations(road_previewer, roads, stations)  # 绘制站点

# 保存结果
road_previewer.save()
print(f"----------\n保存内容在 output/{map_name} 文件夹中。")
