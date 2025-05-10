from src.loader import load_nodes, load_roads, load_stations, load_routes, save_nodes, save_roads, save_stations, save_routes
from pathlib import Path


def reassign_ids(data_dir):
    nodes = load_nodes(data_dir / "nodes.json")
    roads = load_roads(data_dir / "roads.json")
    stations = load_stations(data_dir / "stations.json")
    routes = load_routes(data_dir / "routes.json")

    def remap(items):
        sorted_items = sorted(items, key=lambda x: x.id)
        mapping = {item.id: i + 1 for i, item in enumerate(sorted_items)}
        for i, item in enumerate(sorted_items):
            item.id = i + 1
        return sorted_items, mapping

    nodes, map_nodes = remap(nodes)
    roads, map_roads = remap(roads)
    stations, map_stations = remap(stations)
    routes, map_routes = remap(routes)

    for r in roads:
        r.nodes = [map_nodes[n] for n in r.nodes]

    for s in stations:
        s.road_id = map_roads[s.road_id]

    for route in routes:
        route.start = map_stations[route.start]
        route.end = map_stations[route.end]
        new_path = []
        for pair in route.path:
            road_id, stops = pair
            new_path.append([map_roads[road_id], [map_stations[s] for s in stops]])
        route.path = new_path

    save_nodes(data_dir / "nodes.json", nodes)
    save_roads(data_dir / "roads.json", roads)
    save_stations(data_dir / "stations.json", stations)
    save_routes(data_dir / "routes.json", routes)
    print("ID 重排完成")
