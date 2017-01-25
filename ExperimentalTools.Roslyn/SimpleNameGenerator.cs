using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ExperimentalTools.Roslyn
{
    public class SimpleNameGenerator
    {
        public async Task<string> GetNewMemberNameAsync(TypeDeclarationSyntax declaredTypeSyntax, string proposedName, Document document, CancellationToken cancellationToken)
        {
            if (declaredTypeSyntax == null)
            {
                return proposedName;
            }

            var model = await document.GetSemanticModelAsync(cancellationToken);
            var declaredType = model.GetDeclaredSymbol(declaredTypeSyntax, cancellationToken) as INamedTypeSymbol;

            if (declaredType != null)
            {
                var reservedNames = declaredType.GetMembers().Select(m => m.Name).ToList();
                var name = proposedName;
                var index = 1;
                while (reservedNames.Contains(name))
                {
                    name = $"{name}{index++}";
                }

                return name;
            }

            return proposedName;
        }

        public string GetNewParameterName(ParameterListSyntax parameterList, string proposedName)
        {
            var reservedNames = parameterList.Parameters.Select(parameter => parameter.Identifier.ValueText).ToList();
            var name = StripPrefix(proposedName);
            var index = 1;
            while (reservedNames.Contains(name))
            {
                name = $"{name}{index++}";
            }

            return name;
        }

        private static readonly Regex prefixExpression = new Regex(@"^.{0,1}_(?<name>.+)$");
        private static string StripPrefix(string input)
        {
            var m = prefixExpression.Match(input);
            return m.Success ? m.Groups["name"].Value : input;
        }
    }
}
