#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Test and coverage script for HomeAutomation NetDaemon project

.DESCRIPTION
    Provides easy access to test execution, code coverage analysis, and HTML report generation.
    Supports both interactive menu and command-line parameter usage.

.PARAMETER Action
    The action to perform: test, coverage, report, all, clean
    - test: Run tests only
    - coverage: Run tests with coverage collection
    - report: Run tests with coverage and generate HTML report
    - all: Run tests, coverage, generate report, and open in browser
    - clean: Clean coverage artifacts

.EXAMPLE
    .\test-coverage.ps1
    # Shows interactive menu

.EXAMPLE
    .\test-coverage.ps1 -Action test
    # Runs tests only

.EXAMPLE
    .\test-coverage.ps1 -Action all
    # Runs full coverage workflow with report
#>

param(
    [Parameter(Position=0)]
    [ValidateSet("test", "coverage", "report", "all", "clean", "")]
    [string]$Action = ""
)

# Color functions for better output
function Write-Success { param([string]$Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Info { param([string]$Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Warning { param([string]$Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Error { param([string]$Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Header { param([string]$Message) Write-Host "`n🔧 $Message" -ForegroundColor Magenta }

# Tool validation
function Test-Requirements {
    Write-Info "Checking requirements..."
    
    # Check for dotnet
    if (-not (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue)) {
        Write-Error "dotnet.exe not found. Please install .NET SDK."
        return $false
    }
    
    # Check for reportgenerator (only warn if not found)
    if (-not (Get-Command "reportgenerator" -ErrorAction SilentlyContinue)) {
        Write-Warning "reportgenerator not found. HTML reports will not be available."
        Write-Info "Install with: dotnet tool install -g dotnet-reportgenerator-globaltool"
    }
    
    return $true
}

# Clean coverage artifacts
function Invoke-CleanCoverage {
    Write-Header "Cleaning coverage artifacts"
    
    $foldersToClean = @("coverage", "coverage-report", "TestResults")
    $filesToClean = @("coverage.*", "*.coverage", "*.coveragexml")
    
    foreach ($folder in $foldersToClean) {
        if (Test-Path $folder) {
            Write-Info "Removing folder: $folder"
            Remove-Item $folder -Recurse -Force
        }
    }
    
    foreach ($pattern in $filesToClean) {
        $files = Get-ChildItem -Path . -Name $pattern -ErrorAction SilentlyContinue
        foreach ($file in $files) {
            Write-Info "Removing file: $file"
            Remove-Item $file -Force
        }
    }
    
    Write-Success "Coverage artifacts cleaned"
}

# Run tests only
function Invoke-Tests {
    Write-Header "Running tests"
    
    $result = & dotnet.exe test --logger "console;verbosity=normal"
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Tests completed successfully"
        return $true
    } else {
        Write-Error "Tests failed"
        return $false
    }
}

# Run tests with coverage
function Invoke-TestsWithCoverage {
    Write-Header "Running tests with coverage"
    
    $result = & dotnet.exe test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Tests with coverage completed successfully"
        return $true
    } else {
        Write-Error "Tests failed or coverage threshold not met"
        return $false
    }
}

# Generate HTML coverage report
function Invoke-CoverageReport {
    Write-Header "Generating HTML coverage report"
    
    # Check if reportgenerator is available
    if (-not (Get-Command "reportgenerator" -ErrorAction SilentlyContinue)) {
        Write-Error "reportgenerator not found. Cannot generate HTML report."
        Write-Info "Install with: dotnet tool install -g dotnet-reportgenerator-globaltool"
        return $false
    }
    
    # Check if coverage files exist
    if (-not (Test-Path "TestResults")) {
        Write-Error "Coverage results directory not found. Run coverage first."
        return $false
    }
    
    # Generate report
    $result = & reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:"Html,HtmlSummary,TextSummary" -verbosity:Info
    if ($LASTEXITCODE -eq 0) {
        Write-Success "HTML coverage report generated in ./coverage-report/"
        return $true
    } else {
        Write-Error "Failed to generate coverage report"
        return $false
    }
}

# Open coverage report in browser
function Open-CoverageReport {
    $reportPath = "coverage-report/index.html"
    if (Test-Path $reportPath) {
        Write-Info "Opening coverage report in browser..."
        if ($IsWindows -or $env:WSL_DISTRO_NAME) {
            # Windows or WSL
            & explorer.exe (Resolve-Path $reportPath).Path
        } elseif ($IsMacOS) {
            # macOS
            & open $reportPath
        } elseif ($IsLinux) {
            # Linux
            & xdg-open $reportPath
        } else {
            Write-Info "Report location: $(Resolve-Path $reportPath)"
        }
    } else {
        Write-Warning "Coverage report not found at $reportPath"
    }
}

# Show interactive menu
function Show-Menu {
    Write-Host "`n🧪 HomeAutomation Test & Coverage Tool" -ForegroundColor Blue
    Write-Host "=====================================" -ForegroundColor Blue
    Write-Host "1. Run tests only"
    Write-Host "2. Run tests with coverage"
    Write-Host "3. Run tests with coverage + generate HTML report"
    Write-Host "4. Run all (tests + coverage + report + open browser)"
    Write-Host "5. Clean coverage artifacts"
    Write-Host "6. Exit"
    Write-Host ""
    
    do {
        $choice = Read-Host "Please select an option (1-6)"
        switch ($choice) {
            "1" { return "test" }
            "2" { return "coverage" }
            "3" { return "report" }
            "4" { return "all" }
            "5" { return "clean" }
            "6" { return "exit" }
            default { Write-Warning "Invalid option. Please select 1-6." }
        }
    } while ($true)
}

# Main execution logic
function Invoke-Main {
    # Validate requirements
    if (-not (Test-Requirements)) {
        exit 1
    }
    
    # Determine action
    $selectedAction = $Action
    if ([string]::IsNullOrEmpty($selectedAction)) {
        $selectedAction = Show-Menu
    }
    
    # Execute selected action
    switch ($selectedAction) {
        "test" {
            $success = Invoke-Tests
            exit ($success ? 0 : 1)
        }
        "coverage" {
            $success = Invoke-TestsWithCoverage
            exit ($success ? 0 : 1)
        }
        "report" {
            if (Invoke-TestsWithCoverage) {
                $success = Invoke-CoverageReport
                exit ($success ? 0 : 1)
            } else {
                exit 1
            }
        }
        "all" {
            if (Invoke-TestsWithCoverage) {
                if (Invoke-CoverageReport) {
                    Open-CoverageReport
                    Write-Success "All operations completed successfully!"
                    exit 0
                } else {
                    exit 1
                }
            } else {
                exit 1
            }
        }
        "clean" {
            Invoke-CleanCoverage
            exit 0
        }
        "exit" {
            Write-Info "Goodbye!"
            exit 0
        }
        default {
            Write-Error "Unknown action: $selectedAction"
            exit 1
        }
    }
}

# Script entry point
Invoke-Main