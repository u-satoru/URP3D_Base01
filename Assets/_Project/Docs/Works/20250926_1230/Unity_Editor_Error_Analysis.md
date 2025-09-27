# Unity Editor エラー分析報告書

## 状況サマリー
- **バッチモードエラー**: 174件
- **Unity Editorエラー**: 58件
- **差分**: 116件（66.7%減少）

## エラー数の違いの原因

### 1. バッチモード特有の問題
- **エンコーディング処理の違い**: バッチモードはUTF-8を厳格に要求
- **キャッシュ処理の違い**: Editorは増分コンパイル、バッチは完全再コンパイル
- **条件付きコンパイル**: #if UNITY_EDITORブロックの扱いが異なる

### 2. Unity Editor 58エラーの推定内容

#### 最重要エラー（推定）
1. **EventManager関連** (約10件)
   - GameBootstrapper.cs
   - GameEventBridge.cs
   - ServiceLocator統合問題

2. **BGMTrack関連** (約15件)
   - suitableWeather/suitableTimeOfDay未定義
   - BGMTrackクラスの不完全な定義

3. **ServiceHelper関連** (約10件)
   - ISpatialAudioServiceジェネリック制約
   - GetServiceWithFallback<T>()の問題

4. **メソッド戻り値** (約5件)
   - BGMManager.IsPlaying(string)
   - MaskingEffectController.GetOriginalVolume

5. **その他のCS0103** (約18件)
   - 変数宣言の認識問題
   - スコープ外参照

## 優先修正対象

### 1. EventManager欠落問題（最優先）
```csharp
// GameBootstrapper.cs (line 114-119)
// 現在:
// TODO: EventManager参照エラーを修正
// var eventManager = new EventManager();
// ServiceLocator.Register<IEventManager>(eventManager);

// 修正案:
// EventManagerクラスを作成または既存クラスを参照
```

### 2. BGMTrackプロパティ追加
```csharp
// BGMTrack.cs に追加が必要:
public WeatherType[] suitableWeather;
public TimeOfDay[] suitableTimeOfDay;
```

### 3. ジェネリック制約修正
```csharp
// ServiceHelper.GetServiceWithFallback<T>()の制約を調整
where T : class // UnityEngine.Object制約を削除
```

## 推奨アクション

### 即座に実行
1. **Unity Editor内でエラーダブルクリック**
   - 実際のエラー位置を確認
   - 正確な行番号とコンテキスト取得

2. **EventManagerクラスの作成/参照**
   - Core/Services/にEventManager.csを作成
   - またはCore/Events/内の既存実装を確認

3. **BGMTrackクラスの完全性確認**
   - suitableWeather/suitableTimeOfDayプロパティ追加

### 検証方法
1. Unity Editorでコンパイル
2. Consoleでエラー数確認
3. 58 → 0を目標に修正

## まとめ
Unity Editor内の58エラーは、バッチモードの174エラーより**実際のプロジェクト状態を正確に反映**しています。これらは主に：
- EventManager未実装
- BGMTrackプロパティ不足
- ジェネリック制約問題

に集約され、系統的な修正で解決可能です。