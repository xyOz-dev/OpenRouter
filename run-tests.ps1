#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Runs the OpenRouter .NET SDK test suite
.DESCRIPTION
    This script runs unit tests, integration tests, and generates coverage reports
.PARAMETER TestType
    Type of tests to run: Unit, Integration, or All (default: All)
.PARAMETER Coverage
    Generate code coverage report (default: false)
.PARAMETER ApiKey
    OpenRouter API key for integration tests
.EXAMPLE
    ./run-tests.ps1 -TestType Unit
    ./run-tests.ps1 -TestType Integration -ApiKey "sk-or-v1-..."
    ./run-tests.ps1 -Coverage
#>

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("Unit", "Integration", "All")]
    [string]$TestType = "All",
    
    [Parameter(Mandatory = $false)]
    [switch]$Coverage = $false,
    
    [Parameter(Mandatory = $false)]
    [string]$ApiKey = $env:OPENROUTER_API_KEY
)

# Colors for output
$Red = [ConsoleColor]::Red
$Green = [ConsoleColor]::Green
$Yellow = [ConsoleColor]::Yellow
$Blue = [ConsoleColor]::Blue
$White = [ConsoleColor]::White

function Write-ColoredOutput {
    param(
        [string]$Message,
        [ConsoleColor]$Color = $White
    )
    Write-Host $Message -ForegroundColor $Color
}

function Test-Prerequisites {
    Write-ColoredOutput "Checking prerequisites..." $Blue
    
    # Check .NET SDK
    try {
        $dotnetVersion = dotnet --version
        Write-ColoredOutput "✓ .NET SDK version: $dotnetVersion" $Green
    }
    catch {
        Write-ColoredOutput "✗ .NET SDK not found. Please install .NET 9.0 or later." $Red
        exit 1
    }
    
    # Check if solution exists
    if (-not (Test-Path "OpenRouter.sln")) {
        Write-ColoredOutput "✗ OpenRouter.sln not found. Please run from the solution root directory." $Red
        exit 1
    }
    
    Write-ColoredOutput "✓ Prerequisites check passed" $Green
}

function Restore-Dependencies {
    Write-ColoredOutput "Restoring NuGet packages..." $Blue
    
    try {
        dotnet restore --verbosity minimal
        Write-ColoredOutput "✓ NuGet packages restored successfully" $Green
    }
    catch {
        Write-ColoredOutput "✗ Failed to restore NuGet packages" $Red
        exit 1
    }
}

function Build-Solution {
    Write-ColoredOutput "Building solution..." $Blue
    
    try {
        dotnet build --no-restore --configuration Release --verbosity minimal
        Write-ColoredOutput "✓ Solution built successfully" $Green
    }
    catch {
        Write-ColoredOutput "✗ Build failed" $Red
        exit 1
    }
}

function Run-UnitTests {
    Write-ColoredOutput "Running unit tests..." $Blue
    
    $filter = "Category=Unit"
    $testCommand = "dotnet test OpenRouter.Tests --no-build --configuration Release --filter `"$filter`""
    
    if ($Coverage) {
        $testCommand += " --collect:`"XPlat Code Coverage`" --results-directory TestResults"
    }
    
    try {
        Invoke-Expression $testCommand
        Write-ColoredOutput "✓ Unit tests completed" $Green
    }
    catch {
        Write-ColoredOutput "✗ Unit tests failed" $Red
        return $false
    }
    
    return $true
}

function Run-IntegrationTests {
    Write-ColoredOutput "Running integration tests..." $Blue
    
    if (-not $ApiKey) {
        Write-ColoredOutput "⚠ No API key provided. Integration tests will be skipped." $Yellow
        Write-ColoredOutput "  Set OPENROUTER_API_KEY environment variable or use -ApiKey parameter." $Yellow
        return $true
    }
    
    # Set API key for tests
    $env:OPENROUTER_API_KEY = $ApiKey
    
    $filter = "Category=Integration"
    $testCommand = "dotnet test OpenRouter.Tests --no-build --configuration Release --filter `"$filter`""
    
    if ($Coverage) {
        $testCommand += " --collect:`"XPlat Code Coverage`" --results-directory TestResults"
    }
    
    try {
        Invoke-Expression $testCommand
        Write-ColoredOutput "✓ Integration tests completed" $Green
    }
    catch {
        Write-ColoredOutput "✗ Integration tests failed" $Red
        return $false
    }
    
    return $true
}

function Run-AllTests {
    Write-ColoredOutput "Running all tests..." $Blue
    
    $testCommand = "dotnet test OpenRouter.Tests --no-build --configuration Release"
    
    if ($Coverage) {
        $testCommand += " --collect:`"XPlat Code Coverage`" --results-directory TestResults"
    }
    
    # Set API key if provided
    if ($ApiKey) {
        $env:OPENROUTER_API_KEY = $ApiKey
    }
    else {
        Write-ColoredOutput "⚠ No API key provided. Integration tests may fail." $Yellow
    }
    
    try {
        Invoke-Expression $testCommand
        Write-ColoredOutput "✓ All tests completed" $Green
    }
    catch {
        Write-ColoredOutput "✗ Some tests failed" $Red
        return $false
    }
    
    return $true
}

function Generate-CoverageReport {
    if (-not $Coverage) {
        return
    }
    
    Write-ColoredOutput "Generating coverage report..." $Blue
    
    # Check if reportgenerator tool is installed
    try {
        dotnet tool list -g | Select-String "dotnet-reportgenerator-globaltool" | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-ColoredOutput "Installing ReportGenerator tool..." $Blue
            dotnet tool install -g dotnet-reportgenerator-globaltool
        }
    }
    catch {
        Write-ColoredOutput "Installing ReportGenerator tool..." $Blue
        dotnet tool install -g dotnet-reportgenerator-globaltool
    }
    
    # Generate HTML report
    try {
        $coverageFiles = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse
        if ($coverageFiles.Count -eq 0) {
            Write-ColoredOutput "⚠ No coverage files found" $Yellow
            return
        }
        
        $coverageFile = $coverageFiles[0].FullName
        reportgenerator "-reports:$coverageFile" "-targetdir:TestResults/Coverage" "-reporttypes:Html;Cobertura"
        
        Write-ColoredOutput "✓ Coverage report generated in TestResults/Coverage/" $Green
        Write-ColoredOutput "  Open TestResults/Coverage/index.html to view the report" $Blue
    }
    catch {
        Write-ColoredOutput "✗ Failed to generate coverage report" $Red
    }
}

function Main {
    Write-ColoredOutput "OpenRouter .NET SDK Test Runner" $Blue
    Write-ColoredOutput "===============================" $Blue
    Write-ColoredOutput ""
    
    Test-Prerequisites
    Restore-Dependencies
    Build-Solution
    
    $success = $true
    
    switch ($TestType) {
        "Unit" {
            $success = Run-UnitTests
        }
        "Integration" {
            $success = Run-IntegrationTests
        }
        "All" {
            $success = Run-AllTests
        }
    }
    
    Generate-CoverageReport
    
    Write-ColoredOutput ""
    if ($success) {
        Write-ColoredOutput "🎉 All tests passed!" $Green
        exit 0
    }
    else {
        Write-ColoredOutput "💥 Some tests failed!" $Red
        exit 1
    }
}

# Run the main function
Main