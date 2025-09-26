import os
import re

def fix_csharp_file(file_path):
    """
    破損したC#ファイルを読み込み、メインのクラス定義だけを抽出して上書きする。
    """
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()

        # 正規表現を使って、最も外側のクラス定義を抽出する
        # (namespace {...} の中身全体をマッチさせる)
        match = re.search(r'namespace\s+[\w\.]+\s*\{.*\}', content, re.DOTALL)
        
        if match:
            clean_content = match.group(0)
            
            # ファイルをクリーンな内容で上書き
            with open(file_path, 'w', encoding='utf-8', newline='\n') as f:
                f.write(clean_content)
            
            print(f"Successfully cleaned and overwrote '{file_path}'")
            return True
        else:
            print(f"Could not find a valid namespace block in '{file_path}'")
            return False

    except Exception as e:
        print(f"Could not process '{file_path}': {e}")
        return False

def main():
    base_dir = os.getcwd()
    
    files_to_fix = [
        "Assets/_Project/Core/Services/AdvancedRollbackMonitor.cs",
        "Assets/_Project/Core/Services/SingletonRemovalPlan.cs",
        "Assets/_Project/Core/Services/MigrationScheduler.cs"
    ]
    
    fixed_count = 0
    for file_rel_path in files_to_fix:
        file_abs_path = os.path.join(base_dir, file_rel_path.replace('/', os.sep))
        if os.path.exists(file_abs_path):
            if fix_csharp_file(file_abs_path):
                fixed_count += 1
        else:
            print(f"File not found: '{file_abs_path}'")

    print(f"\nCleaning complete. {fixed_count}/{len(files_to_fix)} files were fixed.")

if __name__ == "__main__":
    main()
