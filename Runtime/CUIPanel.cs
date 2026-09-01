using System;
using UnityEngine;

namespace CoffeeBean
{
    /// <summary>
    /// 面板抽象基类：
    /// 业务面板继承此类，实现 OnInit/OnOpen/OnShow/OnHide/OnClose 生命周期钩子。
    /// 生命周期由 CUIManager 驱动：Init -> Open -> Show；Close -> OnClose -> Hide -> 卸载。
    /// </summary>
    public abstract class CUIPanel : MonoBehaviour, ICUIPanel
    {
        /// <summary>面板 Transform。</summary>
        public Transform Transform => transform;

        /// <summary>面板加载器（CUIManager 注入）。</summary>
        public ICUIPanelLoader Loader { get; set; }

        /// <summary>面板信息（打开参数，栈导航用）。</summary>
        public CUIPanelInfo Info { get; set; }

        /// <summary>面板状态。</summary>
        public CUIPanelState State { get; set; }

        /// <summary>当前面板数据。</summary>
        protected ICUIData mUIData;

        private Action _onClosed;

        // ========== 生命周期（CUIManager 调用，不允许业务直接调） ==========

        /// <summary>初始化：创建后调用一次，-> OnInit。业务不要直接调用。</summary>
        public void Init(ICUIData uiData = null)
        {
            mUIData = uiData;
            OnInit(uiData);
        }

        /// <summary>打开：-> OnOpen，状态 Opening。业务不要直接调用。</summary>
        public void Open(ICUIData uiData = null)
        {
            State = CUIPanelState.Opening;
            mUIData = uiData;
            OnOpen(uiData);
        }

        /// <summary>显示：SetActive(true) -> OnShow。业务不要直接调用。</summary>
        public void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        /// <summary>隐藏：状态 Hide -> OnHide -> SetActive(false)。业务不要直接调用。</summary>
        public void Hide()
        {
            State = CUIPanelState.Hide;
            OnHide();
            gameObject.SetActive(false);
        }

        /// <summary>关闭卸载：OnClose -> Hide -> 卸载 loader -> 可选销毁。业务不要直接调用。</summary>
        public void Close(bool destroy = true)
        {
            _onClosed?.Invoke();
            _onClosed = null;

            OnClose();
            Hide();
            State = CUIPanelState.Closed;

            if (destroy && gameObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
            }

            // 卸载并回收加载器（回到池，供后续面板复用）
            if (Loader != null)
            {
                Loader.Unload();
                var mgr = CUIManager.Instance;
                if (mgr != null)
                {
                    mgr.PanelLoaderPool.RecycleLoader(Loader);
                }
                Loader = null;
            }
            mUIData = null;
            // 注意：Info 不在此处清空/重置 —— 栈式导航（CUIPanelStack.Push）需要 Info 在 Close 后仍可读。
            // 由 CUIManager 在移除面板时负责清理（ClosePanel / CloseAllPanel）。
        }

        // ========== 业务可重写的钩子 ==========

        /// <summary>初始化钩子（面板创建后调用一次）。</summary>
        protected virtual void OnInit(ICUIData uiData = null) { }

        /// <summary>打开钩子（每次打开都会调用）。</summary>
        protected virtual void OnOpen(ICUIData uiData = null) { }

        /// <summary>显示钩子（Show 时调用）。</summary>
        protected virtual void OnShow() { }

        /// <summary>隐藏钩子（Hide 时调用）。</summary>
        protected virtual void OnHide() { }

        /// <summary>关闭钩子（Close 时调用）。</summary>
        protected virtual void OnClose() { }

        /// <summary>销毁前钩子（OnDestroy 调用，可用于清理）。</summary>
        protected virtual void OnBeforeDestroy() { }

        protected virtual void OnDestroy()
        {
            if (Application.isPlaying)
            {
                OnBeforeDestroy();
            }
        }

        // ========== 面板便捷方法 ==========

        /// <summary>关闭当前面板。</summary>
        protected void CloseSelf() => CUIManager.Instance.ClosePanel(this);

        /// <summary>关闭当前面板并返回栈中上一个面板。</summary>
        protected void Back() => CUIManager.Instance.Back(this);

        /// <summary>注册关闭回调（关闭时触发一次）。</summary>
        public void OnClosed(Action onClosed) => _onClosed = onClosed;
    }
}
