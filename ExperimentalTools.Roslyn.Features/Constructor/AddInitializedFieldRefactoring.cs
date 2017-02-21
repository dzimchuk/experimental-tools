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
using static ExperimentalTools.Roslyn.SyntaxFactory;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace ExperimentalTools.Roslyn.Features.Constructor
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AddInitializedFieldRefactoring)), Shared]
    internal class AddInitializedFieldRefactoring : CodeRefactoringProvider
    {
        private readonly SimpleNameGenerator nameGenerator = new SimpleNameGenerator();
        private readonly IOptions options;
        
        [ImportingConstructor]
        public AddInitializedFieldRefactoring(IOptions options)
        {
            this.options = options;
        }

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            if (!options.IsFeatureEnabled(FeatureIdentifiers.AddInitializedFieldRefactoring))
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
            
            var parameter = node as ParameterSyntax;
            if (parameter == null && context.Span.Start > 0)
            {
                var span = new TextSpan(context.Span.Start - 1, 0);
                node = root.FindNode(span);

                parameter = node as ParameterSyntax;
                if (parameter == null)
                {
                    return;
                }
            }

            if (parameter == null)
            {
                return;
            }

            var constructor = parameter.Ancestors().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
            if (constructor == null)
            {
                return;
            }

            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            if (CheckIfAlreadyInitialized(model, parameter, constructor, context.CancellationToken))
            {
                return;
            }

            var existingField = FindExistingField(model, parameter, parameter.GetParentTypeDeclaration(), context.CancellationToken);
            
            var action = CodeAction.Create(existingField != null ? Resources.InitializeExistingField : Resources.AddInitializedField, 
                cancellationToken => InitializeFieldAsync(context.Document, root, parameter, constructor, cancellationToken));
            context.RegisterRefactoring(action);
        }

        private static bool CheckIfAlreadyInitialized(SemanticModel model, ParameterSyntax parameter,
            ConstructorDeclarationSyntax constructor, CancellationToken cancellationToken)
        {
            var parameterName = parameter.Identifier.ValueText;
            var assignments = (constructor.DescendantNodes().OfType<AssignmentExpressionSyntax>()
                .Where(exp => exp.Right.DescendantTokens().Any(token => token.ValueText == parameterName))).ToList();

            if (!assignments.Any())
            {
                return false;
            }

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
            string fieldName;
            var typeDeclaration = constructor.GetParentTypeDeclaration();
            var trackedRoot = root.TrackNodes(constructor, typeDeclaration);

            var model = await document.GetSemanticModelAsync(cancellationToken);
            var existingField = FindExistingField(model, parameter, constructor.Parent as TypeDeclarationSyntax, cancellationToken);
            if (existingField != null)
            {
                fieldName = existingField.Name;
            }
            else
            {
                fieldName = await nameGenerator.GetNewMemberNameAsync(constructor.Parent as TypeDeclarationSyntax, parameter.Identifier.ValueText, document, cancellationToken);
                var field = CreateFieldDeclaration(parameter.Type, fieldName);
                                
                var insertionPoint = FindInsertionPointBelowFieldGroup(trackedRoot.GetCurrentNode(typeDeclaration));
                if (insertionPoint != null)
                {
                    trackedRoot = trackedRoot.InsertNodesAfter(insertionPoint, SingletonList(field));
                }
                else
                {
                    insertionPoint = trackedRoot.GetCurrentNode(typeDeclaration).ChildNodes().First(node => !(node is BaseListSyntax) && !(node is TypeParameterListSyntax));
                    trackedRoot = trackedRoot.InsertNodesBefore(insertionPoint, SingletonList(field));
                }
            }
                        
            var assignment = constructor.ParameterList.Parameters.Any(p => p.Identifier.ValueText == fieldName)
                ? CreateThisAssignmentStatement(fieldName, parameter.Identifier.ValueText)
                : CreateAssignmentStatement(fieldName, parameter.Identifier.ValueText);
            
            var constructorBody = trackedRoot.GetCurrentNode(constructor).Body;
            var newConstructorBody = constructorBody.AddStatements(assignment)
                .WithAdditionalAnnotations(Formatter.Annotation);

            trackedRoot = trackedRoot.ReplaceNode(constructorBody, newConstructorBody);

            return document.WithSyntaxRoot(trackedRoot);
        }

        private static IFieldSymbol FindExistingField(SemanticModel model, ParameterSyntax parameter,
            TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
        {
            var parameterSymbol = model.GetDeclaredSymbol(parameter, cancellationToken) as IParameterSymbol;

            var declaredTypeSymbol = model.GetDeclaredSymbol(typeDeclaration, cancellationToken) as INamedTypeSymbol;
            var fields = declaredTypeSymbol.GetMembers(parameter.Identifier.ValueText);
            foreach (var field in fields)
            {
                var fieldSymbol = field as IFieldSymbol;
                if (fieldSymbol != null && 
                    fieldSymbol.Type == parameterSymbol.Type &&
                    fieldSymbol.IsReadOnly)
                {
                    return fieldSymbol;
                }
            }

            return null;
        }

        private static SyntaxNode FindInsertionPointBelowFieldGroup(TypeDeclarationSyntax typeDeclaration)
        {
            SyntaxNode insertionPoint = null;

            var tracking = false;
            foreach (var node in typeDeclaration.ChildNodes())
            {
                if (tracking)
                {
                    if (node is FieldDeclarationSyntax)
                    {
                        insertionPoint = node;
                        continue;
                    }

                    break;
                }
                else if (node is FieldDeclarationSyntax)
                {
                    insertionPoint = node;
                    tracking = true;
                }
            }

            return insertionPoint;
        }
    }
}