param(
    [Parameter(Mandatory=$TRUE)]
    [string]$outDir,
	[Parameter(Mandatory=$TRUE)]
	[string]$version
    )

function Update-NuspecVersion (
     [Parameter(Mandatory=$TRUE)]
     [String] $FilePath,
     [Parameter(Mandatory=$TRUE)]
     [String] $Version
){
    if ((Test-Path -Path $FilePath -PathType Leaf) -ne $TRUE) {
        Write-Error -Message ($FilePath + ' not found.') -Category InvalidArgument;
        exit 1;
    }

    #normalize path
    $FilePath = (Resolve-Path -Path $FilePath).Path;

    $nuspecConfig = [xml] (Get-Content -Path $FilePath);
    $nuspecConfig.DocumentElement.metadata.version = $Version;

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
     [String] $Version
){
    if ((Test-Path  -Path $FilePath -PathType Leaf) -ne $TRUE) {
        Write-Error -Message ($FilePath + ' not found.') -Category InvalidArgument;
        exit 1;
    }

    # Normalize path
    $FilePath = (Resolve-Path -Path $FilePath).Path;

    $moduleVersionPattern = "ModuleVersion = '.*'";
    $newVersion = "ModuleVersion = '" + $Version + "'";

    (Get-Content -Path $FilePath) | ForEach-Object {$_ -replace $moduleVersionPattern, $newVersion} | Set-Content -Path $FilePath;

    Write-Host "Updated $FilePath"
}

Write-Host "Updating file version: $version"

Update-ModuleManifest $outDir\poshgit2.psd1 $version
Update-NuspecVersion $outDir\poshgit2.nuspec $version
