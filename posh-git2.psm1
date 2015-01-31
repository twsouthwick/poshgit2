if (Get-Module posh-git) { return }

Push-Location $psScriptRoot

. .\Utils.ps1
. .\GitUtils.ps1
. .\GitPrompt.ps1
. .\GitTabExpansion.ps1

Pop-Location

if (!$Env:HOME) { $Env:HOME = "$Env:HOMEDRIVE$Env:HOMEPATH" }
if (!$Env:HOME) { $Env:HOME = "$Env:USERPROFILE" }

Get-TempEnv 'SSH_AGENT_PID'
Get-TempEnv 'SSH_AUTH_SOCK'