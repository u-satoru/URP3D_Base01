
import os
import shutil
import unittest
from convert_encoding_and_newlines import convert_directory

class TestEncodingConversion(unittest.TestCase):

    def setUp(self):
        """テストの前に一時的なディレクトリとテストファイルを作成する"""
        self.test_dir = "temp_test_dir_for_encoding"
        if os.path.exists(self.test_dir):
            shutil.rmtree(self.test_dir)
        os.makedirs(self.test_dir)

        # テストケースの定義: (ファイル名, 内容, エンコーディング)
        # 改行コードは内容の文字列に直接含める
        self.test_files_info = {
            'correct_utf8_crlf.txt': ("正しく処理されるファイル\r\nCRLF改行", 'utf-8'),
            'needs_conversion_lf.txt': ("LF改行のファイル\nこれは変換される", 'utf-8'),
            'needs_conversion_sjis.txt': ("Shift-JISのファイル\r\nこれは変換される", 'shift_jis'),
            'needs_conversion_utf16.txt': ("UTF-16 LE BOM付き\nそしてLF改行", 'utf-16-le'),
            'needs_conversion_utf8_bom.txt': ("UTF-8 BOM付きファイル\r\n", 'utf-8-sig'),
            'no_newline.txt': ("改行がないファイル", 'utf-8'),
        }

        # テストファイルの作成 (バイナリモードで書き込み、改行コードを正確に制御)
        for name, (content, encoding) in self.test_files_info.items():
            with open(os.path.join(self.test_dir, name), 'wb') as f:
                f.write(content.encode(encoding))
        
        # 除外されるべきバイナリファイル
        with open(os.path.join(self.test_dir, "binary.asset"), 'wb') as f:
            f.write(b'\x00\x01\x02\x03\x04')

    def tearDown(self):
        """テストの後に一時的なディレクトリを削除する"""
        if os.path.exists(self.test_dir):
            shutil.rmtree(self.test_dir)

    def test_conversion_logic(self):
        """変換ロジックが正しく動作するかをテストする"""
        print("\n--- テスト実行開始 ---")
        
        # メインの変換処理をテストディレクトリで実行
        converted, skipped, errors = convert_directory(self.test_dir)

        print("\n--- 結果検証開始 ---")

        # --- 検証 ---
        # 変換されるべきファイルの数を確認
        # correct_utf8_crlf.txt と no_newline.txt (2) はスキップされるはず
        self.assertEqual(converted, 4, "変換されたファイル数が正しくありません。")
        self.assertEqual(skipped, 2, "スキップされたファイル数が正しくありません。")
        self.assertEqual(errors, 0, "エラーが発生したファイルがあってはなりません。")

        for name, (content, _) in self.test_files_info.items():
            filepath = os.path.join(self.test_dir, name)
            
            with open(filepath, 'rb') as f:
                raw_content = f.read()

            # 1. BOMがないことを確認
            self.assertFalse(raw_content.startswith(b'\xef\xbb\xbf'), f"{name} should not have a BOM")

            # 2. 改行コードがCRLFであることを確認
            # (内容に改行が含まれる場合のみ)
            original_content_bytes = content.encode('utf-8')
            if b'\n' in original_content_bytes or b'\r' in original_content_bytes:
                 self.assertIn(b'\r\n', raw_content, f"{name} should have CRLF line endings")
                 self.assertNotIn(b'\n', raw_content.replace(b'\r\n', b''), f"{name} should not have standalone LF")


            # 3. エンコーディングがUTF-8であり、内容が一致することを確認
            try:
                decoded_content = raw_content.decode('utf-8')
                # 元のコンテンツの改行を正規化して比較
                normalized_original = content.replace('\r\n', '\n').replace('\r', '\n')
                normalized_converted = decoded_content.replace('\r\n', '\n')
                self.assertEqual(normalized_converted, normalized_original, f"{name} content mismatch")
                print(f"[OK] {name}: Verified")
            except UnicodeDecodeError:
                self.fail(f"{name} is not a valid UTF-8 file after conversion.")

        # バイナリファイルが変更されていないことを確認
        binary_path = os.path.join(self.test_dir, "binary.asset")
        with open(binary_path, 'rb') as f:
            self.assertEqual(f.read(), b'\x00\x01\x02\x03\x04')
        print("[OK] binary.asset: OK (untouched)")


if __name__ == '__main__':
    unittest.main()
