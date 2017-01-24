using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Threading.Tasks;

namespace ExperimentalTools.Roslyn.Contracts
{
    public interface IRefactoringStrategy
    {
        Task ComputeRefactoringAsync(CodeRefactoringContext context);
    }
}
