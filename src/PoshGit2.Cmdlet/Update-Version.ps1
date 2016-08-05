param(
    [Parameter(Mandatory=$TRUE)]
    [string]$outDir,
	[Parameter(Mandatory=$TRUE)]
	[string]$nugetVersion,
	[Parameter(Mandatory=$TRUE)]
	[string]$moduleVersion
    )

function Update-NuspecVersion (
     [Parameter(Mandatory=$TRUE)]
     [String] $FilePath,
     [Parameter(Mandatory=$TRUE)]
     [String] $nugetVersion
){
    if ((Test-Path -Path $FilePath -PathType Leaf) -ne $TRUE) {
        Write-Error -Message ($FilePath + ' not found.') -Category InvalidArgument;
        exit 1;
    }

    #normalize path
    $FilePath = (Resolve-Path -Path $FilePath).Path;

    $nuspecConfig = [xml] (Get-Content -Path $FilePath);
    $nuspecConfig.DocumentElement.metadata.version = $nugetVersion;

    if (!$?) {
        Write-Error -Message "Unable to perform update.";
        exit 1;
    }

    $nuspecConfig.Save($FilePath);

    Write-Host "Updated $FilePath"
}

function Update-ModuleManifest (
     [Parameter(Mandatory=$TRUE)]
     [String] $FilePath,
     [Parameter(Mandatory=$TRUE)]
     [String] $nugetVersion
){
    if ((Test-Path  -Path $FilePath -PathType Leaf) -ne $TRUE) {
        Write-Error -Message ($FilePath + ' not found.') -Category InvalidArgument;
        exit 1;
    }

    # Normalize path
    $FilePath = (Resolve-Path -Path $FilePath).Path;

    $moduleVersionPattern = "ModuleVersion = '.*'";
    $newVersion = "ModuleVersion = '" + $nugetVersion + "'";

    (Get-Content -Path $FilePath) | ForEach-Object {$_ -replace $moduleVersionPattern, $newVersion} | Set-Content -Path $FilePath;

    Write-Host "Updated $FilePath"
}

Write-Host "Updating module version: $moduleVersion"
Update-ModuleManifest $outDir\poshgit2.psd1 $moduleVersion

Write-Host "Updating NuGet version: $nugetVersion"
Update-NuspecVersion $outDir\poshgit2.nuspec $nugetVersion
