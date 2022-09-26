using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace XamlPreview;

internal class DirectReferenceAssemblyResolver : ICompilationAssemblyResolver
{
    public bool TryResolveAssemblyPaths(CompilationLibrary library, List<string> assemblies)
    {
        if (!string.Equals(library.Type, "reference", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var paths = new List<string>();

        foreach (var assembly in library.Assemblies)
        {
            var path = Path.Combine(ApplicationEnvironment.ApplicationBasePath, assembly);

            if (!File.Exists(path))
            {
                return false;
            }

            paths.Add(path);
        }

        assemblies.AddRange(paths);

        return true;
    }
}

internal sealed class AssemblyResolver : IDisposable
{
    private readonly ICompilationAssemblyResolver assemblyResolver;
    private readonly DependencyContext dependencyContext;
    private readonly AssemblyLoadContext loadContext;

    public AssemblyResolver(string path)
    {
        this.Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
        this.dependencyContext = DependencyContext.Load(this.Assembly);

        var compilationAssemblyResolvers = new ICompilationAssemblyResolver[]
        {
            new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),
            new ReferenceAssemblyPathResolver(),
            new PackageCompilationAssemblyResolver(),
            new DirectReferenceAssemblyResolver()
        };
        this.assemblyResolver = new CompositeCompilationAssemblyResolver(
            compilationAssemblyResolvers);

        this.loadContext = AssemblyLoadContext.GetLoadContext(this.Assembly);
        this.loadContext.Resolving += OnResolving;
    }

    public Assembly Assembly { get; }

    public void Dispose()
    {
        this.loadContext.Resolving -= this.OnResolving;
    }

    private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
    {
        bool NamesMatch(RuntimeLibrary runtime)
        {
            return string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);
        }

        var library = this.dependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatch);
        if (library != null)
        {
            var wrapper = new CompilationLibrary(
                library.Type,
                library.Name,
                library.Version,
                library.Hash,
                library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                library.Dependencies,
                library.Serviceable);
            
            Debug.WriteLine($"{library.Type}, {library.Name}");

            var assemblies = new List<string>();
            this.assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);
            if (assemblies.Count > 0)
            {
                foreach (var assembly in assemblies)
                {
                    Debug.WriteLine($"  assembly={assembly}");
                }
                return this.loadContext.LoadFromAssemblyPath(assemblies[0]);
            }
        }

        return null;
    }
}
