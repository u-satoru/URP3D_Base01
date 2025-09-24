using UnityEngine;

namespace asterivo.Unity60.Features.Camera.States
{
    /// <summary>
    /// カメラの各状態が実装すべきインターフェース
    /// </summary>
    /// <remarks>
    /// 設計思想：
    /// このインターフェースはStateパターンを使用してカメラの複雑な行動を管理します。
    /// 3Dゲームにおける多様な視点（一人称、三人称、エイム、カバー等）を
    /// 統一された方法で制御し、滑らかな視点遷移を実現します。
    /// 
    /// カメラ専用の特徴：
    /// - LateUpdate()の提供：他のオブジェクトの移動後にカメラを更新
    /// - 入力処理の抽象化：視点制御用の入力処理
    /// - Cinemachineとの連携：高品質なカメラワークの実現
    /// 
    /// IPlayerState・IAIStateとの違い：
    /// - プレイヤー：HandleInputで移動入力を処理、FixedUpdateで物理計算
    /// - AI：OnSightTarget、OnHearNoiseで感覚入力を処理
    /// - カメラ：LateUpdateでフレーム後半の更新、HandleInputで視点入力を処理
    /// 
    /// 実装する状態例：
    /// - FirstPersonCameraState: 一人称視点
    /// - ThirdPersonCameraState: 三人称視点
    /// - AimCameraState: エイミング視点
    /// - CoverCameraState: カバー視点
    /// 
    /// 状態遷移の典型的なパターン：
    /// - 移動時: ThirdPerson ↔ FirstPerson（プレイヤー設定により）
    /// - 戦闘時: Normal → Aim → Normal
    /// - ステルス時: Normal → Cover → Normal
    /// </remarks>
    public interface ICameraState
    {
        /// <summary>
        /// カメラ状態に遷移した際の初期化処理
        /// </summary>
        /// <param name="stateMachine">このカメラ状態を管理するステートマシン</param>
        /// <remarks>
        /// 実行内容例：
        /// - Cinemachineカメラの有効化・設定
        /// - カメラパラメータ（FOV、距離、角度等）の初期化
        /// - 視点遷移アニメーションの開始
        /// - カーソルロック状態の設定
        /// 
        /// 注意事項：
        /// カメラの切り替えは視覚的に目立つため、可能な限り滑らかな遷移を心掛ける
        /// </remarks>
        void Enter(CameraStateMachine stateMachine);
        
        /// <summary>
        /// カメラ状態から他の状態に遷移する際の終了処理
        /// </summary>
        /// <param name="stateMachine">このカメラ状態を管理するステートマシン</param>
        /// <remarks>
        /// 実行内容例：
        /// - 現在のCinemachineカメラの無効化
        /// - アニメーション・トゥイーンの停止
        /// - 一時的な設定のリセット
        /// - 次の状態への引き継ぎデータの準備
        /// 
        /// 重要な考慮事項：
        /// カメラの急激な変化はプレイヤーの没入感を損なうため、
        /// 適切なフェードアウトや視点補間を実装する
        /// </remarks>
        void Exit(CameraStateMachine stateMachine);
        
        /// <summary>
        /// カメラ状態の毎フレーム更新処理
        /// </summary>
        /// <param name="stateMachine">このカメラ状態を管理するステートマシン</param>
        /// <remarks>
        /// 実行内容例：
        /// - 状態固有のロジック実行
        /// - 状態遷移条件の評価
        /// - アニメーション状態の更新
        /// - UIへの情報提供（クロスヘア表示等）
        /// 
        /// 注意事項：
        /// カメラの位置・回転の直接操作は避け、LateUpdate()で行う。
        /// Update()では主に状態管理とロジック処理に専念する。
        /// </remarks>
        void Update(CameraStateMachine stateMachine);
        
        /// <summary>
        /// 全てのオブジェクトの更新後に実行されるカメラ更新処理
        /// </summary>
        /// <param name="stateMachine">このカメラ状態を管理するステートマシン</param>
        /// <remarks>
        /// 実行内容例：
        /// - プレイヤーの最終位置に基づくカメラ位置の計算
        /// - 視点の回転制御（マウス・スティック入力の適用）
        /// - カメラの物理的制約の適用（壁抜け防止、角度制限等）
        /// - Cinemachineパラメータの動的更新
        /// 
        /// LateUpdate使用の理由：
        /// プレイヤーや他のオブジェクトの移動処理完了後にカメラを更新することで、
        /// 一フレーム遅れのない滑らかな追従を実現します。これはカメラシステムの
        /// 基本原則であり、特に三人称視点で重要です。
        /// 
        /// パフォーマンス考慮：
        /// LateUpdate()は重い処理になりがちなため、必要最小限の計算に留める
        /// </remarks>
        void LateUpdate(CameraStateMachine stateMachine);
        
        /// <summary>
        /// カメラ制御用の入力処理
        /// </summary>
        /// <param name="stateMachine">このカメラ状態を管理するステートマシン</param>
        /// <remarks>
        /// 処理内容例：
        /// - マウス・スティックによる視点回転入力
        /// - ズーム・FOV変更入力
        /// - 視点切り替え入力（一人称⇔三人称）
        /// - エイム・カバー等の特殊状態への遷移入力
        ///
        /// 入力処理の設計方針：
        /// - 各カメラ状態で異なる入力マッピングが可能
        /// - 感度設定の状態別適用（エイム時の低感度等）
        /// - 入力の状態依存フィルタリング（移動中のカメラ制限等）
        ///
        /// プラットフォーム対応：
        /// - PC: マウス入力による高精度制御
        /// - コンソール: アナログスティックによる直感的制御
        /// - モバイル: タッチ入力による簡易制御
        ///
        /// 例外処理：
        /// - カーソルロック中の入力無効化
        /// - UIアクティブ時の入力遮断
        /// - ポーズ中の入力停止
        /// </remarks>
        void HandleInput(CameraStateMachine stateMachine);

        /// <summary>
        /// カメラリセット要求時の処理
        /// </summary>
        /// <param name="stateMachine">このカメラ状態を管理するステートマシン</param>
        /// <remarks>
        /// 実行内容例：
        /// - カメラ位置・回転の初期状態への復帰
        /// - Cinemachineカメラパラメータのリセット
        /// - 視点補間・ズーム状態の初期化
        /// - 状態固有の設定値の復元
        ///
        /// 呼び出しタイミング：
        /// - プレイヤーの明示的なリセット要求時
        /// - 異常な状態からの回復時
        /// - デバッグ目的でのカメラ状態クリア時
        ///
        /// 注意事項：
        /// リセットは即座に実行されるため、滑らかな遷移よりも
        /// 確実な状態復帰を優先する
        /// </remarks>
        void OnResetRequested(CameraStateMachine stateMachine);
    }
}
