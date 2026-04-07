using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 背包事件接口，通过 Source Generator 自动生成事件 ID 和 Wrap。
    /// </summary>
    [EventInterface(EEventGroup.GroupLogic)]
    public interface IInventoryEvent
    {
        /// <summary>物品被添加时触发。</summary>
        void OnItemAdded(int itemId, int count);

        /// <summary>物品被移除时触发。</summary>
        void OnItemRemoved(int itemId, int count);

        /// <summary>背包被清空时触发。</summary>
        void OnInventoryCleared();

        /// <summary>两个格子物品交换后触发。</summary>
        void OnItemSwapped(int srcSlotIndex, int dstSlotIndex);

        /// <summary>背包格子数量扩展后触发。</summary>
        void OnSlotExpanded(int newCapacity);

        /// <summary>背包已满，无法添加物品时触发。</summary>
        void OnInventoryFull(int itemId, int count);
    }
}
