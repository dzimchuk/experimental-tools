using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ExperimentalTools.Localization;

namespace ExperimentalTools.Features.Braces
{
    internal class AddBracesElseClauseInnerStatementStrategy : AddBracesRefactoringStrategy
    {
        protected override Task<CodeAction> ComputeRefactoringAsync(Document document, SyntaxNode root, SyntaxNode selectedNode)
        {
            var statement = selectedNode.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();
            if (statement == null || statement.Parent == null)
            {
                return Task.FromResult<CodeAction>(null);
            }

            if (statement is BlockSyntax || statement.Parent is BlockSyntax)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var elseClause = statement.Parent as ElseClauseSyntax;
            if (elseClause == null)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var action =
                CodeAction.Create(
                    Resources.AddBraces,
                    cancellationToken => AddBracesAsync(document, root, statement, elseClause, cancellationToken));

            return Task.FromResult(action);
        }
    }
}
