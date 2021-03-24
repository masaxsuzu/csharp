
namespace v90.Features
{
    public record RecordType(string X, int Y)
    {
        public override string ToString()
        {
            return $"RecordType is added: X is {X}, Y is {Y}.";
        }
    }
}
