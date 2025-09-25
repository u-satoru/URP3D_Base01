import os
import glob
import argparse
from chardet.universaldetector import UniversalDetector

# --- 設定 ---

# 処理から除外するディレクトリのリスト
EXCLUDE_DIRS = {
    '.git', 'Library', 'Temp', 'Logs', 'Packages', 'ProjectSettings',
    'UserSettings', '.idea', '.vscode', '__pycache__'
}

# 処理から除外するファイル拡張子のリスト (バイナリファイルなど)
EXCLUDE_EXTS = {
    '.png', '.jpg', '.jpeg', '.gif', '.bmp', '.ico', '.tif', '.tiff',
    '.asset', '.prefab', '.unity', '.mat', '.anim', '.controller', '.physic',
    '.fbx', '.obj', '.3ds', '.dae',
    '.dll', '.exe', '.so', '.dylib',
    '.unitypackage', '.zip', '.rar', '.7z',
    '.mp3', '.wav', '.ogg',
    '.mp4', '.mov', '.avi',
    '.pdf', '.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx',
    '.psd', '.ai',
}

# --- スクリプト本体 ---

def is_binary(filepath):
    """ファイルがバイナリかどうかを簡易的に判定する"""
    try:
        with open(filepath, 'rb') as f:
            chunk = f.read(1024)
            if b'\0' in chunk:
                return True
    except Exception:
        return True
    return False

def detect_encoding(filepath):
    """ファイルの文字コードを推定する"""
    detector = UniversalDetector()
    with open(filepath, 'rb') as f:
        for line in f:
            detector.feed(line)
            if detector.done:
                break
    detector.close()
    encoding = detector.result['encoding']
    # chardetがasciiと判定した場合、より汎用的なutf-8として扱う
    if encoding == 'ascii':
        return 'utf-8'
    return encoding

def get_line_endings(content):
    """ファイルの改行コードの種類を判定する"""
    if '\r\n' in content:
        return 'CRLF'
    elif '\n' in content:
        return 'LF'
    elif '\r' in content:
        return 'CR'
    # 改行コードが全くないファイルの場合
    return 'CRLF' # 変換不要とみなす

def convert_directory(root_dir):
    """指定されたディレクトリ内のファイルを変換する"""
    print(f"スキャン対象ディレクトリ: {os.path.abspath(root_dir)}")
    print("プロジェクト内のテキストファイルの文字コードと改行コードをチェック・変換します。")
    print(f"対象エンコーディング: UTF-8 (BOMなし)")
    print(f"対象改行コード: CRLF")
    print("-" * 50)

    converted_files = 0
    skipped_files = 0
    error_files = 0
    
    # スクリプト自身のファイル名は除外リストに追加
    script_filenames = {
        os.path.basename(__file__),
        'test_encoding_converter.py' # テストスクリプトも除外
    }


    # globですべてのファイルを再帰的に取得
    for filepath in glob.glob(os.path.join(root_dir, '**', '*'), recursive=True):
        # ディレクトリはスキップ
        if os.path.isdir(filepath):
            continue

        # 除外ディレクトリに含まれているかチェック
        path_parts = set(filepath.split(os.sep))
        if path_parts.intersection(EXCLUDE_DIRS):
            continue

        # 除外拡張子に含まれているかチェック
        _, ext = os.path.splitext(filepath)
        if ext.lower() in EXCLUDE_EXTS:
            continue
        
        # スクリプト自身は除外
        if os.path.basename(filepath) in script_filenames:
            continue

        # バイナリファイルをヒューリスティックに除外
        if is_binary(filepath):
            continue

        try:
            # 1. エンコーディングと内容を読み込む
            encoding = detect_encoding(filepath)
            if not encoding:
                print(f"[WARN] [{filepath}] エンコーディング不明のためスキップします。")
                error_files += 1
                continue

            with open(filepath, 'r', encoding=encoding, errors='replace', newline='') as f:
                content = f.read()

            # 2. 改行コードを判定
            line_ending = get_line_endings(content)

            # 3. 変換が必要か判定
            # chardetが'UTF-8-SIG'を'utf-8'と誤判定することがあるため、BOMを別途チェック
            with open(filepath, 'rb') as f:
                has_bom = f.read(3) == b'\xef\xbb\xbf'

            is_utf8_no_bom = (encoding.lower() == 'utf-8' and not has_bom)
            is_crlf = (line_ending == 'CRLF')

            if is_utf8_no_bom and is_crlf:
                # print(f"✅ [{filepath}] は既に UTF-8/CRLF です。")
                skipped_files += 1
                continue

            # 4. 変換実行
            print(f"[CONVERTING] [{filepath}] を変換します... (元: {encoding}, BOM: {has_bom}, 改行: {line_ending})")
            
            # universal_newlines=True は使わず、手動でCRLFに統一
            normalized_content = content.replace('\r\n', '\n').replace('\r', '\n').replace('\n', '\r\n')

            with open(filepath, 'w', encoding='utf-8', newline='\r\n') as f:
                f.write(normalized_content)
            
            converted_files += 1

        except Exception as e:
            print(f"[ERROR] [{filepath}] の処理中にエラーが発生しました: {e}")
            error_files += 1

    print("-" * 50)
    print("[DONE] 処理が完了しました。")
    print(f"変換したファイル数: {converted_files}")
    print(f"スキップしたファイル数: {skipped_files}")
    print(f"エラーが発生したファイル数: {error_files}")
    
    return converted_files, skipped_files, error_files

def main():
    """メイン処理"""
    parser = argparse.ArgumentParser(description="プロジェクト内のテキストファイルのエンコーディングと改行コードを変換します。")
    parser.add_argument(
        'directory', 
        nargs='?', 
        default='.', 
        help='処理対象のルートディレクトリ (デフォルト: カレントディレクトリ)'
    )
    args = parser.parse_args()
    
    convert_directory(args.directory)

if __name__ == '__main__':
    main()
