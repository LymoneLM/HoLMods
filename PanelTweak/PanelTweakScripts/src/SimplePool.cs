using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PanelTweak;

public class SimplePool<T> where T : MonoBehaviour
{
    private T _prefab;
    private Transform _poolRoot;
        
    private Queue<T> _pool = new Queue<T>();
        
    public SimplePool(T prefab)
    {
        _prefab = prefab;
        _poolRoot = new GameObject("SimplePoolRoot").transform;
        Object.DontDestroyOnLoad(_poolRoot);
    }

    public T Get()
    {
        var obj = _pool.Count > 0 ? _pool.Dequeue() : Object.Instantiate(_prefab, _poolRoot);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        obj.transform.SetParent(_poolRoot);
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
        
    ~SimplePool()
    {
        Object.Destroy(_poolRoot);
    }
}