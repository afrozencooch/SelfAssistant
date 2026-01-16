param(
    [string]$ServiceName = 'SelfAssistant'
)

Write-Host "Stopping service (if running)..."
sc.exe stop $ServiceName 2>$null
Start-Sleep -Seconds 1

Write-Host "Deleting service..."
sc.exe delete $ServiceName

Write-Host "Service $ServiceName removed. Remove published files manually if desired." 
