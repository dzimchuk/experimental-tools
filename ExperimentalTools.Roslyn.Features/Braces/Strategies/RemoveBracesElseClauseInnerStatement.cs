using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentalTools.Roslyn.Features.Braces.Strategies
{
    internal class RemoveBracesElseClauseInnerStatement : RemoveBracesRefactoringStrategy
    {
        public override Task<CodeAction> CalculateActionAsync(Document document, SyntaxNode root, SyntaxNode selectedNode, CancellationToken cancellationToken)
        {
            var statement = selectedNode.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();
            if (statement == null ||
                statement is BlockSyntax ||
                !(statement.Parent is BlockSyntax) ||
                statement.Parent?.Parent == null)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var elseClause = statement.Parent.Parent as ElseClauseSyntax;
            if (elseClause == null)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var block = statement.Parent as BlockSyntax;
            var innerStatements = block.ChildNodes().OfType<StatementSyntax>().ToList();
            if (innerStatements.Count > 1)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var action =
                CodeAction.Create(
                    Resources.RemoveBraces,
                    token => RemoveBracesAsync(document, root, statement, elseClause, token));

            return Task.FromResult(action);
        }
    }
}
