using System.Composition;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using ExperimentalTools.Roslyn.Refactoring;
using ExperimentalTools.Roslyn.Features.Braces.Strategies;

namespace ExperimentalTools.Roslyn.Features.Braces
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(RemoveBracesRefactoring)), Shared]
    internal class RemoveBracesRefactoring : CodeRefactoringProvider
    {
        private readonly IOptions options;
        private readonly List<ICodeRefactoringStrategy> strategies = new List<ICodeRefactoringStrategy>
        {
            new RemoveBracesInnerStatementStrategy(),
            new RemoveBracesBlockStrategy(),
            new RemoveBracesParentStatementStrategy(),
            new RemoveBracesElseClauseInnerStatementStrategy(),
            new RemoveBracesElseClauseBlockStrategy(),
            new RemoveBracesElseClauseStrategy()
        };

        [ImportingConstructor]
        public RemoveBracesRefactoring(IOptions options)
        {
            this.options = options;
        }

        public sealed override Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            if (!options.IsFeatureEnabled(FeatureIdentifiers.RemoveBraces))
            {
                return Task.FromResult(0);
            }

            var refactoring = new CodeRefactoring(strategies);
            return refactoring.ComputeRefactoringsAsync(context);
        }
    }
}