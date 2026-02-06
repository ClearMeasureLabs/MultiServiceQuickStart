# GitHub Actions Workflows

This directory contains the CI/CD workflows for the ClearHostedService solution.

## Workflows

### Build and Publish (`build-and-publish.yml`)

Automated build, test, and package publishing workflow that runs on every push, pull request, and release.

#### Triggers

- **Push**: Runs on pushes to `main` and `develop` branches
- **Pull Request**: Runs on PRs targeting `main` and `develop` branches  
- **Release**: Runs when a GitHub release is published
- **Manual**: Can be triggered manually via workflow_dispatch

#### Jobs

##### 1. Build and Test

Builds the solution, runs all tests, and collects code coverage metrics.

**Steps:**
1. Checkout code with full git history
2. Setup .NET 10.0 SDK
3. Restore NuGet dependencies
4. Build solution in Release configuration
5. Run tests with code coverage collection
6. Upload test results as artifacts (retained for 30 days)
7. Upload code coverage reports as artifacts (retained for 30 days)
8. Publish test results to GitHub Actions UI
9. Generate and display code coverage summary
10. Add coverage report as PR comment (on pull requests)
11. Create NuGet packages (on push/release/manual trigger)
12. Upload packages as artifacts (retained for 90 days)

**Test Results:**
- Test results are displayed in the GitHub Actions UI
- TRX files are uploaded for detailed analysis
- Code coverage is shown in PR comments with:
  - Line coverage percentage
  - Branch coverage percentage
  - Badge indicators
  - Coverage threshold comparison (warning at <60%, good at >80%)

**Code Coverage:**
- Uses XPlat Code Coverage (Coverlet)
- Generates Cobertura format reports
- Coverage summary is added to PR comments
- Individual project coverage is tracked

##### 2. Publish to GitHub Packages

Publishes NuGet packages to GitHub Package Registry.

**Triggers:**
- Push to `main` branch
- Release publication

**Steps:**
1. Download NuGet packages from build job
2. Setup .NET with GitHub Packages authentication
3. Push all `.nupkg` files to GitHub Package Registry
4. Generate publish summary in GitHub Actions UI

**Package Location:**
```
https://nuget.pkg.github.com/ClearMeasureLabs/index.json
```

##### 3. Publish Release to GitHub Packages

Publishes release packages and attaches them to the GitHub release.

**Triggers:**
- Release publication only

**Steps:**
1. Download NuGet packages from build job
2. Setup .NET SDK
3. Publish packages to GitHub Package Registry
4. Attach `.nupkg` files to the GitHub release

## Build Script Integration

The workflows leverage the `src/build.ps1` PowerShell script for all build operations:

### Build Script Parameters

| Parameter | Description | Used By Workflow |
|-----------|-------------|------------------|
| `-Configuration` | Build configuration (Debug/Release) | ✅ Yes (Release) |
| `-SkipTests` | Skip test execution | ✅ Yes (during build-only step) |
| `-TestOnly` | Only run tests | ✅ Yes (separate test step) |
| `-EnableCodeCoverage` | Collect code coverage | ✅ Yes (test step) |
| `-GenerateTestReport` | Generate HTML test reports | ✅ Yes (test step) |
| `-Pack` | Create NuGet packages | ✅ Yes (pack step) |
| `-PackOnly` | Only create packages | ✅ Yes (pack step) |
| `-PackageOutputPath` | Package output directory | ✅ Yes (custom path) |
| `-Clean` | Clean before build | ❌ No (fresh checkout) |
| `-VerboseOutput` | Verbose MSBuild output | ❌ No (uses defaults) |

### Workflow Execution Flow

