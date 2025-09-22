# Template層のusing文を一括追加するPowerShellスクリプト

$templatesPath = "D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Templates"

# 追加すべきusing文のマッピング
$usingMappings = @{
    "DetailedPlayerStateMachine" = "using asterivo.Unity60.Features.Player.States;"
    "PlayerStateType" = "using asterivo.Unity60.Features.Player.States;"
    "IPlayerState" = "using asterivo.Unity60.Features.Player.States;"
    "PlayerStateMachine" = "using asterivo.Unity60.Features.Player.States;"
    "StealthMovementController" = "using asterivo.Unity60.Features.Player.Stealth;"
    "IEventLogger" = "using asterivo.Unity60.Core.Debug;"
    "CoverSystem" = "using asterivo.Unity60.Features.Player.Stealth;"
}

# 各ファイルを処理する関数
function Add-MissingUsings {
    param (
        [string]$FilePath
    )

    $content = Get-Content $FilePath -Raw -ErrorAction SilentlyContinue
    if (-not $content) {
        return
    }

    $lines = Get-Content $FilePath -ErrorAction SilentlyContinue
    if (-not $lines) {
        return
    }

    # ファイルに必要なusing文を特定
    $requiredUsings = @()

    foreach ($key in $usingMappings.Keys) {
        $pattern = "\b$key\b"
        $usingStatement = $usingMappings[$key]
        $escapedUsing = [regex]::Escape($usingStatement)

        if (($content -match $pattern) -and ($content -notmatch $escapedUsing)) {
            $requiredUsings += $usingStatement
        }
    }

    if ($requiredUsings.Count -gt 0) {
        Write-Host "修正中: $FilePath"

        # namespace行を探す
        $namespaceLineIndex = -1
        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match "^namespace ") {
                $namespaceLineIndex = $i
                break
            }
        }

        if ($namespaceLineIndex -gt 0) {
            # using文を追加（namespaceの前に）
            $newLines = @()

            # 既存のusing文をコピー
            for ($i = 0; $i -lt $namespaceLineIndex; $i++) {
                $newLines += $lines[$i]
            }

            # 新しいusing文を追加（重複チェック）
            foreach ($usingStmt in $requiredUsings) {
                $escapedStmt = [regex]::Escape($usingStmt)
                if ($content -notmatch $escapedStmt) {
                    $newLines += $usingStmt
                }
            }

            # 残りの行をコピー
            for ($i = $namespaceLineIndex; $i -lt $lines.Count; $i++) {
                $newLines += $lines[$i]
            }

            # ファイルを保存
            $newLines | Set-Content $FilePath -Encoding UTF8
            Write-Host "  - 追加したusing文: $($requiredUsings.Count)個"
        }
    }
}

# Template層のすべての.csファイルを処理
$files = Get-ChildItem -Path $templatesPath -Filter "*.cs" -Recurse

Write-Host "Template層のファイル数: $($files.Count)"
Write-Host "修正を開始します..."

foreach ($file in $files) {
    Add-MissingUsings -FilePath $file.FullName
}

Write-Host "`n修正完了！"