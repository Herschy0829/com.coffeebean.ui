using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeBean
{
    /// <summary>
    /// 面板索引：按 key 索引数据项，支持一 key 多值。
    /// </summary>
    internal sealed class CUIPanelIndex<TKey, TData>
    {
        private readonly Dictionary<TKey, List<TData>> _index = new Dictionary<TKey, List<TData>>();
        private readonly Func<TData, TKey> _keyGetter;

        public CUIPanelIndex(Func<TData, TKey> keyGetter) => _keyGetter = keyGetter;

        public IDictionary<TKey, List<TData>> Dictionary => _index;

        public void Add(TData item)
        {
            TKey key = _keyGetter(item);
            if (_index.TryGetValue(key, out var list))
            {
                list.Add(item);
            }
            else
            {
                _index.Add(key, new List<TData> { item });
            }
        }

        public void Remove(TData item)
        {
            TKey key = _keyGetter(item);
            if (_index.TryGetValue(key, out var list))
            {
                list.Remove(item);
                if (list.Count == 0) _index.Remove(key);
            }
        }

        public IEnumerable<TData> Get(TKey key)
            => _index.TryGetValue(key, out var list) ? (IEnumerable<TData>)list : Enumerable.Empty<TData>();

        public void Clear()
        {
            foreach (var list in _index.Values) list.Clear();
            _index.Clear();
        }
    }

    /// <summary>
    /// 面板表：双索引（预制体名/类型）管理所有已创建面板。
    /// </summary>
    public sealed class CUIPanelTable : IEnumerable<ICUIPanel>
    {
        private readonly CUIPanelIndex<string, ICUIPanel> _nameIndex = new CUIPanelIndex<string, ICUIPanel>(p => p.Transform.name);
        private readonly CUIPanelIndex<Type, ICUIPanel> _typeIndex = new CUIPanelIndex<Type, ICUIPanel>(p => p.GetType());

        public void Add(ICUIPanel panel)
        {
            _nameIndex.Add(panel);
            _typeIndex.Add(panel);
        }

        public void Remove(ICUIPanel panel)
        {
            _nameIndex.Remove(panel);
            _typeIndex.Remove(panel);
        }

        public void Clear()
        {
            _nameIndex.Clear();
            _typeIndex.Clear();
        }

        /// <summary>按打开参数查询面板。</summary>
        public IEnumerable<ICUIPanel> GetPanels(CUIPanelSearchKeys keys)
        {
            if (keys == null) return Enumerable.Empty<ICUIPanel>();

            // 类型 + 名称/实例 组合
            if (keys.PanelType != null && (!string.IsNullOrEmpty(keys.GameObjName) || keys.Panel != null))
            {
                return _typeIndex.Get(keys.PanelType)
                    .Where(p => p.Transform.name == keys.GameObjName || p == keys.Panel);
            }

            if (keys.PanelType != null)
            {
                return _typeIndex.Get(keys.PanelType);
            }

            if (keys.Panel != null)
            {
                return _nameIndex.Get(keys.Panel.Transform.name).Where(p => p == keys.Panel);
            }

            if (!string.IsNullOrEmpty(keys.GameObjName))
            {
                return _nameIndex.Get(keys.GameObjName);
            }

            return Enumerable.Empty<ICUIPanel>();
        }

        public IEnumerator<ICUIPanel> GetEnumerator()
            => _nameIndex.Dictionary.SelectMany(kv => kv.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
