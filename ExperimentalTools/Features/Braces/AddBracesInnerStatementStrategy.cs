using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ExperimentalTools.Localization;

namespace ExperimentalTools.Features.Braces
{
    internal class AddBracesInnerStatementStrategy : AddBracesRefactoringStrategy
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

            var parentStatement = statement.Parent as StatementSyntax;
            if (parentStatement == null)
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
