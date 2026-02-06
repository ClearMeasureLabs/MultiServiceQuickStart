# NuGet Packaging Guide

This guide covers creating and testing NuGet packages for the ClearHostedService solution.

## Quick Start

### Create Packages

```powershell
# Windows (PowerShell)
.\build.ps1 -Pack

# Windows (CMD)
build.cmd --pack

# Linux/Mac
./build.sh --pack
```

### Custom Output Directory

```powershell
# PowerShell
.\build.ps1 -Pack -PackageOutputPath "C:\MyPackages"

# Bash
./build.sh --pack --package-output ~/mypackages
```

## Packages Created

The build script creates NuGet packages for the three library projects:

### 1. ClearMeasure.HostedService
**Purpose**: Core hosted service abstraction library  
**Package ID**: `ClearMeasure.HostedService`  
**Description**: Opinionated base implementation of IHostedService for building robust background services

### 2. ClearMeasure.HostedEndpoint
**Purpose**: NServiceBus endpoint integration  
**Package ID**: `ClearMeasure.HostedEndpoint`  
**Description**: Seamless integration between ClearHostedService and NServiceBus endpoints

### 3. ClearMeasure.HostedEndpoint.SqlServerTransport
**Purpose**: SQL Server transport extensions  
**Package ID**: `ClearMeasure.HostedEndpoint.SqlServerTransport`  
**Description**: SQL Server transport configuration with best practices for NServiceBus

## Package Contents

Each package includes:

- **Compiled assemblies** (.dll) - The library binaries
- **XML documentation** - IntelliSense documentation
- **Dependencies** - Referenced NuGet packages
- **Symbols** (Release builds) - Symbol packages (.snupkg) for debugging
- **Source code** (Release builds) - Source files for step-through debugging

## Package Metadata

Package metadata is defined in the `.csproj` files. Key properties:

```xml
<PropertyGroup>
  <PackageId>ClearMeasure.HostedService</PackageId>
  <Version>1.0.0</Version>
  <Authors>ClearMeasure Labs</Authors>
  <Company>ClearMeasure</Company>
  <Description>Opinionated base implementation of IHostedService</Description>
  <PackageTags>hosted-service;background-service;dotnet</PackageTags>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageProjectUrl>https://github.com/ClearMeasureLabs/MultiServiceQuickStart</PackageProjectUrl>
  <RepositoryUrl>https://github.com/ClearMeasureLabs/MultiServiceQuickStart</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

## Testing Packages Locally

Before publishing, test packages locally:

### Option 1: Local Package Source

```powershell
# 1. Create packages
.\build.ps1 -Pack

# 2. Add local package source
dotnet nuget add source ./artifacts/packages --name LocalPackages

# 3. Create a test project
mkdir test-project
cd test-project
dotnet new console

# 4. Add package from local source
dotnet add package ClearMeasure.HostedService --source LocalPackages

# 5. Verify the package works
# (Write code using the package)

# 6. Clean up - remove local source
dotnet nuget remove source LocalPackages
```

### Option 2: Direct Package Reference

In your test project's `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="ClearMeasure.HostedService" Version="1.0.0" />
</ItemGroup>

<ItemGroup>
  <None Include="..\artifacts\packages\ClearMeasure.HostedService.1.0.0.nupkg" 
        Pack="false" 
        Visible="false" />
</ItemGroup>
```

### Option 3: NuGet.Config in Solution

Create a `NuGet.Config` file in your test solution:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="LocalPackages" value="../ClearHostedService/artifacts/packages" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

## Package Versioning

Version numbers follow [Semantic Versioning](https://semver.org/):

**Format**: `MAJOR.MINOR.PATCH[-SUFFIX]`

- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)
- **SUFFIX**: Pre-release versions (e.g., `-alpha`, `-beta`, `-rc1`)

### Updating Version

Edit the `.csproj` file:

```xml
<PropertyGroup>
  <Version>1.2.0</Version>
</PropertyGroup>
```

### Version Suffix for Pre-Release

```xml
<PropertyGroup>
  <Version>1.2.0</Version>
  <VersionSuffix>beta1</VersionSuffix>
</PropertyGroup>
```

This creates version: `1.2.0-beta1`

## Build Configurations

### Debug Packages

```powershell
.\build.ps1 -Configuration Debug -Pack
```

- Larger package size
- Includes debug symbols
- No optimization
- Useful for debugging package issues

### Release Packages (Recommended)

```powershell
.\build.ps1 -Configuration Release -Pack
```

- Optimized code
- Separate symbol packages (.snupkg)
- Source link for debugging
- Ready for distribution

## Package Validation

Before publishing, validate packages:

### 1. Check Package Contents

```powershell
# Extract and inspect package contents
Expand-Archive -Path ./artifacts/packages/ClearMeasure.HostedService.1.0.0.nupkg -DestinationPath ./temp-extract

# View contents
tree ./temp-extract

# Clean up
Remove-Item ./temp-extract -Recurse -Force
```

### 2. Verify Dependencies

```powershell
# Install NuGet CLI
dotnet tool install -g nuget

