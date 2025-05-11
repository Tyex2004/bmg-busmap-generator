import math
from dataclasses import dataclass


@dataclass
class Node:
    id: int
    name: str
    coord: tuple[float, float]

    def geo_coord(self,
                  paper_size: tuple[float, float],
                  prior_center: tuple[float, float]
                  ) -> tuple[float, float]:
        shifted_x = self.coord[0] - prior_center[0] + paper_size[0] / 2
        shifted_y = prior_center[1] - self.coord[1] + paper_size[1] / 2
        return shifted_x, shifted_y


@dataclass
class Road:
    id: int
    name: str
    nodes: tuple[int, int]

    def start(self,
              nodes: dict[int, Node],
              paper_size: tuple[float, float],
              prior_center: tuple[float, float]
              ) -> tuple[float, float]:
        return nodes[self.nodes[0]].geo_coord(paper_size, prior_center)

    def end(self,
            nodes: dict[int, Node],
            paper_size: tuple[float, float],
            prior_center: tuple[float, float]
            ) -> tuple[float, float]:
        return nodes[self.nodes[1]].geo_coord(paper_size, prior_center)

    def angle(self,
              nodes: dict[int, Node],
              paper_size: tuple[float, float],
              prior_center: tuple[float, float]
              ) -> float:
        x0, y0 = self.start(nodes, paper_size, prior_center)
        x1, y1 = self.end(nodes, paper_size, prior_center)
        dx = x1 - x0
        dy = y1 - y0
        return math.degrees(math.atan2(dy, dx))


@dataclass
class Station:
    id: int
    name: str
    en_name: str
    road_id: int
    on_road_pos: float
    side: str
    connects_mtr: list[str]
    note: list[str]

    def geo_coord(self,
                  nodes: dict[int, Node],
                  roads: dict[int, Road],
                  paper_size: tuple[float, float],
                  prior_center: tuple[float, float]
                  ) -> tuple[float, float]:
        x0, y0 = roads[self.road_id].start(nodes, paper_size, prior_center)
        x1, y1 = roads[self.road_id].end(nodes, paper_size, prior_center)
        return x0 + (x1 - x0) * self.on_road_pos, y0 + (y1 - y0) * self.on_road_pos

    def geo_side(self,
                 nodes: dict[int, Node],
                 roads: dict[int, Road],
                 paper_size: tuple[float, float],
                 prior_center: tuple[float, float]
                 ):
        angle = roads[self.road_id].angle(nodes, paper_size, prior_center)
        if abs(angle) <= 15:
            if self.side == 'left':
                return 'up'
            else:
                return 'down'
        elif abs(180 - angle) <= 15:
            if self.side == 'left':
                return 'down'
            else:
                return 'up'
        elif 15 < angle < 165:
            return 'right' if self.side == 'left' else 'left'
        else:
            return 'left' if self.side == 'left' else 'right'

    def annotate_coord_in_previewer(self,
                                    nodes: dict[int, Node],
                                    roads: dict[int, Road],
                                    paper_size: tuple[float, float],
                                    prior_center: tuple[float, float]
                                    ) -> tuple[float, float]:
        # 获取道路起止点坐标
        x0, y0 = roads[self.road_id].start(nodes, paper_size, prior_center)
        x1, y1 = roads[self.road_id].end(nodes, paper_size, prior_center)

        # 计算道路方向向量
        dx = x1 - x0
        dy = y1 - y0

        # 单位化，并求垂直向量（顺时针旋转90度）
        length = (dx ** 2 + dy ** 2) ** 0.5
        ux, uy = dx / length, dy / length
        per_x, per_y = -uy, ux  # 垂直方向向量

        # 确定标注方向（左为垂直向量方向，右为相反方向）
        direction = 1 if self.side == 'right' else -1

        # 站点坐标
        sx, sy = self.geo_coord(nodes, roads, paper_size, prior_center)

        # 标注坐标 = 站点坐标 + 6 * 垂直方向单位向量
        # ax = sx + direction * per_x * 6
        # ay = sy + direction * per_y * 6
        if self.geo_side(nodes, roads, paper_size, prior_center) == 'up':
            ax = sx
            ay = sy + direction * per_y * 8.2
        elif self.geo_side(nodes, roads, paper_size, prior_center) == 'down':
            ax = sx
            ay = sy + direction * per_y * 8.2
        elif self.geo_side(nodes, roads, paper_size, prior_center) == 'left':
            ax = sx + direction * per_x * 3.4
            ay = sy + direction * per_y * 8.2
        else:
            ax = sx + direction * per_x * 3.4
            ay = sy + direction * per_y * 8.2

        return ax, ay

    def text_anchor(self,
                    nodes: dict[int, Node],
                    roads: dict[int, Road],
                    paper_size: tuple[float, float],
                    prior_center: tuple[float, float]
                    ) -> str:
        if (self.geo_side(nodes, roads, paper_size, prior_center) == 'up'
                or self.geo_side(nodes, roads, paper_size, prior_center) == 'down'):
            return 'middle'
        elif self.geo_side(nodes, roads, paper_size, prior_center) == 'left':
            return 'end'
        else:
            return 'start'


@dataclass
class Route:
    # JSON 字段
    id: int
    name: str
    color: str
    start: int
    path: list[tuple[int, list]]
    end: int
