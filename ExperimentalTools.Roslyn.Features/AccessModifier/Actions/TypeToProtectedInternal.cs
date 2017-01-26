using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ExperimentalTools.Roslyn.Features.AccessModifier.Actions
{
    internal class TypeToProtectedInternal : ITypeActionProvider
    {
        public CodeAction CalculateAction(Document document, SyntaxNode root, BaseTypeDeclarationSyntax typeDeclaration)
        {
            var accessModifiers = typeDeclaration.FindAccessModifiers();
            if (accessModifiers.Count == 2 && 
                accessModifiers.All(modifier => modifier.Kind() == SyntaxKind.ProtectedKeyword || modifier.Kind() == SyntaxKind.InternalKeyword))
            {
                return null;
            }
            
            return CodeAction.Create(Resources.ToProtectedInternal, cancellationToken =>
            {
                var modifiers = new List<SyntaxToken> { Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.InternalKeyword) };
                modifiers.AddRange(typeDeclaration.FindOtherModifiers());

                var newTypeDeclaration = typeDeclaration.WithModifiers(TokenList(modifiers));
                var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration);

                return Task.FromResult(document.WithSyntaxRoot(newRoot));
            });
        }
    }
}
