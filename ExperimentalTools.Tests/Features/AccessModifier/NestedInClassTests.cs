using ExperimentalTools.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ExperimentalTools.Tests.Features.AccessModifier
{
    public class NestedInClassTests : AccessModifierTest
    {
        public NestedInClassTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Theory, MemberData("HasActionTestData")]
        public Task HasActionTest(string test, string actionTitle, string input, string expectedResult) =>
            RunMultipleActionsTestAsync(actionTitle, input, expectedResult);

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
    class Test
    {
        @::@class Nested
        {
        }
    }
}",
                    @"
namespace HelloWorld
{
    class Test
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
    class Test
    {
        @::@public class Nested
        {
        }
    }
}",
                    @"
namespace HelloWorld
{
    class Test
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
    class Test
    {
        @::@class Nested
        {
        }
    }
}",
                    @"
namespace HelloWorld
{
    class Test
    {
        public class Nested
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
    class Test
    {
        @::@class Nested
        {
        }
    }
}",
                    @"
namespace HelloWorld
{
    class Test
    {
        protected class Nested
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
    class Test
    {
        @::@class Nested
        {
        }
    }
}",
                    @"
namespace HelloWorld
{
    class Test
    {
        protected internal class Nested
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
    class Test
    {
        @::@class Nested
        {
        }
    }
}",
                    @"
namespace HelloWorld
{
    class Test
    {
        internal class Nested
        {
        }
    }
}"
                }
            };

        [Theory, MemberData("NoActionTestData")]
        public Task NoActionTest(string test, string actionTitle, string input) =>
            RunNoSpecificActionTestAsync(actionTitle, input);

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
    class Test
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
    class Test
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
    class Test
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
    class Test
    {
        @::@protected class Nested
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
    class Test
    {
        @::@protected internal class Nested
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
    class Test
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
