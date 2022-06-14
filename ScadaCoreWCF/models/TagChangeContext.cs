using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScadaCoreWCF.models
{
    public class TagChangeContext : DbContext
    {
        public DbSet<TagChange> tagChanges { get; set; }
    }
}
