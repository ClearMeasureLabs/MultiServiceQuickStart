# Build Script - Code Coverage and Test Reporting

This document describes the enhanced test reporting and code coverage features added to `build.ps1`.

## New Parameters

### `-EnableCodeCoverage`
Enables code coverage collection during test execution using Coverlet (XPlat Code Coverage).

**Usage:**
```powershell
.\build.ps1 -EnableCodeCoverage
```

**Output:**
- Cobertura XML coverage reports
- Files located in `TestResults/Coverage/`
- Individual coverage files per test project

### `-GenerateTestReport`
Generates HTML test reports in addition to TRX files.

**Usage:**
```powershell
.\build.ps1 -GenerateTestReport
```

**Output:**
- HTML test result files
- Located in project-specific `TestResults/` subdirectories
- Viewable in any web browser

### `-TestOnly`
Runs only tests without building the solution. Assumes projects are already built.

**Usage:**
```powershell
# Build first
.\build.ps1 -SkipTests

# Then run tests separately
.\build.ps1 -TestOnly -EnableCodeCoverage
```

**Useful for:**
- CI/CD pipelines with separate build/test stages
- Quick test reruns without rebuilding
- Code coverage collection after build

### `-PackOnly`
Creates NuGet packages without building or testing. Assumes projects are already built.

**Usage:**
```powershell
# Build and test first
.\build.ps1

# Then create packages
.\build.ps1 -PackOnly -PackageOutputPath "C:\packages"
```

**Useful for:**
- CI/CD pipelines with separate packaging stages
- Package creation after successful tests
- Custom package output locations

## Combined Usage Examples

### Full Build with Coverage
```powershell
.\build.ps1 -Configuration Release -EnableCodeCoverage -GenerateTestReport
```
Builds, tests with coverage, and generates test reports.

### CI/CD Pipeline Simulation
```powershell
# Stage 1: Build
.\build.ps1 -Configuration Release -SkipTests

# Stage 2: Test with Coverage
.\build.ps1 -TestOnly -EnableCodeCoverage -GenerateTestReport

# Stage 3: Package
.\build.ps1 -PackOnly -PackageOutputPath "$PWD\artifacts\packages"
```

### Quick Development Cycle
```powershell
# Initial full build
.\build.ps1 -Configuration Debug

# Make code changes...

# Quick rebuild and test
.\build.ps1 -Configuration Debug -SkipTests
.\build.ps1 -TestOnly
```

## Test Results Output

### Directory Structure

After running tests with coverage and reporting enabled:

```
src/
└── TestResults/
    ├── ClearHostedService.Tests/
    │   ├── test-results.trx         (Visual Studio Test Results)
    │   ├── test-results.html        (HTML Report - if GenerateTestReport)
    │   └── {guid}/
    │       └── coverage.cobertura.xml (Coverage Data)
    ├── ClearHostedEndpoint.Tests/
    │   ├── test-results.trx
    │   ├── test-results.html
    │   └── {guid}/
    │       └── coverage.cobertura.xml
    └── Coverage/
        ├── coverage-1.cobertura.xml  (Consolidated)
        └── coverage-2.cobertura.xml
```

### File Descriptions

| File | Format | Description |
|------|--------|-------------|
| `test-results.trx` | XML (TRX) | Visual Studio test results format |
| `test-results.html` | HTML | Human-readable test report |
| `coverage.cobertura.xml` | XML (Cobertura) | Code coverage data |

## Viewing Test Results

### TRX Files (Visual Studio)
1. Open Visual Studio
2. Go to **Test** → **Test Explorer**
3. Right-click → **Import Test Results**
4. Select a `.trx` file from `TestResults/`

### HTML Reports (Browser)
1. Navigate to `TestResults/{ProjectName}/`
2. Open `test-results.html` in any web browser
3. View test outcomes, durations, and error messages

### Coverage Reports (Command Line)
```powershell
# View coverage summary
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator `
  -reports:"TestResults/Coverage/*.xml" `
  -targetdir:"TestResults/CoverageReport" `
  -reporttypes:Html

# Open report
Start-Process TestResults/CoverageReport/index.html
```

## Code Coverage Details

### Coverage Collection

The build script uses **Coverlet** for code coverage:
- Cross-platform coverage collector
- Integrated with `dotnet test`
- Outputs Cobertura XML format

### Coverage Metrics

Collected metrics include:
- **Line Coverage**: Percentage of code lines executed
- **Branch Coverage**: Percentage of conditional branches taken
- **Method Coverage**: Percentage of methods called

### Coverage Configuration

Coverage is collected with default settings:
- Includes all projects under test
- Excludes test projects themselves
- Excludes auto-generated code
- Excludes third-party assemblies

### Customizing Coverage

To customize coverage collection, modify the test arguments in `Invoke-Test`:

```powershell
$testArgs += "--collect:XPlat Code Coverage"
$testArgs += "--"
$testArgs += "DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura"
# Add more settings here
```

## GitHub Actions Integration

The enhanced build script is designed for GitHub Actions:

### Build Job
```yaml
- name: Build solution
  run: |
    cd src
    .\build.ps1 -Configuration Release -SkipTests
  shell: pwsh
