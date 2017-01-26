using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace ExperimentalTools.Roslyn.Features.AccessModifier
{
    public interface ITypeRecipe
    {
        bool CanHandle(BaseTypeDeclarationSyntax typeDeclaration);
        IEnumerable<CodeAction> Apply(Document document, SyntaxNode root, BaseTypeDeclarationSyntax typeDeclaration);
    }
}
