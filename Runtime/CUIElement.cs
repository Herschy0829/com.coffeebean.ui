using UnityEngine;

namespace CoffeeBean
{
    /// <summary>
    /// UI 元素基类：
    /// 复杂子元素（如一个按钮组合）可继承此类并配合 CBind(IsElement=true) 生成独立脚本。
    /// 普通面板内字段绑定用 <see cref="CBind"/> 即可，无需此基类。
    /// </summary>
    public abstract class CUIElement : MonoBehaviour
    {
        /// <summary>元素销毁前钩子。</summary>
        protected virtual void OnBeforeDestroy() { }

        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                OnBeforeDestroy();
            }
        }
    }
}
