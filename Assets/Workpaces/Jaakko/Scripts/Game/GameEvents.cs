using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action OnLoadStarted;
    public static event Action OnLoadFinished;
    public static event Action<Vector3> OnPlayerMoved;

    public static void LoadStarted() => OnLoadStarted?.Invoke();
    public static void LoadFinished() => OnLoadFinished?.Invoke();
    public static void PlayerMoved(Vector3 velocity) => OnPlayerMoved?.Invoke(velocity);
}