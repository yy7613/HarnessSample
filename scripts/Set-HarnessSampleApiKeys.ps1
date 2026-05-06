[CmdletBinding()]
param(
    [string]$TavilyApiKey,
    [string]$TinyFishApiKey,
    [string]$TinyFishLocation = "JP",
    [string]$TinyFishLanguage = "ja",
    [ValidateSet("Process", "User")]
    [string]$Scope = "User"
)

function Set-HarnessEnvironmentVariable {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [string]$Value,

        [Parameter(Mandatory = $true)]
        [ValidateSet("Process", "User")]
        [string]$TargetScope
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return
    }

    Set-Item -Path "Env:$Name" -Value $Value

    if ($TargetScope -eq "User") {
        [Environment]::SetEnvironmentVariable($Name, $Value, [EnvironmentVariableTarget]::User)
    }

    Write-Host "Set $Name ($TargetScope)" -ForegroundColor Green
}

if ([string]::IsNullOrWhiteSpace($TavilyApiKey) -and [string]::IsNullOrWhiteSpace($TinyFishApiKey)) {
    Write-Warning "TavilyApiKey または TinyFishApiKey のどちらかを指定してください。"
    Write-Host "例:" -ForegroundColor Yellow
    Write-Host "  .\scripts\Set-HarnessSampleApiKeys.ps1 -TavilyApiKey 'tvly-xxx' -Scope User"
    Write-Host "  .\scripts\Set-HarnessSampleApiKeys.ps1 -TinyFishApiKey 'tf-xxx' -TinyFishLocation 'JP' -TinyFishLanguage 'ja' -Scope User"
    exit 1
}

Set-HarnessEnvironmentVariable -Name "TAVILY_API_KEY" -Value $TavilyApiKey -TargetScope $Scope
Set-HarnessEnvironmentVariable -Name "TINYFISH_API_KEY" -Value $TinyFishApiKey -TargetScope $Scope
Set-HarnessEnvironmentVariable -Name "TINYFISH_LOCATION" -Value $TinyFishLocation -TargetScope $Scope
Set-HarnessEnvironmentVariable -Name "TINYFISH_LANGUAGE" -Value $TinyFishLanguage -TargetScope $Scope

Write-Host ""
Write-Host "HarnessSample 用の API キー設定が完了しました。" -ForegroundColor Cyan
Write-Host "- TAVILY_API_KEY     : $(if ($TavilyApiKey) { 'configured' } else { 'skipped' })"
Write-Host "- TINYFISH_API_KEY  : $(if ($TinyFishApiKey) { 'configured' } else { 'skipped' })"
Write-Host "- TINYFISH_LOCATION : $(if ($TinyFishLocation) { $TinyFishLocation } else { 'skipped' })"
Write-Host "- TINYFISH_LANGUAGE : $(if ($TinyFishLanguage) { $TinyFishLanguage } else { 'skipped' })"

if ($Scope -eq "User") {
    Write-Host "新しいターミナルを開くと User 環境変数としても利用できます。" -ForegroundColor Cyan
}
else {
    Write-Host "Process スコープは現在の PowerShell セッションでのみ有効です。必要に応じてドットソース実行してください。" -ForegroundColor Cyan
    Write-Host "  . .\scripts\Set-HarnessSampleApiKeys.ps1 -TavilyApiKey 'tvly-xxx' -Scope Process"
}
