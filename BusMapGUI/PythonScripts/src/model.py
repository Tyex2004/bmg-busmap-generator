from dataclasses import dataclass, field


@dataclass
class Node:
    # 从 JSON 加载
    id: int
    name: str
    coord: tuple[float, float]
    # 后期计算几何属性
    geo_coord: tuple[float, float] = field(init=False)


@dataclass
class Road:
    # 从 JSON 加载
    id: int
    name: str
    nodes: tuple[int, int]
    # 后期计算几何属性
    start: tuple[float, float] = field(init=False)
    end: tuple[float, float] = field(init=False)


@dataclass
class Station:
    # 从 JSON 加载
    id: int
    name: str
    en_name: str
    road_id: int
    on_road_pos: float
    side: str
    connects_mtr: list[str]
    note: list[str]
    # 后期计算几何属性
    geo_coord: tuple[float, float] = field(init=False)


@dataclass
class Route:
    id: int
    name: str
    color: str
    start: int
    path: list[tuple[int, list]]
    end: int
