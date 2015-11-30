using System.Collections.Generic;
using EFIndexMaintenance.Index;

namespace EFIndexMaintenance.Database
{
    [Index("IX_Name")]
    public class Blog
    {
        [IndexMember("IX_Name", 0)]
        public int BlogId { get; set; }

        [IndexInclude("IX_Name", 1)]
        public string Name { get; set; }

        [IndexInclude("IX_Name", 2)]
        public string Url { get; set; } 

        public virtual List<Post> Posts { get; set; }
    }
}
