using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.CodeActions;
using ExperimentalTools.Localization;
using System.Threading;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static ExperimentalTools.SyntaxFactory;

namespace ExperimentalTools.Refactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AddConstructorParameterRefactoring)), Shared]
    internal class AddConstructorParameterRefactoring : CodeRefactoringProvider
    {
        private readonly INameGenerator nameGenerator;

        public AddConstructorParameterRefactoring(INameGenerator nameGenerator)
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

            FieldDeclarationSyntax fieldDeclaration;
            VariableDeclaratorSyntax variableDeclarator;

            variableDeclarator = node as VariableDeclaratorSyntax;
            if (variableDeclarator != null)
            {
                fieldDeclaration = variableDeclarator.Ancestors().OfType<FieldDeclarationSyntax>().FirstOrDefault();
            }
            else
            {
                fieldDeclaration = node as FieldDeclarationSyntax;
                if (fieldDeclaration != null)
                {
                    variableDeclarator = fieldDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
                }
            }

            if (fieldDeclaration == null || variableDeclarator == null)
            {
                return;
            }

            var typeDeclaration = fieldDeclaration.GetParentTypeDeclaration();
            if (typeDeclaration == null)
            {
                return;
            }

            var constructors = typeDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>().ToList();
            if (!constructors.Any())
            {
                return;
            }

            if (await CheckIfAlreadyInitializedAsync(context.Document, variableDeclarator, typeDeclaration, 
                constructors, context.CancellationToken))
            {
                return;
            }

            var actionTitle = constructors.Count > 1
                ? Resources.InitializeFieldInExistingConstructors
                : Resources.InitializeFieldInExistingConstructor;
            var action = CodeAction.Create(actionTitle, 
                token => InitializeFieldInConstructorsAsync(context.Document, root, fieldDeclaration, constructors));
            context.RegisterRefactoring(action);
        }

        private static async Task<bool> CheckIfAlreadyInitializedAsync(Document document, VariableDeclaratorSyntax variableDeclarator,
            TypeDeclarationSyntax typeDeclaration, IEnumerable<ConstructorDeclarationSyntax> constructors,
            CancellationToken cancellationToken)
        {
            var model = await document.GetSemanticModelAsync(cancellationToken);
            var fieldSymbol = model.GetDeclaredSymbol(variableDeclarator, cancellationToken);

            foreach (var constructor in constructors)
            {
                var assignments = constructor.DescendantNodes().OfType<AssignmentExpressionSyntax>().ToList();
                foreach (var assignment in assignments)
                {
                    var leftSymbol = model.GetSymbolInfo(assignment.Left, cancellationToken).Symbol;
                    if (leftSymbol != null && leftSymbol == fieldSymbol)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        private Task<Document> InitializeFieldInConstructorsAsync(Document document, SyntaxNode root, 
            FieldDeclarationSyntax fieldDeclaration, IEnumerable<ConstructorDeclarationSyntax> constructors)
        {
            var variableDeclaration = fieldDeclaration.DescendantNodes().OfType<VariableDeclarationSyntax>().FirstOrDefault();
            var variableDeclarator = variableDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
            var fieldName = variableDeclarator.Identifier.ValueText;

            var trackedRoot = root.TrackNodes(constructors);
            foreach (var constructor in constructors)
            {
                var currentConstructor = trackedRoot.GetCurrentNode(constructor);
                var parameterName = nameGenerator.GetNewParameterName(currentConstructor.ParameterList, fieldName);

                var newConstructor = currentConstructor.WithParameterList(currentConstructor.ParameterList.AddParameters(
                    Parameter(Identifier(parameterName))
                        .WithType(variableDeclaration.Type)));

                var assignment = CreateThisAssignmentStatement(fieldName, parameterName);
                newConstructor = newConstructor.WithBody(newConstructor.Body.AddStatements(assignment));

                trackedRoot = trackedRoot.ReplaceNode(currentConstructor, newConstructor);
            }

            return Task.FromResult(document.WithSyntaxRoot(trackedRoot));
        }
    }
}