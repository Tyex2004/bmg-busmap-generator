from src.model import Node


# 判断道路节点是否存在
def whether_nodes_exist(node_dict: dict[int, Node]):
    if not node_dict:
        raise ValueError("找不到道路节点，请检查数据是否正常")


# 获取道路节点最大 ID
def get_nodes_max_id(node_dict: dict[int, Node]) -> int:
    whether_nodes_exist(node_dict)
    return max(node_dict.values(), key=lambda node: node.id).id


# 获取道路节点最小横坐标
def get_nodes_min_x(node_dict: dict[int, Node]) -> float:
    whether_nodes_exist(node_dict)
    return min(node_dict.values(), key=lambda x: x.coord[0]).coord[0]


# 获取道路节点最大横坐标
def get_nodes_max_x(node_dict: dict[int, Node]) -> float:
    whether_nodes_exist(node_dict)
    return max(node_dict.values(), key=lambda x: x.coord[0]).coord[0]


# 获取道路节点最小纵坐标
def get_nodes_min_y(node_dict: dict[int, Node]) -> float:
    whether_nodes_exist(node_dict)
    return min(node_dict.values(), key=lambda x: x.coord[1]).coord[1]


# 获取道路节点最大纵坐标
def get_nodes_max_y(node_dict: dict[int, Node]) -> float:
    whether_nodes_exist(node_dict)
    return max(node_dict.values(), key=lambda x: x.coord[1]).coord[1]


# 求纸张大小
def get_papersize(node_dict: dict[int, Node]) -> tuple[float, float]:
    origin_min_x = get_nodes_min_x(node_dict)
    origin_max_x = get_nodes_max_x(node_dict)
    origin_min_y = get_nodes_min_y(node_dict)
    origin_max_y = get_nodes_max_y(node_dict)
    return (origin_max_x - origin_min_x) + 60, (origin_max_y - origin_min_y) + 60


# 求原来中心坐标
def get_origin_center_coord(node_dict: dict[int, Node]) -> tuple[float, float]:
    origin_min_x = get_nodes_min_x(node_dict)
    origin_max_x = get_nodes_max_x(node_dict)
    origin_min_y = get_nodes_min_y(node_dict)
    origin_max_y = get_nodes_max_y(node_dict)
    return origin_min_x + (origin_max_x - origin_min_x) / 2, origin_min_y + (origin_max_y - origin_min_y) / 2

