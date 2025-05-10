# shell-scripts/move_nodes.ps1

$scriptPath = Join-Path $PSScriptRoot "..\management-tools\move_nodes.py"
python $scriptPath @args