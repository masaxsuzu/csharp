
using System;
using System.Text;

using codegen;

namespace v90.Features
{
    public partial class CodeGenerator
    {
        [Utf8("😀")]
        public static partial ReadOnlySpan<byte> Smile();
        public override string ToString()
        {
            var sb = new StringBuilder();
            var smile = Smile();
            sb.Append($"[");

            for (int i = 0; i < smile.Length; i++)
            {
                sb.Append($"{smile[i]}");
                if (i < smile.Length - 1) {
                    sb.Append($", ");
                }
            }
            sb.Append($"]");
            return $"CodeGenerator is added: {sb.ToString()}";
        }
    }
}