```

### Test Job with Coverage
```yaml
- name: Run tests with coverage
  run: |
    cd src
    .\build.ps1 -TestOnly -EnableCodeCoverage -GenerateTestReport
  shell: pwsh
```

### Package Job
```yaml
- name: Create packages
  run: |
    cd src
    .\build.ps1 -PackOnly -PackageOutputPath "${{ github.workspace }}/packages"
  shell: pwsh
```

## Performance Considerations

### Build Time Impact

| Configuration | Typical Duration | Notes |
|---------------|------------------|-------|
| Standard build + test | 30-60 seconds | Baseline |
| With `-EnableCodeCoverage` | +10-20 seconds | Coverage instrumentation overhead |
| With `-GenerateTestReport` | +5-10 seconds | HTML generation |
| Combined | +15-30 seconds | Both coverage and reporting |

### Optimization Tips

1. **Use `-TestOnly`** for incremental test runs
2. **Skip coverage** for quick local testing
3. **Use `-PackOnly`** to avoid rebuilding when packaging
4. **Parallel testing** is automatic (xUnit default behavior)

## Troubleshooting

### Coverage Files Not Generated

**Problem**: No `.cobertura.xml` files found in TestResults.

**Solutions:**
1. Ensure `EnableCodeCoverage` switch is used
2. Verify Coverlet collector is installed (included in .NET SDK)
3. Check test output for coverage collection errors
4. Verify tests are actually running and passing

### Coverage Reports Empty or Incomplete

**Problem**: Coverage report shows 0% or missing projects.

**Solutions:**
1. Ensure tested projects have PDB files (Debug or Release with symbols)
2. Verify test projects reference the projects under test
3. Check that tests are actually executing code (not all mocked)
4. Review Coverlet configuration for exclusions

### HTML Reports Not Generated

**Problem**: No `test-results.html` files created.

**Solutions:**
1. Ensure `GenerateTestReport` switch is used
2. Verify HTML logger is available (may require additional package)
3. Check test execution logs for HTML generation errors
4. Try running with `-VerboseOutput` for more details

### TestResults Directory Cleanup

**Problem**: Old test results accumulating.

**Solution:** The script automatically cleans the `TestResults` directory before each test run.

To manually clean:
```powershell
Remove-Item -Path src/TestResults -Recurse -Force -ErrorAction SilentlyContinue
```

## Best Practices

### Development Workflow
1. **Local testing**: Run without coverage for speed
   ```powershell
   .\build.ps1 -Configuration Debug
   ```

2. **Pre-commit**: Run with coverage to check quality
   ```powershell
   .\build.ps1 -Configuration Release -EnableCodeCoverage
   ```

3. **Before PR**: Generate full reports
   ```powershell
   .\build.ps1 -Configuration Release -EnableCodeCoverage -GenerateTestReport
   ```

### CI/CD Pipeline
1. **Build**: Separate build step for faster feedback
   ```powershell
   .\build.ps1 -Configuration Release -SkipTests
   ```

2. **Test**: Dedicated test step with coverage
   ```powershell
   .\build.ps1 -TestOnly -EnableCodeCoverage -GenerateTestReport
   ```

3. **Package**: Separate packaging step
   ```powershell
   .\build.ps1 -PackOnly -PackageOutputPath ./artifacts
   ```

### Coverage Thresholds
- **Minimum**: 60% line coverage
- **Target**: 80% line coverage
- **Excellent**: 90%+ line coverage

Track coverage trends over time:
```powershell
# Extract coverage percentage from Cobertura report
[xml]$coverage = Get-Content TestResults/Coverage/coverage-1.cobertura.xml
$lineRate = [double]$coverage.coverage.'line-rate' * 100
Write-Host "Line Coverage: $lineRate%"
```

## Additional Tools

### ReportGenerator (Coverage HTML)
```powershell
# Install
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate report
reportgenerator `
  -reports:"TestResults/Coverage/*.xml" `
  -targetdir:"TestResults/CoverageReport" `
  -reporttypes:"Html;Badges"

# View
Start-Process TestResults/CoverageReport/index.html
```

### dotnet-coverage (Microsoft)
```powershell
# Install
dotnet tool install -g dotnet-coverage

# Merge coverage files
dotnet-coverage merge `
  TestResults/**/coverage.cobertura.xml `
  -o TestResults/merged-coverage.xml `
  -f cobertura
```

### Visual Studio Code Extensions
- **Coverage Gutters**: Shows coverage inline in editor
- **.NET Core Test Explorer**: Run/debug tests from UI

## Summary

The enhanced `build.ps1` script now supports:
✅ Code coverage collection with Coverlet  
✅ HTML test report generation  
✅ Separate build/test/pack stages for CI/CD  
✅ Automatic test results organization  
✅ Cobertura XML output for tooling integration  
✅ GitHub Actions integration  
✅ Coverage file consolidation  
✅ Detailed test execution logging  

This provides a robust foundation for quality assurance and continuous integration workflows.
