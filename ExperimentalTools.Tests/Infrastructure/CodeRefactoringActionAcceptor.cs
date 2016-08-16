using Microsoft.CodeAnalysis.CodeActions;

namespace ExperimentalTools.Tests.Infrastructure
{
    internal class CodeRefactoringActionAcceptor
    {
        private CodeAction action;

        public void Accept(CodeAction action)
        {
            this.action = action;
        }

        public bool HasAction => action != null;
    }
}
