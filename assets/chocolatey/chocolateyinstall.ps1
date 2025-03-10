$ErrorActionPreference = 'Stop'

$folderPath = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url = 'https://github.com/javierleyes/CosmosExplorer/releases/download/Latest/CosmosExplorer.zip'
$packageName = $env:ChocolateyPackageName
$checksum = "4454C227DC9EF3CCCA22D030B0FFFF34D6F7E7E8F6BC6F277E61C3664996CAB4"
$exePath = $folderPath + "\CosmosExplorer.UI.exe"
$desktop = $([System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::CommonDesktopDirectory))
$desktopLink = Join-Path $desktop "Cosmos Explorer.lnk"

Install-ChocolateyZipPackage `
    -PackageName $packageName `
    -Url $url `
    -UnzipLocation $folderPath `
    -ChecksumType "sha256" `
    -Checksum $checksum

Install-ChocolateyShortcut `
    -shortcutFilePath $desktopLink `
    -targetPath $exePath 