# management.ps1

# 把 shell-scripts 目录加入到 PATH
$toolsPath = Join-Path (Get-Location) "shell-scripts"
$env:PATH = "$toolsPath;$env:PATH"

# 打开新 PowerShell 窗口
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$(Get-Location)'" 

# -------------------
# |                             |
# |           注意!          |
# |                             |
# -------------------
#                |
#                |
#             \ | /
#               V
# 如果你用了文本编辑器打开了这个shell，请你退出这个文本编辑器，然后右键点击“使用 PowerShell 运行”