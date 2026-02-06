#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Private build script for ClearHostedService solution
.DESCRIPTION
    Builds and tests the entire solution with optional configurations
.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release
.PARAMETER SkipTests
    Skip running tests
.PARAMETER Clean
    Clean before build
.PARAMETER Pack
    Create NuGet packages for library projects
.PARAMETER PackageOutputPath
    Output directory for NuGet packages. Default: ./artifacts/packages
.PARAMETER VerboseOutput
    Enable verbose output
.PARAMETER EnableCodeCoverage
    Enable code coverage collection during tests
.PARAMETER GenerateTestReport
    Generate detailed test reports
.PARAMETER TestOnly
    Only run tests (skip build)
.PARAMETER PackOnly
    Only create packages (skip build and tests)
.EXAMPLE
    .\build.ps1
.EXAMPLE
    .\build.ps1 -Configuration Debug -Clean
.EXAMPLE
    .\build.ps1 -SkipTests
.EXAMPLE
    .\build.ps1 -Pack
.EXAMPLE
    .\build.ps1 -Pack -PackageOutputPath "C:\packages"
.EXAMPLE
    .\build.ps1 -EnableCodeCoverage -GenerateTestReport
.EXAMPLE
    .\build.ps1 -TestOnly -EnableCodeCoverage
#>

[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [switch]$SkipTests,
    
    [switch]$Clean,
    
    [switch]$Pack,
    
    [string]$PackageOutputPath,
    
    [switch]$VerboseOutput,
    
    [switch]$EnableCodeCoverage,
    
    [switch]$GenerateTestReport,
    
    [switch]$TestOnly,
    
    [switch]$PackOnly
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

# Set console to UTF-8 for proper Unicode character display
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

# Script variables
$scriptPath = if ($PSScriptRoot) { $PSScriptRoot } else { Get-Location }
$solutionName = "ClearHostedService"
$testResultsPath = Join-Path $scriptPath "TestResults"

# Set default package output path if not provided
if (-not $PackageOutputPath) {
    $PackageOutputPath = Join-Path $scriptPath "artifacts\packages"
}

function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host ""
    Write-BoxedMessage -Message $Message -Color Yellow
}

function Write-BoxedMessage {
    param(
        [string]$Message,
        [ConsoleColor]$Color = 'Cyan'
    )
    
    $padding = 8
    $lineLength = $Message.Length + ($padding * 2)
    
    $topLine = "╔" + ("═" * $lineLength) + "╗"
    $middleLine = "║" + (" " * $padding) + $Message + (" " * $padding) + "║"
    $bottomLine = "╚" + ("═" * $lineLength) + "╝"
    
    Write-Host $topLine -ForegroundColor $Color
    Write-Host $middleLine -ForegroundColor $Color
    Write-Host $bottomLine -ForegroundColor $Color
}


function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "→ $Message" -ForegroundColor Blue
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}


function Get-DotNetVersion {
    try {
        $dotnetVersion = dotnet --version
        return $dotnetVersion
    }
    catch {
        return $null
    }
}

function Test-Prerequisites {
    Write-Header "Checking Prerequisites"
    
    # Check for .NET SDK
    $dotnetVersion = Get-DotNetVersion
    if (-not $dotnetVersion) {
        Write-Error ".NET SDK not found. Please install .NET 10 SDK or later."
        exit 1
    }
    
    Write-Success ".NET SDK version: $dotnetVersion"
    
    # Verify we're in the correct directory
    $projects = @(
        "ClearHostedService\ClearMeasure.HostedService.csproj",
        "ClearHostedEndpoint\ClearMeasure.HostedEndpoint.csproj",
        "ClearHostedService.Tests\ClearHostedService.Tests.csproj",
        "ClearHostedEndpoint.Tests\ClearHostedEndpoint.Tests.csproj",
        "ClearHostedEndpoint.SqlServerTransport\ClearMeasure.HostedEndpoint.SqlServerTransport.csproj"
    )
    
    $allProjectsExist = $true
    foreach ($project in $projects) {
        if (-not (Test-Path (Join-Path $scriptPath $project))) {
            Write-Warning "Project not found: $project"
            $allProjectsExist = $false
        }
    }
    
    if (-not $allProjectsExist) {
        Write-Warning "Some projects are missing. Continuing anyway..."
    } else {
        Write-Success "All expected projects found"
    }
}

