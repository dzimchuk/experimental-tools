using ExperimentalTools.Localization;
using ExperimentalTools.Refactorings;
using ExperimentalTools.Tests.Infrastructure;
using ExperimentalTools.Tests.Infrastructure.ActionAcceptors;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Refactorings
{
    public class ChangeAccessModifierRefactoringTests
    {
        private static async Task Run(string input, Func<MultipleCodeActionsAcceptor, CodeRefactoringContext, Task> verifyAsync)
        {
            var acceptor = new MultipleCodeActionsAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            var provider = new ChangeAccessModifierRefactoring();
            await provider.ComputeRefactoringsAsync(context);

            await verifyAsync(acceptor, context);
        }

        [Fact]
        public async Task TopLevelImplicitClass_ToInternalExplicit()
        {
            var input = @"
namespace HelloWorld
{
    @::@class Test
    {
    }
}
";
            var expectedResult = @"
namespace HelloWorld
{
    internal class Test
    {
    }
}
";
            await Run(input, async (acceptor, context) =>
            {
                Assert.Equal(2, acceptor.Count);
                Assert.True(acceptor.HasAction(Resources.ToInternalExplicit));

                var result = await acceptor.GetResultAsync(Resources.ToInternalExplicit, context);
                Assert.Equal(expectedResult.HomogenizeLineEndings(), result.HomogenizeLineEndings());
            });
        }

        [Fact]
        public async Task TopLevelImplicitClass_ToInternalPublic()
        {
            var input = @"
namespace HelloWorld
{
    @::@class Test
    {
    }
}
";
            var expectedResult = @"
namespace HelloWorld
{
    public class Test
    {
    }
}
";
            await Run(input, async (acceptor, context) =>
            {
                Assert.Equal(2, acceptor.Count);
                Assert.True(acceptor.HasAction(Resources.ToPublic));

                var result = await acceptor.GetResultAsync(Resources.ToPublic, context);
                Assert.Equal(expectedResult.HomogenizeLineEndings(), result.HomogenizeLineEndings());
            });
        }
    }
}
