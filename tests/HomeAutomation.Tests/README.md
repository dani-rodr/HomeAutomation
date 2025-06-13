# HomeAutomation Tests

This project contains unit tests for the HomeAutomation NetDaemon application.

## Structure

The test project mirrors the source project structure:

- `/Common` - Tests for base classes and shared functionality
- `/Area` - Tests for area-specific automations
- `/Helpers` - Tests for utility classes and helper methods

## Running Tests

From the solution root:

```bash
# Run all tests
dotnet.exe test

# Run tests with detailed output
dotnet.exe test --logger "console;verbosity=detailed"

# Run tests with code coverage
dotnet.exe test --collect:"XPlat Code Coverage"
```

## Code Coverage

This project includes comprehensive code coverage analysis with 80% minimum threshold.

### Basic Coverage Commands

```bash
# Run tests with basic coverage collection
dotnet.exe test --collect:"XPlat Code Coverage"

# Run tests with custom coverage settings
dotnet.exe test --settings coverlet.runsettings

# Run with coverage threshold enforcement (fails if below 80%)
dotnet.exe test /p:CollectCoverage=true /p:Threshold=80
```

### HTML Coverage Reports

```bash
# Generate detailed HTML coverage report
dotnet.exe test --collect:"XPlat Code Coverage" --results-directory ./TestResults
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./coverage-report" -reporttypes:Html

# Open the coverage report (Windows)
start ./coverage-report/index.html

# Open the coverage report (WSL/Linux)
explorer.exe ./coverage-report/index.html
```

### Coverage Output Formats

The project is configured to generate multiple coverage formats:
- **JSON** - Machine-readable format for CI/CD integration
- **Cobertura** - XML format compatible with most CI systems
- **OpenCover** - Detailed XML format with branch coverage
- **lcov** - Format compatible with VS Code extensions

### Coverage Configuration

Coverage settings are configured in:
- `HomeAutomation.Tests.csproj` - MSBuild properties for thresholds and exclusions
- `coverlet.runsettings` - Detailed collector settings for advanced scenarios

**Exclusions:**
- Generated code (`HomeAssistantGenerated.cs`)
- Global usings (`GlobalUsings.cs`)
- Test assemblies
- Obsolete/deprecated code

**Thresholds:**
- Minimum 80% coverage for lines, branches, and methods
- Configurable per coverage type in project file

## Test Frameworks

- **xUnit** - Test framework
- **FluentAssertions** - Assertion library for readable test assertions
- **Moq** - Mocking framework for creating test doubles

## Writing Tests

Tests follow standard xUnit patterns with FluentAssertions:

```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var mock = new Mock<IService>();
    
    // Act
    var result = TestMethod();
    
    // Assert
    result.Should().Be(expected);
}
```