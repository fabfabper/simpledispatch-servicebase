# 🚀 Deployment Checklist for SimpleDispatch.ServiceBase

## ✅ Pre-Deployment Checklist

# 🚀 Deployment Checklist for SimpleDispatch.ServiceBase

## ✅ Pre-Deployment Checklist

### Repository Setup

- [ ] Code is committed to GitHub repository
- [ ] Repository has GitHub Actions enabled
- [ ] All files are properly organized and documented

### GitHub Packages Configuration

- [ ] Repository has Packages permission enabled
- [ ] GitHub Actions has access to write packages
- [ ] Package will be published to GitHub Packages (not NuGet.org)

### Project Configuration

- [ ] Package version is set in `simpledispatch-servicebase.csproj`
- [ ] Package metadata is correct (author, description, tags, license)
- [ ] Dependencies are properly specified
- [ ] Target framework is set to .NET 9.0

### Documentation

- [ ] README.md is complete with GitHub Packages installation instructions
- [ ] CHANGELOG.md has entry for current version
- [ ] Usage examples are provided
- [ ] License file is included

## 🔧 Deployment Steps

### Step 1: Final Preparation

```bash
# 1. Ensure you're on the main branch
git checkout main
git pull origin main

# 2. Update version (if needed)
# Edit simpledispatch-servicebase.csproj:
# <PackageVersion>1.1.0</PackageVersion>

# 3. Update CHANGELOG.md with release notes

# 4. Test build locally
dotnet build --configuration Release
dotnet pack --configuration Release
```

### Step 2: Deploy

```bash
# Commit final changes
git add .
git commit -m "Release v1.1.0: Add PostgreSQL database support"
git push origin main
```

### Step 3: Monitor

- [ ] Check GitHub Actions workflow completion
- [ ] Verify package appears in GitHub Packages
- [ ] Test installation in a new project
- [ ] Verify artifacts are uploaded

## 🧪 Post-Deployment Testing

### Test Package Installation

```bash
# Create test project
mkdir test-simpledispatch
cd test-simpledispatch
dotnet new console

# Configure GitHub Packages access
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

# Install your package
dotnet add package SimpleDispatch.ServiceBase --version 1.1.0

# Test basic functionality
```

### Verify Package Contents

- [ ] Package contains all necessary assemblies
- [ ] Dependencies are correctly resolved
- [ ] Documentation files are included
- [ ] Examples work as expected

## 🔄 Alternative Deployment Methods

### Manual Deployment (If GitHub Actions fails)

```bash
# Build and pack
dotnet build --configuration Release
dotnet pack --configuration Release --output ./output

# Publish manually to GitHub Packages
dotnet nuget push "./output/*.nupkg" --api-key [YOUR_GITHUB_TOKEN] --source https://nuget.pkg.github.com/[YOUR_GITHUB_USERNAME]/index.json
```

## 🚨 Troubleshooting

### Common Issues and Solutions

1. **Build Errors**

   - [ ] Check .NET version compatibility
   - [ ] Verify all dependencies are available
   - [ ] Review compilation errors in Actions logs

2. **GitHub Packages Access Errors**

   - [ ] Verify GitHub token has `write:packages` permission
   - [ ] Check repository permissions for packages
   - [ ] Ensure correct package source URL

3. **GitHub Actions Failures**
   - [ ] Check workflow syntax
   - [ ] Verify GITHUB_TOKEN permissions
   - [ ] Review Actions logs for detailed errors

### Debug Commands

```bash
# Test locally
./scripts/release.sh current-version
./scripts/release.sh build
./scripts/release.sh pack

# Check package contents
dotnet nuget verify ./output/*.nupkg

# Test GitHub Packages access
dotnet nuget list source
```

## ✅ Success Criteria

Your deployment is successful when:

- [ ] GitHub Actions workflow completes without errors
- [ ] Package appears in GitHub Packages within 5-10 minutes
- [ ] Package can be installed in a new project (with proper NuGet.config)
- [ ] All functionality works as expected
- [ ] Package artifacts are available in Actions
- [ ] Package version matches your project file

## 📞 Need Help?

If you encounter issues:

1. Check the GitHub Actions logs
2. Review the [CI/CD Setup Guide](CICD_SETUP.md)
3. Test locally using the release script
4. Verify all secrets and configurations

## 🎉 Post-Success Tasks

Once successfully deployed:

- [ ] Update project documentation
- [ ] Notify team members
- [ ] Create sample projects using the package
- [ ] Plan next version features
- [ ] Monitor package download statistics

**Ready to deploy? Start with the Pre-Deployment Checklist above!** 🚀
