using System.Collections;
using UnityEngine;

namespace CoffeeBean
{
    /// <summary>面板转场动画类型。</summary>
    public enum CUIPanelAnimType
    {
        None,      // 无动画（默认）
        Fade,      // 淡入/淡出
        SlideLeft, // 从左滑入/滑出
        SlideUp,   // 从下滑入/滑出
        Scale,     // 缩放弹出
    }

    /// <summary>
    /// 面板转场动画（轻量实现，纯协程，无外部依赖）：
    /// 给 CUIPanel 挂此组件并设置 AnimType/时长，Show/Hide 时自动播放动画。
    /// 通过 <see cref="CUIPanelTransition"/> 静态方法注册到 CUIManager 生命周期。
    /// </summary>
    [AddComponentMenu("CoffeeBean/UI/CUIPanelTransition")]
    [DisallowMultipleComponent]
    public sealed class CUIPanelTransition : MonoBehaviour
    {
        [Tooltip("显示动画类型")]
        public CUIPanelAnimType ShowAnim = CUIPanelAnimType.None;

        [Tooltip("隐藏动画类型")]
        public CUIPanelAnimType HideAnim = CUIPanelAnimType.None;

        [Tooltip("动画时长（秒）")]
        public float Duration = 0.2f;

        [Tooltip("滑动距离（Slide 用，单位：屏幕比例/像素）")]
        public float SlideDistance = 400f;

        private CanvasGroup _canvasGroup;
        private Coroutine _running;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>播放显示动画（CUIManager Show 前调用：置初态 → 播放 → 回调）。</summary>
        public void PlayShow(System.Action onCompleted)
        {
            if (ShowAnim == CUIPanelAnimType.None || !gameObject.activeInHierarchy)
            {
                ResetState();
                onCompleted?.Invoke();
                return;
            }
            RunAnimation(ShowAnim, forward: true, onCompleted);
        }

        /// <summary>播放隐藏动画（CUIManager Hide 前调用：播放 → 完成后 Hide）。</summary>
        public void PlayHide(System.Action onCompleted)
        {
            if (HideAnim == CUIPanelAnimType.None || !gameObject.activeInHierarchy)
            {
                ResetState();
                onCompleted?.Invoke();
                return;
            }
            RunAnimation(HideAnim, forward: false, onCompleted);
        }

        private void RunAnimation(CUIPanelAnimType type, bool forward, System.Action onCompleted)
        {
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(Animate(type, forward, onCompleted));
        }

        private IEnumerator Animate(CUIPanelAnimType type, bool forward, System.Action onCompleted)
        {
            // 初态
            RectTransform rect = (RectTransform)transform;
            Vector2 startPos = rect.anchoredPosition;
            float startAlpha = GetAlpha();
            float duration = Mathf.Max(0.01f, Duration);

            if (type == CUIPanelAnimType.Fade) SetAlpha(forward ? 0f : 1f);
            else if (type == CUIPanelAnimType.SlideLeft) rect.anchoredPosition = startPos + new Vector2(forward ? -SlideDistance : 0f, 0);
            else if (type == CUIPanelAnimType.SlideUp) rect.anchoredPosition = startPos + new Vector2(0, forward ? -SlideDistance : 0f);
            else if (type == CUIPanelAnimType.Scale) transform.localScale = forward ? Vector3.zero : Vector3.one;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration;
                float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                float k = forward ? eased : 1f - eased; // 正向：0→1；反向：1→0

                if (type == CUIPanelAnimType.Fade) SetAlpha(k);
                else if (type == CUIPanelAnimType.SlideLeft) rect.anchoredPosition = startPos + new Vector2(-SlideDistance * (1f - k), 0);
                else if (type == CUIPanelAnimType.SlideUp) rect.anchoredPosition = startPos + new Vector2(0, -SlideDistance * (1f - k));
                else if (type == CUIPanelAnimType.Scale) transform.localScale = Vector3.one * k;

                yield return null;
            }

            // 终态复位
            rect.anchoredPosition = startPos;
            transform.localScale = Vector3.one;
            SetAlpha(1f);
            _running = null;
            onCompleted?.Invoke();
        }

        private void ResetState()
        {
            if (_running != null)
            {
                StopCoroutine(_running);
                _running = null;
            }
            if (transform is RectTransform rect) rect.anchoredPosition = Vector2.zero;
            transform.localScale = Vector3.one;
            SetAlpha(1f);
        }

        private float GetAlpha()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            return _canvasGroup != null ? _canvasGroup.alpha : 1f;
        }

        private void SetAlpha(float alpha)
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = alpha;
        }
    }
}
