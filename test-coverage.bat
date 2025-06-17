@echo off
REM Test and coverage script for HomeAutomation NetDaemon project
REM Provides easy access to test execution, code coverage analysis, and HTML report generation.

setlocal EnableDelayedExpansion

REM Color definitions (for Windows 10+)
set "GREEN=[92m"
set "RED=[91m"
set "YELLOW=[93m"
set "CYAN=[96m"
set "MAGENTA=[95m"
set "RESET=[0m"

REM Parse command line argument
set "ACTION=%~1"

REM Main entry point
if "%ACTION%"=="" goto :ShowMenu
if /i "%ACTION%"=="test" goto :RunTests
if /i "%ACTION%"=="coverage" goto :RunCoverage
if /i "%ACTION%"=="report" goto :RunReport
if /i "%ACTION%"=="all" goto :RunAll
if /i "%ACTION%"=="clean" goto :CleanCoverage
echo %RED%Unknown action: %ACTION%%RESET%
goto :Usage

:ShowMenu
echo.
echo %CYAN%🧪 HomeAutomation Test ^& Coverage Tool%RESET%
echo %CYAN%=====================================%RESET%
echo 1. Run tests only
echo 2. Run tests with coverage
echo 3. Run tests with coverage + generate HTML report
echo 4. Run all (tests + coverage + report + open browser)
echo 5. Clean coverage artifacts
echo 6. Exit
echo.
set /p "choice=Please select an option (1-6): "

if "%choice%"=="1" goto :RunTests
if "%choice%"=="2" goto :RunCoverage
if "%choice%"=="3" goto :RunReport
if "%choice%"=="4" goto :RunAll
if "%choice%"=="5" goto :CleanCoverage
if "%choice%"=="6" goto :Exit
echo %YELLOW%Invalid option. Please select 1-6.%RESET%
goto :ShowMenu

:CheckRequirements
echo %CYAN%ℹ️  Checking requirements...%RESET%
where dotnet.exe >nul 2>&1
if errorlevel 1 (
    echo %RED%❌ dotnet.exe not found. Please install .NET SDK.%RESET%
    exit /b 1
)

where reportgenerator >nul 2>&1
if errorlevel 1 (
    echo %YELLOW%⚠️  reportgenerator not found. HTML reports will not be available.%RESET%
    echo %CYAN%ℹ️  Install with: dotnet tool install -g dotnet-reportgenerator-globaltool%RESET%
)
exit /b 0

:CleanCoverage
call :CheckRequirements
if errorlevel 1 exit /b 1

echo.
echo %MAGENTA%🔧 Cleaning coverage artifacts%RESET%

if exist coverage (
    echo %CYAN%ℹ️  Removing folder: coverage%RESET%
    rmdir /s /q coverage
)

if exist coverage-report (
    echo %CYAN%ℹ️  Removing folder: coverage-report%RESET%
    rmdir /s /q coverage-report
)

if exist TestResults (
    echo %CYAN%ℹ️  Removing folder: TestResults%RESET%
    rmdir /s /q TestResults
)

for %%f in (coverage.* *.coverage *.coveragexml) do (
    if exist "%%f" (
        echo %CYAN%ℹ️  Removing file: %%f%RESET%
        del /q "%%f"
    )
)

echo %GREEN%✅ Coverage artifacts cleaned%RESET%
goto :Exit

:RunTests
call :CheckRequirements
if errorlevel 1 exit /b 1

echo.
echo %MAGENTA%🔧 Running tests%RESET%

dotnet.exe test --logger "console;verbosity=normal"
if errorlevel 1 (
    echo %RED%❌ Tests failed%RESET%
    exit /b 1
) else (
    echo %GREEN%✅ Tests completed successfully%RESET%
)
goto :Exit

:RunCoverage
call :CheckRequirements
if errorlevel 1 exit /b 1

echo.
echo %MAGENTA%🔧 Running tests with coverage%RESET%

dotnet.exe test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
if errorlevel 1 (
    echo %YELLOW%⚠️  Some tests failed, but coverage data was still collected%RESET%
    set "TESTS_FAILED=1"
) else (
    echo %GREEN%✅ Tests with coverage completed successfully%RESET%
    set "TESTS_FAILED=0"
)
goto :Exit

:RunReport
call :CheckRequirements
if errorlevel 1 exit /b 1

echo.
echo %MAGENTA%🔧 Running tests with coverage%RESET%

