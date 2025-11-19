# PowerShell build/orchestration script for Acme Platform
param(
    [string]$Action = "build"
)

switch ($Action) {
    "build" { dotnet build Acme.Platform.sln --configuration Release }
    "test" { dotnet test Acme.Platform.sln --no-build --configuration Release --collect:"XPlat Code Coverage" }
    "lint" { dotnet format --verify-no-changes }
    "docker-up" { docker-compose up -d }
    "docker-down" { docker-compose down }
    "clean" {
        dotnet clean Acme.Platform.sln
        Get-ChildItem -Recurse -Include bin,obj | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    }
    default { Write-Host "Unknown action: $Action" }
}
