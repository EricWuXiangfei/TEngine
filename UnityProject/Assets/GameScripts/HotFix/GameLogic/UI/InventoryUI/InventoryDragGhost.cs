using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 拖动浮层 Widget，跟随指针移动，显示被拖物品的图标。
    /// 由 UIInventoryPanel 在拖动开始时创建，拖动结束时销毁。
    ///
    /// 预制体节点结构（挂在 UIInventoryPanel Canvas 最顶层）：
    ///   DragGhost
    ///     └─ m_img_Icon (Image)
    /// </summary>
    public class InventoryDragGhost : UIWidget
    {
        private Image          _imgIcon;
        private RectTransform  _rectSelf;
        private Canvas         _rootCanvas;

        #region 脚本工具生成的代码

        protected override void ScriptGenerator()
        {
            _imgIcon  = FindChildComponent<Image>("m_img_Icon");
            _rectSelf = rectTransform;
        }

        #endregion

        /// <summary>
        /// 初始化浮层，设置图标并绑定所属 Canvas（用于坐标转换）。
        /// </summary>
        public void Setup(Sprite icon, Canvas rootCanvas)
        {
            _rootCanvas = rootCanvas;
            if (_imgIcon != null && icon != null)
            {
                _imgIcon.sprite  = icon;
                _imgIcon.enabled = true;
            }
        }

        /// <summary>
        /// 每帧跟随屏幕指针位置（由 UIInventoryPanel.OnUpdate 调用）。
        /// </summary>
        public void FollowPointer()
        {
            if (_rectSelf == null || _rootCanvas == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rootCanvas.transform as RectTransform,
                UnityEngine.Input.mousePosition,
                _rootCanvas.worldCamera,
                out Vector2 localPoint);

            _rectSelf.localPosition = localPoint;
        }
    }
}
