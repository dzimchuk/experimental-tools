using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Threading;

namespace ExperimentalTools.Features.Braces
{
    internal abstract class AddBracesRefactoringStrategy : RefactoringStrategy
    {
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
