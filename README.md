poshgit2
========

[![Build status](https://ci.appveyor.com/api/projects/status/wltxy9an91vlj5ms/branch/master?svg=true)](https://ci.appveyor.com/project/twsouthwick/poshgit2/branch/master)
[![Current Version](https://img.shields.io/myget/poshgit2/v/poshgit2.svg)](https://www.myget.org/feed/Packages/poshgit2)

A set of PowerShell cmdlets which provide Git/PowerShell integration

## Cmdlets

- Expand a git command given the line and last word

    `Expand-GitCommand [[-FullLine] <string>] [[-LastWord] <string>]`
    
- Retrieve the process of the current running service. If the service is not running, no response is given

    `Get-PoshGit2Server`
    
- Retrieve repository information of the current directory

    `Get-Repository [-CurrentDirectory]` *Default*
    
    `Get-Repository [-All]`

- Get a string representation of the output

    `Get-RepositoryStatus [-PlainText]` *Default*

    `Get-RepositoryStatus [-VT100]`

- Remove single repository or all from server cache

    `Remove-Repository [-Repository] <string[]>`

    `Remove-Repository [-All]`

- Write the colored output to the console

    `Write-RepositoryStatus`

## Prompt for Git repositories
   The prompt within Git repositories can show the current branch and the state of files (additions, modifications, deletions) within.  This is based off of [posh-git](https://github.com/dahlbyk/posh-git), but watches the repos in an out of process server with libgit2.  The result is it is faster to display staus and expand tabs, which is very noticeable on larger repos. A single server is maintained for the user.
  
## Tab completion
   Provides tab completion for common commands when using git.  
   E.g. `git ch<tab>` --> `git checkout`

Installing via OneGet
--------------------

On Win10, OneGet includes a provider for PSGallery.  You must add a new PS repository located at `https://www.myget.org/F/poshgit2/api/v2`

```
Register-PackageSource -Name poshgit2 -Location https://www.myget.org/F/poshgit2/api/v2 -Trusted -ProviderName PSModule
Install-Package poshgit2 -Scope CurrentUser
```

The Prompt
----------

This tool is intended to be incorporated as part of the PowerShell prompt.  For example, add the following function to your `profile.ps1` file:

```
function prompt 
{
	$dir = $pwd.Path.Replace("Microsoft.PowerShell.Core\FileSystem::", "");

	$status = Get-RepositoryStatus -VT100
	$esc = [char]0x1b
	
	return "`n${esc}[0m${esc}[32;3m[$dir]${esc}[0m$status`n$ "
}
```

This results in the following:

```
[C:\Users\user] [master]
$
```
By default, the status summary has the following format:

    [{HEAD-name} +A ~B -C !D | +E ~F -G !H !]

* `{HEAD-name}` is the current branch, or the SHA of a detached HEAD
 * Cyan means the branch matches its remote
 * Green means the branch is ahead of its remote (green light to push)
 * Red means the branch is behind its remote
 * Yellow means the branch is both ahead of and behind its remote
* ABCD represent the index; EFGH represent the working directory
 * `+` = Added files
 * `~` = Modified files
 * `-` = Removed files
 * `!` = Conflicted files
 * As in `git status`, index status is dark green and working directory status is dark red
 * The trailing `!` means there are untracked files

For example, a status of `[master +0 ~2 -1 | +1 ~1 -0]` corresponds to the following `git status`:

    # On branch master
    #
    # Changes to be committed:
    #   (use "git reset HEAD <file>..." to unstage)
    #
    #        modified:   this-changed.txt
    #        modified:   this-too.txt
    #        deleted:    gone.ps1
    #
    # Changed but not updated:
    #   (use "git add <file>..." to update what will be committed)
    #   (use "git checkout -- <file>..." to discard changes in working directory)
    #
    #        modified:   not-staged.ps1
    #
    # Untracked files:
    #   (use "git add <file>..." to include in what will be committed)
    #
    #        new.file

### Parameters

The coloring of `Write-GitStatus` can be modified by defining a `$GitPromptSettings` hashset; Write-GitStatus will pick it up, including any changes made to it during the course of the PowerShell session.  For example;

```csharp
$GitPromptSettings = New-Object PSObject -Property `
    @{ 
        WorkingForegroundColor = [System.ConsoleColor]::Red; 
        UntrackedForegroundColor = [System.ConsoleColor]::Red
    }
```

The following are variables that can be set and will affect the coloring/display of the output:

The following parameters can be changed by setting them in `$GitPromptSettings`.  Those marked as `System.ConsoleColor` must 
append `ForegroundColor` or `BackgroundColor` to the end of the name.

| Parameter              | Type                   |
|------------------------|------------------------|
| After                  | `System.ConsoleColor`  |
| Before                 | `System.ConsoleColor`  |
| BeforeIndex            | `System.ConsoleColor`  |
| Branch                 | `System.ConsoleColor`  |
| BranchAhead            | `System.ConsoleColor`  |
| BranchBehind           | `System.ConsoleColor`  |
| BranchBehindAndAhead   | `System.ConsoleColor`  |
| Delim                  | `System.ConsoleColor`  |
| Index                  | `System.ConsoleColor`  |
| Untracked              | `System.ConsoleColor`  |
| Working                | `System.ConsoleColor`  |
| UntrackedText          | `System.String`        |
| AfterText              | `System.String`        |
| BeforeText             | `System.String`        |
| BeforeIndexText        | `System.String`        |
| DelimText              | `System.String`        |
| ShowStatusWhenZero     | `System.Bool`          |
| EnablePromptStatus     | `System.Bool`          |
| EnableFileStatus       | `System.Bool`          |
