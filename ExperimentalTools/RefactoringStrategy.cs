using ExperimentalTools.Roslyn.Contracts;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace ExperimentalTools
{
    internal abstract class RefactoringStrategy : IRefactoringStrategy
    {
        public async Task ComputeRefactoringAsync(CodeRefactoringContext context)
        {
            if (context.Document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
            {
                return;
            }

            if (!context.Span.IsEmpty)
            {
                return;
            }

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

            if (node == null)
            {
                return;
            }

            var action = await ComputeRefactoringAsync(context.Document, root, node);
            if (action != null)
            {
                context.RegisterRefactoring(action);
            }
        }

        protected abstract Task<CodeAction> ComputeRefactoringAsync(Document document, SyntaxNode root, SyntaxNode selectedNode);
    }
}
