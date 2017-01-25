using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ExperimentalTools.Localization;

namespace ExperimentalTools.Features.Braces
{
    internal class AddBracesElseClauseStrategy : AddBracesRefactoringStrategy
    {
        protected override Task<CodeAction> ComputeRefactoringAsync(Document document, SyntaxNode root, SyntaxNode selectedNode)
        {
            var elseClause = selectedNode as ElseClauseSyntax;
            if (elseClause == null)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var statement = elseClause.ChildNodes().OfType<StatementSyntax>().FirstOrDefault();
            if (statement == null || statement is BlockSyntax)
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
