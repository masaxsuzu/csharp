using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace codegen
{
    [Generator]
    public class Utf8StringLiteralGenerator : ISourceGenerator
    {
        private const string attributeText = @"using System;
namespace codegen
{
    [System.Diagnostics.Conditional(""COMPILE_TIME_ONLY"")]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class Utf8Attribute : Attribute
    {
        public Utf8Attribute(string s) { }
    }
}
";
        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("Utf8Attribute", SourceText.From(attributeText, Encoding.UTF8));

            if (context.SyntaxReceiver is not SyntaxReceiver receiver) return;

            CSharpParseOptions options = (CSharpParseOptions)((CSharpCompilation)context.Compilation).SyntaxTrees[0].Options;

            Compilation compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(attributeText, Encoding.UTF8), options));

            if (!(compilation.GetTypeByMetadataName("codegen.Utf8Attribute") is { } attributeSymbol)) return;
            if (!(compilation.GetTypeByMetadataName("System.ReadOnlySpan`1") is { } spanSymbol)) return;
            if (!(compilation.GetTypeByMetadataName("System.Byte") is { } byteSymbol)) return;

            var byteSpanSymbol = spanSymbol.Construct(byteSymbol);

            var buffer = new StringBuilder();

            var group = enumerate().GroupBy(x => x.containingType, x => (x.methodName, x.value, x.accessibility));

            foreach (var g in group)
            {
                var containingType = g.Key;
                var generatedSource = generate(containingType, g);
                var filename = getFilename(containingType);
                context.AddSource(filename, SourceText.From(generatedSource, Encoding.UTF8));
            }

            string getFilename(INamedTypeSymbol type)
            {
                buffer.Clear();

                foreach (var part in type.ContainingNamespace.ToDisplayParts())
                {
                    if (part.Symbol is { Name: var name } && !string.IsNullOrEmpty(name))
                    {
                        buffer.Append(name);
                        buffer.Append('_');
                    }
                }
                buffer.Append(type.Name);
                buffer.Append("_utf8literal.cs");

                return buffer.ToString();
            }

            IEnumerable<(INamedTypeSymbol containingType, string methodName, string value, Accessibility accessibility)> enumerate()
            {
                foreach (var m in receiver.CandidateMethods)
                {
                    if (!isStaticPartial(m)) continue;

                    SemanticModel model = compilation.GetSemanticModel(m.SyntaxTree);

                    if (m.ParameterList.Parameters.Count != 0) continue;
                    if (!returnsString(model, m)) continue;
                    if (!(getUtf8Attribute(model, m) is ({ } value, var containingType, var accessibility))) continue;

                    yield return (containingType, m.Identifier.ValueText, value, accessibility);
                }
            }

            string generate(INamedTypeSymbol containingType, IEnumerable<(string methodName, string value, Accessibility accessibility)> methods)
            {
                buffer.Clear();

                if (!string.IsNullOrEmpty(containingType.ContainingNamespace.Name))
                {
                    buffer.Append(@"namespace ");
                    buffer.Append(containingType.ContainingNamespace.ToDisplayString());
                    buffer.Append(@"
{
");
                }
                buffer.Append(@"partial class ");
                buffer.Append(containingType.Name);
                buffer.Append(@"
{
");
                foreach (var (methodName, value, accessibility) in methods)
                {
                    buffer.Append("    ");
                    buffer.Append(AccessibilityText(accessibility));
                    buffer.Append(" static partial System.ReadOnlySpan<byte> ");
                    buffer.Append(methodName);
                    buffer.Append("() => new byte[] {");

                    foreach (var b in Encoding.UTF8.GetBytes(value))
                    {
                        buffer.Append(b);
                        buffer.Append(", ");
                    }

                    buffer.Append(@"};
");
                }

                buffer.Append(@"}
");
                if (!string.IsNullOrEmpty(containingType.ContainingNamespace.Name))
                {
                    buffer.Append(@"}
");
                }

                return buffer.ToString();
            }

            static bool isStaticPartial(MemberDeclarationSyntax m)
            {
                bool isStatic = false;
                bool isPartial = false;
                foreach (var mod in m.Modifiers)
                {
                    isStatic |= mod.Text == "static";
                    isPartial |= mod.Text == "partial";
                }
                return isStatic && isPartial;
            }

            bool returnsString(SemanticModel model, MethodDeclarationSyntax m) => model.GetSymbolInfo(m.ReturnType).Symbol is INamedTypeSymbol s
                    && SymbolEqualityComparer.Default.Equals(s, byteSpanSymbol);

            (string? value, INamedTypeSymbol containingType, Accessibility accessibility) getUtf8Attribute(SemanticModel model, MethodDeclarationSyntax m)
            {
                if (!(model.GetDeclaredSymbol(m) is { } s)) return default;

                foreach (var a in s.GetAttributes())
                {
                    if (SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol))
                    {
                        var args = a.ConstructorArguments;
                        if (args.Length != 1) continue;

                        if (args[0].Value is string value) return (value, s.ContainingType, s.DeclaredAccessibility);
                    }
                }

                return default;
            }
        }

        private static string AccessibilityText(Accessibility accessibility) => accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Protected => "protected",
            Accessibility.Private => "private",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => throw new InvalidOperationException(),
        };

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        class SyntaxReceiver : ISyntaxReceiver
        {
            public List<MethodDeclarationSyntax> CandidateMethods { get; } = new List<MethodDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // any field with at least one attribute is a candidate for property generation
                if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax
                    && methodDeclarationSyntax.AttributeLists.Count > 0)
                {
                    CandidateMethods.Add(methodDeclarationSyntax);
                }
            }
        }
    }
}
