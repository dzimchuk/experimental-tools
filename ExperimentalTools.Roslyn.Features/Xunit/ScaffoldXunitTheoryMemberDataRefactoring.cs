using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExperimentalTools.Roslyn.Features.Xunit
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ScaffoldXunitTheoryMemberDataRefactoring)), Shared]
    internal class ScaffoldXunitTheoryMemberDataRefactoring : ScaffoldXunitTheoryRefactoring
    {
        private readonly SimpleNameGenerator nameGenerator = new SimpleNameGenerator();
        
        [ImportingConstructor]
        public ScaffoldXunitTheoryMemberDataRefactoring(IOptions options)
            : base(options)
        {
        }

        protected override string FeatureIdentifier => FeatureIdentifiers.ScaffoldXunitTheoryMemberData;
        protected override string Title => Resources.ScaffoldXunitTheoryMemberData;
        protected override string TestDataAttribute => "Xunit.MemberDataAttribute";
        protected override string AlternateTestDataAttribute => "Xunit.InlineDataAttribute";

        protected override bool CheckIfAlreadyScaffolded(SemanticModel model, AttributeSyntax memberDataAttribute,
            CancellationToken cancellationToken)
        {
            var memberName = GetMemberName(memberDataAttribute);
            if (string.IsNullOrWhiteSpace(memberName))
            {
                return false;
            }

            var declaredType = model.GetDeclaredSymbol(memberDataAttribute.GetParentTypeDeclaration(), cancellationToken) as INamedTypeSymbol;
            return declaredType.MemberNames.Contains(memberName);
        }

        private static string GetMemberName(AttributeSyntax memberDataAttribute)
        {
            var arg = memberDataAttribute?.ArgumentList?.Arguments.FirstOrDefault();
            if (arg != null)
            {
                var expr = arg.Expression as LiteralExpressionSyntax;
                if (expr != null && expr.Kind() == SyntaxKind.StringLiteralExpression)
                {
                    return expr.Token.ValueText;
                }
            }

            return null;
        }
        
        protected override SyntaxNode AddUsings(CompilationUnitSyntax compilationUnit, ISet<INamespaceSymbol> existingUsings, SyntaxNode trackedRoot)
        {
            trackedRoot = base.AddUsings(compilationUnit, existingUsings, trackedRoot);
            trackedRoot = AddUsingIfNeeded(trackedRoot, compilationUnit, existingUsings, "System.Collections.Generic");
            
            return trackedRoot;
        }

        protected override async Task<SyntaxNode> SetupTestDataAttributeAsync(Document document, AttributeListSyntax attrList, 
            AttributeSyntax memberDataAttribute, CancellationToken cancellationToken, TypeDeclarationSyntax typeDeclaration, 
            MethodDeclarationSyntax methodDeclaration, SyntaxNode trackedRoot)
        {
            var memberName = await GetNewMemberNameAsync(document, cancellationToken, typeDeclaration, methodDeclaration, memberDataAttribute);

            var propertyDeclaration = CreatePropertyDeclaration(memberName);
            trackedRoot = trackedRoot.InsertNodesAfter(trackedRoot.GetCurrentNode(methodDeclaration), SingletonList(propertyDeclaration));

            trackedRoot = SetupMemberDataAttribute(trackedRoot, attrList, memberName, memberDataAttribute);
            return trackedRoot;
        }

        private Task<string> GetNewMemberNameAsync(Document document, CancellationToken cancellationToken, 
            TypeDeclarationSyntax typeDeclaration, MethodDeclarationSyntax methodDeclaration, AttributeSyntax memberDataAttribute)
        {
            var memberName = GetMemberName(memberDataAttribute);
            if (string.IsNullOrWhiteSpace(memberName))
            {
                memberName = $"{methodDeclaration.Identifier.ToString()}Data";
            }
                
            return nameGenerator.GetNewMemberNameAsync(typeDeclaration, memberName, document, cancellationToken);
        }
        
        private static SyntaxNode SetupMemberDataAttribute(SyntaxNode trackedRoot, AttributeListSyntax attrList, 
            string memberName, AttributeSyntax memberDataAttribute)
        {
            var newMemberDataAttr = Attribute(IdentifierName("MemberData")).WithArgumentList(
                                AttributeArgumentList(
                                    SingletonSeparatedList<AttributeArgumentSyntax>(
                                        AttributeArgument(
                                            LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal(memberName))))));

            if (memberDataAttribute != null)
            {
                var trackedMemberDataAttr = trackedRoot.GetCurrentNode(memberDataAttribute);
                return trackedRoot.ReplaceNode(trackedMemberDataAttr, newMemberDataAttr);
            }
            else
            {
                var trackedAttrList = trackedRoot.GetCurrentNode(attrList);
                var newAttrList = trackedAttrList.AddAttributes(newMemberDataAttr);

                return trackedRoot.ReplaceNode(trackedAttrList, newAttrList);
            }
        }

        public static PropertyDeclarationSyntax CreatePropertyDeclaration(string propertyName) =>
            PropertyDeclaration(
                GenericName(
                    Identifier("IEnumerable"))
                .WithTypeArgumentList(
                    TypeArgumentList(
                        SingletonSeparatedList<TypeSyntax>(
                            ArrayType(
                                PredefinedType(
                                    Token(SyntaxKind.ObjectKeyword)))
                            .WithRankSpecifiers(
                                SingletonList<ArrayRankSpecifierSyntax>(
                                    ArrayRankSpecifier(
                                        SingletonSeparatedList<ExpressionSyntax>(
                                            OmittedArraySizeExpression()))))))),
                Identifier(propertyName))
            .WithModifiers(
                TokenList(
                    new[]{
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.StaticKeyword)}))
            .WithExpressionBody(
                ArrowExpressionClause(
                    Token(SyntaxKind.EqualsGreaterThanToken).WithTrailingTrivia(LineFeed),
                    ImplicitArrayCreationExpression(
                        InitializerExpression(
                            SyntaxKind.ArrayInitializerExpression,
                            SeparatedList<ExpressionSyntax>(
                            new SyntaxNodeOrToken[]{
                                ArrayCreationExpression(
                                    ArrayType(
                                        PredefinedType(
                                            Token(SyntaxKind.ObjectKeyword)))
                                    .WithRankSpecifiers(
                                        SingletonList<ArrayRankSpecifierSyntax>(
                                            ArrayRankSpecifier(
                                                SingletonSeparatedList<ExpressionSyntax>(
                                                    OmittedArraySizeExpression())))))
                                .WithInitializer(
                                    InitializerExpression(
                                        SyntaxKind.ArrayInitializerExpression,
                                        SeparatedList<ExpressionSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression,
                                                    Literal("value1")),
                                                Token(SyntaxKind.CommaToken),
                                                LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression,
                                                    Literal("value2"))})))})))))
            .WithSemicolonToken(
                Token(SyntaxKind.SemicolonToken));
    }
}
