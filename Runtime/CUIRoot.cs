using UnityEngine;
using UnityEngine.UI;

namespace CoffeeBean
{
    /// <summary>
    /// UI 根节点：单例，6 个层级节点（Bg/Common/PopUI/Guide/Toast/Top），
    /// 负责面板挂载层级、分辨率设置、渲染模式切换。
    /// 首次访问时自动创建（DontDestroyOnLoad）。
    /// </summary>
    public sealed class CUIRoot : MonoBehaviour
    {
        private const string Tag = "CoffeeBean.UI";
        private const string RootName = "[CoffeeBean] UIRoot";

        private static CUIRoot _instance;

        /// <summary>UI 根单例（自动创建）。</summary>
        public static CUIRoot Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CUIRoot>();
                }
                if (_instance == null)
                {
                    var go = new GameObject(RootName);
                    go.transform.position = Vector3.zero;
                    _instance = go.AddComponent<CUIRoot>();
                    _instance.Build();
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasScaler _scaler;
        [SerializeField] private GraphicRaycaster _raycaster;

        // 6 个层级节点
        [SerializeField] private RectTransform _bg;
        [SerializeField] private RectTransform _common;
        [SerializeField] private RectTransform _popUi;
        [SerializeField] private RectTransform _guide;
        [SerializeField] private RectTransform _toast;
        [SerializeField] private RectTransform _top;

        public Canvas Canvas => _canvas;
        public Camera UICamera { get; private set; }

        /// <summary>测试用：重置单例（EditMode 测试 TearDown 调用，释放旧实例）。</summary>
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

        /// <summary>构建 UI 根结构（首次自动创建时调用；场景预制体挂载时 Awake 后由 CUIManager 补建）。</summary>
        private void Build()
        {
            // 根 Canvas（ScreenSpaceOverlay）
            if (!TryGetComponent(out _canvas)) _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            if (!TryGetComponent(out _raycaster)) _raycaster = gameObject.AddComponent<GraphicRaycaster>();
            if (!TryGetComponent(out _scaler)) _scaler = gameObject.AddComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = new Vector2(1920, 1080);
            _scaler.matchWidthOrHeight = 0.5f;

            // 专用 UI 相机（ScreenSpaceCamera 模式用）
            var camGo = new GameObject("UICamera");
            camGo.transform.SetParent(transform, false);
            UICamera = camGo.AddComponent<Camera>();
            UICamera.clearFlags = CameraClearFlags.Depth;
            UICamera.cullingMask = 1 << 5; // UI layer
            UICamera.orthographic = true;
            UICamera.enabled = false;

            _bg = CreateLayer("Bg", 0);
            _common = CreateLayer("Common", 1);
            _popUi = CreateLayer("PopUI", 2);
            _guide = CreateLayer("Guide", 3);
            _toast = CreateLayer("Toast", 4);
            _top = CreateLayer("Top", 5);
        }

        private RectTransform CreateLayer(string layerName, int siblingIndex)
        {
            var go = new GameObject(layerName, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.SetSiblingIndex(siblingIndex);
            return rect;
        }

        /// <summary>获取层级节点。</summary>
        public RectTransform GetLayer(CUILevel level)
        {
            switch (level)
            {
                case CUILevel.Bg: return _bg;
                case CUILevel.Common: return _common;
                case CUILevel.PopUI: return _popUi;
                case CUILevel.Guide: return _guide;
                case CUILevel.Toast: return _toast;
                case CUILevel.Top: return _top;
                default: return _common;
            }
        }

        /// <summary>
        /// 将面板挂到对应层级：
        /// 面板自带 Canvas 则挂到根（独立渲染），否则挂到对应层级节点。
        /// </summary>
        public void SetLevelOfPanel(CUILevel level, ICUIPanel panel)
        {
            if (panel?.Transform == null) return;

            var panelCanvas = panel.Transform.GetComponent<Canvas>();
            if (panelCanvas != null)
            {
                panel.Transform.SetParent(transform, false);
            }
            else
            {
                panel.Transform.SetParent(GetLayer(level), false);
            }

            if (panel.Info != null && panel.Info.Level != level)
            {
                panel.Info.Level = level;
            }
        }

        /// <summary>设置 UI 分辨率（参考分辨率 + 匹配模式）。</summary>
        public void SetResolution(int width, int height, float matchOnWidthOrHeight)
        {
            if (_scaler == null) TryGetComponent(out _scaler);
            _scaler.referenceResolution = new Vector2(width, height);
            _scaler.matchWidthOrHeight = matchOnWidthOrHeight;
        }

        /// <summary>ScreenSpaceOverlay 渲染模式（默认）。</summary>
        public void ScreenSpaceOverlayRenderMode()
        {
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            if (UICamera != null) UICamera.enabled = false;
        }

        /// <summary>ScreenSpaceCamera 渲染模式（配合 UICamera）。</summary>
        public void ScreenSpaceCameraRenderMode()
        {
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = UICamera;
            if (UICamera != null) UICamera.enabled = true;
        }

        /// <summary>日志辅助。</summary>
        private static void Log(string msg) => CLog.Info(Tag, msg);
    }
}
