#if COFFEEBEAN_CORE
using CoffeeBean;

[assembly: CoffeeBeanModule(
    "com.coffeebean.ui",
    "0.2.2",
    DisplayName = "UI",
    Description = "UI module: UGUI panel management (CUIManager/CUIPanel, layers, stack, pluggable loader) + code generation (CBind, two-file separation).",
    Dependencies = new[] { "com.coffeebean.core", "com.coffeebean.tools" }
)]

namespace CoffeeBean
{
    /// <summary>
    /// Core 集成：CUIManager 由业务在启动时初始化（自动创建单例），
    /// 本模块标记使 ui 可被 Core 发现、启停与版本检查。
    /// </summary>
    public sealed class UIModule : ICoffeeBeanModule
    {
        public void OnLoad(CoffeeBeanContext context)
        {
            context.Log("CoffeeBean.UI integrated (CUIManager auto-creates on first access).");
        }

        public void OnStart()
        {
        }

        public void OnShutdown()
        {
        }
    }
}
#endif
