namespace CoffeeBean
{
    /// <summary>
    /// 面板传参数据接口（对齐 QFramework 的 IUIData）：打开面板时传给 <see cref="CUIPanel.OnInit/OnOpen"/>。
    /// 业务面板定义自己的 Data 类实现此接口。
    /// </summary>
    public interface ICUIData
    {
    }

    /// <summary>面板数据基类（业务面板 Data 可继承，也可直接实现 ICUIData）。</summary>
    public class CUIPanelData : ICUIData
    {
    }
}
