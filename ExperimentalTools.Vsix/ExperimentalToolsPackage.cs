using EnvDTE;
using EnvDTE80;
using ExperimentalTools.Options;
using ExperimentalTools.Vsix.Features.LocateInSolutionExplorer;
using ExperimentalTools.Vsix.Features.Options;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;

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
        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            var componentModel = await GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
            Assumes.Present(componentModel);

            var dte = await GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(dte);

            OptionsBucket.Instance.Initialize(GetVSVersion(dte));

            await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);

            LocateInSolutionExplorerCommand.Initialize(this);

            var generalOptions = (GeneralOptions)GetDialogPage(typeof(GeneralOptions));
            generalOptions.UpdateFeatureStates();
        }

        private static Version GetVSVersion(DTE2 dte)
        {
            try
            {
                var fileVersion = FileVersionInfo.GetVersionInfo(dte.FullName);
                return new Version(
                    fileVersion.FileMajorPart != 0 ? fileVersion.FileMajorPart : 15,
                    fileVersion.FileMinorPart);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return OptionsBucket.DefaultVersion;
            }
        }
    }
}
