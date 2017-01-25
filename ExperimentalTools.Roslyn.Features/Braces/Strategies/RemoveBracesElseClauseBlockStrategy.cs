using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentalTools.Roslyn.Features.Braces.Strategies
{
    internal class RemoveBracesElseClauseBlockStrategy : RemoveBracesRefactoringStrategy
    {
        public override Task<CodeAction> CalculateActionAsync(Document document, SyntaxNode root, SyntaxNode selectedNode, CancellationToken cancellationToken)
        {
            var block = selectedNode as BlockSyntax;
            if (block == null || block.Parent == null)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var elseClause = block.Parent as ElseClauseSyntax;
            if (elseClause == null)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var innerStatements = block.ChildNodes().OfType<StatementSyntax>().ToList();
            if (innerStatements.Count != 1)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var action =
                CodeAction.Create(
                    Resources.RemoveBraces,
                    token => RemoveBracesAsync(document, root, innerStatements.First(), elseClause, token));

            return Task.FromResult(action);
        }
    }
}
