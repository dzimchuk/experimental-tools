using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host;
using System;
using System.Reflection;

namespace ExperimentalTools.Workspace
{
    public static class Extensions
    {
        public static void InitializeWorkspaceService(this AnalyzerOptions analyzerOptions)
        {
            try
            {
                var prop = analyzerOptions.GetType().GetRuntimeProperty("Services");
                var workspaceServices = prop?.GetValue(analyzerOptions) as HostWorkspaceServices;

                var workspace = workspaceServices?.Workspace;
                if (workspace == null)
                {
                    return;
                }

                WorkspaceManager.Instance.Initialize(workspace);
            }
            catch (Exception ex)
            {
                SimpleLogger.WriteLine(ex.ToString(), writeToDebug: true);
            }
        }
    }
}
