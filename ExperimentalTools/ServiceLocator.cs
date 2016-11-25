using ExperimentalTools.Services;
using ExperimentalTools.Workspace;
using System;

namespace ExperimentalTools
{
    internal static class ServiceLocator
    {
        public static T GetService<T>() where T: class
        {
            if (typeof(T) == typeof(IGeneratedCodeRecognitionService))
            {
                return new GeneratedCodeRecognitionService() as T;
            }

            if (typeof(T) == typeof(IOptions))
            {
                return new OptionsService() as T;
            }

            if (typeof(T) == typeof(IWorkspace))
            {
                return new WorkspaceService() as T;
            }

            throw new NotSupportedException($"Service {typeof(T)} not supported");
        }
    }
}
