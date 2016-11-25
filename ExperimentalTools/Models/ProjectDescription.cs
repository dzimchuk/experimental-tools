using Microsoft.CodeAnalysis;

namespace ExperimentalTools.Models
{
    internal class ProjectDescription
    {
        public ProjectId Id { get; set; }
        public string Path { get; set; }
        public string AssemblyName { get; set; }
    }
}
