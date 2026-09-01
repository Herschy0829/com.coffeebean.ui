using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeBean
{
    /// <summary>
    /// 面板加载器接口（对齐 QFramework IPanelLoader，可插拔）：
    /// 默认实现 <see cref="CResourcesPanelLoader"/>（Resources.Load）；
    /// asset 模块发布后可用 Addressables 实现替换（CAssetPanelLoader）。
    /// </summary>
    public interface ICUIPanelLoader
    {
        /// <summary>同步加载面板预制体。</summary>
        GameObject LoadPanelPrefab(CUIPanelSearchKeys keys);

        /// <summary>异步加载面板预制体。</summary>
        void LoadPanelPrefabAsync(CUIPanelSearchKeys keys, Action<GameObject> onLoaded);

        /// <summary>卸载（关闭面板时调用）。</summary>
        void Unload();
    }

    /// <summary>加载器池接口（对齐 QFramework IPanelLoaderPool）：分配/回收加载器。</summary>
    public interface ICUIPanelLoaderPool
    {
        ICUIPanelLoader AllocateLoader();
        void RecycleLoader(ICUIPanelLoader loader);
    }

    /// <summary>
    /// 抽象加载器池：复用闲置加载器，减少 GC。
    /// </summary>
    public abstract class CAbstractPanelLoaderPool : ICUIPanelLoaderPool
    {
        private readonly Stack<ICUIPanelLoader> _pool = new Stack<ICUIPanelLoader>(16);

        public ICUIPanelLoader AllocateLoader()
            => _pool.Count > 0 ? _pool.Pop() : CreatePanelLoader();

        protected abstract ICUIPanelLoader CreatePanelLoader();

        public void RecycleLoader(ICUIPanelLoader loader) => _pool.Push(loader);
    }

    /// <summary>默认加载器池：Resources 实现。</summary>
    public sealed class CDefaultPanelLoaderPool : CAbstractPanelLoaderPool
    {
        protected override ICUIPanelLoader CreatePanelLoader() => new CResourcesPanelLoader();
    }

    /// <summary>
    /// Resources 加载器（默认实现）：从 Resources 目录加载面板预制体（按 keys.ResolvePrefabName()）。
    /// </summary>
    public sealed class CResourcesPanelLoader : ICUIPanelLoader
    {
        private GameObject _panelPrefab;

        public GameObject LoadPanelPrefab(CUIPanelSearchKeys keys)
        {
            _panelPrefab = Resources.Load<GameObject>(keys.ResolvePrefabName());
            return _panelPrefab;
        }

        public void LoadPanelPrefabAsync(CUIPanelSearchKeys keys, Action<GameObject> onLoaded)
        {
            var request = Resources.LoadAsync<GameObject>(keys.ResolvePrefabName());
            request.completed += _ =>
            {
                _panelPrefab = request.asset as GameObject;
                onLoaded?.Invoke(_panelPrefab);
            };
        }

        public void Unload()
        {
            _panelPrefab = null;
        }
    }
}
