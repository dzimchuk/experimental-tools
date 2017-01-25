using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.IO;

namespace ExperimentalTools.Roslyn
{
    public class GeneratedCodeRecognitionService
    {
        public bool IsGeneratedCode(Document document) => IsFileNameForGeneratedCode(document.Name);

        public bool IsGeneratedCode(SyntaxTreeAnalysisContext context) => IsGeneratedCode(context.Tree);

        public bool IsGeneratedCode(SyntaxNodeAnalysisContext context) => IsGeneratedCode(context.Node.SyntaxTree);

        public bool IsGeneratedCode(SymbolAnalysisContext context) => IsGeneratedCode(context.Symbol);

        private static bool IsGeneratedCode(ISymbol symbol)
        {
            foreach (var reference in symbol.DeclaringSyntaxReferences)
            {
                if (IsGeneratedCode(reference.SyntaxTree))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsGeneratedCode(SyntaxTree tree) => IsFileNameForGeneratedCode(tree.FilePath);

        private static bool IsFileNameForGeneratedCode(string fileName)
        {
            if (fileName == null || fileName.StartsWith("TemporaryGeneratedFile_", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string extension = Path.GetExtension(fileName);
            if (extension != string.Empty)
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);

                if (fileName.EndsWith(".designer", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".generated", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".g", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".g.i", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
