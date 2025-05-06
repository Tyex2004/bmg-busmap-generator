# shell-scripts/undo.ps1

$scriptPath = Join-Path $PSScriptRoot "..\management-tools\undo.py"
python $scriptPath @args
