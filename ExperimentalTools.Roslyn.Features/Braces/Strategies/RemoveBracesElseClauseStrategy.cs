using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentalTools.Roslyn.Features.Braces.Strategies
{
    internal class RemoveBracesElseClauseStrategy : RemoveBracesRefactoringStrategy
    {
        public override Task<CodeAction> CalculateActionAsync(Document document, SyntaxNode root, SyntaxNode selectedNode, CancellationToken cancellationToken)
        {
            var elseClause = selectedNode as ElseClauseSyntax;
            if (elseClause == null)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var childStatements = elseClause.ChildNodes().OfType<StatementSyntax>().ToList();
            if (childStatements.Count != 1)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var block = childStatements.First() as BlockSyntax;
            if (block == null)
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
