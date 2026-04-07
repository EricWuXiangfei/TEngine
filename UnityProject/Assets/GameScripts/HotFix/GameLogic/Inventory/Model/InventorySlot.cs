using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 背包物品格，记录物品 ID、当前数量及最大堆叠数。
    /// </summary>
    public class InventorySlot
    {
        /// <summary>物品 ID。</summary>
        public int ItemId { get; }

        /// <summary>当前数量。</summary>
        public int Count { get; private set; }

        /// <summary>最大堆叠数。</summary>
        public int MaxStack { get; }

        public InventorySlot(int itemId, int count, int maxStack)
        {
            ItemId   = itemId;
            MaxStack = maxStack > 0 ? maxStack : 1;
            Count    = UnityEngine.Mathf.Clamp(count, 0, MaxStack);
        }

        /// <summary>
        /// 增加数量，返回溢出数量（无法放入的部分）。
        /// </summary>
        public int Add(int amount)
        {
            if (amount <= 0) return 0;
            int available = MaxStack - Count;
            int added     = UnityEngine.Mathf.Min(amount, available);
            Count += added;
            int overflow = amount - added;
            if (overflow > 0)
            {
                Log.Warning($"[InventorySlot] ItemId={ItemId} 堆叠已满，溢出 {overflow} 个。");
            }
            return overflow;
        }

        /// <summary>
        /// 减少数量，数量不足时返回 false。
        /// </summary>
        public bool Remove(int amount)
        {
            if (amount <= 0 || Count < amount) return false;
            Count -= amount;
            return true;
        }
    }
}
