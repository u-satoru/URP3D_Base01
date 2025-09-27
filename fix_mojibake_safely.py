# fix_mojibake_safely.py
import os
import re

# 修正の基準となる「正解データ」のレポートファイル
source_report_path = r"D:\UnityProjects\URP3D_Base01\mojibake_fix_report.txt"
# 今回の実行結果を保存する、新しい最終確認レポートファイル
output_report_path = r"D:\UnityProjects\URP3D_Base01\mojibake_safe_fix_report.txt"
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

def apply_corrections_safely(corrections):
    """
    解析した修正内容をファイルに適用し、レポートを生成する。
    改行コードを完全に維持する。
    """
    final_report_lines = []
    
    print(f"Found {len(corrections)} files to process based on the report.")

    for file_path, line_data in corrections.items():
        try:
            # --- 1. ファイルをバイナリモードで読み込み、改行コードを推測 ---
            with open(file_path, 'rb') as f:
                content_bytes = f.read()
            
            # chardetでエンコーディングを再確認
            import chardet
            detected = chardet.detect(content_bytes)
            encoding = detected.get('encoding', 'utf-8')

            # ファイルの内容をテキストとしてデコード
            content_text = content_bytes.decode(encoding)
            
            # 改行コードの種類を判別 (CRLFかLFか)
            if '\r\n' in content_text:
                newline_char = '\r\n'
            else:
                newline_char = '\n'

            lines = content_text.splitlines()

            # --- 2. メモリ上で該当行を置換 ---
            for line_num, correct_text in line_data.items():
                if 1 <= line_num <= len(lines):
                    original_line = lines[line_num - 1]
                    indentation = re.match(r"^\s*", original_line).group(0)
                    lines[line_num - 1] = indentation + correct_text
                else:
                    print(f"Warning: Line number {line_num} is out of range for file {os.path.basename(file_path)}")

            # --- 3. 修正後の内容でファイルを上書き保存 ---
            # 元の改行コードを維持し、UTF-8(BOM無し)で保存
            with open(file_path, 'w', encoding='utf-8', newline=newline_char) as f:
                f.write(newline_char.join(lines))

            # --- 4. 新しいレポートに結果を追記 ---
            final_report_lines.append(f"--- File Corrected: {file_path} ---")
            with open(file_path, 'r', encoding='utf-8') as f:
                final_lines = f.readlines()
                for line_num in sorted(line_data.keys()):
                    if 1 <= line_num <= len(final_lines):
                        final_report_lines.append(f"L{line_num}: {final_lines[line_num - 1].strip()}")
            
            print(f"Successfully replaced lines in: {os.path.basename(file_path)}")

        except Exception as e:
            error_message = f"Error processing file {os.path.basename(file_path)}: {e}"
            print(error_message)
            final_report_lines.append(error_message)
            
    return final_report_lines

# --- メイン処理 ---
if __name__ == "__main__":
    # chardetライブラリがなければインストール
    try:
        import chardet
    except ImportError:
        print("chardet library not found. Installing...")
        import subprocess
        import sys
        subprocess.check_call([sys.executable, "-m", "pip", "install", "chardet"])

    corrections_data = parse_source_report(source_report_path)
    if corrections_data:
        report_lines = apply_corrections_safely(corrections_data)
        
        try:
            with open(output_report_path, 'w', encoding='utf-8') as f:
                f.write("\n".join(report_lines))
            print(f"\nSuccessfully created verification report: {output_report_path}")
        except Exception as e:
            print(f"\nError writing verification report file: {e}")

    print("Process complete.")
