#!/bin/bash

# SimpleDispatch.ServiceBase Release Helper Script

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Project configuration
PROJECT_FILE="simpledispatch-servicebase.csproj"
CHANGELOG_FILE="CHANGELOG.md"

# Function to display usage
usage() {
    echo -e "${BLUE}SimpleDispatch.ServiceBase Release Helper${NC}"
    echo ""
    echo "Usage: $0 [COMMAND] [OPTIONS]"
    echo ""
    echo "Commands:"
    echo "  bump-version <version>    Bump version and update changelog"
    echo "  build                     Build the project"
    echo "  pack                      Create NuGet package"
    echo "  publish-local            Publish to local NuGet cache"
    echo "  current-version          Show current version"
    echo "  help                     Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 bump-version 1.2.0"
    echo "  $0 build"
    echo "  $0 pack"
    echo ""
}

# Function to get current version
get_current_version() {
    if [[ -f "$PROJECT_FILE" ]]; then
        grep -oP '(?<=<PackageVersion>)[^<]+' "$PROJECT_FILE" || echo "Unknown"
    else
        echo "Project file not found"
        exit 1
    fi
}

# Function to bump version
bump_version() {
    local new_version=$1
    
    if [[ -z "$new_version" ]]; then
        echo -e "${RED}Error: Version number required${NC}"
        echo "Usage: $0 bump-version <version>"
        exit 1
    fi
    
    # Validate version format (basic semantic versioning)
    if [[ ! "$new_version" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
        echo -e "${RED}Error: Invalid version format. Use semantic versioning (e.g., 1.2.0)${NC}"
        exit 1
    fi
    
    local current_version=$(get_current_version)
    echo -e "${BLUE}Bumping version from ${current_version} to ${new_version}${NC}"
    
    # Update project file
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS
        sed -i '' "s/<PackageVersion>.*<\/PackageVersion>/<PackageVersion>${new_version}<\/PackageVersion>/" "$PROJECT_FILE"
    else
        # Linux
        sed -i "s/<PackageVersion>.*<\/PackageVersion>/<PackageVersion>${new_version}<\/PackageVersion>/" "$PROJECT_FILE"
    fi
    
    echo -e "${GREEN}✓ Updated $PROJECT_FILE${NC}"
    
    # Update changelog with new version entry
    local date=$(date +"%Y-%m-%d")
    local temp_file=$(mktemp)
    
    # Create new changelog entry
    {
        head -n 3 "$CHANGELOG_FILE"
        echo ""
        echo "## [$new_version] - $date"
        echo ""
        echo "### Added"
        echo "- [Add your changes here]"
        echo ""
        echo "### Changed"
        echo "- [Add your changes here]"
        echo ""
        echo "### Fixed"
        echo "- [Add your changes here]"
        echo ""
        tail -n +4 "$CHANGELOG_FILE"
    } > "$temp_file"
    
    mv "$temp_file" "$CHANGELOG_FILE"
    echo -e "${GREEN}✓ Updated $CHANGELOG_FILE${NC}"
    echo -e "${YELLOW}⚠ Please edit $CHANGELOG_FILE to add your changes${NC}"
}

# Function to build project
build_project() {
    echo -e "${BLUE}Building project...${NC}"
    dotnet restore "$PROJECT_FILE"
    dotnet build "$PROJECT_FILE" --configuration Release --no-restore
    echo -e "${GREEN}✓ Build completed${NC}"
}

# Function to create package
create_package() {
    echo -e "${BLUE}Creating NuGet package...${NC}"
    local output_dir="./output"
    mkdir -p "$output_dir"
    
    dotnet pack "$PROJECT_FILE" --configuration Release --output "$output_dir" --no-build
    
    echo -e "${GREEN}✓ Package created in $output_dir${NC}"
    ls -la "$output_dir"/*.nupkg
}

# Function to publish to local NuGet cache
publish_local() {
    echo -e "${BLUE}Publishing to local NuGet cache...${NC}"
    local output_dir="./output"
    
    if [[ ! -d "$output_dir" ]]; then
        echo -e "${YELLOW}No packages found. Creating package first...${NC}"
        create_package
    fi
    
    for package in "$output_dir"/*.nupkg; do
        if [[ -f "$package" ]]; then
            echo "Publishing $package to local cache..."
            dotnet nuget push "$package" --source ~/.nuget/packages
        fi
    done
    
    echo -e "${GREEN}✓ Published to local NuGet cache${NC}"
}

# Main script logic
case "${1:-help}" in
    "bump-version")
        bump_version "$2"
        ;;
    "build")
        build_project
        ;;
    "pack")
        build_project
        create_package
        ;;
    "publish-local")
        build_project
        publish_local
        ;;
    "current-version")
        current_version=$(get_current_version)
        echo -e "${GREEN}Current version: $current_version${NC}"
        ;;
    "help"|*)
        usage
        ;;
esac
