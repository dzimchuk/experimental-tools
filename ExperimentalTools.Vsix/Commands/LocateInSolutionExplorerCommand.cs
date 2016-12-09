﻿using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;

namespace ExperimentalTools.Vsix.Commands
{
    internal sealed class LocateInSolutionExplorerCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("19e14031-1353-4195-87d9-e5657920db5c");

        private const string miscFilesProjectItemKind = "{66A2671F-8FB5-11D2-AA7E-00C04F688DDE}";

        private readonly OleMenuCommand command;
        private readonly DTE2 dte;

        public LocateInSolutionExplorerCommand(IMenuCommandService commandService, DTE2 dte)
        {
            this.dte = dte;

            var commandId = new CommandID(CommandSet, CommandId);
            command = new OleMenuCommand(InvokeCommand, commandId);
            command.BeforeQueryStatus += UpdateStatus;

            commandService.AddCommand(command);
        }

        public static LocateInSolutionExplorerCommand Instance { get; private set; }
        
        public static void Initialize(Package package)
        {
            var serviceProvider = (IServiceProvider)package;
            var commandService = serviceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            var dte = serviceProvider.GetService(typeof(DTE)) as DTE2;

            Instance = new LocateInSolutionExplorerCommand(commandService, dte);
        }

        private void UpdateStatus(object sender, EventArgs e)
        {
            var enabled = !miscFilesProjectItemKind.Equals(dte.ActiveDocument.ProjectItem.Kind, StringComparison.OrdinalIgnoreCase);
            command.Visible = enabled;
            command.Enabled = enabled;
        }

        private void InvokeCommand(object sender, EventArgs e)
        {
            dte.ExecuteCommand("SolutionExplorer.SyncWithActiveDocument");
        }
    }
}
