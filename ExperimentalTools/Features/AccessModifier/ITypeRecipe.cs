using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace ExperimentalTools.Features.AccessModifier
{
    public interface ITypeRecipe
    {
        IEnumerable<CodeAction> Apply(Document document, SyntaxNode root, BaseTypeDeclarationSyntax typeDeclaration);
    }
}
