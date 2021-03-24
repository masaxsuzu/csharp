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
    () => {
        var x = new PatternMatchEnhancement(1);
        var v = x.Version switch {
            0 => $"alpha version",
            1 or 2 => $"version {x.ToString()}",
            _ => "stable version"
        };
        return $"PatternMatchEnhancement is added: {v}";        
    },
    () => {
        FitAndFinish1 x = new();
        return x.ToString(); 
    },
    () => {
        FitAndFinish2 x = new(new());
        return x.ToString(); 
    },
    () => new CodeGenerator().ToString(),
};

foreach (var feature in features)
{
    Console.WriteLine(feature());
}
