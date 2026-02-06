# ClearHostedService Build Scripts

This document provides quick reference information about the build scripts.

## Quick Commands

### Windows (PowerShell) - Recommended
```powershell
# Build and test (Release)
.\build.ps1

# Build without tests
.\build.ps1 -SkipTests

# Build Debug configuration
.\build.ps1 -Configuration Debug

# Clean build
.\build.ps1 -Clean


# Verbose output
.\build.ps1 -VerboseOutput

# Create NuGet packages
.\build.ps1 -Pack

# Build and pack without tests (faster)
.\build.ps1 -SkipTests -Pack
```

### Windows (Batch)
```cmd
# Build and test (Release)
build.cmd

# Build without tests
build.cmd --skip-tests

# Build Debug configuration
build.cmd Debug

# Create NuGet packages
build.cmd --pack

# Build and pack without tests
build.cmd -s --pack
```

### Linux/Mac (Bash)
```bash
# Make executable (first time only)
chmod +x build.sh

# Build and test (Release)
./build.sh

# Build without tests
./build.sh --skip-tests

# Build Debug configuration
./build.sh -c Debug

# Create NuGet packages
./build.sh --pack

# Build and pack without tests
./build.sh -s --pack


# Clean build
./build.sh --clean
```

## What Gets Built

The build scripts build all projects in order:

1. **ClearMeasure.HostedService** - Core hosted service library
2. **ClearMeasure.HostedEndpoint** - NServiceBus endpoint integration
3. **ClearMeasure.HostedEndpoint.SqlServerTransport** - SQL Server transport extensions
4. **ClearHostedService.Tests** - Core library tests
5. **ClearHostedEndpoint.Tests** - Endpoint integration tests

## Exit Codes

- **0** - Success
- **1** - Build or test failure

## Test Results

Test results are saved to:
- `<Project>/TestResults/*.trx` - Visual Studio test results format

## Build Artifacts

Build outputs are in:
- `<Project>/bin/<Configuration>/net10.0/` - Compiled assemblies
- `<Project>/obj/` - Intermediate build files

## NuGet Packages

When using the pack option (`-Pack` or `--pack`), packages are created in:
- `./artifacts/packages/` - NuGet package files (*.nupkg and *.snupkg)

**Packages created**:
1. ClearMeasure.HostedService.{version}.nupkg
2. ClearMeasure.HostedEndpoint.{version}.nupkg
3. ClearMeasure.HostedEndpoint.SqlServerTransport.{version}.nupkg

### Testing Packages Locally

To test packages locally before publishing:

```powershell
# Create packages
.\build.ps1 -Pack

# Add local package source
dotnet nuget add source ./artifacts/packages --name LocalPackages

# Use in another project
dotnet add package ClearMeasure.HostedService --source LocalPackages

# Remove local source when done
dotnet nuget remove source LocalPackages
```

## Prerequisites

- .NET 10 SDK or later
- PowerShell 7+ (for build.ps1, works with PowerShell 5.1 on Windows)
- Bash (for build.sh on Linux/Mac)

## Common Issues

### "Project not found" warnings
Some projects may not exist in your workspace. This is normal if you don't have all projects.

### NuGet restore warnings
Azure DevOps feed warnings (NU1900) are informational and don't affect the build.

### Test failures
Run with verbose logging to diagnose:
```powershell
.\build.ps1 -VerboseOutput
```

## CI/CD Examples

### Azure Pipelines
```yaml
steps:
- task: PowerShell@2
  displayName: 'Build and Test'
  inputs:
    filePath: 'build.ps1'
    pwsh: true
```

### GitHub Actions
```yaml
steps:
- name: Build and Test
  run: |
    chmod +x build.sh
    ./build.sh
```

### GitLab CI
```yaml
build:
  script:
    - chmod +x build.sh
    - ./build.sh
```

## For More Information

See [BUILD.md](BUILD.md) for comprehensive documentation.
