using System.Collections.Generic;

namespace CoffeeBean
{
    /// <summary>
    /// 面板栈（对齐 QFramework UIPanelStack）：
    /// Push = 记录当前面板信息并关闭它；Pop = 按记录信息重新打开上一个面板。
    /// 适用于"A 打开 B，B 返回 A"的导航场景。
    /// </summary>
    public sealed class CUIPanelStack
    {
        private readonly Stack<CUIPanelInfo> _stack = new Stack<CUIPanelInfo>();

        /// <summary>栈内数量。</summary>
        public int Count => _stack.Count;

        /// <summary>Push：记录面板信息并关闭该面板（从表移除）。</summary>
        public void Push(ICUIPanel panel)
        {
            if (panel == null) return;

            // 顺序关键：先记录 Info（引用，Close 后仍可读）→ 从表移除（需对象存活）→ 再 Close（销毁）
            var info = panel.Info;
            if (info == null) return;

            _stack.Push(info);
            CUIManager.Instance.Table.Remove(panel);
            panel.Close();
        }

        /// <summary>Pop：按记录信息重新打开上一个面板。</summary>
        public void Pop()
        {
            if (_stack.Count == 0) return;

            var info = _stack.Pop();
            var keys = new CUIPanelSearchKeys
            {
                GameObjName = info.GameObjName,
                Level = info.Level,
                UIData = info.UIData,
                PanelType = info.PanelType,
                OpenType = CUIPanelOpenType.Single,
            };
            CUIManager.Instance.OpenPanel(keys);
        }

        /// <summary>清空栈（不关闭任何面板，仅丢弃记录）。</summary>
        public void Clear() => _stack.Clear();
    }
}
