param([string]$outDir)

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

	#normalize path
	$FilePath = (Resolve-Path -Path $FilePath).Path;

	$moduleVersionPattern = "ModuleVersion = '.*'";
	$newVersion = "ModuleVersion = '" + $Version + "'";

	(Get-Content -Path $FilePath) | ForEach-Object {$_ -replace $moduleVersionPattern, $newVersion} | Set-Content -Path $FilePath;
	
	Write-Host "Updated $FilePath"
}

$version = (Get-ChildItem -Path $outDir\PoshGit2.Cmdlet.dll).VersionInfo.FileVersion
	
Write-Host "Updating file version: $version"

Update-ModuleManifest $PSScriptRoot\posh-git2.psd1 $version
Update-NuspecVersion $PSScriptRoot\posh-git2.nuspec $version