using System;

namespace CoffeeBean
{
    /// <summary>
    /// 面板打开参数。
    /// 打开面板时指定：类型/预制体名/层级/传参/打开类型等。
    /// </summary>
    public sealed class CUIPanelSearchKeys
    {
        /// <summary>面板脚本类型（泛型打开时必填）。</summary>
        public Type PanelType;

        /// <summary>预制体名（加载器按此加载；为 null 时默认用 PanelType.Name）。</summary>
        public string GameObjName;

        /// <summary>目标层级。</summary>
        public CUILevel Level = CUILevel.Common;

        /// <summary>传给面板的数据。</summary>
        public ICUIData UIData;

        /// <summary>已存在的面板实例（按实例查询/关闭时使用）。</summary>
        public ICUIPanel Panel;

        /// <summary>打开类型：Single 复用 / Multiple 新建。</summary>
        public CUIPanelOpenType OpenType = CUIPanelOpenType.Single;

        /// <summary>面板名（仅脚本层 API 使用，等价 GameObjName）。</summary>
        public string Name
        {
            get => GameObjName ?? PanelType?.Name;
            set => GameObjName = value;
        }

        /// <summary>获取用于加载的预制体名（GameObjName 优先，否则 PanelType.Name）。</summary>
        public string ResolvePrefabName()
            => GameObjName ?? PanelType?.Name;

        public void Reset()
        {
            PanelType = null;
            GameObjName = null;
            UIData = null;
            Panel = null;
            Level = CUILevel.Common;
            OpenType = CUIPanelOpenType.Single;
        }

        public override string ToString()
            => $"CUIPanelSearchKeys Type:{PanelType} Prefab:{GameObjName} Level:{Level} OpenType:{OpenType}";
    }
}
