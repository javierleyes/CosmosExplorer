$ErrorActionPreference = 'Stop' # stop on all errors

$exePath = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)" + "\CosmosExplorer.UI.exe"
$desktop = $([System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::CommonDesktopDirectory))
$desktopLink = Join-Path $desktop "Cosmos Explorer.lnk"

Install-ChocolateyShortcut `
    -shortcutFilePath $desktopLink `
    -targetPath $exePath 