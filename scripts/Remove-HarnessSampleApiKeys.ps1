[CmdletBinding()]
param(
    [ValidateSet("Process", "User")]
    [string]$Scope = "User"
)

$names = @(
    "TAVILY_API_KEY",
    "TINYFISH_API_KEY",
    "TINYFISH_LOCATION",
    "TINYFISH_LANGUAGE"
)

foreach ($name in $names) {
    Remove-Item -Path "Env:$name" -ErrorAction SilentlyContinue

    if ($Scope -eq "User") {
        [Environment]::SetEnvironmentVariable($name, $null, [EnvironmentVariableTarget]::User)
    }

    Write-Host "Removed $name ($Scope)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "HarnessSample 用の環境変数を削除しました。" -ForegroundColor Cyan
if ($Scope -eq "User") {
    Write-Host "既存のターミナルには古い値が残ることがあるため、新しいターミナルを開いて確認してください。" -ForegroundColor Cyan
}
else {
    Write-Host "Process スコープの削除は現在の PowerShell セッションにのみ反映されます。" -ForegroundColor Cyan
}
