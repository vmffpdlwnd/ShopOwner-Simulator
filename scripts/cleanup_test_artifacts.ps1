<#
  cleanup_test_artifacts.ps1
  - Deletes local build/publish artifacts that may contain test 'dummy' files
  - Leaves source files intact
  - Use cautiously; this removes `app`, `bin` and `obj` folders under the repo
#>

Write-Host "Starting cleanup of local build/publish artifacts..."

Push-Location (Split-Path -Parent $MyInvocation.MyCommand.Path) | Out-Null

# Delete root-level app/ (publish output)
if (Test-Path .\app) {
    Write-Host "Removing ./app/"
    Remove-Item -Recurse -Force .\app
} else { Write-Host "No ./app/ folder found" }

# Remove all bin and obj folders under the repository
Get-ChildItem -Recurse -Directory -Force | Where-Object { $_.Name -in 'bin','obj' } | ForEach-Object {
    try {
        Write-Host "Removing: $($_.FullName)"
        Remove-Item -Recurse -Force -LiteralPath $_.FullName
    } catch {
        Write-Warning "Failed to remove $($_.FullName): $_"
    }
}

# Optionally clear known runtime dummy files in source GameData (if present)
$pathsToClear = @(
    '.\ShopOwnerSimulator\GameData\user_data.json',
    '.\ShopOwnerSimulator\GameData\playfab_error.json',
    '.\app\GameData\user_data.json',
    '.\app\GameData\playfab_error.json'
)

foreach ($p in $pathsToClear) {
    if (Test-Path $p) {
        try {
            Write-Host "Clearing: $p"
            Set-Content -Path $p -Value "{}"
        } catch {
            Write-Warning "Failed to clear $p: $_"
        }
    }
}

Write-Host "Cleanup finished. You may want to run 'git status' to review changes." 

Pop-Location | Out-Null
