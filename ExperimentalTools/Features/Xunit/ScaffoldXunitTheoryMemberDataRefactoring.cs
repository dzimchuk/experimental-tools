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
            
            var memberDataAttribute = FindAttribute(attrList, "Xunit.MemberDataAttribute", model, context.CancellationToken);
            if (memberDataAttribute != null)
            {
                if (CheckIfAlreadyScaffolded(model, memberDataAttribute, context.CancellationToken))
                {
                    return;
                }
            }
            
            var action = CodeAction.Create(Resources.ScaffoldXunitTheoryMemberData,
                cancellationToken => ScaffoldAsync(context.Document, root, model, attrList, memberDataAttribute, cancellationToken));
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

        private static bool CheckAssembly(IAssemblySymbol assembly) => 
            assembly != null && assembly.Name.ToUpperInvariant().Contains("XUNIT") && assembly.Identity.Version.Major >= 2;

        private static bool CheckIfAlreadyScaffolded(SemanticModel model, AttributeSyntax memberDataAttribute,
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

        private async Task<Document> ScaffoldAsync(Document document, SyntaxNode root, SemanticModel model, 
            AttributeListSyntax attrList, AttributeSyntax memberDataAttribute, CancellationToken cancellationToken)
        {
            var typeDeclaration = attrList.GetParentTypeDeclaration();
            var compilationUnit = typeDeclaration.Ancestors().OfType<CompilationUnitSyntax>().First();
            var methodDeclaration = (MethodDeclarationSyntax)attrList.Parent;

            var usings = model.GetUsingNamespacesInScope(methodDeclaration);

            var trackedRoot = memberDataAttribute != null
                ? root.TrackNodes(methodDeclaration, typeDeclaration, attrList, compilationUnit, memberDataAttribute)
                : root.TrackNodes(methodDeclaration, typeDeclaration, attrList, compilationUnit);

            var memberName = await GetNewMemberNameAsync(document, cancellationToken, typeDeclaration, methodDeclaration, memberDataAttribute);

            var propertyDeclaration = CreatePropertyDeclaration(memberName);
            trackedRoot = trackedRoot.InsertNodesAfter(trackedRoot.GetCurrentNode(methodDeclaration), SingletonList(propertyDeclaration));

            trackedRoot = SetupMemberDataAttribute(trackedRoot, attrList, memberName, memberDataAttribute);
            trackedRoot = UpdateTestMethodParameters(trackedRoot, methodDeclaration);

            trackedRoot = AddUsingIfNeeded(trackedRoot, compilationUnit, usings, "System.Collections.Generic");
            trackedRoot = AddUsingIfNeeded(trackedRoot, compilationUnit, usings, "Xunit");

            return document.WithSyntaxRoot(trackedRoot);
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
