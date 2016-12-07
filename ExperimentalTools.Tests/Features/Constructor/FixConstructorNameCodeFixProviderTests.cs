using ExperimentalTools.Features.Constructor;
using ExperimentalTools.Services;
using ExperimentalTools.Tests.Infrastructure;
using ExperimentalTools.Tests.Infrastructure.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Features.Constructor
{
    public class FixConstructorNameCodeFixProviderTests
    {
        private readonly CodeFixProvider codeFixProvider = new FixConstructorNameCodeFixProvider(new OptionsService());

        private static async Task<Diagnostic> GetDiagnosticAsync(Document document)
        {
            var diagnostics = await CodeFixVerifier.GetCompilerDiagnosticsAsync(document);
            Assert.NotEmpty(diagnostics);

            var methodNameDiagnostic = diagnostics.FirstOrDefault(d => d.Id == "CS1520");
            Assert.NotNull(methodNameDiagnostic);

            return methodNameDiagnostic;
        }

        [Fact]
        public async Task CodeFixTest()
        {
            var source = @"
namespace HelloWorld
{
    class TestService
    {
        public Test()
        {
        }
    }
}";
            var expected = @"
namespace HelloWorld
{
    class TestService
    {
        public TestService()
        {
        }
    }
}";
            var document = DocumentProvider.GetDocument(source);
            var diagnostic = await GetDiagnosticAsync(document);

            var actions = new List<CodeAction>();
            var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
            await codeFixProvider.RegisterCodeFixesAsync(context);

            Assert.Equal(1, actions.Count);

            document = await CodeFixVerifier.ApplyFixAsync(document, actions[0]);
            var actual = await CodeFixVerifier.GetStringFromDocumentAsync(document);

            Assert.Equal(expected, actual);
        }
    }
}
