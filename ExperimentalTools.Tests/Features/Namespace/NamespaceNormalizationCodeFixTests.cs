using ExperimentalTools.Tests.Infrastructure.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using ExperimentalTools.Roslyn.Features.Namespace;
using ExperimentalTools.Options;
using Xunit;
using ExperimentalTools.Workspace;
using Microsoft.CodeAnalysis;

namespace ExperimentalTools.Tests.Features.Namespace
{
    public class NamespaceNormalizationCodeFixTests : CodeFixTest
    {
        private static readonly ProjectId projectId = ProjectId.CreateNewId();

        protected override DiagnosticAnalyzer Analyzer => 
            new NamespaceNormalizationAnalyzer();

        protected override CodeFixProvider CodeFixProvider => 
            new NamespaceNormalizationCodeFix(new OptionsService());

        public NamespaceNormalizationCodeFixTests()
        {
            WorkspaceCache.Instance.AddOrUpdateProject(new ProjectDescription
            {
                Id = projectId,
                Path = @"c:\temp",
                AssemblyName = "TestProject"
            });
        }

        [Fact]
        public Task TypeRenameTest()
        {
            var source = @"
namespace HelloWorld
{
    class Test
    {
    }
}";

            var expected = @"
namespace TestProject.Inner
{
    class Test
    {
    }
}";

            return RunAsync(source, expected, sourcePath: @"c:\temp\Inner\Test.cs");
        }
    }
}
