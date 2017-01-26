using ExperimentalTools.Roslyn.Refactoring;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentalTools.Roslyn.Features.Braces.Strategies
{
    internal abstract class RemoveBracesRefactoringStrategy : ICodeRefactoringStrategy
    {
        public abstract Task<CodeAction> CalculateActionAsync(Document document, SyntaxNode root, SyntaxNode selectedNode, CancellationToken cancellationToken);

        protected Task<Document> RemoveBracesAsync(Document document, SyntaxNode root, StatementSyntax statement, StatementSyntax parentStatement, CancellationToken token)
        {
            var newParentStatement = parentStatement.WithStatement(statement);
            var newRoot = root.ReplaceNode(parentStatement, newParentStatement);
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }

        protected Task<Document> RemoveBracesAsync(Document document, SyntaxNode root, StatementSyntax statement, ElseClauseSyntax elseClause, CancellationToken token)
        {
            var newElseClause = elseClause.WithStatement(statement);
            var newRoot = root.ReplaceNode(elseClause, newElseClause);
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}
