using System.Collections.Generic;
using System.Linq;

namespace PoshGit2
{
    public class ChangedItemsCollection
    {
        public ChangedItemsCollection()
        {
            var empty = new List<string>().AsReadOnly();

            Added = empty;
            Modified = empty;
            Deleted = empty;
            Unmerged = empty;
        }

        public bool HasAny => Added.Any() || Modified.Any() || Deleted.Any() || Unmerged.Any();

        public IReadOnlyCollection<string> Added { get; set; }
        public IReadOnlyCollection<string> Modified { get; set; }
        public IReadOnlyCollection<string> Deleted { get; set; }
        public IReadOnlyCollection<string> Unmerged { get; set; }

        public int Count => Added.Count + Modified.Count + Deleted.Count + Unmerged.Count;

        public int Length => Count;

        public override string ToString()
        {
            return $"{Added.Count} | {Modified.Count} | {Deleted.Count} | {Unmerged.Count}";
        }
    }
}
