using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Data.Entity;

namespace ScadaCoreWCF.models
{
    public class UsersContext : DbContext
    {
        public DbSet<User> Users { get; set; }
    }
}
