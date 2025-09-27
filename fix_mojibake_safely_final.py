# fix_mojibake_safely_final.py
import os
import re

# 修正の基準となる「正解データ」のレポートファイル
source_report_path = r"D:\UnityProjects\URP3D_Base01\mojibake_fix_report.txt"
# 今回の実行結果を保存する、新しい最終確認レポートファイル
output_report_path = r"D:\UnityProjects\URP3D_Base01\mojibake_final_fix_report_2.txt"
# プロジェクトのルートディレクトリ
project_root = r"D:\UnityProjects\URP3D_Base01"

def parse_source_report(report_path):
    """
    mojibake_fix_report.txtを解析し、修正内容を辞書形式で返す。
    """
    corrections = {}
    current_file_path = None
    
    try:
        with open(report_path, 'r', encoding='utf-8') as f:
            for line in f:
                file_match = re.match(r"File: (.*)", line)
                if file_match:
                    relative_path = file_match.group(1).strip().replace('\\', os.sep)
                    current_file_path = os.path.join(project_root, relative_path)
                    corrections[current_file_path] = {}
                    continue

                line_match = re.match(r"L(\d+): (.*)", line)
                if line_match and current_file_path:
                    line_number = int(line_match.group(1))
                    line_content = line_match.group(2).strip()
                    corrections[current_file_path][line_number] = line_content
    except Exception as e:
        print(f"Error parsing source report file: {e}")
        return None
                
    return corrections

def apply_corrections_with_line_ending_preservation(corrections):
    """
    解析した修正内容をファイルに適用し、レポートを生成する。
    元の改行コードを完全に維持する。
    """
    final_report_lines = []
    
    print(f"Found {len(corrections)} files to process based on the report.")

    for file_path, line_data in corrections.items():
        try:
            # --- 1. ファイルをバイナリモードで読み込み、内容をテキストにデコード ---
            with open(file_path, 'rb') as f:
                content_bytes = f.read()
            
            # chardetでエンコーディングを再確認
            import chardet
            encoding = chardet.detect(content_bytes)['encoding'] or 'utf-8'
            content_text = content_bytes.decode(encoding)
            
            # splitlines(True)で行末の改行コードを保持したまま行に分割
            lines = content_text.splitlines(True)

            # --- 2. メモリ上で該当行を置換 ---
            for line_num, correct_text in line_data.items():
                if 1 <= line_num <= len(lines):
                    original_line = lines[line_num - 1]
                    indentation = re.match(r"^\s*", original_line).group(0)
                    # 元の改行コードを取得
                    original_ending_match = re.search(r'(\r\n|\n)$', original_line)
                    newline_char = original_ending_match.group(0) if original_ending_match else ''
                    
                    # テキスト部分のみを置換し、元の改行コードを付与
                    lines[line_num - 1] = indentation + correct_text + newline_char
                else:
                    print(f"Warning: Line number {line_num} is out of range for file {os.path.basename(file_path)}")

            # --- 3. 修正後の内容でファイルをバイナリモードで上書き保存 ---
            repaired_content = "".join(lines)
            with open(file_path, 'wb') as f:
                f.write(repaired_content.encode('utf-8'))

            # --- 4. 新しいレポートに結果を追記 ---
            final_report_lines.append(f"--- File Corrected: {file_path} ---")
            repaired_lines_for_report = repaired_content.splitlines()
            for line_num in sorted(line_data.keys()):
                 if 1 <= line_num <= len(repaired_lines_for_report):
                    final_report_lines.append(f"L{line_num}: {repaired_lines_for_report[line_num - 1].strip()}")
            
            print(f"Successfully replaced lines in: {os.path.basename(file_path)}")

        except Exception as e:
            error_message = f"Error processing file {os.path.basename(file_path)}: {e}"
            final_report_lines.append(error_message)
            print(error_message)
            
    return final_report_lines

# --- メイン処理 ---
if __name__ == "__main__":
    try:
        import chardet
    except ImportError:
        print("Installing chardet...")
        import subprocess, sys
        subprocess.check_call([sys.executable, "-m", "pip", "install", "chardet"])

    corrections_data = parse_source_report(source_report_path)
    if corrections_data:
        report_lines = apply_corrections_with_line_ending_preservation(corrections_data)
        
        try:
            with open(output_report_path, 'w', encoding='utf-8') as f:
                f.write("\n".join(report_lines))
            print(f"\nSuccessfully created verification report: {output_report_path}")
        except Exception as e:
            print(f"\nError writing verification report file: {e}")

    print("Process complete.")
