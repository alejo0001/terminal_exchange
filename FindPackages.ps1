# FindPackages.ps1
Param (
    [string]$SolutionDir = ".",
    [string]$PackagesOutput = "packages",
    [string]$PublishVersionOverride = $null
)

# Find all .csproj files in the solution directory
$projects = Get-ChildItem -Path $SolutionDir -Recurse -Filter *.csproj

# Invoke MSBuild for each project to check if it's packable
foreach ($project in $projects) {
    Write-Host "Geting package metadata for $($project.FullName) ..."

    $output = dotnet msbuild $($project.FullName) -t:GetPackageMetadata -v:m -p:only=true

    # Extract the JSON object
    $jsonObject = [regex]::Match($output, '{[\s\S]*}') | ConvertFrom-Json
    if (-not $jsonObject)
    {
        Write-Error "Error: Project metadata retrieval failed, check target name, 'Only=true' property when calling, or if target is intact!"
        exit 1
    }

    if ($jsonObject.IsTestProject -eq $true)
    {
        continue
    }

    $version = $PublishVersionOverride ? $PublishVersionOverride : $jsonObject.Version
    $packageName = "$($jsonObject.PackageId).$version.nupkg"

    # Script result
    Get-ChildItem -Path $PackagesOutput -Filter $packageName
}
