using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoffeeBean.UI.Tests
{
    /// <summary>测试用 asset 后端（实现 asset 模块公共接口，不依赖真实 Addressables）。</summary>
    internal sealed class TestAssetBackend : IAssetBackend
    {
        private readonly Dictionary<string, Object> _assets = new Dictionary<string, Object>();

        public void Register(string address, Object asset) => _assets[address] = asset;

        public bool HasAddress(string address) => _assets.ContainsKey(address);

        public Task<bool> HasAddressAsync(string address)
        {
            return Task.FromResult(_assets.ContainsKey(address));
        }

        public T LoadAssetSync<T>(string address) where T : Object
            => _assets.TryGetValue(address, out var asset) && asset is T t ? t : null;

        public Task<T> LoadAssetAsync<T>(string address) where T : Object
        {
            T result = _assets.TryGetValue(address, out var asset) && asset is T t ? t : null;
            return Task.FromResult(result);
        }

        public void Release(string address, Object asset)
        {
            _assets.Remove(address);
        }

        public void ReleaseAll() => _assets.Clear();
    }

    /// <summary>CAssetPanelLoader 测试：通过 CAssetSystem（mock 后端）加载面板。</summary>
    public class CAssetPanelLoaderTests
    {
        private GameObject _managerGo;
        private TestAssetBackend _backend;
        private GameObject _prefab;

        [SetUp]
        public void SetUp()
        {
            // 创建 CAssetSystem 实例并注入 mock 后端
            _managerGo = new GameObject("[CoffeeBean] CAssetSystem");
            var assetSystem = _managerGo.AddComponent<CAssetSystem>();
            CAssetSystem.SetInstanceForTest(assetSystem);
            _backend = new TestAssetBackend();
            assetSystem.Backend = _backend;

            // 注册测试面板 prefab（带 CUIPanel 组件）
            _prefab = new GameObject("TestAssetPanel");
            _prefab.AddComponent<TestPanel>();
            _backend.Register("TestAssetPanel", _prefab);
        }

        [TearDown]
        public void TearDown()
        {
            CAssetSystem.ResetInstanceForTest();
            if (_prefab != null) Object.DestroyImmediate(_prefab);
            if (_managerGo != null) Object.DestroyImmediate(_managerGo);
        }

        private static CUIPanelSearchKeys MakeKeys() => new CUIPanelSearchKeys { PanelType = typeof(TestPanel), GameObjName = "TestAssetPanel" };

        [Test]
        public void LoadPanelPrefab_LoadsViaAssetSystem()
        {
            var loader = new CAssetPanelLoader();
            GameObject result = loader.LoadPanelPrefab(MakeKeys());

            Assert.IsNotNull(result, "应通过 asset 系统加载面板");
            Assert.IsNotNull(result.GetComponent<TestPanel>(), "面板应有 CUIPanel 组件");
        }

        [Test]
        public void Unload_ReleasesRefCount()
        {
            var loader = new CAssetPanelLoader();
            loader.LoadPanelPrefab(MakeKeys());

            // 加载后 CAssetSystem 有缓存；Unload 释放
            Assert.IsTrue(CAssetSystem.Instance.IsLoaded("TestAssetPanel"), "加载后应缓存");
            loader.Unload();

            Assert.IsFalse(CAssetSystem.Instance.IsLoaded("TestAssetPanel"), "Unload 应释放引用（计数归零）");
        }

        [Test]
        public async Task LoadPanelPrefabAsync_LoadsViaAssetSystem()
        {
            var loader = new CAssetPanelLoader();
            GameObject loaded = null;
            loader.LoadPanelPrefabAsync(MakeKeys(), go => loaded = go);

            // 等待异步完成（mock 后端同步返回，一帧内完成）
            for (int i = 0; i < 10 && loaded == null; i++)
            {
                await Task.Delay(10);
            }

            Assert.IsNotNull(loaded, "异步应加载面板");
            Assert.IsTrue(CAssetSystem.Instance.IsLoaded("TestAssetPanel"));
        }

        [Test]
        public void Pool_AllocatesAssetLoader()
        {
            var pool = new CAssetPanelLoaderPool();
            var loader = pool.AllocateLoader();

            Assert.IsInstanceOf<CAssetPanelLoader>(loader, "池应分配 CAssetPanelLoader");
            pool.RecycleLoader(loader);
        }
    }
}
