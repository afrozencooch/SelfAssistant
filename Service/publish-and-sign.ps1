param(
    [string]$ProjectDir = "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)",
    [string]$Configuration = 'Release',
    [string]$Output = "$env:TEMP\SelfAssistant_Publish",
    [string]$CertificatePath = '',
    [string]$CertificatePassword = '',
    [string]$CertThumbprint = ''
)

Write-Host "Publishing project from: $ProjectDir"
dotnet publish "$ProjectDir" -c $Configuration -o "$Output"

$exe = Join-Path $Output 'SelfAssistant.Service.exe'
if (-not (Test-Path $exe)) {
    Write-Error "Published executable not found at $exe"
    exit 1
}

if ($CertificatePath -or $CertThumbprint) {
    Write-Host "Signing published executable..."

    # Try to find signtool
    $signtoolPaths = @(
        "$env:ProgramFiles(x86)\Windows Kits\10\bin\x64\signtool.exe",
        "$env:ProgramFiles\Windows Kits\10\bin\x64\signtool.exe",
        "C:\Program Files (x86)\Windows Kits\8.1\bin\x64\signtool.exe",
        "C:\Program Files\Windows Kits\8.1\bin\x64\signtool.exe"
    )
    $signtool = $signtoolPaths | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $signtool) {
        Write-Warning "signtool.exe not found. Skipping signing. Install Windows SDK to get signtool.exe."
        exit 0
    }

    if ($CertificatePath) {
        $passwordArg = ''
        if ($CertificatePassword) { $passwordArg = "/p $CertificatePassword" }
        & $signtool sign /f "$CertificatePath" $passwordArg /tr "http://timestamp.digicert.com" /td sha256 /fd sha256 "$exe"
    }
    else {
        # Sign using certificate in store by thumbprint
        & $signtool sign /sha1 $CertThumbprint /tr "http://timestamp.digicert.com" /td sha256 /fd sha256 "$exe"
    }
    if ($LASTEXITCODE -ne 0) { Write-Warning "signtool reported non-zero exit code ($LASTEXITCODE)." }
    else { Write-Host "Signing completed." }
}
else {
    Write-Host "No certificate provided; skipping signing." 
}

Write-Host "Published output is at: $Output"
