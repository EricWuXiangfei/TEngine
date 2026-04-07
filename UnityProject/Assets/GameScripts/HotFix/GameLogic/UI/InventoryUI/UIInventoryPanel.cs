using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 背包 UI 面板。
    /// 通过 GameModule.UI.ShowUIAsync&lt;UIInventoryPanel&gt;() 打开。
    /// 预制体资源命名：UIInventoryPanel
    ///
    /// 预制体节点规范：
    ///   UIInventoryPanel
    ///     ├─ m_go_Empty          空背包提示
    ///     ├─ m_tf_ItemContent    格子容器（挂 GridLayoutGroup, CellSize=80）
    ///     ├─ m_btn_Close         关闭按钮
    ///     ├─ m_btn_AddItem       新增物品按钮（打开 UIItemPickerPanel）
    ///     └─ DragGhost           拖动浮层（InventoryDragGhost，默认隐藏）
    /// </summary>
    [Window(UILayer.UI, location: "UIInventoryPanel")]
    public class UIInventoryPanel : UIWindow
    {
        // ─── 静态拖放通信 ──────────────────────────────────────────
        // InventoryItemCell.OnDrop 调用此静态方法将事件路由回当前面板实例

        private static UIInventoryPanel _instance;

        /// <summary>
        /// 由 InventoryItemCell.OnDrop 调用，将拖放目标格通知给面板。
        /// </summary>
        public static void HandleDrop(InventoryItemCell targetCell)
        {
            _instance?.OnDropToCell(targetCell);
        }

        // ─── 节点绑定 ─────────────────────────────────────────────

        private GameObject _goEmpty;
        private Transform _tfItemContent;
        private Button _btnClose;
        private Button _btnAddItem;
        private Canvas _rootCanvas;

        // 拖动浮层（DragGhost 子 Widget）
        private InventoryDragGhost _dragGhost;

        // 格子 Widget 列表，索引与 SlotCapacity 对应
        private readonly List<InventoryItemCell> _cellWidgets = new List<InventoryItemCell>();

        // 当前正在拖动的源格子
        private InventoryItemCell _draggingCell;

        #region 脚本工具生成的代码

        protected override void ScriptGenerator()
        {
            _goEmpty = FindChild("m_go_Empty")?.gameObject;
            _tfItemContent = FindChild("m_tf_ItemContent");
            _btnClose = FindChildComponent<Button>("m_btn_Close");
            _btnAddItem = FindChildComponent<Button>("m_btn_AddItem");
            _rootCanvas = transform.GetComponent<Canvas>();

            if (_btnClose != null)
                _btnClose.onClick.AddListener(OnCloseClicked);

            if (_btnAddItem != null)
                _btnAddItem.onClick.AddListener(OnAddItemClicked);
        }

        #endregion

        #region 事件注册

        protected override void RegisterEvent()
        {
            // Source Generator 生成后取消注释以下三行：
            // AddUIEvent<int, int>(IInventoryEvent_Event.OnItemAdded,   OnItemChanged);
            // AddUIEvent<int, int>(IInventoryEvent_Event.OnItemRemoved, OnItemChanged);
            // AddUIEvent(IInventoryEvent_Event.OnInventoryCleared,      OnInventoryCleared);
            // AddUIEvent<int>(IInventoryEvent_Event.OnSlotExpanded,     OnSlotExpanded);
            Log.Warning("[UIInventoryPanel] IInventoryEvent 事件尚未注册，等待 Source Generator 生成后启用。");
        }

        #endregion

        #region 生命周期

        protected override void OnCreate()
        {
            _instance = this;
            RebuildGrid();
        }

        protected override void OnRefresh()
        {
            RebuildGrid();
        }

        protected override void OnUpdate()
        {
            // 拖动中每帧驱动 DragGhost 跟随指针
            if (_draggingCell != null && _dragGhost != null)
            {
                _dragGhost.FollowPointer();
            }
        }

        protected override void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        #endregion

        #region 格子网格管理

        /// <summary>
        /// 根据 SlotCapacity 重建格子列表，并按 GetAllSlots() 填充数据。
        /// </summary>
        private void RebuildGrid()
        {
            var mgr = InventoryManager.Instance;
            var allSlots = mgr.GetAllSlots();   // 长度 = SlotCapacity，空格为 ItemData.Empty
            int capacity = mgr.SlotCapacity;

            // 空背包提示（全空格时显示）
            if (_goEmpty != null)
                _goEmpty.SetActive(mgr.GetAllItems().Count == 0);

            // 扩容到 capacity
            while (_cellWidgets.Count < capacity)
            {
                // 通过 CreateWidget<T> 在 _tfItemContent 下实例化格子 Widget
                // 预制体路径与 UIScriptGenerator 约定一致：Assets/.../ItemCellPrefab
                // 此处留空：等预制体制作后替换为：
                //   var cell = CreateWidget<InventoryItemCell>(_tfItemContent, "ItemCellPrefab");
                //   _cellWidgets.Add(cell);
                // 目前占位 null，不影响编译
                _cellWidgets.Add(null);
            }

            // 超出容量的格子隐藏
            for (int i = 0; i < _cellWidgets.Count; i++)
            {
                var cell = _cellWidgets[i];
                if (cell == null) continue;

                if (i < capacity)
                {
                    cell.Visible = true;
                    cell.SetData(i, allSlots[i],
                        onClickCallback: OnItemCellClicked,
                        onBeginDrag: OnCellBeginDrag,
                        onEndDrag: OnCellEndDrag);
                }
                else
                {
                    cell.Visible = false;
                }
            }
        }

        #endregion

        #region 拖动逻辑

        private void OnCellBeginDrag(InventoryItemCell sourceCell)
        {
            _draggingCell = sourceCell;

            // 隐藏源格图标
            sourceCell.SetIconVisible(false);

            // 显示 DragGhost
            if (_dragGhost != null)
            {
                _dragGhost.Setup(sourceCell.IconSprite, _rootCanvas);
                _dragGhost.Visible = true;
            }
        }

        private void OnCellEndDrag(InventoryItemCell sourceCell)
        {
            // 恢复源格图标（如果未发生有效 Drop，图标应恢复显示）
            sourceCell.SetIconVisible(true);

            // 隐藏 DragGhost
            if (_dragGhost != null)
                _dragGhost.Visible = false;

            _draggingCell = null;
        }

        /// <summary>
        /// 由 InventoryItemCell.OnDrop 通过静态 HandleDrop 调用。
        /// </summary>
        private void OnDropToCell(InventoryItemCell targetCell)
        {
            if (_draggingCell == null) return;
            if (_draggingCell == targetCell) return;

            int srcIdx = _draggingCell.SlotIndex;
            int dstIdx = targetCell.SlotIndex;

            if (InventoryManager.Instance.SwapItems(srcIdx, dstIdx))
            {
                // 交换成功：刷新两个格子的数据
                RefreshCell(_draggingCell, dstIdx);  // 源格现在显示目标格原物品
                RefreshCell(targetCell, srcIdx);   // 目标格现在显示源格原物品
            }
        }

        /// <summary>
        /// 刷新单个格子的显示（交换后局部更新，避免全量重建）。
        /// </summary>
        private void RefreshCell(InventoryItemCell cell, int newSlotIndex)
        {
            var data = InventoryManager.Instance.GetItemAtSlot(newSlotIndex);
            cell.SetData(newSlotIndex, data,
                onClickCallback: OnItemCellClicked,
                onBeginDrag: OnCellBeginDrag,
                onEndDrag: OnCellEndDrag);
        }

        #endregion

        #region 事件回调

        private void OnCloseClicked()
        {
            GameModule.UI.CloseUI<UIInventoryPanel>();
        }

        /// <summary>
        /// 点击「新增物品」按钮，弹出物品选择弹窗。
        /// </summary>
        private void OnAddItemClicked()
        {
            GameModule.UI.ShowUIAsync<UIItemPickerPanel>();
        }

        private void OnItemCellClicked(ItemData data)
        {
            Log.Info($"[UIInventoryPanel] 点击物品：ItemId={data.ItemId}, Count={data.Count}");
            // TODO: 弹出物品详情面板，传入 data.ItemId
        }

        private void OnItemChanged(int itemId, int count)
        {
            RebuildGrid();
        }

        private void OnInventoryCleared()
        {
            RebuildGrid();
        }

        private void OnSlotExpanded(int newCapacity)
        {
            // 容量扩展后重建格子
            RebuildGrid();
        }

        #endregion
    }
}
