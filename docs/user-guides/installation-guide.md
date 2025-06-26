---
title: "Installation Guide"
description: "Complete guide for installing and setting up RACEngine development environment"
version: "1.0.0"
last_updated: "2025-06-26"
author: "RACEngine Team"
tags: ["installation", "setup", "getting-started"]
---

# Installation Guide

## Overview

This guide will walk you through setting up RACEngine on your development machine. RACEngine supports Windows, macOS, and Linux, with cross-platform development using .NET 8.0.

## Prerequisites

### System Requirements

#### Minimum Requirements
- **Operating System**: Windows 10, macOS 10.15, or Ubuntu 18.04+ (or equivalent Linux distribution)
- **Memory**: 4 GB RAM
- **Storage**: 2 GB available space
- **Graphics**: OpenGL 3.3 compatible graphics card
- **Network**: Internet connection for downloading dependencies

#### Recommended Requirements
- **Operating System**: Windows 11, macOS 12+, or Ubuntu 20.04+
- **Memory**: 8 GB RAM or more
- **Storage**: 5 GB available space (includes development tools)
- **Graphics**: Dedicated graphics card with updated drivers
- **CPU**: Multi-core processor for faster compilation

## Step 1: Install .NET SDK

RACEngine requires .NET 8.0 SDK or later.

### Windows

