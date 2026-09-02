using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeBean
{
    /// <summary>
    /// 面板遮罩服务（点击穿透拦截）：
    /// 打开需要遮罩层级（PopUI 及以上）的面板时，在面板所在层下方插入全屏半透明遮罩，
    /// 拦截点击防止穿透到底层面板；支持点击遮罩关闭面板；面板关闭时自动移除遮罩。
    ///
    /// 用法：CUIManager.OpenPanel 内部自动调用（无需业务介入）；
    /// 通过 <see cref="CUIManagerOptions"/> 或 <see cref="NeedMaskForLevel"/> 配置。
    /// </summary>
    public static class CUIMaskService
    {
        private const string Tag = "CoffeeBean.UI";

        // 遮罩条目：按层级记录（同层多面板共享一个遮罩）
        private sealed class MaskEntry
        {
            public GameObject MaskGo;
            public CUILevel Level;
        }

        private static readonly List<MaskEntry> Stack = new List<MaskEntry>();

        /// <summary>是否需要为该层级生成遮罩（默认 PopUI 及以上；业务可改）。</summary>
        public static System.Func<CUILevel, bool> NeedMaskForLevel = level => level >= CUILevel.PopUI;

        /// <summary>是否允许点击遮罩关闭面板（默认 true）。</summary>
        public static bool ClickToClose = true;

        /// <summary>遮罩颜色（半透明黑）。</summary>
        public static Color MaskColor = new Color(0f, 0f, 0f, 0.55f);

        /// <summary>当前活动遮罩数。</summary>
        public static int ActiveMaskCount => Stack.Count;

        /// <summary>
        /// 面板 Show 后调用：若层级需遮罩，在面板所在层生成遮罩（同层共享一个）。
        /// </summary>
        public static void OnPanelOpened(ICUIPanel panel)
        {
            if (panel == null || panel.Transform == null) return;
            CUILevel level = panel.Info != null ? panel.Info.Level : CUILevel.Common;
            if (!NeedMaskForLevel(level)) return;

            // 同层级已有遮罩：共享，不重复生成
            if (HasMaskForLevel(level)) return;

            CreateMask(panel, level);
        }

        /// <summary>面板关闭前调用：该层级无其他活动面板时移除遮罩。</summary>
        public static void OnPanelClosed(ICUIPanel panel)
        {
            if (panel == null) return;
            CUILevel level = panel.Info != null ? panel.Info.Level : CUILevel.Common;

            // 该层是否还有其他活动面板（排除关闭的）
            if (HasOtherPanelOnLevel(panel, level)) return;

            // 无其他面板 → 移除该层遮罩
            for (int i = Stack.Count - 1; i >= 0; i--)
            {
                if (Stack[i].Level == level)
                {
                    RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>清空全部遮罩（CUIManager.CloseAllPanel 时）。</summary>
        public static void ClearAll()
        {
            for (int i = Stack.Count - 1; i >= 0; i--)
            {
                RemoveAt(i);
            }
        }

        /// <summary>测试用：是否有指定层级的遮罩。</summary>
        public static bool HasMaskForLevel(CUILevel level)
        {
            for (int i = 0; i < Stack.Count; i++)
            {
                if (Stack[i].Level == level) return true;
            }
            return false;
        }

        // ========== 内部 ==========

        private static void CreateMask(ICUIPanel panel, CUILevel level)
        {
            var root = CUIRoot.Instance;
            if (root == null) return;

            var layer = root.GetLayer(level);
            if (layer == null) return;

            // 遮罩 Go：Image（半透明拦截）+ Button（点击关闭）
            var maskGo = new GameObject("Mask_" + level, typeof(RectTransform), typeof(Image), typeof(Button));
            maskGo.transform.SetParent(layer, false);

            var rect = (RectTransform)maskGo.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = maskGo.GetComponent<Image>();
            image.color = MaskColor;

            // 遮罩放在面板下方（拦截点击但视觉在面板后）
            maskGo.transform.SetSiblingIndex(0);

            if (ClickToClose)
            {
                var button = maskGo.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    // 点击遮罩关闭最上层该层级的需遮罩面板
                    ICUIPanel toClose = FindTopPanelOnLevel(level);
                    if (toClose != null)
                    {
                        CUIManager.Instance.ClosePanel(toClose);
                    }
                });
            }
            else
            {
                var btn = maskGo.GetComponent<Button>();
                if (Application.isPlaying) Object.Destroy(btn);
                else Object.DestroyImmediate(btn);
            }

            Stack.Add(new MaskEntry { MaskGo = maskGo, Level = level });
            CLog.Info(Tag, $"生成面板遮罩（{level}）");
        }

        private static void RemoveAt(int index)
        {
            MaskEntry entry = Stack[index];
            Stack.RemoveAt(index);
            if (entry.MaskGo != null)
            {
                if (Application.isPlaying) Object.Destroy(entry.MaskGo);
                else Object.DestroyImmediate(entry.MaskGo);
            }
        }

        private static ICUIPanel FindTopPanelOnLevel(CUILevel level)
        {
            var mgr = CUIManager.Instance;
            if (mgr == null) return null;
            foreach (var p in mgr.Table)
            {
                if (p.Info != null && p.Info.Level == level)
                {
                    return p;
                }
            }
            return null;
        }

        private static bool HasOtherPanelOnLevel(ICUIPanel closing, CUILevel level)
        {
            var mgr = CUIManager.Instance;
            if (mgr == null) return false;
            foreach (var p in mgr.Table)
            {
                if (ReferenceEquals(p, closing)) continue;
                if (p.Info != null && p.Info.Level == level) return true;
            }
            return false;
        }
    }
}
