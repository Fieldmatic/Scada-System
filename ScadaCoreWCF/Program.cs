using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScadaCoreWCF
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost svc = new ServiceHost(typeof(ScadaCoreWCF));
            svc.Open();
            Console.WriteLine("Service running...");
            Console.ReadLine();
            svc.Close();
        }
    }
}
