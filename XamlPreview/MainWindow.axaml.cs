using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace XamlPreview;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            SolutionPath.Text =  @"c:\Users\Administrator\Documents\GitHub\WalletWasabi\WalletWasabi.sln";
            ProjectPath.Text = @"c:\Users\Administrator\Documents\GitHub\WalletWasabi\WalletWasabi.Fluent\WalletWasabi.Fluent.csproj";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            SolutionPath.Text =  @"/Users/wieslawsoltes/Documents/GitHub/WalletWasabi/WalletWasabi.sln";
            ProjectPath.Text = @"/Users/wieslawsoltes/Documents/GitHub/WalletWasabi/WalletWasabi.Fluent/WalletWasabi.Fluent.csproj";
        }
    }

    private async void LoadButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var dlg = new OpenFileDialog()
            {
                Filters = new List<FileDialogFilter>()
                {
                    new() { Name = "Xaml files (.axaml)", Extensions = new List<string> { "axaml" } },
                    new() { Name = "All files", Extensions = new List<string> { "*" } }
                },
                AllowMultiple = false
            };

            var result = await dlg.ShowAsync(this);
            if (result is { })
            {
                var path = result.FirstOrDefault();
                if (path is null)
                {
                    return;
                }

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
                    var types = assembly.GetTypes();
                    if (kvp.Key.Equals(projectFileName))
                    {
                        scriptAssembly = assembly;
                    }
                }

                //Assembly? scriptAssembly = context.LoadFromStream(ms);

                
                var control = AvaloniaRuntimeXamlLoader.Parse<IControl?>(xaml, scriptAssembly);
                if (control is { })
                {
                    XamlContentControl.Content = control;
                }
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }
}
