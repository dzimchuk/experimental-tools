using ExperimentalTools.Tests.Infrastructure.ActionAcceptors;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Infrastructure.Refactoring
{
    public abstract class RefactoringTest
    {
        protected abstract CodeRefactoringProvider Provider { get; }

        public async Task RunSingleActionTestAsync(string input, string expectedOutput)
        {
            var acceptor = new SingleCodeActionAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            await Provider.ComputeRefactoringsAsync(context);

            Assert.True(acceptor.HasAction);

            var result = await acceptor.GetResultAsync(context);
            Assert.Equal(expectedOutput.HomogenizeLineEndings(), result.HomogenizeLineEndings());
        }

        public async Task RunMultipleActionsTestAsync(string actionTitle, string input, string expectedOutput)
        {
            var acceptor = new MultipleCodeActionsAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            await Provider.ComputeRefactoringsAsync(context);

            Assert.True(acceptor.HasAction(actionTitle));

            var result = await acceptor.GetResultAsync(actionTitle, context);
            Assert.Equal(expectedOutput.HomogenizeLineEndings(), result.HomogenizeLineEndings());
        }

        public async Task RunNoActionTestAsync(string input)
        {
            var acceptor = new SingleCodeActionAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            await Provider.ComputeRefactoringsAsync(context);

            Assert.False(acceptor.HasAction);
        }

        public async Task RunNoSpecificActionTestAsync(string actionTitle, string input)
        {
            var acceptor = new MultipleCodeActionsAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            await Provider.ComputeRefactoringsAsync(context);

            Assert.False(acceptor.HasAction(actionTitle));
        }
    }
}
