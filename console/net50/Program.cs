using System;
using System.Linq;
using v90.Features;
namespace net50
{
    class Program
    {
        static void Main(string[] args)
        {
            var features = new Func<string>[] {
                () => new RecordType("Record type", 1).ToString(),
                () => {
                    var x = new InitOnlySetter() {
                        X = "Init only setter",
                        Y = 2,
                    };
                    // x.X = "Never override once initialized";
                    return x.ToString();
                },
            };

            foreach (var feature in features)
            {
                Console.WriteLine(feature());
            }
        }
    }
}
