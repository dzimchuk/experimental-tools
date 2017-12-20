using ExperimentalTools.Roslyn.Refactoring;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using System.Collections.Generic;
using System.Linq;
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
            
            var writeExpressions = await FindWriteExpressionsAsync(fieldSymbol, document, cancellationToken).ConfigureAwait(false);

            if (!writeExpressions.Any())
            {
                return declaration.Value.VariableDeclarator.Initializer == null
                    ? null
                    : CodeAction.Create(Resources.FieldCanBeMadeReadOnly, token => AddReadOnlyModifierAsync(document, root, declaration.Value.FieldDeclaration));
            }

            if (!AreAllWritesInConstructorsOfDeclaringType(writeExpressions, declaration.Value.FieldDeclaration, fieldSymbol.IsStatic, model, cancellationToken))
            {
                return null;
            }

            return CodeAction.Create(Resources.FieldCanBeMadeReadOnly,
                token => AddReadOnlyModifierAsync(document, root, declaration.Value.FieldDeclaration));
        }
        
        private static bool AreAllWritesInConstructorsOfDeclaringType(IEnumerable<ExpressionSyntax> writeExpressions, FieldDeclarationSyntax fieldDeclaration, 
            bool shouldBeStatic, SemanticModel model, CancellationToken cancellationToken)
        {
            var typeDeclaration = fieldDeclaration.GetParentTypeDeclaration();
            if (typeDeclaration == null)
            {
                return false;
            }

            var constructors = typeDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>().ToList();
            if (!constructors.Any())
            {
                return false;
            }

            var expressions = from expression in writeExpressions
                              let constructor = expression.Ancestors().OfType<ConstructorDeclarationSyntax>().FirstOrDefault()
                              where constructor != null && constructors.Contains(constructor)
                              let isStatic = (model.GetDeclaredSymbol(constructor, cancellationToken).IsStatic)
                              where isStatic == shouldBeStatic
                              select expression;

            return expressions.Count() == writeExpressions.Count();
        }

        private static async Task<IEnumerable<ExpressionSyntax>> FindWriteExpressionsAsync(IFieldSymbol fieldSymbol, Document document, CancellationToken cancellationToken)
        {
            var references = await SymbolFinder.FindReferencesAsync(fieldSymbol, document.Project.Solution, cancellationToken).ConfigureAwait(false);
            var locations = references.SelectMany(reference => reference.Locations).ToArray();
            if (!locations.Any())
            {
                return new ExpressionSyntax[0];
            }

            var expressions = new List<ExpressionSyntax>();
            foreach (var location in locations)
            {
                var root = await location.Document.GetSyntaxRootAsync(cancellationToken);
                var node = root.FindNode(location.Location.SourceSpan);
                if (node != null)
                {
                    var expression = node.GetAncestorOrThis<ExpressionSyntax>();
                    if (expression != null && expression.IsWrittenTo())
                    {
                        expressions.Add(expression);
                    }
                }
            }

            return expressions;
        }

        private static Task<Document> AddReadOnlyModifierAsync(Document document, SyntaxNode root, FieldDeclarationSyntax fieldDeclaration)
        {
            var newFieldDeclaration = fieldDeclaration.WithModifiers(fieldDeclaration.Modifiers.Add(Token(SyntaxKind.ReadOnlyKeyword)));
            var newRoot = root.ReplaceNode(fieldDeclaration, newFieldDeclaration);
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}
