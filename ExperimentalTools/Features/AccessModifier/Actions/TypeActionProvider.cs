using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExperimentalTools.Features.AccessModifier.Actions
{
    internal abstract class TypeActionProvider : ITypeActionProvider
    {
        private readonly string title;
        private readonly SyntaxKind targetAccessModifier;

        public TypeActionProvider(string title, SyntaxKind targetAccessModifier)
        {
            this.title = title;
            this.targetAccessModifier = targetAccessModifier;
        }
                
        public CodeAction CalculateAction(Document document, SyntaxNode root, BaseTypeDeclarationSyntax typeDeclaration)
        {
            var accessModifiers = typeDeclaration.FindAccessModifiers();
            if (accessModifiers.Count > 1)
            {
                return CodeAction.Create(title, cancellationToken =>
                {
                    var modifiers = new List<SyntaxToken> { Token(targetAccessModifier) };
                    modifiers.AddRange(typeDeclaration.FindOtherModifiers());

                    var newTypeDeclaration = typeDeclaration.WithModifiers(TokenList(modifiers));
                    var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration);

                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                });
            }
            else if (accessModifiers.Count == 1 && accessModifiers[0].Kind() != targetAccessModifier)
            {
                return CodeAction.Create(title, cancellationToken =>
                {
                    var newRoot = root.ReplaceToken(accessModifiers[0], Token(targetAccessModifier));
                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                });
            }
            else if (!accessModifiers.Any())
            {
                return CodeAction.Create(title, cancellationToken =>
                {
                    var modifiers = typeDeclaration.Modifiers.Insert(0, Token(targetAccessModifier));
                    var newTypeDeclaration = typeDeclaration.WithModifiers(modifiers);
                    var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration);

                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                });
            }

            return null;
        }
    }
}
