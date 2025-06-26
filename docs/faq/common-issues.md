---
title: "Common Issues and Solutions"
description: "Frequently encountered problems and their solutions when working with RACEngine"
version: "1.0.0"
last_updated: "2025-06-26"
author: "RACEngine Team"
tags: ["troubleshooting", "faq", "problems", "solutions"]
---

# Common Issues and Solutions

## Overview

This document addresses the most frequently encountered issues when working with RACEngine, providing step-by-step solutions and preventive measures. Issues are organized by category for easy navigation.

## Prerequisites

- Basic understanding of C# and .NET development
- RACEngine successfully installed (see [Installation Guide](../user-guides/installation-guide.md))
- Familiarity with debugging tools and techniques

## Build and Compilation Issues

### Issue: "SDK Not Found" or "Framework Not Found"

**Symptoms:**
- Error: `The specified framework 'Microsoft.NETCore.App', version 'X.X.X' was not found`
- Build fails with SDK-related errors
- `dotnet --version` command not recognized

**Solutions:**

1. **Verify .NET Installation:**
   ```bash
   # Check installed SDKs
   dotnet --list-sdks
   
   # Check installed runtimes
   dotnet --list-runtimes
   
   # Expected: .NET 8.0.x or later
   ```

2. **Reinstall .NET SDK:**
   ```bash
   # Windows (using chocolatey)
   choco uninstall dotnet-sdk
   choco install dotnet-8.0-sdk
   
   # macOS (using homebrew)
   brew uninstall --cask dotnet-sdk
   brew install --cask dotnet-sdk
   
   # Linux (Ubuntu)
   sudo apt remove dotnet-sdk-8.0
   sudo apt install dotnet-sdk-8.0
   ```

3. **Fix PATH Issues:**
   ```bash
   # Windows - Add to PATH
   setx PATH "%PATH%;C:\Program Files\dotnet"
   
   # macOS/Linux - Add to shell profile
   echo 'export PATH="$PATH:/usr/local/share/dotnet"' >> ~/.bashrc
   source ~/.bashrc
   ```

### Issue: "NuGet Package Restore Failed"

**Symptoms:**
- Error: `Package 'PackageName' was not found`
- Missing references in IDE
- Build succeeds but runtime errors occur

**Solutions:**

1. **Clear and Restore Packages:**
   ```bash
   # Clear all NuGet caches
   dotnet nuget locals all --clear
   
   # Restore packages
   dotnet restore
   
   # Clean and rebuild
   dotnet clean
   dotnet build
   ```

2. **Check Package Sources:**
   ```bash
   # List configured sources
   dotnet nuget list source
   
   # Add official NuGet source if missing
   dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
   ```

3. **Fix Corporate Network Issues:**
   ```bash
   # If behind corporate firewall
   dotnet nuget add source [internal-nuget-url] -n corporate
   
   # Clear HTTP cache if proxy issues
   dotnet nuget locals http-cache --clear
   ```

### Issue: "Project Reference Cannot Be Loaded"

**Symptoms:**
- Error: `The project file 'path' does not exist`
- Some projects show as unloaded in IDE
- Circular dependency warnings

**Solutions:**

1. **Verify Project Paths:**
   ```xml
   <!-- Check .csproj file for correct relative paths -->
   <ProjectReference Include="..\..\src\Rac.Core\Rac.Core.csproj" />
   ```

2. **Fix Circular Dependencies:**
   ```bash
   # Analyze project dependencies
   dotnet list package --include-transitive
   
   # Check for circular references in solution structure
   ```

3. **Rebuild Solution Structure:**
   ```bash
   # Remove obj and bin directories
   find . -name "obj" -type d -exec rm -rf {} +
   find . -name "bin" -type d -exec rm -rf {} +
   
   # Rebuild from scratch
   dotnet restore
   dotnet build
   ```

## Runtime and Execution Issues

### Issue: "OpenGL Context Creation Failed"

**Symptoms:**
- Window opens but remains black
- Error: `Failed to create OpenGL context`
- Graphics not rendering

**Solutions:**

1. **Update Graphics Drivers:**
   ```bash
   # Windows - Update through Device Manager or manufacturer website
   # NVIDIA: https://www.nvidia.com/drivers/
   # AMD: https://www.amd.com/support/
   # Intel: Windows Update or Intel Driver Assistant
   
   # Linux - Update graphics drivers
   sudo apt update && sudo apt upgrade
   
   # For NVIDIA
   sudo apt install nvidia-driver-XXX
   
   # For AMD
   sudo apt install mesa-vulkan-drivers
   ```

2. **Check OpenGL Support:**
   ```bash
   # Linux - Check OpenGL version
   sudo apt install mesa-utils
   glxinfo | grep "OpenGL version"
   
   # Should show 3.3 or higher
   ```

3. **Run with Software Rendering (Fallback):**
   ```bash
   # Force software rendering if hardware acceleration fails
   export LIBGL_ALWAYS_SOFTWARE=1
   dotnet run
   ```

