using ExperimentalTools.Features.AccessModifier;
using ExperimentalTools.Features.AccessModifier.Recipes;
using ExperimentalTools.Localization;
using ExperimentalTools.Tests.Infrastructure;
using ExperimentalTools.Tests.Infrastructure.ActionAcceptors;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Features.AccessModifier
{
    public class TopLevelClassTests
    {
        private static async Task Run(string input, Func<MultipleCodeActionsAcceptor, CodeRefactoringContext, Task> verifyAsync)
        {
            var acceptor = new MultipleCodeActionsAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);
            
            var provider = new ChangeAccessModifierRefactoring(new TopLevelTypeRecipe());
            await provider.ComputeRefactoringsAsync(context);

            await verifyAsync(acceptor, context);
        }

        [Fact]
        public async Task Implicit_ToInternalExplicit()
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
                Assert.True(acceptor.HasAction(Resources.ToInternalExplicit));

                var result = await acceptor.GetResultAsync(Resources.ToInternalExplicit, context);
                Assert.Equal(expectedResult.HomogenizeLineEndings(), result.HomogenizeLineEndings());
            });
        }

        [Fact]
        public async Task Implicit_ToPublic()
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
                Assert.True(acceptor.HasAction(Resources.ToPublic));

                var result = await acceptor.GetResultAsync(Resources.ToPublic, context);
                Assert.Equal(expectedResult.HomogenizeLineEndings(), result.HomogenizeLineEndings());
            });
        }

        [Fact]
        public async Task Public_ToInternalExplicit()
        {
            var input = @"
namespace HelloWorld
{
    @::@public class Test
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
                Assert.True(acceptor.HasAction(Resources.ToInternalExplicit));

                var result = await acceptor.GetResultAsync(Resources.ToInternalExplicit, context);
                Assert.Equal(expectedResult.HomogenizeLineEndings(), result.HomogenizeLineEndings());
            });
        }

        [Fact]
        public async Task Internal_ToPublic()
        {
            var input = @"
namespace HelloWorld
{
    internal @::@class Test
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
                Assert.True(acceptor.HasAction(Resources.ToPublic));

                var result = await acceptor.GetResultAsync(Resources.ToPublic, context);
                Assert.Equal(expectedResult.HomogenizeLineEndings(), result.HomogenizeLineEndings());
            });
        }

        [Fact]
        public async Task MultipleModifiers_ToInternalExplicit()
        {
            var input = @"
namespace HelloWorld
{
    private static @::@public class Test
    {
    }
}
";
            var expectedResult = @"
namespace HelloWorld
{
    internal static class Test
    {
    }
}
";
            await Run(input, async (acceptor, context) =>
            {
                Assert.True(acceptor.HasAction(Resources.ToInternalExplicit));

                var result = await acceptor.GetResultAsync(Resources.ToInternalExplicit, context);
                Assert.Equal(expectedResult.HomogenizeLineEndings(), result.HomogenizeLineEndings());
            });
        }

        [Fact]
        public async Task MultipleModifiers_ToPublic()
        {
            var input = @"
namespace HelloWorld
{
    private static @::@public class Test
    {
    }
}
";
            var expectedResult = @"
namespace HelloWorld
{
    public static class Test
    {
    }
}
";
            await Run(input, async (acceptor, context) =>
            {
                Assert.True(acceptor.HasAction(Resources.ToPublic));

                var result = await acceptor.GetResultAsync(Resources.ToPublic, context);
                Assert.Equal(expectedResult.HomogenizeLineEndings(), result.HomogenizeLineEndings());
            });
        }

        [Fact]
        public async Task Public_ToInternalImplicit()
        {
            var input = @"
namespace HelloWorld
{
    @::@public class Test
    {
    }
}
";
            var expectedResult = @"
namespace HelloWorld
{
    class Test
    {
    }
}
";
            await Run(input, async (acceptor, context) =>
            {
                Assert.True(acceptor.HasAction(Resources.ToInternalImplicit));

                var result = await acceptor.GetResultAsync(Resources.ToInternalImplicit, context);
                Assert.Equal(expectedResult.HomogenizeLineEndings(), result.HomogenizeLineEndings());
            });
        }

        [Fact]
        public async Task Internal_ToInternalImplicit()
        {
            var input = @"
namespace HelloWorld
{
    @::@internal class Test
    {
    }
}
";
            var expectedResult = @"
namespace HelloWorld
{
    class Test
    {
    }
}
";
            await Run(input, async (acceptor, context) =>
            {
                Assert.True(acceptor.HasAction(Resources.ToInternalImplicit));

                var result = await acceptor.GetResultAsync(Resources.ToInternalImplicit, context);
                Assert.Equal(expectedResult.HomogenizeLineEndings(), result.HomogenizeLineEndings());
            });
        }

        [Fact]
        public async Task MultipleModifiers_ToInternalImplicit()
        {
            var input = @"
namespace HelloWorld
{
    internal protected static class Test@::@
    {
    }
}
";
            var expectedResult = @"
namespace HelloWorld
{
    static class Test
    {
    }
}
";
            await Run(input, async (acceptor, context) =>
            {
                Assert.True(acceptor.HasAction(Resources.ToInternalImplicit));

                var result = await acceptor.GetResultAsync(Resources.ToInternalImplicit, context);
                Assert.Equal(expectedResult.HomogenizeLineEndings(), result.HomogenizeLineEndings());
            });
        }
    }
}
