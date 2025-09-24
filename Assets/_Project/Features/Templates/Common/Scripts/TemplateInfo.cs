using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Common
{
    /// <summary>
    /// テンプレートの情報を格納するデータ構造
    /// BaseTemplateManager.GetTemplateInfo()の戻り値として使用
    /// </summary>
    [Serializable]
    public class TemplateInfo
    {
        [Header("基本情報")]
        [SerializeField] private string _name;
        [SerializeField] private string _description;
        [SerializeField] private GenreType _genreType;
        [SerializeField] private string _version = "1.0.0";

        [Header("機能情報")]
        [SerializeField] private string[] _specialFeatures;
        [SerializeField] private string[] _requiredComponents;
        [SerializeField] private string[] _optionalComponents;

        [Header("学習情報")]
        [SerializeField] private int _estimatedLearningTimeMinutes;
        [SerializeField] private int _estimatedGameplayMinutes;
        [SerializeField] private GenrePriority _priority;

        [Header("技術情報")]
        [SerializeField] private bool _requiresAI;
        [SerializeField] private bool _requiresAudio;
        [SerializeField] private bool _requiresNetworking;
        [SerializeField] private int _maxNPCCount;

        // Properties
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public string Description
        {
            get => _description;
            set => _description = value;
        }

        public GenreType GenreType
        {
            get => _genreType;
            set => _genreType = value;
        }

        public string Version
        {
            get => _version;
            set => _version = value;
        }

        public string[] SpecialFeatures
        {
            get => _specialFeatures ?? new string[0];
            set => _specialFeatures = value;
        }

        public string[] RequiredComponents
        {
            get => _requiredComponents ?? new string[0];
            set => _requiredComponents = value;
        }

        public string[] OptionalComponents
        {
            get => _optionalComponents ?? new string[0];
            set => _optionalComponents = value;
        }

        public int EstimatedLearningTimeMinutes
        {
            get => _estimatedLearningTimeMinutes;
            set => _estimatedLearningTimeMinutes = value;
        }

        public int EstimatedGameplayMinutes
        {
            get => _estimatedGameplayMinutes;
            set => _estimatedGameplayMinutes = value;
        }

        public GenrePriority Priority
        {
            get => _priority;
            set => _priority = value;
        }

        public bool RequiresAI
        {
            get => _requiresAI;
            set => _requiresAI = value;
        }

        public bool RequiresAudio
        {
            get => _requiresAudio;
            set => _requiresAudio = value;
        }

        public bool RequiresNetworking
        {
            get => _requiresNetworking;
            set => _requiresNetworking = value;
        }

        public int MaxNPCCount
        {
            get => _maxNPCCount;
            set => _maxNPCCount = value;
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public TemplateInfo()
        {
            _specialFeatures = new string[0];
            _requiredComponents = new string[0];
            _optionalComponents = new string[0];
        }

        /// <summary>
        /// 基本情報を指定するコンストラクタ
        /// </summary>
        /// <param name="name">テンプレート名</param>
        /// <param name="description">説明</param>
        /// <param name="genreType">ジャンルタイプ</param>
        public TemplateInfo(string name, string description, GenreType genreType) : this()
        {
            _name = name;
            _description = description;
            _genreType = genreType;
            _priority = GenreUtilities.GetPriority(genreType);
            _estimatedLearningTimeMinutes = GenreUtilities.GetEstimatedLearningTimeMinutes(genreType);
        }

        /// <summary>
        /// テンプレート情報の妥当性をチェック
        /// </summary>
        /// <returns>有効な場合true</returns>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(_name))
                return false;

            if (string.IsNullOrEmpty(_description))
                return false;

            if (_estimatedLearningTimeMinutes <= 0)
                return false;

            if (_estimatedGameplayMinutes <= 0)
                return false;

            return true;
        }

        /// <summary>
        /// デバッグ用の文字列表現
        /// </summary>
        /// <returns>デバッグ文字列</returns>
        public override string ToString()
        {
            return $"{_name} ({_genreType}) - {_description}";
        }
    }
}
