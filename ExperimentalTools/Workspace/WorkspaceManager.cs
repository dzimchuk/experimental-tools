using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExperimentalTools.Workspace
{
    internal class WorkspaceManager
    {
        public WorkspaceManager(Microsoft.CodeAnalysis.Workspace workspace)
        {
            workspace.WorkspaceChanged += WorkspaceChanged;
            if (workspace.CurrentSolution != null)
            {
                Task.Run(() => AddToCacheAsync(workspace.CurrentSolution.Projects));
            }
        }
        
        private void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            switch (e.Kind)
            {
                case WorkspaceChangeKind.SolutionAdded:
                    Task.Run(() => AddToCacheAsync(e.NewSolution.Projects));
                    break;
                case WorkspaceChangeKind.SolutionRemoved:
                    WorkspaceCache.Instance.Clear();
                    break;
                case WorkspaceChangeKind.ProjectAdded:
                    var addedProject = e.NewSolution.Projects.FirstOrDefault(p => p.Id == e.ProjectId);
                    if (addedProject != null)
                    {
                        Task.Run(() => AddToCacheAsync(addedProject));
                    }
                    break;
                case WorkspaceChangeKind.ProjectRemoved:
                    WorkspaceCache.Instance.RemoveProject(e.ProjectId);
                    break;
                default:
                    break;
            }
        }

        private static async Task AddToCacheAsync(IEnumerable<Project> projects)
        {
            foreach (var project in projects)
            {
                await AddToCacheAsync(project);
            }
        }

        private static async Task AddToCacheAsync(Project project)
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

            await AddDetailsAsync(project.FilePath, description);

            WorkspaceCache.Instance.AddOrUpdateProject(description);
        }

        private static readonly XName rootNamespace = XName.Get("RootNamespace", "http://schemas.microsoft.com/developer/msbuild/2003");
        private static async Task AddDetailsAsync(string projectFile, ProjectDescription description)
        {
            try
            {
                using (var stream = File.OpenText(projectFile))
                {
                    var content = await stream.ReadToEndAsync();
                    var doc = XDocument.Parse(content);

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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
    }
}
