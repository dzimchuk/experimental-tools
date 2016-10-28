using ExperimentalTools.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Features.AccessModifier
{
    public class TopLevelClassTests
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
                    "Implicit to InternalExplicit",
                    Resources.ToInternalExplicit,
                    @"
namespace HelloWorld
{
    @::@class Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    internal class Test
    {
    }
}"
                },
                new object[]
                {
                    "Implicit to Public",
                    Resources.ToPublic,
                    @"
namespace HelloWorld
{
    @::@class Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    public class Test
    {
    }
}"
                },
                new object[]
                {
                    "Public to InternalExplicit",
                    Resources.ToInternalExplicit,
                    @"
namespace HelloWorld
{
    @::@public class Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    internal class Test
    {
    }
}"
                },
                new object[]
                {
                    "Internal to Public",
                    Resources.ToPublic,
                    @"
namespace HelloWorld
{
    internal @::@class Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    public class Test
    {
    }
}"
                },
                new object[]
                {
                    "Multiple modifiers to InternalExplicit",
                    Resources.ToInternalExplicit,
                    @"
namespace HelloWorld
{
    private static @::@public class Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    internal static class Test
    {
    }
}"
                },
                new object[]
                {
                    "Multiple modifiers to Public",
                    Resources.ToPublic,
                    @"
namespace HelloWorld
{
    private static @::@public class Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    public static class Test
    {
    }
}"
                },
                new object[]
                {
                    "Public to InternalImplicit",
                    Resources.ToInternalImplicit,
                    @"
namespace HelloWorld
{
    @::@public class Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    class Test
    {
    }
}"
                },
                new object[]
                {
                    "Internal to InternalImplicit",
                    Resources.ToInternalImplicit,
                    @"
namespace HelloWorld
{
    @::@internal class Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    class Test
    {
    }
}"
                },
                new object[]
                {
                    "Multiple modifiers to InternalImplicit",
                    Resources.ToInternalImplicit,
                    @"
namespace HelloWorld
{
    internal protected static class Test@::@
    {
    }
}",
                    @"
namespace HelloWorld
{
    static class Test
    {
    }
}"
                }
            };

        [Theory, MemberData("NoActionTestData")]
        public async Task NoActionTest(string test, string actionTitle, string input)
        {
            await TestRunner.RunAsync(input, (acceptor, context) =>
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
                    "To InternalExplicit",
                    Resources.ToInternalExplicit,
                    @"
namespace HelloWorld
{
    @::@internal class Test
    {
    }
}"
                },
                new object[]
                {
                    "To InternalImplicit",
                    Resources.ToInternalImplicit,
                    @"
namespace HelloWorld
{
    @::@class Test
    {
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
    @::@public class Test
    {
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
    @::@class Test
    {
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
    @::@class Test
    {
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
    @::@class Test
    {
    }
}"
                }
            };
    }
}
