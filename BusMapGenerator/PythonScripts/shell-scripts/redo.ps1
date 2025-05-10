# shell-scripts/redo.ps1

$scriptPath = Join-Path $PSScriptRoot "..\management-tools\redo.py"
python $scriptPath @args
