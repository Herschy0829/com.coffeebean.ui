using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CoffeeBean
{
    /// <summary>
    /// UI 管理门面：
    /// 静态 API 打开/关闭/显示/隐藏面板、栈式导航、层级挂载、加载器池。
    /// 单例（MonoBehaviour 常驻）。
    /// </summary>
    public sealed class CUIManager : MonoBehaviour
    {
        private const string Tag = "CoffeeBean.UI";
        private const string ManagerName = "[CoffeeBean] CUIManager";

        private static CUIManager _instance;

        /// <summary>UI 管理单例（自动创建）。</summary>
        public static CUIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CUIManager>();
                }
                if (_instance == null)
                {
                    var go = new GameObject(ManagerName);
                    _instance = go.AddComponent<CUIManager>();
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        /// <summary>面板表（所有已创建面板，双索引）。</summary>
        public CUIPanelTable Table { get; } = new CUIPanelTable();

        /// <summary>面板栈（栈式导航）。</summary>
        public CUIPanelStack Stack { get; } = new CUIPanelStack();

        /// <summary>加载器池（默认 Resources，可替换）。</summary>
        public ICUIPanelLoaderPool PanelLoaderPool { get; set; } = new CDefaultPanelLoaderPool();

        /// <summary>面板统计（打开/关闭计数，供调试与统计面板）。</summary>
        public CUIPanelStats Stats { get; } = new CUIPanelStats();

        /// <summary>测试用：重置单例（EditMode 测试 SetUp/TearDown 调用，释放旧实例）。</summary>
        public static void ResetInstanceForTest()
        {
            if (_instance != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_instance.gameObject);
                }
                else
                {
                    DestroyImmediate(_instance.gameObject);
                }
                _instance = null;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        // ========== 打开 ==========

        /// <summary>打开面板（同步，Single 复用）。</summary>
        public T OpenPanel<T>(CUILevel level = CUILevel.Common, ICUIData uiData = null, string prefabName = null)
            where T : CUIPanel
        {
            var keys = new CUIPanelSearchKeys
            {
                PanelType = typeof(T),
                Level = level,
                UIData = uiData,
                GameObjName = prefabName,
                OpenType = CUIPanelOpenType.Single,
            };
            return OpenPanel(keys) as T;
        }

        /// <summary>打开面板（同步，可指定打开类型与参数）。</summary>
        public ICUIPanel OpenPanel(CUIPanelSearchKeys keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            if (keys.OpenType == CUIPanelOpenType.Single)
            {
                var existed = Table.GetPanels(keys).FirstOrDefault();
                if (existed != null)
                {
                    // 层级变化则重新挂载
                    if (existed.Info != null && existed.Info.Level != keys.Level)
                    {
                        CUIRoot.Instance.SetLevelOfPanel(keys.Level, existed);
                    }
                    existed.Open(keys.UIData);
                    existed.Show();
                    CUIMaskService.OnPanelOpened(existed);
                    Stats.RecordOpen(existed.GetType());
                    return existed;
                }
            }

            var panel = CreateUI(keys);
            if (panel == null) return null;
            panel.Open(keys.UIData);
            panel.Show();
            CUIMaskService.OnPanelOpened(panel);
            Stats.RecordOpen(panel.GetType());
            return panel;
        }

        /// <summary>异步打开面板（Single）。</summary>
        public void OpenPanelAsync<T>(CUILevel level, ICUIData uiData, Action<ICUIPanel> onLoaded)
            where T : CUIPanel
        {
            var keys = new CUIPanelSearchKeys
            {
                PanelType = typeof(T),
                Level = level,
                UIData = uiData,
                OpenType = CUIPanelOpenType.Single,
            };

            if (keys.OpenType == CUIPanelOpenType.Single)
            {
                var existed = Table.GetPanels(keys).FirstOrDefault();
                if (existed != null)
                {
                    if (existed.Info != null && existed.Info.Level != keys.Level)
                    {
                        CUIRoot.Instance.SetLevelOfPanel(keys.Level, existed);
                    }
                    existed.Open(keys.UIData);
                    existed.Show();
                    CUIMaskService.OnPanelOpened(existed);
                    Stats.RecordOpen(existed.GetType());
                    onLoaded?.Invoke(existed);
                    return;
                }
            }

            CreateUIAsync(keys, panel =>
            {
                if (panel == null)
                {
                    onLoaded?.Invoke(null);
                    return;
                }
                panel.Open(keys.UIData);
                panel.Show();
                CUIMaskService.OnPanelOpened(panel);
                Stats.RecordOpen(panel.GetType());
                onLoaded?.Invoke(panel);
            });
        }

        /// <summary>创建面板（加载 prefab + 实例化 + 挂层级 + 入表 + Init）。</summary>
        private ICUIPanel CreateUI(CUIPanelSearchKeys keys)
        {
            var loader = PanelLoaderPool.AllocateLoader();
            var prefab = loader.LoadPanelPrefab(keys);
            if (prefab == null)
            {
                CLog.Error(Tag, $"面板加载失败: {keys.ResolvePrefabName()}");
                PanelLoaderPool.RecycleLoader(loader);
                return null;
            }

            var panel = CreatePanelInstance(prefab, keys, loader);
            return panel;
        }

        /// <summary>异步创建面板。</summary>
        private void CreateUIAsync(CUIPanelSearchKeys keys, Action<ICUIPanel> onCreated)
        {
            var loader = PanelLoaderPool.AllocateLoader();
            loader.LoadPanelPrefabAsync(keys, prefab =>
            {
                if (prefab == null)
                {
                    CLog.Error(Tag, $"面板异步加载失败: {keys.ResolvePrefabName()}");
                    PanelLoaderPool.RecycleLoader(loader);
                    onCreated?.Invoke(null);
                    return;
                }
                var panel = CreatePanelInstance(prefab, keys, loader);
                onCreated?.Invoke(panel);
            });
        }

        private ICUIPanel CreatePanelInstance(GameObject prefab, CUIPanelSearchKeys keys, ICUIPanelLoader loader)
        {
            var go = Instantiate(prefab);
            go.name = keys.ResolvePrefabName();

            if (!go.TryGetComponent(out CUIPanel panel))
            {
                CLog.Error(Tag, $"预制体 {go.name} 上未挂载 CUIPanel 脚本！");
                if (Application.isPlaying)
                {
                    Destroy(go);
                }
                else
                {
                    DestroyImmediate(go);
                }
                PanelLoaderPool.RecycleLoader(loader);
                return null;
            }

            // 挂层级 + 默认尺寸
            CUIRoot.Instance.SetLevelOfPanel(keys.Level, panel);
            SetDefaultSize(panel);

            panel.Loader = loader;
            panel.Info = CUIPanelInfo.Allocate(keys.ResolvePrefabName(), keys.Level, keys.UIData, keys.PanelType);

            Table.Add(panel);
            panel.Init(keys.UIData);
            return panel;
        }

        /// <summary>面板铺满层级节点。</summary>
        private static void SetDefaultSize(ICUIPanel panel)
        {
            if (panel.Transform is RectTransform rect)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.anchoredPosition3D = Vector3.zero;
                rect.localScale = Vector3.one;
            }
        }

        // ========== 关闭/显示/隐藏/查询 ==========

        /// <summary>关闭面板（类型）。</summary>
        public void ClosePanel<T>() where T : CUIPanel
        {
            var keys = new CUIPanelSearchKeys { PanelType = typeof(T) };
            ClosePanel(keys);
        }

        /// <summary>关闭面板（实例）。</summary>
        public void ClosePanel(ICUIPanel panel)
        {
            if (panel == null) return;
            var keys = new CUIPanelSearchKeys { Panel = panel };
            ClosePanel(keys);
        }

        /// <summary>关闭面板（按参数查询，取最后一个匹配）。</summary>
        public void ClosePanel(CUIPanelSearchKeys keys)
        {
            var panel = Table.GetPanels(keys).LastOrDefault();
            if (panel == null) return;

            // 顺序关键：先移除遮罩（需 Info.Level）→ 记录统计 → 移除 → 清 Info → Close（销毁）
            CUIMaskService.OnPanelClosed(panel);
            Stats.RecordClose(panel.GetType());
            Table.Remove(panel);
            panel.Info?.Reset();
            panel.Info = null;
            panel.Close();
        }

        /// <summary>关闭全部面板。</summary>
        public void CloseAllPanel()
        {
            // 先收集列表再逐个关闭（避免遍历中修改集合 / 访问已销毁对象）
            var panels = Table.ToList();
            foreach (var panel in panels)
            {
                CUIMaskService.OnPanelClosed(panel);
                Stats.RecordClose(panel.GetType());
                Table.Remove(panel);
                panel.Info?.Reset();
                panel.Info = null;
                panel.Close();
            }
            Table.Clear();
            CUIMaskService.ClearAll();
            Stats.Reset();
        }

        /// <summary>隐藏全部面板（保留实例）。</summary>
        public void HideAllPanel()
        {
            foreach (var panel in Table)
            {
                panel.Hide();
            }
        }

        /// <summary>显示面板（已存在的）。</summary>
        public void ShowPanel<T>() where T : CUIPanel
        {
            var panel = GetPanel<T>();
            panel?.Show();
        }

        /// <summary>隐藏面板（已存在的）。</summary>
        public void HidePanel<T>() where T : CUIPanel
        {
            var panel = GetPanel<T>();
            panel?.Hide();
        }

        /// <summary>获取面板（类型）。</summary>
        public T GetPanel<T>() where T : CUIPanel
            => Table.GetPanels(new CUIPanelSearchKeys { PanelType = typeof(T) }).FirstOrDefault() as T;

        /// <summary>获取面板（名称）。</summary>
        public CUIPanel GetPanel(string panelName)
            => Table.GetPanels(new CUIPanelSearchKeys { GameObjName = panelName }).FirstOrDefault() as CUIPanel;

        /// <summary>关闭当前面板并返回栈中上一个（Back）。</summary>
        public void Back(ICUIPanel currentPanel)
        {
            if (currentPanel != null)
            {
                ClosePanel(currentPanel);
            }
            Stack.Pop();
        }

        /// <summary>关闭当前面板并返回栈中上一个（Back，按名称）。</summary>
        public void Back(string currentPanelName)
        {
            if (!string.IsNullOrEmpty(currentPanelName))
            {
                ClosePanel(new CUIPanelSearchKeys { GameObjName = currentPanelName });
            }
            Stack.Pop();
        }
    }
}
