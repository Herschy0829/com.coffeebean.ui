using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoffeeBean.EditorTools
{
    /// <summary>
    /// 面板代码生成器（对齐 QFramework UICodeGenerator + UISerializer）：
    /// 扫描预制体上的 CBind 标记 → 生成 主脚本 + Designer（两文件分离）→ 注册编译后自动挂载。
    /// 入口：右键菜单 / CUIPanelInspector 按钮。
    /// </summary>
    public static class CUICodeGenerator
    {
        private const string AutoBindPrefsKey = "CoffeeBean.UI.AutoBindPrefabs";

        [MenuItem("Assets/CoffeeBean UI/生成面板代码 (alt+c) &c")]
        public static void GenerateBySelection()
        {
            var objs = Selection.GetFiltered<GameObject>(SelectionMode.Assets | SelectionMode.TopLevel);
            if (objs == null || objs.Length == 0)
            {
                Debug.LogWarning("[CoffeeBean.UI] 请先在 Project 窗口选中一个或多个 UI 预制体");
                return;
            }

            foreach (var go in objs)
            {
                Generate(go);
            }

            AssetDatabase.Refresh();
            Debug.Log($"[CoffeeBean.UI] 已生成 {objs.Length} 个面板代码（编译完成后自动挂载字段引用）");
        }

        /// <summary>对单个预制体生成代码。</summary>
        public static void Generate(GameObject uiPrefab)
        {
            if (uiPrefab == null) return;

            var prefabPath = AssetDatabase.GetAssetPath(uiPrefab);
            if (string.IsNullOrEmpty(prefabPath) || !prefabPath.EndsWith(".prefab"))
            {
                Debug.LogWarning($"[CoffeeBean.UI] {uiPrefab.name} 不是预制体资源，跳过");
                return;
            }

            // 收集 CBind 标记
            var info = CollectBinds(uiPrefab);

            // 生成目录：与预制体同目录
            var dir = Path.GetDirectoryName(prefabPath).Replace('\\', '/');
            var panelName = uiPrefab.name;
            var ns = string.IsNullOrEmpty(CUISettings.Namespace) ? "CoffeeBean" : CUISettings.Namespace;

            // 主脚本（仅首次生成，不覆盖用户代码）
            var mainPath = $"{dir}/{panelName}.cs";
            if (!File.Exists(mainPath))
            {
                File.WriteAllText(mainPath, CUICodeTemplates.PanelMainScript(panelName, ns));
            }

            // Designer（每次覆盖）
            var designerPath = $"{dir}/{panelName}.Designer.cs";
            File.WriteAllText(designerPath, CUICodeTemplates.PanelDesignerScript(panelName, ns, info));

            // 独立元素脚本（IsElement 标记的节点）
            foreach (var field in info.Fields)
            {
                if (field.IsElement)
                {
                    var elementPath = $"{dir}/{field.FieldName}.cs";
                    if (!File.Exists(elementPath))
                    {
                        File.WriteAllText(elementPath, CUICodeTemplates.ElementScript(field.FieldName, ns));
                    }
                }
            }

            // 注册编译后自动挂载
            RegisterAutoBind(prefabPath);
        }

        /// <summary>扫描预制体上的 CBind 标记。</summary>
        private static CUIPanelCodeInfo CollectBinds(GameObject root)
        {
            var info = new CUIPanelCodeInfo();
            var binds = root.GetComponentsInChildren<CBind>(true);
            foreach (var bind in binds)
            {
                info.Fields.Add(new CUIPanelCodeInfo.BindField
                {
                    FieldName = bind.FieldName,
                    ComponentType = bind.ResolveTypeName(),
                    Comment = bind.Comment,
                    IsElement = bind.IsElement,
                });
            }
            return info;
        }

        /// <summary>记录待挂载的预制体路径（编译完成后 CUISerializer 处理）。</summary>
        public static void RegisterAutoBind(string prefabPath)
        {
            var current = EditorPrefs.GetString(AutoBindPrefsKey, string.Empty);
            var paths = new List<string>();
            if (!string.IsNullOrEmpty(current))
            {
                paths.AddRange(current.Split(';'));
            }
            if (!paths.Contains(prefabPath))
            {
                paths.Add(prefabPath);
            }
            EditorPrefs.SetString(AutoBindPrefsKey, string.Join(";", paths.ToArray()));
        }

        /// <summary>读取并清空待挂载列表。</summary>
        public static List<string> DrainAutoBindPaths()
        {
            var current = EditorPrefs.GetString(AutoBindPrefsKey, string.Empty);
            EditorPrefs.DeleteKey(AutoBindPrefsKey);
            var paths = new List<string>();
            if (!string.IsNullOrEmpty(current))
            {
                paths.AddRange(current.Split(';'));
            }
            return paths;
        }
    }
}
