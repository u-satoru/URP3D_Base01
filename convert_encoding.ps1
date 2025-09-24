$files = Get-ChildItem -Path . -Recurse -File -Include @("*.cs", "*.asmdef", "*.json", "*.md", "*.txt", "*.xml", "*.shader", "*.cginc", "*.hlsl", "*.glsl", "*.uss", "*.uxml")
foreach ($file in $files) {
    $bytes = Get-Content -Path $file.FullName -Encoding Byte -TotalCount 3
    if ($bytes.Count -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        Write-Host "Converting $($file.FullName) from UTF-8-BOM to UTF-8"
        $content = Get-Content -Path $file.FullName -Raw
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
    }
}
Write-Host "Conversion check complete."