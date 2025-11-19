#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Comprehensive test execution script for Identity Service
.DESCRIPTION
    This script runs all test categories for the Identity Service including unit tests,
    integration tests, performance tests, and security tests with detailed reporting.
.PARAMETER TestType
    Specifies which test type to run: All, Unit, Integration, Performance, Security
.PARAMETER Coverage
    Enables code coverage collection
.PARAMETER DetailedOutput
    Enables detailed output
.EXAMPLE
    .\run-tests.ps1 -TestType All -Coverage -DetailedOutput
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("All", "Unit", "Integration", "Performance", "Security")]
    [string]$TestType = "All",
    
    [Parameter(Mandatory=$false)]
    [switch]$Coverage,
    
    [Parameter(Mandatory=$false)]
    [switch]$DetailedOutput
)

# Get the script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $ScriptDir))

# Test project paths
$TestProjects = @{
    Unit = "tests/Services/IdentityService/IdentityService.UnitTests"
    Integration = "tests/Services/IdentityService/IdentityService.IntegrationTests"
    Performance = "tests/Services/IdentityService/IdentityService.PerformanceTests"
    Security = "tests/Services/IdentityService/IdentityService.SecurityTests"
}

# Colors for output
$Green = "Green"
$Red = "Red"
$Yellow = "Yellow"
$Cyan = "Cyan"

function Write-Header {
    param([string]$Message)
    Write-Host "`n$('=' * 60)" -ForegroundColor $Cyan
    Write-Host $Message -ForegroundColor $Cyan
    Write-Host $('=' * 60) -ForegroundColor $Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor $Yellow
}

function Run-TestProject {
    param(
        [string]$ProjectPath,
        [string]$TestName,
        [bool]$EnableCoverage = $false,
        [bool]$DetailedOutputEnabled = $false
    )

    Write-Header "Running $TestName Tests"
    
    $FullPath = Join-Path $RootDir $ProjectPath
    
    if (-not (Test-Path $FullPath)) {
        Write-Error "Test project not found: $FullPath"
        return $false
    }

    # Build test command
    $TestArgs = @("test", $FullPath)
    
    if ($EnableCoverage) {
        $TestArgs += "--collect:XPlat Code Coverage"
        $TestArgs += "--results-directory:TestResults"
    }
    
    if ($DetailedOutputEnabled) {
        $TestArgs += "--verbosity:normal"
    } else {
        $TestArgs += "--verbosity:minimal"
    }
    
    $TestArgs += "--logger:console;verbosity=detailed"
    
    Write-Host "Executing: dotnet $($TestArgs -join ' ')" -ForegroundColor $Yellow
    
    $StartTime = Get-Date
    
    try {
        & dotnet @TestArgs
        $ExitCode = $LASTEXITCODE
        
        $EndTime = Get-Date
        $Duration = $EndTime - $StartTime
        
        if ($ExitCode -eq 0) {
            Write-Success "$TestName tests completed successfully in $($Duration.TotalSeconds.ToString('F2')) seconds"
            return $true
        } else {
            Write-Error "$TestName tests failed with exit code $ExitCode"
            return $false
        }
    }
    catch {
        Write-Error "Error running $TestName tests: $_"
        return $false
    }
}

function Generate-CoverageReport {
    Write-Header "Generating Code Coverage Report"
    
    $CoverageFiles = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse
    
    if ($CoverageFiles.Count -eq 0) {
        Write-Warning "No coverage files found"
        return
    }
    
    # Check if reportgenerator is installed
    $ReportGenerator = Get-Command "reportgenerator" -ErrorAction SilentlyContinue
    
    if (-not $ReportGenerator) {
        Write-Warning "reportgenerator tool not found. Installing..."
        dotnet tool install -g dotnet-reportgenerator-globaltool
    }
    
    $Reports = ($CoverageFiles | ForEach-Object { $_.FullName }) -join ";"
    $TargetDir = "TestResults/CoverageReport"
    
    try {
        reportgenerator -reports:$Reports -targetdir:$TargetDir -reporttypes:Html
        Write-Success "Coverage report generated: $TargetDir/index.html"
        
        # Try to open the report
        if ($IsWindows) {
            Start-Process "$TargetDir/index.html"
        }
    }
    catch {
        Write-Error "Failed to generate coverage report: $_"
    }
}

