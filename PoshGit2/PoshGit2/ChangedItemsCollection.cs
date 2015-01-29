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

        public bool HasAny { get { return Added.Any() || Modified.Any() || Deleted.Any() || Unmerged.Any(); } }
        public ICollection<string> Added { get; set; }
        public ICollection<string> Modified { get; set; }
        public ICollection<string> Deleted { get; set; }
        public ICollection<string> Unmerged { get; set; }

        public override string ToString()
        {
            return $"{Added.Count} | {Modified.Count} | {Deleted.Count} | {Unmerged.Count}";
        }
    }
}