```
┌─────────────────┐
│  Checkout Code  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Setup .NET 10  │
└────────┬────────┘
         │
         ▼
┌─────────────────────────────┐
│ build.ps1 (Build Only)      │
│ -Configuration Release      │
│ -SkipTests                  │
└────────┬────────────────────┘
         │
         ▼
┌─────────────────────────────┐
│ build.ps1 (Test + Coverage) │
│ -TestOnly                   │
│ -EnableCodeCoverage         │
│ -GenerateTestReport         │
└────────┬────────────────────┘
         │
         ├──► Upload Test Results (TRX files)
         │
         ├──► Upload Code Coverage (Cobertura XML)
         │
         ├──► Publish Test Results (GitHub UI)
         │
         └──► Add Coverage PR Comment
         │
         ▼
┌─────────────────────────────┐
│ build.ps1 (Pack Only)       │  ← Only on push/release
│ -PackOnly                   │
│ -PackageOutputPath          │
└────────┬────────────────────┘
         │
         └──► Upload NuGet Packages
              │
              ▼
         ┌────────────────────┐
         │ Publish to GitHub  │  ← Only on main/release
         │ Package Registry   │
         └────────────────────┘
```

## Test Results and Coverage

### Viewing Test Results

Test results are available in multiple formats:

1. **GitHub Actions UI**: 
   - Navigate to Actions tab → Select workflow run
   - View "Test Results" check
   - See pass/fail status and test count

2. **Artifacts**:
   - Download `test-results` artifact
   - Contains TRX files for detailed analysis
   - Contains HTML reports (if generated)

3. **PR Comments** (for pull requests):
   - Test summary with pass/fail counts
   - Code coverage report with percentages
   - Coverage badge and indicators

### Test Results Structure

```
TestResults/
├── ClearHostedService.Tests/
│   ├── test-results.trx
│   ├── test-results.html (if GenerateTestReport enabled)
│   └── {guid}/
│       └── coverage.cobertura.xml
├── ClearHostedEndpoint.Tests/
│   ├── test-results.trx
│   ├── test-results.html
│   └── {guid}/
│       └── coverage.cobertura.xml
└── Coverage/
    ├── coverage-1.cobertura.xml
    └── coverage-2.cobertura.xml
```

### Code Coverage Thresholds

The workflow uses the following coverage thresholds:

| Threshold | Status | Color |
|-----------|--------|-------|
| < 60% | ⚠️ Warning | Red |
| 60-80% | ⚡ Moderate | Yellow |
| > 80% | ✅ Good | Green |

Coverage is reported for:
- **Line Coverage**: Percentage of executable lines covered by tests
- **Branch Coverage**: Percentage of conditional branches covered
- **Method Coverage**: Percentage of methods invoked by tests

## Package Publishing

### GitHub Package Registry

Packages are automatically published to:
```
https://nuget.pkg.github.com/ClearMeasureLabs/index.json
```

### Published Packages

1. **ClearMeasure.HostedService** - Core hosted service library
2. **ClearMeasure.HostedEndpoint** - NServiceBus endpoint hosting
3. **ClearMeasure.HostedEndpoint.SqlServerTransport** - SQL Server transport extensions

### Authentication

To consume packages from GitHub Package Registry:

```bash
# Add package source
dotnet nuget add source "https://nuget.pkg.github.com/ClearMeasureLabs/index.json" \
  --name "GitHub" \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text

# Install package
dotnet add package ClearMeasure.HostedService --source "GitHub"
```

### Package Versioning

Package versions are controlled by the `.csproj` files:
- `<Version>` property specifies the package version
- Follows Semantic Versioning (Major.Minor.Patch)
- Pre-release versions use suffixes (e.g., `1.0.0-beta`)

## Workflow Artifacts

| Artifact | Retention | Description |
|----------|-----------|-------------|
| `test-results` | 30 days | TRX and HTML test result files |
| `code-coverage` | 30 days | Cobertura coverage reports |
| `nuget-packages` | 90 days | Built NuGet packages (.nupkg) |

## Environment Variables

