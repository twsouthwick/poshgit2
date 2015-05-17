# Inspired by Mark Embling
# http://www.markembling.info/view/my-ideal-powershell-prompt-with-git-integration

function Get-GitDirectory {
    if ($Env:GIT_DIR) {
        $Env:GIT_DIR
    } else {
        Get-LocalOrParentPath .git
    }
}

function InDisabledRepository {
    $currentLocation = Get-Location

    foreach ($repo in $Global:GitPromptSettings.RepositoriesInWhichToDisableFileStatus)
    {
        if ($currentLocation -like "$repo*") {
            return $true
        }
    }

    return $false
}

function Enable-GitColors {
    $env:TERM = 'cygwin'
}

function Get-AliasPattern($exe) {
   $aliases = @($exe) + @(Get-Alias | where { $_.Definition -eq $exe } | select -Exp Name)
   "($($aliases -join '|'))"
}

function setenv($key, $value) {
    [void][Environment]::SetEnvironmentVariable($key, $value, [EnvironmentVariableTarget]::Process)
    Set-TempEnv $key $value
}

function Get-TempEnv($key) {
    $path = Join-Path ($Env:TEMP) ".ssh\$key.env"
    if (Test-Path $path) {
        $value =  Get-Content $path
        [void][Environment]::SetEnvironmentVariable($key, $value, [EnvironmentVariableTarget]::Process)
    }
}

function Set-TempEnv($key, $value) {
    $path = Join-Path ($Env:TEMP) ".ssh\$key.env"
    if ($value -eq $null) {
        if (Test-Path $path) {
            Remove-Item $path
        }
    } else {
        New-Item $path -Force -ItemType File > $null
        $value > $path
    }
}

# Retrieve the current SSH agent PID (or zero). Can be used to determine if there
# is a running agent.
function Get-SshAgent() {
    if ($env:GIT_SSH -imatch 'plink') {
        $pageantPid = Get-Process | Where-Object { $_.Name -eq 'pageant' } | Select -ExpandProperty Id
        if ($null -ne $pageantPid) { return $pageantPid }
    } else {
        $agentPid = $Env:SSH_AGENT_PID
        if ($agentPid) {
            $sshAgentProcess = Get-Process | Where-Object { $_.Id -eq $agentPid -and $_.Name -eq 'ssh-agent' }
            if ($null -ne $sshAgentProcess) {
                return $agentPid
            } else {
                setenv 'SSH_AGENT_PID', $null
                setenv 'SSH_AUTH_SOCK', $null
            }
        }
    }

    return 0
}

# Loosely based on bash script from http://help.github.com/ssh-key-passphrases/
function Start-SshAgent([switch]$Quiet) {
    [int]$agentPid = Get-SshAgent
    if ($agentPid -gt 0) {
        if (!$Quiet) {
            $agentName = Get-Process -Id $agentPid | Select -ExpandProperty Name
            if (!$agentName) { $agentName = "SSH Agent" }
            Write-Host "$agentName is already running (pid $($agentPid))"
        }
        return
    }

    if ($env:GIT_SSH -imatch 'plink') {
        Write-Host "GIT_SSH set to $($env:GIT_SSH), using Pageant as SSH agent."
        $pageant = Get-Command pageant -TotalCount 1 -Erroraction SilentlyContinue
        if (!$pageant) { Write-Warning "Could not find Pageant."; return }
        & $pageant
    } else {
        $sshAgent = Get-Command ssh-agent -TotalCount 1 -ErrorAction SilentlyContinue
        if (!$sshAgent) { Write-Warning 'Could not find ssh-agent'; return }

        & $sshAgent | foreach {
            if($_ -match '(?<key>[^=]+)=(?<value>[^;]+);') {
                setenv $Matches['key'] $Matches['value']
            }
        }
    }
    Add-SshKey
}

function Get-SshPath($File = 'id_rsa')
{
    $home = Resolve-Path (Invoke-NullCoalescing $Env:HOME ~)
    Resolve-Path (Join-Path $home ".ssh\$File") -ErrorAction SilentlyContinue 2> $null
}

# Add a key to the SSH agent
function Add-SshKey() {
    if ($env:GIT_SSH -imatch 'plink') {
        $pageant = Get-Command pageant -Erroraction SilentlyContinue | Select -First 1 -ExpandProperty Name
        if (!$pageant) { Write-Warning 'Could not find Pageant'; return }

        if ($args.Count -eq 0) {
            $keystring = ""
            $keyPath = Join-Path $Env:HOME ".ssh"
            $keys = Get-ChildItem $keyPath/"*.ppk" | Select -ExpandProperty Name
            foreach ( $key in $keys ) { $keystring += "`"$keyPath\$key`" " }
            if ( $keystring ) { & $pageant "$keystring" }
        } else {
            foreach ($value in $args) {
                & $pageant $value
            }
        }
    } else {
        $sshAdd = Get-Command ssh-add -TotalCount 1 -ErrorAction SilentlyContinue
        if (!$sshAdd) { Write-Warning 'Could not find ssh-add'; return }

        if ($args.Count -eq 0) {
            & $sshAdd
        } else {
            foreach ($value in $args) {
                & $sshAdd $value
            }
        }
    }
}

# Stop a running SSH agent
function Stop-SshAgent() {
    [int]$agentPid = Get-SshAgent
    if ($agentPid -gt 0) {
        # Stop agent process
        $proc = Get-Process -Id $agentPid -ErrorAction SilentlyContinue
        if ($proc -ne $null) {
            Stop-Process $agentPid
        }

        setenv 'SSH_AGENT_PID', $null
        setenv 'SSH_AUTH_SOCK', $null
    }
}

function Update-AllBranches($Upstream = 'master', [switch]$Quiet) {
    $head = git rev-parse --abbrev-ref HEAD
    git checkout -q $Upstream
    $branches = (git branch --no-color --no-merged) | where { $_ -notmatch '^\* ' }
    foreach ($line in $branches) {
        $branch = $line.SubString(2)
        if (!$Quiet) { Write-Host "Rebasing $branch onto $Upstream..." }
        git rebase -q $Upstream $branch > $null 2> $null
        if ($LASTEXITCODE) {
            git rebase --abort
            Write-Warning "Rebase failed for $branch"
        }
    }
    git checkout -q $head
}
