using System.Collections.Generic;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 背包管理器，提供物品的增删查消耗、格子扩展、格子间拖动交换等标准接口。
    /// 继承 Singleton&lt;T&gt;，由 SingletonSystem 统一管理生命周期。
    ///
    /// 存储结构：
    ///   _slots 是固定大小的有序列表（长度 = MaxSlotCount），null 表示空槽。
    ///   SlotCapacity 标记当前已解锁的格子数，超出部分为锁定格（不可放物品）。
    /// </summary>
    public class InventoryManager : Singleton<InventoryManager>
    {
        private const int DefaultMaxStack    = 99;
        private const int FallbackBaseSlots  = 24;
        private const int FallbackExpandStep = 6;
        private const int FallbackMaxSlots   = 60;

        // ─── 格子容量 ────────────────────────────────────────────

        /// <summary>当前已解锁的格子数（可存放物品的上限）。</summary>
        public int SlotCapacity { get; private set; }

        /// <summary>背包格子总上限（含锁定格）。</summary>
        public int MaxSlotCount { get; private set; }

        /// <summary>每次扩展增加的格子数。</summary>
        public int ExpandStep { get; private set; }

        // ─── 存储结构 ────────────────────────────────────────────

        // 有序槽位列表，长度 = MaxSlotCount，null 表示空槽。
        private List<InventorySlot> _slots;

        // ─── 初始化 ──────────────────────────────────────────────

        protected override void OnInit()
        {
            // 从 Luban 配置读取容量参数；配置表生成前使用默认值。
            // 配置生成后替换以下注释块：
            // var cfg = ConfigSystem.Instance.Tables.TbInventoryConfig;
            // SlotCapacity = cfg?.BaseSlotCount ?? FallbackBaseSlots;
            // ExpandStep   = cfg?.ExpandStep   ?? FallbackExpandStep;
            // MaxSlotCount = cfg?.MaxSlotCount  ?? FallbackMaxSlots;
            SlotCapacity = FallbackBaseSlots;
            ExpandStep   = FallbackExpandStep;
            MaxSlotCount = FallbackMaxSlots;

            _slots = new List<InventorySlot>(MaxSlotCount);
            for (int i = 0; i < MaxSlotCount; i++)
            {
                _slots.Add(null);
            }
        }

        // ─── 容量管理 ────────────────────────────────────────────

        /// <summary>
        /// 扩展背包格子数量（每次增加 ExpandStep）。
        /// 超出 MaxSlotCount 则不再扩展。
        /// </summary>
        /// <returns>扩展后的 SlotCapacity；若已达上限则返回当前值。</returns>
        public int ExpandSlots()
        {
            if (SlotCapacity >= MaxSlotCount)
            {
                Log.Warning("[InventoryManager] 背包已达最大格子数，无法继续扩展。");
                return SlotCapacity;
            }

            SlotCapacity = UnityEngine.Mathf.Min(SlotCapacity + ExpandStep, MaxSlotCount);
            GameEvent.Get<IInventoryEvent>().OnSlotExpanded(SlotCapacity);
            return SlotCapacity;
        }

        /// <summary>
        /// 设置已解锁格子数（由存档系统调用以恢复持久化数据）。
        /// </summary>
        public void SetSlotCapacity(int capacity)
        {
            SlotCapacity = UnityEngine.Mathf.Clamp(capacity, FallbackBaseSlots, MaxSlotCount);
        }

        // ─── 物品操作 ────────────────────────────────────────────

        /// <summary>
        /// 向背包中添加物品。背包已满时触发 OnInventoryFull 并返回 false。
        /// </summary>
        public bool AddItem(int itemId, int count)
        {
            if (count <= 0) return false;

            int maxStack = GetMaxStack(itemId);

            // 先尝试叠加到已有格子
            for (int i = 0; i < SlotCapacity; i++)
            {
                var slot = _slots[i];
                if (slot != null && slot.ItemId == itemId && slot.Count < slot.MaxStack)
                {
                    int overflow = slot.Add(count);
                    GameEvent.Get<IInventoryEvent>().OnItemAdded(itemId, count - overflow);
                    if (overflow <= 0) return true;
                    // 有溢出，继续尝试放入其他格
                    count = overflow;
                }
            }

            // 放入空格
            for (int i = 0; i < SlotCapacity; i++)
            {
                if (_slots[i] == null)
                {
                    _slots[i] = new InventorySlot(itemId, count, maxStack);
                    GameEvent.Get<IInventoryEvent>().OnItemAdded(itemId, count);
                    return true;
                }
            }

            // 背包已满
            Log.Warning($"[InventoryManager] 背包已满，无法添加 itemId={itemId} x{count}");
            GameEvent.Get<IInventoryEvent>().OnInventoryFull(itemId, count);
            return false;
        }

        /// <summary>
        /// 从背包中移除物品（按 itemId 跨格扣减）。数量不足时返回 false。
        /// </summary>
        public bool RemoveItem(int itemId, int count)
        {
            if (count <= 0) return false;

            // 先检查总量是否足够
            if (GetCount(itemId) < count) return false;

            int remaining = count;
            for (int i = 0; i < SlotCapacity && remaining > 0; i++)
            {
                var slot = _slots[i];
                if (slot == null || slot.ItemId != itemId) continue;

                int toRemove = UnityEngine.Mathf.Min(slot.Count, remaining);
                slot.Remove(toRemove);
                remaining -= toRemove;

                if (slot.Count == 0) _slots[i] = null;
            }

            GameEvent.Get<IInventoryEvent>().OnItemRemoved(itemId, count);
            return true;
        }

        /// <summary>
        /// 获取物品当前持有总量。
        /// </summary>
        public int GetCount(int itemId)
        {
            int total = 0;
            for (int i = 0; i < SlotCapacity; i++)
            {
                if (_slots[i] != null && _slots[i].ItemId == itemId)
                    total += _slots[i].Count;
            }
            return total;
        }

        /// <summary>
        /// 判断是否持有足够数量的物品。
        /// </summary>
        public bool HasItem(int itemId, int count = 1)
        {
            return GetCount(itemId) >= count;
        }

        /// <summary>
        /// 消耗物品：数量足够则移除并返回 true，否则返回 false。
        /// </summary>
        public bool ConsumeItem(int itemId, int count)
        {
            if (!HasItem(itemId, count)) return false;
            return RemoveItem(itemId, count);
        }

        /// <summary>
        /// 获取指定槽位的物品快照（slotIndex 从 0 开始）。槽位为空返回 ItemData.Empty。
        /// </summary>
        public ItemData GetItemAtSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlotCount) return ItemData.Empty;
            var slot = _slots[slotIndex];
            return slot != null ? new ItemData(slot.ItemId, slot.Count) : ItemData.Empty;
        }

        /// <summary>
        /// 获取指定 itemId 第一个匹配槽的快照。不存在返回 ItemData.Empty。
        /// </summary>
        public ItemData GetItem(int itemId)
        {
            for (int i = 0; i < SlotCapacity; i++)
            {
                if (_slots[i] != null && _slots[i].ItemId == itemId)
                    return new ItemData(_slots[i].ItemId, _slots[i].Count);
            }
            return ItemData.Empty;
        }

        /// <summary>
        /// 获取所有槽位快照列表（长度 = SlotCapacity，空槽为 ItemData.Empty）。
        /// </summary>
        public IReadOnlyList<ItemData> GetAllSlots()
        {
            var list = new List<ItemData>(SlotCapacity);
            for (int i = 0; i < SlotCapacity; i++)
            {
                var slot = _slots[i];
                list.Add(slot != null ? new ItemData(slot.ItemId, slot.Count) : ItemData.Empty);
            }
            return list;
        }

        /// <summary>
        /// 获取背包中所有非空物品的快照列表（不含空格）。
        /// </summary>
        public IReadOnlyList<ItemData> GetAllItems()
        {
            var list = new List<ItemData>();
            for (int i = 0; i < SlotCapacity; i++)
            {
                if (_slots[i] != null)
                    list.Add(new ItemData(_slots[i].ItemId, _slots[i].Count));
            }
            return list;
        }

        /// <summary>
        /// 交换两个槽位的物品（支持空槽，等同移动）。
        /// srcSlotIndex 和 dstSlotIndex 必须在 [0, SlotCapacity) 内。
        /// </summary>
        public bool SwapItems(int srcSlotIndex, int dstSlotIndex)
        {
            if (srcSlotIndex == dstSlotIndex) return false;
            if (srcSlotIndex < 0 || srcSlotIndex >= SlotCapacity) return false;
            if (dstSlotIndex < 0 || dstSlotIndex >= SlotCapacity) return false;

            (_slots[srcSlotIndex], _slots[dstSlotIndex]) = (_slots[dstSlotIndex], _slots[srcSlotIndex]);

            GameEvent.Get<IInventoryEvent>().OnItemSwapped(srcSlotIndex, dstSlotIndex);
            return true;
        }

        /// <summary>
        /// 清空背包中所有物品并广播 OnInventoryCleared。
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < MaxSlotCount; i++) _slots[i] = null;
            GameEvent.Get<IInventoryEvent>().OnInventoryCleared();
        }

        // ─── 私有辅助 ────────────────────────────────────────────

        private int GetMaxStack(int itemId)
        {
            // 配置表生成后取消注释以下代码，并删除最后一行 return：
            var cfg = ConfigSystem.Instance.Tables.TbItemConfig.GetOrDefault(itemId);
            if (cfg == null)
            {
                Log.Warning($"[InventoryManager] 物品配置不存在：itemId={itemId}，使用默认 MaxStack={DefaultMaxStack}");
                return DefaultMaxStack;
            }
            return cfg.MaxStack;
        }
    }
}
