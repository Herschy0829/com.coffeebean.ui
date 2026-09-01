using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CoffeeBean.EditorTools
{
    /// <summary>
    /// 编译后自动挂载：
    /// 代码生成后注册的预制体，在脚本编译完成后（[DidReloadScripts]）按 CBind 标记
    /// 把节点引用填充到面板脚本的序列化字段上。
    /// </summary>
    public static class CUISerializer
    {
        /// <summary>编译完成后自动挂载字段引用。</summary>
        [DidReloadScripts]
        private static void OnAfterCompile()
        {
            var paths = CUICodeGenerator.DrainAutoBindPaths();
            if (paths == null || paths.Count == 0) return;

            var assemblies = GetAssemblies();
            foreach (var path in paths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                BindFields(prefab, assemblies);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CoffeeBean.UI] 自动挂载完成，共 {paths.Count} 个面板");
        }

        /// <summary>将 CBind 节点引用填充到面板的 Designer 序列化字段。</summary>
        private static void BindFields(GameObject prefab, List<Assembly> assemblies)
        {
            if (!prefab.TryGetComponent(out CUIPanel panelScript))
            {
                // 面板脚本是编译后新生成的，需要先 AddComponent
                var panelType = FindType($"{CUISettings.Namespace}.{prefab.name}", assemblies);
                if (panelType == null)
                {
                    Debug.LogWarning($"[CoffeeBean.UI] 找不到面板类型 {CUISettings.Namespace}.{prefab.name}，跳过挂载");
                    return;
                }
                panelScript = (CUIPanel)prefab.AddComponent(panelType);
            }

            var serialized = new SerializedObject(panelScript);
            var binds = prefab.GetComponentsInChildren<CBind>(true);
            foreach (var bind in binds)
            {
                var property = serialized.FindProperty(bind.FieldName);
                if (property == null)
                {
                    // 节点名与字段名不一致时按字段名回退查找
                    continue;
                }
                property.objectReferenceValue = bind.gameObject;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();

            // 移除 CBind 标记（生成后运行时无开销）
            foreach (var bind in binds)
            {
                UnityEngine.Object.DestroyImmediate(bind, true);
            }
        }

        private static Type FindType(string fullName, List<Assembly> assemblies)
        {
            foreach (var asm in assemblies)
            {
                var t = asm.GetType(fullName);
                if (t != null) return t;
            }
            return null;
        }

        private static List<Assembly> GetAssemblies()
        {
            var result = new List<Assembly>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name.Contains("CoffeeBean"))
                {
                    result.Add(asm);
                }
            }
            return result;
        }
    }
}
