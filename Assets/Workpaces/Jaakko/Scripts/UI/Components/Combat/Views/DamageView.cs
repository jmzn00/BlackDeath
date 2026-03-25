using UnityEngine;

public class DamageView : MonoBehaviour, IUIComponentView
{
    public void View() { gameObject.SetActive(true); }
    public void Hide() { gameObject.SetActive(false); }
    public void Init() 
    {
        
    }
}
