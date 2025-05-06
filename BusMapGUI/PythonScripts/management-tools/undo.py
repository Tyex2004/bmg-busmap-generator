import os
import shutil
from datetime import datetime

BASE_DIR = os.path.dirname(os.path.dirname(__file__))
DATA_DIR = os.path.join(BASE_DIR, 'data')
BACKUP_DIR = os.path.join(BASE_DIR, 'backup')
UNDONE_DIR = os.path.join(BASE_DIR, 'undone')


def latest_before_backup():
    candidates = sorted(
        [d for d in os.listdir(BACKUP_DIR) if '-before-' in d],
        reverse=True
    )
    return os.path.join(BACKUP_DIR, candidates[0]) if candidates else None


def backup_current_to_undone(tool_name='move_nodes'):
    timestamp = datetime.now().strftime('%y%m%d%H%M%S')
    dst = os.path.join(UNDONE_DIR, f'data-{timestamp}-{tool_name}')
    shutil.copytree(DATA_DIR, dst)
    print(f"[UNDO] 当前 data 目录已备份到: {dst}")
    return dst


def restore(backup_path):
    if os.path.exists(DATA_DIR):
        shutil.rmtree(DATA_DIR)
    shutil.copytree(backup_path, DATA_DIR)
    print(f"[UNDO] 从备份恢复: {backup_path}")


def remove_backup(path):
    shutil.rmtree(path)
    print(f"[UNDO] 已删除备份目录: {path}")


if __name__ == '__main__':
    backup_path = latest_before_backup()
    if not backup_path:
        print("[UNDO] 未找到任何可用的 -before- 备份。")
        exit(1)

    os.makedirs(UNDONE_DIR, exist_ok=True)
    backup_current_to_undone()
    restore(backup_path)
    remove_backup(backup_path)
    print("[UNDO] 撤销成功。")