function Invoke-Clean {
    Write-Header "Cleaning Solution"
    
    try {
        # Clean all projects
        $projects = Get-ChildItem -Path $scriptPath -Recurse -Filter "*.csproj"
        
        foreach ($project in $projects) {
            Write-Info "Cleaning $($project.BaseName)..."
            dotnet clean $project.FullName --configuration $Configuration --verbosity quiet
            
            # Remove bin and obj directories
            $projectDir = $project.Directory.FullName
            $binPath = Join-Path $projectDir "bin"
            $objPath = Join-Path $projectDir "obj"
            
            if (Test-Path $binPath) {
                Remove-Item $binPath -Recurse -Force -ErrorAction SilentlyContinue
            }
            if (Test-Path $objPath) {
                Remove-Item $objPath -Recurse -Force -ErrorAction SilentlyContinue
            }
        }
        
        Write-Success "Clean completed successfully"
    }
    catch {
        Write-Error "Clean failed: $_"
        exit 1
    }
}

function Invoke-Restore {
Write-Header "Restoring NuGet Packages"
    
try {
    $verbosityLevel = if ($VerboseOutput) { "normal" } else { "minimal" }
        
    # Restore each project individually in order
    $projects = @(
        "ClearHostedService\ClearMeasure.HostedService.csproj",
        "ClearHostedEndpoint\ClearMeasure.HostedEndpoint.csproj",
        "ClearHostedEndpoint.SqlServerTransport\ClearMeasure.HostedEndpoint.SqlServerTransport.csproj",
        "ClearHostedService.Tests\ClearHostedService.Tests.csproj",
        "ClearHostedEndpoint.Tests\ClearHostedEndpoint.Tests.csproj"
    )
        
    foreach ($project in $projects) {
        $projectPath = Join-Path $scriptPath $project
        if (Test-Path $projectPath) {
            Write-Info "Restoring $(Split-Path $project -Leaf)"
            dotnet restore $projectPath --verbosity $verbosityLevel
                
            if ($LASTEXITCODE -ne 0) {
                throw "Restore failed for $project with exit code $LASTEXITCODE"
            }
        }
        else {
            Write-Warning "Project not found: $project"
        }
    }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Restore failed with exit code $LASTEXITCODE"
        }
        
        Write-Success "Restore completed successfully"
    }
    catch {
        Write-Error "Restore failed: $_"
        exit 1
    }
}

function Invoke-Build {
Write-Header "Building Solution ($Configuration)"
    
try {
    $verbosityLevel = if ($VerboseOutput) { "normal" } else { "minimal" }
        
    # Build each project individually in order
    $projects = @(
        "ClearHostedService\ClearMeasure.HostedService.csproj",
        "ClearHostedEndpoint\ClearMeasure.HostedEndpoint.csproj",
        "ClearHostedEndpoint.SqlServerTransport\ClearMeasure.HostedEndpoint.SqlServerTransport.csproj",
        "ClearHostedService.Tests\ClearHostedService.Tests.csproj",
        "ClearHostedEndpoint.Tests\ClearHostedEndpoint.Tests.csproj"
    )
        
    foreach ($project in $projects) {
        $projectPath = Join-Path $scriptPath $project
        if (Test-Path $projectPath) {
            Write-Info "Building $(Split-Path $project -Leaf)"
                
            $buildArgs = @(
                "build"
                $projectPath
                "--configuration", $Configuration
                "--no-restore"
                "--verbosity", $verbosityLevel
            )
                
            dotnet @buildArgs
                
            if ($LASTEXITCODE -ne 0) {
                throw "Build failed for $project with exit code $LASTEXITCODE"
            }
        }
        else {
            Write-Warning "Project not found: $project"
        }
    }
        
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed with exit code $LASTEXITCODE"
        }
        
        Write-Success "Build completed successfully"
    }
    catch {
        Write-Error "Build failed: $_"
        exit 1
    }
}

