using ExperimentalTools.Options;
using ExperimentalTools.Workspace;
using System;

namespace ExperimentalTools
{
    public static class ServiceLocator
    {
        private static readonly Lazy<IOptions> options = new Lazy<IOptions>(() => new OptionsService(), true);
        private static readonly Lazy<IWorkspace> workspace = new Lazy<IWorkspace>(() => new WorkspaceService(), true);

        public static T GetService<T>() where T: class
        {
            if (typeof(T) == typeof(IOptions))
            {
                return options.Value as T;
            }

            if (typeof(T) == typeof(IWorkspace))
            {
                return workspace.Value as T;
            }

            throw new NotSupportedException($"Service {typeof(T)} not supported");
        }
    }
}
