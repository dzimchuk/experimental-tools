using ExperimentalTools.Tests.Infrastructure.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using ExperimentalTools.Roslyn.Features.ReadOnly;
using Xunit;
using ExperimentalTools.Roslyn.Features;
using Microsoft.CodeAnalysis;

namespace ExperimentalTools.Tests.Features.ReadOnly
{
    public class FieldCanBeMadeReadOnlyAnalyzerTests : DiagnosticTest
    {
        protected override DiagnosticAnalyzer Analyzer => new FieldCanBeMadeReadOnlyAnalyzer();

        [Theory, MemberData("HasActionTestData")]
        public Task HasActionTest(string test, string source, DiagnosticResult expected) =>
            RunAsync(source, expected);

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
        private int index;

        public TestService()
        {
            index = 1;
        }
    }
}",
                    new DiagnosticResult
                    {
                        Id = DiagnosticCodes.FieldCanBeMadeReadOnlyAnalyzer,
                        Message = "Field 'index' can be made readonly",
                        Severity = DiagnosticSeverity.Info,
                        Locations =
                            new[] {
                                    new DiagnosticResultLocation(@"Test.cs", 6, 21)
                                }
                    }
                }
            };

        //[Theory, MemberData("NoActionTestData")]
        public Task NoActionTest(string test, string source, string fileName = null) =>
            string.IsNullOrEmpty(fileName) ? RunAsync(source) : RunAsync(source, fileName);

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
        private const int index = 1;
    }
}"
                },
                new object[]
                {
                    "Auto-generated",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private int index;

        public TestService()
        {
            index = 1;
        }
    }
}",
                    @"c:\temp\TestService.designer.cs"
                },
                new object[]
                {
                    "Field is alredy readonly",
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
                    "Field is volatile",
                    @"
namespace HelloWorld
{
    class TestService
    {
        private volatile string value;
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
        private int index;
    }
}"
                }
            };
    }
}