#### Option 1: Download from Microsoft
1. Visit [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
2. Download .NET 8.0 SDK
3. Run the installer and follow the setup wizard
4. Restart your command prompt or terminal

#### Option 2: Using Package Managers
```powershell
# Using Chocolatey
choco install dotnet-8.0-sdk

# Using Winget
winget install Microsoft.DotNet.SDK.8

# Using Scoop
scoop install dotnet-sdk
```

### macOS

#### Option 1: Download from Microsoft
1. Visit [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
2. Download .NET 8.0 SDK for macOS
3. Run the .pkg installer
4. Restart your terminal

#### Option 2: Using Homebrew
```bash
# Install Homebrew if not already installed
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install .NET SDK
brew install --cask dotnet-sdk
```

### Linux (Ubuntu/Debian)

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Update package index
sudo apt update

# Install .NET SDK
sudo apt install -y dotnet-sdk-8.0
```

### Linux (Other Distributions)

For other Linux distributions, see the [official .NET installation guide](https://docs.microsoft.com/en-us/dotnet/core/install/linux).

### Verify Installation

```bash
# Check .NET version
dotnet --version
# Should output 8.0.x or later

# List installed SDKs
dotnet --list-sdks

# List installed runtimes
dotnet --list-runtimes
```

## Step 2: Install Development Tools

### Recommended: Visual Studio 2022

#### Windows
1. Download [Visual Studio 2022 Community](https://visualstudio.microsoft.com/downloads/) (free)
2. During installation, select these workloads:
   - **.NET desktop development**
   - **Game development with Unity** (optional, for additional game dev tools)
3. Individual components to add:
   - **Git for Windows**
   - **GitHub Extension for Visual Studio**

#### macOS
1. Download [Visual Studio 2022 for Mac](https://visualstudio.microsoft.com/vs/mac/)
2. Follow the installation wizard
3. Ensure .NET workload is selected

### Alternative: Visual Studio Code

Available for all platforms and completely free:

1. Download [Visual Studio Code](https://code.visualstudio.com/)
2. Install essential extensions:
   ```bash
   # Install VS Code extensions via command line
   code --install-extension ms-dotnettools.csharp
   code --install-extension ms-dotnettools.csdevkit
   code --install-extension ms-vscode.vscode-json
   code --install-extension eamodio.gitlens
   ```

### Alternative: JetBrains Rider

Professional IDE with excellent C# support:
1. Download [JetBrains Rider](https://www.jetbrains.com/rider/)
2. Free for students and open source projects
3. 30-day free trial for evaluation

## Step 3: Install Git

### Windows

#### Option 1: Git for Windows
1. Download from [https://git-scm.com/download/win](https://git-scm.com/download/win)
2. Run installer with default settings
3. Choose your preferred editor (VS Code recommended)

#### Option 2: Package Manager
```powershell
# Using Chocolatey
choco install git

# Using Winget
winget install Git.Git
```

### macOS

```bash
# Using Homebrew (recommended)
brew install git

# Or install Xcode Command Line Tools
xcode-select --install
```

### Linux

```bash
# Ubuntu/Debian
sudo apt install git

# CentOS/RHEL/Fedora
sudo yum install git
# or
sudo dnf install git
```

### Configure Git

```bash
# Set your name and email
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"

# Set default branch name
git config --global init.defaultBranch main

# Optional: Set VS Code as default editor
git config --global core.editor "code --wait"
```

## Step 4: Install Graphics Dependencies

RACEngine uses OpenGL for rendering. Most systems have this pre-installed, but you may need graphics drivers.

### Windows

1. **Update Graphics Drivers**:
   - **NVIDIA**: Download from [NVIDIA Driver Downloads](https://www.nvidia.com/drivers/)
   - **AMD**: Download from [AMD Driver Downloads](https://www.amd.com/support)
   - **Intel**: Use Windows Update or download from [Intel Driver Downloads](https://www.intel.com/content/www/us/en/support/products/80939/graphics.html)

2. **Install Visual C++ Redistributables** (if not already installed):
   ```powershell
   # Using Chocolatey
   choco install vcredist140

   # Or download from Microsoft
   # https://docs.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist
   ```

### macOS

Graphics drivers are typically up-to-date through system updates:
```bash
# Check for system updates
sudo softwareupdate -i -a
```

### Linux

```bash
# Ubuntu/Debian - Install OpenGL development libraries
sudo apt install -y libgl1-mesa-dev libglu1-mesa-dev freeglut3-dev

# For NVIDIA graphics cards
sudo apt install nvidia-driver-XXX  # Replace XXX with appropriate version

# For AMD graphics cards
sudo apt install mesa-vulkan-drivers

# For Intel graphics
sudo apt install intel-media-va-driver
```

## Step 5: Clone RACEngine Repository

### Option 1: Clone via Git Command Line

```bash
# Clone the repository
git clone https://github.com/tomasforsman/RACEngine.git

# Navigate to the directory
cd RACEngine

# Verify the clone
ls -la
```

### Option 2: Clone via Visual Studio

1. Open Visual Studio 2022
2. Click "Clone a repository"
3. Enter repository URL: `https://github.com/tomasforsman/RACEngine.git`
4. Choose local path and click "Clone"

### Option 3: Download ZIP (Not Recommended for Development)

1. Visit [https://github.com/tomasforsman/RACEngine](https://github.com/tomasforsman/RACEngine)
2. Click "Code" → "Download ZIP"
3. Extract to your desired location

## Step 6: Build and Test RACEngine

### Initial Build

```bash
# Navigate to RACEngine directory
cd RACEngine

# Restore NuGet packages
dotnet restore

# Build the entire solution
dotnet build

# Build in Release mode for better performance
dotnet build --configuration Release
```

### Run Sample Games

Test your installation by running the sample games:

```bash
# Navigate to sample game directory
cd samples/SampleGame

# Run different samples
dotnet run -- boidsample
dotnet run -- shootersample
dotnet run -- bloomtest
dotnet run -- camerademo
dotnet run -- pipelinedemo
```

### Expected Output

If everything is installed correctly, you should see:

1. **Console Output**:
   ```
   🚀 RACEngine Sample Game
   📊 Graphics API: OpenGL 3.3+
   🎮 Controls: [specific to each sample]
   ```

2. **Graphics Window**: A window opens showing the sample game running smoothly

3. **No Error Messages**: Console should be free of critical errors

## Step 7: IDE Setup and Configuration

### Visual Studio 2022 Configuration

1. **Open the Solution**:
   - File → Open → Project/Solution
   - Navigate to `RACEngine.sln`

2. **Configure Startup Project**:
   - Right-click on `SampleGame` in Solution Explorer
   - Select "Set as Startup Project"

3. **Build Configuration**:
   - Set to "Debug" for development
   - Set to "Release" for performance testing

### Visual Studio Code Configuration

1. **Open Workspace**:
   ```bash
   cd RACEngine
   code .
   ```

2. **Install Recommended Extensions** (VS Code will prompt):
   - C# Dev Kit
   - GitLens
   - C# Extensions

3. **Configure Launch Settings**:
   VS Code will automatically detect the .NET projects and create appropriate launch configurations.

### Debugging Configuration

#### Visual Studio 2022
- F5: Start with debugging
- Ctrl+F5: Start without debugging
- Breakpoints work out of the box

#### Visual Studio Code
1. Go to Run and Debug view (Ctrl+Shift+D)
2. Select appropriate launch configuration
3. Press F5 to start debugging

## Step 8: Performance Optimization

### Development Performance

```bash
# Enable faster builds during development
export DOTNET_CLI_TELEMETRY_OPTOUT=1

# Use local NuGet cache
dotnet nuget locals all --list
```

### Graphics Performance

1. **Update Graphics Drivers**: Ensure you have the latest drivers
2. **Check OpenGL Version**:
   ```bash
   # The sample games will display OpenGL version in console
   dotnet run -- boidsample
   ```
3. **Monitor Performance**: Sample games show FPS and render statistics

## Troubleshooting

### Common Issues

#### .NET SDK Not Found
```bash
# Error: The command "dotnet" was not found
# Solution: Ensure .NET SDK is in your PATH
echo $PATH  # Linux/macOS
echo %PATH% # Windows

# Re-run installer or manually add to PATH
```

#### Build Errors
```bash
# Clear NuGet cache and rebuild
dotnet nuget locals all --clear
dotnet clean
dotnet restore
dotnet build
```

#### Graphics Issues
```bash
# Check OpenGL support
# Linux: Install mesa-utils
sudo apt install mesa-utils
glxinfo | grep "OpenGL version"

# Windows: Check with sample games console output
# macOS: OpenGL should be available by default
```

#### Git Authentication Issues
```bash
# Configure Git credentials
git config --global credential.helper store

# Or use GitHub CLI
gh auth login
```

### Platform-Specific Issues

#### Windows
- **Antivirus Interference**: Add RACEngine folder to antivirus exclusions
- **Windows Defender**: May flag false positives; add exclusions as needed
- **Path Length Limits**: Enable long path support in Windows 10/11

#### macOS
- **Gatekeeper**: You may need to allow unsigned applications
- **Xcode Tools**: Ensure command line tools are installed
- **Permissions**: Grant necessary permissions for graphics access

#### Linux
- **Missing Dependencies**: Install development packages for your distribution
- **Graphics Drivers**: Ensure proper drivers for your GPU
- **Permissions**: May need to add user to graphics group

### Performance Issues

#### Slow Compilation
```bash
# Use multiple CPU cores for compilation
dotnet build --verbosity minimal --nologo /maxcpucount

# Or set environment variable
export DOTNET_CLI_TELEMETRY_OPTOUT=1
```

#### Low Frame Rate
1. Ensure graphics drivers are updated
2. Close unnecessary applications
3. Use Release build for performance testing
4. Check GPU compatibility with OpenGL 3.3+

### Getting Additional Help

#### Resources
- [RACEngine Documentation](../README.md)
- [.NET Installation Troubleshooting](https://docs.microsoft.com/en-us/dotnet/core/install/troubleshoot)
- [OpenGL Compatibility Check](https://opengl.gpuinfo.org/)

#### Community Support
- **GitHub Issues**: [Report problems](https://github.com/tomasforsman/RACEngine/issues)
- **GitHub Discussions**: [Ask questions](https://github.com/tomasforsman/RACEngine/discussions)

## Next Steps

After successful installation:

1. **Complete the Tutorial**: Follow the [Getting Started Tutorial](../educational-material/getting-started-tutorial.md)
2. **Explore Sample Games**: Study the implementation of different samples
3. **Read Architecture Docs**: Understand the engine's design in [Architecture Documentation](../architecture/)
4. **Create Your First Project**: Use [Project Setup Guide](project-setup.md)

## Verification Checklist

Before proceeding, ensure all of the following work:

- [ ] `dotnet --version` shows 8.0.x or later
- [ ] `git --version` shows a recent version
- [ ] RACEngine builds without errors (`dotnet build`)
- [ ] Sample games run and display graphics (`dotnet run -- boidsample`)
- [ ] Your IDE can open and build the solution
- [ ] Graphics performance is acceptable (>30 FPS in samples)
- [ ] No critical error messages in console output

## Changelog

- 2025-06-26: Comprehensive installation guide covering all major platforms with troubleshooting section