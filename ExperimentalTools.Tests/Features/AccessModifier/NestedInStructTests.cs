using ExperimentalTools.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Features.AccessModifier
{
    public class NestedInStructTests
    {
        [Theory, MemberData("HasActionTestData")]
        public async Task HasActionTest(string test, string actionTitle, string input, string expectedResult)
        {
            await TestRunner.RunAsync(input, async (acceptor, context) =>
            {
                Assert.True(acceptor.HasAction(actionTitle));

                var result = await acceptor.GetResultAsync(actionTitle, context);
                Assert.Equal(expectedResult.HomogenizeLineEndings(), result.HomogenizeLineEndings());
            });
        }

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "To PrivateExplicit",
                    Resources.ToPrivateExplicit,
                    @"
namespace HelloWorld
{
    struct Test
    {
        @::@class Nested
        {
        }
    }
}",
                    @"
namespace HelloWorld
{
    struct Test
    {
        private class Nested
        {
        }
    }
}"
                },
                new object[]
                {
                    "To PrivateImplicit",
                    Resources.ToPrivateImplicit,
                    @"
namespace HelloWorld
{
    struct Test
    {
        @::@public class Nested
        {
        }
    }
}",
                    @"
namespace HelloWorld
{
    struct Test
    {
        class Nested
        {
        }
    }
}"
                },
                new object[]
                {
                    "To Public",
                    Resources.ToPublic,
                    @"
namespace HelloWorld
{
    struct Test
    {
        @::@class Nested
        {
        }
    }
}",
                    @"
namespace HelloWorld
{
    struct Test
    {
        public class Nested
        {
        }
    }
}"
                },
                new object[]
                {
                    "To Internal",
                    Resources.ToInternal,
                    @"
namespace HelloWorld
{
    struct Test
    {
        @::@class Nested
        {
        }
    }
}",
                    @"
namespace HelloWorld
{
    struct Test
    {
        internal class Nested
        {
        }
    }
}"
                }
            };

        [Theory, MemberData("NoActionTestData")]
        public Task NoActionTest(string test, string actionTitle, string input)
        {
            return TestRunner.RunAsync(input, (acceptor, context) =>
             {
                 Assert.False(acceptor.HasAction(actionTitle));
                 return Task.FromResult(0);
             });
        }

        public static IEnumerable<object[]> NoActionTestData =>
            new[]
            {
                new object[]
                {
                    "To PrivateExplicit",
                    Resources.ToPrivateExplicit,
                    @"
namespace HelloWorld
{
    struct Test
    {
        @::@private class Nested
        {
        }
    }
}"
                },
                new object[]
                {
                    "To PrivateImplicit",
                    Resources.ToPrivateImplicit,
                    @"
namespace HelloWorld
{
    struct Test
    {
        @::@class Nested
        {
        }
    }
}"
                },
                new object[]
                {
                    "To Public",
                    Resources.ToPublic,
                    @"
namespace HelloWorld
{
    struct Test
    {
        @::@public class Nested
        {
        }
    }
}"
                },
                new object[]
                {
                    "To Protected",
                    Resources.ToProtected,
                    @"
namespace HelloWorld
{
    struct Test
    {
        @::@class Nested
        {
        }
    }
}"
                },
                new object[]
                {
                    "To Protected Internal",
                    Resources.ToProtectedInternal,
                    @"
namespace HelloWorld
{
    struct Test
    {
        @::@class Nested
        {
        }
    }
}"
                },
                new object[]
                {
                    "To Internal",
                    Resources.ToInternal,
                    @"
namespace HelloWorld
{
    struct Test
    {
        @::@internal class Nested
        {
        }
    }
}"
                }
            };
    }
}
