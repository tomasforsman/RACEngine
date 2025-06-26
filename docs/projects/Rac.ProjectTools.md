Rac.ProjectTools Project Documentation
Project Overview
The Rac.ProjectTools project implements development utilities for the RACEngine using Avalonia UI framework. This project provides fully functional development tools to assist with RACEngine project development, currently featuring a component generation wizard for ECS development workflows.
Key Features

Component Generation Wizard: GUI application for creating ECS component files with proper structure and namespacing
Cross-Platform Development Tools: Avalonia-based UI supporting Windows, Linux, and macOS development environments
User-Friendly Interface: Simple wizard-style interface with validation and folder selection capabilities
Customizable Output: Configurable namespace and output directory selection for project organization

Current Implementation Status
✅ Fully Implemented: This project contains complete, functional implementations ready for development use.
Core Components
App: Avalonia application class providing application lifecycle management and window initialization. Implements standard Avalonia patterns with FluentTheme styling and desktop application lifetime management.
MainWindow: Primary application window implementing the component generation wizard interface. Provides input fields for component name and namespace, folder selection dialog, and generation logic with validation and user feedback.
Program: Application entry point with standard Avalonia configuration supporting cross-platform desktop deployment with proper threading and UI framework initialization.
Functionality Details
Component Generation: Creates properly structured ECS component files using C# record struct pattern with IComponent interface implementation. Supports customizable namespacing and validates user input to prevent invalid file generation.
User Interface: Modern Avalonia UI with FluentTheme styling providing intuitive wizard workflow. Includes real-time validation, folder browsing capabilities, and success/error dialog feedback for user experience.
File Management: Robust file system integration with proper path handling, directory validation, and safe file writing operations preventing data corruption or unauthorized access.
Usage Workflow

Launch Application: Start the component generation wizard
Enter Component Details: Specify component name and target namespace
Select Output Location: Browse and select target directory for file generation
Generate Component: Create properly structured component file with validation
Confirmation: Receive success confirmation with file location details

This tool streamlines ECS component development by automating boilerplate code generation and ensuring consistent component structure across RACEngine projects.