using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoshGit2
{
    public class PromptColor
    {
        public static PromptColor Create(ConsoleColor foreground, ConsoleColor background)
        {
            return new PromptColor
            {
                Foreground = foreground,
                Background = background
            };
        }

        public ConsoleColor Foreground { get; private set; }

        public ConsoleColor Background { get; private set; }
    }
}
