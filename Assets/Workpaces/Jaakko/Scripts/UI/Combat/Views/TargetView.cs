using TMPro;
using UnityEngine;

public class TargetView : MonoBehaviour, IUIComponentView
{
    [SerializeField] private TMP_Text m_targetNameText;

    public void SetPosition(Vector3 position) 
    {
        transform.position = position;
    }
    public void ChangeTarget(CombatActor target) 
    {
        m_targetNameText.text = target.name;
    }
    public void Init()
    {

    }
    public void View() 
    {
        gameObject.SetActive(true);
    }
    public void Hide() 
    {
        gameObject.SetActive(false);
    }    
}
