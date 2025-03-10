## How to deploy (Chocolatey)
1) choco new cosmosexplorer (just the first time)
move to C:\Windows\System32\{packagename}
delete TODO, 

2) Adjust .nuspec file (just the first time)

Example assets/chocolatey/nuspec
```
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2015/06/nuspec.xsd">
  <metadata>
    <id>cosmosexplorer</id>
    <version>1.0.0</version>
    <title>Cosmos Explorer</title>
    <authors>Javier Leyes</authors>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <licenseUrl>https://github.com/javierleyes/CosmosExplorer/blob/main/LICENSE</licenseUrl>
    <projectUrl>https://github.com/javierleyes/CosmosExplorer</projectUrl>
    <packageSourceUrl>https://github.com/javierleyes/CosmosExplorer</packageSourceUrl>
    <copyright>Copyright 2024 Javier Leyes</copyright>
    <iconUrl>https://github.com/javierleyes/CosmosExplorer/blob/main/assets/icons/Cosmos-DB.ico</iconUrl>
    <docsUrl>https://github.com/javierleyes/CosmosExplorer/tree/main/docs</docsUrl>
    <bugTrackerUrl>https://github.com/javierleyes/CosmosExplorer/issues</bugTrackerUrl>
    <tags>cosmosexplorer azure cosmos nosql</tags>
    <summary>Cosmos Explorer allows users to manage data like Azure Portal</summary>
    <description>Cosmos Explorer allows users to manage data like Azure Portal</description>
    <releaseNotes>https://github.com/javierleyes/CosmosExplorer/blob/main/docs/release%20notes.md</releaseNotes>
  </metadata>
    <files>
    <file src="tools\**" target="tools" />
  </files>
</package>
```

3) Generate package from Visual Studio

4) Zip package

5) Upload package to GitHub => Releases

6) Adjust chocolateyinstall.ps1 (See assets/chocolatey/chocolateyinstall.ps1) (every time)
$checksum value, use the command 
```
Get-FileHash -Path .\CosmosExplorer.zip -Algorithm SHA256
```

7) Generate the package (every time) 
choco pack

# This is to test the package
choco install cosmosexplorer --debug --verbose --source .

8) Upload package (every time)
choco push {cosmosexplorer.1.0.0.nupkg} --source https://push.chocolatey.org/
