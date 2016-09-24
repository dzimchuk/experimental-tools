using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Composition;

namespace ExperimentalTools.Components
{
    [Export(typeof(INameGenerator))]
    internal class SimpleNameGenerator : INameGenerator
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
            var name = proposedName;
            var index = 1;
            while (reservedNames.Contains(name))
            {
                name = $"{name}{index++}";
            }

            return name;
        }
    }
}
