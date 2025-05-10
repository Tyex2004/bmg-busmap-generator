import os
import shutil
from datetime import datetime

BASE_DIR = os.path.dirname(os.path.dirname(__file__))
DATA_DIR = os.path.join(BASE_DIR, 'data')
UNDONE_DIR = os.path.join(BASE_DIR, 'undone')
BACKUP_DIR = os.path.join(BASE_DIR, 'backup')


def latest_redo():
    candidates = sorted(
        [d for d in os.listdir(UNDONE_DIR) if not '-before-' in d],
        reverse=True
    )
    return os.path.join(UNDONE_DIR, candidates[0]) if candidates else None


def backup_current_to_before(tool_name='move_nodes'):
    timestamp = datetime.now().strftime('%y%m%d%H%M%S')
    dst = os.path.join(BACKUP_DIR, f'data-{timestamp}-before-{tool_name}')
    shutil.copytree(DATA_DIR, dst)
    print(f"[REDO] 当前 data 目录已备份到: {dst}")
    return dst


def restore(redo_path):
    if os.path.exists(DATA_DIR):
        shutil.rmtree(DATA_DIR)
    shutil.copytree(redo_path, DATA_DIR)
    print(f"[REDO] 恢复自撤销前: {redo_path}")


def remove_redo(path):
    shutil.rmtree(path)
    print(f"[REDO] 已删除 redo 目录: {path}")


if __name__ == '__main__':
    redo_path = latest_redo()
    if not redo_path:
        print("[REDO] 未找到任何撤销记录可恢复。")
        exit(1)

    os.makedirs(BACKUP_DIR, exist_ok=True)
    backup_current_to_before()
    restore(redo_path)
    remove_redo(redo_path)
    print("[REDO] 重做成功。")
