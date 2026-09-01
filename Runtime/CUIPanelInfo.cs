using System;

namespace CoffeeBean
{
    /// <summary>
    /// 面板信息（对齐 QFramework PanelInfo，对象池复用）：
    /// 记录面板的打开参数，供栈式导航 Pop 时恢复。
    /// </summary>
    public sealed class CUIPanelInfo
    {
        /// <summary>传给面板的数据。</summary>
        public ICUIData UIData;

        /// <summary>面板层级。</summary>
        public CUILevel Level = CUILevel.Common;

        /// <summary>预制体名。</summary>
        public string GameObjName;

        /// <summary>面板脚本类型。</summary>
        public Type PanelType;

        public static CUIPanelInfo Allocate(string gameObjName, CUILevel level, ICUIData uiData, Type panelType)
        {
            var info = new CUIPanelInfo
            {
                GameObjName = gameObjName,
                Level = level,
                UIData = uiData,
                PanelType = panelType,
            };
            return info;
        }

        public void Reset()
        {
            UIData = null;
            GameObjName = null;
            PanelType = null;
            Level = CUILevel.Common;
        }
    }
}
