using UnityEngine;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// プレイヤーの吁E��態が実裁E��べきインターフェースです、E    /// スチE�Eト�Eシンは、このインターフェースを介して現在の状態を制御します、E    /// </summary>
    public interface IPlayerState
    {
        /// <summary>
        /// こ�E状態に遷移した際に一度だけ呼び出されます、E        /// 初期化�E琁E��ここで行います、E        /// </summary>
        /// <param name="stateMachine">こ�EスチE�Eトを管琁E��るスチE�Eト�Eシン、E/param>
        void Enter(DetailedPlayerStateMachine stateMachine);

        /// <summary>
        /// こ�E状態から他�E状態に遷移する際に一度だけ呼び出されます、E        /// 終亁E�E琁E��ここで行います、E        /// </summary>
        /// <param name="stateMachine">こ�EスチE�Eトを管琁E��るスチE�Eト�Eシン、E/param>
        void Exit(DetailedPlayerStateMachine stateMachine);

        /// <summary>
        /// こ�E状態である間、毎フレーム呼び出されます、E        /// 主に時間経過めE�E力に依存しなぁE��जिकを処琁E��ます、E        /// </summary>
        /// <param name="stateMachine">こ�EスチE�Eトを管琁E��るスチE�Eト�Eシン、E/param>
        void Update(DetailedPlayerStateMachine stateMachine);

        /// <summary>
        /// こ�E状態である間、固定時間間隔で呼び出されます、E        /// 主に物琁E��算関連のロジチE��を�E琁E��ます、E        /// </summary>
        /// <param name="stateMachine">こ�EスチE�Eトを管琁E��るスチE�Eト�Eシン、E/param>
        void FixedUpdate(DetailedPlayerStateMachine stateMachine);

        /// <summary>
        /// プレイヤーからの入力を処琁E��ます、E        /// </summary>
        /// <param name="stateMachine">こ�EスチE�Eトを管琁E��るスチE�Eト�Eシン、E/param>
        /// <param name="moveInput">移動�E力�Eクトル、E/param>
        /// <param name="jumpInput">ジャンプ�E力フラグ、E/param>
        void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput);
    }
}
