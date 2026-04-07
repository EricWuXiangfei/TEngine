namespace GameLogic
{
    /// <summary>
    /// 物品数据只读快照，供外部模块查询使用。
    /// </summary>
    public readonly struct ItemData
    {
        /// <summary>物品 ID。</summary>
        public int ItemId { get; }

        /// <summary>当前数量。</summary>
        public int Count { get; }

        public ItemData(int itemId, int count)
        {
            ItemId = itemId;
            Count  = count;
        }

        /// <summary>表示"物品不存在"的空值。</summary>
        public static readonly ItemData Empty = new ItemData(0, 0);

        /// <summary>是否为有效物品。</summary>
        public bool IsValid => ItemId > 0;

        /// <summary>是否为空格（ItemId == 0）。</summary>
        public bool IsEmpty => ItemId == 0;
    }
}
