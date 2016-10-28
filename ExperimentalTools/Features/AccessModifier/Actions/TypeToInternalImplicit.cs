using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ExperimentalTools.Localization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Formatting;

namespace ExperimentalTools.Features.AccessModifier.Actions
{
    internal class TypeToInternalImplicit : ITypeActionProvider
    {
        public CodeAction CalculateAction(Document document, SyntaxNode root, BaseTypeDeclarationSyntax typeDeclaration)
        {
            var accessModifiers = typeDeclaration.FindAccessModifiers();
            if (accessModifiers.Count >= 1)
            {
                return CodeAction.Create(Resources.ToInternalImplicit, cancellationToken =>
                {
                    var newTypeDeclaration = typeDeclaration.WithModifiers(TokenList(typeDeclaration.FindOtherModifiers()));
                    var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration)
                        .WithAdditionalAnnotations(Formatter.Annotation);

                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                });
            }

            return null;
        }
    }
}
