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
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

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