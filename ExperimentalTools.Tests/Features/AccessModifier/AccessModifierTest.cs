using ExperimentalTools.Features.AccessModifier;
using ExperimentalTools.Features.AccessModifier.Recipes;
using ExperimentalTools.Services;
using ExperimentalTools.Tests.Infrastructure.Refactoring;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Xunit.Abstractions;

namespace ExperimentalTools.Tests.Features.AccessModifier
{
    public class AccessModifierTest : RefactoringTest
    {
        public AccessModifierTest(ITestOutputHelper output)
            : base(output)
        {
        }

        protected override CodeRefactoringProvider Provider =>
            new ChangeAccessModifierRefactoring(new ITypeRecipe[]
            {
                                            new TopLevelTypeRecipe(),
                                            new NestedInClassRecipe(),
                                            new NestedInStructRecipe()
            }, new OptionsService());
    }
}
