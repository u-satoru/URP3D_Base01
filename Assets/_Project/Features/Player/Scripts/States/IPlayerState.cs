using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    /// <summary>
    /// プレイヤーの各状態が実装すべきインターフェースです。
    /// ステートマシンは、このインターフェースを介して現在の状態を制御します。
    /// </summary>
    public interface IPlayerState
    {
        /// <summary>
        /// この状態に遷移した際に一度だけ呼び出されます。
        /// 初期化処理をここで行います。
        /// </summary>
        /// <param name="stateMachine">このステートを管理するステートマシン。</param>
        void Enter(DetailedPlayerStateMachine stateMachine);

        /// <summary>
        /// この状態から他の状態に遷移する際に一度だけ呼び出されます。
        /// 終了処理をここで行います。
        /// </summary>
        /// <param name="stateMachine">このステートを管理するステートマシン。</param>
        void Exit(DetailedPlayerStateMachine stateMachine);

        /// <summary>
        /// この状態である間、毎フレーム呼び出されます。
        /// 主に時間経過や入力に依存しないロजिकを処理します。
        /// </summary>
        /// <param name="stateMachine">このステートを管理するステートマシン。</param>
        void Update(DetailedPlayerStateMachine stateMachine);

        /// <summary>
        /// この状態である間、固定時間間隔で呼び出されます。
        /// 主に物理演算関連のロジックを処理します。
        /// </summary>
        /// <param name="stateMachine">このステートを管理するステートマシン。</param>
        void FixedUpdate(DetailedPlayerStateMachine stateMachine);

        /// <summary>
        /// プレイヤーからの入力を処理します。
        /// </summary>
        /// <param name="stateMachine">このステートを管理するステートマシン。</param>
        /// <param name="moveInput">移動入力ベクトル。</param>
        /// <param name="jumpInput">ジャンプ入力フラグ。</param>
        void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput);
    }
}
