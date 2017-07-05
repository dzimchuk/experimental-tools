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
        
        [Theory, MemberData("HasActionTestData")]
        public Task HasActionTest(string test, string source, string fileName, string defaultNamespace, DiagnosticResult expected)
        {
            WorkspaceCache.Instance.AddOrUpdateProject(new ProjectDescription
            {
                Id = projectId,
                Path = @"c:\temp",
                AssemblyName = "TestProject",
                DefaultNamespace = defaultNamespace
            });

            return RunAsync(source, fileName, expected);
        }

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "Namespace does not match file path",
                    @"
namespace TestProject.Inner1.Inner2
{
    class TestService
    {
    }
}",
                    @"c:\temp\Inner1\TestService.cs",
                    null,
                    new DiagnosticResult
                    {
                        Id = DiagnosticCodes.NamespaceNormalizationAnalyzer,
                        Message = "Namespace 'TestProject.Inner1.Inner2' does not match file path or default namespace",
                        Severity = DiagnosticSeverity.Warning,
                        Locations =
                            new[] {
                                    new DiagnosticResultLocation(@"c:\temp\Inner1\TestService.cs", 2, 11)
                                }
                    }
                },
                new object[]
                {
                    "Namespace does not match file path and default namespace",
                    @"
namespace TestProject.Inner1.Inner2
{
    class TestService
    {
    }
}",
                    @"c:\temp\Inner1\TestService.cs",
                    "CustomNamespace",
                    new DiagnosticResult
                    {
                        Id = DiagnosticCodes.NamespaceNormalizationAnalyzer,
                        Message = "Namespace 'TestProject.Inner1.Inner2' does not match file path or default namespace",
                        Severity = DiagnosticSeverity.Warning,
                        Locations =
                            new[] {
                                    new DiagnosticResultLocation(@"c:\temp\Inner1\TestService.cs", 2, 11)
                                }
                    }
                },
                new object[]
                {
                    "Namespace does not match default namespace",
                    @"
namespace TestProject.Inner1
{
    class TestService
    {
    }
}",
                    @"c:\temp\Inner1\TestService.cs",
                    "CustomNamespace",
                    new DiagnosticResult
                    {
                        Id = DiagnosticCodes.NamespaceNormalizationAnalyzer,
                        Message = "Namespace 'TestProject.Inner1' does not match file path or default namespace",
                        Severity = DiagnosticSeverity.Warning,
                        Locations =
                            new[] {
                                    new DiagnosticResultLocation(@"c:\temp\Inner1\TestService.cs", 2, 11)
                                }
                    }
                }
            };

        [Theory, MemberData("NoActionTestData")]
        public Task NoActionTest(string test, string source, string fileName, string defaultNamespace)
        {
            WorkspaceCache.Instance.AddOrUpdateProject(new ProjectDescription
            {
                Id = projectId,
                Path = @"c:\temp",
                AssemblyName = "TestProject",
                DefaultNamespace = defaultNamespace
            });

            return RunAsync(source, fileName);
        }

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
                    @"c:\temp\TestService.cs",
                    null
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
                    @"c:\temp\Inner1\Inner2\TestService.cs",
                    null
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
                    @"c:\temp\TestService.designer.cs",
                    null
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
                    @"c:\temp\TestService.cs",
                    null
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
                    @"c:\temp\Test.cs",
                    null
                },
                new object[]
                {
                    "Incomplete file path",
                    @"
namespace TestProject
{
    class TestService
    {
    }
}",
                    @"TestService.cs",
                    null
                },

                new object[]
                {
                    "Namespace matches default namespace",
                    @"
namespace CustomNamespace
{
    class TestService
    {
    }
}",
                    @"c:\temp\TestService.cs",
                    "CustomNamespace"
                },
                new object[]
                {
                    "Namespace matches file path and default namespace",
                    @"
namespace CustomNamespace.Inner1.Inner2
{
    class TestService
    {
    }
}",
                    @"c:\temp\Inner1\Inner2\TestService.cs",
                    "CustomNamespace"
                },
            };
    }
}
