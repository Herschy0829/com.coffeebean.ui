namespace CoffeeBean
{
    /// <summary>面板状态（对齐 QFramework PanelState）。</summary>
    public enum CUIPanelState
    {
        /// <summary>打开中（OnOpen 已调用，尚未 Show 或已 Show）。</summary>
        Opening = 0,

        /// <summary>已隐藏（OnHide 已调用）。</summary>
        Hide = 1,

        /// <summary>已关闭卸载（OnClose 已调用）。</summary>
        Closed = 2,
    }

    /// <summary>
    /// 面板打开类型（对齐 QFramework PanelOpenType）：
    /// Single = 同类型面板全局唯一（重复打开复用并重新 Show）；
    /// Multiple = 每次打开都新建实例（可并存多个）。
    /// </summary>
    public enum CUIPanelOpenType
    {
        Single,
        Multiple,
    }

    /// <summary>
    /// UI 层级（QF 风格 6 层）：Bg（背景）/ Common（普通面板）/ PopUI（弹窗）/
    /// Guide（新手引导）/ Toast（提示）/ Top（最顶层）。
    /// </summary>
    public enum CUILevel
    {
        Bg = 0,
        Common = 1,
        PopUI = 2,
        Guide = 3,
        Toast = 4,
        Top = 5,
    }
}
