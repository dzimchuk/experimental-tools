using System.Composition;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using ExperimentalTools.Roslyn.Contracts;

namespace ExperimentalTools.Features.Braces
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AddBracesRefactoring)), Shared]
    internal class AddBracesRefactoring : CodeRefactoringProvider
    {
        private readonly IOptions options;
        private readonly List<IRefactoringStrategy> strategies = new List<IRefactoringStrategy>
        {
            new AddBracesInnerStatementStrategy(),
            new AddBracesParentStatementStrategy()
        };

        [ImportingConstructor]
        public AddBracesRefactoring(IOptions options)
        {
            this.options = options;
        }

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            if (!options.IsFeatureEnabled(FeatureIdentifiers.AddInitializedFieldRefactoring))
            {
                return;
            }

            foreach (var strategy in strategies)
            {
                await strategy.ComputeRefactoringAsync(context);
            }
        }
    }
}