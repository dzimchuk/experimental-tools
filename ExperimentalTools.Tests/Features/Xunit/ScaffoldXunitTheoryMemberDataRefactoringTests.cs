using ExperimentalTools.Options;
using ExperimentalTools.Roslyn.Features.Xunit;
using ExperimentalTools.Tests.Infrastructure.Refactoring;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ExperimentalTools.Tests.Features.Xunit
{
    public class ScaffoldXunitTheoryMemberDataRefactoringTests : RefactoringTest
    {
        public ScaffoldXunitTheoryMemberDataRefactoringTests(ITestOutputHelper output)
            : base(output)
        {
        }

        protected override CodeRefactoringProvider Provider => 
            new ScaffoldXunitTheoryMemberDataRefactoring(new OptionsService());

        protected override IEnumerable<MetadataReference> AdditionalReferences => 
            new[] { MetadataReference.CreateFromFile(typeof(FactAttribute).Assembly.Location) };

        [Theory, MemberData("HasActionTestData")]
        public Task HasActionTest(string test, string input, string expectedOutput) =>
            RunSingleActionTestAsync(input, expectedOutput);

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "No MemberData",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [The@::@ory]
        public void TestMethod()
        {
        }
    }
}",
                    @"using System.Collections.Generic;
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, MemberData(""TestMethodData"")]
        public void TestMethod(string param1, string param2)
        {
        }

        public static IEnumerable<object[]> TestMethodData => new[] { new object[] { ""value1"", ""value2"" } };
    }
}"
                },
                new object[]
                {
                    "Empty MemberData",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, MemberData]@::@
        public void TestMethod()
        {
        }
    }
}",
                    @"using System.Collections.Generic;
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, MemberData(""TestMethodData"")]
        public void TestMethod(string param1, string param2)
        {
        }

        public static IEnumerable<object[]> TestMethodData => new[] { new object[] { ""value1"", ""value2"" } };
    }
}"
                },
                new object[]
                {
                    "Specified member name",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, MemberData(""TestData"")]@::@
        public void TestMethod()
        {
        }
    }
}",
                    @"using System.Collections.Generic;
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, MemberData(""TestData"")]
        public void TestMethod(string param1, string param2)
        {
        }

        public static IEnumerable<object[]> TestData => new[] { new object[] { ""value1"", ""value2"" } };
    }
}"
                },
                new object[]
                {
                    "Empty MemberData - Separate Attribute List",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory]@::@
        [MemberData]
        public void TestMethod()
        {
        }
    }
}",
                    @"using System.Collections.Generic;
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory]
        [MemberData(""TestMethodData"")]
        public void TestMethod(string param1, string param2)
        {
        }

        public static IEnumerable<object[]> TestMethodData => new[] { new object[] { ""value1"", ""value2"" } };
    }
}"
                },
                new object[]
                {
                    "Specified member name - Separate Attribute List",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory]
        [MemberData(""TestData"")]@::@
        public void TestMethod()
        {
        }
    }
}",
                    @"using System.Collections.Generic;
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory]
        [MemberData(""TestData"")]
        public void TestMethod(string param1, string param2)
        {
        }

        public static IEnumerable<object[]> TestData => new[] { new object[] { ""value1"", ""value2"" } };
    }
}"
                },
                new object[]
                {
                    "Missing Theory",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [MemberData]@::@
        public void TestMethod()
        {
        }
    }
}",
                    @"using System.Collections.Generic;
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, MemberData(""TestMethodData"")]
        public void TestMethod(string param1, string param2)
        {
        }

        public static IEnumerable<object[]> TestMethodData => new[] { new object[] { ""value1"", ""value2"" } };
    }
}"
                }
            };

        [Theory, MemberData("NoActionTestData")]
        public Task NoActionTest(string test, string input) =>
            RunNoActionTestAsync(input);

        public static IEnumerable<object[]> NoActionTestData =>
            new[]
            {
                new object[]
                {
                    "Theory attribute not classified",
                    @"
namespace HelloWorld
{
    class TestService
    {
        [The@::@ory]
        public void TestMethod()
        {
        }
    }
}"
                },
                new object[]
                {
                    "Theory already scaffolded - specified test data exists",
                    @"using System.Collections.Generic;
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, MemberData(""TestMethodData"")]@::@
        public void TestMethod()
        {
        }

        public static IEnumerable<object[]> TestMethodData => new[] { new object[] { ""value1"", ""value2"" } };
    }
}"
                },
                new object[]
                {
                    "Theory already scaffolded - method parameters exist",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory]@::@
        public void TestMethod(string param1, string param2)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Theory already scaffolded - alternate test data attribute",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData]@::@
        public void TestMethod()
        {
        }
    }
}"
                }
            };
    }
}
