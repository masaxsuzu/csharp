using System;
using System.Linq;
using v90.Features;

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
    () => new TopLevelStatement().ToString(),
};

foreach (var feature in features)
{
    Console.WriteLine(feature());
}
