using ExperimentalTools.Localization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExperimentalTools.Features.Xunit
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ScaffoldXunitTheoryMemberDataRefactoring)), Shared]
    internal class ScaffoldXunitTheoryMemberDataRefactoring : CodeRefactoringProvider
    {
        private readonly INameGenerator nameGenerator;
        private readonly IOptions options;

        [ImportingConstructor]
        public ScaffoldXunitTheoryMemberDataRefactoring(INameGenerator nameGenerator, IOptions options)
        {
            this.nameGenerator = nameGenerator;
            this.options = options;
        }

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            if (!options.IsFeatureEnabled(FeatureIdentifiers.ScaffoldXunitTheoryMemberData))
            {
                return;
            }

            if (context.Document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
            {
                return;
            }

            if (!context.Span.IsEmpty)
            {
                return;
            }

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

            var attrList = TryFindAttributeList(node);
            if (attrList == null || 
                !(attrList.Parent is MethodDeclarationSyntax) ||
                !(attrList.Ancestors().OfType<ClassDeclarationSyntax>().Any()))
            {
                return;
            }

            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken);

            var theoryAttribute = FindAttribute(attrList, "Xunit.TheoryAttribute", model, context.CancellationToken);
            if (theoryAttribute == null)
            {
                return;
            }

            var memberDataAttribute = FindMemberDataAttribute(attrList, model, context.CancellationToken);
            if (memberDataAttribute != null)
            {
                return;
            }
            
            var action = CodeAction.Create(Resources.ScaffoldXunitTheoryMemberData,
                cancellationToken => ScaffoldAsync(context.Document, root, model, attrList, cancellationToken));
            context.RegisterRefactoring(action);

        }

        private static AttributeListSyntax TryFindAttributeList(SyntaxNode node)
        {
            AttributeListSyntax attrList = null;

            var attr = node as AttributeSyntax;
            if (attr == null)
            {
                attrList = node as AttributeListSyntax;
            }
            else
            {
                attrList = attr.Parent as AttributeListSyntax;
            }

            return attrList;
        }

        private static AttributeSyntax FindAttribute(AttributeListSyntax attrList, string attrTypeName, SemanticModel model, CancellationToken cancellationToken)
        {
            foreach (var attr in attrList.ChildNodes().OfType<AttributeSyntax>())
            {
                var symbol = model.GetTypeInfo(attr, cancellationToken).Type;
                if (symbol != null && symbol.ToString().Equals(attrTypeName) && CheckAssembly(symbol.ContainingAssembly))
                {
                    return attr;
                }
            }

            return null;
        }
        
        private static AttributeSyntax FindMemberDataAttribute(AttributeListSyntax attrList, SemanticModel model, CancellationToken cancellationToken)
        {
            foreach (var attr in attrList.ChildNodes().OfType<AttributeSyntax>())
            {
                var symbol = model.GetTypeInfo(attr, cancellationToken).Type;
                if (symbol != null && (symbol.Name.Equals("MemberDataAttribute") || symbol.Name.Equals("PropertyDataAttribute")) && CheckAssembly(symbol.ContainingAssembly))
                {
                    return attr;
                }
            }

            return null;
        }

        private static bool CheckAssembly(IAssemblySymbol assembly) => 
            assembly != null && assembly.Name.ToUpperInvariant().Contains("XUNIT") && assembly.Identity.Version.Major >= 2;

        private async Task<Document> ScaffoldAsync(Document document, SyntaxNode root, SemanticModel model, AttributeListSyntax attrList, CancellationToken cancellationToken)
        {
            var typeDeclaration = attrList.GetParentTypeDeclaration();
            var compilationUnit = typeDeclaration.Ancestors().OfType<CompilationUnitSyntax>().First();
            var methodDeclaration = (MethodDeclarationSyntax)attrList.Parent;

            var usings = model.GetUsingNamespacesInScope(methodDeclaration);

            var trackedRoot = root.TrackNodes(methodDeclaration, typeDeclaration, attrList, compilationUnit);

            var propertyName = $"{methodDeclaration.Identifier.ToString()}Data";
            propertyName = await nameGenerator.GetNewMemberNameAsync(typeDeclaration, propertyName, document, cancellationToken);

            var propertyDeclaration = CreatePropertyDeclaration(propertyName);
            trackedRoot = trackedRoot.InsertNodesAfter(trackedRoot.GetCurrentNode(methodDeclaration), SingletonList(propertyDeclaration));

            trackedRoot = SetupMemberDataAttribute(trackedRoot, attrList, propertyName);
            trackedRoot = UpdateTestMethodParameters(trackedRoot, methodDeclaration);

            trackedRoot = AddUsingIfNeeded(trackedRoot, compilationUnit, usings, "System.Collections.Generic");
            trackedRoot = AddUsingIfNeeded(trackedRoot, compilationUnit, usings, "Xunit");

            return document.WithSyntaxRoot(trackedRoot);
        }

        private static SyntaxNode UpdateTestMethodParameters(SyntaxNode trackedRoot, MethodDeclarationSyntax methodDeclaration)
        {
            var trackedMethod = trackedRoot.GetCurrentNode(methodDeclaration);
            var newMethod = trackedMethod.WithParameterList(
                                ParameterList(
                                SeparatedList<ParameterSyntax>(
                                    new SyntaxNodeOrToken[]{
                                        Parameter(
                                            Identifier("param1"))
                                        .WithType(
                                            PredefinedType(
                                                Token(SyntaxKind.StringKeyword))),
                                        Token(SyntaxKind.CommaToken),
                                        Parameter(
                                            Identifier("param2"))
                                        .WithType(
                                            PredefinedType(
                                                Token(SyntaxKind.StringKeyword)))})));

            return trackedRoot.ReplaceNode(trackedMethod, newMethod);
        }

        private static SyntaxNode SetupMemberDataAttribute(SyntaxNode trackedRoot, AttributeListSyntax attrList, string propertyName)
        {
            var trackedAttrList = trackedRoot.GetCurrentNode(attrList);
            var newAttrList = trackedAttrList.AddAttributes(Attribute(IdentifierName("MemberData")).WithArgumentList(
                                AttributeArgumentList(
                                    SingletonSeparatedList<AttributeArgumentSyntax>(
                                        AttributeArgument(
                                            LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal(propertyName)))))));

            return trackedRoot.ReplaceNode(trackedAttrList, newAttrList);
        }

        private static SyntaxNode AddUsingIfNeeded(SyntaxNode trackedRoot, CompilationUnitSyntax compilationUnit, System.Collections.Generic.HashSet<INamespaceSymbol> usings, string @namespace)
        {
            if (!usings.Any(@using => @using.ToString().Equals(@namespace)))
            {
                var trackedCompilationUnit = trackedRoot.GetCurrentNode(compilationUnit);
                trackedRoot = trackedRoot.AddNamespaceUsing(trackedCompilationUnit, @namespace);
            }

            return trackedRoot;
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
