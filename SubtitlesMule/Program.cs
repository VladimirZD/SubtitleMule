using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
namespace SubtitlesMule
{
    class Program
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            _logger.Trace("Hello...");
            Console.WriteLine("Press any key to continue..");
            Console.ReadKey();
        }
    }
}
