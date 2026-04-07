using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 背包内「创建物品」弹出面板。
    /// 玩家在此输入/选择物品 ID 和数量，确认后直接写入背包。
    ///
    /// 通过 GameModule.UI.ShowUIAsync&lt;UIItemPickerPanel&gt;() 打开。
    /// 预制体资源命名：UIItemPickerPanel
    ///
    /// 预制体节点规范：
    ///   UIItemPickerPanel
    ///     ├─ m_inp_ItemId      InputField（输入物品 ID）
    ///     ├─ m_inp_Count       InputField（输入数量）
    ///     ├─ m_btn_Confirm     确认按钮
    ///     ├─ m_btn_Cancel      取消 / 关闭按钮
    ///     └─ m_txt_Error       错误提示 Text（默认隐藏）
    /// </summary>
    [Window(UILayer.UI, location: "UIItemPickerPanel")]
    public class UIItemPickerPanel : UIWindow
    {
        #region 节点绑定

        private InputField _inpItemId;
        private InputField _inpCount;
        private Button     _btnConfirm;
        private Button     _btnCancel;
        private Text       _txtError;

        protected override void ScriptGenerator()
        {
            _inpItemId  = FindChildComponent<InputField>("m_inp_ItemId");
            _inpCount   = FindChildComponent<InputField>("m_inp_Count");
            _btnConfirm = FindChildComponent<Button>("m_btn_Confirm");
            _btnCancel  = FindChildComponent<Button>("m_btn_Cancel");
            _txtError   = FindChildComponent<Text>("m_txt_Error");

            if (_btnConfirm != null) _btnConfirm.onClick.AddListener(OnConfirmClicked);
            if (_btnCancel  != null) _btnCancel.onClick.AddListener(OnCancelClicked);
        }

        #endregion

        #region 生命周期

        protected override void OnCreate()
        {
            // 默认数量填 1
            if (_inpCount != null)
                _inpCount.text = "1";

            SetError(string.Empty);
        }

        #endregion

        #region 按钮回调

        private void OnConfirmClicked()
        {
            // 解析 ItemId
            if (_inpItemId == null || !int.TryParse(_inpItemId.text, out int itemId) || itemId <= 0)
            {
                SetError("请输入有效的物品 ID（正整数）");
                return;
            }

            // 解析 Count
            if (_inpCount == null || !int.TryParse(_inpCount.text, out int count) || count <= 0)
            {
                SetError("请输入有效的数量（正整数）");
                return;
            }

            // 尝试添加到背包
            bool success = InventoryManager.Instance.AddItem(itemId, count);
            if (!success)
            {
                SetError("背包已满，无法添加物品！");
                return;
            }

            Log.Info($"[UIItemPickerPanel] 成功添加物品：itemId={itemId}, count={count}");
            Close();
        }

        private void OnCancelClicked()
        {
            Close();
        }

        #endregion

        #region 私有辅助

        private void Close()
        {
            GameModule.UI.CloseUI<UIItemPickerPanel>();
        }

        private void SetError(string message)
        {
            if (_txtError == null) return;
            _txtError.gameObject.SetActive(!string.IsNullOrEmpty(message));
            _txtError.text = message;
        }

        #endregion
    }
}
