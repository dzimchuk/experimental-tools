using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Formatting;
using ExperimentalTools.Localization;

namespace ExperimentalTools.Refactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(InitializeFieldFromConstructorParameter)), Shared]
    internal class InitializeFieldFromConstructorParameter : CodeRefactoringProvider
    {
        private readonly INameGenerator nameGenerator;
        
        [ImportingConstructor]
        public InitializeFieldFromConstructorParameter(INameGenerator nameGenerator)
        {
            this.nameGenerator = nameGenerator;
        }

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
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
            
            var parameter = node as ParameterSyntax;
            if (parameter == null)
            {
                return;
            }

            var constructor = parameter.Ancestors().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
            if (constructor == null)
            {
                return;
            }
            
            if (await CheckIfAlreadyInitializedAsync(context.Document, parameter, constructor, context.CancellationToken))
            {
                return;
            }

            var action = CodeAction.Create(Resources.AddInitializedField, 
                cancellationToken => InitializeFieldAsync(context.Document, root, parameter, constructor, cancellationToken));
            context.RegisterRefactoring(action);
        }

        private static async Task<bool> CheckIfAlreadyInitializedAsync(Document document, ParameterSyntax parameter,
            ConstructorDeclarationSyntax constructor, CancellationToken cancellationToken)
        {
            var parameterName = parameter.Identifier.ValueText;
            var assignments = (constructor.DescendantNodes().OfType<AssignmentExpressionSyntax>()
                .Where(exp => exp.Right.DescendantTokens().Any(token => token.ValueText == parameterName))).ToList();

            if (!assignments.Any())
            {
                return false;
            }

            var model = await document.GetSemanticModelAsync(cancellationToken);
            var currentType = model.GetDeclaredSymbol(constructor.Parent, cancellationToken) as INamedTypeSymbol;

            return assignments.Any(assignment =>
            {
                var targetType = model.GetSymbolInfo(assignment.Left, cancellationToken).Symbol?.ContainingType;
                return currentType == targetType;
            });
        }

        private async Task<Document> InitializeFieldAsync(Document document, SyntaxNode root,
            ParameterSyntax parameter, ConstructorDeclarationSyntax constructor,
            CancellationToken cancellationToken)
        {
            var fieldType = parameter.Type;
            var fieldName = await nameGenerator.GetNewMemberNameAsync((ClassDeclarationSyntax)constructor.Parent, parameter.Identifier.ValueText, document, cancellationToken);

            var field = CreateFieldDeclaration(fieldType, fieldName);
            var assignment = CreateThisAssignmentStatement(fieldName, parameter.Identifier.ValueText);

            var trackedRoot = root.TrackNodes(constructor);
            var newRoot = trackedRoot.InsertNodesBefore(trackedRoot.GetCurrentNode(constructor), SingletonList(field));

            var constructorBody = newRoot.GetCurrentNode(constructor).Body;
            var newConstructorBody = constructorBody.AddStatements(assignment)
                .WithAdditionalAnnotations(Formatter.Annotation);

            newRoot = newRoot.ReplaceNode(constructorBody, newConstructorBody);

            return document.WithSyntaxRoot(newRoot);
        }

        private static ExpressionStatementSyntax CreateThisAssignmentStatement(string leftIdentifier, string rightIdentifier)
        {
            return ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        ThisExpression(),
                                        IdentifierName(leftIdentifier)),
                                    IdentifierName(rightIdentifier)));
        }

        private static FieldDeclarationSyntax CreateFieldDeclaration(TypeSyntax fieldType, string fieldName)
        {
            return FieldDeclaration(
                        VariableDeclaration(fieldType)
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(
                                        Identifier(fieldName)))))
                        .WithModifiers(
                            TokenList(
                                new[]{
                                    Token(SyntaxKind.PrivateKeyword),
                                    Token(SyntaxKind.ReadOnlyKeyword)}));
        }
    }
}