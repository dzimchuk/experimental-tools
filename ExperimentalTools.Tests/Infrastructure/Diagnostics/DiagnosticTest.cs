using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;

namespace ExperimentalTools.Tests.Infrastructure.Diagnostics
{
    internal abstract class DiagnosticTest
    {
        protected abstract DiagnosticAnalyzer Analyzer { get; }

        protected Task RunAsync(string source, params DiagnosticResult[] expected) => 
            RunAsync(new[] { source }, expected);

        protected async Task RunAsync(string[] sources, params DiagnosticResult[] expected)
        {
            var analyzer = Analyzer;
            var diagnostics = await DiagnosticRunner.GetSortedDiagnosticsAsync(sources, analyzer);
            DiagnosticVerifier.VerifyDiagnosticResults(diagnostics, analyzer, expected);
        }
    }
}
