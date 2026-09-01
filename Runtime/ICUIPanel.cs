using UnityEngine;

namespace CoffeeBean
{
    /// <summary>
    /// 面板接口（对齐 QFramework IPanel）：
    /// CUIManager 通过此接口驱动面板生命周期，业务面板继承 <see cref="CUIPanel"/> 即可。
    /// </summary>
    public interface ICUIPanel
    {
        /// <summary>面板 Transform。</summary>
        Transform Transform { get; }

        /// <summary>面板加载器（由 CUIManager 注入，关闭时卸载）。</summary>
        ICUIPanelLoader Loader { get; set; }

        /// <summary>面板信息（打开参数）。</summary>
        CUIPanelInfo Info { get; set; }

        /// <summary>面板状态。</summary>
        CUIPanelState State { get; set; }

        /// <summary>初始化（创建后调用一次，-> OnInit）。</summary>
        void Init(ICUIData uiData = null);

        /// <summary>打开（-> OnOpen，状态 Opening）。</summary>
        void Open(ICUIData uiData = null);

        /// <summary>显示（SetActive(true) -> OnShow）。</summary>
        void Show();

        /// <summary>隐藏（-> OnHide -> SetActive(false)）。</summary>
        void Hide();

        /// <summary>关闭卸载（-> OnClose -> 卸载 loader -> 可选销毁）。</summary>
        void Close(bool destroy = true);
    }
}
