using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace asterivo.Unity60.Core.Setup
{
    /// <summary>
    /// ジャンル管理システム
    /// Phase 2 Clone & Create価値実現のための6ジャンル管理コンポーネント
    /// SetupWizardWindowとGameGenreScriptableObjectを橋渡しする中央管理システム
    /// </summary>
    public class GenreManager : ScriptableObject
    {
        [Header("ジャンルテンプレート")]
        [SerializeField] private List<GameGenre> availableGenres = new List<GameGenre>();
        
        [Header("デフォルト設定")]
        [SerializeField] private GameGenreType defaultGenre = GameGenreType.Adventure;
        [SerializeField] private bool autoLoadGenres = true;
        
        // 実行時キャッシュ
        private Dictionary<GameGenreType, GameGenre> genreCache = new Dictionary<GameGenreType, GameGenre>();
        private bool cacheInitialized = false;
        
        #region Properties
        
        /// <summary>
        /// 利用可能なジャンル一覧
        /// </summary>
        public IReadOnlyList<GameGenre> AvailableGenres => availableGenres.AsReadOnly();
        
        /// <summary>
        /// サポートするジャンル数
        /// </summary>
        public int GenreCount => availableGenres.Count;
        
        /// <summary>
        /// デフォルトジャンル
        /// </summary>
        public GameGenreType DefaultGenre => defaultGenre;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// ジャンルマネージャーの初期化
        /// </summary>
        public void Initialize()
        {
            if (autoLoadGenres)
            {
                LoadAllGenres();
            }
            InitializeCache();
            ValidateGenres();
        }
        
        /// <summary>
        /// 全ジャンルの自動読み込み
        /// </summary>
        private void LoadAllGenres()
        {
            #if UNITY_EDITOR
            string[] genreGuids = AssetDatabase.FindAssets("t:GameGenre");
            availableGenres.Clear();
            
            foreach (string guid in genreGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameGenre genre = AssetDatabase.LoadAssetAtPath<GameGenre>(path);
                if (genre != null && !availableGenres.Contains(genre))
                {
                    availableGenres.Add(genre);
                }
            }
            
            // ジャンル種別でソート
            availableGenres = availableGenres.OrderBy(g => g.GenreType).ToList();
            
            Debug.Log($"[GenreManager] Loaded {availableGenres.Count} genres");
            #endif
        }
        
        /// <summary>
        /// キャッシュの初期化
        /// </summary>
        private void InitializeCache()
        {
            genreCache.Clear();
            foreach (var genre in availableGenres)
            {
                if (genre != null && !genreCache.ContainsKey(genre.GenreType))
                {
                    genreCache[genre.GenreType] = genre;
                }
            }
            cacheInitialized = true;
        }
        
        #endregion
        
        #region Genre Access
        
        /// <summary>
        /// 指定ジャンルタイプのGameGenreを取得
        /// </summary>
        public GameGenre GetGenre(GameGenreType genreType)
        {
            if (!cacheInitialized)
            {
                InitializeCache();
            }
            
            return genreCache.TryGetValue(genreType, out GameGenre genre) ? genre : null;
        }
        
        /// <summary>
        /// 全サポートジャンルタイプ取得
        /// </summary>
        public GameGenreType[] GetSupportedGenreTypes()
        {
            return genreCache.Keys.ToArray();
        }
        
        /// <summary>
        /// ジャンル存在確認
        /// </summary>
        public bool HasGenre(GameGenreType genreType)
        {
            return genreCache.ContainsKey(genreType);
        }
        
        /// <summary>
        /// デフォルトジャンルの取得
        /// </summary>
        public GameGenre GetDefaultGenre()
        {
            return GetGenre(defaultGenre) ?? (availableGenres.Count > 0 ? availableGenres[0] : null);
        }
        
        #endregion
        
        #region Display Utilities
        
        /// <summary>
        /// ジャンルの表示名取得
        /// </summary>
        public string GetDisplayName(GameGenreType genreType)
        {
            var genre = GetGenre(genreType);
            return genre != null ? genre.DisplayName : genreType.ToString();
        }
        
        /// <summary>
        /// ジャンルの説明文取得
        /// </summary>
        public string GetDescription(GameGenreType genreType)
        {
            var genre = GetGenre(genreType);
            return genre != null ? genre.Description : $"{genreType} genre template";
        }
        
        /// <summary>
        /// プレビュー画像取得
        /// </summary>
        public Texture2D GetPreviewImage(GameGenreType genreType)
        {
            var genre = GetGenre(genreType);
            return genre?.PreviewImage;
        }
        
        /// <summary>
        /// プレビュー動画取得
        /// </summary>
        public VideoClip GetPreviewVideo(GameGenreType genreType)
        {
            var genre = GetGenre(genreType);
            return genre?.PreviewVideo;
        }
        
        /// <summary>
        /// プレビュー音声取得
        /// </summary>
        public AudioClip GetPreviewAudio(GameGenreType genreType)
        {
            var genre = GetGenre(genreType);
            return genre?.PreviewAudio;
        }
        
        #endregion
        
        #region Configuration Access
        
        /// <summary>
        /// 必須モジュール一覧取得
        /// </summary>
        public IReadOnlyList<string> GetRequiredModules(GameGenreType genreType)
        {
            var genre = GetGenre(genreType);
            return genre?.RequiredModules ?? new List<string>().AsReadOnly();
        }
        
        /// <summary>
        /// 推奨モジュール一覧取得
        /// </summary>
        public IReadOnlyList<string> GetRecommendedModules(GameGenreType genreType)
        {
            var genre = GetGenre(genreType);
            return genre?.RecommendedModules ?? new List<string>().AsReadOnly();
        }
        
        /// <summary>
        /// オプションモジュール一覧取得
        /// </summary>
        public IReadOnlyList<string> GetOptionalModules(GameGenreType genreType)
        {
            var genre = GetGenre(genreType);
            return genre?.OptionalModules ?? new List<string>().AsReadOnly();
        }
        
        /// <summary>
        /// ジャンルの全モジュール取得（必須+推奨+オプション）
        /// </summary>
        public List<string> GetAllModules(GameGenreType genreType)
        {
            var genre = GetGenre(genreType);
            if (genre == null) return new List<string>();
            
            var allModules = new List<string>();
            allModules.AddRange(genre.RequiredModules);
            allModules.AddRange(genre.RecommendedModules);
            allModules.AddRange(genre.OptionalModules);
            
            return allModules.Distinct().ToList();
        }
        
        #endregion
        
        #region Validation & Management
        
        /// <summary>
        /// ジャンル設定の検証
        /// </summary>
        public bool ValidateGenres()
        {
            bool isValid = true;
            
            foreach (var genre in availableGenres)
            {
                if (genre == null)
                {
                    Debug.LogError("[GenreManager] Null genre found in available genres list");
                    isValid = false;
                    continue;
                }
                
                if (!genre.ValidateConfiguration())
                {
                    Debug.LogError($"[GenreManager] Invalid configuration for genre: {genre.name}");
                    isValid = false;
                }
            }
            
            // 6ジャンル全て揃っているかチェック
            var expectedGenres = Enum.GetValues(typeof(GameGenreType)).Cast<GameGenreType>().ToArray();
            foreach (var expectedGenre in expectedGenres)
            {
                if (!HasGenre(expectedGenre))
                {
                    Debug.LogWarning($"[GenreManager] Missing genre: {expectedGenre}");
                }
            }
            
            return isValid;
        }
        
        /// <summary>
        /// ジャンル統計情報の出力
        /// </summary>
        public void LogGenreStatistics()
        {
            Debug.Log("=== Genre Manager Statistics ===");
            Debug.Log($"Total Genres: {availableGenres.Count}");
            Debug.Log($"Cached Genres: {genreCache.Count}");
            Debug.Log($"Default Genre: {defaultGenre}");
            
            foreach (var kvp in genreCache)
            {
                var genre = kvp.Value;
                Debug.Log($"Genre: {kvp.Key} - {genre.DisplayName}");
                Debug.Log($"  Required Modules: {genre.RequiredModules.Count}");
                Debug.Log($"  Has Preview Image: {(genre.PreviewImage != null)}");
                Debug.Log($"  Has Preview Video: {(genre.PreviewVideo != null)}");
            }
        }
        
        #endregion
        
        #region Editor Utilities
        
        #if UNITY_EDITOR
        
        /// <summary>
        /// 不足ジャンルの自動作成
        /// </summary>
        [MenuItem("asterivo.Unity60/Setup/Create Missing Genres")]
        public static void CreateMissingGenres()
        {
            var expectedGenres = Enum.GetValues(typeof(GameGenreType)).Cast<GameGenreType>().ToArray();
            
            foreach (var genreType in expectedGenres)
            {
                // 既に存在するかチェック
                string[] existingGuids = AssetDatabase.FindAssets($"GameGenre_{genreType}");
                if (existingGuids.Length > 0) continue;
                
                // 新しいGameGenreアセット作成
                var newGenre = CreateInstance<GameGenre>();
                
                // 基本設定（プライベートフィールドのため、SerializedObjectで設定）
                var serializedObject = new SerializedObject(newGenre);
                serializedObject.FindProperty("genreType").enumValueIndex = (int)genreType;
                serializedObject.FindProperty("displayName").stringValue = GetDefaultDisplayName(genreType);
                serializedObject.FindProperty("description").stringValue = GetDefaultDescription(genreType);
                serializedObject.ApplyModifiedProperties();
                
                // 推奨設定の適用
                newGenre.ApplyRecommendedSettings();
                
                // アセットとして保存
                string path = $"Assets/_Project/Core/Setup/GenreTemplates/GameGenre_{genreType}.asset";
                AssetDatabase.CreateAsset(newGenre, path);
                
                Debug.Log($"[GenreManager] Created genre template: {path}");
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private static string GetDefaultDisplayName(GameGenreType genreType)
        {
            switch (genreType)
            {
                case GameGenreType.FPS: return "First Person Shooter";
                case GameGenreType.TPS: return "Third Person Shooter";
                case GameGenreType.Platformer: return "3D Platformer";
                case GameGenreType.Stealth: return "Stealth Action";
                case GameGenreType.Adventure: return "Adventure Game";
                case GameGenreType.Strategy: return "Real-Time Strategy";
                default: return genreType.ToString();
            }
        }
        
        private static string GetDefaultDescription(GameGenreType genreType)
        {
            switch (genreType)
            {
                case GameGenreType.FPS: 
                    return "一人称視点のシューティングゲーム。プレイヤーが主人公の視点で敵と戦う高アクションゲームです。";
                case GameGenreType.TPS:
                    return "三人称視点のシューティングゲーム。キャラクターを外から見た視点で戦闘とアクションを楽しめます。";
                case GameGenreType.Platformer:
                    return "3Dプラットフォーマーゲーム。ジャンプアクションを中心とした立体的なステージ攻略ゲームです。";
                case GameGenreType.Stealth:
                    return "ステルスアクションゲーム。隠れながら敵の視線をかいくぐり、密かに目標を達成する戦略性の高いゲームです。";
                case GameGenreType.Adventure:
                    return "アドベンチャーゲーム。探索と謎解きを中心とした物語重視のゲーム体験を提供します。";
                case GameGenreType.Strategy:
                    return "リアルタイムストラテジーゲーム。多数のユニットを指揮して戦略的な判断で勝利を目指します。";
                default:
                    return $"{genreType}ジャンルのゲームテンプレートです。";
            }
        }
        
        #endif
        
        #endregion
    }
}