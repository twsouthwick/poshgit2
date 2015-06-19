if (Get-Module posh-git) { return }

Push-Location $psScriptRoot

. .\GitPrompt.ps1

Pop-Location

###############################
##    Set up tab expansion   ##
###############################

if (Test-Path Function:\TabExpansion) {
    Rename-Item Function:\TabExpansion TabExpansionBackup
}

function TabExpansion($line, $lastWord) {
    $result = Expand-GitCommand $line $lastWord

    if($result.IsSuccess){
        $result.Item;
    } else {
        if (Test-Path Function:\TabExpansionBackup) {
            TabExpansionBackup $line $lastWord
        }
    }
}