function Show-TestSummary {
    param(
        [hashtable]$Results,
        [timespan]$TotalDuration
    )
    
    Write-Header "Test Execution Summary"
    
    $SuccessCount = ($Results.Values | Where-Object { $_ }).Count
    $TotalCount = $Results.Count
    
    foreach ($TestType in $Results.Keys) {
        $Status = if ($Results[$TestType]) { "✅ PASSED" } else { "❌ FAILED" }
        Write-Host "$TestType Tests: $Status"
    }
    
    Write-Host "`nOverall: $SuccessCount/$TotalCount test suites passed" -ForegroundColor $(if ($SuccessCount -eq $TotalCount) { $Green } else { $Red })
    Write-Host "Total Duration: $($TotalDuration.TotalSeconds.ToString('F2')) seconds" -ForegroundColor $Cyan
    
    if ($SuccessCount -eq $TotalCount) {
        Write-Success "🎉 All test suites passed successfully!"
    } else {
        Write-Error "💥 Some test suites failed. Please review the output above."
    }
}

# Main execution
$StartTime = Get-Date
$Results = @{}

Write-Header "Identity Service Test Suite Execution"
Write-Host "Test Type: $TestType" -ForegroundColor $Cyan
Write-Host "Coverage: $($Coverage.IsPresent)" -ForegroundColor $Cyan
Write-Host "Detailed Output: $($DetailedOutput.IsPresent)" -ForegroundColor $Cyan

# Clean up previous test results
if (Test-Path "TestResults") {
    Remove-Item "TestResults" -Recurse -Force
    Write-Host "Cleaned up previous test results" -ForegroundColor $Yellow
}

try {
    switch ($TestType) {
        "All" {
            $Results["Unit"] = Run-TestProject -ProjectPath $TestProjects.Unit -TestName "Unit" -EnableCoverage $Coverage.IsPresent -DetailedOutputEnabled $DetailedOutput.IsPresent
            $Results["Integration"] = Run-TestProject -ProjectPath $TestProjects.Integration -TestName "Integration" -EnableCoverage $Coverage.IsPresent -DetailedOutputEnabled $DetailedOutput.IsPresent
            $Results["Performance"] = Run-TestProject -ProjectPath $TestProjects.Performance -TestName "Performance" -EnableCoverage $Coverage.IsPresent -DetailedOutputEnabled $DetailedOutput.IsPresent
            $Results["Security"] = Run-TestProject -ProjectPath $TestProjects.Security -TestName "Security" -EnableCoverage $Coverage.IsPresent -DetailedOutputEnabled $DetailedOutput.IsPresent
        }
        "Unit" {
            $Results["Unit"] = Run-TestProject -ProjectPath $TestProjects.Unit -TestName "Unit" -EnableCoverage $Coverage.IsPresent -DetailedOutputEnabled $DetailedOutput.IsPresent
        }
        "Integration" {
            $Results["Integration"] = Run-TestProject -ProjectPath $TestProjects.Integration -TestName "Integration" -EnableCoverage $Coverage.IsPresent -DetailedOutputEnabled $DetailedOutput.IsPresent
        }
        "Performance" {
            $Results["Performance"] = Run-TestProject -ProjectPath $TestProjects.Performance -TestName "Performance" -EnableCoverage $Coverage.IsPresent -DetailedOutputEnabled $DetailedOutput.IsPresent
        }
        "Security" {
            $Results["Security"] = Run-TestProject -ProjectPath $TestProjects.Security -TestName "Security" -EnableCoverage $Coverage.IsPresent -DetailedOutputEnabled $DetailedOutput.IsPresent
        }
    }

    # Generate coverage report if coverage was collected
    if ($Coverage.IsPresent -and (Test-Path "TestResults")) {
        Generate-CoverageReport
    }

    # Show summary
    $EndTime = Get-Date
    $TotalDuration = $EndTime - $StartTime
    Show-TestSummary -Results $Results -TotalDuration $TotalDuration

    # Exit with appropriate code
    $FailedTests = $Results.Values | Where-Object { -not $_ }
    if ($FailedTests.Count -eq 0) {
        exit 0
    } else {
        exit 1
    }
}
catch {
    Write-Error "Unexpected error during test execution: $_"
    exit 1
}