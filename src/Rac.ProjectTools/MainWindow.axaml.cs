using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace Rac.ProjectTools;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void GenerateButton_Click(object? sender, RoutedEventArgs e)
    {
        string? name = ComponentNameBox.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            var errorDialog = new Window
            {
                Title = "Error",
                Width = 300,
                Height = 100,
                Content = new TextBlock
                {
                    Text = "Please enter a component name.",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
            };
            await errorDialog.ShowDialog(this);
            return;
        }

        // Build the stub using user's namespace
        string ns = NamespaceBox.Text.Trim();
        string stub =
            $@"
namespace {ns}
{{
    public record struct {name} : IComponent {{ }}
}}";

        // Determine output folder (or fallback to CWD)
        string? folder = string.IsNullOrWhiteSpace(FolderPathBox.Text)
            ? Directory.GetCurrentDirectory()
            : FolderPathBox.Text;
        string filePath = Path.Combine(folder, $"{name}.cs");
        File.WriteAllText(filePath, stub);

        var successDialog = new Window
        {
            Title = "Success",
            Width = 300,
            Height = 100,
            Content = new TextBlock
            {
                Text = $"Generated {name}.cs",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            },
        };
        await successDialog.ShowDialog(this);
    }

    private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog { Title = "Select Output Folder" };

        // Show the dialog; parent this window so it’s modal
        string? result = await dlg.ShowAsync(this);
        if (!string.IsNullOrWhiteSpace(result))
            FolderPathBox.Text = result;
    }
}
