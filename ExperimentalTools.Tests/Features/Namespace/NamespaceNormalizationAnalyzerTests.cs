using ExperimentalTools.Roslyn.Features;
using ExperimentalTools.Roslyn.Features.Namespace;
using ExperimentalTools.Tests.Infrastructure.Diagnostics;
using ExperimentalTools.Workspace;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Features.Namespace
{
    public class NamespaceNormalizationAnalyzerTests : DiagnosticTest
    {
        private static readonly ProjectId projectId = ProjectId.CreateNewId();

        protected override DiagnosticAnalyzer Analyzer => 
            new NamespaceNormalizationAnalyzer();

        public NamespaceNormalizationAnalyzerTests()
        {
            WorkspaceCache.Instance.AddOrUpdateProject(new ProjectDescription
            {
                Id = projectId,
                Path = @"c:\temp",
                AssemblyName = "TestProject"
            });
        }

        [Theory, MemberData("HasActionTestData")]
        public Task HasActionTest(string test, string source, string fileName, DiagnosticResult expected) =>
            RunAsync(source, fileName, expected);

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "Namespace matches file path",
                    @"
namespace TestProject.Inner1.Inner2
{
    class TestService
    {
    }
}",
                    @"c:\temp\Inner1\TestService.cs",
                    new DiagnosticResult
                    {
                        Id = DiagnosticCodes.NamespaceNormalizationAnalyzer,
                        Message = "Namespace 'TestProject.Inner1.Inner2' does not match file path",
                        Severity = DiagnosticSeverity.Warning,
                        Locations =
                            new[] {
                                    new DiagnosticResultLocation(@"c:\temp\Inner1\TestService.cs", 2, 11)
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
                    "Namespace matches assembly name",
                    @"
namespace TestProject
{
    class TestService
    {
    }
}",
                    @"c:\temp\TestService.cs"
                },
                new object[]
                {
                    "Namespace matches file path",
                    @"
namespace TestProject.Inner1.Inner2
{
    class TestService
    {
    }
}",
                    @"c:\temp\Inner1\Inner2\TestService.cs"
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
namespace TestProject
{
    namespace Nested
    {
        class MyService {}
    }
}",
                    @"c:\temp\TestService.cs"
                },
                new object[]
                {
                    "More than one namespace in a single document",
                    @"
namespace HelloWorld
{
}

namespace TestProject
{
}
",
                    @"c:\temp\Test.cs"
                }
            };
    }
}
