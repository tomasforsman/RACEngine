using System.Reflection;

namespace Rac.Rendering.Shader;

/// <summary>
/// Utility class for loading shader source code from embedded or external files.
/// </summary>
public static class ShaderLoader
{
    /// <summary>
    /// Loads vertex shader source code.
    /// </summary>
    /// <returns>The vertex shader source code.</returns>
    public static string LoadVertexShader()
    {
        return LoadShaderFromFile("vertex.glsl");
    }

    /// <summary>
    /// Loads fragment shader source code for the specified shader mode.
    /// </summary>
    /// <param name="mode">The shader mode to load.</param>
    /// <returns>The fragment shader source code.</returns>
    public static string LoadFragmentShader(ShaderMode mode)
    {
        var filename = mode switch
        {
            ShaderMode.Normal => "normal.frag",
            ShaderMode.SoftGlow => "softglow.frag",
            ShaderMode.Bloom => "bloom.frag",
            _ => "normal.frag"
        };
        
        return LoadShaderFromFile(filename);
    }

    /// <summary>
    /// Loads shader source code from a file in the shader files directory.
    /// </summary>
    /// <param name="filename">The name of the shader file.</param>
    /// <returns>The shader source code.</returns>
    public static string LoadShaderFromFile(string filename)
    {
        // Get the directory where this assembly is located
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        
        // Look for shader files in the Shader/Files directory relative to the assembly
        var shaderPath = Path.Combine(assemblyDirectory!, "Shader", "Files", filename);
        
        // Fallback: try to find shader files relative to the source directory during development
        if (!File.Exists(shaderPath))
        {
            // Navigate up from bin/Debug/net8.0 to find the source directory
            var currentDir = assemblyDirectory!;
            for (int i = 0; i < 5; i++) // Try up to 5 levels up
            {
                var testPath = Path.Combine(currentDir, "Shader", "Files", filename);
                if (File.Exists(testPath))
                {
                    shaderPath = testPath;
                    break;
                }
                var parentDir = Path.GetDirectoryName(currentDir);
                if (parentDir == null) break;
                currentDir = parentDir;
            }
        }
        
        if (!File.Exists(shaderPath))
        {
            throw new FileNotFoundException($"Shader file not found: {filename}. Looked in: {shaderPath}");
        }
        
        return File.ReadAllText(shaderPath);
    }
}