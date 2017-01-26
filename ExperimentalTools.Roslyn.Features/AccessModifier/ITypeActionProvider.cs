using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentalTools.Roslyn.Features.AccessModifier
{
    public interface ITypeActionProvider
    {
        CodeAction CalculateAction(Document document, SyntaxNode root, BaseTypeDeclarationSyntax typeDeclaration);
    }
}
