using UnityEditor;
using UnityEngine;

namespace CoffeeBean.EditorTools
{
    /// <summary>
    /// 面板 Inspector：
    /// 选中 UI 预制体时显示"生成代码"按钮，一键生成主脚本 + Designer 并注册自动挂载。
    /// </summary>
    [CustomEditor(typeof(GameObject), true, isFallback = true)]
    public class CUIPanelInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var go = target as GameObject;
            if (go == null) return;

            // 只在预制体资源上显示（且项目内路径）
            var path = AssetDatabase.GetAssetPath(go);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab")) return;
            if (!path.StartsWith("Assets/")) return;

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "CoffeeBean UI：给面板节点挂 CBind 标记后点生成，将生成 主脚本 + Designer 并自动绑定字段。",
                MessageType.Info);

            if (GUILayout.Button("生成面板代码 (CoffeeBean UI)"))
            {
                CUICodeGenerator.Generate(go);
                AssetDatabase.Refresh();
                Debug.Log($"[CoffeeBean.UI] {go.name} 代码已生成（编译完成后自动挂载）");
            }
        }
    }
}
