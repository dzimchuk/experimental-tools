using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExperimentalTools.Workspace
{
    internal class WorkspaceManager
    {
        private static readonly Lazy<WorkspaceManager> instance = new Lazy<WorkspaceManager>(true);
        public static WorkspaceManager Instance => instance.Value;

        private FileSystemWatcher watcher;

        public void Initialize(Microsoft.CodeAnalysis.Workspace workspace)
        {
            lock (this)
            {
                if (watcher != null)
                {
                    return;
                }

                SimpleLogger.WriteLine($"Initializing WorkspaceManager. Solution: {workspace.CurrentSolution?.FilePath}");

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
                    AddNewSolutionToCache(workspace.CurrentSolution);
                }
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
                watcher.EnableRaisingEvents = false;

                watcher.Path = Path.GetDirectoryName(solution.FilePath);
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        // when running in VS host: SolutionAdded, ProjectAdded, ProjectRemoved
        // when running in external analyzer processs: SolutionAdded, SolutionChanged
        private void WorkspaceChanged(object sender, WorkspaceChangeEventArgs e) // it may get fired on UI thread
        {
            switch (e.Kind)
            {
                case WorkspaceChangeKind.SolutionAdded:
#pragma warning disable VSTHRD110 // Observe result of async calls
                    Task.Run(() => AddNewSolutionToCache(e.NewSolution));
#pragma warning restore VSTHRD110 // Observe result of async calls
                    break;
                case WorkspaceChangeKind.SolutionChanged:
#pragma warning disable VSTHRD110 // Observe result of async calls
                    Task.Run(() => UpdateCache(e.NewSolution));
#pragma warning restore VSTHRD110 // Observe result of async calls
                    break;
                case WorkspaceChangeKind.ProjectAdded:
                    var addedProject = e.NewSolution.Projects.FirstOrDefault(p => p.Id == e.ProjectId);
                    if (addedProject != null)
                    {
#pragma warning disable VSTHRD110 // Observe result of async calls
                        Task.Run(() => AddToCache(addedProject));
#pragma warning restore VSTHRD110 // Observe result of async calls
                    }
                    break;
                case WorkspaceChangeKind.ProjectRemoved:
                    WorkspaceCache.Instance.RemoveProject(e.ProjectId);
                    SimpleLogger.WriteLine($"Project removed: {e.ProjectId}");
                    break;
                default:
                    break;
            }
        }

        private void AddNewSolutionToCache(Solution solution)
        {
            WorkspaceCache.Instance.Clear();

            foreach (var project in solution.Projects)
            {
                AddToCache(project);
            }

            StartWatcher(solution);

            SimpleLogger.WriteLine($"Added solution to cache: {solution.FilePath}");
        }

        private static void UpdateCache(Solution solution)
        {
            foreach (var project in solution.Projects)
            {
                AddToCache(project);
            }
        }

        private static void AddToCache(Project project)
        {
            if (!string.IsNullOrWhiteSpace(project.FilePath))
            {
                var description = new ProjectDescription
                {
                    Id = project.Id,
                    Path = Path.GetDirectoryName(project.FilePath),
                    AssemblyName = project.AssemblyName,
                    DefaultNamespace = GetDefaultNamespace(project)
                };

                WorkspaceCache.Instance.AddOrUpdateProject(description);

                SimpleLogger.WriteLine($"Added project to cache: {description.Path}. Namespace: {description.DefaultNamespace}, AssemblyName: {description.AssemblyName}");
            }
        }

        private static string GetDefaultNamespace(Project project)
        {
            try
            {
                var prop = project.GetType().GetRuntimeProperty("DefaultNamespace");
                return prop?.GetValue(project) as string;
            }
            catch (Exception ex)
            {
                SimpleLogger.WriteLine(ex.ToString(), writeToDebug: true);
                return null;
            }
        }

        private const string projectNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        private static void AddDetails(string projectFile, ProjectDescription description)
        {
            try
            {
                var doc = XDocument.Parse(File.ReadAllText(projectFile));

                var targetFramework = doc.Descendants("TargetFramework").FirstOrDefault()?.Value;
                var isDotNetCore = !string.IsNullOrEmpty(targetFramework) && targetFramework.StartsWith("net");

                var defaultNamespace = doc.Descendants(XName.Get("RootNamespace", isDotNetCore ? string.Empty : projectNamespace)).FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(defaultNamespace))
                {
                    description.DefaultNamespace = defaultNamespace;
                }

                var assemblyName = doc.Descendants(XName.Get("AssemblyName", isDotNetCore ? string.Empty : projectNamespace)).FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(assemblyName))
                {
                    description.AssemblyName = assemblyName;
                }

                SimpleLogger.WriteLine($"Updated details for project: {projectFile}. Namespace: {description.DefaultNamespace}, AssemblyName: {description.AssemblyName}");
            }
            catch (Exception ex)
            {
                SimpleLogger.WriteLine(ex.ToString(), writeToDebug: true);
            }
        }
    }
}
