using UnityEngine;

namespace CoffeeBean
{
    /// <summary>
    /// 代码生成绑定标记：
    /// 挂在 UI 预制体节点上，代码生成器据此生成字段引用并自动挂载到面板脚本。
    /// 生成后该组件会被编辑器自动移除（运行时无开销）。
    /// </summary>
    [AddComponentMenu("CoffeeBean/UI/CBind")]
    public sealed class CBind : MonoBehaviour
    {
        /// <summary>字段注释（生成的代码中作为 /// summary）。</summary>
        [Tooltip("生成的字段注释")]
        public string Comment;

        /// <summary>
        /// 是否作为独立 CUIElement 生成（true = 生成独立元素脚本，挂此节点；false = 作为面板字段）。
        /// </summary>
        [Tooltip("生成独立 CUIElement 脚本（默认 false = 面板字段）")]
        public bool IsElement;

        /// <summary>
        /// 显式指定绑定的组件类型（完整类型名，如 UnityEngine.UI.Button）；
        /// 为空时按优先级自动推断（TMP > UGUI > Collider > ... > RectTransform > Transform）。
        /// </summary>
        [Tooltip("显式绑定组件类型（完整类型名）；空 = 自动推断")]
        public string CustomComponentName;

        /// <summary>解析生成字段名（默认用节点名）。</summary>
        public string FieldName => name;

        /// <summary>生成时用到的组件类型名（推断或显式）。</summary>
        public string ResolveTypeName()
        {
            if (!string.IsNullOrEmpty(CustomComponentName)) return CustomComponentName;
            return CBindTypeResolver.Resolve(gameObject);
        }
    }

    /// <summary>绑定类型推断器。</summary>
    public static class CBindTypeResolver
    {
        public static string Resolve(GameObject go)
        {
            if (go == null) return "Transform";

            // TMP（TextMeshPro）——用非泛型 GetComponent 避免 Unity 6 泛型缺失抛异常
            if (go.GetComponent("TMPro.TextMeshProUGUI") != null) return "TMPro.TextMeshProUGUI";
            if (go.GetComponent("TMPro.TextMeshPro") != null) return "TMPro.TextMeshPro";
            if (go.GetComponent("TMPro.TMP_InputField") != null) return "TMPro.TMP_InputField";

            // UGUI
            if (go.TryGetComponent<UnityEngine.UI.ScrollRect>(out _)) return "UnityEngine.UI.ScrollRect";
            if (go.TryGetComponent<UnityEngine.UI.InputField>(out _)) return "UnityEngine.UI.InputField";
            if (go.TryGetComponent<UnityEngine.UI.Dropdown>(out _)) return "UnityEngine.UI.Dropdown";
            if (go.TryGetComponent<UnityEngine.UI.Button>(out _)) return "UnityEngine.UI.Button";
            if (go.TryGetComponent<UnityEngine.UI.Text>(out _)) return "UnityEngine.UI.Text";
            if (go.TryGetComponent<UnityEngine.UI.RawImage>(out _)) return "UnityEngine.UI.RawImage";
            if (go.TryGetComponent<UnityEngine.UI.Toggle>(out _)) return "UnityEngine.UI.Toggle";
            if (go.TryGetComponent<UnityEngine.UI.Slider>(out _)) return "UnityEngine.UI.Slider";
            if (go.TryGetComponent<UnityEngine.UI.Scrollbar>(out _)) return "UnityEngine.UI.Scrollbar";
            if (go.TryGetComponent<UnityEngine.UI.Image>(out _)) return "UnityEngine.UI.Image";
            if (go.TryGetComponent<UnityEngine.UI.ToggleGroup>(out _)) return "UnityEngine.UI.ToggleGroup";

            // 其他常用
            if (go.TryGetComponent<Rigidbody>(out _)) return "Rigidbody";
            if (go.TryGetComponent<Rigidbody2D>(out _)) return "Rigidbody2D";
            if (go.TryGetComponent<Collider>(out _)) return "Collider";
            if (go.TryGetComponent<Collider2D>(out _)) return "Collider2D";
            if (go.TryGetComponent<Animator>(out _)) return "Animator";
            if (go.TryGetComponent<Canvas>(out _)) return "Canvas";
            if (go.TryGetComponent<Camera>(out _)) return "Camera";
            if (go.TryGetComponent<SpriteRenderer>(out _)) return "SpriteRenderer";
            if (go.TryGetComponent<ParticleSystem>(out _)) return "ParticleSystem";
            if (go.TryGetComponent<RectTransform>(out _)) return "RectTransform";

            return "Transform";
        }
    }
}
