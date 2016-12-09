using ExperimentalTools.Models;
using System.Composition;

namespace ExperimentalTools.Workspace
{
    [Export(typeof(IWorkspace))]
    internal class WorkspaceService : IWorkspace
    {
        public ProjectDescription FindProjectByPath(string path) => WorkspaceCache.Instance.FindProjectByPath(path);
    }
}