function Invoke-Test {
Write-Header "Running Tests"
    
try {
    $verbosityLevel = if ($VerboseOutput) { "normal" } else { "minimal" }
        
    # Ensure TestResults directory exists
    if (Test-Path $testResultsPath) {
        Remove-Item $testResultsPath -Recurse -Force -ErrorAction SilentlyContinue
    }
    New-Item -ItemType Directory -Path $testResultsPath -Force | Out-Null
        
    # Test each test project individually
    $testProjects = @(
        "ClearHostedService.Tests\ClearHostedService.Tests.csproj",
        "ClearHostedEndpoint.Tests\ClearHostedEndpoint.Tests.csproj"
    )
        
    $allTestsPassed = $true
    $testCounter = 0
    $coverageFiles = @()
        
    foreach ($project in $testProjects) {
        $projectPath = Join-Path $scriptPath $project
        if (Test-Path $projectPath) {
            $testCounter++
            $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project)
            Write-Info "Testing $projectName"
                
            # Create project-specific test results directory
            $projectTestResultsPath = Join-Path $testResultsPath $projectName
            New-Item -ItemType Directory -Path $projectTestResultsPath -Force | Out-Null
                
            $testArgs = @(
                "test"
                $projectPath
                "--configuration", $Configuration
                "--no-build"
                "--no-restore"
                "--verbosity", $verbosityLevel
                "--results-directory", $projectTestResultsPath
                "--logger", "trx;LogFileName=test-results.trx"
                "--logger", "console;verbosity=normal"
            )
                
            # Add code coverage if requested (using built-in Code Coverage)
            if ($EnableCodeCoverage) {
                Write-Info "  Collecting code coverage for $projectName"
                $testArgs += "--collect"
                $testArgs += "Code Coverage"
            }
                
            # Add detailed test reporting if requested
            if ($GenerateTestReport) {
                $testArgs += "--logger"
                $testArgs += "html;LogFileName=test-results.html"
            }
                
            dotnet @testArgs
                
            if ($LASTEXITCODE -ne 0) {
                $allTestsPassed = $false
                Write-Warning "Tests failed for $projectName"
            }
            else {
                Write-Success "Tests passed for $projectName"
            }
                
            # Collect coverage file paths for later merging
            if ($EnableCodeCoverage) {
                $projectCoverageFiles = Get-ChildItem -Path $projectTestResultsPath -Recurse -Filter "*.coverage" -ErrorAction SilentlyContinue
                foreach ($file in $projectCoverageFiles) {
                    $coverageFiles += $file.FullName
                }
            }
        }
        else {
            Write-Warning "Test project not found: $project"
        }
    }
        
    # Process code coverage if enabled
    if ($EnableCodeCoverage -and $coverageFiles.Count -gt 0) {
        Write-Info "Processing code coverage results..."
        $coverageOutputPath = Join-Path $testResultsPath "Coverage"
        New-Item -ItemType Directory -Path $coverageOutputPath -Force | Out-Null
            
        # Check if dotnet-coverage tool is available
        $dotnetCoverageInstalled = $null -ne (Get-Command "dotnet-coverage" -ErrorAction SilentlyContinue)
        if (-not $dotnetCoverageInstalled) {
            Write-Info "Installing dotnet-coverage tool..."
            dotnet tool install --global dotnet-coverage --verbosity quiet
            if ($LASTEXITCODE -ne 0) {
                Write-Warning "Failed to install dotnet-coverage tool. Attempting to continue..."
            }
            else {
                Write-Success "dotnet-coverage tool installed successfully"
            }
        }
            
        # Merge coverage files and convert to Cobertura format
        $outputCoverageFile = Join-Path $coverageOutputPath "coverage.cobertura.xml"
        Write-Info "Merging $($coverageFiles.Count) coverage file(s) to Cobertura format..."
            
        $mergeArgs = @(
            "merge"
            "--output", $outputCoverageFile
            "--output-format", "cobertura"
        )
        $mergeArgs += $coverageFiles
            
        dotnet-coverage @mergeArgs
            
        if ($LASTEXITCODE -eq 0 -and (Test-Path $outputCoverageFile)) {
            Write-Success "Code coverage merged successfully"
            Write-Info "Coverage file: $outputCoverageFile"
        }
        else {
            Write-Warning "Failed to merge coverage files"
        }
    }
    elseif ($EnableCodeCoverage) {
        Write-Warning "Code coverage enabled but no coverage files found"
    }
        
    # Generate test summary
    Write-Host ""
    Write-Info "Test Execution Summary:"
    Write-Info "  Projects tested: $testCounter"
    Write-Info "  Results location: $testResultsPath"
        
    if (-not $allTestsPassed) {
        throw "One or more test projects failed"
    }
        
    if ($LASTEXITCODE -ne 0) {
        throw "Tests failed with exit code $LASTEXITCODE"
    }
        
    Write-Success "All tests passed"
}
catch {
    Write-Error "Tests failed: $_"
    exit 1
}
}

