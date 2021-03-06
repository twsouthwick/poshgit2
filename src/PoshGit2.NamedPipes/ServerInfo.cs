﻿using System;

namespace PoshGit2
{
    public static class ServerInfo
    {
        static ServerInfo()
        {
            Version = typeof(ServerStartupPoshGitClient).Assembly.GetName().Version;
#if DEBUG
            Name = $"PoshGit2_Server_Debug_v{Version}";
            FileName = $"PoshGit2_Server_Pid_Debug_v{Version}";
#else
            Name = $"PoshGit2_Server_v{Version}";
            FileName = $"PoshGit2_Server_Pid_v{Version}";
#endif
        }

        public static string Name { get; }

        internal static string FileName { get; }

        public static Version Version { get; }
    }
}
