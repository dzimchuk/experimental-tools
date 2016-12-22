using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using ExperimentalTools.Localization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Simplification;

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
        protected override string AlternateTestDataAttribute => "Xunit.MemberDataAttribute";

        protected override bool CheckIfAlreadyScaffolded(SemanticModel model, AttributeSyntax testDataAttribute, CancellationToken cancellationToken)
        {
            var arguments = testDataAttribute.ArgumentList?.Arguments;
            if (arguments != null && arguments.Value.Any())
            {
                foreach (var argument in arguments)
                {
                    if (argument.Expression == null || 
                        CreateTypeSyntax(model, argument.Expression, cancellationToken) == null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static TypeSyntax CreateTypeSyntax(SemanticModel model, ExpressionSyntax expression, CancellationToken cancellationToken)
        {
            if (expression.Kind() == SyntaxKind.NullLiteralExpression)
            {
                return PredefinedType(Token(SyntaxKind.StringKeyword));
            }

            var typeSymbol = model.GetTypeInfo(expression, cancellationToken).Type;
            if (typeSymbol == null || string.IsNullOrWhiteSpace(typeSymbol.OriginalDefinition.ToString()))
            {
                return null;
            }

            var isArray = false;
            var definition = typeSymbol.OriginalDefinition.ToString();
            if (definition.Length > 2 && definition.EndsWith("[]"))
            {
                isArray = true;
                definition = definition.Substring(0, definition.Length - 2);
            }

            TypeSyntax syntax = null;

            switch (definition)
            {
                case "int":
                    syntax = PredefinedType(Token(SyntaxKind.IntKeyword));
                    break;
                case "long":
                    syntax = PredefinedType(Token(SyntaxKind.LongKeyword));
                    break;
                case "short":
                    syntax = PredefinedType(Token(SyntaxKind.ShortKeyword));
                    break;
                case "uint":
                    syntax = PredefinedType(Token(SyntaxKind.UIntKeyword));
                    break;
                case "ulong":
                    syntax = PredefinedType(Token(SyntaxKind.ULongKeyword));
                    break;
                case "ushort":
                    syntax = PredefinedType(Token(SyntaxKind.UShortKeyword));
                    break;
                case "double":
                    syntax = PredefinedType(Token(SyntaxKind.DoubleKeyword));
                    break;
                case "float":
                    syntax = PredefinedType(Token(SyntaxKind.FloatKeyword));
                    break;
                case "decimal":
                    syntax = PredefinedType(Token(SyntaxKind.DecimalKeyword));
                    break;
                case "string":
                    syntax = PredefinedType(Token(SyntaxKind.StringKeyword));
                    break;
                case "bool":
                    syntax = PredefinedType(Token(SyntaxKind.BoolKeyword));
                    break;
                case "System.Type":
                    syntax = QualifiedName(IdentifierName("System"), IdentifierName("Type")).WithAdditionalAnnotations(Simplifier.Annotation);
                    break;
            }

            if (syntax != null && isArray)
            {
                syntax = ArrayType(syntax).WithRankSpecifiers(
                                SingletonList<ArrayRankSpecifierSyntax>(
                                    ArrayRankSpecifier(
                                        SingletonSeparatedList<ExpressionSyntax>(
                                            OmittedArraySizeExpression()))));
            }

            return syntax;
        }
        
        protected override Task<SyntaxNode> SetupTestDataAttributeAsync(Document document, AttributeListSyntax attrList, 
            AttributeSyntax testDataAttribute, CancellationToken cancellationToken, TypeDeclarationSyntax typeDeclaration, 
            MethodDeclarationSyntax methodDeclaration, SyntaxNode trackedRoot)
        {
            var arguments = testDataAttribute?.ArgumentList?.Arguments;
            if (arguments != null && arguments.Value.Any())
            {
                return Task.FromResult(trackedRoot);
            }

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

        protected override SyntaxNode UpdateTestMethodParameters(SyntaxNode trackedRoot, 
            MethodDeclarationSyntax methodDeclaration, AttributeSyntax testDataAttribute,
            SemanticModel model, CancellationToken cancellationToken)
        {
            var arguments = testDataAttribute?.ArgumentList?.Arguments;
            if (arguments != null && arguments.Value.Any())
            {
                return ScaffoldTestMethodParameters(trackedRoot, methodDeclaration, arguments.Value, model, cancellationToken);
            }

            return base.UpdateTestMethodParameters(trackedRoot, methodDeclaration, testDataAttribute, model, cancellationToken);
        }

        private static SyntaxNode ScaffoldTestMethodParameters(SyntaxNode trackedRoot,
            MethodDeclarationSyntax methodDeclaration, SeparatedSyntaxList<AttributeArgumentSyntax> arguments,
            SemanticModel model, CancellationToken cancellationToken)
        {
            var parameterList = new List<SyntaxNodeOrToken>();
            for (var i = 0; i < arguments.Count; i++)
            {
                parameterList.Add(
                    Parameter(Identifier($"param{i + 1}"))
                    .WithType(CreateTypeSyntax(model, arguments[i].Expression, cancellationToken)));

                if (i < arguments.Count - 1)
                {
                    parameterList.Add(Token(SyntaxKind.CommaToken));
                }
            }

            var trackedMethod = trackedRoot.GetCurrentNode(methodDeclaration);
            var newMethod = trackedMethod.WithParameterList(
                                ParameterList(
                                SeparatedList<ParameterSyntax>(parameterList)));

            return trackedRoot.ReplaceNode(trackedMethod, newMethod);
        }
    }
}
