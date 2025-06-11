using Rac.Rendering;
using System.Reflection;
using Xunit;

namespace Rac.Rendering.Tests;

public class IRendererTests
{
    [Fact]
    public void IRenderer_HasBasicUpdateVerticesMethod()
    {
        // Verify the original method still exists
        var method = typeof(IRenderer).GetMethod("UpdateVertices", new[] { typeof(float[]) });
        
        Assert.NotNull(method);
        Assert.Equal(typeof(void), method.ReturnType);
    }

    [Fact]
    public void IRenderer_HasGenericUpdateVerticesMethod()
    {
        // Verify the generic method exists with correct constraints
        var methods = typeof(IRenderer).GetMethods()
            .Where(m => m.Name == "UpdateVertices" && m.IsGenericMethod)
            .ToArray();
        
        Assert.Single(methods);
        
        var genericMethod = methods[0];
        Assert.True(genericMethod.IsGenericMethod);
        
        var genericParameters = genericMethod.GetGenericArguments();
        Assert.Single(genericParameters);
        
        var constraints = genericParameters[0].GetGenericParameterConstraints();
        Assert.Contains(constraints, t => t.Name == "ValueType"); // unmanaged constraint shows as ValueType
        
        var parameters = genericMethod.GetParameters();
        Assert.Single(parameters);
        Assert.True(parameters[0].ParameterType.IsArray);
    }

    [Fact]
    public void IRenderer_HasUpdateVerticesWithLayoutMethod()
    {
        // Verify the method with VertexLayout parameter exists
        var method = typeof(IRenderer).GetMethod("UpdateVertices", new[] { typeof(float[]), typeof(VertexLayout) });
        
        Assert.NotNull(method);
        Assert.Equal(typeof(void), method.ReturnType);
        
        var parameters = method.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(float[]), parameters[0].ParameterType);
        Assert.Equal(typeof(VertexLayout), parameters[1].ParameterType);
    }

    [Fact]
    public void IRenderer_HasThreeUpdateVerticesOverloads()
    {
        // Verify there are exactly 3 UpdateVertices methods in total
        var methods = typeof(IRenderer).GetMethods()
            .Where(m => m.Name == "UpdateVertices")
            .ToArray();
        
        Assert.Equal(3, methods.Length);
    }

    [Fact]
    public void OpenGLRenderer_ImplementsAllIRendererUpdateVerticesMethods()
    {
        // Verify OpenGLRenderer implements all the interface methods
        var interfaceType = typeof(IRenderer);
        var implementationType = typeof(OpenGLRenderer);
        
        Assert.True(interfaceType.IsAssignableFrom(implementationType));
        
        // Check each UpdateVertices method is implemented
        var interfaceMethods = interfaceType.GetMethods()
            .Where(m => m.Name == "UpdateVertices")
            .ToArray();
        
        foreach (var interfaceMethod in interfaceMethods)
        {
            var parameterTypes = interfaceMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            
            if (interfaceMethod.IsGenericMethod)
            {
                // For generic methods, we need to check differently
                var implementationMethods = implementationType.GetMethods()
                    .Where(m => m.Name == "UpdateVertices" && m.IsGenericMethod)
                    .ToArray();
                
                Assert.NotEmpty(implementationMethods);
            }
            else
            {
                var implementationMethod = implementationType.GetMethod("UpdateVertices", parameterTypes);
                Assert.NotNull(implementationMethod);
            }
        }
    }
}