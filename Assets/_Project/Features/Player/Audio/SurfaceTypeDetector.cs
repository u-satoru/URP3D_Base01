using UnityEngine;
using asterivo.Unity60.Core.Audio.Data;

namespace asterivo.Unity60.Features.Player.Audio
{
    /// <summary>
    /// 表面材質検出システム
    /// より高度な表面材質検出を提供
    /// </summary>
    public class SurfaceTypeDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 0.5f;
        [SerializeField] private LayerMask detectionLayerMask = -1;
        [SerializeField] private float detectionOffset = 0.1f;
        
        [Header("Material Mapping")]
        [SerializeField] private MaterialSurfaceMapping[] materialMappings;
        [SerializeField] private TagSurfaceMapping[] tagMappings;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = false;
        
        // キャッシュされた情報
        private SurfaceMaterial cachedSurface = SurfaceMaterial.Default;
        private float lastDetectionTime;
        private const float DETECTION_CACHE_TIME = 0.1f;
        
        #region Public Interface
        
        /// <summary>
        /// 現在の表面材質を取得
        /// </summary>
        public SurfaceMaterial GetCurrentSurface()
        {
            // キャッシュ時間内であれば前回の結果を返す
            if (Time.time - lastDetectionTime < DETECTION_CACHE_TIME)
            {
                return cachedSurface;
            }
            
            cachedSurface = DetectSurface();
            lastDetectionTime = Time.time;
            
            return cachedSurface;
        }
        
        /// <summary>
        /// 指定位置での表面材質を検出
        /// </summary>
        public SurfaceMaterial GetSurfaceAtPosition(Vector3 position)
        {
            Vector3 previousPosition = transform.position;
            transform.position = position;
            
            SurfaceMaterial result = DetectSurface();
            
            transform.position = previousPosition;
            return result;
        }
        
        #endregion
        
        #region Detection Logic
        
        /// <summary>
        /// 表面材質の検出を実行
        /// </summary>
        private SurfaceMaterial DetectSurface()
        {
            Vector3 detectionPoint = transform.position - Vector3.up * detectionOffset;
            
            // Spherecast を使用してより確実な検出
            if (Physics.SphereCast(detectionPoint + Vector3.up * 2f, detectionRadius, 
                Vector3.down, out RaycastHit hit, 3f, detectionLayerMask))
            {
                return DetermineSurfaceFromHit(hit);
            }
            
            // フォールバック: 通常のRaycast
            if (Physics.Raycast(detectionPoint + Vector3.up * 1f, Vector3.down, 
                out hit, 2f, detectionLayerMask))
            {
                return DetermineSurfaceFromHit(hit);
            }
            
            return SurfaceMaterial.Default;
        }
        
        /// <summary>
        /// RaycastHitから表面材質を決定
        /// </summary>
        private SurfaceMaterial DetermineSurfaceFromHit(RaycastHit hit)
        {
            // 1. マテリアルベースの検出を試行
            SurfaceMaterial materialResult = GetSurfaceFromMaterial(hit);
            if (materialResult != SurfaceMaterial.Default)
            {
                return materialResult;
            }
            
            // 2. タグベースの検出を試行
            SurfaceMaterial tagResult = GetSurfaceFromTag(hit.collider.tag);
            if (tagResult != SurfaceMaterial.Default)
            {
                return tagResult;
            }
            
            // 3. オブジェクト名ベースの推測
            return GetSurfaceFromObjectName(hit.collider.name);
        }
        
        /// <summary>
        /// マテリアルから表面材質を決定
        /// </summary>
        private SurfaceMaterial GetSurfaceFromMaterial(RaycastHit hit)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer == null || renderer.material == null) return SurfaceMaterial.Default;
            
            Material hitMaterial = renderer.material;
            
            foreach (var mapping in materialMappings)
            {
                if (mapping.material == hitMaterial)
                {
                    return mapping.surfaceType;
                }
            }
            
            return SurfaceMaterial.Default;
        }
        
        /// <summary>
        /// タグから表面材質を決定
        /// </summary>
        private SurfaceMaterial GetSurfaceFromTag(string tag)
        {
            foreach (var mapping in tagMappings)
            {
                if (mapping.tag == tag)
                {
                    return mapping.surfaceType;
                }
            }
            
            return SurfaceMaterial.Default;
        }
        
        /// <summary>
        /// オブジェクト名から表面材質を推測
        /// </summary>
        private SurfaceMaterial GetSurfaceFromObjectName(string objectName)
        {
            string name = objectName.ToLower();
            
            if (name.Contains("concrete") || name.Contains("cement")) return SurfaceMaterial.Concrete;
            if (name.Contains("metal") || name.Contains("steel")) return SurfaceMaterial.Metal;
            if (name.Contains("wood") || name.Contains("plank")) return SurfaceMaterial.Wood;
            if (name.Contains("carpet") || name.Contains("rug")) return SurfaceMaterial.Carpet;
            if (name.Contains("grass") || name.Contains("lawn")) return SurfaceMaterial.Grass;
            if (name.Contains("gravel") || name.Contains("stone")) return SurfaceMaterial.Gravel;
            if (name.Contains("water") || name.Contains("pond")) return SurfaceMaterial.Water;
            
            return SurfaceMaterial.Default;
        }
        
        #endregion
        
        #region Editor Support
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;
            
            // 検出範囲の可視化
            Vector3 detectionPoint = transform.position - Vector3.up * detectionOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detectionPoint, detectionRadius);
            
            // 現在の表面材質の表示
            if (Application.isPlaying)
            {
                SurfaceMaterial currentSurface = GetCurrentSurface();
                Vector3 textPosition = transform.position + Vector3.up * 2f;
                
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(textPosition, $"Surface: {currentSurface}");
                #endif
            }
        }
        
        private void OnValidate()
        {
            if (detectionRadius <= 0f)
                detectionRadius = 0.1f;
        }
        #endif
        
        #endregion
    }
    
    #region Supporting Structures
    
    /// <summary>
    /// マテリアルと表面材質のマッピング
    /// </summary>
    [System.Serializable]
    public struct MaterialSurfaceMapping
    {
        public Material material;
        public SurfaceMaterial surfaceType;
        
        public MaterialSurfaceMapping(Material mat, SurfaceMaterial surface)
        {
            material = mat;
            surfaceType = surface;
        }
    }
    
    /// <summary>
    /// タグと表面材質のマッピング
    /// </summary>
    [System.Serializable]
    public struct TagSurfaceMapping
    {
        public string tag;
        public SurfaceMaterial surfaceType;
        
        public TagSurfaceMapping(string tagName, SurfaceMaterial surface)
        {
            tag = tagName;
            surfaceType = surface;
        }
    }
    
    #endregion
}