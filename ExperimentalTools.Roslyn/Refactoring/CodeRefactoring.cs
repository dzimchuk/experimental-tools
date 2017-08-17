using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExperimentalTools.Roslyn.Refactoring
{
    public class CodeRefactoring
    {
        private readonly List<ICodeRefactoringStrategy> strategies;
        private readonly bool allowMultipleActions;

        public CodeRefactoring(ICodeRefactoringStrategy strategy)
            : this(new List<ICodeRefactoringStrategy> { strategy })
        {
        }

        public CodeRefactoring(List<ICodeRefactoringStrategy> strategies, bool allowMultipleActions = false)
        {
            this.strategies = strategies;
            this.allowMultipleActions = allowMultipleActions;
        }

        public async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
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

            foreach (var strategy in strategies)
            {
                var action = await strategy.CalculateActionAsync(context.Document, root, node, context.CancellationToken).ConfigureAwait(false);
                if (action != null)
                {
                    context.RegisterRefactoring(action);
                    if (!allowMultipleActions)
                    {
                        break;
                    }
                }
            }
        }
    }
}
