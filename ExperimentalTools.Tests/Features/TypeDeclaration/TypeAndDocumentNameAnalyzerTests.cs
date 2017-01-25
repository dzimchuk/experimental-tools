using ExperimentalTools.Roslyn.Features;
using ExperimentalTools.Roslyn.Features.TypeDeclaration;
using ExperimentalTools.Tests.Infrastructure.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Features.TypeDeclaration
{
    public class TypeAndDocumentNameAnalyzerTests : DiagnosticTest
    {
        protected override DiagnosticAnalyzer Analyzer => 
            new TypeAndDocumentNameAnalyzer();

        [Theory, MemberData("HasActionTestData")]
        public Task HasActionTest(string test, string source, string fileName, DiagnosticResult expected) =>
            RunAsync(source, fileName, expected);

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "Type name does not match file name",
                    @"
namespace HelloWorld
{
    class TestService
    {
    }
}",
                    @"c:\temp\Test.cs",
                    new DiagnosticResult
                    {
                        Id = DiagnosticCodes.TypeAndDocumentNameAnalyzer,
                        Message = "Type name 'TestService' does not match file name",
                        Severity = DiagnosticSeverity.Warning,
                        Locations =
                            new[] {
                                    new DiagnosticResultLocation(@"c:\temp\Test.cs", 4, 11)
                                }
                    }
                }
            };

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
                    @"c:\temp\TestService.cs"
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
                    @"c:\temp\TestService.designer.cs"
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
                    @"c:\temp\TestService.cs"
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
                    @"c:\temp\Test.cs"
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

            return RunAsync(sources, new[] { @"c:\temp\Test1.cs", @"c:\temp\Test2.cs" });
        }
    }
}
