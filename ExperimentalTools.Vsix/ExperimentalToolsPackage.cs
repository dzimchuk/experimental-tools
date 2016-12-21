using ExperimentalTools.Models;
using ExperimentalTools.Vsix.Commands;
using ExperimentalTools.Vsix.Options;
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
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace ExperimentalTools.Vsix
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)] // Info on this package for Help/About
    [Guid(Vsix.Id)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideOptionPage(typeof(GeneralOptions), "Experimental Tools", "General", 0, 0, true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class ExperimentalToolsPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            var componentModel = await GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
            var workspace = componentModel.GetService<VisualStudioWorkspace>();
            workspace.WorkspaceChanged += WorkspaceChanged;

            await JoinableTaskFactory.SwitchToMainThreadAsync();
            var generalOptions = (GeneralOptions)GetDialogPage(typeof(GeneralOptions));
            generalOptions.UpdateFeatureStates();

            await LocateInSolutionExplorerCommand.InitializeAsync(this);
        }

        private void WorkspaceChanged(object sender, Microsoft.CodeAnalysis.WorkspaceChangeEventArgs e)
        {
            switch (e.Kind)
            {
                case Microsoft.CodeAnalysis.WorkspaceChangeKind.SolutionRemoved:
                    WorkspaceCache.Instance.Clear();
                    break;
                case Microsoft.CodeAnalysis.WorkspaceChangeKind.ProjectAdded:
                case Microsoft.CodeAnalysis.WorkspaceChangeKind.ProjectChanged:
                    var project = e.NewSolution.Projects.FirstOrDefault(p => p.Id == e.ProjectId);
                    if (project != null)
                    {
                        var description = new ProjectDescription
                        {
                            Id = project.Id,
                            Path = !string.IsNullOrWhiteSpace(project.FilePath) ? Path.GetDirectoryName(project.FilePath) : null,
                            AssemblyName = project.AssemblyName
                        };
                        WorkspaceCache.Instance.AddOrUpdateProject(description);
                    }
                    break;
                case Microsoft.CodeAnalysis.WorkspaceChangeKind.ProjectRemoved:
                    WorkspaceCache.Instance.RemoveProject(e.ProjectId);
                    break;
                default:
                    break;
            }
        }
    }
}
