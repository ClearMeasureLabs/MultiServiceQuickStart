# Build Scripts

This directory contains build scripts for building and testing the ClearHostedService solution.

## Available Scripts

### PowerShell Script (`build.ps1`) - **Recommended for Windows**

The most feature-rich build script with colored output and comprehensive options.

**Basic usage:**
```powershell
.\build.ps1
```

**Options:**
- `-Configuration <Debug|Release>` - Build configuration (default: Release)
- `-SkipTests` - Skip running tests
- `-Clean` - Clean solution before building
- `-Pack` - Create NuGet packages for library projects
- `-PackageOutputPath <path>` - Output directory for packages (default: ./artifacts/packages)
- `-VerboseOutput` - Enable verbose output

**Examples:**
```powershell
# Build and test (Release)
.\build.ps1

# Build Debug configuration with clean
.\build.ps1 -Configuration Debug -Clean

# Build without running tests
.\build.ps1 -SkipTests

# Build with verbose output
.\build.ps1 -VerboseOutput

# Clean, build, and test in Debug mode
.\build.ps1 -Configuration Debug -Clean

# Create NuGet packages
.\build.ps1 -Pack

# Create packages with custom output directory
.\build.ps1 -Pack -PackageOutputPath "C:\MyPackages"

# Build and pack without tests (faster for package testing)
.\build.ps1 -SkipTests -Pack
```

### Batch Script (`build.cmd`) - **Simple Windows alternative**

Simplified batch file for users who prefer CMD.

**Basic usage:**
```cmd
build.cmd
```

**Options:**
- `Debug` or `Release` - Build configuration (default: Release)
- `--skip-tests` or `-s` - Skip running tests
- `--pack` or `-p` - Create NuGet packages

**Examples:**
```cmd
REM Build and test (Release)
build.cmd

REM Build Debug configuration
build.cmd Debug

REM Build without tests
build.cmd --skip-tests

REM Build Debug without tests
build.cmd Debug -s

REM Create NuGet packages
build.cmd --pack

REM Create packages without tests
build.cmd --pack -s
```

### Bash Script (`build.sh`) - **Linux/Mac**

Full-featured build script for Unix-based systems.

**Basic usage:**
```bash
chmod +x build.sh  # First time only
./build.sh
```

**Options:**
- `-c, --configuration <Debug|Release>` - Build configuration (default: Release)
- `-s, --skip-tests` - Skip running tests
- `--clean` - Clean solution before building
- `-p, --pack` - Create NuGet packages
- `--package-output <path>` - Package output directory (default: artifacts/packages)
- `-v, --verbose` - Enable verbose output
- `-h, --help` - Show help message

**Examples:**
```bash
# Build and test (Release)
./build.sh

# Build Debug configuration with clean
./build.sh -c Debug --clean

# Build without running tests
./build.sh --skip-tests

# Build with verbose output
./build.sh -v

# Show help
./build.sh --help

# Create NuGet packages
./build.sh --pack

# Create packages with custom output
./build.sh --pack --package-output ~/mypackages

# Build and pack without tests
./build.sh -s --pack
```

## What the Scripts Do

All build scripts perform the following steps:

1. **Check Prerequisites** - Verify .NET SDK is installed
2. **Restore** - Restore NuGet packages
3. **Build** - Compile all projects in the solution
4. **Test** - Run all unit tests (unless skipped)
5. **Pack** - Create NuGet packages (if requested)

### Projects Built

The solution includes the following projects:

- `ClearMeasure.HostedService` - Core hosted service abstraction (**packaged**)
- `ClearMeasure.HostedEndpoint` - NServiceBus endpoint integration (**packaged**)
- `ClearMeasure.HostedEndpoint.SqlServerTransport` - SQL Server transport extensions (**packaged**)
- `ClearHostedService.Tests` - Unit tests for hosted service
- `ClearHostedEndpoint.Tests` - Unit tests for endpoint

### NuGet Packages

When using the `-Pack` option, NuGet packages are created for the three library projects:

1. **ClearMeasure.HostedService** - Core library package
2. **ClearMeasure.HostedEndpoint** - Endpoint integration package
3. **ClearMeasure.HostedEndpoint.SqlServerTransport** - SQL Server transport package

**Package Output**: `./artifacts/packages/` (configurable with `-PackageOutputPath`)

**Package Contents**:
- Compiled assemblies (.dll)
- XML documentation
- Symbol packages (.snupkg) for Release builds
- Source files (for Release builds with symbols)

## Prerequisites

- **.NET 10 SDK** or later
- **PowerShell 7+** (for build.ps1, optional for Windows PowerShell 5.1)
- **Bash** (for build.sh on Linux/Mac)

## Quick Start

**Windows (PowerShell):**
```powershell
.\build.ps1
```

**Windows (CMD):**
```cmd
build.cmd
```

**Linux/Mac:**
```bash
chmod +x build.sh
./build.sh
```

## CI/CD Integration

These scripts are designed to work in CI/CD pipelines:

**Azure Pipelines:**
```yaml
- task: PowerShell@2
  displayName: 'Build and Test'
  inputs:
    filePath: 'build.ps1'
    arguments: '-Configuration Release'
    pwsh: true
```

**GitHub Actions:**
```yaml
- name: Build and Test
  run: |
    chmod +x build.sh
    ./build.sh --configuration Release
  shell: bash
```

**GitLab CI:**
```yaml
build:
  script:
    - chmod +x build.sh
    - ./build.sh -c Release
```

## Troubleshooting

### .NET SDK not found

**Error:** `.NET SDK not found`

**Solution:** Install the .NET 10 SDK from https://dotnet.microsoft.com/download

### Permission denied (Linux/Mac)

**Error:** `Permission denied: ./build.sh`

**Solution:** Make the script executable:
```bash
chmod +x build.sh
```

### Tests failing

If tests fail, you can:

1. Run tests manually to see detailed output:
   ```bash
   dotnet test --logger "console;verbosity=detailed"
   ```

2. Skip tests during build:
   ```bash
   ./build.ps1 -SkipTests
   ```

3. Run specific test projects:
   ```bash
   dotnet test ClearHostedService.Tests/ClearHostedService.Tests.csproj
   ```

## Manual Build Commands

If you prefer to run commands manually:

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release --no-build

# Clean solution
dotnet clean
```

## Additional Resources

- [.NET CLI Documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/)
- [Project Documentation](./Docs/ClearHostedService/README.md)
- [Contributing Guidelines](./Docs/ClearHostedService/CONTRIBUTING.md)