### Issue: "Audio System Initialization Failed"

**Symptoms:**
- Warning: `Audio device not found`
- No sound output
- Audio-related exceptions

**Solutions:**

1. **Check Audio Devices:**
   ```bash
   # Windows - Check audio devices in Control Panel
   # Linux - List audio devices
   aplay -l
   
   # macOS - Check System Preferences > Sound
   ```

2. **Install Audio Dependencies:**
   ```bash
   # Linux - Install ALSA development libraries
   sudo apt install libasound2-dev
   
   # Or install PulseAudio if using PulseAudio
   sudo apt install pulseaudio-dev
   ```

3. **Use Null Audio Service (Headless Mode):**
   ```csharp
   // In your game code, use null audio service for headless scenarios
   var audioService = new NullAudioService();
   // This allows the engine to run without audio hardware
   ```

### Issue: "File Not Found" for Assets

**Symptoms:**
- Runtime errors about missing shader files
- Textures not loading
- Audio files not found

**Solutions:**

1. **Verify Asset Paths:**
   ```bash
   # Check current working directory
   pwd
   
   # List shader files
   find . -name "*.glsl" -o -name "*.frag" -o -name "*.vert"
   
   # Verify relative paths match code expectations
   ```

2. **Fix Working Directory:**
   ```bash
   # Run from correct directory
   cd samples/SampleGame
   dotnet run
   
   # Or set working directory in IDE launch settings
   ```

3. **Copy Assets to Output:**
   ```xml
   <!-- Add to .csproj to copy assets -->
   <ItemGroup>
     <None Include="assets/**/*">
       <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
     </None>
   </ItemGroup>
   ```

## Performance Issues

### Issue: "Low Frame Rate" or "Stuttering"

**Symptoms:**
- Frame rate below 30 FPS
- Irregular frame timing
- Visual stuttering or lag

**Solutions:**

1. **Check Build Configuration:**
   ```bash
   # Ensure Release build for performance testing
   dotnet build --configuration Release
   dotnet run --configuration Release
   ```

2. **Monitor Performance:**
   ```csharp
   // Add performance monitoring to your game
   var frameTime = stopwatch.ElapsedMilliseconds;
   if (frameTime > 16) // 60 FPS = 16.67ms per frame
   {
       Console.WriteLine($"Slow frame: {frameTime}ms");
   }
   ```

3. **Optimize Entity Count:**
   ```bash
   # Check entity count in sample games
   # Reduce number of entities if system can't handle load
   
   # In boid sample, reduce flock size
   # In particle systems, reduce particle count
   ```

4. **Update Graphics Drivers:**
   - See graphics driver update instructions above

### Issue: "High Memory Usage" or "Memory Leaks"

**Symptoms:**
- Memory usage continuously increases
- Out of memory exceptions
- Garbage collection pauses

**Solutions:**

1. **Profile Memory Usage:**
   ```bash
   # Use dotMemory, JetBrains profiler, or Visual Studio Diagnostics
   # Look for object retention and allocation patterns
   ```

2. **Check Entity Cleanup:**
   ```csharp
   // Ensure entities are properly destroyed
   foreach (var entityToRemove in entitiesToRemove)
   {
       world.DestroyEntity(entityToRemove);
   }
   ```

3. **Implement Object Pooling:**
   ```csharp
   // Use object pools for frequently created/destroyed objects
   var bulletPool = new ObjectPool<Bullet>(maxSize: 1000);
   ```

## IDE and Development Issues

### Issue: "IntelliSense Not Working"

**Symptoms:**
- No code completion
- Missing syntax highlighting
- Error squiggles not appearing

**Solutions:**

1. **Restart IDE Language Service:**
   ```bash
   # Visual Studio Code
   Ctrl+Shift+P → "Developer: Reload Window"
   
   # Visual Studio 2022
   Tools → Options → Text Editor → C# → IntelliSense → Clear Cache
   ```

