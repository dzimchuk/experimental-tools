using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace ExperimentalTools.Roslyn.Features.Namespace
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class NamespaceNormalizationAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.NamespaceNormalizationAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.NamespaceNormalizationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticCodes.NamespaceNormalizationAnalyzer, Title, MessageFormat, Resources.CategoryNaming, GetSeverity(), true);

        private static DiagnosticSeverity GetSeverity() =>
            ServiceLocator.GetService<IOptions>().IsFeatureEnabled(FeatureIdentifiers.NamespaceNormalizationAnalyzer) ? DiagnosticSeverity.Warning : DiagnosticSeverity.Hidden;

        private readonly GeneratedCodeRecognitionService generatedCodeRecognitionService = new GeneratedCodeRecognitionService();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
        
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.NamespaceDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (generatedCodeRecognitionService.IsGeneratedCode(context))
            {
                return;
            }

            var documentPath = context.Node.SyntaxTree.FilePath;
            if (documentPath == null)
            {
                return;
            }

            var assemblyName = context.Compilation.AssemblyName;
            if (assemblyName == null)
            {
                return;
            }

            var namespaceNode = (NamespaceDeclarationSyntax)context.Node;
            if (!namespaceNode.IsTopLevel())
            {
                return;
            }

            if (!IsTheOnlyNamespace(namespaceNode, context.CancellationToken))
            {
                return;
            }

            var desiredName = ConstructDesiredName(documentPath, assemblyName);
            if (string.IsNullOrWhiteSpace(desiredName))
            {
                return;
            }

            if (!namespaceNode.Name.ToString().Equals(desiredName, StringComparison.OrdinalIgnoreCase))
            {
                var properties = ImmutableDictionary<string, string>.Empty.Add("DesiredName", desiredName);
                context.ReportDiagnostic(Diagnostic.Create(Rule, namespaceNode.Name.GetLocation(), properties, namespaceNode.Name.ToString()));
            }
        }

        private static string ConstructDesiredName(string documentPath, string assemblyName)
        {
            var folderStack = new Stack<string>();
            var workspace = ServiceLocator.GetService<IWorkspace>();

            var path = Path.GetDirectoryName(documentPath);
            var found = false;
            do
            {
                var project = workspace.FindProjectByPath(path);
                if (project != null)
                {
                    if (project.AssemblyName == assemblyName)
                    {
                        found = true;
                    }

                    break;
                }

                var folder = Path.GetFileName(path);
                if (!string.IsNullOrWhiteSpace(folder))
                {
                    folderStack.Push(folder);
                }

                path = Path.GetDirectoryName(path);

            } while (!string.IsNullOrWhiteSpace(path));

            if (found)
            {
                var builder = new StringBuilder(assemblyName);
                while (folderStack.Any())
                {
                    builder.AppendFormat(".{0}", folderStack.Pop());
                }

                return builder.ToString();
            }

            return null;
        }

        private static bool IsTheOnlyNamespace(NamespaceDeclarationSyntax namespaceDeclaration, CancellationToken cancellationToken)
        {
            var root = namespaceDeclaration.SyntaxTree.GetRoot(cancellationToken);
            var topLevelTypes = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Where(t => t.IsTopLevel());
            return topLevelTypes.Count() == 1;
        }
    }
}