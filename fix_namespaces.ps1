# PowerShell script to fix namespace references
$directory = "D:\UnityProjects\URP3D_Base01\Assets\_Project\Core"
$files = Get-ChildItem -Path $directory -Recurse -Filter "*.cs"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $original = $content

    # Comment out non-existent namespace references
    $content = $content -replace 'using asterivo\.Unity60\.Core\.Debug;', '// using asterivo.Unity60.Core.Debug;'
    $content = $content -replace 'using asterivo\.Unity60\.Core\.Shared;', '// using asterivo.Unity60.Core.Shared;'
    $content = $content -replace 'using asterivo\.Unity60\.Core\.Data;', '// using asterivo.Unity60.Core.Data;'
    $content = $content -replace 'using asterivo\.Unity60\.Core\.Commands;', '// using asterivo.Unity60.Core.Commands;'
    $content = $content -replace 'using asterivo\.Unity60\.Core\.Lifecycle;', '// using asterivo.Unity60.Core.Lifecycle;'
    $content = $content -replace 'using asterivo\.Unity60\.Core\.Components;', '// using asterivo.Unity60.Core.Components;'
    $content = $content -replace 'using asterivo\.Unity60\.Core\.Helpers;', '// using asterivo.Unity60.Core.Helpers;'
    $content = $content -replace 'using asterivo\.Unity60\.Core\.Patterns;', '// using asterivo.Unity60.Core.Patterns;'

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Fixed: $($file.Name)"
    }
}

Write-Host "Namespace fixes complete!"