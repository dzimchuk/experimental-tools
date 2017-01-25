using ExperimentalTools.Options;
using ExperimentalTools.Roslyn.Features.TypeDeclaration;
using ExperimentalTools.Tests.Infrastructure;
using ExperimentalTools.Tests.Infrastructure.Diagnostics;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Features.TypeDeclaration
{
    public class TypeAndDocumentNameCodeFixProviderTests : CodeFixTest
    {
        protected override DiagnosticAnalyzer Analyzer => 
            new TypeAndDocumentNameAnalyzer();

        protected override CodeFixProvider CodeFixProvider =>
            new TypeAndDocumentNameCodeFixProvider(new OptionsService());

        [Fact]
        public Task TypeRenameTest()
        {
            var source = @"
namespace HelloWorld
{
    class TestService
    {
    }
}";

            var expected = @"
namespace HelloWorld
{
    class Test
    {
    }
}";

            return RunAsync(source, expected, sourcePath: @"c:\temp\Test.cs", codeFixIndex: 0);
        }

        [Fact]
        public Task TypeDocumentTest()
        {
            var source = @"
namespace HelloWorld
{
    class TestService
    {
    }
}";

            var expected = @"
namespace HelloWorld
{
    class TestService
    {
    }
}";

            return RunAsync(source, expected, sourcePath: @"c:\temp\Test.cs", expectedPath: @"c:\temp\TestService.cs", codeFixIndex: 1);
        }

        [Fact]
        public async Task InvalidDocumentNameForTypeNameTest()
        {
            var source = @"
namespace HelloWorld
{
    class TestService
    {
    }
}";

            var document = DocumentProvider.GetDocument(source, @"c:\temp\Test.Part.cs");
            var analyzerDiagnostics = await DiagnosticRunner.GetSortedDiagnosticsFromDocumentsAsync(Analyzer, new[] { document });

            Assert.NotEmpty(analyzerDiagnostics);

            var actions = new List<CodeAction>();
            var context = new CodeFixContext(document, analyzerDiagnostics[0], (a, d) => actions.Add(a), CancellationToken.None);
            await CodeFixProvider.RegisterCodeFixesAsync(context);

            Assert.Equal(1, actions.Count);
            Assert.Equal("Rename 'Test.Part.cs' to 'TestService.cs'", actions[0].Title);
        }
    }
}
