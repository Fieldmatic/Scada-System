using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Trending.ScadaCoreTrendingReference;

namespace Trending
{
    class Program
    {
        class TrendingCallback : ITrendingCallback
        {
            public void ITagValueChanged(string message)
            {
                Console.WriteLine(message);
            }
        }
        static void Main(string[] args)
        {
            TrendingClient proxy = new TrendingClient(new InstanceContext(new TrendingCallback()));
            Console.WriteLine(proxy.TrendingConnection());
            Console.ReadKey();
        }
    }
}
