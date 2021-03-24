
namespace v90.Features
{
    public class InitOnlySetter
    {
        public string? X {get; init; }
        public int Y {get; init; }

        public override string ToString()
        {
            return $"InitOnlySetter is added: X is {X}, Y is {Y}";
        }
    }
}
