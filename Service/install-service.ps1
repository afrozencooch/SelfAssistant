param(
    [string]$ServiceName = 'SelfAssistant',
    [string]$ProjectPath = "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)",
    [string]$PublishDir = "$env:TEMP\SelfAssistant_Publish"
)

Write-Host "Publishing service..."
dotnet publish "$ProjectPath" -c Release -o "$PublishDir"

$exe = Join-Path $PublishDir 'SelfAssistant.Service.exe'
if (-not (Test-Path $exe)) {
    Write-Error "Published executable not found at $exe"
    exit 1
}

Write-Host "Creating Windows service '$ServiceName' pointing to $exe"
sc.exe create $ServiceName binPath= "`"$exe`"" start= auto DisplayName= "SelfAssistant Service"
sc.exe description $ServiceName "SelfAssistant background service for local chat backend"

Write-Host "Starting service..."
sc.exe start $ServiceName

Write-Host "Service installed and started. Use uninstall-service.ps1 to remove it." 
