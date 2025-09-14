# 🚀 CI/CD Setup Guide for SimpleDispatch.ServiceBase

This guide will help you set up automated package publishing to GitHub Packages using GitHub Actions.

## 📋 Prerequisites

- GitHub repository with your code
- GitHub account with packages permission
- Basic understanding of Git and GitHub

## 🔑 Step 1: GitHub Token (Automatically Available)

GitHub Actions automatically provides a `GITHUB_TOKEN` with the necessary permissions to publish to GitHub Packages. No additional setup is required for the token.

## 📦 Step 2: Understand the Workflow

### Main Workflow (`publish-github-packages.yml`)

- **Triggers**: Push to `main` branch
- **Actions**:
  - Builds and tests the project
  - Creates NuGet package
  - Publishes to GitHub Packages
  - Uploads artifacts

### Key Features

- Automatic publishing on every push to main
- Uses the project version from the `.csproj` file
- No manual tag creation required
- Built-in artifact retention

## 🛠️ Step 3: Making Releases

### Automatic Release (Default Behavior)

1. **Update the version** in `simpledispatch-servicebase.csproj`:

   ```xml
   <PackageVersion>1.2.0</PackageVersion>
   ```

2. **Update CHANGELOG.md** with your changes:

   ```markdown
   ## [1.2.0] - 2024-09-14

   ### Added

   - New database feature

   ### Changed

   - Improved performance
   ```

3. **Commit and push to main**:

   ```bash
   git add .
   git commit -m "Release v1.2.0"
   git push origin main
   ```

4. **The workflow will automatically**:
   - Build the project
   - Create and publish the package to GitHub Packages
   - Upload artifacts to GitHub Actions

### Manual Release (Local Development)

Use the provided release script:

```bash
# Make sure the script is executable
chmod +x scripts/release.sh

# Bump version and update changelog
./scripts/release.sh bump-version 1.2.0

# Build and create package
./scripts/release.sh pack

# Publish to local NuGet cache for testing
./scripts/release.sh publish-local
```

## 📊 Step 4: Monitor Your Releases

### GitHub Actions

- Go to the **Actions** tab in your repository
- Monitor workflow runs
- Check for any errors or warnings
- View artifacts and logs

### GitHub Packages

- Go to your repository → **Packages** tab
- Or visit: `https://github.com/[YOUR_GITHUB_USERNAME]/[YOUR_REPOSITORY_NAME]/packages`
- Verify the new version is published
- Monitor download statistics

### Package Artifacts

- Check the **Actions** tab for uploaded package artifacts
- Download packages for local testing if needed

## 🔧 Step 5: Testing Your Package

After publishing, test your package:

```bash
# Create a test project
mkdir test-project
cd test-project
dotnet new console

# Configure NuGet to use GitHub Packages
cat > NuGet.config << EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="github" value="https://nuget.pkg.github.com/[YOUR_GITHUB_USERNAME]/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_TOKEN" />
    </github>
  </packageSourceCredentials>
</configuration>
EOF

# Add your published package
dotnet add package SimpleDispatch.ServiceBase --version 1.2.0

# Test the functionality
```

## 🚨 Troubleshooting

### Common Issues

1. **"Unable to load the service index for source"**

   - Check that your NuGet.config is properly configured
   - Verify your GitHub token has `read:packages` permission
   - Ensure the package source URL is correct

2. **"Package not found"**

   - Verify the package was published successfully in GitHub Actions
   - Check the Packages tab in your GitHub repository
   - Ensure the package version exists

3. **Build failures**

   - Check the Actions logs for detailed error messages
   - Ensure all dependencies are correctly referenced
   - Verify the .NET version in the workflow matches your project

4. **Permission errors**
   - Verify repository permissions for packages
   - Check that GITHUB_TOKEN has sufficient permissions

### Debugging Steps

1. **Check workflow logs**:

   - Go to Actions tab → Select failed workflow → View logs

2. **Validate locally**:

   ```bash
   # Test build locally
   dotnet build --configuration Release

   # Test package creation
   dotnet pack --configuration Release
   ```

3. **Test GitHub Packages access**:
   ```bash
   # List available packages (replace with your details)
   dotnet nuget list source
   ```

## 📈 Best Practices

1. **Version Management**

   - Use semantic versioning (MAJOR.MINOR.PATCH)
   - Document breaking changes clearly
   - Keep CHANGELOG.md up to date

2. **Security**

   - Use GitHub tokens with minimal required permissions
   - Regularly review package access permissions
   - Never commit tokens to the repository

3. **Testing**

   - Test packages locally before publishing
   - Use pre-release versions for testing
   - Maintain backward compatibility when possible

4. **Documentation**
   - Keep README.md updated with installation instructions
   - Document breaking changes in release notes
   - Provide migration guides for major versions

## 🎯 Next Steps

1. **Push code to main branch** to trigger the workflow
2. **Test the workflow** with a minor version bump
3. **Create your first automated release**
4. **Monitor the package** in GitHub Packages
5. **Configure team access** to the packages if needed

## 📞 Support

If you encounter issues:

- Check the [GitHub Actions documentation](https://docs.github.com/en/actions)
- Review [GitHub Packages documentation](https://docs.github.com/en/packages)
- Check the workflow logs for detailed error messages

Happy releasing! 🎉
