using System;
using System.Linq;
using System.Reflection;

namespace Vlingo.Xoom.Http.Resource
{
    public static class EmbeddedResourceLoader
    {
        public static Assembly LoadFromPath(string path)
        {
            var lastIndexOfPathSeparator = path.LastIndexOf('.');
            var loadPath = path.Substring(0, lastIndexOfPathSeparator);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            while (!string.IsNullOrWhiteSpace(loadPath))
            {
                var assembly = assemblies.SingleOrDefault(a => a.GetName().Name == loadPath);
                if (assembly != null)
                {
                    return assembly;
                }

                var nextIndex = loadPath.LastIndexOf('.');
                if (nextIndex > 0)
                {
                    loadPath = loadPath.Substring(0, nextIndex);
                }
                else
                {
                    loadPath = string.Empty;
                }
            }

            return Assembly.GetExecutingAssembly();
        }
        
        public static string CleanPath(string path) => path.Replace("%20", "_").Replace("/", ".");
    }
}