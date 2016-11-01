using ExperimentalTools.Tests.Infrastructure.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using ExperimentalTools.Features.TypeDeclaration;
using ExperimentalTools.Services;
using Xunit;

namespace ExperimentalTools.Tests.Features.TypeDeclaration
{
    public class TypeAndDocumentNameAnalyzerTests : DiagnosticTest
    {
        protected override DiagnosticAnalyzer Analyzer => 
            new TypeAndDocumentNameAnalyzer(new GeneratedCodeRecognitionService());

        [Theory, MemberData("NoActionTestData")]
        public Task NoActionTest(string test, string source, string fileName) =>
            RunAsync(source, fileName);

        public static IEnumerable<object[]> NoActionTestData =>
            new[]
            {
                new object[]
                {
                    "Type name matches file name",
                    @"
namespace HelloWorld
{
    class TestService
    {
    }
}",
                    "TestService.cs"
                },
                new object[]
                {
                    "Auto-generated",
                    @"
namespace HelloWorld
{
    class TestService
    {
    }
}",
                    "TestService.designer.cs"
                },
                new object[]
                {
                    "Nested",
                    @"
namespace HelloWorld
{
    class TestService
    {
        class Nested {}
    }
}",
                    "TestService.cs"
                },
                new object[]
                {
                    "More than one types in a single document",
                    @"
namespace HelloWorld
{
    class TestService
    {
    }

    class AnotherService
    {
    }
}",
                    "Test.cs"
                }
            };

        [Fact]
        public Task NoActionWhenPartialTest()
        {
            var sources = new[]
            {
                @"
namespace HelloWorld
{
    partial class TestService
    {
    }
}",
                @"
namespace HelloWorld
{
    partial class TestService
    {
    }
}"
            };

            return RunAsync(sources, new[] { "Test1.cs", "Test2.cs" });
        }
    }
}
