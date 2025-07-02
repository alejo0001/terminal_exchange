# RunPublishWorkflow.ps1
Param (
    [string]$PublishVersionOverride = $null,
    [switch]$ToLocalSource
)

# Define the required scripts
$requiredScripts = @(
    "EnsureLocalSource.ps1",
    "BuildProjects.ps1",
    "FindPackages.ps1",
    "Publish.ps1"
)

# Check if all required scripts are present
foreach ($script in $requiredScripts) {
    if (-not (Test-Path $script)) {
        Write-Error "Error: Required script $script not found!"
        exit 1
    }
}

.\BuildProjects.ps1 -PublishVersionOverride $PublishVersionOverride
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error: Building & Pack projects step has failed!"
    exit 1
}

if ($ToLocalSource)
{
    $publishSource = .\EnsureLocalSource.ps1 -NuGetLocalSource "..\nuget-local-source-tech"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error: Publishing to machine's local source requested, but ensuring its existence failed!"
        exit 1
    }
    Write-Host "source will be:"$publishSource
    .\Publish.ps1 -PublishVersionOverride $PublishVersionOverride -NuGetFeedUrl $publishSource
}
else {
    .\Publish.ps1 -PublishVersionOverride $PublishVersionOverride
}

if ($LASTEXITCODE -ne 0) {
    Write-Error "Error: Publishing Packages step has failed!"
    exit 1
}

Write-Host "All steps completed successfully!"
