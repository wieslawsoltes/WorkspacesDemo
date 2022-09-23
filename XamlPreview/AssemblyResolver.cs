﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace XamlPreview;

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
            new PackageCompilationAssemblyResolver()
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