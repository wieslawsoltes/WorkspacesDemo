using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

//PrintInstances();

Register();

var path1 = "/Users/wieslawsoltes/Documents/GitHub/WalletWasabi/WalletWasabi.sln";
//var path1 = @"c:\Users\Administrator\Documents\GitHub\WalletWasabi\WalletWasabi.sln";

await Run(path1);
return;
//await Load1(path1);
await Load2(path1);

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

async Task Run(string path)
{
    var projectFilePath = @"/Users/wieslawsoltes/Documents/GitHub/WalletWasabi/WalletWasabi.Fluent/WalletWasabi.Fluent.csproj";
    //var projectFilePath = @"c:\Users\Administrator\Documents\GitHub\WalletWasabi\WalletWasabi.Fluent\WalletWasabi.Fluent.csproj";
    AnalyzerManager manager = new AnalyzerManager(path);

    foreach (var project in manager.Projects)
    {
        Console.WriteLine($"[Projects] {project.Value.ProjectFile.Name}");
    }

    //*
    IProjectAnalyzer analyzer = manager.GetProject(projectFilePath);

    var result = analyzer.Build();

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

async Task Load1(string path)
{
    try
    {
        var workspace = MSBuildWorkspace.Create();
        var solution = await workspace.OpenSolutionAsync(path);
 
        await PrintSolution(solution);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

async Task Load2(string path)
{
    try
    {
        var workspace = MSBuildWorkspace.Create();
        var loader = new MSBuildProjectLoader(workspace);
        var solutionInfo = await loader.LoadSolutionInfoAsync(path);

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
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}
