# convert_encoding.py
import os

# 文字化けが確認されたファイルのリスト
files_to_convert = [
    # r"" は、バックスラッシュをエスケープ文字として扱わないための記述です
    r"D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\AmbientManager.cs",
    r"D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\AudioManager.cs",
    r"D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\BGMManager.cs",
    r"D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\Controllers\MaskingEffectController.cs",
    r"D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\Controllers\TimeAmbientController.cs",
    r"D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\Controllers\WeatherAmbientController.cs",
    r"D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\DynamicAudioEnvironment.cs",
    r"D:\UnityProjects\URP3D_Base01\Assets\_Project\Core\Audio\Services\AudioService.cs",
    r"D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Player\Scripts\States\CoverState.cs",
    r"D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Player\Scripts\States\JumpingState.cs",
    r"D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Player\Scripts\States\ProneState.cs",
    r"D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Player\Scripts\States\RollingState.cs",
    r"D:\UnityProjects\URP3D_Base01\Assets\_Project\Features\Player\Scripts\States\WalkingState.cs"
]

# 変換元の文字コード (Windowsの日本語環境で標準的なShift-JIS)
source_encoding = 'cp932'
# 変換先の文字コード (BOM無しのUTF-8)
target_encoding = 'utf-8'

print(f"Starting conversion of {len(files_to_convert)} files from {source_encoding} to {target_encoding}...")

# 各ファイルを順番に処理
for file_path in files_to_convert:
    try:
        # 1. 元の文字コード(cp932)としてファイルの内容を読み込む
        with open(file_path, 'r', encoding=source_encoding, errors='replace') as f:
            content = f.read()

        # 2. 読み込んだ内容をUTF-8(BOM無し)で同じファイルに書き込む
        with open(file_path, 'w', encoding=target_encoding) as f:
            f.write(content)

        print(f"Successfully converted: {os.path.basename(file_path)}")
    except Exception as e:
        print(f"Error converting file {os.path.basename(file_path)}: {e}")

print("Conversion process complete.")
