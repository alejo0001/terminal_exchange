# BuildProjects.ps1
Param (
    [string]$PublishVersionOverride = $null,
    [string]$Config = "Release"
)

$fragmentConfigVersion = "Config: '$Config', Version: $($PublishVersionOverride `
    ? "[override]: '$PublishVersionOverride'" `
    : ": each project's applicable version") ..."

$buildMessage = "Building solution with: $fragmentConfigVersion"
$packMessage = "Packing solution with: $fragmentConfigVersion"

# For DX, projects have GeneratePackageOnBuild=true (solution-level), it means CLI "pack" will not run build task!
# That's why boths command are called, because this is rather publishing workflow, intenden to run by pipeline too.
if ($PublishVersionOverride){
    Write-Host $buildMessage
    dotnet build -c:$Config -p:Version=$PublishVersionOverride -v:q

    Write-Host $packMessage
    dotnet pack -c:$Config -p:Version=$PublishVersionOverride -v:q
}
else {
    Write-Host $buildMessage
    dotnet build -c:$Config -v:q

    Write-Host $packMessage
    dotnet pack -c:$Config -v:q
}
