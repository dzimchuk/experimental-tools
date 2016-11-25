using ExperimentalTools.Models;

namespace ExperimentalTools
{
    internal interface IWorkspace
    {
        ProjectDescription FindProjectByPath(string path);
    }
}
