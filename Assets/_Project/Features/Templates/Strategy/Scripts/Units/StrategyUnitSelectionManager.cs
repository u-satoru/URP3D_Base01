using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.Strategy.Units
{
    /// <summary>
    /// ストラテジーゲーム用ユニット選択管理システム
    /// マルチ選択、ドラッグ選択、グループ選択を実装
    /// </summary>
    public class StrategyUnitSelectionManager : MonoBehaviour
    {
        [Header("Selection Settings")]
        [SerializeField] private LayerMask selectableLayerMask = 1;
        [SerializeField] private LayerMask groundLayerMask = 1;
        [SerializeField] private bool enableMultiSelection = true;
        [SerializeField] private bool enableDragSelection = true;
        [SerializeField] private float doubleClickTime = 0.3f;
        
        [Header("Selection Visual")]
        [SerializeField] private GameObject selectionBoxPrefab;
        [SerializeField] private Material selectionBoxMaterial;
        [SerializeField] private Color selectionColor = Color.green;
        [SerializeField] private Color hoverColor = Color.yellow;
        
        [Header("Events")]
        [SerializeField] private GameEvent onUnitSelected;
        [SerializeField] private GameEvent onUnitDeselected;
        [SerializeField] private GameEvent onSelectionChanged;
        [SerializeField] private GameEvent onMoveCommand;
        
        private List<ISelectableUnit> selectedUnits = new List<ISelectableUnit>();
        private ISelectableUnit hoveredUnit;
        private UnityEngine.Camera playerCamera;
        private PlayerInput playerInput;
        
        // Drag Selection
        private bool isDragging = false;
        private Vector3 dragStartScreenPos;
        private Vector3 dragEndScreenPos;
        private GameObject selectionBox;
        
        // Double Click Detection
        private float lastClickTime;
        private ISelectableUnit lastClickedUnit;
        
        // Properties
        public IReadOnlyList<ISelectableUnit> SelectedUnits => selectedUnits.AsReadOnly();
        public int SelectedUnitCount => selectedUnits.Count;
        public bool HasSelection => selectedUnits.Count > 0;
        
        private void Awake()
        {
            playerCamera = UnityEngine.Camera.main;
            if (playerCamera == null)
                playerCamera = FindFirstObjectByType<UnityEngine.Camera>();
                
            playerInput = GetComponent<PlayerInput>();
        }
        
        private void Start()
        {
            SetupInputActions();
            CreateSelectionBox();
        }

        /// <summary>
        /// 外部からの初期化
        /// </summary>
        public void Initialize()
        {
            SetupInputActions();
            CreateSelectionBox();
            Debug.Log("[StrategyUnitSelectionManager] External initialization completed");
        }

        private void Update()
        {
            HandleHover();
            HandleDragSelection();
        }
        
        /// <summary>
        /// 入力アクション設定
        /// </summary>
        private void SetupInputActions()
        {
            if (playerInput != null && playerInput.actions != null)
            {
                var selectAction = playerInput.actions["Select"];
                var moveAction = playerInput.actions["Move"];
                var deselectAction = playerInput.actions["Deselect"];
                
                if (selectAction != null)
                {
                    selectAction.performed += OnSelectPerformed;
                    selectAction.canceled += OnSelectCanceled;
                }
                
                if (moveAction != null)
                    moveAction.performed += OnMovePerformed;
                    
                if (deselectAction != null)
                    deselectAction.performed += OnDeselectPerformed;
            }
        }
        
        /// <summary>
        /// 選択ボックス作成
        /// </summary>
        private void CreateSelectionBox()
        {
            if (selectionBoxPrefab != null)
            {
                selectionBox = Instantiate(selectionBoxPrefab);
                selectionBox.SetActive(false);
            }
            else
            {
                // デフォルトの選択ボックスを作成
                selectionBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                selectionBox.name = "Selection Box";
                Destroy(selectionBox.GetComponent<Collider>());
                
                var renderer = selectionBox.GetComponent<Renderer>();
                if (renderer != null && selectionBoxMaterial != null)
                {
                    renderer.material = selectionBoxMaterial;
                }
                
                selectionBox.SetActive(false);
            }
        }
        
        /// <summary>
        /// ホバー処理
        /// </summary>
        private void HandleHover()
        {
            var hoveredObject = GetObjectUnderMouse();
            var selectableUnit = hoveredObject?.GetComponent<ISelectableUnit>();
            
            if (selectableUnit != hoveredUnit)
            {
                // 前回のホバーを解除
                if (hoveredUnit != null)
                {
                    hoveredUnit.SetHovered(false);
                }
                
                // 新しいホバーを設定
                hoveredUnit = selectableUnit;
                if (hoveredUnit != null)
                {
                    hoveredUnit.SetHovered(true);
                }
            }
        }
        
        /// <summary>
        /// ドラッグ選択処理
        /// </summary>
        private void HandleDragSelection()
        {
            if (!enableDragSelection || !isDragging) return;
            
            dragEndScreenPos = Input.mousePosition;
            
            // 選択ボックスの更新
            UpdateSelectionBox();
            
            // ドラッグ範囲内のユニットを選択
            var unitsInDragArea = GetUnitsInScreenRect();
            PreviewSelection(unitsInDragArea);
        }
        
        /// <summary>
        /// マウス下のオブジェクト取得
        /// </summary>
        private GameObject GetObjectUnderMouse()
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayerMask))
            {
                return hit.collider.gameObject;
            }
            return null;
        }
        
        /// <summary>
        /// 画面矩形内のユニット取得
        /// </summary>
        private List<ISelectableUnit> GetUnitsInScreenRect()
        {
            var unitsInRect = new List<ISelectableUnit>();
            
            // スクリーン座標での矩形を計算
            var screenRect = GetScreenRect(dragStartScreenPos, dragEndScreenPos);
            
            // すべての選択可能ユニットをチェック
            var allUnits = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISelectableUnit>();
            foreach (var unit in allUnits)
            {
                var unitGameObject = (unit as MonoBehaviour)?.gameObject;
                if (unitGameObject != null)
                {
                    Vector3 screenPos = playerCamera.WorldToScreenPoint(unitGameObject.transform.position);
                    if (screenRect.Contains(screenPos))
                    {
                        unitsInRect.Add(unit);
                    }
                }
            }
            
            return unitsInRect;
        }
        
        /// <summary>
        /// スクリーン矩形計算
        /// </summary>
        private Rect GetScreenRect(Vector3 screenPos1, Vector3 screenPos2)
        {
            screenPos1.y = Screen.height - screenPos1.y;
            screenPos2.y = Screen.height - screenPos2.y;
            
            var topLeft = Vector3.Min(screenPos1, screenPos2);
            var bottomRight = Vector3.Max(screenPos1, screenPos2);
            
            return new Rect(topLeft.x, topLeft.y, bottomRight.x - topLeft.x, bottomRight.y - topLeft.y);
        }
        
        /// <summary>
        /// 選択ボックス更新
        /// </summary>
        private void UpdateSelectionBox()
        {
            if (selectionBox == null) return;
            
            var screenRect = GetScreenRect(dragStartScreenPos, dragEndScreenPos);
            
            // ワールド座標での位置とサイズを計算
            Vector3 center = playerCamera.ScreenToWorldPoint(new Vector3(screenRect.center.x, screenRect.center.y, 10f));
            Vector3 size = new Vector3(screenRect.width / 100f, 0.1f, screenRect.height / 100f);
            
            selectionBox.transform.position = center;
            selectionBox.transform.localScale = size;
            selectionBox.SetActive(true);
        }
        
        /// <summary>
        /// 選択プレビュー
        /// </summary>
        private void PreviewSelection(List<ISelectableUnit> units)
        {
            // 全ユニットのプレビューを一旦解除
            foreach (var unit in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISelectableUnit>())
            {
                unit.SetPreviewSelection(false);
            }
            
            // プレビュー対象のユニットを設定
            foreach (var unit in units)
            {
                unit.SetPreviewSelection(true);
            }
        }
        
        /// <summary>
        /// ユニット選択
        /// </summary>
        public void SelectUnit(ISelectableUnit unit, bool addToSelection = false)
        {
            if (unit == null) return;
            
            if (!addToSelection)
            {
                ClearSelection();
            }
            
            if (!selectedUnits.Contains(unit))
            {
                selectedUnits.Add(unit);
                unit.SetSelected(true);
                onUnitSelected?.Raise();
                onSelectionChanged?.Raise();
            }
        }
        
        /// <summary>
        /// ユニット選択解除
        /// </summary>
        public void DeselectUnit(ISelectableUnit unit)
        {
            if (unit == null || !selectedUnits.Contains(unit)) return;
            
            selectedUnits.Remove(unit);
            unit.SetSelected(false);
            onUnitDeselected?.Raise();
            onSelectionChanged?.Raise();
        }
        
        /// <summary>
        /// すべて選択解除
        /// </summary>
        public void ClearSelection()
        {
            foreach (var unit in selectedUnits)
            {
                unit.SetSelected(false);
            }
            
            selectedUnits.Clear();
            onSelectionChanged?.Raise();
        }
        
        /// <summary>
        /// 複数ユニット選択
        /// </summary>
        public void SelectUnits(IEnumerable<ISelectableUnit> units, bool addToSelection = false)
        {
            if (!addToSelection)
            {
                ClearSelection();
            }
            
            foreach (var unit in units)
            {
                if (unit != null && !selectedUnits.Contains(unit))
                {
                    selectedUnits.Add(unit);
                    unit.SetSelected(true);
                }
            }
            
            if (units.Any())
            {
                onSelectionChanged?.Raise();
            }
        }
        
        /// <summary>
        /// 移動命令
        /// </summary>
        public void CommandMoveToPosition(Vector3 worldPosition)
        {
            if (selectedUnits.Count == 0) return;
            
            foreach (var unit in selectedUnits)
            {
                unit.MoveTo(worldPosition);
            }
            
            onMoveCommand?.Raise();
        }
        
        /// <summary>
        /// 同種ユニット全選択
        /// </summary>
        public void SelectAllUnitsOfType(System.Type unitType)
        {
            var unitsOfType = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Where(obj => obj.GetType() == unitType || obj.GetType().IsSubclassOf(unitType))
                .OfType<ISelectableUnit>();
                
            SelectUnits(unitsOfType);
        }
        
        // Input Callbacks
        private void OnSelectPerformed(InputAction.CallbackContext context)
        {
            if (enableDragSelection)
            {
                isDragging = true;
                dragStartScreenPos = Input.mousePosition;
            }
            
            var hitObject = GetObjectUnderMouse();
            if (hitObject != null)
            {
                var selectableUnit = hitObject.GetComponent<ISelectableUnit>();
                if (selectableUnit != null)
                {
                    bool isDoubleClick = Time.time - lastClickTime < doubleClickTime && selectableUnit == lastClickedUnit;
                    bool addToSelection = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                    
                    if (isDoubleClick)
                    {
                        // ダブルクリック：同種ユニット全選択
                        SelectAllUnitsOfType(selectableUnit.GetType());
                    }
                    else
                    {
                        SelectUnit(selectableUnit, addToSelection);
                    }
                    
                    lastClickTime = Time.time;
                    lastClickedUnit = selectableUnit;
                }
                else if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                {
                    ClearSelection();
                }
            }
        }
        
        private void OnSelectCanceled(InputAction.CallbackContext context)
        {
            if (isDragging)
            {
                isDragging = false;
                
                // ドラッグ選択完了
                var unitsInDragArea = GetUnitsInScreenRect();
                bool addToSelection = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                
                if (unitsInDragArea.Count > 0)
                {
                    SelectUnits(unitsInDragArea, addToSelection);
                }
                
                // 選択ボックスを非表示
                if (selectionBox != null)
                    selectionBox.SetActive(false);
                    
                // プレビューを解除
                PreviewSelection(new List<ISelectableUnit>());
            }
        }
        
        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            if (selectedUnits.Count == 0) return;
            
            // 地面への移動命令
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
            {
                CommandMoveToPosition(hit.point);
            }
        }
        
        private void OnDeselectPerformed(InputAction.CallbackContext context)
        {
            ClearSelection();
        }
        
        private void OnDestroy()
        {
            if (playerInput != null && playerInput.actions != null)
            {
                var selectAction = playerInput.actions["Select"];
                var moveAction = playerInput.actions["Move"];
                var deselectAction = playerInput.actions["Deselect"];
                
                if (selectAction != null)
                {
                    selectAction.performed -= OnSelectPerformed;
                    selectAction.canceled -= OnSelectCanceled;
                }
                
                if (moveAction != null)
                    moveAction.performed -= OnMovePerformed;
                    
                if (deselectAction != null)
                    deselectAction.performed -= OnDeselectPerformed;
            }
            
            if (selectionBox != null)
                Destroy(selectionBox);
        }
    }
    
    /// <summary>
    /// 選択可能ユニットのインターフェース
    /// </summary>
    public interface ISelectableUnit
    {
        void SetSelected(bool selected);
        void SetHovered(bool hovered);
        void SetPreviewSelection(bool preview);
        void MoveTo(Vector3 position);
    }
}