using System;
using System.Threading.Tasks;
using UnityEngine;

namespace CoffeeBean
{
    /// <summary>
    /// Addressables 面板加载器（基于 asset 模块的 CAssetSystem）：
    /// 面板预制体通过 Addressables 加载（默认资源路径或 address 名），
    /// 替代默认的 <see cref="CResourcesPanelLoader"/>。
    ///
    /// 使用：<c>CUIManager.Instance.PanelLoaderPool = new CAssetPanelLoaderPool();</c>
    /// （一次性；之后打开面板都走 Addressables 加载）
    /// </summary>
    public sealed class CAssetPanelLoader : ICUIPanelLoader
    {
        private GameObject _panelPrefab;
        private string _loadedAddress;

        public GameObject LoadPanelPrefab(CUIPanelSearchKeys keys)
        {
            string address = keys.ResolvePrefabName();
            _panelPrefab = CAssetSystem.Instance.LoadAsset<GameObject>(address);
            _loadedAddress = _panelPrefab != null ? address : null;
            return _panelPrefab;
        }

        public void LoadPanelPrefabAsync(CUIPanelSearchKeys keys, Action<GameObject> onLoaded)
        {
            LoadPanelPrefabAsyncInternal(keys, onLoaded);
        }

        private async void LoadPanelPrefabAsyncInternal(CUIPanelSearchKeys keys, Action<GameObject> onLoaded)
        {
            string address = keys.ResolvePrefabName();
            _panelPrefab = await CAssetSystem.Instance.LoadAssetAsync<GameObject>(address);
            _loadedAddress = _panelPrefab != null ? address : null;
            onLoaded?.Invoke(_panelPrefab);
        }

        public void Unload()
        {
            // 面板关闭时释放引用（CAssetSystem 引用计数 -1；归零才真正释放）
            if (!string.IsNullOrEmpty(_loadedAddress))
            {
                CAssetSystem.Instance.Release(_loadedAddress);
                _loadedAddress = null;
            }
            _panelPrefab = null;
        }
    }

    /// <summary>Addressables 面板加载器池（供 CUIManager.PanelLoaderPool 使用）。</summary>
    public sealed class CAssetPanelLoaderPool : CAbstractPanelLoaderPool
    {
        protected override ICUIPanelLoader CreatePanelLoader() => new CAssetPanelLoader();
    }
}
