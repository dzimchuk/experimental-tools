using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Threading;

namespace ExperimentalTools.Tests.Infrastructure.ActionAcceptors
{
    internal class MultipleCodeActionsAcceptor : ICodeActionAcceptor
    {
        private readonly Dictionary<string, CodeAction> actions = new Dictionary<string, CodeAction>();

        public void Accept(CodeAction action)
        {
            actions.Add(action.Title, action);
        }

        public int Count => actions.Count;

        public bool HasAction(string title) => actions.ContainsKey(title);

        public async Task<string> GetResultAsync(string title, CodeRefactoringContext context)
        {
            var workspace = context.Document.Project.Solution.Workspace;
            var action = GetAction(title);

            var operations = await action.GetOperationsAsync(CancellationToken.None);
            foreach (var operation in operations)
            {
                operation.Apply(workspace, CancellationToken.None);
            }

            var updatedDocument = workspace.CurrentSolution.GetDocument(context.Document.Id);
            return (await updatedDocument.GetTextAsync(CancellationToken.None)).ToString();
        }

        private CodeAction GetAction(string title)
        {
            CodeAction action;
            if (!actions.TryGetValue(title, out action))
            {
                throw new ArgumentException($"Action '{title}' not found.");
            }

            actions.Clear();

            return action;
        }
    }
}
