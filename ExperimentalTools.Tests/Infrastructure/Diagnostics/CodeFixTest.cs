using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;

namespace ExperimentalTools.Tests.Infrastructure.Diagnostics
{
    public abstract class CodeFixTest
    {
        public CodeFixTest()
        {
            OptionsBucket.Instance.EnableAllFeatures();
        }

        protected abstract DiagnosticAnalyzer Analyzer { get; }
        protected abstract CodeFixProvider CodeFixProvider { get; }
        
        protected Task RunAsync(string source, string expected, string sourcePath = null, string expectedPath = null, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false) =>
            CodeFixVerifier.VerifyFixAsync(Analyzer, CodeFixProvider, source, expected, sourcePath, expectedPath, codeFixIndex, allowNewCompilerDiagnostics);
    }
}
