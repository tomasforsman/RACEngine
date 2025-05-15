using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Rac.ProjectTools
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private async void GenerateButton_Click(object? sender, RoutedEventArgs e)
		{
			var name = ComponentNameBox.Text?.Trim();
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
						VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
						HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
					}
				};
				await errorDialog.ShowDialog(this);
				return;
			}

			// Build the stub using user's namespace
			var ns = NamespaceBox.Text.Trim();
			var stub = $@"
namespace {ns}
{{
    public record struct {name} : IComponent {{ }}
}}";

// Determine output folder (or fallback to CWD)
			var folder = string.IsNullOrWhiteSpace(FolderPathBox.Text)
				? Directory.GetCurrentDirectory()
				: FolderPathBox.Text;
			var filePath = Path.Combine(folder, $"{name}.cs");
			File.WriteAllText(filePath, stub);

			

			var successDialog = new Window
			{
				Title = "Success",
				Width = 300,
				Height = 100,
				Content = new TextBlock
				{
					Text = $"Generated {name}.cs",
					VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
					HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
				}
			};
			await successDialog.ShowDialog(this);
		}
		
		private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
		{
			var dlg = new OpenFolderDialog
			{
				Title = "Select Output Folder"
			};

			// Show the dialog; parent this window so it’s modal
			var result = await dlg.ShowAsync(this);
			if (!string.IsNullOrWhiteSpace(result))
			{
				FolderPathBox.Text = result;
			}
		}
	}
}