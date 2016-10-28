using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Threading;
using System.Threading.Tasks;

namespace ExperimentalTools.Tests.Infrastructure.ActionAcceptors
{
    internal class SingleCodeActionAcceptor : ICodeActionAcceptor
    {
        private CodeAction action;

        public void Accept(CodeAction action)
        {
            this.action = action;
        }

        public bool HasAction => action != null;
        
        public async Task<string> GetResultAsync(CodeRefactoringContext context)
        {
            var workspace = context.Document.Project.Solution.Workspace;

            var operations = await action.GetOperationsAsync(CancellationToken.None);
            foreach (var operation in operations)
            {   
                operation.Apply(workspace, CancellationToken.None);
            }

            var updatedDocument = workspace.CurrentSolution.GetDocument(context.Document.Id);
            return (await updatedDocument.GetTextAsync(CancellationToken.None)).ToString();
        }
    }
}
