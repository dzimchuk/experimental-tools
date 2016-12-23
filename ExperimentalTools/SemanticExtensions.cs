using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Threading;

namespace ExperimentalTools
{
    internal static class SemanticExtensions
    {
        public static bool IsConstant(this SemanticModel model, VariableDeclaratorSyntax variableDeclarator,
            CancellationToken cancellationToken)
        {
            var symbol = model.GetDeclaredSymbol(variableDeclarator, cancellationToken);
            if (symbol is IFieldSymbol)
            {
                return ((IFieldSymbol)symbol).IsConst;
            }
            else if (symbol is ILocalSymbol)
            {
                return ((ILocalSymbol)symbol).IsConst;
            }

            return false;
        }

        public static ISet<INamespaceSymbol> GetUsingNamespacesInScope(this SemanticModel model, SyntaxNode location)
        {
            var result = new HashSet<INamespaceSymbol>();

            foreach (var @using in location.GetEnclosingUsingDirectives())
            {
                if (@using.Alias == null)
                {
                    var symbolInfo = model.GetSymbolInfo(@using.Name);
                    if (symbolInfo.Symbol != null && symbolInfo.Symbol.Kind == SymbolKind.Namespace)
                    {
                        result.Add((INamespaceSymbol)symbolInfo.Symbol);
                    }
                }
            }

            return result;
        }
    }
}
