using Microsoft.CodeAnalysis;

namespace ExperimentalTools
{
    internal interface ISyntaxFactsService
    {
        bool IsIdentifier(SyntaxToken token);
        bool IsValidIdentifier(string identifier);
    }
}
