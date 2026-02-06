# CodeQL Security Analysis Setup

This document describes the CodeQL security analysis integration for the MultiServiceQuickStart repository.

## Overview

CodeQL is a semantic code analysis engine that helps identify security vulnerabilities and code quality issues in your codebase. This setup ensures that:

1. ✅ CodeQL analysis runs on **every commit** to `main` and `develop` branches
2. ✅ CodeQL analysis runs on **every pull request** targeting `main` and `develop`
3. ✅ CodeQL runs **daily** at 2 AM UTC for continuous monitoring
4. ✅ Critical and High severity issues **block the build**
5. ✅ Results are displayed in the **GitHub Security tab**
6. ✅ Results appear as **PR checks**

## Workflow Configuration

**File:** `.github/workflows/codeql-analysis.yml`

### Triggers

- **Push events** to `main` and `develop` branches
- **Pull request events** targeting `main` and `develop` branches
- **Scheduled runs** daily at 2 AM UTC
- **Manual trigger** via workflow_dispatch

### Analysis Settings

- **Language:** C# (csharp)
- **Query Suite:** `security-extended` - Comprehensive security analysis
- **Build Method:** Uses repository's `build.ps1` script
- **Results Format:** SARIF (uploaded to GitHub Security tab)

## Severity Thresholds

The workflow enforces the following severity-based behavior:

| Severity | Score Range | Behavior |
|----------|-------------|----------|
| **Critical** | 9.0 - 10.0 | ❌ **Blocks Build** |
| **High** | 7.0 - 8.9 | ❌ **Blocks Build** |
| **Medium** | 4.0 - 6.9 | ⚠️ Warning (does not block) |
| **Low** | 0.0 - 3.9 | ℹ️ Info (does not block) |

### What This Means

- **Critical/High Issues:** The workflow will **FAIL** and prevent the build/merge from proceeding
- **Medium/Low Issues:** Reported for awareness but do not block the build
- **Action Required:** Critical and High issues must be fixed or dismissed before merging

## Required Permissions

The workflow requires the following GitHub permissions to function properly:

```yaml
permissions:
  actions: read           # Read workflow runs
  contents: read          # Read repository content
  security-events: write  # Upload SARIF to Security tab
  pull-requests: write    # Comment on PRs (future feature)
```

### Note on Permissions

If the workflow shows "action_required" status, it may indicate that:
1. The workflow needs approval for first-time execution in the repository
2. Security permissions need to be enabled in the repository settings

To enable security features:
1. Go to repository **Settings** → **Code security and analysis**
2. Enable **Code scanning** if not already enabled
3. CodeQL will start working automatically

## Viewing Results

### GitHub Security Tab

1. Navigate to the repository **Security** tab
2. Click **Code scanning alerts**
3. View all detected issues with:
   - Severity levels
   - Affected code locations
   - Remediation guidance
   - Links to CWE/CVE references

### Pull Request Checks

1. CodeQL appears as a **required check** on PRs
2. Check status shows in the PR "Checks" tab
3. Click the check to see details
4. Blocking issues prevent merge

### Workflow Job Summary

After each run, the workflow generates a summary showing:
- Total issues by severity
- First 5-10 critical/high issues with locations
- Quick overview without navigating away

## Handling CodeQL Alerts

### Fixing Security Issues

1. Navigate to **Security** tab → **Code scanning alerts**
2. Click on a specific alert to see:
   - The vulnerable code
   - Why it's a problem
   - How to fix it
3. Fix the code and commit the changes
4. CodeQL will re-scan and close the alert if fixed

### Dismissing False Positives

If an alert is a false positive:

1. Navigate to the alert in the Security tab
2. Click **Dismiss alert**
3. Select a reason:
   - **False positive** - Not a real issue
   - **Won't fix** - Accepted risk
   - **Used in tests** - Test code only
4. Add a comment explaining why
5. Save the dismissal

Once dismissed, the alert will not block future builds.

## Branch Protection

To make CodeQL checks **required** before merging:

1. Go to repository **Settings** → **Branches**
2. Add/edit branch protection rule for `main` and `develop`
3. Enable **"Require status checks to pass before merging"**
4. Search for and select **"Analyze Code with CodeQL"**
5. Save the protection rule

This ensures PRs cannot be merged until CodeQL passes.

## Local Testing

While you cannot run CodeQL locally with the full GitHub integration, you can:

1. **Build locally** to catch compilation issues:
   ```powershell
   cd src
   .\build.ps1 -Configuration Release -SkipTests
   ```

2. **Run tests locally** to catch runtime issues:
   ```powershell
   cd src
   .\build.ps1 -Configuration Release
   ```

3. **Install CodeQL CLI** (optional) for local analysis:
   - Download from: https://github.com/github/codeql-cli-binaries
   - Follow CodeQL CLI documentation for local scanning

## Troubleshooting

### Workflow Shows "action_required"

This typically means:
- First-time workflow approval needed (click "Approve and run")
- Repository security features need to be enabled
- Permissions need to be granted

### No SARIF Results Generated

Check:
- Build step completed successfully
- CodeQL initialization step ran without errors
- Permissions are correctly set (security-events: write)

### Build Fails During CodeQL Analysis

Check:
- Build script works locally: `.\src\build.ps1 -Configuration Release -SkipTests`
- All dependencies are properly restored
- .NET 10.0 SDK is available

### False Positive Can't Be Dismissed

Check:
- You have write access to the repository
- Security tab is accessible (not a permission issue)
- Try refreshing the browser or using incognito mode

## Performance Considerations

- **Build Time:** CodeQL adds ~5-10 minutes to workflow time
- **First Run:** Initial analysis may take longer (database creation)
- **Subsequent Runs:** Faster due to incremental analysis
- **Scheduled Runs:** Daily scans ensure new vulnerabilities are caught

## Query Customization

The current setup uses the `security-extended` query suite. You can customize this by:

1. Editing `.github/workflows/codeql-analysis.yml`
2. Changing the `queries` parameter in the "Initialize CodeQL" step
3. Available options:
   - `security-extended` - More comprehensive security checks (current)
   - `security-and-quality` - Security + code quality checks
   - Custom query packs (requires additional setup)

## Additional Resources

- [CodeQL Documentation](https://codeql.github.com/docs/)
- [GitHub Code Scanning](https://docs.github.com/en/code-security/code-scanning)
- [CodeQL for C#](https://codeql.github.com/docs/codeql-language-guides/codeql-for-csharp/)
- [Security Best Practices](https://docs.github.com/en/code-security/getting-started/securing-your-organization)

## Support

For issues or questions:
1. Check the workflow logs in GitHub Actions
2. Review the CodeQL documentation
3. Open an issue in the repository
4. Contact the repository maintainers
