using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace ExperimentalTools.Roslyn.Features.Braces.Strategies
{
    internal class AddBracesInnerStatementStrategy : AddBracesRefactoringStrategy
    {
        public override Task<CodeAction> CalculateActionAsync(Document document, SyntaxNode root, SyntaxNode selectedNode, CancellationToken cancellationToken)
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

            var parentStatement = statement.Parent as StatementSyntax;
            if (parentStatement == null)
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
