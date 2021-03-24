
namespace v90.Features
{
    public class PatternMatchEnhancement {
        public int Version {get;set;}
        public PatternMatchEnhancement(int version) {
            Version = version;
        }
        public override string ToString()
        {
            return Version.ToString();
        }
    }
}
