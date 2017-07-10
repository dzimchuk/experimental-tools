using ExperimentalTools.Options;
using ExperimentalTools.Roslyn.Features.ReadOnly;
using ExperimentalTools.Tests.Infrastructure.Refactoring;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ExperimentalTools.Tests.Features.ReadOnly
{
    public class FieldCanBeMadeReadOnlyRefactoringTests : RefactoringTest
    {
        public FieldCanBeMadeReadOnlyRefactoringTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override CodeRefactoringProvider Provider => new FieldCanBeMadeReadOnlyRefactoring(new OptionsService());
        
        [Theory, MemberData("HasActionTestData")]
        public Task HasActionTest(string test, string input, string expectedOutput) =>
            RunSingleActionTestAsync(input, expectedOutput);

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "Field is assigned in constructor",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private int @::@index;

        public TestService()
        {
            index = 1;
        }
    }
}",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private readonly int index;

        public TestService()
        {
            index = 1;
        }
    }
}"
                },
                new object[]
                {
                    "Cursor placement test (after semicolon)",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private int index;@::@

        public TestService()
        {
            index = 1;
        }
    }
}",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private readonly int index;

        public TestService()
        {
            index = 1;
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
                    "Field is a constant",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private const int @::@index = 1;
    }
}"
                },
                new object[]
                {
                    "Field is alredy readonly",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private readonly int @::@index;

        public TestService()
        {
            index = 1;
        }
    }
}"
                },
                new object[]
                {
                    "Field is volatile",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private volatile string @::@value;
    }
}"
                },
                new object[]
                {
                    "Field is never assigned",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private int @::@index;
    }
}"
                }
            };
    }
}
