[CmdletBinding()]
param(
    [string]$OutputPath = "",
    [string]$TavilyApiKey = "",
    [string]$TinyFishApiKey = "",
    [string]$TinyFishLocation = "JP",
    [string]$TinyFishLanguage = "ja",
    [switch]$Force
)

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path (Split-Path -Parent $PSScriptRoot) ".env"
}

if ((Test-Path $OutputPath) -and -not $Force) {
    Write-Warning "$OutputPath は既に存在します。上書きする場合は -Force を指定してください。"
    exit 1
}

$content = @"
# HarnessSample Web Search Tool settings
# 実際のキーを設定して使用してください
TAVILY_API_KEY=$TavilyApiKey
TINYFISH_API_KEY=$TinyFishApiKey
TINYFISH_LOCATION=$TinyFishLocation
TINYFISH_LANGUAGE=$TinyFishLanguage
"@

Set-Content -Path $OutputPath -Value $content -Encoding UTF8
Write-Host "$OutputPath を作成しました。" -ForegroundColor Green
Write-Host "既定ではリポジトリ直下の .env を作成します。" -ForegroundColor Cyan
Write-Host "実際の API キーを設定後、このファイル自体は Git へコミットしないでください。" -ForegroundColor Cyan
