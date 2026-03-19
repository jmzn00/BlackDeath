using TMPro;
using UnityEngine;

public class TargetView : MonoBehaviour, IUIComponentView
{
    [Header("Elements")]
    [SerializeField] private TMP_Text m_targetNameText;
    [Header("Position")]
    [SerializeField] private Vector3 m_positionOffset;
    public void SetPosition(Vector3 position) 
    {
        transform.position = position + m_positionOffset;
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
