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

        [Theory, MemberData("HasActionTestData")]
        public async Task CodeFixTest(string test, string source, string expected)
        {
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

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "No constructor",
                    @"
namespace HelloWorld
{
    class TestService
    {
        public Test()
        {
        }
    }
}",
                    @"
namespace HelloWorld
{
    class TestService
    {
        public TestService()
        {
        }
    }
}"
                },
                new object[]
                {
                    "Constructor with a different number of parameters exists",
                    @"
namespace HelloWorld
{
    class TestService
    {
        public TestService(int index)
        {
        }

        public Test(int i, double b)
        {
        }
    }
}",
                    @"
namespace HelloWorld
{
    class TestService
    {
        public TestService(int index)
        {
        }

        public TestService(int i, double b)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Constructor with different parameters exists",
                    @"
namespace HelloWorld
{
    class TestService
    {
        public TestService(int index, string name)
        {
        }

        public Test(int i, double b)
        {
        }
    }
}",
                    @"
namespace HelloWorld
{
    class TestService
    {
        public TestService(int index, string name)
        {
        }

        public TestService(int i, double b)
        {
        }
    }
}"
                }
            };

        [Theory, MemberData("NoActionTestData")]
        public async Task NoActionTest(string test, string source)
        {
            var document = DocumentProvider.GetDocument(source);
            var diagnostic = await GetDiagnosticAsync(document);

            var actions = new List<CodeAction>();
            var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);
            await codeFixProvider.RegisterCodeFixesAsync(context);

            Assert.Empty(actions);
        }

        public static IEnumerable<object[]> NoActionTestData =>
            new[]
            {
                new object[]
                {
                    "Constructor already exists",
                    @"
namespace HelloWorld
{
    class TestService
    {
        public TestService()
        {
        }

        public Test()
        {
        }
    }
}"
                },
                new object[]
                {
                    "Constructor with the same parameters already exists",
                    @"
namespace HelloWorld
{
    class TestService
    {
        public TestService(int index, double value)
        {
        }

        public Test(int i, double b)
        {
        }
    }
}"
                }
            };
    }
}
