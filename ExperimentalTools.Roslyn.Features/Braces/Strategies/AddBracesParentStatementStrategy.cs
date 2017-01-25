using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace ExperimentalTools.Roslyn.Features.Braces.Strategies
{
    internal class AddBracesParentStatementStrategy : AddBracesRefactoringStrategy
    {
        public override Task<CodeAction> CalculateActionAsync(Document document, SyntaxNode root, SyntaxNode selectedNode, CancellationToken cancellationToken)
        {
            var elseClause = selectedNode as ElseClauseSyntax;
            if (elseClause != null)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var parentStatement = selectedNode.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();
            if (parentStatement == null)
            {
                return Task.FromResult<CodeAction>(null);
            }

            if (parentStatement is BlockSyntax)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var statement = parentStatement.ChildNodes().OfType<StatementSyntax>().FirstOrDefault();
            if (statement == null || statement is BlockSyntax)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var action =
                CodeAction.Create(
                    Resources.AddBraces,
                    token => AddBracesAsync(document, root, statement, parentStatement, token));

            return Task.FromResult(action);
        }
    }
}
