using System;
using System.Collections.Generic;

namespace CoffeeBean
{
    /// <summary>
    /// 面板统计（轻量）：累计打开次数、当前打开面板数、按类型/层级分布。
    /// CUIManager 打开/关闭路径自动记录；<see cref="CUIManager.GetPanelStats"/> 查询。
    /// </summary>
    public sealed class CUIPanelStats
    {
        private readonly Dictionary<Type, int> _openCount = new Dictionary<Type, int>();
        private readonly Dictionary<Type, int> _openTotal = new Dictionary<Type, int>();
        private int _currentOpen;
        private int _totalOpens;
        private int _totalCloses;

        /// <summary>当前打开面板总数。</summary>
        public int CurrentOpen => _currentOpen;

        /// <summary>累计打开次数。</summary>
        public int TotalOpens => _totalOpens;

        /// <summary>累计关闭次数。</summary>
        public int TotalCloses => _totalCloses;

        /// <summary>某类型面板累计打开次数。</summary>
        public int GetOpenTotal(Type panelType)
            => panelType != null && _openTotal.TryGetValue(panelType, out var c) ? c : 0;

        /// <summary>某类型面板当前打开数（Single 通常 0/1，Multiple 可 >1）。</summary>
        public int GetCurrentOpen(Type panelType)
            => panelType != null && _openCount.TryGetValue(panelType, out var c) ? c : 0;

        /// <summary>记录一次打开。</summary>
        public void RecordOpen(Type panelType)
        {
            _totalOpens++;
            _currentOpen++;
            if (panelType == null) return;
            _openTotal.TryGetValue(panelType, out var t);
            _openTotal[panelType] = t + 1;
            _openCount.TryGetValue(panelType, out var c);
            _openCount[panelType] = c + 1;
        }

        /// <summary>记录一次关闭（当前打开数不会小于 0）。</summary>
        public void RecordClose(Type panelType)
        {
            _totalCloses++;
            if (_currentOpen > 0) _currentOpen--;
            if (panelType == null) return;
            if (_openCount.TryGetValue(panelType, out var c) && c > 0)
            {
                _openCount[panelType] = c - 1;
            }
        }

        /// <summary>重置（测试/切场景用）。</summary>
        public void Reset()
        {
            _openCount.Clear();
            _openTotal.Clear();
            _currentOpen = 0;
            _totalOpens = 0;
            _totalCloses = 0;
        }

        /// <summary>统计摘要（调试/UI 显示用）。</summary>
        public override string ToString()
        {
            return $"打开 {TotalOpens} / 关闭 {TotalCloses} / 当前 {CurrentOpen}";
        }
    }
}
