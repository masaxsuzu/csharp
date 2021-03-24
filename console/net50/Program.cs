using System;
using System.Linq;
using v90.Features;
namespace net50
{
    class Program
    {
        static void Main(string[] args)
        {
            var features = new string[] {
                new RecordType("Record type", 1).ToString(),
            };

            foreach (var feature in features)
            {
                Console.WriteLine(feature);
            }
        }
    }
}
