# convert_encoding_and_report.py
import os

# 文字化けが確認されたファイルのリスト
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

# 変換元の文字コード (Windowsの日本語環境で標準的なShift-JIS)
source_encoding = 'cp932'
# 変換先の文字コード (BOM無しのUTF-8)
target_encoding = 'utf-8'
# レポートファイルのパス
report_file_path = r"D:\UnityProjects\URP3D_Base01\mojibake_fix_and_verify_report.txt"

print(f"Starting conversion and reporting for {len(files_to_convert)} files...")

# レポート内容を格納するリスト
report_content = []

# 各ファイルを順番に処理
for file_path in files_to_convert:
    try:
        # --- ステップ1: 変換前のファイルから文字化けのある行番号を特定 ---
        mojibake_line_numbers = set()
        with open(file_path, 'r', encoding=source_encoding, errors='ignore') as f:
            for i, line in enumerate(f):
                # '�' (U+FFFD) は置換文字。文字化けの典型的な兆候。
                if '�' in line:
                    mojibake_line_numbers.add(i + 1) # 行番号は1から始まるため +1

        # --- ステップ2: ファイル全体を読み込んでメモリ上で変換 ---
        with open(file_path, 'r', encoding=source_encoding, errors='replace') as f:
            original_content = f.read()

        # --- ステップ3: 変換後の内容でファイルを上書き ---
        with open(file_path, 'w', encoding=target_encoding) as f:
            f.write(original_content)

        # --- ステップ4: レポートを作成 ---
        report_content.append(f"--- File: {os.path.basename(file_path)} ---")
        
        # 変換後のファイルを再度読み込み、該当行をレポートに追加
        with open(file_path, 'r', encoding=target_encoding) as f:
            all_lines = f.readlines()
            for line_num in sorted(list(mojibake_line_numbers)):
                # 配列のインデックスは0から始まるため -1
                corrected_line = all_lines[line_num - 1].strip()
                report_content.append(f"L{line_num}: {corrected_line}")
        
        print(f"Successfully converted and analyzed: {os.path.basename(file_path)}")

    except Exception as e:
        error_message = f"Error processing file {os.path.basename(file_path)}: {e}"
        print(error_message)
        report_content.append(error_message)

# --- ステップ5: レポートファイルに書き出す ---
try:
    with open(report_file_path, 'w', encoding='utf-8') as f:
        f.write("\n".join(report_content))
    print(f"\nSuccessfully created report file: {report_file_path}")
except Exception as e:
    print(f"\nError writing report file: {e}")

print("Conversion and reporting process complete.")
