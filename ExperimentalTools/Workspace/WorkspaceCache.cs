using ExperimentalTools.Models;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;

namespace ExperimentalTools.Workspace
{
    internal class WorkspaceCache
    {
        private static readonly Lazy<WorkspaceCache> instance = new Lazy<WorkspaceCache>(true);
        public static WorkspaceCache Instance => instance.Value;

        private readonly ConcurrentDictionary<ProjectId, ProjectDescription> projectsById 
            = new ConcurrentDictionary<ProjectId, ProjectDescription>();
        private readonly ConcurrentDictionary<string, ProjectDescription> projectsByPath = 
            new ConcurrentDictionary<string, ProjectDescription>();

        public ProjectDescription FindByPath(string path)
        {
            ProjectDescription description;
            if (projectsByPath.TryGetValue(path, out description))
            {
                return description;
            }

            return null;
        }

        public void Clear()
        {
            projectsById.Clear();
            projectsByPath.Clear();
        }

        public void AddOrUpdate(ProjectDescription project)
        {
            if (string.IsNullOrWhiteSpace(project.Path))
            {
                return;
            }
            
            projectsById.AddOrUpdate(project.Id, project, (id, desc) => project);
            projectsByPath.AddOrUpdate(project.Path, project, (path, desc) => project);
        }

        public void Remove(ProjectId projectId)
        {
            ProjectDescription project;
            if (projectsById.TryRemove(projectId, out project))
            {
                projectsByPath.TryRemove(project.Path, out project);
            }
        }
    }
}
