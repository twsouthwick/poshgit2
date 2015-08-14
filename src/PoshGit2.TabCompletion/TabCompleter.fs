namespace PoshGit2

open PoshGit2
open System
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks

module TabCompletion = 
    let (|GitInvoked|_|) (line : string) = 
        [| "git "; "git.exe " |]
        |> Seq.where (fun item -> line.StartsWith(item, StringComparison.OrdinalIgnoreCase))
        |> Seq.map (fun item -> line.Remove(0, item.Length))
        |> Seq.tryHead
    
    let ProcessGitCommand line (repo : IRepositoryStatus) = 
        let createCollection s = new HashSet<string>(s, StringComparer.OrdinalIgnoreCase) :> ICollection<string>
        let applyFilter filter = 
            Seq.where (fun (m : string) -> m.StartsWith(filter, StringComparison.OrdinalIgnoreCase))
        let applyFilter' filter = Seq.where (fun (m : string) -> m.StartsWith(filter, StringComparison.Ordinal))
        
        let nullFilter f = 
            function 
            | null -> Seq.empty<string>
            | e -> f e :> seq<string>
        
        let getUnmerged (c : ChangedItemsCollection) = c |> nullFilter (fun f -> f.Unmerged)
        let getModified (c : ChangedItemsCollection) = c |> nullFilter (fun f -> f.Modified)
        let getAdded (c : ChangedItemsCollection) = c |> nullFilter (fun f -> f.Added)
        let getDeleted (c : ChangedItemsCollection) = c |> nullFilter (fun f -> f.Deleted)
        
        let subcommands = 
            [| ("bisect", [| "start"; "bad"; "good"; "skip"; "reset"; "visualize"; "replay"; "log"; "run" |])
               ("notes", [| "edit"; "show" |])
               ("reflog", [| "expire"; "delete"; "show" |])
               ("remote", [| "add"; "rename"; "rm"; "set-head"; "show"; "prune"; "update" |])
               ("stash", [| "list"; "save"; "show"; "drop"; "pop"; "apply"; "branch"; "save"; "clear"; "create" |])
               ("submodule", [| "add"; "status"; "init"; "update"; "summary"; "foreach"; "sync" |])
               ("svn", 
                [| "init"; "fetch"; "clone"; "rebase"; "dcommit"; "branch"; "tag"; "log"; "blame"; "find-rev"; 
                   "set-tree"; "create-ignore"; "show-ignore"; "mkdirs"; "commit-diff"; "info"; "proplist"; "propget"; 
                   "show-externals"; "gc"; "reset" |]) |]
            |> Map.ofArray
        
        let gitIndex filter = 
            [| getUnmerged; getModified; getAdded; getDeleted |]
            |> Seq.map (fun f -> f repo.Index)
            |> Seq.map (applyFilter filter)
            |> Seq.concat
        
        let gitAddFiles (filter : string) = 
            [| getUnmerged; getModified; getAdded |]
            |> Seq.map (fun f -> f repo.Working)
            |> Seq.map (applyFilter filter)
            |> Seq.concat
        
        let gitCheckoutFiles (filter : string) = 
            [| getUnmerged; getModified; getDeleted |]
            |> Seq.map (fun f -> f repo.Working)
            |> Seq.map (applyFilter filter)
            |> Seq.concat
        
        let gitDiffFiles (filter : string) = 
            function 
            | true -> 
                repo.Index
                |> getModified
                |> applyFilter filter
            | false -> 
                [| repo.Working |> getUnmerged
                   repo.Working |> getModified
                   repo.Index   |> getModified |]
                |> Seq.map (applyFilter filter)
                |> Seq.concat
        
        let gitDeleted (filter : string) = 
            repo.Working
            |> getDeleted
            |> applyFilter filter
        
        let gitMergeFiles (filter : string) = 
            repo.Working
            |> getUnmerged
            |> applyFilter filter
        
        let gitRemotes (filter : string) = repo.Remotes |> applyFilter filter
        
        let gitBranches filter includeHead = 
            let localAndRemote = Seq.append repo.LocalBranches repo.RemoteBranches
            match includeHead with
            | true -> 
                [| "HEAD"; "FETCH_HEAD"; "ORIG_HEAD"; "MERGE_HEAD" |]
                |> Seq.append localAndRemote
                |> applyFilter' filter
            | false -> localAndRemote |> applyFilter' filter
        
        let gitRemoteBranches remote ref branch = 
            let beginning = sprintf "%s/%s" remote branch
            repo.RemoteBranches 
            |> Seq.filter (fun b -> b.StartsWith(beginning, StringComparison.Ordinal)) 
            |> Seq.map (fun b -> b.Substring(remote.Length + 1))
        
        let (|Match|_|) pattern input = 
            let m = System.Text.RegularExpressions.Regex.Match(input, pattern)
            if m.Success then 
                Some(List.tail [ for g in m.Groups -> g.Value ])
            else None
        
        let (|EmptyString|_|) str = 
            if String.IsNullOrWhiteSpace(str) then Some()
            else None
        
        let (|StringEquals|_|) actual expected = 
            if String.Equals(actual, expected, StringComparison.Ordinal) then Some()
            else None
        
        let commands = 
            match repo with
            | null -> [| "clone"; "init" |]
            | _ -> 
                [| "add"; "am"; "annotate"; "archive"; "bisect"; "blame"; "branch"; "bundle"; "checkout"; "cherry"; 
                   "cherry-pick"; "citool"; "clean"; "clone"; "commit"; "config"; "describe"; "diff"; "difftool"; 
                   "fetch"; "format-patch"; "gc"; "grep"; "gui"; "help"; "init"; "instaweb"; "log"; "merge"; "mergetool"; 
                   "mv"; "notes"; "prune"; "pull"; "push"; "rebase"; "reflog"; "remote"; "rerere"; "reset"; "revert"; 
                   "rm"; "shortlog"; "show"; "stash"; "status"; "submodule"; "svn"; "tag"; "whatchanged" |]
        
        let gitCommands filter includeAliases = commands |> applyFilter filter
        
        let subcommandRegex = 
            let cmds = String.Join("|", subcommands |> Seq.map (fun f -> f.Key))
            sprintf "^(?<cmd>%s)\\s+(?<op>\\S*)$" cmds
        
        let getSubCommand item = 
            match subcommands |> Map.tryFind item with
            | Some a -> a :> string seq
            | None -> Seq.empty<string>
        
        let prepend c str = c + str

        match line with
        | EmptyString -> commands :> seq<string>
        | Match subcommandRegex (cmd :: op :: []) -> cmd |> getSubCommand |> applyFilter op
        | Match @"^remote.* (?:rename|rm|set-head|set-branches|set-url|show|prune).* (?<remote>\S*)$" (remote :: []) -> 
            gitRemotes remote
        | Match @"^stash (?:show|apply|drop|pop|branch).* (?<stash>\S*)$" (stash :: []) -> 
            repo.Stashes |> applyFilter stash
        | Match @"^bisect (?:bad|good|reset|skip).* (?<ref>\S*)$" (ref :: []) -> gitBranches ref true
        | Match @"^branch.* (?<branch>\S*)$" (branch :: []) -> gitBranches branch false
        | Match @"^(?<cmd>\S*)$" (cmd :: []) -> gitCommands cmd true
        | Match @"^help (?<cmd>\S*)$" (cmd :: []) -> gitCommands cmd false
        | Match @"^push.* (?<remote>\S+) (?<ref>[^\s\:]*\:)(?<branch>\S*)$" (remote :: ref :: branch :: []) -> gitRemoteBranches remote ref branch |> Seq.map (prepend ":")
        | Match @"^(?:push|pull).* (?:\S+) (?<branch>[^\s\:]*)$" (branch :: []) -> gitBranches branch false
        | Match @"^(?:push|pull|fetch).* (?<remote>\S*)$" (remote :: []) -> gitRemotes remote
        | Match @"^reset.* HEAD(?:\s+--)? (?<path>\S*)$" (path :: []) -> gitIndex path
        | Match @"^commit.*-C\s+(?<ref>\S*)$" (ref :: []) -> gitBranches ref true
        | Match @"^add.* (?<files>\S*)$" (file :: []) -> file |> gitAddFiles
        | Match @"^checkout.* -- (?<files>\S*)$" (files :: []) -> gitCheckoutFiles files
        | Match @"^rm.* (?<index>\S*)$" (file :: []) -> file |> gitDeleted
        | Match @"^(?:diff|difftool)(?:.* (?<staged>(?:--cached|--staged))|.*) (?<files>\S*)$" 
          (StringEquals "--cached" :: files :: []) -> gitDiffFiles files false |> Seq.append (gitBranches files true)
        | Match @"^(?:diff|difftool)(?:.* (?<staged>(?:--cached|--staged))|.*) (?<files>\S*)$" 
          (StringEquals "--staged" :: files :: []) -> gitDiffFiles files true |> Seq.append (gitBranches files true)
        | Match @"^(?:diff|difftool)(?:.* (?<staged>(?:--cached|--staged))|.*) (?<files>\S*)$" 
          (EmptyString :: files :: []) -> gitDiffFiles files true |> Seq.append (gitBranches files true)
        | Match @"^(?:checkout|cherry|cherry-pick|diff|difftool|log|merge|rebase|reflog\s+show|reset|revert|show).* (?<ref>\S*)$" 
          (ref :: []) -> gitBranches ref true
        // TODO: This won't be hit - should only happen when in a merge state
        | Match @"^(?:merge|mergetool).* (?<files>\S*)$" (files :: []) -> gitMergeFiles files
        | _ -> Seq.empty<string>

type TabCompletionResult = 
    | Success of string seq
    | Failure

type ITabCompleter =
    abstract member CompleteAsync : string -> CancellationToken -> Task<TabCompletionResult>

type TabCompleter(repo : Task<IRepositoryStatus>) = 
    member private this.RepoTask = repo

    member this.CompleteAsync line token = (this :> ITabCompleter).CompleteAsync line token

    interface ITabCompleter with
        member this.CompleteAsync line (token: CancellationToken) = 
            async { 
                let! repo = Async.AwaitTask this.RepoTask
                let result = 
                    match line with
                    | TabCompletion.GitInvoked command -> 
                        TabCompletion.ProcessGitCommand command repo
                        |> Seq.sort
                        |> Seq.distinct
                        |> Success
                    | _ -> Failure
                return result
            }
            |> Async.StartAsTask
