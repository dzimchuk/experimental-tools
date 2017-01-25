using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExperimentalTools.Roslyn.Features.AccessModifier.Actions
{
    internal class TypeToPrivateImplicit : ITypeActionProvider
    {
        public CodeAction CalculateAction(Document document, SyntaxNode root, BaseTypeDeclarationSyntax typeDeclaration)
        {
            var accessModifiers = typeDeclaration.FindAccessModifiers();
            if (accessModifiers.Count >= 1)
            {
                return CodeAction.Create(Resources.ToPrivateImplicit, cancellationToken =>
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
