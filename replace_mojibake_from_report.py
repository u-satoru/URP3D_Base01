# replace_mojibake_from_report.py
import os
import re

# 修正の基準となるレポートファイル
source_report_path = r"D:\UnityProjects\URP3D_Base01\mojibake_fix_report.txt"
# 実行結果を保存する新しいレポートファイル
output_report_path = r"D:\UnityProjects\URP3D_Base01\mojibake_replacement_verification_report.txt"
# プロジェクトのルートディレクトリ
project_root = r"D:\UnityProjects\URP3D_Base01"

def parse_source_report(report_path):
    """
    mojibake_fix_report.txtを解析し、修正内容を辞書形式で返す。
    戻り値の形式: { "ファイルパス": { 行番号: "修正後の行の内容" } }
    """
    corrections = {}
    current_file_path = None
    
    with open(report_path, 'r', encoding='utf-8') as f:
        for line in f:
            # "File: ..." の行からファイルパスを抽出
            file_match = re.match(r"File: (.*)", line)
            if file_match:
                # レポート内のパスは相対パスなので、絶対パスに変換
                relative_path = file_match.group(1).strip()
                current_file_path = os.path.join(project_root, relative_path)
                corrections[current_file_path] = {}
                continue

            # "L<行番号>: ..." の行から行番号と内容を抽出
            line_match = re.match(r"L(\d+): (.*)", line)
            if line_match and current_file_path:
                line_number = int(line_match.group(1))
                line_content = line_match.group(2).strip()
                corrections[current_file_path][line_number] = line_content
                
    return corrections

def apply_corrections_and_report(corrections):
    """
    解析した修正内容を実際のファイルに適用し、結果をレポートする。
    """
    final_report_lines = []
    
    print(f"Found {len(corrections)} files to process based on the report.")

    # 修正対象のファイルごとに処理
    for file_path, line_data in corrections.items():
        try:
            # --- 1. 対象の.csファイルを読み込む ---
            # chardetの結果に基づき、UTF-8として読み込むのが安全
            with open(file_path, 'r', encoding='utf-8') as f:
                all_lines = f.readlines()

            # --- 2. メモリ上で該当行を置換 ---
            for line_num, correct_text in line_data.items():
                # 配列のインデックスは0から始まるため、行番号から1を引く
                # 元の行のインデントを維持するように置換
                original_line = all_lines[line_num - 1]
                indentation = re.match(r"^\s*", original_line).group(0)
                # レポートには改行が含まれていないため、末尾に追加
                all_lines[line_num - 1] = indentation + correct_text + '\n'

            # --- 3. 修正後の内容でファイルを上書き保存 ---
            # Unity推奨のUTF-8(BOM無し)で保存
            with open(file_path, 'w', encoding='utf-8') as f:
                f.writelines(all_lines)

            # --- 4. 新しいレポートに結果を追記 ---
            final_report_lines.append(f"--- File Corrected: {file_path} ---")
            for line_num in sorted(line_data.keys()):
                # 修正後の行全体をレポートに追加
                final_report_lines.append(f"L{line_num}: {all_lines[line_num - 1].strip()}")
            
            print(f"Successfully replaced lines in: {os.path.basename(file_path)}")

        except Exception as e:
            error_message = f"Error processing file {os.path.basename(file_path)}: {e}"
            print(error_message)
            final_report_lines.append(error_message)
            
    return final_report_lines

# --- メイン処理 ---
if __name__ == "__main__":
    if not os.path.exists(source_report_path):
        print(f"Error: Source report file not found at {source_report_path}")
    else:
        # ステップ1: レポートを解析
        corrections_data = parse_source_report(source_report_path)
        
        # ステップ2: 修正を適用し、新しいレポート内容を生成
        report_lines = apply_corrections_and_report(corrections_data)
        
        # ステップ3: 新しいレポートをファイルに出力
        try:
            with open(output_report_path, 'w', encoding='utf-8') as f:
                f.write("\n".join(report_lines))
            print(f"\nSuccessfully created verification report: {output_report_path}")
        except Exception as e:
            print(f"\nError writing verification report file: {e}")

    print("Process complete.")
