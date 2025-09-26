import os
import glob

def convert_file(file_path):
    """
    指定されたファイルをBOM無しUTF-8、改行コードLFに変換する。
    """
    try:
        # BOM付きUTF-8として読み込みを試みる
        with open(file_path, 'r', encoding='utf-8-sig') as f:
            content = f.read()
        
        print(f"Successfully read '{file_path}' with UTF-8-SIG encoding.")

        # BOM無しUTF-8、改行コードLFで書き込む
        with open(file_path, 'w', encoding='utf-8', newline='\n') as f:
            f.write(content)
        
        print(f"Successfully converted '{file_path}' to UTF-8 (No BOM) with LF line endings.")
        return True

    except Exception as e:
        print(f"Could not convert '{file_path}': {e}")
        return False

def main():
    # プロジェクトのルートディレクトリを基準にファイルを検索
    base_dir = os.getcwd()
    
    # 修正対象のファイルリスト
    files_to_fix = [
        "Assets/_Project/Core/Services/AdvancedRollbackMonitor.cs",
        "Assets/_Project/Core/Services/SingletonRemovalPlan.cs",
        "Assets/_Project/Core/Services/MigrationScheduler.cs"
    ]
    
    converted_count = 0
    for file_rel_path in files_to_fix:
        file_abs_path = os.path.join(base_dir, file_rel_path.replace('/', os.sep))
        if os.path.exists(file_abs_path):
            if convert_file(file_abs_path):
                converted_count += 1
        else:
            print(f"File not found: '{file_abs_path}'")

    print(f"\nConversion complete. {converted_count}/{len(files_to_fix)} files were converted.")

if __name__ == "__main__":
    main()