using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    }
}
