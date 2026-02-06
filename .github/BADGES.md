# GitHub Actions Status Badges

Add these badges to your README.md to show build and test status.

## Build Status Badge

```markdown
![Build and Publish](https://github.com/ClearMeasureLabs/MultiServiceQuickStart/workflows/Build%20and%20Publish/badge.svg)
```

**Badge appears as:**
- ✅ **Passing** - Green badge when build succeeds
- ❌ **Failing** - Red badge when build fails
- 🟡 **Running** - Yellow badge during execution

## Build Status by Branch

**Main Branch:**
```markdown
![Build Status (main)](https://github.com/ClearMeasureLabs/MultiServiceQuickStart/workflows/Build%20and%20Publish/badge.svg?branch=main)
```

**Develop Branch:**
```markdown
![Build Status (develop)](https://github.com/ClearMeasureLabs/MultiServiceQuickStart/workflows/Build%20and%20Publish/badge.svg?branch=develop)
```

## Example README Section

Add this to the top of your `README.md`:

```markdown
# Multi-Service Quick Start

![Build and Publish](https://github.com/ClearMeasureLabs/MultiServiceQuickStart/workflows/Build%20and%20Publish/badge.svg)
![License](https://img.shields.io/github/license/ClearMeasureLabs/MultiServiceQuickStart)
![.NET](https://img.shields.io/badge/.NET-10.0-blue)

An opinionated base implementation of `IHostedService` for building robust, production-ready background services in .NET.
```

## All Available Badges

```markdown
<!-- Build Status -->
![Build and Publish](https://github.com/ClearMeasureLabs/MultiServiceQuickStart/workflows/Build%20and%20Publish/badge.svg)

<!-- License -->
![License](https://img.shields.io/github/license/ClearMeasureLabs/MultiServiceQuickStart)

<!-- .NET Version -->
![.NET](https://img.shields.io/badge/.NET-10.0-blue)

<!-- Latest Release -->
![GitHub release](https://img.shields.io/github/v/release/ClearMeasureLabs/MultiServiceQuickStart)

<!-- NuGet Version (if published) -->
![NuGet](https://img.shields.io/nuget/v/ClearMeasure.HostedService)

<!-- Downloads (if published to nuget.org) -->
![NuGet Downloads](https://img.shields.io/nuget/dt/ClearMeasure.HostedService)

<!-- Issues -->
![GitHub issues](https://img.shields.io/github/issues/ClearMeasureLabs/MultiServiceQuickStart)

<!-- Pull Requests -->
![GitHub pull requests](https://img.shields.io/github/issues-pr/ClearMeasureLabs/MultiServiceQuickStart)

<!-- Last Commit -->
![GitHub last commit](https://img.shields.io/github/last-commit/ClearMeasureLabs/MultiServiceQuickStart)

<!-- Contributors -->
![GitHub contributors](https://img.shields.io/github/contributors/ClearMeasureLabs/MultiServiceQuickStart)
```

## Custom Shields.io Badges

### Code Coverage Badge (Manual)
After setting up Codecov or similar service:

```markdown
![Code Coverage](https://img.shields.io/codecov/c/github/ClearMeasureLabs/MultiServiceQuickStart)
```

### Custom Static Badges

```markdown
<!-- Quality Badge -->
![Code Quality](https://img.shields.io/badge/code%20quality-A+-brightgreen)

<!-- Maintained Badge -->
![Maintained](https://img.shields.io/badge/maintained-yes-brightgreen)

<!-- Documentation Badge -->
![Documentation](https://img.shields.io/badge/docs-complete-blue)
```

## Badge Placement Examples

### Horizontal Layout
```markdown
![Build](badge1) ![Tests](badge2) ![Coverage](badge3) ![License](badge4)
```

### Table Layout
```markdown
| Build | Tests | Coverage | License |
|-------|-------|----------|---------|
| ![Build](badge1) | ![Tests](badge2) | ![Coverage](badge3) | ![License](badge4) |
```

## Linking Badges to Actions

Make badges clickable to link to workflow runs:

```markdown
[![Build and Publish](https://github.com/ClearMeasureLabs/MultiServiceQuickStart/workflows/Build%20and%20Publish/badge.svg)](https://github.com/ClearMeasureLabs/MultiServiceQuickStart/actions/workflows/build-and-publish.yml)
```

## Dynamic Coverage Badge (via GitHub Actions)

The workflow includes code coverage badge generation. The badge will be available in PR comments.

To add a permanent coverage badge, consider using:
- [Codecov](https://codecov.io/)
- [Coveralls](https://coveralls.io/)
- [Code Climate](https://codeclimate.com/)

## Complete Example

```markdown
# ClearHostedService

<div align="center">

[![Build and Publish](https://github.com/ClearMeasureLabs/MultiServiceQuickStart/workflows/Build%20and%20Publish/badge.svg)](https://github.com/ClearMeasureLabs/MultiServiceQuickStart/actions/workflows/build-and-publish.yml)
[![License](https://img.shields.io/github/license/ClearMeasureLabs/MultiServiceQuickStart)](LICENSE)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![GitHub release](https://img.shields.io/github/v/release/ClearMeasureLabs/MultiServiceQuickStart)](https://github.com/ClearMeasureLabs/MultiServiceQuickStart/releases)

</div>

An opinionated base implementation of `IHostedService` for building robust, production-ready background services in .NET with minimal boilerplate code.

[Features](#features) • [Quick Start](#quick-start) • [Documentation](#documentation) • [Contributing](#contributing)

---
```

This will display the badges centered at the top of your README with clickable links.

## Additional Resources

- [GitHub Badges Documentation](https://docs.github.com/en/actions/monitoring-and-troubleshooting-workflows/adding-a-workflow-status-badge)
- [Shields.io Badge Builder](https://shields.io/)
- [Simple Icons (for logos)](https://simpleicons.org/)
