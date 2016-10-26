using ExperimentalTools.Features.AccessModifier;
using ExperimentalTools.Features.AccessModifier.Recipes;
using ExperimentalTools.Tests.Infrastructure;
using ExperimentalTools.Tests.Infrastructure.ActionAcceptors;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System;
using System.Threading.Tasks;

namespace ExperimentalTools.Tests.Features.AccessModifier
{
    internal static class TestRunner
    {
        public static async Task RunAsync(string input, Func<MultipleCodeActionsAcceptor, CodeRefactoringContext, Task> verifyAsync)
        {
            var acceptor = new MultipleCodeActionsAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            var provider = new ChangeAccessModifierRefactoring(new TopLevelTypeRecipe());
            await provider.ComputeRefactoringsAsync(context);

            await verifyAsync(acceptor, context);
        }
    }
}
