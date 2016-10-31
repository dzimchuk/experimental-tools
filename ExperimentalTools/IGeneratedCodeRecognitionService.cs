using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExperimentalTools
{
    public interface IGeneratedCodeRecognitionService
    {
        bool IsGeneratedCode(Document document);
        bool IsGeneratedCode(SyntaxNodeAnalysisContext context);
        bool IsGeneratedCode(SymbolAnalysisContext context);
    }
}
