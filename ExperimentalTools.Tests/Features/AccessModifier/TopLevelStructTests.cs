using ExperimentalTools.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Features.AccessModifier
{
    public class TopLevelStructTests
    {
        [Theory, MemberData("TestData")]
        public async Task Test(string test, string actionTitle, string input, string expectedResult)
        {
            await TestRunner.RunAsync(input, async (acceptor, context) =>
            {
                Assert.True(acceptor.HasAction(actionTitle));

                var result = await acceptor.GetResultAsync(actionTitle, context);
                Assert.Equal(expectedResult.HomogenizeLineEndings(), result.HomogenizeLineEndings());
            });
        }

        public static IEnumerable<object[]> TestData =>
            new[]
            {
                new object[]
                {
                    "Implicit to InternalExplicit",
                    Resources.ToInternalExplicit,
                    @"
namespace HelloWorld
{
    @::@struct Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    internal struct Test
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
    @::@struct Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    public struct Test
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
    @::@public struct Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    internal struct Test
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
    internal @::@struct Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    public struct Test
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
    private @::@public struct Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    internal struct Test
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
    private @::@public struct Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    public struct Test
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
    @::@public struct Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    struct Test
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
    @::@internal struct Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    struct Test
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
    internal protected struct Test@::@
    {
    }
}",
                    @"
namespace HelloWorld
{
    struct Test
    {
    }
}"
                }
            };
    }
}
