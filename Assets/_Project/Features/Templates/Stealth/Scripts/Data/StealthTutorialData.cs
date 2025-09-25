using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// チュートリアルの種類
    /// Learn & Grow価値実現: 段階的学習による70%学習コスト削減
    /// </summary>
    public enum TutorialType
    {
        /// <summary>基本移動とクラウチ操作</summary>
        BasicMovement,
        
        /// <summary>隠蔽スポットの利用方法</summary>
        HidingSpots,
        
        /// <summary>AI検知システムの理解</summary>
        AIDetection,
        
        /// <summary>環境音の活用とマスキング</summary>
        AudioMasking,
        
        /// <summary>複合ステルスタクティクス</summary>
        AdvancedStealth,
        
        /// <summary>ミッション完走</summary>
        MissionComplete,
        
        /// <summary>設定カスタマイズ</summary>
        Configuration,
        
        /// <summary>独自ルール作成</summary>
        CustomRules
    }

    /// <summary>
    /// チュートリアル段階
    /// 5段階学習システムによる段階的成長支援
    /// </summary>
    public enum TutorialPhase
    {
        /// <summary>基礎フェーズ（5-10分）</summary>
        Foundation,
        
        /// <summary>応用フェーズ（10-15分）</summary>
        Application,
        
        /// <summary>実践フェーズ（15-20分）</summary>
        Practice,
        
        /// <summary>カスタマイズフェーズ（20-25分）</summary>
        Customization,
        
        /// <summary>習得完了フェーズ（25-30分）</summary>
        Mastery
    }

    /// <summary>
    /// ステルスチュートリアルステップ
    /// Learn & Grow価値実現のための構造化学習単位
    /// </summary>
    [System.Serializable]
    public class StealthTutorialStep
    {
        [Header("基本情報")]
        [SerializeField] private string _stepName;
        [SerializeField] private TutorialType _tutorialType;
        [SerializeField] private TutorialPhase _phase;
        
        [Header("学習目標")]
        [SerializeField, TextArea(3, 5)] private string _description;
        [SerializeField] private string[] _learningObjectives;
        
        [Header("進捗管理")]
        [SerializeField] private bool _isCompleted;
        [SerializeField] private float _completionTime;
        [SerializeField] private float _startTime;
        [SerializeField] private int _attemptCount;
        
        [Header("難易度設定")]
        [Range(1, 5)]
        [SerializeField] private int _difficultyLevel;
        [SerializeField] private float _expectedDurationMinutes;
        
        [Header("前提条件")]
        [SerializeField] private TutorialType[] _prerequisites;
        [SerializeField] private bool _requiresPreviousCompletion;

        #region Properties

        /// <summary>チュートリアルステップ名</summary>
        public string StepName => _stepName;
        
        /// <summary>チュートリアルタイプ</summary>
        public TutorialType TutorialType => _tutorialType;
        
        /// <summary>学習フェーズ</summary>
        public TutorialPhase Phase => _phase;
        
        /// <summary>説明文</summary>
        public string Description => _description;
        
        /// <summary>学習目標一覧</summary>
        public string[] LearningObjectives => _learningObjectives;
        
        /// <summary>完了状態</summary>
        public bool IsCompleted => _isCompleted;
        
        /// <summary>完了にかかった時間</summary>
        public float CompletionTime => _completionTime;
        
        /// <summary>開始時刻</summary>
        public float StartTime => _startTime;
        
        /// <summary>試行回数</summary>
        public int AttemptCount => _attemptCount;
        
        /// <summary>難易度レベル</summary>
        public int DifficultyLevel => _difficultyLevel;
        
        /// <summary>予想所要時間（分）</summary>
        public float ExpectedDurationMinutes => _expectedDurationMinutes;
        
        /// <summary>前提条件</summary>
        public TutorialType[] Prerequisites => _prerequisites;
        
        /// <summary>前のステップ完了要求</summary>
        public bool RequiresPreviousCompletion => _requiresPreviousCompletion;

        #endregion

        #region Constructors

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public StealthTutorialStep()
        {
            _stepName = "New Tutorial Step";
            _tutorialType = TutorialType.BasicMovement;
            _phase = TutorialPhase.Foundation;
            _description = "";
            _learningObjectives = new string[0];
            _isCompleted = false;
            _completionTime = 0f;
            _startTime = 0f;
            _attemptCount = 0;
            _difficultyLevel = 1;
            _expectedDurationMinutes = 5f;
            _prerequisites = new TutorialType[0];
            _requiresPreviousCompletion = true;
        }

        /// <summary>
        /// パラメータ指定コンストラクタ
        /// </summary>
        public StealthTutorialStep(string stepName, TutorialType tutorialType)
        {
            _stepName = stepName;
            _tutorialType = tutorialType;
            _phase = DeterminePhaseFromType(tutorialType);
            _description = GenerateDefaultDescription(tutorialType);
            _learningObjectives = GenerateDefaultObjectives(tutorialType);
            _isCompleted = false;
            _completionTime = 0f;
            _startTime = 0f;
            _attemptCount = 0;
            _difficultyLevel = DetermineDifficultyFromType(tutorialType);
            _expectedDurationMinutes = DetermineExpectedDuration(tutorialType);
            _prerequisites = DeterminePrerequisites(tutorialType);
            _requiresPreviousCompletion = true;
        }

        /// <summary>
        /// 完全パラメータ指定コンストラクタ
        /// </summary>
        public StealthTutorialStep(string stepName, TutorialType tutorialType, TutorialPhase phase,
                                   string description, string[] objectives, int difficulty, float duration)
        {
            _stepName = stepName;
            _tutorialType = tutorialType;
            _phase = phase;
            _description = description;
            _learningObjectives = objectives ?? new string[0];
            _isCompleted = false;
            _completionTime = 0f;
            _startTime = 0f;
            _attemptCount = 0;
            _difficultyLevel = Mathf.Clamp(difficulty, 1, 5);
            _expectedDurationMinutes = Mathf.Max(duration, 1f);
            _prerequisites = DeterminePrerequisites(tutorialType);
            _requiresPreviousCompletion = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// チュートリアルステップを開始
        /// </summary>
        public void StartStep()
        {
            if (_isCompleted)
            {
                Debug.LogWarning($"Tutorial step '{_stepName}' is already completed");
                return;
            }

            _startTime = Time.time;
            _attemptCount++;
            
            Debug.Log($"Started tutorial step: {_stepName} (Attempt {_attemptCount})");
        }

        /// <summary>
        /// チュートリアルステップを完了
        /// </summary>
        public void CompleteStep()
        {
            if (_isCompleted)
            {
                Debug.LogWarning($"Tutorial step '{_stepName}' is already completed");
                return;
            }

            _isCompleted = true;
            _completionTime = Time.time - _startTime;
            
            Debug.Log($"Completed tutorial step: {_stepName} in {_completionTime:F1} seconds");
        }

        /// <summary>
        /// チュートリアルステップをリセット
        /// </summary>
        public void ResetStep()
        {
            _isCompleted = false;
            _completionTime = 0f;
            _startTime = 0f;
            _attemptCount = 0;
        }

        /// <summary>
        /// 前提条件が満たされているかチェック
        /// </summary>
        public bool ArePrerequisitesMet(StealthTutorialStep[] completedSteps)
        {
            if (_prerequisites == null || _prerequisites.Length == 0)
                return true;

            foreach (var prerequisite in _prerequisites)
            {
                bool found = false;
                foreach (var step in completedSteps)
                {
                    if (step._tutorialType == prerequisite && step._isCompleted)
                    {
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 学習効率を計算（完了時間 vs 予想時間）
        /// </summary>
        public float CalculateLearningEfficiency()
        {
            if (!_isCompleted || _completionTime <= 0f)
                return 0f;

            float expectedSeconds = _expectedDurationMinutes * 60f;
            return Mathf.Clamp01(expectedSeconds / _completionTime);
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// チュートリアルタイプから学習フェーズを決定
        /// </summary>
        private TutorialPhase DeterminePhaseFromType(TutorialType type)
        {
            return type switch
            {
                TutorialType.BasicMovement or TutorialType.HidingSpots => TutorialPhase.Foundation,
                TutorialType.AIDetection or TutorialType.AudioMasking => TutorialPhase.Application,
                TutorialType.AdvancedStealth or TutorialType.MissionComplete => TutorialPhase.Practice,
                TutorialType.Configuration => TutorialPhase.Customization,
                TutorialType.CustomRules => TutorialPhase.Mastery,
                _ => TutorialPhase.Foundation
            };
        }

        /// <summary>
        /// デフォルトの説明文を生成
        /// </summary>
        private string GenerateDefaultDescription(TutorialType type)
        {
            return type switch
            {
                TutorialType.BasicMovement => "WASDキーでの移動とCtrlキーでのクラウチ操作を学習します",
                TutorialType.HidingSpots => "環境内の隠蔽スポットを見つけて効果的に利用する方法を学習します",
                TutorialType.AIDetection => "NPCのAI検知システムの仕組みと回避方法を理解します",
                TutorialType.AudioMasking => "環境音を利用したオーディオマスキング技術を習得します",
                TutorialType.AdvancedStealth => "複数の技術を組み合わせた高度なステルス戦術を実践します",
                TutorialType.MissionComplete => "学習した技術を使って実際のミッションを完了します",
                TutorialType.Configuration => "ゲーム設定をカスタマイズしてプレイスタイルを調整します",
                TutorialType.CustomRules => "独自のルールやチャレンジを作成して楽しみ方を拡張します",
                _ => "チュートリアルの説明文"
            };
        }

        /// <summary>
        /// デフォルトの学習目標を生成
        /// </summary>
        private string[] GenerateDefaultObjectives(TutorialType type)
        {
            return type switch
            {
                TutorialType.BasicMovement => new[] { "WASD移動をマスターする", "クラウチ操作を理解する", "音の出方を意識する" },
                TutorialType.HidingSpots => new[] { "隠蔽スポットを特定する", "効果的な隠れ方を学ぶ", "隠蔽度を理解する" },
                TutorialType.AIDetection => new[] { "視覚検知範囲を理解する", "聴覚検知の仕組みを学ぶ", "警戒レベルを把握する" },
                TutorialType.AudioMasking => new[] { "環境音を識別する", "マスキング効果を活用する", "音のタイミングを計る" },
                TutorialType.AdvancedStealth => new[] { "複合技術を組み合わせる", "状況判断力を向上させる", "効率的なルート選択をする" },
                TutorialType.MissionComplete => new[] { "ミッションを理解する", "戦略を立てる", "目標を達成する" },
                TutorialType.Configuration => new[] { "設定画面を理解する", "好みに合わせて調整する", "変更効果を確認する" },
                TutorialType.CustomRules => new[] { "ルール作成方法を学ぶ", "独自チャレンジを考案する", "コミュニティと共有する" },
                _ => new[] { "基本目標" }
            };
        }

        /// <summary>
        /// チュートリアルタイプから難易度を決定
        /// </summary>
        private int DetermineDifficultyFromType(TutorialType type)
        {
            return type switch
            {
                TutorialType.BasicMovement => 1,
                TutorialType.HidingSpots => 2,
                TutorialType.AIDetection => 2,
                TutorialType.AudioMasking => 3,
                TutorialType.AdvancedStealth => 4,
                TutorialType.MissionComplete => 4,
                TutorialType.Configuration => 2,
                TutorialType.CustomRules => 5,
                _ => 1
            };
        }

        /// <summary>
        /// 予想所要時間を決定
        /// </summary>
        private float DetermineExpectedDuration(TutorialType type)
        {
            return type switch
            {
                TutorialType.BasicMovement => 5f,
                TutorialType.HidingSpots => 5f,
                TutorialType.AIDetection => 10f,
                TutorialType.AudioMasking => 10f,
                TutorialType.AdvancedStealth => 15f,
                TutorialType.MissionComplete => 20f,
                TutorialType.Configuration => 10f,
                TutorialType.CustomRules => 15f,
                _ => 5f
            };
        }

        /// <summary>
        /// 前提条件を決定
        /// </summary>
        private TutorialType[] DeterminePrerequisites(TutorialType type)
        {
            return type switch
            {
                TutorialType.HidingSpots => new[] { TutorialType.BasicMovement },
                TutorialType.AIDetection => new[] { TutorialType.BasicMovement, TutorialType.HidingSpots },
                TutorialType.AudioMasking => new[] { TutorialType.BasicMovement, TutorialType.AIDetection },
                TutorialType.AdvancedStealth => new[] { TutorialType.AIDetection, TutorialType.AudioMasking },
                TutorialType.MissionComplete => new[] { TutorialType.AdvancedStealth },
                TutorialType.CustomRules => new[] { TutorialType.MissionComplete, TutorialType.Configuration },
                _ => new TutorialType[0]
            };
        }

        #endregion

        #region Debug & Utility

        /// <summary>
        /// デバッグ用文字列表現
        /// </summary>
        public override string ToString()
        {
            return $"{_stepName} ({_tutorialType}, Phase: {_phase}, " +
                   $"Difficulty: {_difficultyLevel}, Completed: {_isCompleted})";
        }

        /// <summary>
        /// 詳細情報を取得
        /// </summary>
        public string GetDetailedInfo()
        {
            return $"=== Tutorial Step Info ===\n" +
                   $"Name: {_stepName}\n" +
                   $"Type: {_tutorialType}\n" +
                   $"Phase: {_phase}\n" +
                   $"Description: {_description}\n" +
                   $"Difficulty: {_difficultyLevel}/5\n" +
                   $"Expected Duration: {_expectedDurationMinutes} minutes\n" +
                   $"Completed: {_isCompleted}\n" +
                   $"Completion Time: {_completionTime:F1} seconds\n" +
                   $"Attempts: {_attemptCount}\n" +
                   $"Learning Efficiency: {CalculateLearningEfficiency():P1}";
        }

        #endregion
    }
}