| Variable | Value | Description |
|----------|-------|-------------|
| `DOTNET_VERSION` | `10.0.x` | .NET SDK version to use |
| `DOTNET_SKIP_FIRST_TIME_EXPERIENCE` | `1` | Skip .NET welcome message |
| `DOTNET_NOLOGO` | `true` | Hide .NET logo in output |
| `DOTNET_CLI_TELEMETRY_OPTOUT` | `1` | Disable telemetry |

## Required Secrets

| Secret | Description | Required For |
|--------|-------------|--------------|
| `GITHUB_TOKEN` | Automatically provided by GitHub Actions | Package publishing |

No additional secrets are required. The workflow uses the built-in `GITHUB_TOKEN` for all operations.

## Manual Workflow Trigger

You can manually trigger the workflow from the GitHub UI:

1. Go to **Actions** tab
2. Select **Build and Publish** workflow
3. Click **Run workflow** button
4. Select branch
5. Click **Run workflow**

This will execute the full build, test, and package creation process.

## Troubleshooting

### Tests Failing

1. Check the **Test Results** section in the workflow run
2. Download the `test-results` artifact for detailed TRX files
3. Review console output in the "Run tests with coverage" step
4. Check for environment-specific issues (GitHub Actions uses Windows runners)

### Package Publishing Failures

1. Verify `GITHUB_TOKEN` has package write permissions
2. Check package version doesn't already exist (duplicate)
3. Ensure package metadata is valid in `.csproj` files
4. Review publish step logs for authentication errors

### Code Coverage Not Appearing

1. Ensure `EnableCodeCoverage` parameter is used
2. Check that Coverlet collector is working (look for coverage files in logs)
3. Verify Cobertura XML files are being generated
4. Check Code Coverage Report step for errors

### Build Script Errors

1. Review build.ps1 execution logs
2. Check PowerShell version compatibility (.NET 10 requires PowerShell 7+)
3. Verify all required projects are present
4. Check for encoding issues (UTF-8 BOM can cause problems)

## Local Testing

Test the workflow behavior locally:

```powershell
# Test build-only
cd src
.\build.ps1 -Configuration Release -SkipTests

# Test with coverage
.\build.ps1 -Configuration Release -TestOnly -EnableCodeCoverage -GenerateTestReport

# Test packaging
.\build.ps1 -Configuration Release -PackOnly -PackageOutputPath "$PWD\packages"

# Full workflow simulation
.\build.ps1 -Configuration Release -Clean
.\build.ps1 -Configuration Release -TestOnly -EnableCodeCoverage -GenerateTestReport
.\build.ps1 -Configuration Release -PackOnly
```

## Performance

Typical workflow execution times:

| Job | Duration | Notes |
|-----|----------|-------|
| Build and Test | 3-5 minutes | Includes restore, build, test, coverage |
| Publish to GitHub Packages | 1-2 minutes | Network dependent |
| Total (Push to main) | 4-7 minutes | Combined time |

Optimizations:
- Restore is cached by dotnet/setup-dotnet action
- Separate build and test steps allow parallel artifact processing
- PackOnly mode skips unnecessary build steps

## Best Practices

1. **Always run tests locally** before pushing
2. **Use conventional commits** for clear changelog generation
3. **Tag releases** with semantic versions (v1.0.0)
4. **Review coverage reports** on PRs before merging
5. **Monitor artifact storage** to avoid exceeding GitHub limits
6. **Update package versions** before releases
7. **Test package installation** from GitHub Packages after publishing

## Future Enhancements

Potential improvements to consider:

- [ ] Add integration with external code coverage services (Codecov, Coveralls)
- [ ] Implement automated version bumping
- [ ] Add security scanning (dependency check, vulnerability scanning)
- [ ] Create separate workflows for PR validation vs. deployment
- [ ] Add performance benchmarking
- [ ] Implement automated changelog generation
- [ ] Add mutation testing
- [ ] Create deployment workflows for production environments

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [GitHub Packages Documentation](https://docs.github.com/en/packages)
- [.NET CLI Documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [Semantic Versioning](https://semver.org/)
