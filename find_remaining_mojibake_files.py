# find_remaining_mojibake_files.py
import os
import chardet
import re

# 調査対象のルートディレクトリ
search_root = r"D:\UnityProjects\URP3D_Base01\Assets"
# 結果を出力するファイル
output_file_path = r"D:\UnityProjects\URP3D_Base01\remaining_mojibake_candidates.txt"

# 日本語の文字範囲（ひらがな、カタカナ、CJK統合漢字）
japanese_pattern = re.compile(r'[\u3040-\u309F\u30A0-\u30FF\u4E00-\u9FFF]+')

print(f"Searching for files with Japanese characters under '{search_root}'...")

# 修正が必要な可能性のあるファイルのリスト
candidate_files = []

# 指定されたディレクトリ以下のすべてのファイルを探索
for root, _, files in os.walk(search_root):
    for file in files:
        # .csファイルのみを対象とする
        if file.endswith(".cs"):
            file_path = os.path.join(root, file)
            try:
                # ファイルをバイナリで読み込み、エンコーディングを推定
                with open(file_path, 'rb') as f:
                    raw_data = f.read()
                
                # chardetでエンコーディングを検出
                result = chardet.detect(raw_data)
                encoding = result['encoding']
                
                # UTF-8 (BOM無し) 以外、または内容に日本語が含まれるUTF-8ファイルを候補とする
                # Shift-JISなどはBOMが無いため、UTF-8と誤判定されることがある
                if encoding is None or 'utf-8' not in encoding.lower():
                     candidate_files.append(file_path)
                     print(f"Found potential non-UTF-8 file: {file_path} (Detected: {encoding})")
                else:
                    # UTF-8ファイルでも、念のため日本語が含まれているかチェック
                    content = raw_data.decode(encoding, errors='ignore')
                    if japanese_pattern.search(content):
                        # BOMが付いているか確認
                        if not content.startswith('\ufeff'):
                             candidate_files.append(file_path)
                             print(f"Found UTF-8 file without BOM containing Japanese: {file_path}")

            except Exception as e:
                print(f"Error processing file {file_path}: {e}")

# --- 結果をファイルに出力 ---
try:
    with open(output_file_path, 'w', encoding='utf-8') as f:
        if candidate_files:
            f.write("# The following files contain Japanese characters and may need encoding correction to UTF-8 with BOM.\n")
            for path in candidate_files:
                f.write(path + "\n")
        else:
            f.write("No files requiring encoding correction were found.\n")
            
    print(f"\nSearch complete. Found {len(candidate_files)} candidate files.")
    print(f"Report saved to: {output_file_path}")

except Exception as e:
    print(f"\nError writing report file: {e}")
