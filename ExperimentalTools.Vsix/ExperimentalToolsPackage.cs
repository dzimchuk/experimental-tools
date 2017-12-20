using ExperimentalTools.Vsix.Features.LocateInSolutionExplorer;
using ExperimentalTools.Vsix.Features.Options;
using ExperimentalTools.Workspace;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace ExperimentalTools.Vsix
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)] // Info on this package for Help/About
    [Guid(Vsix.Id)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideOptionPage(typeof(GeneralOptions), "Experimental Tools", "General", 0, 0, true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class ExperimentalToolsPackage : Package
    {
        protected override void Initialize()
        {
            base.Initialize();

            var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
            var workspace = componentModel.GetService<VisualStudioWorkspace>();
            workspace.WorkspaceChanged += WorkspaceChanged;

            var generalOptions = (GeneralOptions)GetDialogPage(typeof(GeneralOptions));
            generalOptions.UpdateFeatureStates();

            LocateInSolutionExplorerCommand.Initialize(this);
        }        

        private void WorkspaceChanged(object sender, Microsoft.CodeAnalysis.WorkspaceChangeEventArgs e)
        {
            switch (e.Kind)
            {
                case Microsoft.CodeAnalysis.WorkspaceChangeKind.SolutionAdded:
                    foreach (var project in e.NewSolution.Projects)
                    {
                        AddToCache(project);
                    }
                    break;
                case Microsoft.CodeAnalysis.WorkspaceChangeKind.SolutionRemoved:
                    WorkspaceCache.Instance.Clear();
                    break;
                case Microsoft.CodeAnalysis.WorkspaceChangeKind.ProjectAdded:
                    var addedProject = e.NewSolution.Projects.FirstOrDefault(p => p.Id == e.ProjectId);
                    if (addedProject != null)
                    {
                        AddToCache(addedProject);
                    }
                    break;
                case Microsoft.CodeAnalysis.WorkspaceChangeKind.ProjectRemoved:
                    WorkspaceCache.Instance.RemoveProject(e.ProjectId);
                    break;
                default:
                    break;
            }
        }

        private static void AddToCache(Microsoft.CodeAnalysis.Project project)
        {
            if (string.IsNullOrWhiteSpace(project.FilePath))
            {
                return;
            }

            var description = new ProjectDescription
            {
                Id = project.Id,
                Path = Path.GetDirectoryName(project.FilePath),
                AssemblyName = project.AssemblyName
            };

            AddDetails(project.FilePath, description);

            WorkspaceCache.Instance.AddOrUpdateProject(description);
        }

        private static readonly XName rootNamespace = XName.Get("RootNamespace", "http://schemas.microsoft.com/developer/msbuild/2003");
        private static void AddDetails(string projectFile, ProjectDescription description)
        {
            try
            {
                var doc = XDocument.Parse(File.ReadAllText(projectFile));

                var targetFramework = doc.Descendants("TargetFramework").FirstOrDefault()?.Value;
                if (!string.IsNullOrEmpty(targetFramework) && (targetFramework.StartsWith("netcoreapp") || targetFramework.StartsWith("netstandard")))
                {
                    description.IsDotNetCore = true;

                    var defaultNamespace = doc.Descendants("RootNamespace").FirstOrDefault()?.Value;
                    if (!string.IsNullOrWhiteSpace(defaultNamespace))
                    {
                        description.DefaultNamespace = defaultNamespace;
                    }
                }
                else
                {
                    var defaultNamespace = doc.Descendants(rootNamespace).FirstOrDefault()?.Value;
                    if (!string.IsNullOrWhiteSpace(defaultNamespace))
                    {
                        description.DefaultNamespace = defaultNamespace;
                    }
                }
            }
            catch
            {
            }
        }
    }
}
