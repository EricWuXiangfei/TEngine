using System.Collections.Generic;
using System.Reflection;
using GameLogic;
#if ENABLE_OBFUZ
using Obfuz;
#endif
using TEngine;
#pragma warning disable CS0436


/// <summary>
/// 游戏App。
/// </summary>
#if ENABLE_OBFUZ
[ObfuzIgnore(ObfuzScope.TypeName | ObfuzScope.MethodName)]
#endif
public partial class GameApp
{
    private static List<Assembly> _hotfixAssembly;

    /// <summary>
    /// 热更域App主入口。
    /// </summary>
    /// <param name="objects"></param>
    public static void Entrance(object[] objects)
    {
        GameEventHelper.Init();
        _hotfixAssembly = (List<Assembly>)objects[0];
        Log.Warning("======= 看到此条日志代表你成功运行了热更新代码 =======");
        Log.Warning("======= Entrance GameApp =======");
        Utility.Unity.AddDestroyListener(Release);
        Log.Warning("======= StartGameLogic =======");
        StartGameLogic();
    }

    private static void StartGameLogic()
    {
        // GameEvent.Get<ILoginUI>().ShowLoginUI();
        GameModule.UI.ShowUIAsync<UIInventoryPanel>();

        //// [6.1 集成验证] 测试背包系统基本功能，验证编译通过后移除此代码块
        //InventoryManager.Instance.AddItem(10001, 3);
        //Log.Warning($"[背包测试] 物品10001数量={InventoryManager.Instance.GetCount(10001)}"); // 期望：3
        //InventoryManager.Instance.AddItem(10001, 2);
        //Log.Warning($"[背包测试] 再次添加后数量={InventoryManager.Instance.GetCount(10001)}"); // 期望：5
        //bool removed = InventoryManager.Instance.RemoveItem(10001, 1);
        //Log.Warning($"[背包测试] 移除1个 结果={removed} 剩余={InventoryManager.Instance.GetCount(10001)}"); // 期望：true, 4
        //bool consumed = InventoryManager.Instance.ConsumeItem(10001, 10);
        //Log.Warning($"[背包测试] 消耗10个（数量不足）结果={consumed}"); // 期望：false
    }

    private static void Release()
    {
        SingletonSystem.Release();
        Log.Warning("======= Release GameApp =======");
    }
}