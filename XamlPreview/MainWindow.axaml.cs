using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace XamlPreview;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        SolutionPath.Text =  @"c:\Users\Administrator\Documents\GitHub\WalletWasabi\WalletWasabi.sln";
        ProjectPath.Text = @"c:\Users\Administrator\Documents\GitHub\WalletWasabi\WalletWasabi.Fluent\WalletWasabi.Fluent.csproj";
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

                var loader = new ProjectLoader();
                loader.Register();

                var compilation = await loader.Compile(SolutionPath.Text, ProjectPath.Text);
                var assembly = loader.GetScriptAssembly(compilation);

                Assembly? scriptAssembly = assembly;

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
