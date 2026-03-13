using System;

public static class GameEvents
{
    public static event Action<CombatState> OnCombatStateChange;   
    
    public static void CombatStateChanged(CombatState state) 
    {
        OnCombatStateChange?.Invoke(state);
    }
}
