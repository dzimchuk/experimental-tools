using System.Composition;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using ExperimentalTools.Roslyn.Refactoring;
using ExperimentalTools.Features.Braces.Strategies;

namespace ExperimentalTools.Features.Braces
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AddBracesRefactoring)), Shared]
    internal class AddBracesRefactoring : CodeRefactoringProvider
    {
        private readonly IOptions options;
        private readonly List<ICodeRefactoringStrategy> strategies = new List<ICodeRefactoringStrategy>
        {
            new AddBracesInnerStatementStrategy(),
            new AddBracesElseClauseInnerStatementStrategy(),
            new AddBracesElseClauseStrategy(),
            new AddBracesParentStatementStrategy()
        };

        [ImportingConstructor]
        public AddBracesRefactoring(IOptions options)
        {
            this.options = options;
        }

        public sealed override Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            if (!options.IsFeatureEnabled(FeatureIdentifiers.AddInitializedFieldRefactoring))
            {
                return Task.FromResult(0);
            }

            var refactoring = new CodeRefactoring(strategies);
            return refactoring.ComputeRefactoringsAsync(context);
        }
    }
}