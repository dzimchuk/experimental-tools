using Microsoft.CodeAnalysis;

namespace ExperimentalTools.Workspace
{
    public class ProjectDescription
    {
        public ProjectId Id { get; set; }
        public string Path { get; set; }
        public string AssemblyName { get; set; }
    }
}
