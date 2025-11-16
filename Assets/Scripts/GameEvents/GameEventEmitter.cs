using System;
using UnityEngine;

public static class GameEventEmitter<T>
{
    public static event Action<T> OnEvent;

    public static void Emit(T data)
    {
        OnEvent?.Invoke(data);
    }
}
