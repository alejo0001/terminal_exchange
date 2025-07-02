Param (
    [string]$NuGetLocalSource = "..\nuget-local-source-tech",
    [string]$NuGetLocalSourceName = "nuget-local-source-tech"
)

$absolutePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($NuGetLocalSource)

if (-not (Test-Path $absolutePath)) {
    New-Item -Path $absolutePath -ItemType Directory
}

$output = dotnet nuget list source --format short

if (-not ($output -like "*$($absolutePath)"))
{
    dotnet nuget add source $absolutePath --protocol-version 3 -n $NuGetLocalSourceName --configfile $env:appdata\NuGet\NuGet.Config | Out-Null
}

$absolutePath
