# repair_double_encoding.py
import os
import re

# 修復対象のファイルリスト
files_to_repair = [
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

# 中間エンコーディングと本来のエンコーディング
intermediate_encoding = 'latin-1' # UTF-8 -> bytes の変換に使われることが多い
original_encoding = 'cp932'       # 本来の日本語エンコーディング
target_encoding = 'utf-8-sig'     # Unityが確実に認識するBOM付きUTF-8

# 修復結果を保存するレポートファイル
report_file_path = r"D:\UnityProjects\URP3D_Base01\mojibake_repair_report.txt"

print(f"Starting repair process for {len(files_to_repair)} files...")

final_report_lines = []

for file_path in files_to_repair:
    try:
        # --- 1. ファイルをUTF-8として読み込む ---
        with open(file_path, 'r', encoding='utf-8') as f:
            garbled_content = f.read()

        # --- 2. 二重エンコーディングを修復 ---
        # 文字化けテキスト -> (latin-1) -> バイト -> (cp932) -> 正しい日本語テキスト
        re_encoded_bytes = garbled_content.encode(intermediate_encoding)
        repaired_content = re_encoded_bytes.decode(original_encoding)

        # --- 3. 修復後の内容をBOM付きUTF-8で書き込む ---
        with open(file_path, 'w', encoding=target_encoding) as f:
            f.write(repaired_content)

        # --- 4. 修復レポートを作成 ---
        final_report_lines.append(f"--- File Repaired: {os.path.basename(file_path)} ---")
        
        # 修復前後の行を比較してレポートに追加
        garbled_lines = garbled_content.splitlines()
        repaired_lines = repaired_content.splitlines()
        
        for i, (garbled, repaired) in enumerate(zip(garbled_lines, repaired_lines)):
            # 内容が変化した行のみをレポート
            if garbled != repaired:
                final_report_lines.append(f"L{i+1} [BEFORE]: {garbled.strip()}")
                final_report_lines.append(f"L{i+1} [AFTER]:  {repaired.strip()}")
        
        print(f"Successfully repaired: {os.path.basename(file_path)}")

    except Exception as e:
        error_message = f"Error repairing file {os.path.basename(file_path)}: {e}"
        print(error_message)
        final_report_lines.append(error_message)

# --- 5. 最終レポートをファイルに出力 ---
try:
    with open(report_file_path, 'w', encoding='utf-8') as f:
        f.write("\n".join(final_report_lines))
    print(f"\nSuccessfully created repair report: {report_file_path}")
except Exception as e:
    print(f"\nError writing repair report file: {e}")

print("Repair process complete.")
