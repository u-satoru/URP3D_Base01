using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Data
{
    /// <summary>
    /// 武器の反動パターンを定義するScriptableObject
    /// 詳細設計書3.2準拠: WeaponDataの反動パターンデータ
    /// </summary>
    [CreateAssetMenu(menuName = "Templates/FPS/Recoil Pattern", fileName = "New_RecoilPattern")]
    public class RecoilPattern : ScriptableObject
    {
        [Header("Basic Information")]
        [SerializeField] private string _patternName = "Default Pattern";
        [SerializeField] private string _description = "";
        [SerializeField] private Texture2D _previewTexture;

        [Header("Vertical Recoil (Required)")]
        [SerializeField] private float[] _verticalRecoil = new float[] { 1.0f, 1.2f, 1.5f, 1.8f, 2.0f };
        [SerializeField] private float _verticalIntensity = 1.0f;

        [Header("Horizontal Recoil (Optional)")]
        [SerializeField] private float[] _horizontalRecoil = new float[] { };
        [SerializeField] private float _horizontalIntensity = 1.0f;
        [SerializeField] private bool _enableHorizontalRandomization = true;

        [Header("Pattern Settings")]
        [SerializeField] private bool _loopPattern = true;
        [SerializeField] private int _maxPatternLength = 30;
        [SerializeField] private float _patternResetTime = 2.0f;

        [Header("Weapon Category")]
        [SerializeField] private WeaponType _recommendedWeaponType = WeaponType.AssaultRifle;
        [SerializeField] private bool _isFullAutoOptimized = true;

        [Header("Balance Settings")]
        [SerializeField] private float _skillCompensation = 0.1f; // スキルによる反動軽減係数
        [SerializeField] private float _firstShotMultiplier = 0.8f; // 初弾反動倍率

        [Header("Debug & Validation")]
        [SerializeField] private bool _enableDebugVisualization = false;
        [SerializeField] private Color _recoilVisualizationColor = Color.red;

        /// <summary>
        /// パターン名
        /// </summary>
        public string PatternName => _patternName;

        /// <summary>
        /// 説明
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// 垂直反動配列
        /// </summary>
        public float[] verticalRecoil => _verticalRecoil;

        /// <summary>
        /// 水平反動配列
        /// </summary>
        public float[] horizontalRecoil => _horizontalRecoil;

        /// <summary>
        /// 垂直反動強度
        /// </summary>
        public float VerticalIntensity => _verticalIntensity;

        /// <summary>
        /// 水平反動強度
        /// </summary>
        public float HorizontalIntensity => _horizontalIntensity;

        /// <summary>
        /// 水平ランダム化有効
        /// </summary>
        public bool EnableHorizontalRandomization => _enableHorizontalRandomization;

        /// <summary>
        /// パターンループ
        /// </summary>
        public bool LoopPattern => _loopPattern;

        /// <summary>
        /// 最大パターン長
        /// </summary>
        public int MaxPatternLength => _maxPatternLength;

        /// <summary>
        /// パターンリセット時間
        /// </summary>
        public float PatternResetTime => _patternResetTime;

        /// <summary>
        /// 推奨武器タイプ
        /// </summary>
        public WeaponType RecommendedWeaponType => _recommendedWeaponType;

        /// <summary>
        /// フルオート最適化済み
        /// </summary>
        public bool IsFullAutoOptimized => _isFullAutoOptimized;

        /// <summary>
        /// スキル補正
        /// </summary>
        public float SkillCompensation => _skillCompensation;

        /// <summary>
        /// 初弾倍率
        /// </summary>
        public float FirstShotMultiplier => _firstShotMultiplier;

        /// <summary>
        /// 指定したショット番号での反動値を取得
        /// </summary>
        public Vector2 GetRecoilAtShot(int shotNumber, bool applyFirstShotMultiplier = true)
        {
            if (_verticalRecoil == null || _verticalRecoil.Length == 0)
                return Vector2.zero;

            // 初弾補正
            float firstShotMod = (shotNumber == 0 && applyFirstShotMultiplier) ? _firstShotMultiplier : 1.0f;

            // 垂直反動計算
            int verticalIndex = _loopPattern
                ? shotNumber % _verticalRecoil.Length
                : Mathf.Min(shotNumber, _verticalRecoil.Length - 1);

            float vertical = _verticalRecoil[verticalIndex] * _verticalIntensity * firstShotMod;

            // 水平反動計算
            float horizontal = 0f;
            if (_horizontalRecoil != null && _horizontalRecoil.Length > 0)
            {
                int horizontalIndex = _loopPattern
                    ? shotNumber % _horizontalRecoil.Length
                    : Mathf.Min(shotNumber, _horizontalRecoil.Length - 1);

                horizontal = _horizontalRecoil[horizontalIndex] * _horizontalIntensity * firstShotMod;
            }
            else if (_enableHorizontalRandomization)
            {
                // 水平配列がない場合、垂直反動ベースのランダム水平反動
                horizontal = vertical * 0.15f * (Random.Range(-1f, 1f));
            }

            return new Vector2(horizontal, vertical);
        }

        /// <summary>
        /// パターン全体の平均反動を計算
        /// </summary>
        public Vector2 GetAverageRecoil()
        {
            if (_verticalRecoil == null || _verticalRecoil.Length == 0)
                return Vector2.zero;

            float avgVertical = 0f;
            for (int i = 0; i < _verticalRecoil.Length; i++)
            {
                avgVertical += _verticalRecoil[i];
            }
            avgVertical = (avgVertical / _verticalRecoil.Length) * _verticalIntensity;

            float avgHorizontal = 0f;
            if (_horizontalRecoil != null && _horizontalRecoil.Length > 0)
            {
                for (int i = 0; i < _horizontalRecoil.Length; i++)
                {
                    avgHorizontal += Mathf.Abs(_horizontalRecoil[i]);
                }
                avgHorizontal = (avgHorizontal / _horizontalRecoil.Length) * _horizontalIntensity;
            }
            else if (_enableHorizontalRandomization)
            {
                avgHorizontal = avgVertical * 0.15f * 0.5f; // ランダム水平反動の期待値
            }

            return new Vector2(avgHorizontal, avgVertical);
        }

        /// <summary>
        /// パターンの最大反動を計算
        /// </summary>
        public Vector2 GetMaxRecoil()
        {
            if (_verticalRecoil == null || _verticalRecoil.Length == 0)
                return Vector2.zero;

            float maxVertical = Mathf.Max(_verticalRecoil) * _verticalIntensity;

            float maxHorizontal = 0f;
            if (_horizontalRecoil != null && _horizontalRecoil.Length > 0)
            {
                maxHorizontal = 0f;
                for (int i = 0; i < _horizontalRecoil.Length; i++)
                {
                    float abs = Mathf.Abs(_horizontalRecoil[i]);
                    if (abs > maxHorizontal) maxHorizontal = abs;
                }
                maxHorizontal *= _horizontalIntensity;
            }
            else if (_enableHorizontalRandomization)
            {
                maxHorizontal = maxVertical * 0.15f; // ランダム水平反動の最大値
            }

            return new Vector2(maxHorizontal, maxVertical);
        }

        /// <summary>
        /// プリセットパターン生成: ライフル型
        /// </summary>
        [ContextMenu("Generate Rifle Pattern")]
        public void GenerateRiflePattern()
        {
            _patternName = "Rifle Pattern";
            _description = "Standard assault rifle recoil pattern with vertical climb and slight horizontal drift";
            _recommendedWeaponType = WeaponType.AssaultRifle;
            _isFullAutoOptimized = true;

            _verticalRecoil = new float[]
            {
                0.8f, 1.0f, 1.2f, 1.5f, 1.8f, 2.0f, 2.2f, 2.5f, 2.8f, 3.0f,
                3.1f, 3.2f, 3.4f, 3.5f, 3.6f, 3.8f, 4.0f, 4.2f, 4.5f, 4.8f
            };

            _horizontalRecoil = new float[]
            {
                0.1f, -0.2f, 0.3f, -0.1f, 0.4f, -0.3f, 0.2f, -0.4f, 0.5f, -0.2f,
                0.3f, -0.5f, 0.4f, -0.3f, 0.6f, -0.4f, 0.5f, -0.6f, 0.7f, -0.5f
            };

            _verticalIntensity = 1.0f;
            _horizontalIntensity = 0.8f;
            _enableHorizontalRandomization = true;
            _firstShotMultiplier = 0.7f;
        }

        /// <summary>
        /// プリセットパターン生成: SMG型
        /// </summary>
        [ContextMenu("Generate SMG Pattern")]
        public void GenerateSMGPattern()
        {
            _patternName = "SMG Pattern";
            _description = "Fast firing SMG pattern with moderate recoil and high horizontal movement";
            _recommendedWeaponType = WeaponType.SMG;
            _isFullAutoOptimized = true;

            _verticalRecoil = new float[]
            {
                0.5f, 0.7f, 0.9f, 1.1f, 1.3f, 1.5f, 1.7f, 1.8f, 1.9f, 2.0f,
                2.1f, 2.2f, 2.3f, 2.4f, 2.5f
            };

            _horizontalRecoil = new float[]
            {
                0.2f, -0.4f, 0.6f, -0.3f, 0.8f, -0.6f, 0.4f, -0.8f, 1.0f, -0.5f,
                0.7f, -1.0f, 0.8f, -0.7f, 0.9f
            };

            _verticalIntensity = 0.8f;
            _horizontalIntensity = 1.2f;
            _enableHorizontalRandomization = true;
            _firstShotMultiplier = 0.6f;
        }

        /// <summary>
        /// プリセットパターン生成: スナイパー型
        /// </summary>
        [ContextMenu("Generate Sniper Pattern")]
        public void GenerateSniperPattern()
        {
            _patternName = "Sniper Pattern";
            _description = "High impact single shot pattern with strong initial kick";
            _recommendedWeaponType = WeaponType.Sniper;
            _isFullAutoOptimized = false;

            _verticalRecoil = new float[] { 3.0f, 2.5f, 2.0f };
            _horizontalRecoil = new float[] { 0.2f, -0.3f, 0.1f };

            _verticalIntensity = 1.5f;
            _horizontalIntensity = 0.5f;
            _enableHorizontalRandomization = false;
            _firstShotMultiplier = 1.2f;
            _loopPattern = false;
        }

        /// <summary>
        /// 設定の検証
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;

            if (_verticalRecoil == null || _verticalRecoil.Length == 0)
            {
                Debug.LogError($"[RecoilPattern] {name}: Vertical recoil array is required");
                isValid = false;
            }

            if (_verticalIntensity < 0f)
            {
                Debug.LogWarning($"[RecoilPattern] {name}: Vertical intensity should be positive");
                isValid = false;
            }

            if (_horizontalIntensity < 0f)
            {
                Debug.LogWarning($"[RecoilPattern] {name}: Horizontal intensity should be positive");
                isValid = false;
            }

            if (_maxPatternLength <= 0)
            {
                Debug.LogWarning($"[RecoilPattern] {name}: Max pattern length should be positive");
                isValid = false;
            }

            if (_patternResetTime < 0f)
            {
                Debug.LogWarning($"[RecoilPattern] {name}: Pattern reset time should be non-negative");
                isValid = false;
            }

            if (_skillCompensation < 0f || _skillCompensation > 1f)
            {
                Debug.LogWarning($"[RecoilPattern] {name}: Skill compensation should be between 0 and 1");
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Inspector表示用の統計情報
        /// </summary>
        [System.Serializable]
        public struct PatternStats
        {
            public Vector2 averageRecoil;
            public Vector2 maxRecoil;
            public int patternLength;
            public float totalIntensity;
        }

        /// <summary>
        /// パターン統計を取得
        /// </summary>
        public PatternStats GetPatternStats()
        {
            return new PatternStats
            {
                averageRecoil = GetAverageRecoil(),
                maxRecoil = GetMaxRecoil(),
                patternLength = _verticalRecoil?.Length ?? 0,
                totalIntensity = GetAverageRecoil().magnitude
            };
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタでのプレビュー描画
        /// </summary>
        public void DrawPreview(Rect rect, int maxShots = 20)
        {
            if (_verticalRecoil == null || _verticalRecoil.Length == 0) return;

            Vector2 center = rect.center;
            float scale = Mathf.Min(rect.width, rect.height) * 0.4f;

            Vector2 currentPos = center;
            Vector2 totalRecoil = Vector2.zero;

            for (int i = 0; i < Mathf.Min(maxShots, _maxPatternLength); i++)
            {
                Vector2 recoil = GetRecoilAtShot(i, false);
                totalRecoil += recoil;

                Vector2 nextPos = center + new Vector2(
                    totalRecoil.x * scale * 0.1f,
                    -totalRecoil.y * scale * 0.1f // Y軸反転（画面座標系）
                );

                // 反動ポイントを描画
                UnityEditor.Handles.color = Color.Lerp(_recoilVisualizationColor, Color.white, (float)i / maxShots);
                UnityEditor.Handles.DrawLine(currentPos, nextPos);
                UnityEditor.Handles.DrawWireDisc(nextPos, Vector3.forward, 2f);

                currentPos = nextPos;
            }
        }
#endif

        private void OnValidate()
        {
            // Inspector で値が変更された時の自動検証
            ValidateSettings();

            // 配列の長さ制限
            if (_verticalRecoil != null && _verticalRecoil.Length > _maxPatternLength)
            {
                System.Array.Resize(ref _verticalRecoil, _maxPatternLength);
            }

            if (_horizontalRecoil != null && _horizontalRecoil.Length > _maxPatternLength)
            {
                System.Array.Resize(ref _horizontalRecoil, _maxPatternLength);
            }
        }
    }
}