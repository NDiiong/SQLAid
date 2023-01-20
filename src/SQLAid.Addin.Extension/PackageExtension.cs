using System;

namespace SQLAid.Addin.Extension
{
    public static class PackageExtension
    {
        public static TService GetService<TInterface, TService>(this IServiceProvider serviceProvider)
            where TService : class, TInterface
        {
            return serviceProvider.GetService(typeof(TInterface)) as TService;
        }
    }
}