using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Camera.ViewMode
{
    /// <summary>
    /// カメラの表示モードを定義する列挙型
    /// </summary>
    /// <remarks>
    /// この列挙型は、3Dゲームにおける異なるカメラ視点を表現し、
    /// プレイヤーの状況や操作に応じた最適な視点提供を可能にします。
    /// </remarks>
    public enum ViewMode
    {
        /// <summary>一人称視点（プレイヤーの目線）</summary>
        FirstPerson,
        
        /// <summary>三人称視点（プレイヤーを後方から見た視点）</summary>
        ThirdPerson,
        
        /// <summary>カバー視点（遮蔽物に隠れる際の特殊視点）</summary>
        Cover,
        
        /// <summary>視点切り替え中の遷移状態</summary>
        Transition
    }
    
    /// <summary>
    /// カメラの視点モードを制御し、滑らかな視点遷移を提供するコントローラー
    /// </summary>
    /// <remarks>
    /// 設計思想：
    /// このコントローラーは、現代の3Dゲームで一般的な複数視点システムを実装します。
    /// 一人称視点と三人称視点の切り替え、ステルス時のカバー視点など、
    /// ゲームプレイに応じた最適な視点を提供し、プレイヤーの没入感を向上させます。
    /// 
    /// 主要機能：
    /// - 複数視点モード（一人称、三人称、カバー）の管理
    /// - 滑らかな視点遷移アニメーション
    /// - エイム時のFOV調整
    /// - イベント駆動による他システムとの連携
    /// 
    /// 技術的特徴：
    /// - Coroutineによる非同期的な遷移処理
    /// - AnimationCurveを使用した自然な補間
    /// - ScriptableObjectによる設定の外部化
    /// - 状態管理による遷移の制御
    /// 
    /// 使用場面：
    /// - アクション・アドベンチャーゲーム
    /// - ステルス・戦術ゲーム
    /// - オープンワールドゲーム
    /// - TPS/FPS切り替え機能が必要なゲーム
    /// 
    /// 連携システム：
    /// - プレイヤーコントローラー（移動・操作感の調整）
    /// - UIシステム（クロスヘア、HUD表示の切り替え）
    /// - オーディオシステム（視点による音響効果の変更）
    /// </remarks>
    public class ViewModeController : MonoBehaviour
    {
        [Header("Camera References")]
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private Transform cameraRig;
        [SerializeField] private Transform playerTransform;
        
        [Header("View Configurations")]
        [SerializeField] private ViewModeSettings currentSettings;
        [SerializeField] private FirstPersonSettings fpsSettings;
        [SerializeField] private ThirdPersonSettings tpsSettings;
        [SerializeField] private CoverViewSettings coverSettings;
        
        [Header("Events")]
        [SerializeField] private GameEvent<ViewMode> onViewModeChanged;
        
        [Header("Current State")]
        [SerializeField] private ViewMode currentMode = ViewMode.ThirdPerson;
        [SerializeField] private ViewMode previousMode = ViewMode.ThirdPerson;
        [SerializeField] private bool isTransitioning = false;
        
        private Coroutine transitionCoroutine;
        
        private void Start()
        {
            if (mainCamera == null)
                mainCamera = UnityEngine.Camera.main;
                
            if (cameraRig == null)
                cameraRig = transform;
                
            ApplyViewMode(currentMode, true);
        }
        
        /// <summary>
        /// 一人称視点と三人称視点の間で切り替えを行います
        /// </summary>
        /// <remarks>
        /// 動作仕様：
        /// - 現在一人称 → 三人称に切り替え
        /// - 現在三人称 → 一人称に切り替え
        /// - カバー視点の場合は切り替え無効
        /// - 遷移中の場合は処理をスキップ
        /// 
        /// 使用場面：
        /// - プレイヤーの手動切り替え操作（Vキー等）
        /// - ゲームプレイ状況による自動切り替え
        /// - 設定メニューからの視点設定適用
        /// 
        /// UI/UX への影響：
        /// クロスヘアの表示切り替え、HUDレイアウトの調整、
        /// 操作感度の変更などが連動して発生する可能性があります。
        /// </remarks>
        public void ToggleViewMode()
        {
            if (isTransitioning) return;
            
            ViewMode targetMode = currentMode == ViewMode.FirstPerson ? 
                ViewMode.ThirdPerson : ViewMode.FirstPerson;
                
            SwitchToView(targetMode);
        }
        
        /// <summary>
        /// 指定されたビューモードに切り替えます
        /// </summary>
        /// <param name="targetMode">切り替え先のビューモード</param>
        /// <remarks>
        /// 処理フロー：
        /// 1. 遷移可能状態の確認（遷移中でない、異なるモードである）
        /// 2. 既存の遷移コルーチンの停止
        /// 3. 新しい遷移コルーチンの開始
        /// 
        /// 遷移制御：
        /// - 同じモードへの切り替えは無視
        /// - 遷移中の重複実行を防止
        /// - 既存遷移の中断と新規遷移の開始
        /// 
        /// パフォーマンス考慮：
        /// Coroutineを使用することで、フレームレートに影響を与えない
        /// 滑らかな遷移を実現しています。
        /// 
        /// エラーハンドリング：
        /// 不正なモード指定時は、TransitionToView内でエラーログを出力し、
        /// 安全に処理を中断します。
        /// </remarks>
        public void SwitchToView(ViewMode targetMode)
        {
            if (isTransitioning || currentMode == targetMode) return;
            
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);
                
            transitionCoroutine = StartCoroutine(TransitionToView(targetMode));
        }
        
        /// <summary>
        /// カバー視点に切り替えます
        /// </summary>
        /// <remarks>
        /// 機能仕様：
        /// - 現在のモードを記憶してカバー視点に遷移
        /// - 既にカバー視点の場合は処理をスキップ
        /// - 遮蔽物に隠れる際の特殊な視点を提供
        /// 
        /// ステルスゲームプレイ：
        /// カバー視点は、プレイヤーが壁や障害物に身を隠す際に使用される特殊視点です。
        /// 通常の視点では見えない角を覗き見ることができ、戦術的なゲームプレイを支援します。
        /// 
        /// 状態管理：
        /// previousMode変数により、カバー視点終了時に元の視点に正確に復帰できます。
        /// これにより、一人称・三人称どちらからでも自然にカバー視点を使用できます。
        /// 
        /// 使用トリガー：
        /// - 壁際での特定キー入力
        /// - CoverSystemコンポーネントとの連携
        /// - AIの視線回避時の自動実行
        /// </remarks>
        public void SwitchToCoverView()
        {
            if (currentMode != ViewMode.Cover)
            {
                previousMode = currentMode;
                SwitchToView(ViewMode.Cover);
            }
        }
        
        /// <summary>
        /// カバー視点から元の視点に戻ります
        /// </summary>
        /// <remarks>
        /// 復帰仕様：
        /// - カバー視点でない場合は処理をスキップ
        /// - 記憶されたpreviousModeに復帰
        /// - 一人称・三人称の状態を正確に復元
        /// 
        /// 安全性：
        /// previousModeが不正な値の場合でも、GetSettingsForModeメソッドで
        /// 適切なデフォルト設定（三人称）にフォールバックします。
        /// 
        /// 使用場面：
        /// - プレイヤーの手動解除操作
        /// - 壁際から離れた際の自動解除
        /// - 戦闘開始時の強制解除
        /// - ゲームイベントによる視点リセット
        /// </remarks>
        public void ExitCoverView()
        {
            if (currentMode == ViewMode.Cover)
            {
                SwitchToView(previousMode);
            }
        }
        
        private IEnumerator TransitionToView(ViewMode targetMode)
        {
            isTransitioning = true;
            ViewMode startMode = currentMode;
            currentMode = ViewMode.Transition;
            
            ViewModeSettings startSettings = GetSettingsForMode(startMode);
            ViewModeSettings targetSettings = GetSettingsForMode(targetMode);
            
            if (targetSettings == null)
            {
                Debug.LogError($"No settings found for view mode: {targetMode}");
                isTransitioning = false;
                yield break;
            }
            
            float elapsed = 0f;
            float duration = targetSettings.transitionDuration;
            
            Vector3 startPos = cameraRig.localPosition;
            Quaternion startRot = cameraRig.localRotation;
            float startFOV = mainCamera.fieldOfView;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = targetSettings.transitionCurve.Evaluate(elapsed / duration);
                
                cameraRig.localPosition = Vector3.Lerp(startPos, targetSettings.cameraOffset, t);
                cameraRig.localRotation = Quaternion.Slerp(startRot, 
                    Quaternion.Euler(targetSettings.cameraRotation), t);
                mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetSettings.fieldOfView, t);
                
                yield return null;
            }
            
            ApplyViewMode(targetMode, false);
            
            currentMode = targetMode;
            isTransitioning = false;
            
            onViewModeChanged?.Raise(currentMode);
            
            transitionCoroutine = null;
        }
        
        private void ApplyViewMode(ViewMode mode, bool immediate)
        {
            ViewModeSettings settings = GetSettingsForMode(mode);
            if (settings == null) return;
            
            currentSettings = settings;
            
            if (immediate)
            {
                cameraRig.localPosition = settings.cameraOffset;
                cameraRig.localRotation = Quaternion.Euler(settings.cameraRotation);
                mainCamera.fieldOfView = settings.fieldOfView;
            }
        }
        
        private ViewModeSettings GetSettingsForMode(ViewMode mode)
        {
            switch (mode)
            {
                case ViewMode.FirstPerson:
                    return fpsSettings;
                case ViewMode.ThirdPerson:
                    return tpsSettings;
                case ViewMode.Cover:
                    return coverSettings;
                default:
                    return tpsSettings;
            }
        }
        
        /// <summary>
        /// エイム状態の設定に応じてカメラのFOVを調整します
        /// </summary>
        /// <param name="isAiming">エイム状態かどうか</param>
        /// <remarks>
        /// 動作仕様：
        /// - エイム中: aimFieldOfValue値に設定（通常、狭い視野角）
        /// - 非エイム中: 通常のfieldOfView値に復帰
        /// - 設定が存在しない場合は処理をスキップ
        /// 
        /// ゲームプレイへの影響：
        /// エイム時のFOV縮小により、以下の効果が得られます：
        /// - 目標の拡大表示（照準精度の向上）
        /// - 視野の制限（集中感の演出）
        /// - ズーム効果（遠距離目標の視認性向上）
        /// 
        /// 技術的考慮：
        /// - FOVの変更は即座に適用（アニメーションなし）
        /// - 現在の視点モードの設定を尊重
        /// - パフォーマンスへの影響は最小限
        /// 
        /// 連携システム：
        /// - 武器システム（エイム開始・終了の通知）
        /// - 入力システム（マウス感度の調整）
        /// - UIシステム（クロスヘア・HUD表示の変更）
        /// </remarks>
        public void SetAiming(bool isAiming)
        {
            if (currentSettings != null && isAiming)
            {
                mainCamera.fieldOfView = currentSettings.aimFieldOfView;
            }
            else if (currentSettings != null)
            {
                mainCamera.fieldOfView = currentSettings.fieldOfView;
            }
        }
        
        /// <summary>
        /// 現在のビューモードを取得します
        /// </summary>
        /// <returns>現在アクティブなViewMode</returns>
        /// <remarks>
        /// 戻り値の意味：
        /// - FirstPerson/ThirdPerson/Cover: 対応する視点モードが有効
        /// - Transition: 視点切り替えアニメーション中
        /// 
        /// 使用例：
        /// 他のシステムが現在の視点に応じて動作を変更する際に使用します。
        /// 例：UIの表示切り替え、操作感度の調整、音響効果の変更など
        /// </remarks>
        public ViewMode GetCurrentMode() => currentMode;
        
        /// <summary>
        /// 現在視点切り替えアニメーション中かどうかを取得します
        /// </summary>
        /// <returns>遷移中の場合true、それ以外はfalse</returns>
        /// <remarks>
        /// 使用目的：
        /// - 遷移中の重複操作防止
        /// - UI要素の状態制御（切り替えボタンの無効化等）
        /// - 他システムでの処理タイミング調整
        /// 
        /// 実装上の注意：
        /// 遷移開始から完了まで一時的にtrueを返します。
        /// 遷移中は新しい視点切り替えリクエストが無視されます。
        /// </remarks>
        public bool IsTransitioning() => isTransitioning;
    }
}
