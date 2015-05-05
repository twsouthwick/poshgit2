poshgit2
========

A set of PowerShell scripts which provide Git/PowerShell integration

### Prompt for Git repositories
   The prompt within Git repositories can show the current branch and the state of files (additions, modifications, deletions) within.  This is based off of posh-git, but watches the repos in the background with libgit2.  The result is it is faster to display staus, which is very noticeable on larger repos.
   
### Tab completion
   Provides tab completion for common commands when using git.  
   E.g. `git ch<tab>` --> `git checkout`
   
Usage
-----

See `profile.example.ps1` as to how you can integrate the tab completion and/or git prompt into your own profile. Prompt formatting, among other things, can be customized using `$GitPromptSettings`, `$GitTabSettings`.

Installing via OneGet
--------------------

On Win10, OneGet includes a provider for PSGallery.  You must add a new PS repository located at `https://www.myget.org/feed/Packages/tws-ps`

```
    Register-PSRepository -Name tws-ps -SourceLocation https://www.myget.org/F/tws-ps/ -InstallationPolicy Trusted
	Find-Package posh-git2 -Source tws-ps | Install-Package -Scope CurrentUser -Force
```

The Prompt
----------

PowerShell generates its prompt by executing a `prompt` function, if one exists. posh-git defines such a function in `profile.example.ps1` that outputs the current working directory followed by an abbreviated `git status`:

    C:\Users\user [master]>

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

*Based on work by  [PoshGit](https://github.com/dahlbyk/posh-git)*
