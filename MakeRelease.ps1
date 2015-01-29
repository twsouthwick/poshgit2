$sln = "$PSScriptRoot\PoshGit2\PoshGit2.sln";
& "C:\Users\tasou\AppData\Local\OneGet\ProviderAssemblies\nuget-anycpu.exe" restore $sln
msbuild $sln /t:Rebuild /p:Configuration=Release

$targetDir = "${env:Temp}\PoshGit2"

if (Test-Path -Path $targetDir)
{
    rmdir -Recurse $targetDir
}

mkdir $targetDir | Out-Null
mkdir $targetDir\NativeBinaries\amd64 | Out-Null
mkdir $targetDir\NativeBinaries\x86 | Out-Null

$files = @(	'CheckVersion.ps1',
			'GitPrompt.ps1',
			'GitTabExpansion.ps1',
			'GitUtils.ps1',
			'LICENSE.txt',
			'posh-git2.psd1',
			'posh-git2.psm1',
			'readme.md',
			'TortoiseGit.ps1',
			'Utils.ps1',
			'PoshGit2\PoshGit2\bin\Release\Autofac.dll',
			'PoshGit2\PoshGit2\bin\Release\LibGit2Sharp.dll',
			'PoshGit2\PoshGit2\bin\Release\PoshGit2.dll',
			'PoshGit2\PoshGit2\bin\Release\PoshGit2.pdb')

foreach ($file in $files)
{
    copy $PSScriptRoot\$file $targetDir
}

copy $PSScriptRoot\PoshGit2\PoshGit2\bin\Release\NativeBinaries\amd64\git2-4a30c53.dll $targetDir\NativeBinaries\amd64\git2-4a30c53.dll
copy $PSScriptRoot\PoshGit2\PoshGit2\bin\Release\NativeBinaries\x86\git2-4a30c53.dll $targetDir\NativeBinaries\x86\git2-4a30c53.dll

#$version = (Get-ChildItem -Path $targetDir\poshgit2.dll).VersionInfo.FileVersion

#& $PSScriptRoot\Update-ModuleManifest.ps1 $targetDir\posh-git2.psd1 $version
#& $PSScriptRoot\Update-NuspecVersion.ps1 "$PSScriptRoot\posh-git2.nuspec" $version

copy $PSScriptRoot\posh-git2.nuspec $targetDir

cpack "$targetDir\posh-git2.nuspec"
