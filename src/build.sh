#!/bin/bash
# Build script for ClearHostedService solution (Linux/Mac)

set -e

# Default values
CONFIGURATION="Release"
SKIP_TESTS=0
CLEAN=0
PACK=0
PACKAGE_OUTPUT="artifacts/packages"
VERBOSE=0

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Functions
print_header() {
    echo ""
    echo -e "${CYAN}========================================${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}========================================${NC}"
    echo ""
}

print_success() {
    echo -e "${GREEN}? $1${NC}"
}

print_info() {
    echo -e "${BLUE}? $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}? $1${NC}"
}

print_error() {
    echo -e "${RED}? $1${NC}"
}

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        -s|--skip-tests)
            SKIP_TESTS=1
            shift
            ;;
        --clean)
            CLEAN=1
            shift
            ;;
        -p|--pack)
            PACK=1
            shift
            ;;
        --package-output)
            PACKAGE_OUTPUT="$2"
            shift 2
            ;;
        -v|--verbose)
            VERBOSE=1
            shift
            ;;
        -h|--help)
            echo "Usage: ./build.sh [options]"
            echo ""
            echo "Options:"
            echo "  -c, --configuration <Debug|Release>  Build configuration (default: Release)"
            echo "  -s, --skip-tests                     Skip running tests"
            echo "  --clean                              Clean before build"
            echo "  -p, --pack                           Create NuGet packages"
            echo "  --package-output <path>              Package output directory (default: artifacts/packages)"
            echo "  -v, --verbose                        Enable verbose output"
            echo "  -h, --help                           Show this help message"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            echo "Use -h or --help for usage information"
            exit 1
            ;;
    esac
done

# Set verbosity
if [ $VERBOSE -eq 1 ]; then
    VERBOSITY="normal"
else
    VERBOSITY="minimal"
fi

# Main execution
echo ""
echo -e "${CYAN}??????????????????????????????????????????${NC}"
echo -e "${CYAN}?   ClearHostedService Build Script     ?${NC}"
echo -e "${CYAN}??????????????????????????????????????????${NC}"
echo ""

# Check prerequisites
print_header "Checking Prerequisites"

if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK not found. Please install .NET 10 SDK or later."
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
print_success ".NET SDK version: $DOTNET_VERSION"

# Clean if requested
if [ $CLEAN -eq 1 ]; then
    print_header "Cleaning Solution"
    dotnet clean --configuration $CONFIGURATION --verbosity quiet
    
    # Remove bin and obj directories
    find . -type d -name "bin" -o -name "obj" | xargs rm -rf
    
    print_success "Clean completed successfully"
fi

# Restore
print_header "Restoring NuGet Packages"

projects=(
    "ClearHostedService/ClearMeasure.HostedService.csproj"
    "ClearHostedEndpoint/ClearMeasure.HostedEndpoint.csproj"
    "ClearHostedEndpoint.SqlServerTransport/ClearMeasure.HostedEndpoint.SqlServerTransport.csproj"
    "ClearHostedService.Tests/ClearHostedService.Tests.csproj"
    "ClearHostedEndpoint.Tests/ClearHostedEndpoint.Tests.csproj"
)

for project in "${projects[@]}"; do
    if [ -f "$project" ]; then
        print_info "Restoring $(basename $project)"
        dotnet restore "$project" --verbosity $VERBOSITY
        
        if [ $? -ne 0 ]; then
            print_error "Restore failed for $project"
            exit 1
        fi
    else
        print_warning "Project not found: $project"
    fi
done

print_success "Restore completed successfully"

# Build
print_header "Building Projects ($CONFIGURATION)"

for project in "${projects[@]}"; do
    if [ -f "$project" ]; then
        print_info "Building $(basename $project)"
        dotnet build "$project" --configuration $CONFIGURATION --no-restore --verbosity $VERBOSITY
        
        if [ $? -ne 0 ]; then
            print_error "Build failed for $project"
            exit 1
        fi
    fi
done

print_success "Build completed successfully"

# Test
if [ $SKIP_TESTS -eq 0 ]; then
    print_header "Running Tests"
    
    test_projects=(
        "ClearHostedService.Tests/ClearHostedService.Tests.csproj"
        "ClearHostedEndpoint.Tests/ClearHostedEndpoint.Tests.csproj"
    )
    
    for project in "${test_projects[@]}"; do
        if [ -f "$project" ]; then
            print_info "Testing $(basename $project)"
            dotnet test "$project" --configuration $CONFIGURATION --no-build --no-restore --verbosity $VERBOSITY --logger "console;verbosity=normal"
            
            if [ $? -ne 0 ]; then
                print_error "Tests failed for $project"
                exit 1
            fi
        fi
    done
    
    print_success "All tests passed"
else
    print_warning "Tests skipped"
fi

# Pack
if [ $PACK -eq 1 ]; then
    print_header "Creating NuGet Packages"
    
    # Create output directory if it doesn't exist
    if [ ! -d "$PACKAGE_OUTPUT" ]; then
        mkdir -p "$PACKAGE_OUTPUT"
        print_info "Created package output directory: $PACKAGE_OUTPUT"
    fi
    
    pack_projects=(
        "ClearHostedService/ClearMeasure.HostedService.csproj"
        "ClearHostedEndpoint/ClearMeasure.HostedEndpoint.csproj"
        "ClearHostedEndpoint.SqlServerTransport/ClearMeasure.HostedEndpoint.SqlServerTransport.csproj"
    )
    
    packages_created=0
    
    for project in "${pack_projects[@]}"; do
        if [ -f "$project" ]; then
            print_info "Packing $(basename $project)"
            
            pack_args="pack $project --configuration $CONFIGURATION --no-build --no-restore --output $PACKAGE_OUTPUT --verbosity $VERBOSITY"
            
            # Add include symbols for Release builds
            if [ "$CONFIGURATION" == "Release" ]; then
                pack_args="$pack_args --include-symbols --include-source"
            fi
            
            dotnet $pack_args
            
            if [ $? -ne 0 ]; then
                print_error "Pack failed for $project"
                exit 1
            fi
            
            packages_created=$((packages_created + 1))
        else
            print_warning "Project not found: $project"
        fi
    done
    
    print_success "Created $packages_created NuGet package(s)"
    print_info "Package location: $PACKAGE_OUTPUT"
    
    # List created packages
    if ls "$PACKAGE_OUTPUT"/*.nupkg 1> /dev/null 2>&1; then
        echo ""
        print_info "Packages created:"
        for package in "$PACKAGE_OUTPUT"/*.nupkg; do
            if [ -f "$package" ]; then
                filename=$(basename "$package")
                size=$(du -h "$package" | cut -f1)
                echo "  • $filename ($size)"
            fi
        done
    fi
fi


# Summary
print_header "Build Summary"
print_info "Configuration: $CONFIGURATION"
print_info "Tests Run: $([ $SKIP_TESTS -eq 0 ] && echo 'Yes' || echo 'No (Skipped)')"
print_info "Packages Created: $([ $PACK -eq 1 ] && echo 'Yes' || echo 'No')"
print_success "BUILD SUCCESSFUL"
echo ""

exit 0
