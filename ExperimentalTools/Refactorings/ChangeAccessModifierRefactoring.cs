using System;
using System.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using ExperimentalTools.Localization;

namespace ExperimentalTools.Refactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ChangeAccessModifierRefactoring)), Shared]
    internal class ChangeAccessModifierRefactoring : CodeRefactoringProvider
    {
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

            IEnumerable<CodeAction> actions = null;

            var typeDeclaration = node as ClassDeclarationSyntax;
            if (typeDeclaration != null)
            {
                actions = CalculateActions(context.Document, root, typeDeclaration);
            }

            if (actions != null && actions.Any())
            {
                foreach (var action in actions)
                {
                    context.RegisterRefactoring(action);
                }
            }
        }

        private IEnumerable<CodeAction> CalculateActions(Document document, SyntaxNode root,
            ClassDeclarationSyntax typeDeclaration)
        {
            var actions = new List<CodeAction>();
            CalculateInternalExplicit(document, root, typeDeclaration, actions);
            CalculatePublic(document, root, typeDeclaration, actions);

            return actions;
        }

        private static void CalculateInternalExplicit(Document document, SyntaxNode root,
            ClassDeclarationSyntax typeDeclaration, List<CodeAction> actions)
        {
            var accessModifiers = FindAccessModifiers(typeDeclaration);
            if (accessModifiers.Count > 1)
            {

            }
            else if (accessModifiers.Count == 1 && accessModifiers[0].Kind() != SyntaxKind.InternalKeyword)
            {

            }
            else if (!accessModifiers.Any())
            {
                actions.Add(CodeAction.Create(Resources.ToInternalExplicit, cancellationToken =>
                {
                    var modifiers = typeDeclaration.Modifiers.Insert(0, Token(SyntaxKind.InternalKeyword));
                    var newTypeDeclaration = typeDeclaration.WithModifiers(modifiers);
                    var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration);

                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }));
            }
        }

        private static void CalculatePublic(Document document, SyntaxNode root,
            ClassDeclarationSyntax typeDeclaration, List<CodeAction> actions)
        {
            var accessModifiers = FindAccessModifiers(typeDeclaration);
            if (accessModifiers.Count > 1)
            {

            }
            else if (accessModifiers.Count == 1 && accessModifiers[0].Kind() != SyntaxKind.PublicKeyword)
            {

            }
            else if (!accessModifiers.Any())
            {
                actions.Add(CodeAction.Create(Resources.ToPublic, cancellationToken =>
                {
                    var modifiers = typeDeclaration.Modifiers.Insert(0, Token(SyntaxKind.PublicKeyword));
                    var newTypeDeclaration = typeDeclaration.WithModifiers(modifiers);
                    var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration);

                    return Task.FromResult(document.WithSyntaxRoot(newRoot));
                }));
            }
        }

        private static List<SyntaxToken> FindAccessModifiers(BaseTypeDeclarationSyntax typeDeclaration)
        {
            var modifierKinds = new[]
            {
                SyntaxKind.PublicKeyword,
                SyntaxKind.InternalKeyword,
                SyntaxKind.PrivateKeyword,
                SyntaxKind.ProtectedKeyword
            };

            return typeDeclaration.Modifiers.Where(modifier => modifierKinds.Contains(modifier.Kind())).ToList();
        }
    }
}