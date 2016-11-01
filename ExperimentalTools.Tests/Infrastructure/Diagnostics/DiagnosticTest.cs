using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;

namespace ExperimentalTools.Tests.Infrastructure.Diagnostics
{
    public abstract class DiagnosticTest
    {
        protected abstract DiagnosticAnalyzer Analyzer { get; }

        protected Task RunAsync(string source, params DiagnosticResult[] expected) => 
            RunAsync(new[] { source }, expected);

        protected Task RunAsync(string source, string fileName, params DiagnosticResult[] expected) =>
            RunAsync(new[] { source }, new[] { fileName }, expected);

        protected Task RunAsync(string[] sources, params DiagnosticResult[] expected) => 
            RunAsync(sources, null, expected);

        protected async Task RunAsync(string[] sources, string[] fileNames, params DiagnosticResult[] expected)
        {
            var analyzer = Analyzer;
            var diagnostics = await DiagnosticRunner.GetSortedDiagnosticsAsync(sources, fileNames, analyzer);
            DiagnosticVerifier.VerifyDiagnosticResults(diagnostics, analyzer, expected);
        }
    }
}