2. **Rebuild Project:**
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```

3. **Check Extension Installation:**
   ```bash
   # VS Code - Verify C# extension is installed and up to date
   code --list-extensions | grep ms-dotnettools.csharp
   ```

### Issue: "Debugger Not Attaching"

**Symptoms:**
- Breakpoints not hit
- Debug symbols not loaded
- Cannot inspect variables

**Solutions:**

1. **Check Debug Configuration:**
   ```json
   // .vscode/launch.json
   {
       "type": "coreclr",
       "request": "launch",
       "program": "${workspaceFolder}/samples/SampleGame/bin/Debug/net8.0/SampleGame.dll",
       "cwd": "${workspaceFolder}/samples/SampleGame",
       "console": "internalConsole"
   }
   ```

2. **Verify Debug Build:**
   ```bash
   # Ensure Debug configuration generates symbols
   dotnet build --configuration Debug
   
   # Check for .pdb files
   ls bin/Debug/net8.0/*.pdb
   ```

3. **Clean and Rebuild:**
   ```bash
   dotnet clean
   dotnet build --configuration Debug
   ```

## Platform-Specific Issues

### Windows-Specific Issues

#### Issue: "Windows Defender Blocking Execution"

**Solutions:**
```powershell
# Add exclusion for RACEngine directory
Add-MpPreference -ExclusionPath "C:\path\to\RACEngine"

# Or temporarily disable real-time protection for testing
```

#### Issue: "Long Path Issues"

**Solutions:**
```powershell
# Enable long path support (Windows 10/11)
New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" -Name "LongPathsEnabled" -Value 1 -PropertyType DWORD -Force
```

### macOS-Specific Issues

#### Issue: "Code Signing Issues"

**Solutions:**
```bash
# Allow unsigned applications
sudo spctl --master-disable

# Or sign the application (for distribution)
codesign --force --deep --sign - path/to/application
```

#### Issue: "Permission Denied"

**Solutions:**
```bash
# Fix permissions on the directory
chmod -R 755 RACEngine/

# Make scripts executable
chmod +x scripts/*.sh
```

### Linux-Specific Issues

#### Issue: "Missing Development Libraries"

**Solutions:**
```bash
# Ubuntu/Debian - Install common development packages
sudo apt update
sudo apt install build-essential git cmake

# Install graphics development libraries
sudo apt install libgl1-mesa-dev libglu1-mesa-dev freeglut3-dev

# Install audio development libraries
sudo apt install libasound2-dev libpulse-dev
```

#### Issue: "X11 or Wayland Display Issues"

**Solutions:**
```bash
# Check display environment
echo $DISPLAY
echo $WAYLAND_DISPLAY

# For X11 issues
xhost +local:

# For Wayland issues
export GDK_BACKEND=wayland
```

## Networking and Security Issues

### Issue: "Package Download Failed"

**Symptoms:**
- SSL/TLS errors during package restore
- Proxy authentication failures
- Network timeouts

**Solutions:**

1. **Configure Proxy Settings:**
   ```bash
   # Set proxy for dotnet
   dotnet nuget add source https://api.nuget.org/v3/index.json \
     --username [username] --password [password]
   
   # Or configure system proxy
   export HTTP_PROXY=http://proxy.company.com:8080
   export HTTPS_PROXY=http://proxy.company.com:8080
   ```

2. **Fix SSL Certificate Issues:**
   ```bash
   # Trust development certificates
   dotnet dev-certs https --trust
   
   # Clear certificate cache if corrupted
   dotnet dev-certs https --clean
   dotnet dev-certs https --trust
   ```

## Getting Help

### When to Seek Additional Help

If the solutions above don't resolve your issue, consider:

1. **Check GitHub Issues:** [RACEngine Issues](https://github.com/tomasforsman/RACEngine/issues)
2. **Search Documentation:** Use site search for specific error messages
3. **Community Discussion:** [GitHub Discussions](https://github.com/tomasforsman/RACEngine/discussions)

### Information to Include When Reporting Issues

```
- Operating System and version
- .NET SDK version (dotnet --version)
- RACEngine version/commit hash
- Exact error message
- Steps to reproduce
- Expected vs actual behavior
- Relevant logs or stack traces
```

### Debug Information Collection

```bash
# Collect system information
dotnet --info > debug-info.txt

# Collect verbose build output
dotnet build --verbosity diagnostic > build-log.txt 2>&1

# Check OpenGL information (if applicable)
# Linux:
glxinfo > opengl-info.txt

# Collect Git status
git status > git-status.txt
git log --oneline -10 >> git-status.txt
```

## Prevention and Best Practices

### Environment Maintenance

1. **Keep Dependencies Updated:**
   ```bash
   # Regular update schedule
   dotnet list package --outdated
   dotnet tool update --global dotnet-ef
   ```

2. **Clean Builds Regularly:**
   ```bash
   # Weekly cleanup routine
   dotnet clean
   dotnet nuget locals all --clear
   dotnet restore
   ```

3. **Monitor System Resources:**
   - Keep adequate free disk space (>10GB recommended)
   - Monitor memory usage during development
   - Close unnecessary applications while developing

### Development Practices

1. **Use Version Control Effectively:**
   ```bash
   # Commit frequently with descriptive messages
   git add .
   git commit -m "Fix audio initialization for headless mode"
   
   # Use branches for experimental features
   git checkout -b feature/new-feature
   ```

2. **Test on Multiple Platforms:**
   - Regularly test builds on target platforms
   - Use CI/CD for automated cross-platform testing
   - Verify functionality on different hardware configurations

## See Also

- [Installation Guide](../user-guides/installation-guide.md) - Initial setup instructions
- [Platform-Specific Notes](platform-specific-notes.md) - Platform-specific considerations
- [Performance Troubleshooting](performance-troubleshooting.md) - Performance optimization
- [Problem Solving](problem-solving.md) - Systematic debugging approach

## Changelog

- 2025-06-26: Comprehensive common issues guide covering build, runtime, performance, and platform-specific problems