using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace XamlPreview;

public class ProjectLoader
{
    public bool Register()
    {
        if (MSBuildLocator.IsRegistered)
        {
            return true;
        }
        var defaultInstance = MSBuildLocator.RegisterDefaults();
        if (defaultInstance is null)
        {
            return false;
        }
        return true;
    }
    
    public async Task<CSharpCompilation> Compile(string solutionFilePath, string projectFilePath)
    {
        var manager = new AnalyzerManager(solutionFilePath);

        // manager.Projects

        var analyzer = manager.GetProject(projectFilePath);

        var projectFileName = Path.GetFileName(projectFilePath);

        var analyzerResults = analyzer.Build();

        /*
        foreach (var analyzerResult in analyzerResults)
        {
            if (analyzerResult.Items.TryGetValue("AvaloniaXaml", out var projectItems))
            {
                foreach (var projectItem in projectItems)
                {
                    // Debug.WriteLine($"[AvaloniaXaml] {projectItem.ItemSpec}");
                }
            }
        }
        */

        var workspace = analyzer.GetWorkspace();

        var solution = workspace.CurrentSolution;

        // solution.Projects

        var project = solution.Projects.FirstOrDefault(x => x.FilePath.EndsWith(projectFileName));

        var compilationOptions = 
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithAllowUnsafe(true)
                .WithOptimizationLevel(OptimizationLevel.Debug);

        project = project.WithCompilationOptions(compilationOptions);

        var compilation = await project.GetCompilationAsync();

        return compilation as CSharpCompilation;
    }

    public Assembly? GetScriptAssembly(CSharpCompilation compilation)
    {
        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        var errors = result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error);

        if (!result.Success || errors.Any())
        {
            return null;
        }

        ms.Seek(0, SeekOrigin.Begin);

        var context = new AssemblyLoadContext(name: Path.GetRandomFileName(), isCollectible: true);
        return context.LoadFromStream(ms);
    }
}
