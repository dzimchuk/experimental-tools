using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using ExperimentalTools.Localization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentalTools.Features.Xunit
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ScaffoldXunitTheoryInlineDataRefactoring)), Shared]
    internal class ScaffoldXunitTheoryInlineDataRefactoring : ScaffoldXunitTheoryRefactoring
    {
        [ImportingConstructor]
        public ScaffoldXunitTheoryInlineDataRefactoring(IOptions options) : base(options)
        {
        }

        protected override string FeatureIdentifier => FeatureIdentifiers.ScaffoldXunitTheoryInlineData;
        protected override string Title => Resources.ScaffoldXunitTheoryInlineData;
        protected override string TestDataAttribute => "Xunit.InlineDataAttribute";

        protected override bool CheckIfAlreadyScaffolded(SemanticModel model, AttributeSyntax testDataAttribute, CancellationToken cancellationToken)
        {
            var arguments = testDataAttribute.ArgumentList?.Arguments;
            if (arguments != null && arguments.Value.Any())
            {
                return true;
            }

            return false;
        }

        protected override Task<SyntaxNode> SetupTestDataAttributeAsync(Document document, AttributeListSyntax attrList, 
            AttributeSyntax testDataAttribute, CancellationToken cancellationToken, TypeDeclarationSyntax typeDeclaration, 
            MethodDeclarationSyntax methodDeclaration, SyntaxNode trackedRoot)
        {
            var newTestDataAttr = Attribute(IdentifierName("InlineData")).WithArgumentList(
                                  AttributeArgumentList(
                                      SeparatedList<AttributeArgumentSyntax>(
                                          new SyntaxNodeOrToken[]{
                                              AttributeArgument(
                                                  LiteralExpression(
                                                      SyntaxKind.StringLiteralExpression,
                                                      Literal("value1"))),
                                              Token(SyntaxKind.CommaToken),
                                              AttributeArgument(
                                                  LiteralExpression(
                                                      SyntaxKind.StringLiteralExpression,
                                                      Literal("value2")))})));

            if (testDataAttribute != null)
            {
                var trackedTestDataAttr = trackedRoot.GetCurrentNode(testDataAttribute);
                return Task.FromResult(trackedRoot.ReplaceNode(trackedTestDataAttr, newTestDataAttr));
            }
            else
            {
                var trackedAttrList = trackedRoot.GetCurrentNode(attrList);
                var newAttrList = AttributeList(SingletonSeparatedList(newTestDataAttr));

                return Task.FromResult(trackedRoot.InsertNodesAfter(trackedAttrList, SingletonList(newAttrList)));
            }
        }
    }
}
