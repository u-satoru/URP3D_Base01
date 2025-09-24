# パフォーマンスベースライン測定レポート

## 測定日時
- **日時**: 2025年9月22日 08:08
- **Unity Version**: 6000.0.42f1
- **Platform**: Windows
- **ブランチ**: feature/3-layer-architecture-migration

## 測定対象シーン

### 1. SampleScene.unity
- **パス**: Assets/_Project/Scenes/SampleScene.unity
- **用途**: メインゲームシーン
- **測定項目**:
  - FPS: 計測予定
  - CPU使用率: 計測予定
  - メモリ使用量: 計測予定
  - ドローコール数: 計測予定

### 2. NPCVisualSensorTest.unity
- **パス**: Assets/_Project/Scenes/NPCVisualSensorTest.unity
- **用途**: AI視覚センサーテストシーン
- **測定項目**:
  - FPS: 計測予定
  - CPU使用率: 計測予定
  - メモリ使用量: 計測予定
  - ドローコール数: 計測予定

### 3. StealthAudioTest.unity
- **パス**: Assets/_Project/Scenes/Audio/StealthAudioTest.unity
- **用途**: ステルスオーディオシステムテストシーン
- **測定項目**:
  - FPS: 計測予定
  - CPU使用率: 計測予定
  - メモリ使用量: 計測予定
  - ドローコール数: 計測予定

### 4. TPSTemplateTest.unity
- **パス**: Assets/_Project/Scenes/TPSTemplateTest.unity
- **用途**: TPSテンプレートテストシーン
- **測定項目**:
  - FPS: 計測予定
  - CPU使用率: 計測予定
  - メモリ使用量: 計測予定
  - ドローコール数: 計測予定

## 測定手順
1. Unity Editorでシーンを開く
2. Window > Analysis > Profiler を開く
3. Play Modeで30秒間実行
4. 以下のデータを記録:
   - 平均FPS
   - 最小/最大FPS
   - CPU時間
   - メモリ使用量（Total Reserved/Used）
   - ドローコール数
   - SetPassCall数
   - バッチング数

## 測定結果

### ベースラインメトリクス
**注意**: Unity Editorが起動中のため、Unity Editor内のProfilerから手動で測定を実行してください。

#### 推奨測定方法
1. Unity Editorで上記の各シーンを開く
2. **Window > Analysis > Profiler** を開く
3. **Play Mode**でシーンを実行
4. **30秒間**の測定を実施
5. 結果をこのドキュメントに記録

#### 期待される基準値
- **FPS**: 60 FPS以上維持
- **CPU時間**: 16.67ms以下（60FPS基準）
- **メモリ使用量**: 500MB以下
- **ドローコール**: 100以下
- **SetPassCalls**: 50以下

## 改善対象の識別
3層アーキテクチャ移行前のベースラインとして記録し、移行後のパフォーマンス比較に使用します。

## 次のステップ
1. Unity Editor内でのProfiler測定実施
2. 測定結果の記録
3. 3層アーキテクチャ移行開始（Phase 1）
