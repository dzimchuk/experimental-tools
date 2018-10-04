using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExperimentalTools.Workspace
{
    internal class WorkspaceManager
    {
        private readonly FileSystemWatcher watcher;

        public WorkspaceManager(Microsoft.CodeAnalysis.Workspace workspace)
        {
            watcher = new FileSystemWatcher
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                IncludeSubdirectories = true,
                Filter = "*.csproj"
            };

            watcher.Changed += (s, e) => UpdateProjectDescription(e.FullPath);
            watcher.Renamed += (s, e) => UpdateProjectDescription(e.FullPath);

            workspace.WorkspaceChanged += WorkspaceChanged;
            if (workspace.CurrentSolution != null)
            {
                Task.Run(() => AddToCache(workspace.CurrentSolution));
            }
        }
        
        private void UpdateProjectDescription(string fullPath)
        {
            var description = WorkspaceCache.Instance.FindProjectByPath(Path.GetDirectoryName(fullPath));
            if (description != null)
            {
                AddDetails(fullPath, description);
            }
        }

        private void StartWatcher(Solution solution)
        {
            try
            {
                watcher.Path = Path.GetDirectoryName(solution.FilePath);
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private void StopWatcher()
        {
            watcher.EnableRaisingEvents = false;
        }

        private void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e) // gets fired on UI thread
        {
            switch (e.Kind)
            {
                case WorkspaceChangeKind.SolutionAdded:
                    Task.Run(() => AddToCache(e.NewSolution));
                    break;
                case WorkspaceChangeKind.SolutionRemoved:
                    StopWatcher();
                    WorkspaceCache.Instance.Clear();
                    break;
                case WorkspaceChangeKind.ProjectAdded:
                    var addedProject = e.NewSolution.Projects.FirstOrDefault(p => p.Id == e.ProjectId);
                    if (addedProject != null)
                    {
                        Task.Run(() => AddToCache(addedProject));
                    }
                    break;
                case WorkspaceChangeKind.ProjectRemoved:
                    WorkspaceCache.Instance.RemoveProject(e.ProjectId);
                    break;
                default:
                    break;
            }
        }

        private void AddToCache(Solution solution)
        {
            foreach (var project in solution.Projects)
            {
                AddToCache(project);
            }

            StartWatcher(solution);
        }

        private static void AddToCache(Project project)
        {
            if (!string.IsNullOrWhiteSpace(project.FilePath))
            {
                var description = new ProjectDescription
                {
                    Id = project.Id,
                    Path = Path.GetDirectoryName(project.FilePath),
                    AssemblyName = project.Name
                };

                AddDetails(project.FilePath, description);

                WorkspaceCache.Instance.AddOrUpdateProject(description);
            }
        }

        private const string projectNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        private static void AddDetails(string projectFile, ProjectDescription description)
        {
            try
            {
                var doc = XDocument.Parse(File.ReadAllText(projectFile));

                var targetFramework = doc.Descendants("TargetFramework").FirstOrDefault()?.Value;
                if (!string.IsNullOrEmpty(targetFramework) && (targetFramework.StartsWith("netcoreapp") || targetFramework.StartsWith("netstandard")))
                {
                    description.IsDotNetCore = true;
                }

                var defaultNamespace = doc.Descendants(XName.Get("RootNamespace", description.IsDotNetCore ? string.Empty : projectNamespace)).FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(defaultNamespace))
                {
                    description.DefaultNamespace = defaultNamespace;
                }

                var assemblyName = doc.Descendants(XName.Get("AssemblyName", description.IsDotNetCore ? string.Empty : projectNamespace)).FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(assemblyName))
                {
                    description.AssemblyName = assemblyName;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
    }
}
