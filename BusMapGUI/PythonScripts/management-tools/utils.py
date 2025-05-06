import json
from pathlib import Path


def save_nodes(nodes, filepath):
    data = [
        {"id": n.id, "name": n.name, "coord": n.coord}
        for n in nodes
    ]
    _save_json(data, filepath)


def save_roads(roads, filepath):
    data = [
        {"id": r.id, "name": r.name, "nodes": r.nodes}
        for r in roads
    ]
    _save_json(data, filepath)


def save_stations(stations, filepath):
    data = [
        {
            "id": s.id,
            "name": s.name,
            "en_name": s.en_name,
            "road_id": s.road_id,
            "on_road_pos": s.on_road_pos,
            "side": s.side,
            "connects_mtr": s.connects_mtr,
            "note": s.note
        }
        for s in stations
    ]
    _save_json(data, filepath)


def save_routes(routes, filepath):
    data = [
        {
            "id": r.id,
            "name": r.name,
            "color": r.color,
            "start": r.start,
            "path": r.path,
            "end": r.end
        }
        for r in routes
    ]
    _save_json(data, filepath)


def _save_json(data, filepath):
    filepath = Path(filepath)
    filepath.parent.mkdir(parents=True, exist_ok=True)
    with open(filepath, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
