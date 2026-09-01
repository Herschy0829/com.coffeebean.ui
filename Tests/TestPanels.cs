using UnityEngine;

namespace CoffeeBean.UI.Tests
{
    /// <summary>测试面板数据。</summary>
    public class TestPanelData : CUIPanelData
    {
        public string Message;
    }

    /// <summary>测试面板：记录生命周期调用顺序。</summary>
    public class TestPanel : CUIPanel
    {
        public System.Collections.Generic.List<string> Lifecycle = new System.Collections.Generic.List<string>();

        public ICUIData LastInitData;
        public ICUIData LastOpenData;
        public bool Shown;
        public bool Hidden;

        protected override void OnInit(ICUIData uiData = null)
        {
            LastInitData = uiData;
            Lifecycle.Add("OnInit");
        }

        protected override void OnOpen(ICUIData uiData = null)
        {
            LastOpenData = uiData;
            Lifecycle.Add("OnOpen");
        }

        protected override void OnShow()
        {
            Shown = true;
            Lifecycle.Add("OnShow");
        }

        protected override void OnHide()
        {
            Hidden = true;
            Lifecycle.Add("OnHide");
        }

        protected override void OnClose()
        {
            Lifecycle.Add("OnClose");
        }

        protected override void OnBeforeDestroy()
        {
            Lifecycle.Add("OnBeforeDestroy");
        }
    }

    /// <summary>测试面板 B（Multiple 打开测试用）。</summary>
    public class TestPanelB : CUIPanel
    {
        public System.Collections.Generic.List<string> Lifecycle = new System.Collections.Generic.List<string>();
        public int OpenCount;

        protected override void OnOpen(ICUIData uiData = null)
        {
            OpenCount++;
            Lifecycle.Add("OnOpen");
        }

        protected override void OnClose()
        {
            Lifecycle.Add("OnClose");
        }
    }

    /// <summary>测试工具：创建面板 GameObject 并挂 CUIPanel 派生类。</summary>
    public static class UITestUtil
    {
        /// <summary>创建测试面板 prefab（EditMode 可直接 Instantiate）。</summary>
        public static GameObject CreatePanelPrefab<T>() where T : CUIPanel
        {
            var go = new GameObject(typeof(T).Name, typeof(RectTransform));
            go.AddComponent<T>();
            return go;
        }
    }
}
