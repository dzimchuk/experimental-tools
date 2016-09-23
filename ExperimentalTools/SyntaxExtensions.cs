using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace ExperimentalTools
{
    internal static class SyntaxExtensions
    {
        public static TypeDeclarationSyntax GetParentTypeDeclaration(this SyntaxNode node) => node.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
    }
}
