using System.Collections.Generic;
using UnityEngine;

namespace PanelTweak;

public class SimplePool<T> where T : MonoBehaviour
{
    private readonly T _prefab;
    private readonly Transform _poolRoot;
    private readonly Queue<T> _available = new();
    private readonly HashSet<T> _inUse = [];
    
    private readonly int _maxCapacity;

    public System.Action<T> OnCreate { get; set; }
    public System.Action<T> OnGet { get; set; }
    public System.Action<T> OnReturn {get; set;}

    /// <summary>
    /// 创建对象池
    /// </summary>
    /// <param name="prefab">池中对象预制体</param>
    /// <param name="maxCapacity">最大容量限制，0 表示无限制</param>
    /// <param name="poolRootName">池根节点名称，留空自动生成</param>
    public SimplePool(T prefab, int maxCapacity = 0, string poolRootName = null)
    {
        _prefab = prefab;
        _maxCapacity = Mathf.Max(0, maxCapacity);

        string rootName = string.IsNullOrEmpty(poolRootName) ? $"PoolRoot_{typeof(T).Name}" : poolRootName;
        var go = new GameObject(rootName);
        Object.DontDestroyOnLoad(go);
        go.SetActive(false);
        _poolRoot = go.transform;
    }

    /// <summary>
    /// 从池中获取一个可用对象（若无则实例化）
    /// </summary>
    /// <param name="parent">指定父节点，传 null 则脱离池根节点放到场景根下</param>
    /// <param name="worldPositionStays">改变父节点时保持世界坐标，默认 true</param>
    public T Get(Transform parent = null, bool worldPositionStays = true)
    {
        while (true)
        {
            T obj;
            if (_available.Count > 0)
            {
                obj = _available.Dequeue();
                if (obj == null)
                    continue;
            }
            else
            {
                obj = Object.Instantiate(_prefab, _poolRoot);
                OnCreate?.Invoke(obj);
            }

            obj.transform.SetParent(parent, worldPositionStays);
            obj.gameObject.SetActive(true);

            _inUse.Add(obj);
            OnGet?.Invoke(obj);
            return obj;
        }
    }

    /// <summary>
    /// 归还对象到池中。若已超出最大容量，直接销毁对象。
    /// </summary>
    public void Return(T obj)
    {
        if (obj == null) return;

        if (!_inUse.Remove(obj))
        {
            if (obj != null) Object.Destroy(obj.gameObject);
            return;
        }

        if (_maxCapacity > 0 && _available.Count >= _maxCapacity)
        {
            Object.Destroy(obj.gameObject);
            return;
        }

        obj.transform.SetParent(_poolRoot, false);
        obj.gameObject.SetActive(false);
        OnReturn?.Invoke(obj);
        _available.Enqueue(obj);
    }

    /// <summary>
    /// 预实例化指定数量的对象并放入池中
    /// </summary>
    public void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (_maxCapacity > 0 && _available.Count + _inUse.Count >= _maxCapacity)
                break;

            var obj = Object.Instantiate(_prefab, _poolRoot);
            obj.gameObject.SetActive(false);
            OnCreate?.Invoke(obj);
            _available.Enqueue(obj);
        }
    }

    /// <summary>
    /// 清空池中所有未使用对象（销毁）
    /// </summary>
    public void ClearAvailable()
    {
        while (_available.Count > 0)
        {
            var obj = _available.Dequeue();
            if (obj != null) Object.Destroy(obj.gameObject);
        }
    }

    /// <summary>
    /// 销毁池中所有对象（包括使用中的），并清理根节点
    /// </summary>
    public void Dispose()
    {
        ClearAvailable();
        _inUse.Clear();
        if (_poolRoot != null) Object.Destroy(_poolRoot.gameObject);
    }

    /// <summary>
    /// 获取当前池中总的存活对象数（使用中+可用）
    /// </summary>
    public int TotalCount => _available.Count + _inUse.Count;

    /// <summary>
    /// 当前可用对象数
    /// </summary>
    public int AvailableCount => _available.Count;
}