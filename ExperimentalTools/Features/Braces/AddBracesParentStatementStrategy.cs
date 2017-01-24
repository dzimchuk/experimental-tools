using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ExperimentalTools.Localization;

namespace ExperimentalTools.Features.Braces
{
    internal class AddBracesParentStatementStrategy : AddBracesRefactoringStrategy
    {
        protected override Task<CodeAction> ComputeRefactoringAsync(Document document, SyntaxNode root, SyntaxNode selectedNode)
        {
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
                    cancellationToken => AddBracesAsync(document, root, statement, parentStatement, cancellationToken));

            return Task.FromResult(action);
        }
    }
}
