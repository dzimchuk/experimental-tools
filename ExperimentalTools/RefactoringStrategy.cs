using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace ExperimentalTools
{
    internal abstract class RefactoringStrategy
    {
        public async Task<bool> ComputeRefactoringAsync(CodeRefactoringContext context)
        {
            if (context.Document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
            {
                return false;
            }

            if (!context.Span.IsEmpty)
            {
                return false;
            }

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

            if (node == null)
            {
                return false;
            }

            var action = await ComputeRefactoringAsync(context.Document, root, node);
            if (action != null)
            {
                context.RegisterRefactoring(action);
                return true;
            }

            return false;
        }

        protected abstract Task<CodeAction> ComputeRefactoringAsync(Document document, SyntaxNode root, SyntaxNode selectedNode);
    }
}
