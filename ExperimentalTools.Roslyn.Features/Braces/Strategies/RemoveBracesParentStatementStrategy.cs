using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExperimentalTools.Roslyn.Features.Braces.Strategies
{
    internal class RemoveBracesParentStatementStrategy : RemoveBracesRefactoringStrategy
    {
        public override Task<CodeAction> CalculateActionAsync(Document document, SyntaxNode root, SyntaxNode selectedNode, CancellationToken cancellationToken)
        {
            var parentStatement = selectedNode.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();
            if (parentStatement == null)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var childStatements = parentStatement.ChildNodes().OfType<StatementSyntax>().ToList();
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
                    token => RemoveBracesAsync(document, root, innerStatements.First(), parentStatement, token));

            return Task.FromResult(action);
        }
    }
}
