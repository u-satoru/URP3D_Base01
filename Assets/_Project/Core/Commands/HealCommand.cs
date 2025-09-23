using UnityEngine;
// using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// 対象の体力を回復させるコマンド実裁E��E    /// IResettableCommandを実裁E��ており、ObjectPoolによる再利用に対応してぁE��す、E    /// 
    /// 主な機�E�E�E    /// - 体力の回復処琁E�E実衁E    /// - Undo操作によるダメージの適用�E�回復の取り消し�E�E    /// - ObjectPool使用時�E状態リセチE��機�E
    /// - パラメーターによる柔軟な初期匁E    /// </summary>
    public class HealCommand : IResettableCommand
    {
        /// <summary>
        /// 回復処琁E�E対象となる�EルスターゲチE��
        /// </summary>
        private IHealthTarget _target;
        
        /// <summary>
        /// 回復する体力の釁E        /// </summary>
        private int _healAmount;

        /// <summary>
        /// こ�EコマンドがUndo操作をサポ�EトするかどぁE��を示します、E        /// 回復コマンド�E常にUndo可能�E�ダメージに変換�E�です、E        /// </summary>
        public bool CanUndo => true; 

        /// <summary>
        /// プ�Eル化対応�EチE��ォルトコンストラクタ、E        /// ObjectPool使用時に忁E��な引数なしコンストラクタです、E        /// 実際のパラメータは後でInitialize()メソチE��で設定します、E        /// </summary>
        public HealCommand()
        {
            // プ�Eル化対応：パラメーターなしコンストラクタ
        }

        /// <summary>
        /// パラメーター付きコンストラクタ。直接インスタンス化時に使用されます、E        /// </summary>
        /// <param name="target">回復対象のヘルスターゲチE��</param>
        /// <param name="healAmount">回復する体力量（正の値�E�E/param>
        public HealCommand(IHealthTarget target, int healAmount)
        {
            _target = target;
            _healAmount = healAmount;
        }

        /// <summary>
        /// 回復コマンドを実行します、E        /// 対象のIHealthTargetに対してHeal()メソチE��を呼び出し、指定された量�E体力を回復させます、E        /// </summary>
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
        /// コマンド�E状態をリセチE��し、ObjectPoolに返却する準備をします、E        /// IResettableCommandの実裁E��して、�Eール化された際�E再利用前に呼び出されます、E        /// </summary>
        public void Reset()
        {
            _target = null;
            _healAmount = 0;
        }

        /// <summary>
        /// ObjectPool使用時に新しいパラメーターでコマンドを初期化します、E        /// IResettableCommandの実裁E��して、�Eールからの取得時に呼び出されます、E        /// </summary>
        /// <param name="parameters">初期化パラメーター配�E、E0]=IHealthTarget, [1]=int�E�回復量！E/param>
        public void Initialize(params object[] parameters)
        {
            if (parameters.Length < 2)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("HealCommand.Initialize: 最佁Eつのパラメーター�E�Earget, healAmount�E�が忁E��です、E);
#endif
                return;
            }

            _target = parameters[0] as IHealthTarget;
            if (_target == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("HealCommand.Initialize: 最初�EパラメーターはIHealthTargetである忁E��があります、E);
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
                UnityEngine.Debug.LogError("HealCommand.Initialize: 2番目のパラメーターはint�E�回復量）である忁E��があります、E);
#endif
                return;
            }
        }
        
        /// <summary>
        /// より型安�Eな初期化メソチE��、E        /// object[]を使用する汎用版と異なり、型安�E性が保証されてぁE��す、E        /// </summary>
        /// <param name="target">回復対象のヘルスターゲチE��</param>
        /// <param name="healAmount">回復する体力量（正の値�E�E/param>
        public void Initialize(IHealthTarget target, int healAmount)
        {
            _target = target;
            _healAmount = healAmount;
        }
        
        /// <summary>
        /// 回復コマンドを取り消します！Endo�E�、E        /// 回復した量と同じダメージを対象に与えることで、回復を取り消します、E        /// </summary>
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