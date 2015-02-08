# Inspired by Mark Embling
# http://www.markembling.info/view/my-ideal-powershell-prompt-with-git-integration

$global:GitPromptSettings = New-Object PSObject -Property @{
    DefaultForegroundColor    = $Host.UI.RawUI.ForegroundColor

    BeforeText                = ' ['
    BeforeForegroundColor     = [ConsoleColor]::Yellow
    BeforeBackgroundColor     = $Host.UI.RawUI.BackgroundColor
    DelimText                 = ' |'
    DelimForegroundColor      = [ConsoleColor]::Yellow
    DelimBackgroundColor      = $Host.UI.RawUI.BackgroundColor

    AfterText                 = ']'
    AfterForegroundColor      = [ConsoleColor]::Yellow
    AfterBackgroundColor      = $Host.UI.RawUI.BackgroundColor

    BranchForegroundColor       = [ConsoleColor]::Cyan
    BranchBackgroundColor       = $Host.UI.RawUI.BackgroundColor
    BranchAheadForegroundColor  = [ConsoleColor]::Green
    BranchAheadBackgroundColor  = $Host.UI.RawUI.BackgroundColor
    BranchBehindForegroundColor = [ConsoleColor]::Red
    BranchBehindBackgroundColor = $Host.UI.RawUI.BackgroundColor
    BranchBehindAndAheadForegroundColor = [ConsoleColor]::Yellow
    BranchBehindAndAheadBackgroundColor = $Host.UI.RawUI.BackgroundColor

    BeforeIndexText           = ""
    BeforeIndexForegroundColor= [ConsoleColor]::DarkGreen
    BeforeIndexBackgroundColor= $Host.UI.RawUI.BackgroundColor

    IndexForegroundColor      = [ConsoleColor]::DarkGreen
    IndexBackgroundColor      = $Host.UI.RawUI.BackgroundColor

    WorkingForegroundColor    = [ConsoleColor]::DarkRed
    WorkingBackgroundColor    = $Host.UI.RawUI.BackgroundColor

    UntrackedText             = ' !'
    UntrackedForegroundColor  = [ConsoleColor]::DarkRed
    UntrackedBackgroundColor  = $Host.UI.RawUI.BackgroundColor

    ShowStatusWhenZero        = $true

    AutoRefreshIndex          = $true

    EnablePromptStatus        = !$Global:GitMissing
    EnableFileStatus          = $true
    RepositoriesInWhichToDisableFileStatus = @( ) # Array of repository paths
    DescribeStyle             = ''

    EnableWindowTitle         = 'posh~git ~ '

    Debug                     = $false
}

if(!(Test-Path Variable:Global:VcsPromptStatuses)) {
    $Global:VcsPromptStatuses = @()
}
function Global:Write-VcsStatus { $Global:VcsPromptStatuses | foreach { & $_ } }

# Add scriptblock that will execute for Write-VcsStatus
$Global:VcsPromptStatuses += {
    $Global:GitStatus = Get-GitStatus
    Write-GitStatus
}
