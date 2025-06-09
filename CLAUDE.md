# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a NetDaemon v5 home automation application that runs on Home Assistant. NetDaemon allows writing Home Assistant automations in C# (.NET 9). The project defines automations for various areas (bathroom, bedroom, kitchen, living room, pantry) and includes device controls, security features, and smart climate management.

## Key Commands

### Build & Deploy
```bash
# Standard build
dotnet build

# Publish to release
dotnet publish -c Release

# Deploy to Home Assistant (PowerShell)
.\publish.ps1
```

### Code Generation
```bash
# Generate Home Assistant entities and services from metadata
nd-codegen
```

### Dependency Management
```bash
# Update all dependencies (PowerShell)
.\update_all_dependencies.ps1

# Update code generator
dotnet tool update -g NetDaemon.HassModel.CodeGen
```

## Architecture Overview

### Core Structure
- **`/apps`** - All automation applications
  - **`/Area`** - Room-specific automations (Bathroom, Bedroom, Kitchen, etc.)
  - **`/Common`** - Base classes and interfaces for all automations
  - **`/Security`** - Security-related automations (locks, location, notifications)
  - **`/Helpers`** - Constants and utility functions

### Key Base Classes
- **`AutomationBase`** - Abstract base for all automations with master switch support
- **`MotionAutomationBase`** - Base for motion-triggered automations
- **`DimmingMotionAutomationBase`** - Extended motion automation with dimming capabilities
- **`IAutomation`** - Interface all automations must implement

### Code Generation
The project uses NetDaemon's code generation to create strongly-typed entities and services:
- Generated code is in `HomeAssistantGenerated.cs`
- Metadata stored in `/NetDaemonCodegen/`
- Configuration in `appsettings.json` under `CodeGeneration` section

### Configuration
- **`appsettings.json`** - Main configuration (Home Assistant connection, logging)
- **`appsettings.Development.json`** - Development-specific settings (contains auth token)
- Home Assistant connection configured to `homeassistant.local:8123`

### Deployment
The `publish.ps1` script:
1. Stops the NetDaemon addon in Home Assistant
2. Publishes the project to the addon's config directory
3. Restarts the addon

### Development Notes
- Uses .NET 9 with C# 13
- Follows strict EditorConfig rules (see `.editorconfig`)
- Global usings defined in `apps/GlobalUsings.cs`
- All automations use reactive extensions (System.Reactive)
- Master switches control automation groups