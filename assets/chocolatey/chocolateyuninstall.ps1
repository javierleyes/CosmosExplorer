$ErrorActionPreference = 'Stop';

$desktop = $([System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::CommonDesktopDirectory))
$desktopLink = Join-Path $desktop "Cosmos Explorer.lnk"
Remove-Item $desktopLink -force -erroraction silentlycontinue