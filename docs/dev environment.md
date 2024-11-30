# Development environment

## Requirements
* NET 8
* Visual Studio 2022 or Visual Studio Code
* Azure Cosmos DB emulator - Windows
* Docker (Azure Cosmos DB emulator) - Linux

### How to install Azure NoSQL (Docker)
https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator?tabs=docker-windows%2Ccsharp&pivots=api-nosql

Note: It's very important to use BASH.

1. docker pull mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest

2. docker run
--publish 8081:8081
--publish 10250-10255:10250-10255
--name linux-emulator
--detach
mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest

3. Go to https://localhost:8081/_explorer/index.html

## How to run the application (Docker support)

1. Open a terminal
2. docker run -it cosmosexplorerterminal (Interactive mode)

## How to use Cosmos Explorer UI

1. Install Azure cosmos DB Emulator
2. Select CosmosExplorer.UI as startup project
3. F5

## How to deploy (Chocolatey)
1) choco new cosmosexplorer (just the first time)
move to C:\Windows\System32\{packagename}

delete TODO, 
2) Adjust .nuspec file

Example 
```
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2015/06/nuspec.xsd">
  <metadata>
    <id>cosmosexplorer</id>
    <version>1.0.0</version>
    <owners>Javier Leyes</owners>
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

3) Adjust chocolateyinstall.ps1
```
$ErrorActionPreference = 'Stop' # stop on all errors
```

# Generate the package
4) choco pack

# This is to test the package
5) choco install cosmosexplorer --debug --verbose --source .

6) Upload package
choco push {cosmosexplorer.1.0.0.nupkg} --source https://push.chocolatey.org/


