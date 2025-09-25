using UnityEngine;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// 対象の体力を回復させるコマンド実装。
    /// IResettableCommandを実装しており、ObjectPoolによる再利用に対応しています。
    /// 
    /// 主な機能：
    /// - 体力の回復処理の実行
    /// - Undo操作によるダメージの適用（回復の取り消し）
    /// - ObjectPool使用時の状態リセット機能
    /// - パラメーターによる柔軟な初期化
    /// </summary>
    public class HealCommand : IResettableCommand
    {
        /// <summary>
        /// 回復処理の対象となるヘルスターゲット
        /// </summary>
        private IHealthTarget _target;
        
        /// <summary>
        /// 回復する体力の量
        /// </summary>
        private int _healAmount;

        /// <summary>
        /// このコマンドがUndo操作をサポートするかどうかを示します。
        /// 回復コマンドは常にUndo可能（ダメージに変換）です。
        /// </summary>
        public bool CanUndo => true; 

        /// <summary>
        /// プール化対応のデフォルトコンストラクタ。
        /// ObjectPool使用時に必要な引数なしコンストラクタです。
        /// 実際のパラメータは後でInitialize()メソッドで設定します。
        /// </summary>
        public HealCommand()
        {
            // プール化対応：パラメーターなしコンストラクタ
        }

        /// <summary>
        /// パラメーター付きコンストラクタ。直接インスタンス化時に使用されます。
        /// </summary>
        /// <param name="target">回復対象のヘルスターゲット</param>
        /// <param name="healAmount">回復する体力量（正の値）</param>
        public HealCommand(IHealthTarget target, int healAmount)
        {
            _target = target;
            _healAmount = healAmount;
        }

        /// <summary>
        /// 回復コマンドを実行します。
        /// 対象のIHealthTargetに対してHeal()メソッドを呼び出し、指定された量の体力を回復させます。
        /// </summary>
        public void Execute()
        {
            if (_target == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("HealCommand: Target is null, cannot execute heal");
#endif
                return;
            }
            
            _target.Heal(_healAmount);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Healed {_healAmount} health");
#endif
        }
        

        /// <summary>
        /// コマンドの状態をリセットし、ObjectPoolに返却する準備をします。
        /// IResettableCommandの実装として、プール化された際の再利用前に呼び出されます。
        /// </summary>
        public void Reset()
        {
            _target = null;
            _healAmount = 0;
        }

        /// <summary>
        /// ObjectPool使用時に新しいパラメーターでコマンドを初期化します。
        /// IResettableCommandの実装として、プールからの取得時に呼び出されます。
        /// </summary>
        /// <param name="parameters">初期化パラメーター配列。[0]=IHealthTarget, [1]=int（回復量）</param>
        public void Initialize(params object[] parameters)
        {
            if (parameters.Length < 2)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("HealCommand.Initialize: 最低2つのパラメーター（target, healAmount）が必要です。");
#endif
                return;
            }

            _target = parameters[0] as IHealthTarget;
            if (_target == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("HealCommand.Initialize: 最初のパラメーターはIHealthTargetである必要があります。");
#endif
                return;
            }

            if (parameters[1] is int healAmount)
            {
                _healAmount = healAmount;
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("HealCommand.Initialize: 2番目のパラメーターはint（回復量）である必要があります。");
#endif
                return;
            }
        }
        
        /// <summary>
        /// より型安全な初期化メソッド。
        /// object[]を使用する汎用版と異なり、型安全性が保証されています。
        /// </summary>
        /// <param name="target">回復対象のヘルスターゲット</param>
        /// <param name="healAmount">回復する体力量（正の値）</param>
        public void Initialize(IHealthTarget target, int healAmount)
        {
            _target = target;
            _healAmount = healAmount;
        }
        
        /// <summary>
        /// 回復コマンドを取り消します（Undo）。
        /// 回復した量と同じダメージを対象に与えることで、回復を取り消します。
        /// </summary>
        public void Undo()
        {
            if (_target == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("HealCommand: Target is null, cannot undo heal");
#endif
                return;
            }
            
            _target.TakeDamage(_healAmount, "healing_undo");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Undid {_healAmount} healing (dealt damage)");
#endif
        }
    }
}