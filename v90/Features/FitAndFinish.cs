
namespace v90.Features
{
    public class FitAndFinish1
    {

        public override string ToString()
        {
            return $"FitAndFinish is added: FitAndFinish1 x = new() is valid.";
        }
    }
    public class FitAndFinish2
    {
        public FitAndFinish2(FitAndFinish1 _){}

        public override string ToString()
        {
            return $"FitAndFinish is added: FitAndFinish2 x = new(new()) is valid";
        }
    }
}
