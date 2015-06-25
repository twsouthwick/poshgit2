using System;

namespace PoshGit2
{
    public static class ServerInfo
    {
        static ServerInfo()
        {
            Version = typeof(ServerStartupPoshGitClient).Assembly.GetName().Version;
            Name = $"PoshGit2_Server_v{Version}";
        }

        public static string Name { get; }

        public static Version Version { get; }
    }
}
