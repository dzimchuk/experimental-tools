using ExperimentalTools.Roslyn.Refactoring;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Composition;
using System.Threading.Tasks;

namespace ExperimentalTools.Roslyn.Features.ReadOnly
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(FieldCanBeMadeReadOnlyRefactoring)), Shared]
    internal class FieldCanBeMadeReadOnlyRefactoring : CodeRefactoringProvider
    {
        private readonly IOptions options;
        private readonly CodeRefactoring refactoring = new CodeRefactoring(new FieldCanBeMadeReadOnlyStrategy());

        [ImportingConstructor]
        public FieldCanBeMadeReadOnlyRefactoring(IOptions options)
        {
            this.options = options;
        }

        public override Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            if (!options.IsFeatureEnabled(FeatureIdentifiers.FieldCanBeMadeReadOnly))
            {
                return Task.FromResult(0);
            }
            
            return refactoring.ComputeRefactoringsAsync(context);
        }
    }
}
