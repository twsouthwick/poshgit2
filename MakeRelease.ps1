$sln = "$PSScriptRoot\PoshGit2\PoshGit2.sln";
$nuspec = "posh-git2.nuspec"

if (-not(Get-Command -Name msbuild -ErrorAction Ignore))
{
	Write-Warning "Could not find 'msbuild'."
	exit
}

if (-not(Get-Command -Name nuget -ErrorAction Ignore))
{
	Write-Warning "Could not find 'nuget'."
	exit
}

Write-Host "------------------------------------"
Write-Host "          Restore packages" 
Write-Host "------------------------------------"
Write-Host ""

nuget restore $sln

Write-Host ""
Write-Host "------------------------------------"
Write-Host "          Build binaries" 
Write-Host "------------------------------------"
Write-Host ""

msbuild $sln /t:Rebuild /p:Configuration=Release

$targetDir = "${env:Temp}\PoshGit2\" + [System.Guid]::NewGuid().ToString()

mkdir $targetDir | Out-Null
mkdir $targetDir\NativeBinaries\amd64 | Out-Null
mkdir $targetDir\NativeBinaries\x86 | Out-Null

$files = @(	'GitPrompt.ps1',
			'GitTabExpansion.ps1',
			'GitUtils.ps1',
			'LICENSE.txt',
			'posh-git2.psd1',
			'posh-git2.psm1',
			'readme.md',
			'Utils.ps1',
			'PoshGit2\PoshGit2\bin\Release\Autofac.dll',
			'PoshGit2\PoshGit2\bin\Release\LibGit2Sharp.dll',
			'PoshGit2\PoshGit2\bin\Release\PoshGit2.dll',
			'PoshGit2\PoshGit2\bin\Release\PoshGit2.pdb')

foreach ($file in $files)
{
    copy $PSScriptRoot\$file $targetDir
}

copy $PSScriptRoot\PoshGit2\PoshGit2\bin\Release\NativeBinaries\amd64\git2-*.dll $targetDir\NativeBinaries\amd64\
copy $PSScriptRoot\PoshGit2\PoshGit2\bin\Release\NativeBinaries\x86\git2-*.dll $targetDir\NativeBinaries\x86\

#$version = (Get-ChildItem -Path $targetDir\poshgit2.dll).VersionInfo.FileVersion

#& $PSScriptRoot\Update-ModuleManifest.ps1 $targetDir\posh-git2.psd1 $version
#& $PSScriptRoot\Update-NuspecVersion.ps1 "$PSScriptRoot\posh-git2.nuspec" $version

copy $PSScriptRoot\$nuspec $targetDir

Write-Host ""
Write-Host "------------------------------------"
Write-Host "          Creating nupkg" 
Write-Host "------------------------------------"
Write-Host ""

nuget pack $targetDir\$nuspec -NoPackageAnalysis -NonInteractive

