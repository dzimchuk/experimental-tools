using ExperimentalTools.Workspace;

namespace ExperimentalTools
{
    public interface IWorkspace
    {
        ProjectDescription FindProjectByPath(string path);
    }
}
