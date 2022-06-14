using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScadaCoreWCF.models
{
    public class AlarmEventContext : DbContext
    {
        public DbSet<AlarmEvent> alarmEvents { get; set; }
    }
}