# Inspect package
nuget spec ./artifacts/packages/ClearMeasure.HostedService.1.0.0.nupkg
```

### 3. Test Installation

```powershell
# Create test console app
dotnet new console -n PackageTest
cd PackageTest

# Add local package
dotnet add package ClearMeasure.HostedService --source ../artifacts/packages

# Build to ensure no errors
dotnet build
```

## Common Packaging Scenarios

### Scenario 1: Quick Package Test

```powershell
# Build and pack in one command (skip tests for speed)
.\build.ps1 -SkipTests -Pack

# Test immediately
dotnet add package ClearMeasure.HostedService --source ./artifacts/packages
```

### Scenario 2: Clean Package Build

```powershell
# Clean build with packages
.\build.ps1 -Clean -Pack

# This ensures no stale artifacts
```

### Scenario 3: Package All Configurations

```powershell
# Debug packages
.\build.ps1 -Configuration Debug -Pack -PackageOutputPath "./artifacts/packages-debug"

# Release packages
.\build.ps1 -Configuration Release -Pack -PackageOutputPath "./artifacts/packages-release"
```

## CI/CD Integration

### Azure Pipelines

```yaml
- task: PowerShell@2
  displayName: 'Create NuGet Packages'
  inputs:
    filePath: 'build.ps1'
    arguments: '-Pack -PackageOutputPath "$(Build.ArtifactStagingDirectory)"'
    pwsh: true

- task: PublishBuildArtifacts@1
  displayName: 'Publish Packages'
  inputs:
    PathtoPublish: '$(Build.ArtifactStagingDirectory)'
    ArtifactName: 'packages'
```

### GitHub Actions

```yaml
- name: Create Packages
  run: |
    chmod +x build.sh
    ./build.sh --pack --package-output ./packages

- name: Upload Packages
  uses: actions/upload-artifact@v3
  with:
    name: nuget-packages
    path: ./packages/*.nupkg
```

### GitLab CI

```yaml
build-packages:
  script:
    - chmod +x build.sh
    - ./build.sh --pack --package-output ./packages
  artifacts:
    paths:
      - packages/*.nupkg
    expire_in: 1 week
```

## Publishing Packages

Once validated, packages can be published to:

### NuGet.org (Public)

```powershell
# Get API key from nuget.org
# Then publish
dotnet nuget push ./artifacts/packages/ClearMeasure.HostedService.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

### Azure Artifacts (Private)

```powershell
# Configure Azure Artifacts source
dotnet nuget add source https://pkgs.dev.azure.com/YOUR_ORG/_packaging/YOUR_FEED/nuget/v3/index.json --name AzureArtifacts --username YOUR_USERNAME --password YOUR_PAT

# Push package
dotnet nuget push ./artifacts/packages/ClearMeasure.HostedService.1.0.0.nupkg --source AzureArtifacts
```

### GitHub Packages

```powershell
# Configure GitHub Packages
dotnet nuget add source https://nuget.pkg.github.com/OWNER/index.json --name github --username USERNAME --password GITHUB_TOKEN

# Push package
dotnet nuget push ./artifacts/packages/ClearMeasure.HostedService.1.0.0.nupkg --source github
```

## Troubleshooting

### Package Not Found

**Issue**: Local package not found when installing

**Solution**: Verify package output directory
```powershell
Get-ChildItem ./artifacts/packages -Filter *.nupkg
```

### Version Conflicts

**Issue**: Package version already exists

**Solution**: Update version in `.csproj` file

### Missing Dependencies

**Issue**: Package dependencies not included

**Solution**: Ensure `<PackageReference>` items in `.csproj` have proper settings

### Symbol Packages Not Created

**Issue**: No .snupkg files generated

**Solution**: Use Release configuration for symbol packages
```powershell
.\build.ps1 -Configuration Release -Pack
```

## Best Practices

1. **Always test locally** before publishing
2. **Use semantic versioning** for version numbers
3. **Include XML documentation** for IntelliSense
4. **Create symbol packages** for Release builds
5. **Validate package contents** before publishing
6. **Use local package sources** for testing
7. **Clean build** when creating final packages
8. **Version consistently** across all packages

## Package Output Structure

```
artifacts/
??? packages/
    ??? ClearMeasure.HostedService.1.0.0.nupkg
    ??? ClearMeasure.HostedService.1.0.0.snupkg
    ??? ClearMeasure.HostedEndpoint.1.0.0.nupkg
    ??? ClearMeasure.HostedEndpoint.1.0.0.snupkg
    ??? ClearMeasure.HostedEndpoint.SqlServerTransport.1.0.0.nupkg
    ??? ClearMeasure.HostedEndpoint.SqlServerTransport.1.0.0.snupkg
```

## Additional Resources

- [NuGet Package Creation](https://docs.microsoft.com/en-us/nuget/create-packages/overview-and-workflow)
- [Semantic Versioning](https://semver.org/)
- [Symbol Packages](https://docs.microsoft.com/en-us/nuget/create-packages/symbol-packages-snupkg)
- [Package Validation](https://docs.microsoft.com/en-us/nuget/reference/analyzers/nuget-analyzers)

---

**Last Updated**: 2026-02-04  
**Build Script Version**: 1.1.0
