using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System.Threading;
using System.Threading.Tasks;

namespace ExperimentalTools.Roslyn.Refactoring
{
    public interface ICodeRefactoringStrategy
    {
        Task<CodeAction> CalculateActionAsync(Document document, SyntaxNode root, SyntaxNode selectedNode, CancellationToken cancellationToken);
    }
}
