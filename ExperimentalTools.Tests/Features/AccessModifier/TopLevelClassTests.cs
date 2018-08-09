using ExperimentalTools.Roslyn.Features;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ExperimentalTools.Tests.Features.AccessModifier
{
    public class TopLevelClassTests : AccessModifierTest
    {
        public TopLevelClassTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Theory, MemberData(nameof(HasActionTestData))]
        public Task HasActionTest(string test, string actionTitle, string input, string expectedResult) =>
            RunMultipleActionsTestAsync(actionTitle, input, expectedResult);

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

        [Theory, MemberData(nameof(NoActionTestData))]
        public Task NoActionTest(string test, string actionTitle, string input) =>
            RunNoSpecificActionTestAsync(actionTitle, input);

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
