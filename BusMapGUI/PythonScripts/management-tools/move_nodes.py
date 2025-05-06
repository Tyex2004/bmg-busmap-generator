import os
import sys
import json
import shutil
from datetime import datetime


map_name = sys.argv[1]
DATA_DIR = os.path.join('data', map_name)
BACKUP_DIR = os.path.join('data', 'backup', map_name)


def backup_data(tool_name='move_nodes'):
    timestamp = datetime.now().strftime('%y%m%d%H%M%S')
    dst = os.path.join(BACKUP_DIR, f'data-{timestamp}-before-{tool_name}')
    shutil.copytree(DATA_DIR, dst)
    print(f"[INFO] 数据已备份至: {dst}")


def load_nodes():
    path = os.path.join(DATA_DIR, 'nodes.json')
    with open(path, 'r', encoding='utf-8') as f:
        return json.load(f), path


def save_nodes(nodes, path):
    with open(path, 'w', encoding='utf-8') as f:
        json.dump(nodes, f, indent=2, ensure_ascii=False)
    print(f"[INFO] 已更新的节点已保存到 {path}")


def in_box(node, x_min, y_min, x_max, y_max):
    coord = node.get('coord')
    if not coord or len(coord) != 2:
        return False
    x, y = coord
    return x_min <= x <= x_max and y_min <= y <= y_max


def move_nodes(x1, y1, x2, y2, dx, dy):
    x_min, x_max = min(x1, x2), max(x1, x2)
    y_min, y_max = min(y1, y2), max(y1, y2)

    nodes, path = load_nodes()

    moved_count = 0
    for node in nodes:
        if in_box(node, x_min, y_min, x_max, y_max):
            node['coord'][0] += dx
            node['coord'][1] += dy
            moved_count += 1

    save_nodes(nodes, path)
    print(f"[INFO] 通过 ({dx}, {dy}) 移动了 {moved_count} 节点")


if __name__ == '__main__':
    if len(sys.argv) != 7:
        print("格式: python move_nodes.py x1 y1 x2 y2 dx dy")
        sys.exit(1)

    try:
        x1, y1, x2, y2, dx, dy = map(float, sys.argv[2:])
    except ValueError:
        print("[ERROR] 所有六个参数都必须是数值！")
        sys.exit(1)

    if not os.path.isdir(DATA_DIR):
        print("[ERROR] 未找到 data 目录。")
        sys.exit(1)

    backup_data()
    move_nodes(x1, y1, x2, y2, dx, dy)
