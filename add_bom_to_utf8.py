# add_bom_to_utf8.py
import os

# BOMを付与するファイルのリスト
files_to_convert = [
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

# 変換元はUTF-8(BOM無し)、変換先はUTF-8(BOM付き)
source_encoding = 'utf-8'
target_encoding = 'utf-8-sig' # This specifies UTF-8 with BOM

print(f"Starting conversion of {len(files_to_convert)} files to {target_encoding}...")

for file_path in files_to_convert:
    try:
        # Read the content as standard UTF-8
        with open(file_path, 'r', encoding=source_encoding) as f:
            content = f.read()

        # Write the content back with a BOM
        with open(file_path, 'w', encoding=target_encoding) as f:
            f.write(content)

        print(f"Successfully added BOM to: {os.path.basename(file_path)}")
    except Exception as e:
        print(f"Error converting file {os.path.basename(file_path)}: {e}")

print("\nConversion to UTF-8 with BOM complete.")
