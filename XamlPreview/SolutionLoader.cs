using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

namespace XamlPreview;

public class SolutionLoader
{    
    public static bool Register()
    {
        var defaultInstance = MSBuildLocator.RegisterDefaults();
        if (defaultInstance is null)
        {
            return false;
        }
        return true;
    }

    public static bool CompileSolution(string solutionUrl, out Dictionary<string, MemoryStream> assemblies)
    {
        var success = true;
        var workspace = MSBuildWorkspace.Create();
        var solution = workspace.OpenSolutionAsync(solutionUrl).Result;
        var projectGraph = solution.GetProjectDependencyGraph(); 

        assemblies = new Dictionary<string, MemoryStream>();

        foreach (var projectId in projectGraph.GetTopologicallySortedProjects())
        {
            var project = solution.GetProject(projectId);
            if (project.CompilationOptions.Language != "C#")
            {
                continue;
            }

            var compilationOptions = 
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithAllowUnsafe(true)
                    .WithOptimizationLevel(OptimizationLevel.Debug);

            project = project.WithCompilationOptions(compilationOptions);

            var projectCompilation = project.GetCompilationAsync().Result;
            if (projectCompilation is { } && !string.IsNullOrEmpty(projectCompilation.AssemblyName))
            { 
                var ms = new MemoryStream();
                var result = projectCompilation.Emit(ms);
                if (result.Success)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    assemblies[projectCompilation.AssemblyName] = ms;
                }
                else
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }
        }

        return success;
    }
}
