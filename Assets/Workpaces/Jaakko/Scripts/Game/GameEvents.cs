using System;

public static class GameEvents 
{
    public static event Action OnLoadStarted;
    public static event Action OnLoadFinished;

    public static void LoadStarted() 
    {
        OnLoadStarted?.Invoke();
    }
    public static void LoadFinished() 
    {
        OnLoadFinished?.Invoke();
    }
}