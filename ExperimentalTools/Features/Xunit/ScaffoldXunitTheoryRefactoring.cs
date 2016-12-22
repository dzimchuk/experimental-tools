using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExperimentalTools.Features.Xunit
{
    internal abstract class ScaffoldXunitTheoryRefactoring : CodeRefactoringProvider
    {
        private readonly IOptions options;
        
        public ScaffoldXunitTheoryRefactoring(IOptions options)
        {
            this.options = options;
        }

        protected abstract string FeatureIdentifier { get; }
        protected abstract string Title { get; }
        protected abstract string TestDataAttribute { get; }
        protected abstract string AlternateTestDataAttribute { get; }

        protected abstract bool CheckIfAlreadyScaffolded(SemanticModel model, AttributeSyntax testDataAttribute,
            CancellationToken cancellationToken);
        protected abstract Task<SyntaxNode> SetupTestDataAttributeAsync(Document document, AttributeListSyntax attrList, 
            AttributeSyntax testDataAttribute, CancellationToken cancellationToken, TypeDeclarationSyntax typeDeclaration, 
            MethodDeclarationSyntax methodDeclaration, SyntaxNode trackedRoot);

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            if (!options.IsFeatureEnabled(FeatureIdentifier))
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

            var methodDeclaration = (MethodDeclarationSyntax)attrList.Parent;
            if (methodDeclaration.ParameterList.Parameters.Any())
            {
                return;
            }

            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken);

            var alternateTestDataAttribute = FindAttribute(methodDeclaration, AlternateTestDataAttribute, model, context.CancellationToken);
            if (alternateTestDataAttribute != null)
            {
                return;
            }

            var testDataAttribute = FindAttribute(methodDeclaration, TestDataAttribute, model, context.CancellationToken);
            if (testDataAttribute != null)
            {
                if (CheckIfAlreadyScaffolded(model, testDataAttribute, context.CancellationToken))
                {
                    return;
                }
            }

            var theoryAttribute = FindAttribute(methodDeclaration, "Xunit.TheoryAttribute", model, context.CancellationToken);
            if (theoryAttribute == null && testDataAttribute == null)
            {
                return;
            }

            var action = CodeAction.Create(Title,
                cancellationToken =>
                    ScaffoldAsync(context.Document,
                                  root,
                                  model,
                                  (testDataAttribute != null ? testDataAttribute.Parent : theoryAttribute.Parent) as AttributeListSyntax,
                                  testDataAttribute,
                                  theoryAttribute == null,
                                  cancellationToken));
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

        private static AttributeSyntax FindAttribute(MethodDeclarationSyntax methodDeclaration, string attrTypeName,
            SemanticModel model, CancellationToken cancellationToken)
        {
            foreach (var attr in methodDeclaration.DescendantNodes().OfType<AttributeSyntax>())
            {
                if (CheckAttribute(attr, attrTypeName, model, cancellationToken))
                {
                    return attr;
                }
            }

            return null;
        }

        private static bool CheckAttribute(AttributeSyntax attr, string attrTypeName,
            SemanticModel model, CancellationToken cancellationToken)
        {
            var symbol = model.GetTypeInfo(attr, cancellationToken).Type;
            return symbol != null && symbol.ToString().Equals(attrTypeName) && CheckAssembly(symbol.ContainingAssembly);
        }

        private static bool CheckAssembly(IAssemblySymbol assembly) =>
            assembly != null && assembly.Name.ToUpperInvariant().Contains("XUNIT") && assembly.Identity.Version.Major >= 2;

        private async Task<Document> ScaffoldAsync(Document document, SyntaxNode root, SemanticModel model,
            AttributeListSyntax attrList, AttributeSyntax testDataAttribute, bool addTheory, CancellationToken cancellationToken)
        {
            var typeDeclaration = attrList.GetParentTypeDeclaration();
            var compilationUnit = typeDeclaration.Ancestors().OfType<CompilationUnitSyntax>().First();
            var methodDeclaration = (MethodDeclarationSyntax)attrList.Parent;

            var existingUsings = model.GetUsingNamespacesInScope(methodDeclaration);

            var trackedRoot = testDataAttribute != null
                ? root.TrackNodes(methodDeclaration, typeDeclaration, attrList, compilationUnit, testDataAttribute)
                : root.TrackNodes(methodDeclaration, typeDeclaration, attrList, compilationUnit);

            trackedRoot = await SetupTestDataAttributeAsync(document, attrList, testDataAttribute, cancellationToken, typeDeclaration, methodDeclaration, trackedRoot);
            trackedRoot = UpdateTestMethodParameters(trackedRoot, methodDeclaration, testDataAttribute, model, cancellationToken);

            if (addTheory)
            {
                trackedRoot = AddTheoryAttribute(trackedRoot, attrList);
            }

            trackedRoot = AddUsings(compilationUnit, existingUsings, trackedRoot);

            return document.WithSyntaxRoot(trackedRoot);
        }

        protected virtual SyntaxNode AddUsings(CompilationUnitSyntax compilationUnit, ISet<INamespaceSymbol> existingUsings, SyntaxNode trackedRoot) => 
            AddUsingIfNeeded(trackedRoot, compilationUnit, existingUsings, "Xunit");

        protected SyntaxNode AddUsingIfNeeded(SyntaxNode trackedRoot, CompilationUnitSyntax compilationUnit, ISet<INamespaceSymbol> existingUsings, string @namespace)
        {
            if (!existingUsings.Any(@using => @using.ToString().Equals(@namespace)))
            {
                var trackedCompilationUnit = trackedRoot.GetCurrentNode(compilationUnit);
                trackedRoot = trackedRoot.AddNamespaceUsing(trackedCompilationUnit, @namespace);
            }

            return trackedRoot;
        }

        protected virtual SyntaxNode UpdateTestMethodParameters(SyntaxNode trackedRoot, 
            MethodDeclarationSyntax methodDeclaration, AttributeSyntax testDataAttribute,
            SemanticModel model, CancellationToken cancellationToken)
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

        private static SyntaxNode AddTheoryAttribute(SyntaxNode trackedRoot, AttributeListSyntax attrList)
        {
            var trackedAttrList = trackedRoot.GetCurrentNode(attrList);
            var attributes = trackedAttrList.Attributes.Insert(0, Attribute(IdentifierName("Theory")));
            var newAttrList = trackedAttrList.WithAttributes(attributes);

            return trackedRoot.ReplaceNode(trackedAttrList, newAttrList);
        }
    }
}