function Invoke-Pack {
    Write-Header "Creating NuGet Packages"
    
    try {
        $verbosityLevel = if ($VerboseOutput) { "normal" } else { "minimal" }
        
        # Create output directory if it doesn't exist
        if (-not (Test-Path $PackageOutputPath)) {
            New-Item -ItemType Directory -Path $PackageOutputPath -Force | Out-Null
            Write-Info "Created package output directory: $PackageOutputPath"
        }
        
        # Projects to pack (library projects only, not tests)
        $packProjects = @(
            "ClearHostedService\ClearMeasure.HostedService.csproj",
            "ClearHostedEndpoint\ClearMeasure.HostedEndpoint.csproj",
            "ClearHostedEndpoint.SqlServerTransport\ClearMeasure.HostedEndpoint.SqlServerTransport.csproj"
        )
        
        $packagesCreated = 0
        
        foreach ($project in $packProjects) {
            $projectPath = Join-Path $scriptPath $project
            if (Test-Path $projectPath) {
                Write-Info "Packing $(Split-Path $project -Leaf)"
                
                $packArgs = @(
                    "pack"
                    $projectPath
                    "--configuration", $Configuration
                    "--no-build"
                    "--no-restore"
                    "--output", $PackageOutputPath
                    "--verbosity", $verbosityLevel
                )
                
                # Add include symbols option for Release builds
                if ($Configuration -eq "Release") {
                    $packArgs += "--include-symbols"
                    $packArgs += "--include-source"
                }
                
                dotnet @packArgs
                
                if ($LASTEXITCODE -ne 0) {
                    throw "Pack failed for $project with exit code $LASTEXITCODE"
                }
                
                $packagesCreated++
            }
            else {
                Write-Warning "Project not found: $project"
            }
        }
        
        Write-Success "Created $packagesCreated NuGet package(s)"
        Write-Info "Package location: $PackageOutputPath"
        
        # List created packages with multi-color formatting
        $packages = Get-ChildItem -Path $PackageOutputPath -Filter "*.nupkg" -ErrorAction SilentlyContinue
        if ($packages) {
            Write-Host ""
            Write-Info "Packages created:"
            foreach ($package in $packages | Sort-Object LastWriteTime -Descending | Select-Object -First 10) {
                $size = [math]::Round($package.Length / 1KB, 2)
                # Multi-color line: bullet and size in gray, filename in white
                Write-Host "  • " -NoNewline -ForegroundColor DarkGray
                Write-Host $package.Name -NoNewline -ForegroundColor White
                Write-Host " ($size KB)" -ForegroundColor DarkGray
            }
        }
    }
    catch {
        Write-Error "Pack failed: $_"
        exit 1
    }
}

function Show-Summary {
param(
    [DateTime]$StartTime,
    [bool]$TestsRun,
    [bool]$PackagesCreated,
    [bool]$CodeCoverageEnabled,
    [string]$Mode
)
    
$duration = (Get-Date) - $StartTime
    
Write-Header "Build Summary"
Write-Info "Mode: $Mode"
Write-Info "Configuration: $Configuration"
Write-Info "Tests Run: $(if ($TestsRun) { 'Yes' } else { 'No (Skipped)' })"
if ($CodeCoverageEnabled) {
    Write-Info "Code Coverage: Enabled"
}
Write-Info "Packages Created: $(if ($PackagesCreated) { 'Yes' } else { 'No' })"
    Write-Info "Duration: $($duration.ToString('mm\:ss'))"
    Write-Success "BUILD SUCCESSFUL"
}

# Main execution
try {
    $startTime = Get-Date
    
    Write-Host ""
    Write-BoxedMessage "ClearHostedService Build Script"
    Write-Host ""
    
    # Determine execution mode
    $executionMode = "Full Build"
    if ($TestOnly) {
        $executionMode = "Test Only"
    }
    elseif ($PackOnly) {
        $executionMode = "Pack Only"
    }
    
    Write-Info "Execution Mode: $executionMode"
    Write-Host ""
    
    # Check prerequisites (always)
    Test-Prerequisites
    
    # Handle TestOnly mode
    if ($TestOnly) {
        Invoke-Test
        Show-Summary -StartTime $startTime -TestsRun $true -PackagesCreated $false -CodeCoverageEnabled $EnableCodeCoverage -Mode $executionMode
        exit 0
    }
    
    # Handle PackOnly mode
    if ($PackOnly) {
        Invoke-Pack
        Show-Summary -StartTime $startTime -TestsRun $false -PackagesCreated $true -CodeCoverageEnabled $false -Mode $executionMode
        exit 0
    }
    
    # Standard build flow
    # Clean if requested
    if ($Clean) {
        Invoke-Clean
    }
    
    # Restore packages
    Invoke-Restore
    
    # Build solution
    Invoke-Build
    
    # Run tests unless skipped
    $testsRun = $false
    if (-not $SkipTests) {
        Invoke-Test
        $testsRun = $true
    }
    else {
        Write-Warning "Tests skipped (use without -SkipTests to run tests)"
    }
    
    # Create NuGet packages if requested
    $packagesCreated = $false
    if ($Pack) {
        Invoke-Pack
        $packagesCreated = $true
    }
    
    # Show summary
    Show-Summary -StartTime $startTime -TestsRun $testsRun -PackagesCreated $packagesCreated -CodeCoverageEnabled $EnableCodeCoverage -Mode $executionMode
    
    exit 0
}
catch {
    Write-Host ""
    Write-Error "BUILD FAILED: $_"
    Write-Host ""
    exit 1
}
