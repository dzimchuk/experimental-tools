using ExperimentalTools.Roslyn.Features;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ExperimentalTools.Tests.Features.AccessModifier
{
    public class TopLevelInterfaceTests : AccessModifierTest
    {
        public TopLevelInterfaceTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Theory, MemberData(nameof(TestData))]
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
    @::@interface Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    internal interface Test
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
    @::@interface Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    public interface Test
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
    @::@public interface Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    internal interface Test
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
    internal @::@interface Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    public interface Test
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
    private @::@public interface Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    internal interface Test
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
    private @::@public interface Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    public interface Test
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
    @::@public interface Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    interface Test
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
    @::@internal interface Test
    {
    }
}",
                    @"
namespace HelloWorld
{
    interface Test
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
    internal protected interface Test@::@
    {
    }
}",
                    @"
namespace HelloWorld
{
    interface Test
    {
    }
}"
                }
            };
    }
}
