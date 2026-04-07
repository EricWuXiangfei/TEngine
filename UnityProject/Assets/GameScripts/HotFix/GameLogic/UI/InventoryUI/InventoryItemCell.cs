using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 背包物品格 Widget，显示物品图标和数量。
    /// 支持格子间拖动交换（IBeginDragHandler / IDragHandler / IEndDragHandler / IDropHandler）。
    /// 绑定到预制体中的 ItemCellPrefab 节点。
    /// </summary>
    public class InventoryItemCell : UIWidget,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        private Image  _imgIcon;
        private Text   _txtCount;
        private Button _btnCell;

        private ItemData _data;
        private int      _slotIndex = -1;
        private System.Action<ItemData> _onClickCallback;

        // 由 UIInventoryPanel 在 BeginDrag 时注入，用于创建 DragGhost
        private System.Action<InventoryItemCell> _onBeginDragCallback;
        // 由 UIInventoryPanel 在 EndDrag 时注入
        private System.Action<InventoryItemCell> _onEndDragCallback;

        #region 脚本工具生成的代码

        protected override void ScriptGenerator()
        {
            _imgIcon  = FindChildComponent<Image>("m_img_Icon");
            _txtCount = FindChildComponent<Text>("m_txt_Count");
            _btnCell  = FindChildComponent<Button>("m_btn_Cell");

            if (_btnCell != null)
            {
                _btnCell.onClick.AddListener(OnCellClicked);
            }
        }

        #endregion

        /// <summary>
        /// 设置物品数据并刷新显示。
        /// </summary>
        /// <param name="slotIndex">该格在背包中的槽位索引（0-based）</param>
        /// <param name="data">物品数据快照</param>
        /// <param name="onClickCallback">点击回调</param>
        /// <param name="onBeginDrag">拖动开始回调（由面板注入）</param>
        /// <param name="onEndDrag">拖动结束回调（由面板注入）</param>
        public void SetData(int slotIndex, ItemData data,
            System.Action<ItemData>          onClickCallback = null,
            System.Action<InventoryItemCell> onBeginDrag     = null,
            System.Action<InventoryItemCell> onEndDrag       = null)
        {
            _slotIndex           = slotIndex;
            _data                = data;
            _onClickCallback     = onClickCallback;
            _onBeginDragCallback = onBeginDrag;
            _onEndDragCallback   = onEndDrag;
            Refresh();
        }

        /// <summary>格子的槽位索引（0-based）。</summary>
        public int SlotIndex => _slotIndex;

        /// <summary>当前持有的物品数据。</summary>
        public ItemData Data => _data;

        /// <summary>格子图标图片组件（用于 DragGhost 复制图标）。</summary>
        public Sprite IconSprite => _imgIcon != null ? _imgIcon.sprite : null;

        /// <summary>
        /// 控制图标是否可见（拖动中源格隐藏图标）。
        /// </summary>
        public void SetIconVisible(bool visible)
        {
            if (_imgIcon != null) _imgIcon.enabled = visible;
        }

        private void Refresh()
        {
            bool isEmpty = _data.IsEmpty;

            // 图标
            if (_imgIcon != null)
            {
                _imgIcon.enabled = !isEmpty;
                // 配置表生成后取消注释：
                // if (!isEmpty)
                // {
                //     var cfg = ConfigSystem.Instance.Tables.TbItemConfig.GetOrDefault(_data.ItemId);
                //     if (cfg != null) _imgIcon.SetSprite(cfg.IconPath);
                // }
            }

            // 数量：仅非空且数量 > 1 时显示
            if (_txtCount != null)
            {
                _txtCount.gameObject.SetActive(!isEmpty && _data.Count > 1);
                _txtCount.text = _data.Count.ToString();
            }
        }

        #region 点击事件

        private void OnCellClicked()
        {
            if (!_data.IsEmpty)
                _onClickCallback?.Invoke(_data);
        }

        #endregion

        #region 拖动接口

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_data.IsEmpty) return;
            _onBeginDragCallback?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // 拖动位移由 UIInventoryPanel.OnUpdate 驱动 DragGhost，此处不处理
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _onEndDragCallback?.Invoke(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            // 接受拖放：由 UIInventoryPanel 通过当前正在拖动的源格索引来调用 SwapItems
            // 这里通知面板：有物品被拖入本格
            UIInventoryPanel.HandleDrop(this);
        }

        #endregion
    }
}
