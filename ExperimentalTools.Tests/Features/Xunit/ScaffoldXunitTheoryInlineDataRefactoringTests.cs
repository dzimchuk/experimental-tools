using ExperimentalTools.Features.Xunit;
using ExperimentalTools.Services;
using ExperimentalTools.Tests.Infrastructure.Refactoring;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ExperimentalTools.Tests.Features.Xunit
{
    public class ScaffoldXunitTheoryInlineDataRefactoringTests : RefactoringTest
    {
        public ScaffoldXunitTheoryInlineDataRefactoringTests(ITestOutputHelper output)
            : base(output)
        {
        }

        protected override CodeRefactoringProvider Provider =>
            new ScaffoldXunitTheoryInlineDataRefactoring(new OptionsService());

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
                    "No InlineData",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [The@::@ory]
        public TestMethod()
        {
        }
    }
}",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory]
        [InlineData(""value1"", ""value2"")]
        public TestMethod(string param1, string param2)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Empty InlineData",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData]@::@
        public TestMethod()
        {
        }
    }
}",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(""value1"", ""value2"")]
        public TestMethod(string param1, string param2)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Empty InlineData - Separate Attribute List",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory]@::@
        [InlineData]
        public TestMethod()
        {
        }
    }
}",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory]
        [InlineData(""value1"", ""value2"")]
        public TestMethod(string param1, string param2)
        {
        }
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
        [InlineData]@::@
        public TestMethod()
        {
        }
    }
}",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(""value1"", ""value2"")]
        public TestMethod(string param1, string param2)
        {
        }
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
        public TestMethod()
        {
        }
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
        [Theory, InlineData]@::@
        public TestMethod(string param1, int param2)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Theory already scaffolded - inline data not empty",
                    @"
using Xunit;

namespace HelloWorld
{
    class TestService
    {
        [Theory, InlineData(""value1"")]@::@
        public TestMethod()
        {
        }
    }
}"
                }
            };
    }
}
