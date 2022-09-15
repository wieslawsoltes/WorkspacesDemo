using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

//PrintInstances();

Register();

var solutionPath = @"c:\Users\Administrator\Documents\GitHub\WalletWasabi\WalletWasabi.sln";
var projectPath = @"c:\Users\Administrator\Documents\GitHub\WalletWasabi\WalletWasabi.Fluent\WalletWasabi.Fluent.csproj";

//var solutionPath = "/Users/wieslawsoltes/Documents/GitHub/WalletWasabi/WalletWasabi.sln";
//var projectPath = @"/Users/wieslawsoltes/Documents/GitHub/WalletWasabi/WalletWasabi.Fluent/WalletWasabi.Fluent.csproj";

await Run(solutionPath, projectPath);
return;
//await Load1(solutionPath);
await Load2(solutionPath);

void PrintVisualStudioInstanceInfo(VisualStudioInstance x)
{
    Console.WriteLine($"Name: {x.Name}");
    Console.WriteLine($"Version: {x.Version}");
    Console.WriteLine($"MSBuildPath: {x.MSBuildPath}");
    Console.WriteLine($"VisualStudioRootPath: {x.VisualStudioRootPath}");
    Console.WriteLine($"DiscoveryType: {x.DiscoveryType}");
}

void PrintInstances()
{
    var instances = MSBuildLocator.QueryVisualStudioInstances();
    foreach (var instance in instances)
    {
        PrintVisualStudioInstanceInfo(instance);
    }
}

void Register()
{
    var defaultInstance = MSBuildLocator.RegisterDefaults();
    if (defaultInstance is null)
    {
        return;
    }
    PrintVisualStudioInstanceInfo(defaultInstance);
}

async Task Run(string solutionFilePath, string projectFilePath)
{
    AnalyzerManager manager = new AnalyzerManager(solutionFilePath);

    foreach (var project in manager.Projects)
    {
        Console.WriteLine($"[Projects] {project.Value.ProjectFile.Name}");
    }

    //*
    IProjectAnalyzer analyzer = manager.GetProject(projectFilePath);

    var analyzerResults = analyzer.Build();

    foreach (var analyzerResult in analyzerResults)
    {
        if (analyzerResult.Items.TryGetValue("AvaloniaXaml", out var projectItems))
        {
            foreach (var projectItem in projectItems)
            {
                Console.WriteLine($"[AvaloniaXaml] {projectItem.ItemSpec}");
            }
        }
    }

    
    AdhocWorkspace workspace = analyzer.GetWorkspace();
    
    await PrintSolution(workspace.CurrentSolution);
    //*/
}

async Task PrintSolution(Solution solution)
{
    foreach (var project in solution.Projects)
    {
        var compilation = await project.GetCompilationAsync();

        Console.WriteLine($"[Projects] {project.Name}");

        foreach (var document in project.Documents)
        {
            Console.WriteLine($"  [Documents] {document.Name}");
        }

        foreach (var document in project.AdditionalDocuments)
        {
            Console.WriteLine($"  [AdditionalDocuments] {document.Name}");
        }
    }
}

void PrintSolutionInfo(SolutionInfo solutionInfo)
{
    foreach (var projectInfo in solutionInfo.Projects)
    {
        Console.WriteLine($"[Projects] {projectInfo.Name}");

        foreach (var document in projectInfo.Documents)
        {
            Console.WriteLine($"  [Documents] {document.Name}");
        }

        foreach (var document in projectInfo.AdditionalDocuments)
        {
            Console.WriteLine($"  [AdditionalDocuments] {document.Name}");
        }
    }
}

async Task Load1(string solutionFilePath)
{
    try
    {
        var workspace = MSBuildWorkspace.Create();
        var solution = await workspace.OpenSolutionAsync(solutionFilePath);
 
        await PrintSolution(solution);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

async Task Load2(string solutionFilePath)
{
    try
    {
        var workspace = MSBuildWorkspace.Create();
        var loader = new MSBuildProjectLoader(workspace);
        var solutionInfo = await loader.LoadSolutionInfoAsync(solutionFilePath);

        PrintSolutionInfo(solutionInfo);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}
