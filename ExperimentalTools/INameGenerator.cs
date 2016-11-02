using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Threading.Tasks;

namespace ExperimentalTools
{
    internal interface INameGenerator
    {
        Task<string> GetNewMemberNameAsync(TypeDeclarationSyntax declaredTypeSyntax, string proposedName, Document document, CancellationToken cancellationToken);
        string GetNewParameterName(ParameterListSyntax parameterList, string proposedName);
    }
}
