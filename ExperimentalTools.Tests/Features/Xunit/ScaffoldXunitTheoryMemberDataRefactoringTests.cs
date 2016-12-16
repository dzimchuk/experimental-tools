using ExperimentalTools.Tests.Infrastructure.Refactoring;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using ExperimentalTools.Features.Xunit;
using ExperimentalTools.Services;
using Xunit;
using Microsoft.CodeAnalysis;
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
            new ScaffoldXunitTheoryMemberDataRefactoring(new SimpleNameGenerator(), new OptionsService());

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
        public TestMethod()
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
        public TestMethod(string param1, string param2)
        {
        }

        public static IEnumerable<object[]> TestMethodData => new[] { new object[] { ""value1"", ""value2"" } };
    }
}"
                }
            };
    }
}
