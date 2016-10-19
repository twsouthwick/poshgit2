using System;
using System.Runtime.InteropServices;

namespace PoshGit2
{
    internal static class VirtualTerminalHelper
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private const int ENABLE_VIRTUAL_TERMINAL_INPUT = 0x4;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        public static bool IsEnabled
        {
            get
            {
                var stdOut = GetStdHandle(STD_OUTPUT_HANDLE);

                return SetConsoleMode(stdOut, ENABLE_VIRTUAL_TERMINAL_INPUT);
            }
        }
    }
}
