using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Interaction;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.ActionRPG.Components;
using DG.Tweening;

namespace asterivo.Unity60.Features.ActionRPG.Components
{
    /// <summary>
    /// レベルアップ神殿コンポーネント
    /// プレイヤーがステータスポイントを消費してステータスを上昇させるためのインタラクション対象
    /// </summary>
    public class LevelUpShrine : MonoBehaviour, IInteractable
    {
        [Header("神殿設定")]
        [SerializeField] private string _shrineTitle = "レベルアップ神殿";
        [SerializeField] private string _interactionPrompt = "ステータスを向上させる";
        [SerializeField] private float _interactionRange = 3.0f;
        
        [Header("視覚効果")]
        [SerializeField] private GameObject _glowEffect;
        [SerializeField] private ParticleSystem _interactionParticles;
        [SerializeField] private Light _shrineLight;
        
        [Header("音響効果")]
        [SerializeField] private AudioClip _interactionSound;
        [SerializeField] private AudioClip _levelUpSound;
        [SerializeField] private AudioSource _audioSource;
        
        [Header("イベント")]
        [SerializeField] private GameEvent _onShrineUsed;
        [SerializeField] private GameEvent _onLevelUpUIRequested;

        // UI関連
        private bool _isUIOpen;
        private StatComponent _currentPlayerStats;

        // IInteractable実装
        public string InteractionPrompt => _interactionPrompt;
        public float InteractionRange => _interactionRange;
        public bool CanInteract => !_isUIOpen;
        public int InteractionPriority => 1;
        public Transform InteractionTransform => transform;
        public bool HighlightWhenInRange => true;

        // IInteractable events
        public event System.Action<IInteractable, bool> OnInteractionStateChanged;
        public event System.Action<IInteractable, IInteractor> OnInteracted;

        void Start()
        {
            InitializeShrine();
        }

        /// <summary>
        /// 神殿を初期化
        /// </summary>
        private void InitializeShrine()
        {
            // AudioSourceが設定されていない場合は作成
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                    _audioSource = gameObject.AddComponent<AudioSource>();
            }

            // 光源の初期設定
            if (_shrineLight != null)
            {
                _shrineLight.intensity = 2.0f;
                _shrineLight.color = Color.cyan;
            }

            // エフェクトの初期化
            if (_glowEffect != null)
                _glowEffect.SetActive(true);
        }

        /// <summary>
        /// IInteractable interface implementation
        /// </summary>
        public bool OnInteract(IInteractor interactor)
        {
            if (!CanInteract) return false;

            // プレイヤーのStatComponentを取得
            var playerGameObject = interactor.InteractionOrigin.gameObject;
            _currentPlayerStats = playerGameObject.GetComponent<StatComponent>();
            if (_currentPlayerStats == null)
            {
                Debug.LogWarning("LevelUpShrine: プレイヤーにStatComponentが見つかりません。");
                return false;
            }

            // ステータスポイントがあるかチェック
            if (_currentPlayerStats.AvailableStatPoints <= 0)
            {
                ShowMessage("利用可能なステータスポイントがありません。");
                return false;
            }

            // インタラクションを実行
            PerformInteraction();

            // Invoke interaction event
            OnInteracted?.Invoke(this, interactor);

            return true;
        }

        public void OnInteractionRangeEntered(IInteractor interactor)
        {
            // Optional: Add visual feedback when player enters range
            if (_glowEffect != null)
                _glowEffect.SetActive(true);
        }

        public void OnInteractionRangeExited(IInteractor interactor)
        {
            // Optional: Remove visual feedback when player exits range
        }

        public void OnInteractionFocused(IInteractor interactor)
        {
            // Optional: Add focus feedback
        }

        public void OnInteractionUnfocused(IInteractor interactor)
        {
            // Optional: Remove focus feedback
        }

        public bool CanInteractWith(IInteractor interactor)
        {
            return CanInteract && interactor.CanPerformInteractions;
        }

        public InteractionStatus GetInteractionStatus(IInteractor interactor)
        {
            if (!CanInteract)
                return InteractionStatus.Failure("UI is currently open");

            if (!interactor.CanPerformInteractions)
                return InteractionStatus.Failure("Interactor cannot perform interactions");

            var playerGameObject = interactor.InteractionOrigin.gameObject;
            var stats = playerGameObject.GetComponent<StatComponent>();
            if (stats == null)
                return InteractionStatus.Failure("Player missing StatComponent");

            if (stats.AvailableStatPoints <= 0)
                return InteractionStatus.Failure("No available stat points");

            return InteractionStatus.Success(InteractionPriority);
        }

        /// <summary>
        /// インタラクションを実行
        /// </summary>
        private void PerformInteraction()
        {
            _isUIOpen = true;

            // エフェクト再生
            PlayInteractionEffects();

            // レベルアップUIを開くイベントを発行
            if (_onLevelUpUIRequested != null)
                _onLevelUpUIRequested.Raise();

            // 神殿使用イベントを発行
            if (_onShrineUsed != null)
                _onShrineUsed.Raise();

            Debug.Log("レベルアップ神殿にアクセスしました。");
        }

