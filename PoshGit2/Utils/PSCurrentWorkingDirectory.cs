using Microsoft.PowerShell.Commands;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PoshGit2
{
    public class PSCurrentWorkingDirectory : ICurrentWorkingDirectory
    {
        private const int MAX_PATH = 260;

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern bool PathRelativePathTo([Out] StringBuilder pszPath, [In] string pszFrom, [In] FileAttributes dwAttrFrom, [In] string pszTo, [In] FileAttributes dwAttrTo);
        private readonly ISessionState _sessionState;

        public PSCurrentWorkingDirectory(ISessionState sessionState)
        {
            _sessionState = sessionState;
        }

        public bool IsValid
        {
            get
            {
                return _sessionState.Path.CurrentLocation.Provider.ImplementingType == typeof(FileSystemProvider);
            }
        }

        public string CWD
        {
            get
            {
                return _sessionState.Path.CurrentLocation.ProviderPath;
            }
        }

        public string CreateRelativePath(string path)
        {
            var str = new StringBuilder(MAX_PATH);
            var success = PathRelativePathTo(str, CWD, FileAttributes.Directory, path, FileAttributes.Normal);

            return success ? str.ToString() : string.Empty;
        }
    }
}