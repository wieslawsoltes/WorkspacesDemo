using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace XamlPreview;

public class Config
{
    public string? SolutionFilePath { get; set; }

    public string? ProjectFilePath { get; set; }
    
    public string? XamlFilePath { get; set; }
}

public partial class MainWindow : Window
{
    private const string ConfigFileJson = "config.json";

    public MainWindow()
    {
        InitializeComponent();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var basePath = @"c:\Users\Administrator\Documents\GitHub\WalletWasabi";
            SolutionPath.Text =  @$"{basePath}\WalletWasabi.sln";
            ProjectPath.Text = @$"{basePath}\WalletWasabi.Fluent\WalletWasabi.Fluent.csproj";
            XamlPath.Text = @$"{basePath}\WalletWasabi.Fluent\Views\Wallets\ClosedHardwareWalletView.axaml";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var basePath = @"/Users/wieslawsoltes/Documents/GitHub/WalletWasabi";
            SolutionPath.Text =  @$"{basePath}/WalletWasabi.sln";
            ProjectPath.Text = @$"{basePath}/WalletWasabi.Fluent/WalletWasabi.Fluent.csproj";
            XamlPath.Text = @$"{basePath}/WalletWasabi.Fluent/Views/Wallets/ClosedHardwareWalletView.axaml";
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        LoadConfig(ConfigFileJson);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        SaveConfig(ConfigFileJson);
    }

    private async void BrowseSolutionButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await BrowseSolution();
    }

    private async void BrowseProjectButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await BrowseProject();
    }

    private async void BrowseXamlButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await BrowseXaml();
    }

    private async void LoadButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await Load(XamlPath.Text);
    }

    private void LoadConfig(string configFileJson)
    {
        if (File.Exists(configFileJson))
        {
            var json = File.ReadAllText(configFileJson);
            var config = System.Text.Json.JsonSerializer.Deserialize<Config>(json);
            if (config is { })
            {
                if (config.SolutionFilePath is { })
                {
                    SolutionPath.Text = config.SolutionFilePath;
                }

                if (config.ProjectFilePath is { })
                {
                    ProjectPath.Text = config.ProjectFilePath;
                }

                if (config.XamlFilePath is { })
                {
                    XamlPath.Text = config.XamlFilePath;
                }
            }
        }
    }

    private void SaveConfig(string configFileJson)
    {
        var config = new Config
        {
            SolutionFilePath = SolutionPath.Text,
            ProjectFilePath = ProjectPath.Text,
            XamlFilePath = XamlPath.Text
        };

        var json = System.Text.Json.JsonSerializer.Serialize(config);
        File.WriteAllText(configFileJson, json);
    }

    private async Task BrowseProject()
    {
        try
        {
            var dlg = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter>
                {
                    new() { Name = "Project files (*.csproj)", Extensions = new List<string> { "csproj" } },
                    new() { Name = "All files (*.*)", Extensions = new List<string> { "*" } }
                },
                AllowMultiple = false
            };

            var result = await dlg.ShowAsync(this);
            if (result is { })
            {
                var path = result.FirstOrDefault();
                if (path is not null)
                {
                    ProjectPath.Text = path;
                }
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private async Task BrowseSolution()
    {
        try
        {
            var dlg = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter>
                {
                    new() { Name = "Solution files (*.sln)", Extensions = new List<string> { "sln" } },
                    new() { Name = "All files (*.*)", Extensions = new List<string> { "*" } }
                },
                AllowMultiple = false
            };

            var result = await dlg.ShowAsync(this);
            if (result is { })
            {
                var path = result.FirstOrDefault();
                if (path is not null)
                {
                    SolutionPath.Text = path;
                }
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private async Task BrowseXaml()
    {
        try
        {
            var dlg = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter>
                {
                    new() { Name = "Xaml files (*.xaml,*.axaml)", Extensions = new List<string> { "xaml", "axaml" } },
                    new() { Name = "All files (*.*)", Extensions = new List<string> { "*" } }
                },
                AllowMultiple = false
            };

            var result = await dlg.ShowAsync(this);
            if (result is { })
            {
                var path = result.FirstOrDefault();
                if (path is not null)
                {
                    XamlPath.Text = path;
                }
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private async Task Load(string path)
    {
        try
        {
            {
                //Directory.SetCurrentDirectory(Path.GetDirectoryName(path));
                var context1 = new AssemblyLoadContext(name: Path.GetRandomFileName(), isCollectible: true);
                context1.Resolving += (loadContext, name) =>
                {
                    var p = Path.GetDirectoryName(path);
                    var f = Path.Combine(p, name.Name + ".dll");
                    return Assembly.LoadFile(f);
                };
                
                
                var rawAssembly = File.Open(path, FileMode.Open);
                var scriptAssembly1 = context1.LoadFromStream(rawAssembly);
                var types = scriptAssembly1.GetTypes();
                var t = types.FirstOrDefault(x => x.Name == "MainView");
                var c = Activator.CreateInstance(t);
                XamlContentControl.Content = c;
            }
            return;
            
            
            
            
            
            
            
            
            
            
            
            var xaml = await File.ReadAllTextAsync(path);

            Assembly? scriptAssembly = null;


    /*
            var loader = new ProjectLoader();
            loader.Register();

            var compilation = await loader.Compile(SolutionPath.Text, ProjectPath.Text);
            var assembly = loader.GetScriptAssembly(compilation);

            scriptAssembly = assembly;
    */


            SolutionLoader.Register();
            if (!SolutionLoader.CompileSolution(SolutionPath.Text, out var assemblies))
            {
                return;
            }

            var projectFilePath = ProjectPath.Text;
            var projectFileName = Path.GetFileNameWithoutExtension(projectFilePath);
            //var ms = assemblies.FirstOrDefault(x => x.Key.Equals(projectFileName)).Value;
            var context = new AssemblyLoadContext(name: Path.GetRandomFileName(), isCollectible: true);

            foreach (var kvp in assemblies)
            {
                //var assembly = Assembly.Load(kvp.Value.ToArray());
                var assembly = context.LoadFromStream(kvp.Value);
                if (kvp.Key.Equals(projectFileName))
                {
                    scriptAssembly = assembly;
                }
            }


            // Assembly? scriptAssembly = context.LoadFromStream(ms);

            /*
            var types = scriptAssembly.GetTypes();
            var t = types.FirstOrDefault(x => x.Name == "MainView");
            var c = Activator.CreateInstance(t);
            XamlContentControl.Content = c;
            //*/

            //*
            var control = AvaloniaRuntimeXamlLoader.Parse<IControl?>(xaml, scriptAssembly);
            if (control is { })
            {
                XamlContentControl.Content = control;
            }
            //*/
            
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }
}