        /// <summary>
        /// ステータス配分を実行
        /// </summary>
        public bool AllocateStatPoint(StatType statType)
        {
            if (_currentPlayerStats == null) return false;
            if (!_isUIOpen) return false;

            bool success = _currentPlayerStats.AllocateStatPoint(statType, 1);
            
            if (success)
            {
                // レベルアップエフェクト再生
                PlayLevelUpEffects();
                
                ShowMessage($"{GetStatDisplayName(statType)}が上昇しました！");
                
                Debug.Log($"ステータス配分: {statType} +1");
            }
            else
            {
                ShowMessage("ステータスポイントが不足しています。");
            }

            return success;
        }

        /// <summary>
        /// UIを閉じる
        /// </summary>
        public void CloseUI()
        {
            _isUIOpen = false;
            _currentPlayerStats = null;
            
            Debug.Log("レベルアップUIを閉じました。");
        }

        /// <summary>
        /// インタラクションエフェクトを再生
        /// </summary>
        private void PlayInteractionEffects()
        {
            // パーティクルエフェクト
            if (_interactionParticles != null)
                _interactionParticles.Play();

            // 光の強さを一時的に増加
            if (_shrineLight != null)
            {
                _shrineLight.intensity = 4.0f;
                DOTween.To(() => _shrineLight.intensity, x => _shrineLight.intensity = x, 2.0f, 1.0f);
            }

            // インタラクション音再生
            if (_audioSource != null && _interactionSound != null)
                _audioSource.PlayOneShot(_interactionSound);
        }

        /// <summary>
        /// レベルアップエフェクトを再生
        /// </summary>
        private void PlayLevelUpEffects()
        {
            // 強力なパーティクルエフェクト
            if (_interactionParticles != null)
            {
                var emission = _interactionParticles.emission;
                emission.SetBursts(new ParticleSystem.Burst[]
                {
                    new ParticleSystem.Burst(0.0f, 50)
                });
                _interactionParticles.Play();
            }

            // 光の色を変更（一時的に金色に）
            if (_shrineLight != null)
            {
                Color originalColor = _shrineLight.color;
                _shrineLight.color = Color.yellow;
                _shrineLight.intensity = 6.0f;

                DOTween.To(() => _shrineLight.intensity, x => _shrineLight.intensity = x, 2.0f, 1.5f)
                    .OnComplete(() => _shrineLight.color = originalColor);
            }

            // レベルアップ音再生
            if (_audioSource != null && _levelUpSound != null)
                _audioSource.PlayOneShot(_levelUpSound);
        }

        /// <summary>
        /// ステータスの表示名を取得
        /// </summary>
        private string GetStatDisplayName(StatType statType)
        {
            return statType switch
            {
                StatType.Vitality => "生命力",
                StatType.Strength => "筋力",
                StatType.Dexterity => "器用さ",
                StatType.Intelligence => "知力",
                StatType.Faith => "信仰",
                StatType.Luck => "運",
                _ => statType.ToString()
            };
        }

        /// <summary>
        /// メッセージを表示
        /// </summary>
        private void ShowMessage(string message)
        {
            // TODO: UIマネージャーとの連携でメッセージを表示
            Debug.Log($"[神殿メッセージ] {message}");
        }

        /// <summary>
        /// プレイヤーの現在の状態情報を取得
        /// </summary>
        public PlayerStatInfo GetCurrentPlayerInfo()
        {
            if (_currentPlayerStats == null) return null;

            return new PlayerStatInfo
            {
                AvailablePoints = _currentPlayerStats.AvailableStatPoints,
                CurrentLevel = _currentPlayerStats.CurrentLevel,
                Vitality = _currentPlayerStats.Vitality,
                Strength = _currentPlayerStats.Strength,
                Dexterity = _currentPlayerStats.Dexterity,
                Intelligence = _currentPlayerStats.Intelligence,
                Faith = _currentPlayerStats.Faith,
                Luck = _currentPlayerStats.Luck,
                MaxHealth = _currentPlayerStats.MaxHealth,
                MaxFocus = _currentPlayerStats.MaxFocus,
                AttackPower = _currentPlayerStats.AttackPower,
                Defense = _currentPlayerStats.Defense
            };
        }

        /// <summary>
        /// デバッグ用：強制的にステータスポイントを追加
        /// </summary>
        [ContextMenu("Add Stat Points (5)")]
        public void AddTestStatPoints()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var statComponent = player.GetComponent<StatComponent>();
                if (statComponent != null)
                {
                    // リフレクションを使ってAvailableStatPointsを増加
                    var field = typeof(StatComponent).GetField("_availableStatPoints", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        int current = (int)field.GetValue(statComponent);
                        field.SetValue(statComponent, current + 5);
                        Debug.Log("テスト用ステータスポイント +5 を追加しました。");
                    }
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            // インタラクション範囲を表示
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _interactionRange);
        }

        /// <summary>
        /// プレイヤーのステータス情報を格納するデータクラス
        /// </summary>
        public class PlayerStatInfo
        {
            public int AvailablePoints;
            public int CurrentLevel;
            public int Vitality;
            public int Strength;
            public int Dexterity;
            public int Intelligence;
            public int Faith;
            public int Luck;
            public int MaxHealth;
            public int MaxFocus;
            public int AttackPower;
            public int Defense;
        }
    }
}
