using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;
using Task = System.Threading.Tasks.Task;

namespace ExperimentalTools.Vsix.Commands
{
    internal sealed class LocateInSolutionExplorerCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("19e14031-1353-4195-87d9-e5657920db5c");

        private const string miscFilesProjectItemKind = "{66A2671F-8FB5-11D2-AA7E-00C04F688DDE}";

        private readonly OleMenuCommand command;
        private readonly DTE2 dte;
        private readonly IOptions options;

        public LocateInSolutionExplorerCommand(IMenuCommandService commandService, DTE2 dte, IOptions options)
        {
            this.dte = dte;
            this.options = options;

            var commandId = new CommandID(CommandSet, CommandId);
            command = new OleMenuCommand(InvokeCommand, commandId);
            command.BeforeQueryStatus += UpdateStatus;

            commandService.AddCommand(command);
        }

        public static LocateInSolutionExplorerCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage serviceProvider)
        {
            var commandService = await serviceProvider.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var dte = await serviceProvider.GetServiceAsync(typeof(DTE)) as DTE2;

            var options = ServiceLocator.GetService<IOptions>();

            Instance = new LocateInSolutionExplorerCommand(commandService, dte, options);
        }

        private void UpdateStatus(object sender, EventArgs e)
        {
            var enabled = options.IsFeatureEnabled(FeatureIdentifiers.LocateInSolutionExplorerCommand) &&
                !miscFilesProjectItemKind.Equals(dte.ActiveDocument.ProjectItem.Kind, StringComparison.OrdinalIgnoreCase);
            command.Visible = enabled;
            command.Enabled = enabled;
        }

        private void InvokeCommand(object sender, EventArgs e)
        {
            dte.ExecuteCommand("SolutionExplorer.SyncWithActiveDocument");
        }
    }
}
