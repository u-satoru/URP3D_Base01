using System;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.ActionRPG.Data
{
    /// <summary>
    /// ActionRPGテンプレート用キャラクターカスタマイゼーション管理
    /// 外見、装備外観、カラーパレット管理
    /// </summary>
    public class CharacterCustomizationManager : MonoBehaviour
    {
        [Header("カスタマイゼーション設定")]
        [SerializeField] private bool enableRuntimeCustomization = true;
        [SerializeField] private bool saveCustomizationData = true;
        [SerializeField] private string customizationSaveKey = "CharacterCustomization";
        
        [Header("外見パーツ")]
        [SerializeField] private List<GameObject> hairStyles = new();
        [SerializeField] private List<GameObject> faceVariations = new();
        [SerializeField] private List<GameObject> bodyTypes = new();
        [SerializeField] private List<Material> skinTones = new();
        [SerializeField] private List<Color> hairColors = new();
        [SerializeField] private List<Color> eyeColors = new();
        
        [Header("装備外観")]
        [SerializeField] private List<GameObject> weaponModels = new();
        [SerializeField] private List<GameObject> armorSets = new();
        [SerializeField] private List<GameObject> accessoryModels = new();
        
        [Header("イベント")]
        [SerializeField] private StringGameEvent onCustomizationChanged;
        [SerializeField] private StringGameEvent onEquipmentAppearanceChanged;
        
        // カスタマイゼーションデータ
        public CharacterCustomizationData CurrentCustomization { get; private set; } = new();
        
        // 現在適用中の外見パーツ
        private GameObject currentHair;
        private GameObject currentFace;
        private GameObject currentBody;
        private Renderer[] characterRenderers;
        
        private void Start()
        {
            InitializeCustomization();
            LoadCustomizationData();
        }
        
        /// <summary>
        /// カスタマイゼーション初期化
        /// </summary>
        private void InitializeCustomization()
        {
            // キャラクターレンダラー取得
            characterRenderers = GetComponentsInChildren<Renderer>();
            
            // デフォルト外見適用
            ApplyDefaultAppearance();
            
            Debug.Log("[CharacterCustomizationManager] カスタマイゼーション初期化完了");
        }
        
        /// <summary>
        /// デフォルト外見適用
        /// </summary>
        private void ApplyDefaultAppearance()
        {
            CurrentCustomization.HairStyleIndex = 0;
            CurrentCustomization.FaceIndex = 0;
            CurrentCustomization.BodyTypeIndex = 0;
            CurrentCustomization.SkinToneIndex = 0;
            CurrentCustomization.HairColorIndex = 0;
            CurrentCustomization.EyeColorIndex = 0;
            
            ApplyCustomization(CurrentCustomization);
        }
        
        /// <summary>
        /// カスタマイゼーション適用
        /// </summary>
        public void ApplyCustomization(CharacterCustomizationData customization)
        {
            if (customization == null) return;
            
            CurrentCustomization = customization;
            
            // ヘアスタイル適用
            ApplyHairStyle(customization.HairStyleIndex);
            
            // 顔タイプ適用
            ApplyFaceVariation(customization.FaceIndex);
            
            // 体型適用
            ApplyBodyType(customization.BodyTypeIndex);
            
            // 肌色適用
            ApplySkinTone(customization.SkinToneIndex);
            
            // 髪色適用
            ApplyHairColor(customization.HairColorIndex);
            
            // 瞳色適用
            ApplyEyeColor(customization.EyeColorIndex);
            
            onCustomizationChanged?.Raise("全体外見変更");
            
            Debug.Log("[CharacterCustomizationManager] カスタマイゼーション適用完了");
        }
        
        /// <summary>
        /// ヘアスタイル変更
        /// </summary>
        public void ChangeHairStyle(int styleIndex)
        {
            if (styleIndex < 0 || styleIndex >= hairStyles.Count) return;
            
            CurrentCustomization.HairStyleIndex = styleIndex;
            ApplyHairStyle(styleIndex);
            
            onCustomizationChanged?.Raise($"ヘアスタイル: {styleIndex}");
        }
        
        /// <summary>
        /// ヘアスタイル適用
        /// </summary>
        private void ApplyHairStyle(int styleIndex)
        {
            // 現在の髪を非表示
            if (currentHair != null)
                currentHair.SetActive(false);
                
            // 新しい髪を表示
            if (styleIndex >= 0 && styleIndex < hairStyles.Count && hairStyles[styleIndex] != null)
            {
                currentHair = hairStyles[styleIndex];
                currentHair.SetActive(true);
            }
        }
        
        /// <summary>
        /// 顔タイプ変更
        /// </summary>
        public void ChangeFaceVariation(int faceIndex)
        {
            if (faceIndex < 0 || faceIndex >= faceVariations.Count) return;
            
            CurrentCustomization.FaceIndex = faceIndex;
            ApplyFaceVariation(faceIndex);
            
            onCustomizationChanged?.Raise($"顔タイプ: {faceIndex}");
        }
        
        /// <summary>
        /// 顔タイプ適用
        /// </summary>
        private void ApplyFaceVariation(int faceIndex)
        {
            // 現在の顔を非表示
            if (currentFace != null)
                currentFace.SetActive(false);
                
            // 新しい顔を表示
            if (faceIndex >= 0 && faceIndex < faceVariations.Count && faceVariations[faceIndex] != null)
            {
                currentFace = faceVariations[faceIndex];
                currentFace.SetActive(true);
            }
        }
        
        /// <summary>
        /// 体型変更
        /// </summary>
        public void ChangeBodyType(int bodyIndex)
        {
            if (bodyIndex < 0 || bodyIndex >= bodyTypes.Count) return;
            
            CurrentCustomization.BodyTypeIndex = bodyIndex;
            ApplyBodyType(bodyIndex);
            
            onCustomizationChanged?.Raise($"体型: {bodyIndex}");
        }
        
        /// <summary>
        /// 体型適用
        /// </summary>
        private void ApplyBodyType(int bodyIndex)
        {
            // 現在の体型を非表示
            if (currentBody != null)
                currentBody.SetActive(false);
                
            // 新しい体型を表示
            if (bodyIndex >= 0 && bodyIndex < bodyTypes.Count && bodyTypes[bodyIndex] != null)
            {
                currentBody = bodyTypes[bodyIndex];
                currentBody.SetActive(true);
            }
        }
        
        /// <summary>
        /// 肌色変更
        /// </summary>
        public void ChangeSkinTone(int toneIndex)
        {
            if (toneIndex < 0 || toneIndex >= skinTones.Count) return;
            
            CurrentCustomization.SkinToneIndex = toneIndex;
            ApplySkinTone(toneIndex);
            
            onCustomizationChanged?.Raise($"肌色: {toneIndex}");
        }
        
        /// <summary>
        /// 肌色適用
        /// </summary>
        private void ApplySkinTone(int toneIndex)
        {
            if (toneIndex >= 0 && toneIndex < skinTones.Count && skinTones[toneIndex] != null)
            {
                var skinMaterial = skinTones[toneIndex];
                
                // キャラクターの肌部分に適用
                foreach (var renderer in characterRenderers)
                {
                    if (renderer.gameObject.name.Contains("Skin") || 
                        renderer.gameObject.name.Contains("Body"))
                    {
                        renderer.material = skinMaterial;
                    }
                }
            }
        }
        
        /// <summary>
        /// 髪色変更
        /// </summary>
        public void ChangeHairColor(int colorIndex)
        {
            if (colorIndex < 0 || colorIndex >= hairColors.Count) return;
            
            CurrentCustomization.HairColorIndex = colorIndex;
            ApplyHairColor(colorIndex);
            
            onCustomizationChanged?.Raise($"髪色: {colorIndex}");
        }
        
        /// <summary>
        /// 髪色適用
        /// </summary>
        private void ApplyHairColor(int colorIndex)
        {
            if (colorIndex >= 0 && colorIndex < hairColors.Count && currentHair != null)
            {
                var hairColor = hairColors[colorIndex];
                var hairRenderer = currentHair.GetComponent<Renderer>();
                
                if (hairRenderer != null)
                {
                    hairRenderer.material.color = hairColor;
                }
            }
        }
        
        /// <summary>
        /// 瞳色変更
        /// </summary>
        public void ChangeEyeColor(int colorIndex)
        {
            if (colorIndex < 0 || colorIndex >= eyeColors.Count) return;
            
            CurrentCustomization.EyeColorIndex = colorIndex;
            ApplyEyeColor(colorIndex);
            
            onCustomizationChanged?.Raise($"瞳色: {colorIndex}");
        }
        
        /// <summary>
        /// 瞳色適用
        /// </summary>
        private void ApplyEyeColor(int colorIndex)
        {
            if (colorIndex >= 0 && colorIndex < eyeColors.Count && currentFace != null)
            {
                var eyeColor = eyeColors[colorIndex];
                
                // 顔パーツ内の瞳部分を探す
                var eyeRenderers = currentFace.GetComponentsInChildren<Renderer>();
                foreach (var renderer in eyeRenderers)
                {
                    if (renderer.gameObject.name.Contains("Eye"))
                    {
                        renderer.material.color = eyeColor;
                    }
                }
            }
        }
        
        /// <summary>
        /// 装備外観変更
        /// </summary>
        public void ChangeEquipmentAppearance(string equipmentSlot, int modelIndex)
        {
            switch (equipmentSlot.ToLower())
            {
                case "weapon":
                    ChangeWeaponModel(modelIndex);
                    break;
                case "armor":
                    ChangeArmorModel(modelIndex);
                    break;
                case "accessory":
                    ChangeAccessoryModel(modelIndex);
                    break;
            }
            
            onEquipmentAppearanceChanged?.Raise($"{equipmentSlot}: {modelIndex}");
        }
        
        /// <summary>
        /// 武器モデル変更
        /// </summary>
        private void ChangeWeaponModel(int modelIndex)
        {
            // 武器モデル変更処理（実装は装備システムと連携）
            Debug.Log($"[CharacterCustomizationManager] 武器モデル変更: {modelIndex}");
        }
        
        /// <summary>
        /// 防具モデル変更
        /// </summary>
        private void ChangeArmorModel(int modelIndex)
        {
            // 防具モデル変更処理
            Debug.Log($"[CharacterCustomizationManager] 防具モデル変更: {modelIndex}");
        }
        
        /// <summary>
        /// アクセサリモデル変更
        /// </summary>
        private void ChangeAccessoryModel(int modelIndex)
        {
            // アクセサリモデル変更処理
            Debug.Log($"[CharacterCustomizationManager] アクセサリモデル変更: {modelIndex}");
        }
        
        /// <summary>
        /// カスタマイゼーションデータ保存
        /// </summary>
        public void SaveCustomizationData()
        {
            if (!saveCustomizationData) return;
            
            string jsonData = JsonUtility.ToJson(CurrentCustomization);
            PlayerPrefs.SetString(customizationSaveKey, jsonData);
            PlayerPrefs.Save();
            
            Debug.Log("[CharacterCustomizationManager] カスタマイゼーションデータ保存完了");
        }
        
        /// <summary>
        /// カスタマイゼーションデータ読み込み
        /// </summary>
        private void LoadCustomizationData()
        {
            if (!saveCustomizationData) return;
            
            if (PlayerPrefs.HasKey(customizationSaveKey))
            {
                string jsonData = PlayerPrefs.GetString(customizationSaveKey);
                var loadedCustomization = JsonUtility.FromJson<CharacterCustomizationData>(jsonData);
                
                if (loadedCustomization != null)
                {
                    ApplyCustomization(loadedCustomization);
                    Debug.Log("[CharacterCustomizationManager] カスタマイゼーションデータ読み込み完了");
                }
            }
        }
        
        /// <summary>
        /// ランダムカスタマイゼーション生成
        /// </summary>
        public void GenerateRandomCustomization()
        {
            var randomCustomization = new CharacterCustomizationData
            {
                HairStyleIndex = UnityEngine.Random.Range(0, hairStyles.Count),
                FaceIndex = UnityEngine.Random.Range(0, faceVariations.Count),
                BodyTypeIndex = UnityEngine.Random.Range(0, bodyTypes.Count),
                SkinToneIndex = UnityEngine.Random.Range(0, skinTones.Count),
                HairColorIndex = UnityEngine.Random.Range(0, hairColors.Count),
                EyeColorIndex = UnityEngine.Random.Range(0, eyeColors.Count)
            };
            
            ApplyCustomization(randomCustomization);
            
            Debug.Log("[CharacterCustomizationManager] ランダムカスタマイゼーション適用");
        }
    }
    
    /// <summary>
    /// キャラクターカスタマイゼーションデータ
    /// </summary>
    [System.Serializable]
    public class CharacterCustomizationData
    {
        [Header("外見設定")]
        public int HairStyleIndex = 0;
        public int FaceIndex = 0;
        public int BodyTypeIndex = 0;
        public int SkinToneIndex = 0;
        public int HairColorIndex = 0;
        public int EyeColorIndex = 0;
        
        [Header("装備外観設定")]
        public int WeaponModelIndex = 0;
        public int ArmorModelIndex = 0;
        public int AccessoryModelIndex = 0;
        
        [Header("追加設定")]
        public string CharacterName = "Player";
        public string CreationDate = "";
        
        public CharacterCustomizationData()
        {
            CreationDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}