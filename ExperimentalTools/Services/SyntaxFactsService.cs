using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Composition;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExperimentalTools.Services
{
    [Export(typeof(ISyntaxFactsService))]
    internal class SyntaxFactsService : ISyntaxFactsService
    {
        public bool IsIdentifier(SyntaxToken token)
        {
            return token.IsKind(SyntaxKind.IdentifierToken);
        }

        public bool IsValidIdentifier(string identifier)
        {
            var token = ParseToken(identifier);
            return IsIdentifier(token) && !token.ContainsDiagnostics && token.ToString().Length == identifier.Length;
        }
    }
}
