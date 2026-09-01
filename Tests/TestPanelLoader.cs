using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeBean.UI.Tests
{
    /// <summary>
    /// 测试加载器：从内存字典返回 prefab（EditMode 无真实 Resources 资源）。
    /// 同时记录加载/卸载调用次数，验证可插拔加载器与池回收。
    /// </summary>
    public sealed class TestPanelLoader : ICUIPanelLoader
    {
        private static readonly Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();

        public int LoadCount;
        public int UnloadCount;

        /// <summary>注册测试 prefab（key = prefab 名）。</summary>
        public static void RegisterPrefab(string name, GameObject prefab)
        {
            Prefabs[name] = prefab;
        }

        public static void ClearPrefabs() => Prefabs.Clear();

        public GameObject LoadPanelPrefab(CUIPanelSearchKeys keys)
        {
            LoadCount++;
            Prefabs.TryGetValue(keys.ResolvePrefabName(), out var prefab);
            return prefab;
        }

        public void LoadPanelPrefabAsync(CUIPanelSearchKeys keys, Action<GameObject> onLoaded)
        {
            LoadCount++;
            Prefabs.TryGetValue(keys.ResolvePrefabName(), out var prefab);
            onLoaded?.Invoke(prefab);
        }

        public void Unload() => UnloadCount++;
    }

    /// <summary>测试加载器池：每个 loader 独立计数，池回收可验证。</summary>
    public sealed class TestPanelLoaderPool : ICUIPanelLoaderPool
    {
        private readonly Stack<TestPanelLoader> _pool = new Stack<TestPanelLoader>();

        public int CreatedCount { get; private set; }
        public int RecycleCount { get; private set; }

        public ICUIPanelLoader AllocateLoader()
        {
            if (_pool.Count > 0) return _pool.Pop();
            CreatedCount++;
            return new TestPanelLoader();
        }

        public void RecycleLoader(ICUIPanelLoader loader)
        {
            RecycleCount++;
            _pool.Push((TestPanelLoader)loader);
        }
    }
}
