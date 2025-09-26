import json
import os

def add_references_to_asmdef(file_path, new_references):
    """
    指定された.asmdefファイルに参照を追加する。
    """
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)

        if "references" not in data:
            data["references"] = []

        # 既存の参照と新しい参照を結合し、重複を排除する
        existing_references = set(data["references"])
        for ref in new_references:
            existing_references.add(ref)
        
        # Core層への自己参照や不要な参照を削除
        if data["name"] in existing_references:
            existing_references.remove(data["name"])
            
        # 元のCore層サブアセンブリへの参照も削除
        core_subs = ["asterivo.Unity60.Core.Audio", "asterivo.Unity60.Core.Commands", "asterivo.Unity60.Core.Components", "asterivo.Unity60.Core.Constants", "asterivo.Unity60.Core.Debug", "asterivo.Unity60.Core.Events", "asterivo.Unity60.Core.Services", "asterivo.Unity60.Core.Shared", "asterivo.Unity60.Core.Lifecycle"]
        existing_references -= set(core_subs)


        data["references"] = sorted(list(existing_references)) # ソートして見やすくする

        with open(file_path, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=4)
            f.write('\n') # 末尾に改行を追加

        print(f"Successfully updated references in '{file_path}'")
        return True

    except Exception as e:
        print(f"Could not update '{file_path}': {e}")
        return False

def main():
    base_dir = os.getcwd()
    editor_asmdef_path = os.path.join(base_dir, "Assets/_Project/Core/Editor/asterivo.Unity60.Core.Editor.asmdef".replace('/', os.sep))

    features_references = [
        "asterivo.Unity60.Features.ActionRPG",
        "asterivo.Unity60.Features.AI",
        "asterivo.Unity60.Features.AI.Visual",
        "asterivo.Unity60.Features.Camera",
        "asterivo.Unity60.Features.Combat",
        "asterivo.Unity60.Features.GameManagement",
        "asterivo.Unity60.Features.Player",
        "asterivo.Unity60.Features.Player.Audio",
        "asterivo.Unity60.Features.StateManagement",
        "asterivo.Unity60.Features.Templates.ActionRPG",
        "asterivo.Unity60.Features.Templates.Common",
        "asterivo.Unity60.Features.Templates.FPS",
        "asterivo.Unity60.Features.Templates.Platformer",
        "asterivo.Unity60.Features.Templates.Stealth",
        "asterivo.Unity60.Features.Templates.SurvivalHorror",
        "asterivo.Unity60.Features.Templates.TPS",
        "asterivo.Unity60.Features.UI",
        "asterivo.Unity60.Features.Validation",
        "asterivo.Unity60.Player",
        "asterivo.Unity60.Stealth"
    ]

    if os.path.exists(editor_asmdef_path):
        add_references_to_asmdef(editor_asmdef_path, features_references)
    else:
        print(f"File not found: '{editor_asmdef_path}'")

if __name__ == "__main__":
    main()
