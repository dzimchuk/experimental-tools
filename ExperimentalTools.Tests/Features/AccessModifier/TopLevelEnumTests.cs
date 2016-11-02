using ExperimentalTools.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Features.AccessModifier
{
    public class TopLevelEnumTests : AccessModifierTest
    {
        [Theory, MemberData("TestData")]
        public Task HasActionTest(string test, string actionTitle, string input, string expectedResult) =>
            RunMultipleActionsTestAsync(actionTitle, input, expectedResult);

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
    @::@enum Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    internal enum Test
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
    @::@enum Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    public enum Test
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
    @::@public enum Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    internal enum Test
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
    internal @::@enum Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    public enum Test
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
    private @::@public enum Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    internal enum Test
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
    private @::@public enum Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    public enum Test
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
    @::@public enum Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    enum Test
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
    @::@internal enum Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    enum Test
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
    internal protected enum Test@::@
    {
    }
}",
                    @"
namespace HelloWorld
{
    enum Test
    {
    }
}"
                }
            };
    }
}
