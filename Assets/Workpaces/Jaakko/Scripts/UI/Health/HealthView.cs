using UnityEngine;

public class HealthView : MonoBehaviour, IUIComponentView
{
    public void View() 
    {
    
    }
    public void Hide() 
    {
    
    }
    public void OnHealthChanged(float newHealth) 
    {
        Debug.Log($"Health changed to {newHealth}");
    }
}