dotnet.exe test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
if errorlevel 1 (
    echo %YELLOW%⚠️  Some tests failed, but coverage data was still collected%RESET%
    set "TESTS_FAILED=1"
) else (
    echo %GREEN%✅ Tests with coverage completed successfully%RESET%
    set "TESTS_FAILED=0"
)

echo.
echo %MAGENTA%🔧 Generating HTML coverage report%RESET%

where reportgenerator >nul 2>&1
if errorlevel 1 (
    echo %RED%❌ reportgenerator not found. Cannot generate HTML report.%RESET%
    echo %CYAN%ℹ️  Install with: dotnet tool install -g dotnet-reportgenerator-globaltool%RESET%
    exit /b 1
)

if not exist "TestResults" (
    echo %RED%❌ Coverage results not found. Coverage collection may have failed.%RESET%
    exit /b 1
)

reportgenerator -reports:TestResults\*\coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html,HtmlSummary,TextSummary -verbosity:Info
if errorlevel 1 (
    echo %RED%❌ Failed to generate coverage report%RESET%
    exit /b 1
) else (
    if "%TESTS_FAILED%"=="1" (
        echo %YELLOW%⚠️  HTML coverage report generated despite test failures in .\coverage-report\%RESET%
        echo %CYAN%ℹ️  Review the report to identify areas needing more test coverage%RESET%
    ) else (
        echo %GREEN%✅ HTML coverage report generated in .\coverage-report\%RESET%
    )
)
goto :Exit

:RunAll
call :CheckRequirements
if errorlevel 1 exit /b 1

echo.
echo %MAGENTA%🔧 Running tests with coverage%RESET%

dotnet.exe test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
if errorlevel 1 (
    echo %YELLOW%⚠️  Some tests failed, but coverage data was still collected%RESET%
    set "TESTS_FAILED=1"
) else (
    echo %GREEN%✅ Tests with coverage completed successfully%RESET%
    set "TESTS_FAILED=0"
)

echo.
echo %MAGENTA%🔧 Generating HTML coverage report%RESET%

where reportgenerator >nul 2>&1
if errorlevel 1 (
    echo %RED%❌ reportgenerator not found. Cannot generate HTML report.%RESET%
    echo %CYAN%ℹ️  Install with: dotnet tool install -g dotnet-reportgenerator-globaltool%RESET%
    exit /b 1
)

if not exist "TestResults" (
    echo %RED%❌ Coverage results not found. Coverage collection may have failed.%RESET%
    exit /b 1
)

reportgenerator -reports:TestResults\*\coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html,HtmlSummary,TextSummary -verbosity:Info
if errorlevel 1 (
    echo %RED%❌ Failed to generate coverage report%RESET%
    exit /b 1
) else (
    if "%TESTS_FAILED%"=="1" (
        echo %YELLOW%⚠️  HTML coverage report generated despite test failures in .\coverage-report\%RESET%
        echo %CYAN%ℹ️  Review the report to identify areas needing more test coverage%RESET%
    ) else (
        echo %GREEN%✅ HTML coverage report generated in .\coverage-report\%RESET%
    )
)

if exist "coverage-report\index.html" (
    echo %CYAN%ℹ️  Opening coverage report in browser...%RESET%
    start "" "coverage-report\index.html"
    if "%TESTS_FAILED%"=="1" (
        echo %YELLOW%⚠️  Note: Some tests failed. Use the coverage report to identify which areas need attention.%RESET%
    )
) else (
    echo %YELLOW%⚠️  Coverage report not found at coverage-report\index.html%RESET%
)

if "%TESTS_FAILED%"=="1" (
    echo %YELLOW%⚠️  All operations completed with test failures - coverage report is available%RESET%
) else (
    echo %GREEN%✅ All operations completed successfully!%RESET%
)
goto :Exit

:Usage
echo.
echo Usage: %~n0 [ACTION]
echo.
echo Actions:
echo   test     - Run tests only
echo   coverage - Run tests with coverage
echo   report   - Run tests with coverage + generate HTML report
echo   all      - Run all (tests + coverage + report + open browser)
echo   clean    - Clean coverage artifacts
echo.
echo Examples:
echo   %~n0           (shows interactive menu)
echo   %~n0 test      (runs tests only)
echo   %~n0 all       (runs full coverage workflow)
goto :Exit

:Exit
echo.
exit /b 0