using System;
using UnityEngine;

public class GameEventGenericListener<T> : MonoBehaviour
{
    private Action<T> _callback;

    public void Initialize(Action<T> callback)
    {
        _callback = callback;
    }

    private void OnEnable()
    {
        GameEventEmitter<T>.OnEvent += OnEvent;
    }

    private void OnDisable()
    {
        GameEventEmitter<T>.OnEvent -= OnEvent;
    }

    private void OnEvent(T data)
    {
        _callback?.Invoke(data);
    }
}
