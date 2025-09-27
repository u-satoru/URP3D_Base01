# convert_encoding_and_generate_report.py
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

# 変換元の文字コードと変換先の文字コード
source_encoding = 'cp932'
target_encoding = 'utf-8'
# 出力するレポートファイルのパス
report_file_path = r"D:\UnityProjects\URP3D_Base01\mojibake_conversion_report.txt"

print(f"Starting conversion and reporting for {len(files_to_convert)} files...")

# レポート全体の内容を保持するリスト
final_report_lines = []

# リスト内の各ファイルを処理
for file_path in files_to_convert:
    try:
        # --- 1. ファイルをcp932として読み込み、内容と修正対象の行を特定 ---
        
        # ファイル全体の行を格納するリスト
        content_lines = []
        # 修正された行の情報を「行番号: 行の内容」の形式で格納する辞書
        corrected_lines_info = {}

        with open(file_path, 'r', encoding=source_encoding, errors='replace') as f:
            for i, line in enumerate(f):
                line_number = i + 1
                # メモリ上では正しい日本語として読み込まれる
                content_lines.append(line)
                # 行に日本語（ひらがな、カタカナ、漢字）が含まれているかチェック
                if any('\u3040' <= char <= '\u309F' or # Hiragana
                       '\u30A0' <= char <= '\u30FF' or # Katakana
                       '\u4E00' <= char <= '\u9FFF' for char in line): # CJK Unified Ideographs
                    # 含まれていれば、修正対象として行番号と内容を保存
                    corrected_lines_info[line_number] = line.strip()

        # --- 2. ファイル全体をUTF-8（BOM無し）で書き換える ---
        with open(file_path, 'w', encoding=target_encoding) as f:
            f.writelines(content_lines)

        # --- 3. このファイルのレポートを作成 ---
        final_report_lines.append(f"--- File Converted: {os.path.basename(file_path)} ---")
        if corrected_lines_info:
            # 修正された行を行番号順に並べ替えてレポートに追加
            for line_num in sorted(corrected_lines_info.keys()):
                final_report_lines.append(f"L{line_num}: {corrected_lines_info[line_num]}")
        else:
            final_report_lines.append("No Japanese characters found in this file.")
        
        print(f"Successfully processed: {os.path.basename(file_path)}")

    except Exception as e:
        error_message = f"Error processing file {os.path.basename(file_path)}: {e}"
        print(error_message)
        final_report_lines.append(error_message)

# --- 4. 最終的なレポートをファイルに書き出す ---
try:
    with open(report_file_path, 'w', encoding='utf-8') as f:
        f.write("\n".join(final_report_lines))
    print(f"\nSuccessfully created conversion report: {report_file_path}")
except Exception as e:
    print(f"\nError writing report file: {e}")

print("Process complete.")
