using ExperimentalTools.Tests.Infrastructure.ActionAcceptors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ExperimentalTools.Tests.Infrastructure.Refactoring
{
    public abstract class RefactoringTest
    {
        private readonly ITestOutputHelper output;

        public RefactoringTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        protected abstract CodeRefactoringProvider Provider { get; }
        protected virtual IEnumerable<MetadataReference> AdditionalReferences { get; }

        public async Task RunSingleActionTestAsync(string input, string expectedOutput)
        {
            var acceptor = new SingleCodeActionAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor, AdditionalReferences);

            await Provider.ComputeRefactoringsAsync(context);

            Assert.True(acceptor.HasAction);

            var result = await acceptor.GetResultAsync(context);

            var normalizedExpected = expectedOutput.HomogenizeLineEndings();
            var normalizedActual = result.HomogenizeLineEndings();
            output.WriteLine(normalizedActual);

            Assert.Equal(normalizedExpected, normalizedActual);
        }

        public async Task RunMultipleActionsTestAsync(string actionTitle, string input, string expectedOutput)
        {
            var acceptor = new MultipleCodeActionsAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor, AdditionalReferences);

            await Provider.ComputeRefactoringsAsync(context);

            Assert.True(acceptor.HasAction(actionTitle));

            var result = await acceptor.GetResultAsync(actionTitle, context);

            var normalizedExpected = expectedOutput.HomogenizeLineEndings();
            var normalizedActual = result.HomogenizeLineEndings();
            output.WriteLine(normalizedActual);

            Assert.Equal(normalizedExpected, normalizedActual);
        }

        public async Task RunNoActionTestAsync(params string[] input)
        {
            var acceptor = new SingleCodeActionAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor, AdditionalReferences);

            await Provider.ComputeRefactoringsAsync(context);

            Assert.False(acceptor.HasAction);
        }

        public async Task RunNoSpecificActionTestAsync(string actionTitle, string input)
        {
            var acceptor = new MultipleCodeActionsAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor, AdditionalReferences);

            await Provider.ComputeRefactoringsAsync(context);

            Assert.False(acceptor.HasAction(actionTitle));
        }
    }
}
