# add_bom_to_all_candidates.py
import os

# 修正対象のファイルリストが書かれたテキストファイル
candidate_file_list = r"D:\UnityProjects\URP3D_Base01\remaining_mojibake_candidates.txt"
# 変換先のエンコーディング
target_encoding = 'utf-8-sig' # UTF-8 with BOM

def get_files_from_list(file_path):
    """テキストファイルからファイルパスのリストを読み込む"""
    with open(file_path, 'r', encoding='utf-8') as f:
        # コメント行と空行を除外
        files = [line.strip() for line in f if not line.startswith('#') and line.strip()]
    return files

def convert_files_to_utf8_bom(files):
    """ファイルのリストを受け取り、UTF-8 BOM付きに変換する"""
    print(f"Starting conversion for {len(files)} files to {target_encoding}...")
    
    for file_path in files:
        if not os.path.exists(file_path):
            print(f"Warning: File not found, skipping: {file_path}")
            continue
            
        try:
            # まずファイルをバイナリとして読み込む
            with open(file_path, 'rb') as f:
                content_bytes = f.read()
            
            # BOMが付いているかチェック
            if content_bytes.startswith(b'\xef\xbb\xbf'):
                print(f"Skipping, already has BOM: {os.path.basename(file_path)}")
                continue

            # chardetで元のエンコーディングを推測
            import chardet
            detected = chardet.detect(content_bytes)
            source_encoding = detected.get('encoding', 'utf-8') # 不明な場合はutf-8と仮定

            # 推測したエンコーディングでテキストにデコード
            try:
                content_text = content_bytes.decode(source_encoding)
            except (UnicodeDecodeError, TypeError):
                # デコードに失敗した場合は、Windowsのデフォルト(cp932)で再試行
                print(f"Decoding with '{source_encoding}' failed, retrying with 'cp932' for {os.path.basename(file_path)}")
                content_text = content_bytes.decode('cp932', errors='replace')

            # BOM付きUTF-8で書き戻す
            with open(file_path, 'w', encoding=target_encoding) as f:
                f.write(content_text)
            
            print(f"Successfully converted: {os.path.basename(file_path)}")

        except Exception as e:
            print(f"Error converting file {os.path.basename(file_path)}: {e}")

# --- メイン処理 ---
if __name__ == "__main__":
    try:
        import chardet
    except ImportError:
        print("Installing chardet library...")
        import subprocess, sys
        subprocess.check_call([sys.executable, "-m", "pip", "install", "chardet"])

    if os.path.exists(candidate_file_list):
        files_to_process = get_files_from_list(candidate_file_list)
        convert_files_to_utf8_bom(files_to_process)
        print("\nConversion process complete.")
    else:
        print(f"Error: Candidate file list not found at {candidate_file_list}")
