# Publish.ps1
Param (
    [string]$PackagesOutput = "packages",
    [string]$NuGetFeedUrl = "https://nuget.aula-digital.com/v3/index.json",
    [string]$NuGetApiKey = $null,
    [string]$PublishVersionOverride = $null
)

# Stinky :-/ Replace with Secrets Manafer of some kind or at least env.vars.
if (-not $($NuGetApiKey))
{
    $NuGetApiKey = "NUGET-SERVER-API-KEY"
}
#$filter = $PublishVersionOverride ? "*.$PublishVersionOverride.nupkg" : "*.nupkg"

# Loop through each package in the package output and publish it
#$packages = Get-ChildItem -Path $PackagesOutput -Filter $filter
$packages = .\FindPackages.ps1 -PackagesOutput $PackagesOutput -PublishVersionOverride $PublishVersionOverride

foreach ($package in $packages) {
    Write-Host "Publishing package $($package.Name)..."
    dotnet nuget push $($package.FullName) --source $NuGetFeedUrl --api-key $NuGetApiKey --skip-duplicate
}
