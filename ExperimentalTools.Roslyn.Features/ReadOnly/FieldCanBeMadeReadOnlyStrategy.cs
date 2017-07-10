using ExperimentalTools.Roslyn.Refactoring;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExperimentalTools.Roslyn.Features.ReadOnly
{
    internal class FieldCanBeMadeReadOnlyStrategy : ICodeRefactoringStrategy
    {
        public async Task<CodeAction> CalculateActionAsync(Document document, SyntaxNode root, SyntaxNode selectedNode, CancellationToken cancellationToken)
        {
            var declaration = selectedNode.ParseFieldDeclaration();
            if (declaration == null)
            {
                return null;
            }

            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var fieldSymbol = model.GetDeclaredSymbol(declaration.Value.VariableDeclarator) as IFieldSymbol;
            if (fieldSymbol == null)
            {
                return null;
            }

            if (fieldSymbol.IsConst || fieldSymbol.IsReadOnly || fieldSymbol.IsVolatile)
            {
                return null;
            }

            return CodeAction.Create(Resources.FieldCanBeMadeReadOnly,
                token => AddReadOnlyModifierAsync(document, root, declaration.Value.FieldDeclaration));
        }

        private Task<Document> AddReadOnlyModifierAsync(Document document, SyntaxNode root, FieldDeclarationSyntax fieldDeclaration)
        {
            var newFieldDeclaration = fieldDeclaration.WithModifiers(fieldDeclaration.Modifiers.Add(Token(SyntaxKind.ReadOnlyKeyword)));
            var newRoot = root.ReplaceNode(fieldDeclaration, newFieldDeclaration);
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}
