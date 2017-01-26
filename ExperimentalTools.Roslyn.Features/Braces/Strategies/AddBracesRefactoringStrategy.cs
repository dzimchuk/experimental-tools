using ExperimentalTools.Roslyn.Refactoring;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExperimentalTools.Roslyn.Features.Braces.Strategies
{
    internal abstract class AddBracesRefactoringStrategy : ICodeRefactoringStrategy
    {
        public abstract Task<CodeAction> CalculateActionAsync(Document document, SyntaxNode root, SyntaxNode selectedNode, CancellationToken cancellationToken);

        protected Task<Document> AddBracesAsync(Document document, SyntaxNode root, StatementSyntax statement, StatementSyntax parentStatement, CancellationToken cancellationToken)
        {
            var newParentStatement = parentStatement.WithStatement(Block(statement));
            var newRoot = root.ReplaceNode(parentStatement, newParentStatement);
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }

        protected Task<Document> AddBracesAsync(Document document, SyntaxNode root, StatementSyntax statement, ElseClauseSyntax elseClause, CancellationToken cancellationToken)
        {
            var newElseClause = elseClause.WithStatement(Block(statement));
            var newRoot = root.ReplaceNode(elseClause, newElseClause);
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}
