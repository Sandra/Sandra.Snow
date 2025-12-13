# Contributing to Sandra.Snow

Thank you for your interest in contributing to Sandra.Snow! This document outlines how to contribute to the project.

## 🎯 **Getting Started**

### **Development Setup**

1. **Prerequisites**:
   - .NET 9.0 SDK
   - Git
   - IDE: Visual Studio, VS Code, or Rider

2. **Clone and Setup**:
   ```bash
   git clone https://github.com/Sandra/Sandra.Snow.git
   cd Sandra.Snow/v2
   dotnet restore Sandra.Snow.v2.sln
   dotnet build Sandra.Snow.v2.sln
   dotnet test Sandra.Snow.v2.sln
   ```

3. **Verify Setup**:
   ```bash
   # Test CLI tool
   dotnet run --project src/Snow.Cli/Snow.Cli.csproj -- --version
   
   # Run all tests
   dotnet test tests/Snow.Engine.Tests/
   ```

## 🔄 **Contribution Workflow**

### **1. Fork and Branch**
```bash
# Fork the repository on GitHub
# Clone your fork
git clone https://github.com/YOUR-USERNAME/Sandra.Snow.git
cd Sandra.Snow/v2

# Create feature branch
git checkout -b feature/your-feature-name
```

### **2. Make Changes**
- Write clear, focused commits
- Follow existing code style and patterns
- Add tests for new functionality
- Update documentation as needed

### **3. Test Thoroughly**
```bash
# Run all tests
dotnet test Sandra.Snow.v2.sln

# Test CLI functionality
dotnet run --project src/Snow.Cli/Snow.Cli.csproj -- build --help

# Run benchmarks (if performance-related)
dotnet run --project tests/Snow.Benchmarks/ -c Release
```

### **4. Submit Pull Request**
- Create a clear PR title and description
- Reference any related issues
- Ensure all CI checks pass
- Be responsive to feedback

## 📝 **Development Guidelines**

### **Code Style**
- Use C# 12 features and modern patterns
- Follow .NET naming conventions
- Enable nullable reference types
- Use async/await patterns consistently
- Add XML documentation for public APIs

### **Testing**
- **Unit Tests**: Required for all business logic
- **Integration Tests**: Required for CLI commands and file operations
- **Benchmark Tests**: Required for performance-related changes
- **Coverage**: Aim for >85% code coverage

### **Documentation**
- Update README for CLI changes
- Add/update XML docs for public APIs
- Update migration guides for breaking changes
- Include examples in documentation

## 🎯 **Contribution Areas**

### **High-Impact Areas**
1. **Performance Optimization**: Improve build speed and memory usage
2. **Template System**: Enhance template features and compatibility
3. **CLI Experience**: Improve commands and user experience
4. **Migration Tools**: Better v1 → v2 migration support
5. **Documentation**: Examples, guides, and API documentation

### **Good First Issues**
Look for issues labeled `good first issue`:
- Documentation improvements
- Test additions
- Small bug fixes
- Example content creation

### **Feature Development**
Major features should be discussed before implementation:
1. Create a [Feature Request](https://github.com/Sandra/Sandra.Snow/issues/new?template=feature_request.yml)
2. Discuss in [GitHub Discussions](https://github.com/Sandra/Sandra.Snow/discussions)
3. Wait for maintainer approval before starting work

## 🔧 **Development Tips**

### **Project Structure**
```
v2/
├── src/
│   ├── Snow.Abstractions/      # Core interfaces
│   ├── Snow.Engine/            # Generation engine
│   └── Snow.Cli/               # Command-line interface
├── tests/
│   ├── Snow.Engine.Tests/      # Unit and integration tests
│   └── Snow.Benchmarks/        # Performance benchmarks
└── docs/                       # Documentation
```

### **Building Components**
```bash
# Build specific project
dotnet build src/Snow.Engine/Snow.Engine.csproj

# Run specific tests  
dotnet test tests/Snow.Engine.Tests/Snow.Engine.Tests.csproj

# Pack CLI for testing
dotnet pack src/Snow.Cli/Snow.Cli.csproj
```

### **Debugging CLI**
```bash
# Debug CLI with test site
cd test-site/
dotnet run --project ../src/Snow.Cli/Snow.Cli.csproj -- build --verbose

# Debug with specific configuration
dotnet run --project ../src/Snow.Cli/Snow.Cli.csproj -- --config custom.json
```

## 🧪 **Testing Guidelines**

### **Test Categories**
- **Unit Tests**: Fast, isolated tests for business logic
- **Integration Tests**: Test CLI commands and file operations
- **Performance Tests**: BenchmarkDotNet tests for performance regression
- **Cross-Platform Tests**: Validate Windows/macOS/Linux compatibility

### **Test Naming**
```csharp
[Fact]
public void MethodName_Condition_ExpectedBehavior()
{
    // Arrange
    // Act  
    // Assert
}
```

### **Test Data**
- Use deterministic test data
- Clean up temporary files
- Mock external dependencies
- Use `[Theory]` for parameterized tests

## 📋 **Pull Request Guidelines**

### **PR Title Format**
- `feat: add new template feature`
- `fix: resolve CLI argument parsing`
- `docs: update migration guide`
- `perf: optimize markdown processing`
- `test: add integration tests for serve command`

### **PR Description Template**
```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update
- [ ] Performance improvement

## Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed
- [ ] Cross-platform testing (if applicable)

## Documentation
- [ ] README updated
- [ ] API documentation updated
- [ ] Migration guide updated (if breaking change)

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review of code completed
- [ ] Tests added/updated for changes
- [ ] Documentation updated
```

### **Review Process**
1. Automated CI checks must pass
2. Code review by maintainers
3. Manual testing of changes
4. Documentation review
5. Final approval and merge

## 🏷️ **Release Process**

### **Versioning**
- Follow [Semantic Versioning](https://semver.org/)
- Breaking changes increment major version
- New features increment minor version
- Bug fixes increment patch version

### **Release Notes**
- All changes documented in CHANGELOG.md
- Breaking changes highlighted prominently
- Migration guidance provided for breaking changes

## 🤝 **Community Guidelines**

### **Code of Conduct**
- Be respectful and inclusive
- Focus on constructive feedback
- Help newcomers and share knowledge
- Report inappropriate behavior to maintainers

### **Communication**
- **Discussions**: General questions and feature discussions
- **Issues**: Bug reports and specific problems
- **Pull Requests**: Code contributions and changes
- **Email**: Security issues and private matters

## 🎉 **Recognition**

### **Contributors**
All contributors are recognized in:
- GitHub contributors list
- Release notes acknowledgments
- Documentation credits
- Annual contributor highlights

### **Becoming a Maintainer**
Active contributors may be invited to become maintainers based on:
- Consistent high-quality contributions
- Understanding of project goals and architecture
- Community engagement and helpfulness
- Commitment to long-term project health

## 📞 **Getting Help**

### **For Contributors**
- **Technical Questions**: [GitHub Discussions](https://github.com/Sandra/Sandra.Snow/discussions)
- **Contribution Help**: Create an issue with `help wanted` label
- **Architecture Questions**: Tag maintainers in discussions

### **For Users**
- **Bug Reports**: [GitHub Issues](https://github.com/Sandra/Sandra.Snow/issues)
- **Feature Requests**: Use the feature request template
- **Migration Help**: Use the migration help template
- **General Questions**: [GitHub Discussions](https://github.com/Sandra/Sandra.Snow/discussions)

---

**Thank you for contributing to Sandra.Snow!** 

Your contributions help make Sandra.Snow the best static site generator for the .NET ecosystem. 🚀
