using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ExperimentalTools.Localization;
using System.Threading;

namespace ExperimentalTools.Features.Braces.Strategies
{
    internal class AddBracesElseClauseStrategy : AddBracesRefactoringStrategy
    {
        public override Task<CodeAction> CalculateActionAsync(Document document, SyntaxNode root, SyntaxNode selectedNode, CancellationToken cancellationToken)
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
                    token => AddBracesAsync(document, root, statement, elseClause, token));

            return Task.FromResult(action);
        }
    }
}
