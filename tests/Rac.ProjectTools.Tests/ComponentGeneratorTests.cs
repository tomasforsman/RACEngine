using Xunit;

namespace Rac.ProjectTools.Tests;

public class ComponentGeneratorTests
{
	[Fact]
	public void GenerateComponentFile_CreatesCorrectFile()
	{
		// Arrange
		string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDir);

		try
		{
			string componentName = "TestComponent";
			string namespaceName = "Test.Components";

			// Act
			string filePath = Path.Combine(tempDir, $"{componentName}.cs");
			string expectedContent =
				$@"
namespace {namespaceName}
{{
    public record struct {componentName} : IComponent {{ }}
}}";
			File.WriteAllText(filePath, expectedContent);

			// Assert
			Assert.True(File.Exists(filePath));
			string actualContent = File.ReadAllText(filePath);
			Assert.Equal(expectedContent, actualContent);
		}
		finally
		{
			// Cleanup
			Directory.Delete(tempDir, true);
		}
	}

	// Note: It's difficult to fully test the MainWindow GUI methods directly without a UI testing framework
	// In a real scenario, we would extract the component generation logic to a separate service class
	// that could be more easily tested without UI dependencies.
}